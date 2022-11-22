using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using BannerKings.Actions;
using BannerKings.Managers.Populations.Estates;
using BannerKings.Managers.Titles;
using HarmonyLib;
using SandBox.CampaignBehaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement.Categories;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using ActionType = BannerKings.Managers.Titles.ActionType;

namespace BannerKings.Behaviours
{
    public class BKKnighthoodBehavior : CampaignBehaviorBase
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
            if (hero.Clan == null || hero == hero.Clan.Leader || hero == Hero.MainHero ||
                hero.Occupation != Occupation.Lord ||
                BannerKingsConfig.Instance.TitleManager == null)
            {
                return;
            }

            var title = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(hero);
            if (title == null || title.type != TitleType.Lordship || title.fief.Village == null)
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
                var clanName = ClanActions.CanCreateNewClan(hero, title.fief);
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
                else if (hero.IsEnemy(Hero.MainHero))
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
        }

        private void CreateClan(Hero hero, Clan originalClan, FeudalTitle title, TextObject name = null)
        {
            var newClan = ClanActions.CreateNewClan(hero, title.fief, hero.StringId + "_knight_clan", name, 150f, true);
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
                new TextObject("{=!}I would like you to serve as my knight.").ToString(),
                GrantKnighthoodOnCondition,
                delegate
                {
                    InformationManager.ShowInquiry(new InquiryData(new TextObject("{=!}Bestowing Knighthood").ToString(), 
                        new TextObject("{=!}Knighting involves granting a Fee (an estate or lordship) to a commoner, in exchange for Knight Service, or military aid. Once granted, their property can not be revoked. Knights granted a Lordship will pay for their own retinue, but will also have full rights to the village's income. Knights will serve under your clan, but eventually will aim to forge their own family.")
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
                new TextObject("{=!}{TITLE}, it would be my honor. I must ask, what kind of property would you honor me with?")
                .SetTextVariable("TITLE", Hero.MainHero.IsFemale ? GameTexts.FindText("str_my_lady") : GameTexts.FindText("str_my_lord"))
                .ToString(), 
                null, 
                null);


            starter.AddPlayerLine("companion_grant_knighthood_response_lordship", "companion_knighthood_response",
              "companion_knighthood_accepted_fief",
                  new TextObject("{=!}A Lordship.").ToString(),
                  null,
                  SelectFiefOnConsequence,
                  100,
                  GrantKnighthoodFiefOnClickable);


            starter.AddPlayerLine("companion_grant_knighthood_response_estate", "companion_knighthood_response",
                "companion_knighthood_accepted_estate", 
                new TextObject("{=!}An estate property.").ToString(),
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
                new TextObject("{=!}{TITLE}, a Lordship would be very generous. Its income would allow me to raise a retinue and be fully independent to pay for it. Of course, the more rent a village produces, the more troops I can provide us.")
                .SetTextVariable("TITLE", Hero.MainHero.IsFemale ? GameTexts.FindText("str_my_lady") : GameTexts.FindText("str_my_lord"))
                .ToString(),
                null,
                null);


            starter.AddDialogLine("companion_knighthood_accepted_estate", "companion_knighthood_accepted_estate",
                "companion_knighthood_finish_estate",
                new TextObject("{=!}{TITLE}, an estate would be generous. Its income would help pay a retinue, and it's manpower allow us more access to volunteers. Estates with more acres generate more income and provide more manpower.")
                .SetTextVariable("TITLE", Hero.MainHero.IsFemale ? GameTexts.FindText("str_my_lady") : GameTexts.FindText("str_my_lord"))
                .ToString(),
                null,
                null);



            starter.AddPlayerLine("companion_knighthood_finish_fief", "companion_knighthood_finish_fief",
                "companion_knighthood_finished",
                new TextObject("It is decided then. I bestow upon you the title of Knight.").ToString(),
                null, 
                GrantFiefOnConsequence);

            starter.AddPlayerLine("companion_knighthood_finish_fief_return", "companion_knighthood_finish_fief",
                "close_window",
                "{=G4ALCxaA}Never mind.",
                null,
                null);


            starter.AddPlayerLine("companion_knighthood_finish_estate", "companion_knighthood_finish_estate",
                "companion_knighthood_finished",
               new TextObject("It is decided then. I bestow upon you the title of Knight.").ToString(),
               null, 
               GrantEstateOnConsequence);

            starter.AddPlayerLine("companion_knighthood_finish_estate_return", "companion_knighthood_finish_estate",
                "close_window",
                "{=G4ALCxaA}Never mind.",
                null,
                null);


            starter.AddDialogLine("companion_knighthood_finished", "companion_knighthood_finished",
                "close_window",
                new TextObject("{=!}Thank you {TITLE}!")
                .SetTextVariable("TITLE", Hero.MainHero.IsFemale ? GameTexts.FindText("str_my_lady") : GameTexts.FindText("str_my_lord"))
                .ToString(),
                null,
                null);
        }

        private bool GrantKnighthoodOnCondition()
        {
            if (BannerKingsConfig.Instance.TitleManager == null)
            {
                return false;
            }

            var companion = Hero.OneToOneConversationHero;
            return companion != null && companion.Clan == Clan.PlayerClan &&
                   BannerKingsConfig.Instance.TitleManager.Knighthood;
        }


        private List<InquiryElement> GetAvailableTitles()
        {
            var result = new List<InquiryElement>();
            var titles = BannerKingsConfig.Instance.TitleManager.GetAllDeJure(Hero.MainHero);
            foreach (var title in titles)
            {
                if (title.type != TitleType.Lordship || title.fief == null || title.deJure != Hero.MainHero)
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
                if (title.type != TitleType.Lordship || title.fief == null || title.deJure != Hero.MainHero)
                {
                    continue;
                }

                
                var settlement = title.fief;
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                if (data.EstateData != null)
                {
                    foreach (var estate in data.EstateData.Estates)
                    {
                        if (estate.IsDisabled)
                        {
                            var action = BannerKingsConfig.Instance.EstatesModel.GetGrant(estate, Hero.MainHero, Hero.OneToOneConversationHero);
                            result.Add(new InquiryElement(estate,
                                new TextObject("{=!}{VILLAGE} - {ACREAGE}")
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
                reason = new TextObject("{=!}{HERO} already holds property or a title and is considered a knight.")
                        .SetTextVariable("HERO", knight.Name);
                return false;
            }

            if (Utils.Helpers.IsCloseFamily(knight, Hero.MainHero))
            {
                reason = new TextObject("{=!}{HERO} is close family and in your clan. Their social status mirrors yours as head of family.")
                        .SetTextVariable("HERO", knight.Name);
                return false;
            }

            reason = new TextObject("{=!}Bestowing knighthood is possible.");
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

            if (titles.Count == 0)
            {
                reason = new TextObject("{=!}You currently do not lawfully own a lordship.");
                return false;
            }

            if (estates.Any(x => x.IsEnabled))
            {
                reason = new TextObject("{=!}Bestowing knighthood is possible.");
                return true;
            }

            if (estates.Count == 0)
            {
                reason = new TextObject("{=!}Your lordships do not have any vacant estates to be granted away.");
                return false;
            }

            var first = estates.FirstOrDefault(x => !x.IsEnabled);
            reason = first != null ? new TextObject(first.Hint) : new TextObject("{=!}Your lordships do not have any vacant estates to be granted away.");
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
                    hintText = new TextObject("{=!}The {CLAN} does not have adequate Peerage to grant knighthood.")
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
                hintText = new TextObject("{=!}You currently do not lawfully own a lordship.");
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
                hintText = new TextObject("{=!}You currently do not lawfully own a lordship.");
                return false;
            }

            if (lordships.Any(x => x.IsEnabled))
            {
                hintText = new TextObject("{=hN2Eynzu}Bestowing knighthood requires {GOLD} gold to give your vassal financial security.");
                hintText.SetTextVariable("GOLD", 5000);
                return true;
            }

            var first = lordships.FirstOrDefault(x => !x.IsEnabled);
            hintText = first != null ? new TextObject(first.Hint) : new TextObject("{=!}You currently do not lawfully own a lordship that could be given away.");
            return false;
        }

        private void GrantFiefOnConsequence()
        {
            if (lordship != null)
            {
                BannerKingsConfig.Instance.TitleManager.GrantKnighthood(lordship, Hero.OneToOneConversationHero, Hero.MainHero);
            }
        }

        private void GrantEstateOnConsequence()
        {
            if (estate != null)
            {
                BannerKingsConfig.Instance.TitleManager.GrantKnighthood(estate, Hero.OneToOneConversationHero, Hero.MainHero);
            }
        }

        private void SelectFiefOnConsequence()
        {
            MBInformationManager
                .ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                    new TextObject("{=!}Select the Lordship you would like to give away. Grating a lordship also requires a 5000 {GOLD_ICON} payment.").ToString(),
                    string.Empty, 
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
                    new TextObject("{=!}Select the estate you would like to grant.").ToString(),
                    string.Empty, 
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
                        "{=!}Not possible to dismiss a knight with property under your service.")
                        .ToString()));
                    return false;
                }
                
                return true;
            }

            [HarmonyPrefix]
            [HarmonyPatch("fief_grant_answer_consequence", MethodType.Normal)]
            private static bool GrantPeeragePrefix()
            {
                ConversationSentence.SetObjectsToRepeatOver((from x in Hero.MainHero.Clan.Settlements
                                                             where x.IsTown || x.IsCastle &&
                                                             BannerKingsConfig.Instance.TitleManager.GetTitle(x).deJure == Hero.MainHero
                                                             select x).ToList<Settlement>(), 5);

                return false;
            }

            [HarmonyPostfix]
            [HarmonyPatch("ClanNameSelectionIsDone", MethodType.Normal)]
            private static void GrantPeerageFinishedPostfix(CompanionRolesCampaignBehavior __instance)
            {
                Settlement settlement = (Settlement)AccessTools.Field(__instance.GetType(), "_selectedFief").GetValue(__instance);
                var title = BannerKingsConfig.Instance.TitleManager.GetTitle(settlement);
                var action = BannerKingsConfig.Instance.TitleModel.GetAction(ActionType.Grant, title, Hero.MainHero);
                BannerKingsConfig.Instance.TitleManager.GrantTitle(action, Hero.OneToOneConversationHero);
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
                var titles = BannerKingsConfig.Instance.TitleManager.GetAllDeJure(o);
                if (titles is {Count: > 0})
                {
                    var desc = "";
                    FeudalTitle current = null;
                    var finalList = titles.OrderBy(x => (int) x.type).ToList();
                    var government = GovernmentType.Feudal;
                    foreach (var title in finalList)
                    {
                        if (title.contract != null)
                        {
                            government = title.contract.Government;
                        }

                        if (current == null)
                        {
                            desc += string.Format("{0} of {1}",
                                Utils.Helpers.GetTitleHonorary(title.type, government, false), title.shortName);
                        }
                        else if (current.type == title.type)
                        {
                            desc += ", " + title.shortName;
                        }
                        else if (current.type != title.type)
                        {
                            desc += string.Format(" and {0} of {1}",
                                Utils.Helpers.GetTitleHonorary(title.type, government, false), title.shortName);
                        }

                        current = title;
                    }

                    __result = new TextObject("{=3Ug0Pp5p}{RESULT}\n{DESCRIPTION}")
                        .SetTextVariable("RESULT", __result.ToString())
                        .SetTextVariable("DESCRIPTION", desc);
                }
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

                if (__instance.CanCreateNewParty)
                {
                    var list = new List<InquiryElement>();
                    foreach (var hero in (from h in ____faction.Heroes
                                 where !h.IsDisabled
                                 select h).Union(____faction.Companions))
                    {
                        if ((hero.IsActive || hero.IsReleased || hero.IsFugitive) && !hero.IsChild &&
                            hero != Hero.MainHero && hero.CanLeadParty())
                        {
                            var isEnabled = false;
                            var hintMethod = __instance.GetType().GetMethod("GetPartyLeaderAssignmentSkillsHint",
                                BindingFlags.NonPublic | BindingFlags.Instance);
                            var hint = (string) hintMethod.Invoke(__instance, new object[] {hero});
                            if (hero.PartyBelongedToAsPrisoner != null)
                            {
                                hint = new TextObject("{=A2qwGxzR}You cannot assign a prisoner member as a new party leader").ToString();
                            }
                            else if (hero.IsReleased)
                            {
                                hint = new TextObject("{=XX95HPvL}This hero has just escaped from captors and will be available after some time.")
                                    .ToString();
                            }
                            else if (hero.PartyBelongedTo != null && hero.PartyBelongedTo.LeaderHero == hero)
                            {
                                hint = new TextObject("{=DKZjQeN1}This hero is already leading a party.").ToString();
                            }
                            else if (hero.PartyBelongedTo != null && hero.PartyBelongedTo.LeaderHero != Hero.MainHero)
                            {
                                hint = new TextObject("{=P11B5nA2}This hero is already a part of an another party.")
                                    .ToString();
                            }
                            else if (hero.GovernorOf != null)
                            {
                                hint = new TextObject("{=RND66CoK}Governors cannot lead a mobile party and be a governor at the same time.")
                                    .ToString();
                            }
                            else
                            {
                                switch (hero.HeroState)
                                {
                                    case Hero.CharacterStates.Disabled:
                                        hint = new TextObject("{=MiVV06Yo}This hero is lost").ToString();
                                        break;
                                    case Hero.CharacterStates.Fugitive:
                                        hint = new TextObject("{=iWxEQVYg}This hero is a fugitive and running from their captors. They will be available after some time.")
                                            .ToString();
                                        break;
                                    default:
                                    {
                                        if (!Utils.Helpers.IsCloseFamily(hero, Hero.MainHero) &&
                                            !BannerKingsConfig.Instance.TitleManager.IsKnight(hero))
                                        {
                                            hint = new TextObject("{=H48rhfyZ}A hero must be knighted and granted land before being able to raise a personal retinue. You may bestow knighthood by talking to them.")
                                                .ToString();
                                        }
                                        else
                                        {
                                            isEnabled = true;
                                        }

                                        break;
                                    }
                                }
                            }

                            list.Add(new InquiryElement(hero, hero.Name.ToString(),
                                new ImageIdentifier(CampaignUIHelper.GetCharacterCode(hero.CharacterObject)), isEnabled,
                                hint));
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
                }

                return false;
            }
        }

        /*
        [HarmonyPatch(typeof(Settlement))]
        [HarmonyPatch("Owner", MethodType.Getter)]
        class VillageOwnerPatch
        {
            static void Postfix(Settlement __instance, ref Hero __result)
            {
                if (__instance.IsVillage && BannerKingsConfig.Instance.TitleManager != null)
                {
                    FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetTitle(__instance);
                    if (title != null)
                        __result = title.deFacto;
                }
            }
        } */
    }
}