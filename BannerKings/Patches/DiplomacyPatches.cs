using BannerKings.Behaviours.Diplomacy.Wars;
using BannerKings.Behaviours.Diplomacy;
using BannerKings.Models.Vanilla;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.Diplomacy;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Party;
using SandBox.View.Map;
using TaleWorlds.CampaignSystem.BarterSystem.Barterables;
using TaleWorlds.CampaignSystem.CampaignBehaviors.BarterBehaviors;

namespace BannerKings.Patches
{
    internal class DiplomacyPatches
    {
        //AI companion dialogue fixes
        [HarmonyPatch(typeof(FactionManager))]
        internal class LordDialoguePatches
        {
            [HarmonyPostfix]
            [HarmonyPatch("DeclareAlliance")]
            private static void DeclareAlliance(IFaction faction1, IFaction faction2)
            {
                if (faction1 != faction2 && !faction1.IsBanditFaction && !faction2.IsBanditFaction)
                {
                    StanceLink link1 = faction1.GetStanceWith(faction2);
                    link1.IsAllied = true;

                    StanceLink link2 = faction2.GetStanceWith(faction1);
                    link2.IsAllied = true;
                }

                UpdateVisuals(faction1, faction2);
            }

            [HarmonyPostfix]
            [HarmonyPatch("SetNeutral")]
            private static void SetNeutral(IFaction faction1, IFaction faction2)
            {
                if (faction1 != faction2 && !faction1.IsBanditFaction && !faction2.IsBanditFaction)
                {
                    StanceLink link = faction1.GetStanceWith(faction2);
                    link.IsAllied = false;
                }

                UpdateVisuals(faction1, faction2);
            }

            [HarmonyPrefix]
            [HarmonyPatch("DeclareWar")]
            private static bool DeclareWar(IFaction faction1, IFaction faction2)
            {
                if (faction1 != faction2 && !faction1.IsBanditFaction && !faction2.IsBanditFaction)
                {
                    StanceLink link = faction1.GetStanceWith(faction2);
                    link.IsAllied = false;
                }

                return true;
            }

            private static void UpdateVisuals(IFaction faction1, IFaction faction2)
            {
                if (CharacterObject.PlayerCharacter != null && Hero.MainHero != null && (faction1 == Hero.MainHero.MapFaction || faction2 == Hero.MainHero.MapFaction))
                {
                    IFaction dirtySide = (faction1 == Hero.MainHero.MapFaction) ? faction2 : faction1;
                    foreach (Settlement settlement in Settlement.All.Where((Settlement party) => party.IsVisible && party.MapFaction == dirtySide))
                        settlement.Party.SetVisualAsDirty();

                    foreach (MobileParty mobileParty in MobileParty.All.Where((MobileParty party) => party.IsVisible && party.MapFaction == dirtySide))
                        mobileParty.Party.SetVisualAsDirty();
                }
            }
        }

        /*[HarmonyPatch(typeof(Clan), "MapFaction", MethodType.Getter)]
        internal class ClanFactionPatch
        {
            private static BKDiplomacyBehavior behavior;
            private static BKDiplomacyBehavior Behavior
            {
                get
                {
                    if (behavior == null)
                    {
                        behavior = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKDiplomacyBehavior>();
                    }

                    return behavior;
                }
            }
            private static bool Prefix(Clan __instance, ref IFaction __result)
            {
                if (__instance.Kingdom != null)
                {
                    if (Behavior.IsRebelling(__instance.Kingdom))
                    {
                        __result = __instance;
                        return false;
                    }
                }

                return true;
            }
        }*/

        [HarmonyPatch(typeof(KingdomDecisionProposalBehavior), "ConsiderWar")]
        internal class ConsiderWarPatch
        {
            private static bool Prefix(Clan clan, Kingdom kingdom, IFaction otherFaction, ref bool __result)
            {
                __result = false;
                return false;
            }
        }

