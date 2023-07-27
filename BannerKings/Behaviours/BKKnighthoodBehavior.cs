using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BannerKings.Actions;
using BannerKings.Managers.Populations.Estates;
using BannerKings.Managers.Titles;
using BannerKings.Utils.Extensions;
using HarmonyLib;
using SandBox.CampaignBehaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement.Categories;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using ActionType = BannerKings.Managers.Titles.ActionType;

namespace BannerKings.Behaviours
{
    public class BKKnighthoodBehavior : BannerKingsBehavior
    {
        private readonly List<InquiryElement> lordshipsToGive = new();
        private readonly List<InquiryElement> estatesToGive = new();
        private Estate estate = null;
        private FeudalTitle lordship = null;

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
            CampaignEvents.DailyTickHeroEvent.AddNonSerializedListener(this, OnHeroDailyTick);
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void OnHeroDailyTick(Hero hero)
        {
            RunWeekly(() =>
            {
                if (hero.Clan == null || hero == hero.Clan.Leader || hero == Hero.MainHero ||
                               hero.Occupation != Occupation.Lord ||
                               BannerKingsConfig.Instance.TitleManager == null)
                {
                    return;
                }

                var title = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(hero);
                if (title == null || title.TitleType != TitleType.Lordship || title.Fief.Village == null)
                {
                    return;
                }

                //if (hero.Clan.GetName().ToString() == "Prienicos")
                //   if (!hero.Clan.Heroes.Contains(hero))
                //       ClanActions.JoinClan(hero, hero.Clan);

                if (!CanCreateClan(hero))
                {
                    return;
                }

                var originalClan = hero.Clan;
                if (hero.Spouse != null && Utils.Helpers.IsClanLeader(hero.Spouse))
                {
                    return;
                }

                if (originalClan != Clan.PlayerClan)
                {
                    CreateClan(hero, originalClan, title);
                }
                else
                {
                    var clanName = ClanActions.CanCreateNewClan(hero.Culture, title.Fief);
                    if (clanName == null)
                    {
                        return;
                    }

                    TextObject requestText;
                    if (hero.IsFriend(Hero.MainHero))
                    {
                        requestText =
                            new TextObject("{=adCAG0nk}I humbly ask of you to release me of my duties in the {CLAN}. I shall remain as your vassal and loyal friend.");
                    }
                    else if (!hero.IsEnemy(Hero.MainHero))
                    {
                        requestText =
                            new TextObject("{=KwjG1wou}I request of you to release me of my duties in the {CLAN}. It is time for me to lead my own family.");
                    }
                    else
                    {
                        requestText =
                            new TextObject("{=PDD36QvM}I demand of you to release me of the {CLAN}. It is time we part ways.");
                    }

                    requestText = requestText.SetTextVariable("CLAN", originalClan.Name);

                    var cost = BannerKingsConfig.Instance.InfluenceModel.GetRejectKnighthoodCost(originalClan);
                    InformationManager.ShowInquiry(new InquiryData(
                        new TextObject("{=VE2h1JQz}The clan of {HERO}").SetTextVariable("HERO", hero.Name).ToString(),
                        new TextObject("{=MQtSCDgK}{GREETING} {PLAYER}, {TEXT}\nRejecting their request would cost {INFLUENCE} influence.")
                            .SetTextVariable("GREETING",
                                GameTexts.FindText(Hero.MainHero.IsFemale ? "str_my_lady" : "str_my_lord"))
                            .SetTextVariable("PLAYER", Hero.MainHero.Name)
                            .SetTextVariable("TEXT", requestText)
                            .SetTextVariable("INFLUENCE", cost)
                            .ToString(),
                        true, true, GameTexts.FindText("str_accept").ToString(),
                        GameTexts.FindText("str_reject").ToString(),
                        () =>
                        {
                            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(hero, originalClan.Leader, 5);
                            CreateClan(hero, originalClan, title, clanName);
                        },
                        () =>
                        {
                            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(hero, originalClan.Leader, -12);
                            GainKingdomInfluenceAction.ApplyForDefault(originalClan.Leader, -cost);
                            BannerKingsConfig.Instance.TitleManager.AddKnightInfluence(hero, -200);
                        }));
                }
            },
            GetType().Name,
            false); 
        }

