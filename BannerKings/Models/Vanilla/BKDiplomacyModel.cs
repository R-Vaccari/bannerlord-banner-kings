using BannerKings.Behaviours.Diplomacy;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace BannerKings.Models.Vanilla
{
    public class BKDiplomacyModel : DefaultDiplomacyModel
    {
        public override float GetScoreOfDeclaringWar(IFaction factionDeclaresWar, IFaction factionDeclaredWar, IFaction evaluatingClan, out TextObject warReason)
        {
            return GetScoreOfDeclaringWar(factionDeclaresWar, factionDeclaredWar, evaluatingClan, false, out warReason).ResultNumber;
        }

        public ExplainedNumber GetScoreOfDeclaringWar(IFaction factionDeclaresWar, IFaction factionDeclaredWar, IFaction evaluatingClan,
            bool evaluatingPeace, out TextObject warReason, bool explanations = false)
        {
            warReason = TextObject.Empty;
            var result = new ExplainedNumber(0f, explanations);
            result.LimitMin(-50000f);
            result.LimitMax(50000f);

            if (factionDeclaresWar.MapFaction == factionDeclaredWar.MapFaction)
            {
                return new ExplainedNumber(-50000f);
            }

            StanceLink stance = factionDeclaresWar.GetStanceWith(factionDeclaredWar);
            if (stance.IsAllied)
            {
                return new ExplainedNumber(-50000f);
            }

            if (stance.GetDailyTributePaid(factionDeclaredWar) < 0)
            {
                return new ExplainedNumber(-50000f);
            }

            if (factionDeclaredWar.IsKingdomFaction && factionDeclaresWar.IsKingdomFaction)
            {
                StanceLink clanStance = (factionDeclaresWar as Kingdom).RulingClan.GetStanceWith((factionDeclaredWar as Kingdom).RulingClan);
                if (clanStance.IsAllied)
                {
                    return new ExplainedNumber(-50000f);
                }

                var tributes = factionDeclaresWar.Stances.ToList().FindAll(x => x.GetDailyTributePaid(x.Faction2) > 0);
                result.AddFactor(-0.15f * tributes.Count);

                var attackerKingdom = (Kingdom)factionDeclaresWar;
                var defenderKingdom = (Kingdom)factionDeclaredWar;
                KingdomDiplomacy diplomacy = Campaign.Current.GetCampaignBehavior<BKDiplomacyBehavior>().GetKingdomDiplomacy(attackerKingdom);
                if (diplomacy != null)
                {
                    if (diplomacy.HasTradePact(defenderKingdom))
                    {
                        result.AddFactor(-0.25f);
                    }
                }
            }     

            WarStats attackerStats = CalculateWarStats(factionDeclaresWar, factionDeclaredWar);
            WarStats defenderStats = CalculateWarStats(factionDeclaredWar, factionDeclaresWar);
            float attackerScore = attackerStats.Strength + attackerStats.ValueOfSettlements - (attackerStats.TotalStrengthOfEnemies * 1.25f);
            float defenderScore = defenderStats.Strength + defenderStats.ValueOfSettlements - (defenderStats.TotalStrengthOfEnemies * 1.25f);
            float scoreProportion = (attackerScore / defenderScore) - 1f;
            result.Add(scoreProportion * 50000f);

            float relations = attackerStats.RulingClan.GetRelationWithClan(defenderStats.RulingClan);
            result.AddFactor(relations * -0.3f);

            if (evaluatingPeace)
            {
                result.AddFactor(-1f);
            }

            return result;
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
    }
}