        [HarmonyPatch(typeof(DiplomaticBartersBehavior), "ConsiderClanLeaveAsMercenary")]
        internal class ConsiderClanLeaveAsMercenaryPatch
        {
            private static bool Prefix(Clan clan)
            {
                LeaveKingdomAsClanBarterable leaveKingdomAsClanBarterable = new LeaveKingdomAsClanBarterable(clan.Leader, null);
                MercenaryJoinKingdomBarterable mercenaryJoinKingdomBarterable = new MercenaryJoinKingdomBarterable(clan.Leader, null, clan.Kingdom);
                if (leaveKingdomAsClanBarterable.GetValueForFaction(clan) > mercenaryJoinKingdomBarterable.GetValueForFaction(clan))
                {
                    leaveKingdomAsClanBarterable.Apply();
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(KingdomDiplomacyVM))]
        internal class DeclareWarVMPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch("CalculateWarSupport")]
            private static bool CalculateWarSupportText(KingdomDiplomacyVM __instance, IFaction faction, ref int __result)
            {
                __result = MathF.Round(new KingdomElection(
                    new BKDeclareWarDecision(null, Clan.PlayerClan, faction)).GetLikelihoodForSponsor(Clan.PlayerClan) * 100f);
                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch("GetActionStatusForDiplomacyItemWithReason")]
            private static bool ButtonCLickable(KingdomDiplomacyVM __instance, KingdomDiplomacyItemVM item, bool isResolve,
                 out TextObject disabledReason, ref bool __result)
            {
                KingdomTruceItemVM kingdomTruceItemVM;
                if (__result == false && (kingdomTruceItemVM = (item as KingdomTruceItemVM)) != null)
                {
                    disabledReason = TextObject.Empty;
                    __result = true;
                    return false;
                }

                disabledReason = TextObject.Empty;
                return true;
            }