        private void CreateClan(Hero hero, Clan originalClan, FeudalTitle title, TextObject name = null)
        {
            var newClan = ClanActions.CreateNewClan(hero, title.Fief, hero.StringId + "_knight_clan", name, 150f, true);
            if (newClan != null)
            {
                MBInformationManager.AddQuickInformation(
                    new TextObject("{=YjtnRj9r}The {NEW} has been formed by {HERO}, previously a knight of {ORIGINAL}.")
                        .SetTextVariable("NEW", hero.Clan.Name)
                        .SetTextVariable("HERO", hero.Name)
                        .SetTextVariable("ORIGINAL", originalClan.Name));
            }
        }

        private bool CanCreateClan(Hero hero)
        {
            return hero.Gold >= 50000 && BannerKingsConfig.Instance.TitleManager.GetKnightInfluence(hero) >= 350f &&
                   hero.Occupation == Occupation.Lord &&
                   !Utils.Helpers.IsCloseFamily(hero, hero.Clan.Leader);
        }

        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            AddDialog(campaignGameStarter);
        }

        private void AddDialog(CampaignGameStarter starter)
        {
            starter.AddPlayerLine("companion_grant_knighthood", "companion_role", "companion_knighthood_question",
                "{=tioEwnbc}I would like thee to serve as my {KNIGHT_TEXT}.",
                () =>
                {
                    var companion = Hero.OneToOneConversationHero;
                    if (companion != null)
                    {
                        MBTextManager.SetTextVariable("KNIGHT_TEXT", Utils.TextHelper.GetKnightTitle(Clan.PlayerClan.Culture,
                            Hero.OneToOneConversationHero.IsFemale, false));
                    }

                    return companion != null && companion.Clan == Clan.PlayerClan &&
                           BannerKingsConfig.Instance.TitleManager.Knighthood;
                },
                delegate
                {
                    InformationManager.ShowInquiry(new InquiryData(new TextObject("{=mMoVoid6}Bestowing Knighthood").ToString(), 
                        new TextObject("{=FwXL7yLD}Knighting involves granting a Fee (an estate or lordship) to a commoner, in exchange for Knight Service, or military aid. Once granted, their property can not be revoked. Knights granted a Lordship will pay for their own retinue, but will also have full rights to the village's income. Knights will serve under your clan, but eventually will aim to forge their own family.")
                        .ToString(),
                        true, 
                        false, 
                        GameTexts.FindText("str_accept").ToString(), 
                        null, 
                        null, 
                        null));
                }, 
                100,
                GrantKnighthoodOnClickable);

            starter.AddDialogLine("companion_grant_knighthood_response", "companion_knighthood_question",
                "companion_knighthood_response",
                new TextObject("{=KfprN5px}{TITLE}, it would be my honor. I must ask, what kind of property would you honor me with?")
                .SetTextVariable("TITLE", Hero.MainHero.IsFemale ? GameTexts.FindText("str_my_lady") : GameTexts.FindText("str_my_lord"))
                .ToString(), 
                null, 
                null);

            starter.AddPlayerLine("companion_grant_knighthood_response_lordship", "companion_knighthood_response",
              "companion_knighthood_accepted_fief",
                  new TextObject("{=VjMG5nHH}A Lordship.").ToString(),
                  null,
                  SelectFiefOnConsequence,
                  100,
                  GrantKnighthoodFiefOnClickable);

            starter.AddPlayerLine("companion_grant_knighthood_response_estate", "companion_knighthood_response",
                "companion_knighthood_accepted_estate", 
                new TextObject("{=We2p3hwv}An estate property.").ToString(),
                null,
                SelectEstateOnConsequence,
                100,
                GrantKnighthoodEstateOnClickable);

            starter.AddPlayerLine("companion_grant_knighthood_response_return", "companion_knighthood_response",
                "hero_main_options",
                "{=G4ALCxaA}Never mind.",
                null, 
                null);

            starter.AddDialogLine("companion_knighthood_accepted_fief", "companion_knighthood_accepted_fief",
                "companion_knighthood_finish_fief",
                new TextObject("{=uEX33Axr}{TITLE}, a Lordship would be very generous. Its income would allow me to raise a retinue and be fully independent to pay for it. Of course, the more rent a village produces, the more troops I can provide us.")
                .SetTextVariable("TITLE", Hero.MainHero.IsFemale ? GameTexts.FindText("str_my_lady") : GameTexts.FindText("str_my_lord"))
                .ToString(),
                null,
                null);

            starter.AddDialogLine("companion_knighthood_accepted_estate", "companion_knighthood_accepted_estate",
                "companion_knighthood_finish_estate",
                new TextObject("{=op5kY5Qq}{TITLE}, an estate would be generous. Its income would help pay a retinue, and it's manpower allow us more access to volunteers. Estates with more acres generate more income and provide more manpower.")
                .SetTextVariable("TITLE", Hero.MainHero.IsFemale ? GameTexts.FindText("str_my_lady") : GameTexts.FindText("str_my_lord"))
                .ToString(),
                null,
                null);

            starter.AddPlayerLine("companion_knighthood_finish_fief", "companion_knighthood_finish_fief",
                "companion_knighthood_finished",
                "{=zOGWqeBx}It is decided then. Let it be know thou art a {KNIGHT} of the {PLAYER_CLAN}.",
                () =>
                {
                    MBTextManager.SetTextVariable("KNIGHT", Utils.TextHelper.GetKnightTitle(Clan.PlayerClan.Culture,
                        Hero.OneToOneConversationHero.IsFemale, false));
                    MBTextManager.SetTextVariable("PLAYER_CLAN", Clan.PlayerClan.Name);

                    return true;
                }, 
                GrantFiefOnConsequence);

            starter.AddPlayerLine("companion_knighthood_finish_fief_return", "companion_knighthood_finish_fief",
                "close_window",
                "{=G4ALCxaA}Never mind.",
                null,
                null);

            starter.AddPlayerLine("companion_knighthood_finish_estate", "companion_knighthood_finish_estate",
                "companion_knighthood_finished",
                "{=zOGWqeBx}It is decided then. Let it be know thou art a {KNIGHT} of the {PLAYER_CLAN}.",
                () =>
                {
                    MBTextManager.SetTextVariable("KNIGHT", Utils.TextHelper.GetKnightTitle(Clan.PlayerClan.Culture,
                        Hero.OneToOneConversationHero.IsFemale, false));
                    MBTextManager.SetTextVariable("PLAYER_CLAN", Clan.PlayerClan.Name);

                    return true;
                }, 
               GrantEstateOnConsequence);

            starter.AddPlayerLine("companion_knighthood_finish_estate_return", "companion_knighthood_finish_estate",
                "close_window",
                "{=G4ALCxaA}Never mind.",
                null,
                null);

            starter.AddDialogLine("companion_knighthood_finished", "companion_knighthood_finished",
                "close_window",
                new TextObject("{=g09DXXBc}Thank you {TITLE}!")
                .SetTextVariable("TITLE", Hero.MainHero.IsFemale ? GameTexts.FindText("str_my_lady") : GameTexts.FindText("str_my_lord"))
                .ToString(),
                null,
                null);

            starter.AddDialogLine("companion_grant_knighthood_response", "companion_grant_additional_property_response",
               "companion_knighthood_response",
               new TextObject("{=KfprN5px}{TITLE}, it would be my honor. I must ask, what kind of property would you honor me with?")
               .SetTextVariable("TITLE", Hero.MainHero.IsFemale ? GameTexts.FindText("str_my_lady") : GameTexts.FindText("str_my_lord"))
               .ToString(),
               null,
               null);

            starter.AddPlayerLine("companion_grant_additional_property",
                "companion_role",
                "companion_grant_additional_property_response",
                "{=gNRi219n}I aim to grant thee an additional property.",
                IsPlayerKnight,
                null,
                100);

            starter.AddPlayerLine("companion_grant_additional_property_response_lordship", "companion_grant_additional_property_response",
              "companion_grant_additional_property_accepted",
                  new TextObject("{=VjMG5nHH}A Lordship.").ToString(),
                  null,
                  SelectFiefOnConsequence,
                  100,
                  GrantPropertyFiefOnClickable);

            starter.AddPlayerLine("companion_grant_additional_property_response_estate", "companion_grant_additional_property_response",
                "companion_grant_additional_property_accepted",
                new TextObject("{=We2p3hwv}An estate property.").ToString(),
                null,
                SelectEstateOnConsequence,
                100,
                GrantPropertyEstateOnClickable);

            starter.AddDialogLine("companion_grant_additional_property_accepted", "companion_grant_additional_property_accepted",
                "companion_grant_additional_property_finish",
                new TextObject("{=DLx4qCTY}{TITLE}, that would be a generous gift. Additional properties would allow me to field a bigger retinue.")
                .SetTextVariable("TITLE", Hero.MainHero.IsFemale ? GameTexts.FindText("str_my_lady") : GameTexts.FindText("str_my_lord"))
                .ToString(),
                null,
                null);

            starter.AddPlayerLine("companion_grant_additional_property_finish", "companion_grant_additional_property_finish",
                "close_window",
               new TextObject("{=YWw9LkmQ}It is decided then.").ToString(),
               null,
               GrantProperty);

            starter.AddPlayerLine("companion_grant_additional_property_return", "companion_grant_additional_property_finish",
                "close_window",
                "{=G4ALCxaA}Never mind.",
                null,
                null);

            starter.AddPlayerLine("turn_companion_to_lord_start",
                "companion_role", 
                "turn_companion_to_lord_talk_answer", 
                "{=B9uT9wa6}I wish to reward you for your services.",
                IsPlayerKnight, 
                null,
                100);
        }

