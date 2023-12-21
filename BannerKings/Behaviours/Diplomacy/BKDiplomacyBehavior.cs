using BannerKings.Behaviours.Diplomacy.Wars;
using BannerKings.Extensions;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Diplomacy
{
    public class BKDiplomacyBehavior : BannerKingsBehavior
    {
        private Dictionary<Kingdom, KingdomDiplomacy> kingdomDiplomacies = new Dictionary<Kingdom, KingdomDiplomacy>();
        private List<War> wars = new List<War>();

        public bool WillJoinWar(IFaction attacker, IFaction defender, IFaction ally, DeclareWarAction.DeclareWarDetail detail)
            => BannerKingsConfig.Instance.DiplomacyModel.WillJoinWar(attacker, defender, ally, detail).ResultNumber > 0f;

        public void CallToWar(IFaction attacker, IFaction defender, IFaction ally, DeclareWarAction.DeclareWarDetail detail)
        {
            War war = GetWar(attacker, defender);
            if (war != null)
            {
                if (WillJoinWar(attacker, defender, ally, detail))
                {
                    war.AddAlly(attacker, ally);
                }
            }
        }

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

        public void ConsiderAlliance(Kingdom proposer, Kingdom proposed)
        {
            if (proposed.RulingClan == Clan.PlayerClan)
            {
                int denars = MBRandom.RoundRandomized(BannerKingsConfig.Instance.DiplomacyModel.GetAllianceDenarCost(proposer,
                    proposed).ResultNumber);
                InformationManager.ShowInquiry(new InquiryData(new TextObject("{=!}Alliance Offering").ToString(),
                    new TextObject("{=!}The lords of {KINGDOM} offer an alliance with your realm. They are willing to pay you {DENARS} denars to prove their commitment. Accepting this offer will bind your realm to not raise arms against them by any means.")
                    .SetTextVariable("DENARS", denars)
                    .SetTextVariable("KINGDOM", proposer.Name)
                    .ToString(),
                    true,
                    true,
                    GameTexts.FindText("str_accept").ToString(),
                    GameTexts.FindText("str_reject").ToString(),
                    () => MakeAlliance(proposer, proposed),
                    null),
                    true,
                    true);
            }
            else MakeAlliance(proposer, proposed);
        }

        public void MakeAlliance(Kingdom proposer, Kingdom proposed)
        {
            FactionManager.DeclareAlliance(proposer, proposed);
            int denars = MBRandom.RoundRandomized(BannerKingsConfig.Instance.DiplomacyModel.GetAllianceDenarCost(proposer,
                    proposed).ResultNumber);

            proposer.RulingClan.Leader.ChangeHeroGold(-denars);
            proposed.RulingClan.Leader.ChangeHeroGold(denars);
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
            foreach (Kingdom kingdom in Kingdom.All)
            {
                if (kingdom == Clan.PlayerClan.MapFaction) continue;

                float strength = kingdom.TotalStrength;
                int fiefs = kingdom.Fiefs.Count;
                foreach (Kingdom k in Kingdom.All)
                {
                    if (k == kingdom) continue;

                    StanceLink stance = kingdom.GetStanceWith(k);
                    if (fiefs == 1) stance.BehaviorPriority = 1;
                    else
                    {
                        if (strength >= k.TotalStrength * 1.5f) stance.BehaviorPriority = 2;
                        else stance.BehaviorPriority = 0;
                    }
                }

                float highestStrength = 0f;
                foreach (Kingdom k in FactionManager.GetEnemyKingdoms(kingdom))
                {
                    float enemyStrength = k.TotalStrength;
                    if (enemyStrength > highestStrength) highestStrength = enemyStrength;
                }

                MobileParty.PartyObjective objective = MobileParty.PartyObjective.Neutral;
                if (fiefs == 1 || highestStrength >= strength * 1.5f) objective = MobileParty.PartyObjective.Defensive;

                if (strength >= highestStrength * 1.5f) objective = MobileParty.PartyObjective.Aggressive;

                foreach (WarPartyComponent party in kingdom.WarPartyComponents)
                {
                    party.MobileParty.SetPartyObjective(objective);
                }
            }
            
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
                Kingdom kingdom = Kingdom.All.GetRandomElementWithPredicate(x => x.RulingClan != Clan.PlayerClan);

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
                        else
                        {
                            TextObject allianceReason;
                            if (BannerKingsConfig.Instance.KingdomDecisionModel.IsAllianceAllowed(kingdom, target, out allianceReason) &&
                                MBRandom.RandomFloat < MBRandom.RandomFloat)
                            {
                                if (kingdom.RulingClan.Gold >= BannerKingsConfig.Instance.DiplomacyModel.GetAllianceDenarCost(kingdom, target)
                                    .ResultNumber * 3f)
                                {
                                    if (target != Hero.MainHero.MapFaction && !BannerKingsConfig.Instance.DiplomacyModel.WillAcceptAlliance(target, kingdom)) 
                                        continue;

                                    ConsiderAlliance(kingdom, target);
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
            AvaliateAlliances(kingdom, clan);
            if (kingdomDiplomacies.ContainsKey(kingdom))
            {
                var group = kingdomDiplomacies[kingdom].GetHeroGroup(clan.Leader);
                if (group != null)
                {
                    group.RemoveMember(clan.Leader);
                }
            }
        }

        private void AvaliateAlliances(Kingdom kingdom, Clan clan)
        {
            foreach (StanceLink stance in kingdom.Stances)
            {
                IFaction other = stance.Faction1 == kingdom ? stance.Faction2 : stance.Faction1;
                if (other.IsKingdomFaction && stance.IsAllied)
                {
                    if (BannerKingsConfig.Instance.MarriageModel.DiscoverAncestors(clan.Leader, 3)
                        .Intersect(BannerKingsConfig.Instance.MarriageModel.DiscoverAncestors(other.Leader, 3)).Any()) 
                    { 
                        if (kingdom == Clan.PlayerClan.MapFaction && other == Clan.PlayerClan.MapFaction)
                        {
                            InformationManager.DisplayMessage(new InformationMessage(
                                new TextObject("{=!}Despite the new rulership of {KINGDOM}, the realm and the {OTHER} are still allies through blood ties.")
                                .SetTextVariable("KINGDOM", kingdom.Name)
                                .SetTextVariable("OTHER", other.Name)
                                .ToString(),
                                Color.FromUint(Utils.TextHelper.COLOR_LIGHT_BLUE)));
                        }
                    }
                    else
                    {
                        if (kingdom == Clan.PlayerClan.MapFaction && other == Clan.PlayerClan.MapFaction)
                        {
                            InformationManager.DisplayMessage(new InformationMessage(
                                new TextObject("{=!}Due to the absence of blood ties, the new rulership of {KINGDOM} has dissolved its previous alliance with the {OTHER}.")
                                .SetTextVariable("KINGDOM", kingdom.Name)
                                .SetTextVariable("OTHER", other.Name)
                                .ToString(),
                                Color.FromUint(Utils.TextHelper.COLOR_LIGHT_YELLOW)));
                        }
                        FactionManager.SetNeutral(kingdom, other);
                    }
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

                DiplomacyModel diplomacyModel = TaleWorlds.CampaignSystem.Campaign.Current.Models.DiplomacyModel;
                if (clan.Influence < (float)diplomacyModel.GetInfluenceCostOfProposingWar(clan))
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
                Kingdom attacker = faction1 as Kingdom;
                Kingdom defender = faction2 as Kingdom;
                KingdomDiplomacy attackerD = GetKingdomDiplomacy(attacker);
                if (attackerD != null) attackerD.OnWar(defender);

                KingdomDiplomacy defenderD = GetKingdomDiplomacy(defender);
                if (defenderD != null) defenderD.OnWar(attacker);
            }

            foreach (IFaction ally in faction2.GetAllies())
            {
                WillJoinWar(faction1, faction2, ally, detail);
            }
        }
    }
}