            [HarmonyPrefix]
            [HarmonyPatch("OnDeclareWar")]
            private static bool ButtonPopup(KingdomDiplomacyVM __instance, KingdomTruceItemVM item)
            {
                IFaction enemy = item.Faction2;
                if (!enemy.IsKingdomFaction)
                {
                    return true;
                }

                Kingdom enemyKingdom = enemy as Kingdom;
                Kingdom kingdom = item.Faction1 as Kingdom;
                KingdomDiplomacy diplomacy = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKDiplomacyBehavior>().GetKingdomDiplomacy(kingdom);
                if (diplomacy == null)
                {
                    return true;
                }

                if (kingdom.UnresolvedDecisions.Any(x => x is DeclareWarDecision || x is BKDeclareWarDecision))
                {
                    InformationManager.DisplayMessage(new InformationMessage(
                        new TextObject("{=ZdWBHwQL}A war declaration is being voted upon concerning the {FACTION}.")
                        .SetTextVariable("FACTION", enemyKingdom.Name)
                        .ToString()));
                }
                else
                {
                    var list = new List<InquiryElement>();
                    BKKingdomDecisionModel model = new BKKingdomDecisionModel();
                    Action<KingdomDiplomacy, Kingdom, KingdomDiplomacyVM> makeWar = ShowWarOptions;
                    TextObject warHint;
                    bool warPossible = model.IsWarDecisionAllowedBetweenKingdoms(kingdom, enemyKingdom, out warHint);
                    list.Add(new InquiryElement(makeWar,
                        new TextObject("{=eQNY5hgE}Declare War ({INFLUENCE}{INFLUENCE_ICON})")
                        .SetTextVariable("INFLUENCE", BannerKingsConfig.Instance.DiplomacyModel.GetInfluenceCostOfProposingWar(Clan.PlayerClan))
                        .SetTextVariable("INFLUENCE_ICON", Utils.TextHelper.INFLUENCE_ICON)
                        .ToString(),
                        null,
                        warPossible,
                        warHint.ToString()));

                    Action<KingdomDiplomacy, Kingdom, KingdomDiplomacyVM> makeAlliance = ShowAlliance;
                    TextObject allianceHint;
                    bool alliancePossible = model.IsAllianceAllowed(kingdom, enemyKingdom, out allianceHint);       
                    list.Add(new InquiryElement(makeAlliance,
                        new TextObject("{=EBhcYsfJ}Propose Alliance ({DENARS} {GOLD_ICON})")
                        .SetTextVariable("DENARS", 
                        MBRandom.RoundRandomized(BannerKingsConfig.Instance.DiplomacyModel.GetAllianceDenarCost(diplomacy.Kingdom, enemyKingdom).ResultNumber))
                        .ToString(),
                        null,
                        alliancePossible,
                        new TextObject("{=8Z0e830N}Propose a truce between both realms. A truce is a period of a certain amount of years in which both realms formally agree to not declare wars upon each other, in mutual benefit. The proposing realm is assumed to be the major beneficiary of this agreement, and thus is required a fee. The proposed realm is more likely to accept and offer better terms relative to how advantageous a truce is for them.\n\n{POSSIBLE}")
                        .SetTextVariable("POSSIBLE", allianceHint)
                        .ToString()));

                    bool playerRuler = Hero.MainHero == Clan.PlayerClan.Kingdom.RulingClan.Leader;
                    Action<KingdomDiplomacy, Kingdom, KingdomDiplomacyVM> makeTruce = ShowTruce;
                    TextObject truceHint;
                    bool trucePossible = model.IsTruceAllowed(kingdom, enemyKingdom, out truceHint);
                    list.Add(new InquiryElement(makeTruce,
                        new TextObject("{=K177C8ia}Propose Truce ({DENARS} {GOLD_ICON})")
                        .SetTextVariable("DENARS",
                        MBRandom.RoundRandomized(BannerKingsConfig.Instance.DiplomacyModel.GetTruceDenarCost(diplomacy.Kingdom, enemyKingdom).ResultNumber))
                        .ToString(),
                        null,
                        trucePossible && playerRuler,
                        new TextObject("{=8Z0e830N}Propose a truce between both realms. A truce is a period of a certain amount of years in which both realms formally agree to not declare wars upon each other, in mutual benefit. The proposing realm is assumed to be the major beneficiary of this agreement, and thus is required a fee. The proposed realm is more likely to accept and offer better terms relative to how advantageous a truce is for them.\n\n{POSSIBLE}")
                        .SetTextVariable("POSSIBLE", truceHint)
                        .ToString()));

                    Action<KingdomDiplomacy, Kingdom, KingdomDiplomacyVM> makePact = ShowTradePact;
                    TextObject tradeHint;
                    bool tradePossible = model.IsTradePactAllowed(kingdom, enemyKingdom, out tradeHint);
                    list.Add(new InquiryElement(makePact,
                        new TextObject("{=jHuXS5zK}Propose Trade Pact ({INFLUENCE}{INFLUENCE_ICON})")
                        .SetTextVariable("INFLUENCE", 
                        MBRandom.RoundRandomized(BannerKingsConfig.Instance.DiplomacyModel.GetTradePactInfluenceCost(diplomacy.Kingdom, enemyKingdom).ResultNumber))
                        .SetTextVariable("INFLUENCE_ICON", Utils.TextHelper.INFLUENCE_ICON)
                        .ToString(),
                        null,
                        tradePossible && playerRuler,
                        new TextObject("{=qEYgKaNs}Propose a trade pact between both realms. A trade access pact establishes the exemptions of caravan tariffs between both realms, meaning that their caravans will not pay entry fees in your realm's fiefs, nor will your realm's caravans pay in theirs. The absence of fees stimulates caravans to circulate in these fiefs, strengthening mercantilism, prosperity and supply of different goods between both sides, while also diverging trade from other realms. A trade pact does not necessarily bring any revenue to lords. In fact, it may incur in some revenue loss due to the caravan fee exemptions.\n\n{POSSIBLE}")
                        .SetTextVariable("POSSIBLE", tradeHint)
                        .ToString()));

                    MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                        new TextObject("{=7OCs6wMk}Diplomatic Action").ToString(),
                        new TextObject("{=aAOEkjCF}A diplomatic action significantly changes the relationship between your realm and the target realm.").ToString(),
                        list,
                        true,
                        1,
                        1,
                        GameTexts.FindText("str_accept").ToString(),
                        GameTexts.FindText("str_selection_widget_cancel").ToString(),
                        (List<InquiryElement> list) =>
                        {
                            Action<KingdomDiplomacy, Kingdom, KingdomDiplomacyVM> action = (Action<KingdomDiplomacy, Kingdom, KingdomDiplomacyVM>)
                            list[0].Identifier;
                            action.Invoke(diplomacy, enemyKingdom, __instance);
                        },
                        null));
                }