        private bool IsPlayerKnight()
        {
            if (BannerKingsConfig.Instance.TitleManager == null)
            {
                return false;
            }

            var knight = Hero.OneToOneConversationHero;
            var titles = BannerKingsConfig.Instance.TitleManager.GetAllDeJure(knight);
            var estates = BannerKingsConfig.Instance.PopulationManager.GetEstates(knight);
            return knight != null && knight.Clan == Clan.PlayerClan &&
                !knight.IsClanLeader() && (titles.Count > 0 || estates.Count > 0);
        }

        private List<InquiryElement> GetAvailableTitles()
        {
            var result = new List<InquiryElement>();
            var titles = BannerKingsConfig.Instance.TitleManager.GetAllDeJure(Hero.MainHero);
            foreach (var title in titles)
            {
                if (title.TitleType != TitleType.Lordship || title.Fief == null || title.deJure != Hero.MainHero)
                {
                    continue;
                }

                var action = BannerKingsConfig.Instance.TitleModel.GetAction(ActionType.Grant, title, Hero.MainHero);
                result.Add(new InquiryElement(title, title.FullName.ToString(), null, action.Possible,
                    action.Reason.ToString()));
            }

            return result;
        }

        private List<InquiryElement> GetAvailableEstates()
        {
            var result = new List<InquiryElement>();
            var titles = BannerKingsConfig.Instance.TitleManager.GetAllDeJure(Hero.MainHero);
            foreach (var title in titles)
            {
                if (title.TitleType != TitleType.Lordship || title.Fief == null || title.deJure != Hero.MainHero)
                {
                    continue;
                }
                
                var settlement = title.Fief;
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                if (data.EstateData != null)
                {
                    foreach (var estate in data.EstateData.Estates)
                    {
                        if (estate.IsDisabled)
                        {
                            var action = BannerKingsConfig.Instance.EstatesModel.GetGrant(estate, Hero.MainHero, Hero.OneToOneConversationHero);
                            result.Add(new InquiryElement(estate,
                                new TextObject("{=68m3U2ey}{VILLAGE} - {ACREAGE} acres")
                                .SetTextVariable("VILLAGE", settlement.Name)
                                .SetTextVariable("ACREAGE", estate.Acreage.ToString("0.00"))
                                .ToString(),
                                null,
                                action.Possible,
                                action.Reason.ToString()));
                        }
                    }
                }
            }

            return result;
        }

