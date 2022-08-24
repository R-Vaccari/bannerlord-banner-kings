using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using BannerKings.Actions;
using BannerKings.Managers.Titles;
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

namespace BannerKings.Behaviours
{
    public class BKKnighthoodBehavior : CampaignBehaviorBase
    {
        private readonly List<InquiryElement> lordshipsToGive = new();

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
                        new TextObject(
                            "{=!}I humbly ask of you to release me of my duties in the {CLAN}. I shall remain as your vassal and loyal friend.");
                }
                else if (hero.IsEnemy(Hero.MainHero))
                {
                    requestText =
                        new TextObject(
                            "{=!}I request of you to release me of my duties in the {CLAN}. It is time for me to lead my own family.");
                }
                else
                {
                    requestText =
                        new TextObject("{=!}I demand of you to release me of the {CLAN}. It is time we part ways.");
                }

                requestText = requestText.SetTextVariable("CLAN", originalClan.Name);

                var cost = BannerKingsConfig.Instance.InfluenceModel.GetRejectKnighthoodCost(originalClan);
                InformationManager.ShowInquiry(new InquiryData(
                    new TextObject("The clan of {HERO}").SetTextVariable("HERO", hero.Name).ToString(),
                    new TextObject(
                            "{GREETING} {PLAYER}, {TEXT}\nRejecting their request would cost {INFLUENCE} influence.")
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
                    new TextObject("{=!}The {NEW} has been formed by {HERO}, previously a knight of {ORIGINAL}.")
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
            var knighthoodSb = new StringBuilder();
            knighthoodSb.Append(
                "By knighting, you are granting this person nobility and they will be bound to you as your vassal by the standard contract of the kingdom. A lordship must be given away to seal the contract.");
            knighthoodSb.Append(Environment.NewLine);
            knighthoodSb.Append(" ");
            knighthoodSb.Append(Environment.NewLine);
            knighthoodSb.Append(
                "Their lands and titles henceforth can not be revoked without lawful cause, and any fief revenue will be theirs, taxed or not by you as per contract");
            knighthoodSb.Append(Environment.NewLine);
            knighthoodSb.Append(" ");
            knighthoodSb.Append(Environment.NewLine);
            knighthoodSb.Append(
                "As a knight, they are capable of raising a personal retinue to serve your clan and are obliged to fulfill their duties.");

            starter.AddPlayerLine("companion_grant_knighthood", "companion_role", "companion_knighthood_question",
                "Would you like to serve me as my knight?",
                GrantKnighthoodOnCondition,
                delegate
                {
                    InformationManager.ShowInquiry(new InquiryData("Bestowing Knighthood", knighthoodSb.ToString(),
                        true, false, "Understood", null, null, null));
                }, 100, GrantKnighthoodOnClickable);

            starter.AddDialogLine("companion_grant_knighthood_response", "companion_knighthood_question",
                "companion_knighthood_response",
                "My lord, I would be honored.", null, null);

            starter.AddPlayerLine("companion_grant_knighthood_response_confirm", "companion_knighthood_response",
                "companion_knighthood_accepted", "Let us decide your fief.",
                CompanionKnighthoodAcceptedCondition, GrantKnighthoodOnConsequence);

            starter.AddPlayerLine("companion_grant_knighthood_response_cancel", "companion_knighthood_response",
                "companion_role_pretalk", "Actualy, I would like to discuss this at a later time.",
                null, null);

            starter.AddPlayerLine("companion_grant_knighthood_granted", "companion_knighthood_accepted", "close_window",
                "It is decided then. I bestow upon you the title of Knight.",
                null, null);
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

        private bool CompanionKnighthoodAcceptedCondition()
        {
            lordshipsToGive.Clear();
            var titles = BannerKingsConfig.Instance.TitleManager.GetAllDeJure(Hero.MainHero);
            foreach (var title in titles)
            {
                if (title.type != TitleType.Lordship || title.fief == null || title.deJure != Hero.MainHero)
                {
                    continue;
                }

                var action = BannerKingsConfig.Instance.TitleModel.GetAction(ActionType.Grant, title, Hero.MainHero);
                lordshipsToGive.Add(new InquiryElement(title, title.FullName.ToString(), null, action.Possible,
                    action.Reason.ToString()));
            }

            if (lordshipsToGive.Count == 0)
            {
                InformationManager.DisplayMessage(
                    new InformationMessage("You currently do not lawfully own a lordship that could be given away."));
            }

            return lordshipsToGive.Count >= 1;
        }

        public bool GrantKnighthoodOnClickable(out TextObject hintText)
        {
            var companion = Hero.OneToOneConversationHero;
            var title = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(Hero.MainHero);

            var tier = Clan.PlayerClan.Tier;
            if (tier < 2)
            {
                hintText = new TextObject("{=!}Your Clan Tier needs to be at least {TIER}.");
                hintText.SetTextVariable("TIER", 2);
                return false;
            }

            var kingdom = Clan.PlayerClan.Kingdom;
            if (kingdom == null)
            {
                hintText = new TextObject(
                    "{=!}Before bestowing knighthood, you need to be formally part of a kingdom.");
                return false;
            }

            var titles = BannerKingsConfig.Instance.TitleManager.GetAllDeJure(Hero.MainHero);
            if (titles.Count == 0)
            {
                hintText = new TextObject("{=!}You do not legally own any title.");
                return false;
            }

            var lordships = titles.FindAll(x => x.type == TitleType.Lordship);
            switch (lordships.Count)
            {
                case 0:
                    hintText = new TextObject(
                        "{=!}You do not legally own any lordship that could be given to land a new vassal.");
                    return false;
                case 1:
                    hintText = new TextObject("{=!}You cannot grant away your only lordship.");
                    return false;
            }

            var influence = BannerKingsConfig.Instance.TitleModel.GetGrantKnighthoodCost(Hero.MainHero);
            if (Clan.PlayerClan.Influence < influence.ResultNumber)
            {
                hintText = new TextObject(
                        "{=!}Bestowing knighthood requires {INFLUENCE} influence to legitimize your new vassal.\n{EXPLANATIONS}")
                    .SetTextVariable("INFLUENCE", influence.ResultNumber)
                    .SetTextVariable("EXPLANATIONS", influence.GetExplanations());
                return false;
            }


            hintText = new TextObject(
                "{=!}Bestowing knighthood requires {GOLD} gold to give your vassal financial security.");
            hintText.SetTextVariable("GOLD", 5000);

            return Hero.MainHero.Gold >= 5000;
        }

        private void GrantKnighthoodOnConsequence()
        {
            MBInformationManager
                .ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                    new TextObject("{=!}Select the fief you would like to give away").ToString(),
                    string.Empty, lordshipsToGive, false, 1,
                    GameTexts.FindText("str_done").ToString(), string.Empty,
                    delegate(List<InquiryElement> list)
                    {
                        BannerKingsConfig.Instance.TitleManager.GrantKnighthood((FeudalTitle) list[0].Identifier,
                            Hero.OneToOneConversationHero, Hero.MainHero);
                    }, null, string.Empty));
        }
    }

    namespace Patches
    {
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
                        var title = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(heroObject);
                        if (title == null)
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

                    __result = new TextObject("{=!}{RESULT}\n{DESCRIPTION}")
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
                                hint = new TextObject(
                                    "{=vOojEcIf}You cannot assign a prisoner member as a new party leader").ToString();
                            }
                            else if (hero.IsReleased)
                            {
                                hint = new TextObject(
                                        "{=OhNYkblK}This hero has just escaped from captors and will be available after some time.")
                                    .ToString();
                            }
                            else if (hero.PartyBelongedTo != null && hero.PartyBelongedTo.LeaderHero == hero)
                            {
                                hint = new TextObject("{=aFYwbosi}This hero is already leading a party.").ToString();
                            }
                            else if (hero.PartyBelongedTo != null && hero.PartyBelongedTo.LeaderHero != Hero.MainHero)
                            {
                                hint = new TextObject("{=FjJi1DJb}This hero is already a part of an another party.")
                                    .ToString();
                            }
                            else if (hero.GovernorOf != null)
                            {
                                hint = new TextObject(
                                        "{=Hz8XO8wk}Governors cannot lead a mobile party and be a governor at the same time.")
                                    .ToString();
                            }
                            else
                            {
                                switch (hero.HeroState)
                                {
                                    case Hero.CharacterStates.Disabled:
                                        hint = new TextObject("{=slzfQzl3}This hero is lost").ToString();
                                        break;
                                    case Hero.CharacterStates.Fugitive:
                                        hint = new TextObject(
                                                "{=dD3kRDHi}This hero is a fugitive and running from their captors. They will be available after some time.")
                                            .ToString();
                                        break;
                                    default:
                                    {
                                        if (!Utils.Helpers.IsCloseFamily(hero, Hero.MainHero) &&
                                            !BannerKingsConfig.Instance.TitleManager.IsHeroKnighted(hero))
                                        {
                                            hint = new TextObject(
                                                    "A hero must be knighted and granted land before being able to raise a personal retinue. You may bestow knighthood by talking to them.")
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
                            new TextObject("{=0Q4Xo2BQ}Select the Leader of the New Party").ToString(),
                            string.Empty, list, true, 1, GameTexts.FindText("str_done").ToString(), "",
                            delegate(List<InquiryElement> x) { method.Invoke(__instance, new object[] {x}); },
                            delegate(List<InquiryElement> x) { method.Invoke(__instance, new object[] {x}); }));
                    }
                    else
                    {
                        MBInformationManager.AddQuickInformation(new TextObject(
                            "{=qZvNIVGV}There is no one available in your clan who can lead a party right now."));
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