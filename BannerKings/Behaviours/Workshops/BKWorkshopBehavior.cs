using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Workshops
{
    public class BKWorkshopBehavior : CampaignBehaviorBase
    {
        private Dictionary<Workshop, WorkshopData> inventories = new Dictionary<Workshop, WorkshopData>();
        private Workshop selectedWorkshop;
        public WorkshopData GetInventory(Workshop wk)
        {
            if (inventories.ContainsKey(wk))
            {
                return inventories[wk];
            }

            return null;
        }

        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickTownEvent.AddNonSerializedListener(this, OnTownDailyTick);
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
            CampaignEvents.WarDeclared.AddNonSerializedListener(this, OnWarDeclared);
            CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener(this, OnOwnerChanged);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("bannerkings-workshop-inventories", ref inventories);

            if (inventories == null)
            {
                inventories = new Dictionary<Workshop, WorkshopData>();
            }
        }

        private void OnOwnerChanged(Settlement settlement, bool openToClaim, Hero newOwner, Hero oldOwner,
            Hero capturerHero, ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail detail)
        {
            if (newOwner != null && oldOwner != null && newOwner.MapFaction != oldOwner.MapFaction &&
                settlement.Town != null)
            {
                SeizeWorkshops(settlement.Town, oldOwner.MapFaction);
            }
        } 

        private void OnWarDeclared(IFaction faction1, IFaction faction2, DeclareWarAction.DeclareWarDetail detail)
        {
            foreach (var town in faction1.Fiefs)
            {
                SeizeWorkshops(town, faction2);
            }

            foreach (var town in faction2.Fiefs)
            {
                SeizeWorkshops(town, faction1);
            }
        }

        private void SeizeWorkshops(Town town, IFaction oldOwners)
        {
            foreach (var wk in town.Workshops)
            {
                if (wk.Owner != null && wk.Owner.MapFaction == oldOwners)
                {
                    Hero oldOwner = wk.Owner;
                    Hero townOwner = town.OwnerClan.Leader;
                    Hero hero = Campaign.Current.Models.WorkshopModel.SelectNextOwnerForWorkshop(town, wk, wk.Owner, 0);
                    if (hero != null)
                    {
                        ChangeOwnerOfWorkshopAction.ApplyByWarDeclaration(wk, 
                            hero, 
                            wk.WorkshopType,
                            Campaign.Current.Models.WorkshopModel.GetInitialCapital(1), 
                            true, 
                            null);
                    }
                    else
                    {
                        if (Campaign.Current.Models.WorkshopModel.GetMaxWorkshopCountForTier(townOwner.Clan.Tier) > 
                            townOwner.OwnedWorkshops.Count)
                        {
                            ChangeOwnerOfWorkshopAction.ApplyByWarDeclaration(wk,
                                                       townOwner,
                                                       wk.WorkshopType,
                                                       Campaign.Current.Models.WorkshopModel.GetInitialCapital(1),
                                                       true,
                                                       null);
                            if (townOwner == Hero.MainHero)
                            {
                                InformationManager.ShowInquiry(new InquiryData(new TextObject("{=doEjziQe}Workshop Seizure").ToString(),
                                    new TextObject("{=eQbfxno4}The {WORKSHOP} has been seized from the {OWNER} at {TOWN} due to war between your realms.")
                                    .SetTextVariable("WORKSHOP", wk.Name)
                                    .SetTextVariable("OWNER", oldOwner.Name)
                                    .SetTextVariable("TOWN", town.Name).ToString(),
                                    true,
                                    false,
                                    GameTexts.FindText("str_ok").ToString(),
                                    string.Empty,
                                    null,
                                    null),
                                    true);
                                
                            }
                        }
                        else if (townOwner.MapFaction.IsKingdomFaction)
                        {
                            Kingdom kingdom = townOwner.MapFaction as Kingdom;
                            if (townOwner == Hero.MainHero)
                            {
                                List<InquiryElement> options = new List<InquiryElement>();
                                foreach (var clan in kingdom.Clans)
                                {
                                    if (clan.IsClanTypeMercenary || clan == townOwner.Clan)
                                    {
                                        continue;
                                    }

                                    bool enabled = Campaign.Current.Models.WorkshopModel.GetMaxWorkshopCountForTier(clan.Tier) >
                                        clan.Leader.OwnedWorkshops.Count;

                                    options.Add(new InquiryElement(clan,
                                        clan.Name.ToString(),
                                        new ImageIdentifier(clan.Banner),
                                        enabled,
                                        string.Empty));
                                }

                                MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                                    new TextObject("{=jvVup6nx}Workshop Grant").ToString(),
                                    new TextObject("{=eQbfxno4}The {WORKSHOP} has been seized from the {OWNER} at {TOWN} due to war between your realms. {TOWNOWNER} has gifted you this property.")
                                    .SetTextVariable("WORKSHOP", wk.Name)
                                    .SetTextVariable("OWNER", oldOwner.Name)
                                    .SetTextVariable("TOWN", town.Name)
                                    .SetTextVariable("TOWNOWNER", townOwner.Name).ToString(),
                                    options,
                                    false,
                                    1,
                                    GameTexts.FindText("str_ok").ToString(),
                                    string.Empty,
                                    (List<InquiryElement> List) =>
                                    {
                                        Clan clan = (Clan)List[0].Identifier;
                                        ChangeOwnerOfWorkshopAction.ApplyByWarDeclaration(wk,
                                            clan.Leader,
                                            wk.WorkshopType,
                                            Campaign.Current.Models.WorkshopModel.GetInitialCapital(1),
                                            true,
                                            null);
                                    },
                                    null),
                                    true);
                            }
                            else
                            {
                                List<(Hero, float)> options = new List<(Hero, float)>();
                                foreach (var clan in kingdom.Clans)
                                {
                                    if (clan.IsClanTypeMercenary || clan == townOwner.Clan)
                                    {
                                        continue;
                                    }

                                    options.Add(new (clan.Leader, townOwner.GetRelation(clan.Leader)));
                                }

                                Hero result = MBRandom.ChooseWeighted(options);
                                if (result != null)
                                {
                                    ChangeOwnerOfWorkshopAction.ApplyByWarDeclaration(wk,
                                        result,
                                        wk.WorkshopType,
                                        Campaign.Current.Models.WorkshopModel.GetInitialCapital(1),
                                        true,
                                        null);

                                    if (result == Hero.MainHero)
                                    {
                                        InformationManager.ShowInquiry(new InquiryData(new TextObject("{=jvVup6nx}Workshop Grant").ToString(),
                                            new TextObject("{=eQbfxno4}The {WORKSHOP} has been seized from the {OWNER} at {TOWN} due to war between your realms. {TOWNOWNER} has gifted you this property.")
                                            .SetTextVariable("WORKSHOP", wk.Name)
                                            .SetTextVariable("OWNER", oldOwner.Name)
                                            .SetTextVariable("TOWN", town.Name)
                                            .SetTextVariable("TOWNOWNER", townOwner.Name).ToString(),
                                            true,
                                            false,
                                            GameTexts.FindText("str_ok").ToString(),
                                            string.Empty,
                                            null,
                                            null),
                                            true);

                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void OnTownDailyTick(Town town)
        {
            foreach (var workshop in town.Workshops)
            {
                AddInventory(workshop);
                inventories[workshop].Tick();
            }
        }

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            starter.AddPlayerLine("lord_workshop_question",
                "lord_talk_speak_diplomacy_2", 
                "lord_workshop_response", 
                "{=LuLttpc5}I wish to buy one of your workshops.",
                IsWorkshopOwnerLord,
                BuyLordWorkshopAnswerListMultiple, 
                100);

            starter.AddDialogLine("lord_workshop_response",
                "lord_workshop_response", 
                "lord_workshop_select_workshop", 
                "{=j78hz3Qc}Hmm. That's possible. Which one?",
                IsWorkshopOwnerLord, null, 100, null);

            starter.AddRepeatablePlayerLine("lord_workshop_select_workshop",
                "lord_workshop_select_workshop", 
                "lord_workshop_buy_response", 
                "{=V9FPM5aG}{WORKSHOP_NAME}", 
                "{=5z4hEq68}I am thinking of a different kind of workshop.", 
                "workshop_owner_notable_multiple_response",
                BuyLordWorkshopSelectMultiple, 
                () =>
                {
                    selectedWorkshop = (ConversationSentence.SelectedRepeatObject as Workshop);
                }, 
                100, 
                null);

            starter.AddDialogLine("lord_workshop_buy_response",
                "lord_workshop_buy_response",
                "lord_workshop_player_options", 
                "{=hj2SDH6s}I'm willing to sell. But it will cost you {COST} {GOLD_ICON}. {PREMIUM} {INVENTORY} Are you willing to pay?", 
                () =>
                {
                    if (selectedWorkshop != null)
                    {
                        MBTextManager.SetTextVariable("COST", Campaign.Current.Models.WorkshopModel.GetBuyingCostForPlayer(selectedWorkshop));
                        if (selectedWorkshop.Owner.OwnedWorkshops.Count == 1)
                        {
                            MBTextManager.SetTextVariable("PREMIUM", new TextObject("{=13LGeTLO}I'll be charging you a premium of 15% as this is my only workshop."));
                        }

                        var inventoryCost = BannerKingsConfig.Instance.WorkshopModel.GetInventoryCost(selectedWorkshop);
                        if (inventoryCost > 0)
                        {
                            MBTextManager.SetTextVariable("INVENTORY", new TextObject("{=FWQjpwex}{INVENTORY_PRICE} {GOLD_ICON} is added to the final price as the workshop's inventory value.")
                                .SetTextVariable("INVENTORY_PRICE", inventoryCost));
                        }

                        return true;
                    }
                    return false;
                }, 
                null, 
                100, 
                null);

            starter.AddPlayerLine("lord_workshop_positive_answer",
                "lord_workshop_player_options", 
                "lord_workshop_finish_positive", 
                "{=kB65SzbF}Yes.", 
                null, 
                () =>
                {
                    Workshop lastSelectedWorkshop = selectedWorkshop;
                    int buyingCostForPlayer = Campaign.Current.Models.WorkshopModel.GetBuyingCostForPlayer(lastSelectedWorkshop);
                    ChangeOwnerOfWorkshopAction.ApplyByTrade(lastSelectedWorkshop, Hero.MainHero, lastSelectedWorkshop.WorkshopType, Campaign.Current.Models.WorkshopModel.GetInitialCapital(1), true, buyingCostForPlayer, null);
                }, 
                100, 
                delegate (out TextObject explanation)
                {
                    bool flag = Hero.MainHero.Gold < Campaign.Current.Models.WorkshopModel.GetBuyingCostForPlayer(selectedWorkshop);
                    bool flag2 = Campaign.Current.Models.WorkshopModel.GetMaxWorkshopCountForTier(Clan.PlayerClan.Tier) <= Hero.MainHero.OwnedWorkshops.Count;
                    bool result = false;
                    if (flag)
                    {
                        explanation = new TextObject("{=B2jpmFh6}You don't have enough money to buy this workshop.", null);
                    }
                    else if (flag2)
                    {
                        explanation = new TextObject("{=Mzs39I2G}You have reached the maximum amount of workshops you can have.", null);
                    }
                    else
                    {
                        explanation = TextObject.Empty;
                        result = true;
                    }
                    return result;
                },
                null);

            starter.AddDialogLine("lord_workshop_finish_positive",
                "lord_workshop_finish_positive", 
                "hero_main_options", 
                "{=ZtULAKGb}Well then, we have a deal. I will instruct my workers that they are now working for you. Good luck!", 
                null, null, 100, null);

            starter.AddPlayerLine("lord_workshop_negative_answer", 
                "lord_workshop_player_options",
                "lord_workshop_finish_negative", 
                "{=znDzVxVJ}No.", 
                null, 
                null, 
                100, 
                null, 
                null);

            starter.AddDialogLine("lord_workshop_finish_negative",
                "lord_workshop_finish_negative", 
                "hero_main_options", 
                "{=Hj25CLlZ}As you wish. Let me know if you change your mind.",
                null, null, 100);
        }

        private bool IsWorkshopOwnerLord() => Hero.OneToOneConversationHero != null && Hero.OneToOneConversationHero.IsLord &&
            Hero.OneToOneConversationHero.OwnedWorkshops.Count > 0 && !Hero.OneToOneConversationHero.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction);

        private void BuyLordWorkshopAnswerListMultiple()
        {
            ConversationSentence.SetObjectsToRepeatOver((from x in Hero.OneToOneConversationHero.OwnedWorkshops
                                                         where !x.WorkshopType.IsHidden
                                                         select x).ToList<Workshop>(), 5);
        }

        private bool BuyLordWorkshopSelectMultiple()
        {
            Workshop workshop = ConversationSentence.CurrentProcessedRepeatObject as Workshop;
            if (workshop != null)
            {
                ConversationSentence.SelectedRepeatLine.SetTextVariable("WORKSHOP_NAME", workshop.Name);
                return true;
            }
            return false;
        }

        private void AddInventory(Workshop workshop)
        {
            if (!inventories.ContainsKey(workshop))
            {
                inventories.Add(workshop, new WorkshopData(workshop));
            }
        }
    }
}