        private bool GrantKnighthoodOnClickable(out TextObject reason)
        {
            var knight = Hero.OneToOneConversationHero;

            if (BannerKingsConfig.Instance.TitleManager.IsKnight(knight))
            {
                reason = new TextObject("{=t37Hiw4V}{HERO} already holds property or a title and is considered a knight.")
                        .SetTextVariable("HERO", knight.Name);
                return false;
            }

            if (Utils.Helpers.IsCloseFamily(knight, Hero.MainHero))
            {
                reason = new TextObject("{=xfcQrgtb}{HERO} is close family and in your clan. Their social status mirrors yours as head of family.")
                        .SetTextVariable("HERO", knight.Name);
                return false;
            }

            reason = new TextObject("{=OMJDisYX}Bestowing knighthood is possible.");
            return true;
        }

        private bool GrantKnighthoodEstateOnClickable(out TextObject reason)
        {
            estatesToGive.Clear();
            var estates = GetAvailableEstates();
            var titles = GetAvailableTitles();

            foreach (var estate in estates)
            {
                estatesToGive.Add(estate); 
            }

            var influence = BannerKingsConfig.Instance.TitleModel.GetGrantKnighthoodCost(Hero.MainHero);
            if (Clan.PlayerClan.Influence < influence.ResultNumber)
            {
                reason = new TextObject("{=rFXuWbdQ}Bestowing knighthood requires {INFLUENCE} influence to legitimize your new vassal.\n{EXPLANATIONS}")
                    .SetTextVariable("INFLUENCE", influence.ResultNumber)
                    .SetTextVariable("EXPLANATIONS", influence.GetExplanations());
                return false;
            }

            if (estates.Any(x => x.IsEnabled))
            {
                reason = new TextObject("{=OMJDisYX}Bestowing knighthood is possible.");
                return true;
            }

            if (estates.Count == 0)
            {
                reason = new TextObject("{=eT2e7JHu}Your lordships do not have any vacant estates to be granted away.");
                return false;
            }

            var first = estates.FirstOrDefault(x => !x.IsEnabled);
            reason = first != null ? new TextObject(first.Hint) : new TextObject("{=eT2e7JHu}Your lordships do not have any vacant estates to be granted away.");
            return false;
        }

