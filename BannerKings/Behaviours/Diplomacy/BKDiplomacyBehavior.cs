using BannerKings.Behaviours.Diplomacy.Wars;
using BannerKings.Models.Vanilla;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.Diplomacy;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Diplomacy
{
    public class BKDiplomacyBehavior : BannerKingsBehavior
    {
        private Dictionary<Kingdom, KingdomDiplomacy> kingdomDiplomacies = new Dictionary<Kingdom, KingdomDiplomacy>();
        private List<War> wars = new List<War>();

        public War GetWar(IFaction faction1, IFaction faction2)
        {
            if (wars == null)
            {
                wars = new List<War>();
                return null;
            }

            return wars.FirstOrDefault(x => (x.Attacker == faction1 || x.Defender == faction1) &&
            (x.Attacker == faction2 || x.Defender == faction2));
        }

        public CasusBelli GetWarJustification(IFaction faction1, IFaction faction2)
        {
            War war = GetWar(faction1, faction2);   
            if (war != null)
            {
                return war.CasusBelli;
            }

            return null;
        }

        public KingdomDiplomacy GetKingdomDiplomacy(Kingdom kingdom)
        {
            if (kingdom == null)
            {
                return null;
            }

            if (kingdomDiplomacies.ContainsKey(kingdom))
            {
                return kingdomDiplomacies[kingdom];
            }

            return null;
        }

        public void TriggerJustifiedWar(CasusBelli justification, Kingdom attacker, Kingdom defender)
        {
            wars.Add(new War(attacker, defender, justification));
            InformationManager.DisplayMessage(new InformationMessage(justification.WarDeclaredText.ToString()));
        }

        public void ConsiderTruce(Kingdom proposer, Kingdom proposed, float years, bool kingdomBudget = false)
        {
            if (proposed.RulingClan == Clan.PlayerClan)
            {
                int denars = MBRandom.RoundRandomized(BannerKingsConfig.Instance.DiplomacyModel.GetTruceDenarCost(proposer,
                    proposed).ResultNumber);
                InformationManager.ShowInquiry(new InquiryData(new TextObject("{=!}Truce Offering").ToString(),
                    new TextObject("{=!}The lords of {KINGDOM} offer a truce with your realm for {YEARS} years. They are willing to pay you {DENARS} denars to prove their commitment. Accepting this offer will bind your realm to not raise arms against them by any means.")
                    .SetTextVariable("DENARS", denars)
                    .SetTextVariable("KINGDOM", proposer.Name)
                    .SetTextVariable("YEARS", years).ToString(),
                    true,
                    true,
                    GameTexts.FindText("str_accept").ToString(),
                    GameTexts.FindText("str_reject").ToString(),
                    () => MakeTruce(proposer, proposed, years, kingdomBudget),
                    null),
                    true,
                    true);
            }
            else MakeTruce(proposer, proposed, years, kingdomBudget);
        }

        public void MakeTruce(Kingdom proposer, Kingdom proposed, float years, bool kingdomBudget = false)
        {
            int denars = MBRandom.RoundRandomized(BannerKingsConfig.Instance.DiplomacyModel.GetTruceDenarCost(proposer,
                    proposed).ResultNumber);
            if (!kingdomBudget) proposer.RulingClan.Leader.ChangeHeroGold(-denars);
            else proposer.KingdomBudgetWallet -= denars;

            proposed.RulingClan.Leader.ChangeHeroGold(denars);

            var diplomacy1 = GetKingdomDiplomacy(proposer);
            diplomacy1.AddTruce(proposed, years);

            var diplomacy2 = GetKingdomDiplomacy(proposed);
            diplomacy2.AddTruce(proposer, years);

            InformationManager.DisplayMessage(new InformationMessage(
                new TextObject("{=!}The lords of {KINGDOM1} and {KINGDOM2} have settled on a truce until {DATE}.")
                .SetTextVariable("KINGDOM1", proposer.Name)
                .SetTextVariable("KINGDOM2", proposed.Name)
                .SetTextVariable("DATE", CampaignTime.YearsFromNow(years).ToString())
                .ToString(),
                Color.FromUint(proposer == Clan.PlayerClan.MapFaction || proposed == Clan.PlayerClan.MapFaction ? 
                Utils.TextHelper.COLOR_LIGHT_BLUE :
                Utils.TextHelper.COLOR_LIGHT_YELLOW)));
        }

        public void ConsiderTradePact(Kingdom proposer, Kingdom proposed)
        {
            if (proposed.RulingClan == Clan.PlayerClan)
            {
                InformationManager.ShowInquiry(new InquiryData(new TextObject("{=!}Trade Access Offering").ToString(),
                    new TextObject("{=!}The lords of {KINGDOM} offer a trade access pact with your realm. Trade access pacts help develop prosperity on the long term in both kingdoms and set an amicable relation that facilitates future truces and alliances. The pact will not cost or award you any resources, but sustaining the pact will reduce your clan influence cap.")
                    .SetTextVariable("KINGDOM", proposer.Name)
                    .ToString(),
                    true,
                    true,
                    GameTexts.FindText("str_accept").ToString(),
                    GameTexts.FindText("str_reject").ToString(),
                    () => MakeTradePact(proposer, proposed),
                    null),
                    true,
                    true);
            }
            else MakeTradePact(proposer, proposed);
        }


        public void MakeTradePact(Kingdom proposer, Kingdom proposed)
        {
            int influence = MBRandom.RoundRandomized(BannerKingsConfig.Instance.DiplomacyModel.GetPactInfluenceCost(proposer,
                    proposed).ResultNumber);
            ChangeClanInfluenceAction.Apply(proposed.RulingClan, -influence);

            var diplomacy1 = GetKingdomDiplomacy(proposer);
            diplomacy1.AddPact(proposed);

            var diplomacy2 = GetKingdomDiplomacy(proposed);
            diplomacy2.AddPact(proposer);

            InformationManager.DisplayMessage(new InformationMessage(
                new TextObject("{=!}The lords of {KINGDOM1} and {KINGDOM2} have settled on trade access pact.")
                .SetTextVariable("KINGDOM1", proposer.Name)
                .SetTextVariable("KINGDOM2", proposed.Name)
                .ToString(),
                Color.FromUint(proposer == Clan.PlayerClan.MapFaction || proposed == Clan.PlayerClan.MapFaction ? 
                Utils.TextHelper.COLOR_LIGHT_BLUE :
                Utils.TextHelper.COLOR_LIGHT_YELLOW)));
        }

        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
            CampaignEvents.WarDeclared.AddNonSerializedListener(this, OnWarDeclared);
            CampaignEvents.DailyTickClanEvent.AddNonSerializedListener(this, OnDailyTickClan);
            CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, OnNewGameCreated);
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnNewGameCreated);
            CampaignEvents.KingdomCreatedEvent.AddNonSerializedListener(this, OnKingdomCreated);
            CampaignEvents.AiHourlyTickEvent.AddNonSerializedListener(this, OnAiHourlyTick);
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
            CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener(this, OnOwnerChanged);
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);
            CampaignEvents.RulingClanChanged.AddNonSerializedListener(this, OnRulerChanged);
            CampaignEvents.MakePeace.AddNonSerializedListener(this, OnMakePeace);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("bannerkings-kingdom-diplomacies", ref kingdomDiplomacies);
            dataStore.SyncData("bannerkings-kingdom-wars", ref wars);

            if (kingdomDiplomacies == null)
            {
                kingdomDiplomacies = new Dictionary<Kingdom, KingdomDiplomacy>();
            }

            if (wars == null)
            {
                wars = new List<War>();
            }
        }

        private void OnGameLoaded(CampaignGameStarter starter)
        {
            foreach (var diplomacy in kingdomDiplomacies.Values)
            {
                diplomacy.PostInitialize();
            }

            foreach (var war in wars)
            {
                war.PostInitialize();
            }
        }

        private void OnDailyTick()
        {
            InitializeDiplomacies();
            foreach (War war in wars)
            {
                war.Update();
            }

            foreach (var pair in kingdomDiplomacies)
            {
                pair.Value.Update();
            }

            RunWeekly(() =>
            {
                foreach (var kingdom in Kingdom.All)
                {
                    if (kingdom.RulingClan == Clan.PlayerClan) continue;

                    foreach (var target in Kingdom.All)
                    {
                        TextObject pactReason;
                        if (BannerKingsConfig.Instance.KingdomDecisionModel.IsTradePactAllowed(kingdom, target, out pactReason) &&
                            MBRandom.RandomFloat < MBRandom.RandomFloat)
                        {
                            if (kingdom.RulingClan.Influence >= 
                            BannerKingsConfig.Instance.DiplomacyModel.GetTradePactInfluenceCost(kingdom, target)
                                .ResultNumber * 2f)
                            {
                                ConsiderTradePact(kingdom, target);
                                break;
                            }
                        }
                        else
                        {
                            TextObject truceReason;
                            if (BannerKingsConfig.Instance.KingdomDecisionModel.IsTruceAllowed(kingdom, target, out truceReason) &&
                                MBRandom.RandomFloat < MBRandom.RandomFloat)
                            {
                                if (kingdom.RulingClan.Gold >= BannerKingsConfig.Instance.DiplomacyModel.GetTruceDenarCost(kingdom, target)
                                    .ResultNumber * 3f)
                                {
                                    ConsiderTruce(kingdom, target, 3f);
                                    break;
                                }
                            }
                        }
                    }
                }
            },
            GetType().Name);
        }

        private void OnNewGameCreated(CampaignGameStarter starter)
        {
            InitializeDiplomacies();
        }

        private void InitializeDiplomacies()
        {
            if (kingdomDiplomacies == null)
            {
                kingdomDiplomacies = new Dictionary<Kingdom, KingdomDiplomacy>();
            }

            foreach (var kingdom in Kingdom.All)
            {
                if (!kingdomDiplomacies.ContainsKey(kingdom))
                {
                    kingdomDiplomacies.Add(kingdom, new KingdomDiplomacy(kingdom));
                }
            }
        }

        private void OnRulerChanged(Kingdom kingdom, Clan clan)
        {
            if (kingdomDiplomacies.ContainsKey(kingdom))
            {
                var group = kingdomDiplomacies[kingdom].GetHeroGroup(clan.Leader);
                if (group != null)
                {
                    group.RemoveMember(clan.Leader);
                }
            }
        }

        private void OnOwnerChanged(Settlement settlement, bool openToClaim, Hero newOwner, Hero oldOwner,
           Hero capturerHero,
           ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail detail)
        {
            if (newOwner != null && oldOwner != null)
            {
                IFaction attacker = newOwner.MapFaction;
                IFaction defender = oldOwner.MapFaction;
                if (attacker != defender)
                {
                    War war = GetWar(attacker, defender);
                    if (war != null)
                    {
                        war.RecalculateFronts();
                    }
                }
            }
        }

        private void OnKingdomCreated(Kingdom kingdom)
        {
            kingdomDiplomacies.Add(kingdom, new KingdomDiplomacy(kingdom));
        }

        private void OnAiHourlyTick(MobileParty party, PartyThinkParams p)
        {

        }

        private void OnDailyTickClan(Clan clan)
        {
            if (clan.Kingdom == null || clan == Clan.PlayerClan || clan != clan.Kingdom.RulingClan)
            {
                return;
            }

            RunWeekly(() =>
            {
                KingdomDiplomacy diplomacy = GetKingdomDiplomacy(clan.Kingdom);
                if (diplomacy == null)
                {
                    return;
                }

                DiplomacyModel diplomacyModel = Campaign.Current.Models.DiplomacyModel;
                if (clan.Influence < (float)diplomacyModel.GetInfluenceCostOfProposingWar(clan.Kingdom))
                {
                    return;
                }

                List<CasusBelli> casi = diplomacy.GetAvailableCasusBelli();
                if (casi.Count == 0)
                {
                    return;
                }

                foreach (CasusBelli casus in casi)
                {
                    if (clan.Kingdom.UnresolvedDecisions.Any(x => x.GetType() is DeclareWarDecision || x.GetType() is BKDeclareWarDecision))
                    {
                        break;
                    }

                    BKDeclareWarDecision declareWarDecision = new BKDeclareWarDecision(casus, clan, casus.Defender);
                    float support = new KingdomElection(declareWarDecision).GetLikelihoodForOutcome(0);
                    if (support > 0.4f)
                    {
                        clan.Kingdom.AddDecision(declareWarDecision);
                        break;
                    }
                }
            },
            GetType().Name,
            false);
        }
      
        private void OnMakePeace(IFaction faction1, IFaction faction2, MakePeaceAction.MakePeaceDetail detail)
        {
            if (faction1.IsKingdomFaction && faction2.IsKingdomFaction)
            {
                MakeTruce(faction1 as Kingdom, faction2 as Kingdom, 1f);
            }
        }

        private void OnWarDeclared(IFaction faction1, IFaction faction2, DeclareWarAction.DeclareWarDetail detail)
        {
            if (faction1.IsKingdomFaction && faction2.IsKingdomFaction)
            {
               
            }
        }
    }

    namespace Patches
    {
        [HarmonyPatch(typeof(KingdomDecisionProposalBehavior), "ConsiderWar")]
        internal class ConsiderWarPatch
        {
            private static bool Prefix(Clan clan, Kingdom kingdom, IFaction otherFaction, ref bool __result)
            {
                __result = false;
                return false;
            }
        }

        [HarmonyPatch(typeof(KingdomDiplomacyVM), "OnDeclareWar")]
        internal class DeclareWarVMPatch
        {
            private static bool Prefix(KingdomDiplomacyVM __instance, KingdomTruceItemVM item)
            {
                IFaction enemy = item.Faction2;
                if (!enemy.IsKingdomFaction)
                {
                    return true;
                }

                Kingdom enemyKingdom = enemy as Kingdom;
                Kingdom kingdom = item.Faction1 as Kingdom;
                KingdomDiplomacy diplomacy = Campaign.Current.GetCampaignBehavior<BKDiplomacyBehavior>().GetKingdomDiplomacy(kingdom);
                if (diplomacy == null)
                {
                    return true;
                }

                if (kingdom.UnresolvedDecisions.Any(x => x is DeclareWarDecision || x is BKDeclareWarDecision))
                {
                    InformationManager.DisplayMessage(new InformationMessage(
                        new TextObject("{=!}A war declaration is being voted upon concerning the {FACTION}.")
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
                        new TextObject("{=!}Declare War").ToString(),
                        null,
                        warPossible,
                        warHint.ToString()));

                    bool playerRuler = Hero.MainHero == Clan.PlayerClan.Kingdom.RulingClan.Leader;
                    Action<KingdomDiplomacy, Kingdom, KingdomDiplomacyVM> makeTruce = ShowTruce;
                    TextObject truceHint;
                    bool trucePossible = model.IsTruceAllowed(kingdom, enemyKingdom, out truceHint);
                    list.Add(new InquiryElement(makeTruce,
                        new TextObject("{=!}Propose Truce").ToString(),
                        null,
                        trucePossible && playerRuler,
                        new TextObject("{=!}Propose a truce between both realms. A truce is a period of a certain amount of years in which both realms formally agree to not declare wars upon each other, in mutual benefit. The proposing realm is assumed to be the major beneficiary of this agreement, and thus is required a fee. The proposed realm is more likely to accept and offer better terms relative to how advantageous a truce is for them.\n\n{POSSIBLE}")
                        .SetTextVariable("POSSIBLE", truceHint)
                        .ToString()));

                    Action<KingdomDiplomacy, Kingdom, KingdomDiplomacyVM> makePact = ShowTradePact;
                    TextObject tradeHint;
                    bool tradePossible = model.IsTradePactAllowed(kingdom, enemyKingdom, out tradeHint);
                    list.Add(new InquiryElement(makePact,
                        new TextObject("{=!}Propose Trade Pact").ToString(),
                        null,
                        tradePossible && playerRuler,
                        new TextObject("{=!}Propose a trade pact between both realms. A trade access pact establishes the exemptions of caravan tariffs between both realms, meaning that their caravans will not pay entry fees in your realm's fiefs, nor will your realm's caravans pay in theirs. The absence of fees stimulates caravans to circulate in these fiefs, strengthening mercantilism, prosperity and supply of different goods between both sides, while also diverging trade from other realms. A trade pact does not necessarily bring any revenue to lords. In fact, it may incur in some revenue loss due to the caravan fee exemptions.\n\n{POSSIBLE}")
                        .SetTextVariable("POSSIBLE", tradeHint)
                        .ToString()));

                    MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                        new TextObject("{=!}Diplomatic Action").ToString(),
                        new TextObject("{=!}A diplomatic action significantly changes the relationship between your realm and the target realm.").ToString(),
                        list,
                        true,
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

            private static void ShowTruce(KingdomDiplomacy diplomacy, Kingdom enemyKingdom, KingdomDiplomacyVM __instance)
            {
                int denars = MBRandom.RoundRandomized(BannerKingsConfig.Instance.DiplomacyModel.GetTruceDenarCost(diplomacy.Kingdom,
                    enemyKingdom)
                    .ResultNumber);

                InformationManager.ShowInquiry(new InquiryData(new TextObject("{=!}Propose Truce").ToString(),
                    new TextObject("{=!}{LEADER} is interested in accepting a truce proposal of 3 years. In order to formalize it, they request {DENARS}{GOLD_ICON}.")
                    .SetTextVariable("DENARS", denars)
                    .SetTextVariable("LEADER", enemyKingdom.RulingClan.Leader.Name)
                    .ToString(),
                    Hero.MainHero.Gold >= denars,
                    true,
                    GameTexts.FindText("str_policy_propose").ToString(),
                    GameTexts.FindText("str_selection_widget_cancel").ToString(),
                    () =>
                    {
                        Campaign.Current.GetCampaignBehavior<BKDiplomacyBehavior>().MakeTruce(diplomacy.Kingdom, enemyKingdom, 3f);
                        __instance.RefreshValues();
                    },
                    null));
            }

            private static void ShowTradePact(KingdomDiplomacy diplomacy, Kingdom enemyKingdom, KingdomDiplomacyVM __instance)
            {
                int influence = MBRandom.RoundRandomized(BannerKingsConfig.Instance.DiplomacyModel.GetTradePactInfluenceCost(diplomacy.Kingdom,
                    enemyKingdom)
                    .ResultNumber);

                InformationManager.ShowInquiry(new InquiryData(new TextObject("{=!}Propose Trade Access").ToString(),
                    new TextObject("{=!}{LEADER} is interested in accepting a trade pact that provides bilateral access indefinitely. Trading caravans will be allowed access to fiefs without paying tariffs, diverging trade from enemies or competitors while strengthening trade between both realms, likely increasing consumption satisfactions and consequently, overall prosperity. Pressing this proposal would cost {INFLUENCE} influence due to all the Peers within your realm that may be affected due to tariffs loss.\n Sustaining trade access pacts will each also reduce your family's influence cap. Trade pacts faciliate making truces and take effect for an indefinite amount of time so long peace between both sides is upheld.")
                    .SetTextVariable("INFLUENCE", influence)
                    .SetTextVariable("LEADER", enemyKingdom.RulingClan.Leader.Name)
                    .ToString(),
                    Clan.PlayerClan.Influence >= influence,
                    true,
                    GameTexts.FindText("str_policy_propose").ToString(),
                    GameTexts.FindText("str_selection_widget_cancel").ToString(),
                    () =>
                    {
                        Campaign.Current.GetCampaignBehavior<BKDiplomacyBehavior>().MakeTradePact(diplomacy.Kingdom, enemyKingdom);
                        __instance.RefreshValues();
                    },
                    null));
            }

            private static void ShowWarOptions(KingdomDiplomacy diplomacy, Kingdom enemyKingdom, KingdomDiplomacyVM __instance)
            {
                var list = new List<InquiryElement>();
                foreach (var casusBelli in diplomacy.GetAvailableCasusBelli(enemyKingdom))
                {
                    float support = new KingdomElection(new BKDeclareWarDecision(casusBelli, 
                        Clan.PlayerClan,
                        enemyKingdom)).GetLikelihoodForOutcome(0);

                    list.Add(new InquiryElement(casusBelli,
                    new TextObject("{=!}{NAME} ({CHANCE}% approval)")
                    .SetTextVariable("NAME", casusBelli.QueryNameText)
                    .SetTextVariable("CHANCE", (support * 100).ToString("0.00")).ToString(),
                    null,
                    true,
                    casusBelli.GetDescriptionWithModifers().ToString()));
                }

                list.Add(new InquiryElement(null, new TextObject("{=!}No Casus Belli").ToString(), null));
                MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                    new TextObject("{=!}Casus Belli").ToString(),
                    new TextObject("{=!}Select a justification for war.").ToString(),
                    list,
                    true,
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