                return false;
            }

            private static void ShowAlliance(KingdomDiplomacy diplomacy, Kingdom newAlly, KingdomDiplomacyVM __instance)
            {
                int denars = MBRandom.RoundRandomized(BannerKingsConfig.Instance.DiplomacyModel.GetAllianceDenarCost(diplomacy.Kingdom,
                    newAlly)
                    .ResultNumber);

                InformationManager.ShowInquiry(new InquiryData(new TextObject("{=cG3R7J1D}Propose Alliance").ToString(),
                    new TextObject("{=kqy4fUCu}{LEADER} is interested in accepting an alliance between your rulerships. Such alliances will only last while both rulers stay in power. For long-lasting alliances, seek instead a marriage between both families. Blood ties allow alliances to persevere for generations. In order to formalize it, they request {DENARS}{GOLD_ICON}.")
                    .SetTextVariable("DENARS", denars)
                    .SetTextVariable("LEADER", newAlly.RulingClan.Leader.Name)
                    .ToString(),
                    Hero.MainHero.Gold >= denars,
                    true,
                    GameTexts.FindText("str_policy_propose").ToString(),
                    GameTexts.FindText("str_selection_widget_cancel").ToString(),
                    () =>
                    {
                        TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKDiplomacyBehavior>().MakeAlliance(diplomacy.Kingdom, newAlly);
                        __instance.RefreshValues();
                    },
                    null));
            }

            private static void ShowTruce(KingdomDiplomacy diplomacy, Kingdom enemyKingdom, KingdomDiplomacyVM __instance)
            {
                int denars = MBRandom.RoundRandomized(BannerKingsConfig.Instance.DiplomacyModel.GetTruceDenarCost(diplomacy.Kingdom,
                    enemyKingdom)
                    .ResultNumber);

                InformationManager.ShowInquiry(new InquiryData(new TextObject("{=oQ9z60ex}Propose Truce").ToString(),
                    new TextObject("{=5J6SrvjG}{LEADER} is interested in accepting a truce proposal of 3 years. In order to formalize it, they request {DENARS}{GOLD_ICON}.")
                    .SetTextVariable("DENARS", denars)
                    .SetTextVariable("LEADER", enemyKingdom.RulingClan.Leader.Name)
                    .ToString(),
                    Hero.MainHero.Gold >= denars,
                    true,
                    GameTexts.FindText("str_policy_propose").ToString(),
                    GameTexts.FindText("str_selection_widget_cancel").ToString(),
                    () =>
                    {
                        TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKDiplomacyBehavior>().MakeTruce(diplomacy.Kingdom, enemyKingdom, 3f);
                        __instance.RefreshValues();
                    },
                    null));
            }

            private static void ShowTradePact(KingdomDiplomacy diplomacy, Kingdom enemyKingdom, KingdomDiplomacyVM __instance)
            {
                int influence = MBRandom.RoundRandomized(BannerKingsConfig.Instance.DiplomacyModel.GetTradePactInfluenceCost(diplomacy.Kingdom,
                    enemyKingdom)
                    .ResultNumber);

                InformationManager.ShowInquiry(new InquiryData(new TextObject("{=BSDCg6uz}Propose Trade Access").ToString(),
                    new TextObject("{=W8HQ7SFG}{LEADER} is interested in accepting a trade pact that provides bilateral access indefinitely. Trading caravans will be allowed access to fiefs without paying tariffs, diverging trade from enemies or competitors while strengthening trade between both realms, likely increasing consumption satisfactions and consequently, overall prosperity. Pressing this proposal would cost {INFLUENCE} influence due to all the Peers within your realm that may be affected due to tariffs loss.\n Sustaining trade access pacts will each also reduce your family's influence cap. Trade pacts faciliate making truces and take effect for an indefinite amount of time so long peace between both sides is upheld.")
                    .SetTextVariable("INFLUENCE", influence)
                    .SetTextVariable("LEADER", enemyKingdom.RulingClan.Leader.Name)
                    .ToString(),
                    Clan.PlayerClan.Influence >= influence,
                    true,
                    GameTexts.FindText("str_policy_propose").ToString(),
                    GameTexts.FindText("str_selection_widget_cancel").ToString(),
                    () =>
                    {
                        TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKDiplomacyBehavior>().MakeTradePact(diplomacy.Kingdom, enemyKingdom);
                        __instance.RefreshValues();
                    },
                    null));
            }

            private static void ShowWarOptions(KingdomDiplomacy diplomacy, Kingdom enemyKingdom, KingdomDiplomacyVM __instance)
            {
                var list = new List<InquiryElement>();
                bool enabled = Clan.PlayerClan.Influence >= BannerKingsConfig.Instance.DiplomacyModel.GetInfluenceCostOfProposingWar(Clan.PlayerClan);
                foreach (var casusBelli in diplomacy.GetAvailableCasusBelli(enemyKingdom))
                {
                    float support = new KingdomElection(new BKDeclareWarDecision(casusBelli,
                        Clan.PlayerClan,
                        enemyKingdom)).GetLikelihoodForOutcome(0);

                    list.Add(new InquiryElement(casusBelli,
                    new TextObject("{=7eaVjOKn}{NAME} ({CHANCE}% approval)")
                    .SetTextVariable("NAME", casusBelli.QueryNameText)
                    .SetTextVariable("CHANCE", (support * 100).ToString("0.00")).ToString(),
                    null,
                    enabled,
                    casusBelli.GetDescriptionWithModifers().ToString()));
                }

                list.Add(new InquiryElement(null, new TextObject("{=mFVyMjXz}No Casus Belli").ToString(), null, enabled, null));
                MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                    new TextObject("{=Fs2NR9Os}Casus Belli").ToString(),
                    new TextObject("{=fLc7tO0n}Select a justification for war.").ToString(),
                    list,
                    true,
                    1,
                    1,
                    GameTexts.FindText("str_accept").ToString(),
                    GameTexts.FindText("str_selection_widget_cancel").ToString(),
                    (List<InquiryElement> list) =>
                    {
                        object identifier = list[0].Identifier;
                        if (identifier != null)
                        {
                            CasusBelli casusBelli = (CasusBelli)identifier;
                            var decision = new BKDeclareWarDecision(casusBelli, Clan.PlayerClan, enemyKingdom);
                            Clan.PlayerClan.Kingdom.AddDecision(decision, false);
                        }
                        else
                        {
                            DeclareWarDecision declareWarDecision = new DeclareWarDecision(Clan.PlayerClan, enemyKingdom);
                            Clan.PlayerClan.Kingdom.AddDecision(declareWarDecision, false);
                        }
                        __instance.RefreshValues();
                    },
                    null));
            }
        }
    }
}