        public bool GrantKnighthoodFiefOnClickable(out TextObject hintText)
        {
            var companion = Hero.OneToOneConversationHero;
            var title = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(Hero.MainHero);

            var council = BannerKingsConfig.Instance.CourtManager.GetCouncil(Hero.MainHero);
            if (council != null)
            {
                var peerage = council.Peerage;
                if (!peerage.CanGrantKnighthood)
                {
                    hintText = new TextObject("{=6oR14vwH}The {CLAN} does not have adequate Peerage to grant knighthood.")
                        .SetTextVariable("CLAN", Hero.MainHero.Clan.Name);
                    return false;
                }
            }

            var tier = Clan.PlayerClan.Tier;
            if (tier < 2)
            {
                hintText = new TextObject("{=NeMhiUW8}Your Clan Tier needs to be at least {TIER}.");
                hintText.SetTextVariable("TIER", 2);
                return false;
            }

            var kingdom = Clan.PlayerClan.Kingdom;
            if (kingdom == null)
            {
                hintText = new TextObject("{=gM4TtJCf}Before bestowing knighthood, you need to be formally part of a kingdom.");
                return false;
            }

            var influence = BannerKingsConfig.Instance.TitleModel.GetGrantKnighthoodCost(Hero.MainHero);
            if (Clan.PlayerClan.Influence < influence.ResultNumber)
            {
                hintText = new TextObject("{=rFXuWbdQ}Bestowing knighthood requires {INFLUENCE} influence to legitimize your new vassal.\n{EXPLANATIONS}")
                    .SetTextVariable("INFLUENCE", influence.ResultNumber)
                    .SetTextVariable("EXPLANATIONS", influence.GetExplanations());
                return false;
            }

            if (Hero.MainHero.Gold < 5000)
            {
                hintText = new TextObject("{=hN2Eynzu}Bestowing knighthood requires {GOLD} gold to give your vassal financial security.");
                hintText.SetTextVariable("GOLD", 5000);
                return false;
            }

            var titles = BannerKingsConfig.Instance.TitleManager.GetAllDeJure(Hero.MainHero);
            if (titles.Count == 0)
            {
                hintText = new TextObject("{=vNwX2dhL}You do not legally own any title.");
                return false;
            }
           
            if (titles.Count == 0)
            {
                hintText = new TextObject("{=D4PoLUwH}You currently do not lawfully own a lordship.");
                return false;
            }

            var lordships = GetAvailableTitles();
            lordshipsToGive.Clear();
            foreach (var lordship in lordships)
            {
                lordshipsToGive.Add(lordship);
            }

            if (lordshipsToGive.Count == 0)
            {
                hintText = new TextObject("{=D4PoLUwH}You currently do not lawfully own a lordship.");
                return false;
            }

            if (lordships.Any(x => x.IsEnabled))
            {
                hintText = new TextObject("{=hN2Eynzu}Bestowing knighthood requires {GOLD} gold to give your vassal financial security.");
                hintText.SetTextVariable("GOLD", 5000);
                return true;
            }

            var first = lordships.FirstOrDefault(x => !x.IsEnabled);
            hintText = first != null ? new TextObject(first.Hint) : new TextObject("{=gYPtXGDA}You currently do not lawfully own a lordship that could be given away.");
            return false;
        }

