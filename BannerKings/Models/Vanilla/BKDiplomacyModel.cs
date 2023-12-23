using BannerKings.Behaviours.Diplomacy;
using BannerKings.Behaviours.Diplomacy.Wars;
using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Institutions.Religions.Faiths;
using BannerKings.Managers.Traits;
using BannerKings.Utils.Models;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Models.Vanilla
{
    public class BKDiplomacyModel : DefaultDiplomacyModel
    {
        public float TRADE_PACT_INFLUENCE_CAP { get;} = 100f;

        public override int GetInfluenceCostOfProposingWar(Clan proposingClan)
        {
            int result = base.GetInfluenceCostOfProposingWar (proposingClan);
            return result;
        }

        public ExplainedNumber WillJoinWar(IFaction attacker, IFaction defender, IFaction ally,
            DeclareWarAction.DeclareWarDetail detail, bool explanations = false)
        {
            ExplainedNumber result = new ExplainedNumber(0, explanations);
            Clan allyClan = ally.IsClan ? (ally as Clan) : (ally as Kingdom).RulingClan;
            Clan defenderClan = defender.IsClan ? (defender as Clan) : (defender as Kingdom).RulingClan;

            float defenderRelation = allyClan.Leader.GetRelation(defenderClan.Leader);
            result.Add(defenderRelation * 0.2f, new TextObject("{=!}Relation"));

            float honor = allyClan.Leader.GetTraitLevel(DefaultTraits.Honor);
            result.Add(honor * 0.1f, new TextObject("{=!}{HERO}'s honor")
                .SetTextVariable("HERO", allyClan.Name));

            KingdomElection warSupport = new KingdomElection(new BKDeclareWarDecision(null, allyClan, attacker));
            result.Add(warSupport.GetLikelihoodForOutcome(0), new TextObject("{=!}War support in {ALLY}")
                .SetTextVariable("ALLY", ally.Name));

            /*War war = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKDiplomacyBehavior>().GetWar(attacker, defender);
            if (war != null)
            {
                if (war.CasusBelli == null)
                {
                    result.Add(-0.25f, new TextObject("{=!}Unjustified war"));
                }
            }
            else
            {
                result.Add(-0.25f, new TextObject("{=!}Unjustified war"));
            }

            if (detail == DeclareWarAction.DeclareWarDetail.CausedByPlayerHostility)
            {
                result.Add(-0.15f, new TextObject("{=!}War started by illegal aggression"));
            }*/

            return result;
        }

        public ExplainedNumber GetPactInfluenceCost(Kingdom proposer, Kingdom proposed, bool explanations = false)
        {
            ExplainedNumber result = new ExplainedNumber(0, explanations);
            float peace = GetScoreOfDeclaringPeace(proposed, proposer, proposed, out TextObject reason) / 2f;

            foreach (var clan in proposer.Clans)
            {
                if (clan == proposer.RulingClan || clan.IsUnderMercenaryService)
                {
                    continue;
                }

                float relation = clan.Leader.GetRelation(proposer.RulingClan.Leader) / 150f;
                //result.Add((100000f - peace) * MathF.Sqrt(years), clan.Name);
            }
           
            result.AddFactor(-peace / 100000f, new TextObject("{=!}"));
            return result;
        }

        public bool IsTruceAcceptable(Kingdom proposer, Kingdom proposed, bool explanations = false)
        {
            if (proposed == proposer) return false;
            
            float peace = GetScoreOfDeclaringPeace(proposed, proposer, proposed, out TextObject reason);
            return peace > 0;
        }

        public bool IsTradeAcceptable(Kingdom proposer, Kingdom proposed, bool explanations = false)
        {
            if (proposed == proposer) return false;

            float peace = GetScoreOfDeclaringPeace(proposed, proposer, proposed, out TextObject reason);
            float influence = BannerKingsConfig.Instance.InfluenceModel.CalculateInfluenceCap(proposed.RulingClan)
                .ResultNumber;
            return peace > 0 && influence > TRADE_PACT_INFLUENCE_CAP;
        }

        public ExplainedNumber GetTruceDenarCost(Kingdom proposer, Kingdom proposed, float years = 3f, bool explanations = false)
        {
            ExplainedNumber result = new ExplainedNumber(0, explanations);
            float peace = GetScoreOfDeclaringPeace(proposed, proposer, proposed, out TextObject reason) / 2f;
            result.Add((100000f - peace) * MathF.Sqrt(years), new TextObject("{=!}Truce duration"));

            float relation = proposed.RulingClan.Leader.GetRelation(proposer.RulingClan.Leader) / 150f;
            result.AddFactor(-relation, new TextObject("{=BlidMNGT}Relation"));

            return result;
        }

        public ExplainedNumber GetAllianceDesire(Kingdom proposer, Kingdom proposed, bool explanations = false)
        {
            ExplainedNumber result = new ExplainedNumber(0, explanations);
            result.Add(-100f, new TextObject("{=!}Reluctance"));

            KingdomElection election = new KingdomElection(new BKDeclareWarDecision(null, proposed.RulingClan, proposer));
            result.Add(election.GetLikelihoodForOutcome(1) * 85f, new TextObject("{=!}Peace support in {ALLY}")
                .SetTextVariable("ALLY", proposed.Name));

            float relation = proposed.RulingClan.Leader.GetRelation(proposer.RulingClan.Leader);
            result.Add(relation, new TextObject("{=BlidMNGT}Relation"));

            /* War possibleWar = new War(proposer, proposed, null, null);
            if (possibleWar.DefenderFront != null && possibleWar.AttackerFront != null)
            {
                float distance = TaleWorlds.CampaignSystem.Campaign.Current.Models.MapDistanceModel.GetDistance(possibleWar.DefenderFront.Settlement,
                                            possibleWar.AttackerFront.Settlement) * 2f;
                float factor = (TaleWorlds.CampaignSystem.Campaign.AverageDistanceBetweenTwoFortifications / distance) - 0.5f;
                result.AddFactor(factor, new TextObject("{=!}Distance between realms"));
            }*/
                
            if (proposed.RulingClan.Leader.Culture == proposer.RulingClan.Leader.Culture)
            {
                result.Add(10f, new TextObject("{=!}Shared culture"));
            }

            result.Add(proposer.RulingClan.Leader.GetTraitLevel(DefaultTraits.Honor) * 15f,
                new TextObject("{=!}Honor of {HERO}").SetTextVariable("HERO", proposer.RulingClan.Leader.Name));

            Religion proposerReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(proposer.RulingClan.Leader);
            if (proposerReligion != null)
            {
                Religion proposedReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(proposed.RulingClan.Leader);
                if (proposedReligion != null)
                {
                    if (proposerReligion == proposedReligion)
                    {
                        result.Add(25f, new TextObject("{=!}Shared faith"));
                    }
                    else
                    {
                        FaithStance faithStance = proposedReligion.GetStance(proposerReligion.Faith);
                        if (faithStance != FaithStance.Tolerated)
                        {
                            result.Add(faithStance == FaithStance.Untolerated ? -20f : -50f, new TextObject("{=!}Faith differences"));
                        }
                    }
                }
            }

            return result;
        }

        public bool WillAcceptAlliance(Kingdom proposer, Kingdom proposed) => GetAllianceDesire(proposer, proposed).ResultNumber > 0f;

        public ExplainedNumber GetAllianceDenarCost(Kingdom proposer, Kingdom proposed, bool explanations = false)
        {
            ExplainedNumber result = new ExplainedNumber(0, explanations);
            float peace = GetScoreOfDeclaringPeace(proposed, proposer, proposed, out TextObject reason) / 2f;
            result.Add((100000f), new TextObject("{=!}Truce duration"));

            float income = 0f;
            foreach (Clan clan in proposer.Clans)
            {
                income += BannerKingsConfig.Instance.ClanFinanceModel.CalculateClanIncome(clan).ResultNumber;
            }

            result.Add(income, new TextObject("{=!}Truce duration"));

            result.Add(proposed.TotalStrength * 20f, new TextObject("{=!}Truce duration"));

            float relation = proposed.RulingClan.Leader.GetRelation(proposer.RulingClan.Leader) / 150f;
            result.AddFactor(-relation, new TextObject("{=BlidMNGT}Relation"));

            return result;
        }

        public ExplainedNumber GetTradePactInfluenceCost(Kingdom proposer, Kingdom proposed, bool explanations = false)
        {
            ExplainedNumber result = new ExplainedNumber(100, explanations);
            foreach (var fief in proposer.Fiefs)
            {
                if (fief.IsTown && fief.OwnerClan != proposer.RulingClan)
                {
                    result.Add(BannerKingsConfig.Instance.InfluenceModel.CalculateSettlementInfluence(fief.Settlement,
                        BannerKingsConfig.Instance.PopulationManager.GetPopData(fief.Settlement)).ResultNumber,
                        fief.Name);
                }
            }

            float peace = GetScoreOfDeclaringPeace(proposed, proposer, proposed, out TextObject reason);
            result.AddFactor(peace / -75000f, new TextObject("{=!}Peace interest"));
            return result;
        }

        public override float GetScoreOfDeclaringWar(IFaction factionDeclaresWar, IFaction factionDeclaredWar, IFaction evaluatingClan, out TextObject warReason)
        {
            return GetScoreOfDeclaringWar(factionDeclaresWar, factionDeclaredWar, evaluatingClan, out warReason, null).ResultNumber * 10f;
        }

        public override float GetScoreOfDeclaringPeace(IFaction factionDeclaresPeace, IFaction factionDeclaredPeace, IFaction evaluatingClan, out TextObject peaceReason)
        {
            ExplainedNumber result = new ExplainedNumber(-GetScoreOfDeclaringWar(factionDeclaresPeace, 
                factionDeclaredPeace, evaluatingClan, out peaceReason, null).ResultNumber);

            War war = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKDiplomacyBehavior>().GetWar(factionDeclaresPeace,factionDeclaredPeace);
            if (war != null)
            {
                BKExplainedNumber fatigue = BannerKingsConfig.Instance.WarModel.CalculateFatigue(war, Hero.MainHero.MapFaction, true);
                result.AddFactor(fatigue.ResultNumber);
            }

            return result.ResultNumber * 10f;
        }

        public float CalculateThreatFactor(IFaction attacker, IFaction threat)
        {
            float totalThreat = 0f;
            foreach (Kingdom k in Kingdom.All)
            {
                if (k != attacker && k != threat)
                {
                    totalThreat += k.TotalStrength;
                }
            }

            return threat.TotalStrength / totalThreat;
        }

        public ExplainedNumber GetScoreOfDeclaringWar(IFaction factionDeclaresWar, IFaction factionDeclaredWar, IFaction evaluatingClan,
           out TextObject warReason, CasusBelli casusBelli = null, bool explanations = false)
        {
            warReason = TextObject.Empty;
            var result = new ExplainedNumber(0f, explanations);
            result.LimitMin(-50000f);
            result.LimitMax(50000f);

            if (factionDeclaresWar.MapFaction == factionDeclaredWar.MapFaction)
            {
                return new ExplainedNumber(-50000f);
            }

            float baseNumber = 0f;

            WarStats attackerStats = CalculateWarStats(factionDeclaresWar, factionDeclaredWar);
            float attackerScore = attackerStats.Strength + attackerStats.ValueOfSettlements - (attackerStats.TotalStrengthOfEnemies * 1.25f);

            if (factionDeclaresWar.IsKingdomFaction)
            {
                var tributes = factionDeclaresWar.Stances.ToList().FindAll(x => x.GetDailyTributePaid(x.Faction2) > 0);
                int tributeCount = tributes.Count;
                result.Add(baseNumber * -0.15f * tributeCount, new TextObject("{=!}Paying tributes (x{COUNT})")
                    .SetTextVariable("COUNT", tributeCount));
            }

            /*float totalThreat = 0f;
            foreach (Kingdom k in Kingdom.All)
            {
                if (k != factionDeclaredWar && k != factionDeclaresWar)
                {
                    totalThreat += k.TotalStrength;
                }
            }

            float threatFactor = factionDeclaredWar.TotalStrength / totalThreat;
            result.Add(10000f * threatFactor,
                new TextObject("{=!}{THREAT}% threat relative to all possible enemies")
                .SetTextVariable("THREAT", (threatFactor * 100f).ToString("0.0"))); */

            if (factionDeclaredWar.IsKingdomFaction && factionDeclaresWar.IsKingdomFaction)
            {
                var attackerKingdom = (Kingdom)factionDeclaresWar;
                var defenderKingdom = (Kingdom)factionDeclaredWar;

                TextObject reason;
                bool warAllowed = TaleWorlds.CampaignSystem.Campaign.Current.Models.KingdomDecisionPermissionModel
                    .IsWarDecisionAllowedBetweenKingdoms(attackerKingdom, defenderKingdom, out reason);
                if (!warAllowed)
                {
                    return new ExplainedNumber(-50000f, explanations, reason);
                }
   
                KingdomDiplomacy diplomacy = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKDiplomacyBehavior>().GetKingdomDiplomacy(attackerKingdom);
                if (diplomacy != null)
                {
                    if (diplomacy.HasTradePact(defenderKingdom))
                    {
                        result.Add(baseNumber * - 0.25f, new TextObject("{=!}Trade pact between both realms"));
                    }

                    if (casusBelli == null)
                    {
                        List<CasusBelli> justifications = diplomacy.GetAvailableCasusBelli(defenderKingdom);
                        foreach (CasusBelli justification in justifications)
                        {
                            float num = justification.DeclareWarScore / justifications.Count;
                            result.Add(num, new TextObject("{=!}{CASUS} justification")
                                .SetTextVariable("CASUS", justification.Name));
                            GetPersonalityCasusBelliEffect(ref result, num, evaluatingClan, justification);
                        }
                    }
                    else
                    {
                        float num = casusBelli.DeclareWarScore * 2f;
                        result.Add(num, new TextObject("{=!}{CASUS} justification")
                            .SetTextVariable("CASUS", casusBelli.Name));
                        GetPersonalityCasusBelliEffect(ref result, num, evaluatingClan, casusBelli);
                    }

                    baseNumber = result.BaseNumber;
                    result.Add(baseNumber * -diplomacy.Fatigue, new TextObject("{=!}General war fatigue of {FACTION}")
                        .SetTextVariable("FACTION", diplomacy.Kingdom.Name));
                }

                foreach (Kingdom enemyKingdom in FactionManager.GetEnemyKingdoms(attackerKingdom))
                {
                    if (enemyKingdom != attackerKingdom && enemyKingdom != defenderKingdom)
                    {
                        WarStats enemyStats = CalculateWarStats(factionDeclaresWar, enemyKingdom);
                        float enemyScore = enemyStats.Strength + enemyStats.ValueOfSettlements - (enemyStats.TotalStrengthOfEnemies * 1.25f);
                        float proportion = MathF.Clamp((attackerScore / (enemyScore * 4f)) - 1f, -1f, 0f);
                        result.Add(baseNumber * proportion, new TextObject("{=!}Existing war with {FACTION}")
                        .SetTextVariable("FACTION", enemyKingdom.Name));
                    }
                }
            }

            StanceLink stance = factionDeclaresWar.GetStanceWith(factionDeclaredWar);
            int tribute = stance.GetDailyTributePaid(factionDeclaredWar);
            if (tribute > 0)
            {
                result.Add(-10000f, new TextObject("{=!}{FACTION} is paying us tribute")
                    .SetTextVariable("FACTION", factionDeclaredWar.Name));
            }
            else if (tribute < 0)
            {
                result.Add(baseNumber * 0.3f, new TextObject("{=!}We are paying tribute to {FACTION}")
                    .SetTextVariable("FACTION", factionDeclaredWar.Name));
            }

            if (stance.BehaviorPriority == 1)
            {
                result.Add(-baseNumber, new TextObject("{=!}Defensive stance against {FACTION}")
                    .SetTextVariable("FACTION", factionDeclaredWar.Name));
            }

            if (evaluatingClan != null)
            {
                float relations = evaluatingClan.Leader.GetRelation(factionDeclaredWar.Leader);
                result.Add(baseNumber * (-relations / 100f), new TextObject("{=!}{HERO1}`s opinion of {HERO2}")
                    .SetTextVariable("HERO1", evaluatingClan.Leader.Name)
                .SetTextVariable("HERO2", factionDeclaredWar.Leader.Name));
            }
            else
            {
                float relations = factionDeclaresWar.Leader.GetRelation(factionDeclaredWar.Leader);
                result.Add(baseNumber * (-relations / 100f), new TextObject("{=!}{FACTION1} ruler`s opinion of {FACTION2} ruler")
                    .SetTextVariable("FACTION1", factionDeclaresWar.Name)
                .SetTextVariable("FACTION2", factionDeclaredWar.Name));
            }

            float threatFactor = CalculateThreatFactor(factionDeclaresWar, factionDeclaredWar);
            result.Add(baseNumber * threatFactor * 2f, new TextObject("{=!}{THREAT}% threat relative to possible enemies")
                .SetTextVariable("THREAT", (threatFactor * 100f).ToString("0.0")));


            float attackerStrength = factionDeclaresWar.TotalStrength;
            float defenderStrength = factionDeclaredWar.TotalStrength;
            float strengthFactor = (attackerStrength / defenderStrength) - 1f;
            result.Add(baseNumber * MathF.Clamp(strengthFactor * 0.3f, -2f, 2f), new TextObject("{=!}Difference in strength"));

            if (factionDeclaredWar.Fiefs.Count < factionDeclaresWar.Fiefs.Count / 2f)
            {
                float fiefFactor = factionDeclaredWar.Fiefs.Count / factionDeclaresWar.Fiefs.Count;
                result.Add(-baseNumber * (2f - fiefFactor), new TextObject("{=!}Unworthy opponent"));
            }

            if (defenderStrength >= attackerStrength * 1.5f)
            {
                result.Add(baseNumber * MathF.Clamp(strengthFactor * 0.5f, -2f, -0.4f), new TextObject("{=!}Enemy significantly stronger"));
            }

            float attackerFiefs = factionDeclaresWar.Fiefs.Count;
            float defenderFiefs = factionDeclaredWar.Fiefs.Count;
            float fiefsFactor = (attackerFiefs  / defenderFiefs) - 1f;
            result.Add(baseNumber * MathF.Clamp(fiefsFactor * 0.1f, -2f, 2f), new TextObject("{=!}Difference in controlled fiefs"));

            float attackerStability = 0f;
            foreach (var fief in factionDeclaresWar.Fiefs)
                attackerStability += BannerKingsConfig.Instance.PopulationManager.GetPopData(fief.Settlement).Stability / attackerFiefs;

            float defenderStability = 0f;
            foreach (var fief in factionDeclaredWar.Fiefs)
                defenderStability += BannerKingsConfig.Instance.PopulationManager.GetPopData(fief.Settlement).Stability / defenderFiefs;

            result.Add(baseNumber * (attackerStability - 0.5f), new TextObject("{=!}Fief stability in {FACTION}")
                        .SetTextVariable("FACTION", factionDeclaresWar.Name));

            result.Add(-baseNumber * (defenderStability - 0.5f), new TextObject("{=!}Fief stability in {FACTION}")
                        .SetTextVariable("FACTION", factionDeclaredWar.Name));

            War war = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKDiplomacyBehavior>().GetWar(factionDeclaredWar, factionDeclaresWar);
            if (war != null)
            {
                if (war.StartDate.ElapsedYearsUntilNow < 1f) result.Add(50000f, new TextObject("{=!}Recently started war"));

                float score = MathF.Clamp(war.CalculateWarScore(war.Attacker, false).ResultNumber /
                    war.TotalWarScore.ResultNumber, -1f, 1f) * 2f;
                result.Add(baseNumber * (war.Attacker == factionDeclaresWar ? -score : score));

                float fatigue = BannerKingsConfig.Instance.WarModel.CalculateFatigue(war, factionDeclaresWar).ResultNumber * 4f;
                result.Add(baseNumber * -fatigue, new TextObject("{=!}Fatigue over this war"));
            }
            else
            {
                if (stance.IsAtWar)
                {
                    result.Add(-50000f);
                }
                else
                {
                    War possibleWar = new War(factionDeclaresWar, factionDeclaredWar, null, null);
                    if (possibleWar.DefenderFront != null && possibleWar.AttackerFront != null)
                    {
                        float distance = TaleWorlds.CampaignSystem.Campaign.Current.Models.MapDistanceModel.GetDistance(possibleWar.DefenderFront.Settlement,
                            possibleWar.AttackerFront.Settlement) * 4f;
                        float factor = (TaleWorlds.CampaignSystem.Campaign.AverageDistanceBetweenTwoFortifications / distance) - 1f;
                        result.Add(baseNumber * factor, new TextObject("{=!}Distance between realms"));
                    }
                }

                //WarStats enemyStats = CalculateWarStats(factionDeclaresWar, enemyKingdom);
                //float enemyScore = enemyStats.Strength + enemyStats.ValueOfSettlements - (enemyStats.TotalStrengthOfEnemies * 1.25f);
            }

            if (evaluatingClan != null && evaluatingClan is Clan)
            {
                Clan evaluating = (Clan)evaluatingClan;
                Hero leader = evaluating.Leader;
                float traits = leader.GetTraitLevel(DefaultTraits.Valor) - leader.GetTraitLevel(DefaultTraits.Mercy) +
                    leader.GetTraitLevel(BKTraits.Instance.AptitudeViolence);
                result.Add(baseNumber * (traits / 4f));

                float enemies = 1f;
                if (evaluating.Kingdom != null) enemies += FactionManager.GetEnemyKingdoms(evaluating.Kingdom).Count();

                int gold = (int)(leader.Gold / enemies);
                if (gold < 50000)
                {
                    result.Add(result.BaseNumber * -0.8f);
                }
                else if (gold < 100000)
                {
                    result.Add(result.BaseNumber * -0.4f);
                }
            }

            /*WarStats defenderStats = CalculateWarStats(factionDeclaredWar, factionDeclaresWar);
            float defenderScore = defenderStats.Strength + defenderStats.ValueOfSettlements - (defenderStats.TotalStrengthOfEnemies * 1.25f);
            float scoreProportion = (attackerScore / defenderScore) - 1f;
            result.AddFactor(scoreProportion);*/

            return result;
        }

        private void GetPersonalityCasusBelliEffect(ref ExplainedNumber result, float baseResult, IFaction evaluation, CasusBelli casusBelli)
        {
            if (evaluation != null && evaluation.IsClan)
            {
                Hero leader = evaluation.Leader;
                foreach (TraitObject trait in BKTraits.Instance.PersonalityTraits.Concat(BKTraits.Instance.PoliticalTraits))
                {
                    float factor = casusBelli.GetTraitWeight(trait);
                    float level = leader.GetTraitLevel(trait);
                    result.Add(baseResult * factor * level, new TextObject("{=!}{HERO}'s {TRAIT} personality")
                        .SetTextVariable("HERO", leader.Name)
                        .SetTextVariable("TRAIT", trait.Name));
                }
            }
        }

        private WarStats CalculateWarStats(IFaction faction, IFaction targetFaction)
        {
            Clan rulingClan = faction.IsClan ? (faction as Clan) : (faction as Kingdom).RulingClan;
            float valueOfSettlements = faction.Fiefs.Sum((Town f) => (float)(f.IsTown ? 2000 : 1000) + f.Prosperity * 0.33f) * 50f;
            float enemyStrength = 0f;
            foreach (StanceLink stanceLink in faction.Stances)
            {
                if (stanceLink.IsAtWar && stanceLink.Faction1 != targetFaction && stanceLink.Faction2 != targetFaction && (!stanceLink.Faction2.IsMinorFaction || stanceLink.Faction2.Leader == Hero.MainHero))
                {
                    IFaction faction2 = (stanceLink.Faction1 == faction) ? stanceLink.Faction2 : stanceLink.Faction1;
                    enemyStrength += faction2.TotalStrength;
                }
            }

            return new WarStats
            {
                RulingClan = rulingClan,
                Strength = faction.TotalStrength,
                ValueOfSettlements = valueOfSettlements,
                TotalStrengthOfEnemies = enemyStrength
            };
        }

        public struct WarStats
        {
            public Clan RulingClan;
            public float Strength;
            public float ValueOfSettlements;
            public float TotalStrengthOfEnemies;
        }
        public override void GetHeroesForEffectiveRelation(Hero hero1, Hero hero2, out Hero effectiveHero1, out Hero effectiveHero2)
        {
            effectiveHero1 = hero1;
            effectiveHero2 = hero2;

            if (effectiveHero1 == effectiveHero2 || effectiveHero1 == null || effectiveHero2 == null) 
                base.GetHeroesForEffectiveRelation(hero1, hero2, out effectiveHero1, out effectiveHero2);
        }

        public override int GetRelationChangeAfterVotingInSettlementOwnerPreliminaryDecision(Hero supporter, bool hasHeroVotedAgainstOwner)
        {
            return base.GetRelationChangeAfterVotingInSettlementOwnerPreliminaryDecision(supporter, hasHeroVotedAgainstOwner);
        }
    }
}