        public bool GrantPropertyFiefOnClickable(out TextObject hintText)
        {
            var companion = Hero.OneToOneConversationHero;
            var title = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(Hero.MainHero);

            var titles = BannerKingsConfig.Instance.TitleManager.GetAllDeJure(Hero.MainHero);
            if (titles.Count == 0)
            {
                hintText = new TextObject("{=vNwX2dhL}You do not legally own any title.");
                return false;
            }

            var lordships = GetAvailableTitles();
            lordshipsToGive.Clear();
            foreach (var lordship in lordships)
            {
                lordshipsToGive.Add(lordship);
            }

            if (lordshipsToGive.Count == 0)
            {
                hintText = new TextObject("{=D4PoLUwH}You currently do not lawfully own a lordship.");
                return false;
            }

            if (lordships.Any(x => x.IsEnabled))
            {
                hintText = new TextObject("{=BOhrKJtw}You have properties available to be granted.");
                return true;
            }

            var first = lordships.FirstOrDefault(x => !x.IsEnabled);
            hintText = first != null ? new TextObject(first.Hint) : new TextObject("{=gYPtXGDA}You currently do not lawfully own a lordship that could be given away.");
            return false;
        }

        private bool GrantPropertyEstateOnClickable(out TextObject hintText)
        {
            estatesToGive.Clear();
            var estates = GetAvailableEstates();
            var titles = GetAvailableTitles();

            foreach (var estate in estates)
            {
                estatesToGive.Add(estate);
            }

            if (estates.Any(x => x.IsEnabled))
            {
                hintText = new TextObject("{=BOhrKJtw}You have properties available to be granted.");
                return true;
            }

            if (estates.Count == 0)
            {
                hintText = new TextObject("{=eT2e7JHu}Your lordships do not have any vacant estates to be granted away.");
                return false;
            }

            var first = estates.FirstOrDefault(x => !x.IsEnabled);
            hintText = first != null ? new TextObject(first.Hint) : new TextObject("{=eT2e7JHu}Your lordships do not have any vacant estates to be granted away.");
            return false;
        }

        private void GrantProperty()
        {
            if (lordship != null)
            {
                var action = BannerKingsConfig.Instance.TitleModel.GetAction(ActionType.Grant, lordship, Hero.MainHero);
                action.TakeAction(Hero.OneToOneConversationHero);
                lordship = null;
            }
            else if (estate != null)
            {
                var action = BannerKingsConfig.Instance.EstatesModel.GetGrant(estate, Hero.MainHero, Hero.OneToOneConversationHero);
                action.TakeAction(Hero.OneToOneConversationHero);
                estate = null;
            }
        }

        private void GrantFiefOnConsequence()
        {
            if (lordship != null)
            {
                BannerKingsConfig.Instance.TitleManager.GrantKnighthood(lordship, Hero.OneToOneConversationHero, Hero.MainHero);
                lordship = null;
            }
        }

        private void GrantEstateOnConsequence()
        {
            if (estate != null)
            {
                BannerKingsConfig.Instance.TitleManager.GrantKnighthood(estate, Hero.OneToOneConversationHero, Hero.MainHero);
                estate = null;
            }
        }

        private void SelectFiefOnConsequence()
        {
            MBInformationManager
                .ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                    new TextObject("{=LxC6xyv4}Select the Lordship you would like to give away.").ToString(),
                    new TextObject("{=tujsZ1Mu}Lordships with more population and acreage will yield more income to their holder, and thus allow a vassal to field a bigger and better retinue.").ToString(), 
                    lordshipsToGive, 
                    false,
                    1,
                    GameTexts.FindText("str_done").ToString(), 
                    string.Empty,
                    delegate(List<InquiryElement> list)
                    {
                        lordship = (FeudalTitle)list[0].Identifier;
                    }, null, 
                    string.Empty));
        }

        private void SelectEstateOnConsequence()
        {
            MBInformationManager
                .ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                    new TextObject("{=tVvXpPdF}Select the estate you would like to grant.").ToString(),
                    new TextObject("{=CN94YRec}Estates with bigger acreage are likely to yield more income to their holder.").ToString(), 
                    estatesToGive, 
                    false, 
                    1,
                    GameTexts.FindText("str_done").ToString(), 
                    string.Empty,
                    delegate (List<InquiryElement> list)
                    {
                        estate = (Estate)list[0].Identifier;
                    }, null, 
                    string.Empty));
        }
    }

    namespace Patches
    {
        [HarmonyPatch(typeof(CompanionRolesCampaignBehavior))]
        internal class CompanionDialoguesPatches
        {
            [HarmonyPrefix]
            [HarmonyPatch("companion_fire_on_consequence", MethodType.Normal)]
            private static bool FireCompanionPrefix()
            {
                if (BannerKingsConfig.Instance.TitleManager.IsKnight(Hero.OneToOneConversationHero))
                {

                    InformationManager.DisplayMessage(new InformationMessage(new TextObject(
                        "{=EYH5bgdS}Not possible to dismiss a knight with property under your service.")
                        .ToString()));
                    return false;
                }
                
                return true;
            }
        }

        [HarmonyPatch(typeof(LordConversationsCampaignBehavior), "FindSuitableCompanionsToLeadCaravan")]
        internal class SuitableCaravanLeaderPatch
        {
            private static bool Prefix(ref List<CharacterObject> __result)
            {
                if (BannerKingsConfig.Instance.TitleManager == null)
                {
                    return true;
                }

                var list = new List<CharacterObject>();
                foreach (var troopRosterElement in MobileParty.MainParty.MemberRoster.GetTroopRoster())
                {
                    var heroObject = troopRosterElement.Character.HeroObject;
                    if (heroObject != null && heroObject != Hero.MainHero && heroObject.Clan == Clan.PlayerClan &&
                        heroObject.GovernorOf == null && heroObject.CanLeadParty())
                    {
                        if (!BannerKingsConfig.Instance.TitleManager.IsKnight(heroObject))
                        {
                            list.Add(troopRosterElement.Character);
                        }
                    }
                }

                __result = list;
                return false;
            }
        }

        [HarmonyPatch(typeof(Hero), "SetHeroEncyclopediaTextAndLinks")]
        internal class HeroDescriptionPatch
        {
            private static void Postfix(ref TextObject __result, Hero o)
            {
                /*var titles = BannerKingsConfig.Instance.TitleManager.GetAllDeJure(o);
                if (titles is {Count: > 0})
                {
                    var desc = "";
                    FeudalTitle current = null;
                    var finalList = titles.OrderBy(x => (int) x.TitleType).ToList();
                    var government = GovernmentType.Feudal;
                    foreach (var title in finalList)
                    {
                        if (title.Contract != null)
                        {
                            government = title.Contract.Government;
                        }

                        if (current == null)
                        {
                            desc += string.Format("{0} of {1}",
                                Utils.TextHelper.GetTitleHonorary(title.TitleType, government, false), title.shortName);
                        }
                        else if (current.TitleType == title.TitleType)
                        {
                            desc += ", " + title.shortName;
                        }
                        else if (current.TitleType != title.TitleType)
                        {
                            desc += string.Format(" and {0} of {1}",
                                Utils.Helpers.GetTitleHonorary(title.TitleType, government, false), title.shortName);
                        }

                        current = title;
                    }

                    __result = new TextObject("{=3Ug0Pp5p}{RESULT}\n{DESCRIPTION}")
                        .SetTextVariable("RESULT", __result.ToString())
                        .SetTextVariable("DESCRIPTION", desc);
                }*/
            }
        }

        [HarmonyPatch(typeof(LordConversationsCampaignBehavior), "conversation_lord_give_oath_go_on_condition")]
        internal class ShowContractPatch
        {
            private static void Postfix()
            {
                if (BannerKingsConfig.Instance.TitleManager != null)
                {
                    Hero lord = null;
                    var party = PlayerEncounter.EncounteredParty;
                    if (party != null)
                    {
                        lord = party.LeaderHero;
                    }

                    if (lord == null)
                    {
                        lord = Hero.OneToOneConversationHero;
                    }

                    BannerKingsConfig.Instance.TitleManager.ShowContract(lord, "I Accept");
                }
            }
        }

        [HarmonyPatch(typeof(ClanPartiesVM), "ExecuteCreateNewParty")]
        internal class ClanCreatePartyPatch
        {
            private static bool Prefix(ClanPartiesVM __instance, Clan ____faction)
            {
                if (!BannerKingsConfig.Instance.TitleManager.Knighthood)
                {
                    return true;
                }

                if (!__instance.CanCreateNewParty)
                {
                    return false;
                }

                List<InquiryElement> list = new List<InquiryElement>();
                foreach (Hero hero in
                    Enumerable.Union<Hero>(Enumerable.Where<Hero>(____faction.Heroes, (Hero h) => !h.IsDisabled),
                    ____faction.Companions))
                {
                    if ((hero.IsActive || hero.IsReleased || hero.IsFugitive) && !hero.IsChild && hero != Hero.MainHero && hero.CanLeadParty())
                    {
                        bool isEnabled = true;
                        var hintMethod = __instance.GetType().GetMethod("GetPartyLeaderAssignmentSkillsHint",
                            BindingFlags.NonPublic | BindingFlags.Instance);
                        var hint = (string)hintMethod.Invoke(__instance, new object[] { hero });
                        if (hero.PartyBelongedToAsPrisoner != null)
                        {
                            isEnabled = false;
                            hint = new TextObject("{=vOojEcIf}You cannot assign a prisoner member as a new party leader", null).ToString();
                        }
                        else if (hero.IsReleased)
                        {
                            isEnabled = false;
                            hint = new TextObject("{=OhNYkblK}This hero has just escaped from captors and will be available after some time.", null).ToString();
                        }
                        else if (hero.PartyBelongedTo != null && hero.PartyBelongedTo.LeaderHero == hero)
                        {
                            isEnabled = false;
                            hint = new TextObject("{=aFYwbosi}This hero is already leading a party.", null).ToString();
                        }
                        else if (hero.PartyBelongedTo != null && hero.PartyBelongedTo.LeaderHero != Hero.MainHero)
                        {
                            isEnabled = false;
                            hint = new TextObject("{=FjJi1DJb}This hero is already a part of an another party.", null).ToString();
                        }
                        else if (hero.GovernorOf != null)
                        {
                            isEnabled = false;
                            hint = new TextObject("{=Hz8XO8wk}Governors cannot lead a mobile party and be a governor at the same time.", null).ToString();
                        }
                        else if (hero.HeroState == Hero.CharacterStates.Disabled)
                        {
                            isEnabled = false;
                            hint = new TextObject("{=slzfQzl3}This hero is lost", null).ToString();
                        }
                        else if (hero.HeroState == Hero.CharacterStates.Fugitive)
                        {
                            isEnabled = false;
                            hint = new TextObject("{=dD3kRDHi}This hero is a fugitive and running from their captors. They will be available after some time.", null).ToString();
                        }

                        if (isEnabled)
                        {
                            if (!Utils.Helpers.IsCloseFamily(hero, Hero.MainHero) &&
                                                                       !BannerKingsConfig.Instance.TitleManager.IsKnight(hero))
                            {
                                hint = new TextObject("{=H48rhfyZ}A hero must be knighted and granted land before being able to raise a personal retinue. You may bestow knighthood by talking to them.")
                                    .ToString();
                                isEnabled = false;
                            }
                        }

                        list.Add(new InquiryElement(hero, 
                            hero.Name.ToString(), 
                            new ImageIdentifier(CampaignUIHelper.GetCharacterCode(hero.CharacterObject, false)), isEnabled, hint));
                    }
                }
                if (list.Count > 0)
                {
                    var method = __instance.GetType().GetMethod("OnNewPartySelectionOver",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                        new TextObject("{=pyhP5yWu}Select the Leader of the New Party").ToString(),
                        string.Empty, list, true, 1, GameTexts.FindText("str_done").ToString(), "",
                        delegate(List<InquiryElement> x) { method.Invoke(__instance, new object[] {x}); },
                        delegate(List<InquiryElement> x) { method.Invoke(__instance, new object[] {x}); }));
                }
                else
                {
                    MBInformationManager.AddQuickInformation(new TextObject("{=WGo5MzOB}There is no one available in your clan who can lead a party right now."));
                }

                return false;
            }
        }
    }
}