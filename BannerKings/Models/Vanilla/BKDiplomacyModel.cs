using BannerKings.Behaviours.Diplomacy;
using BannerKings.Behaviours.Diplomacy.Wars;
using BannerKings.Utils.Models;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Models.Vanilla
{
    public class BKDiplomacyModel : DefaultDiplomacyModel
    {
        public override int GetInfluenceCostOfProposingWar(Kingdom proposingKingdom)
        {
            return 100;
        }
        public override float GetScoreOfDeclaringWar(IFaction factionDeclaresWar, IFaction factionDeclaredWar, IFaction evaluatingClan, out TextObject warReason)
        {
            return GetScoreOfDeclaringWar(factionDeclaresWar, factionDeclaredWar, evaluatingClan, false, out warReason).ResultNumber;
        }

        public override float GetScoreOfDeclaringPeace(IFaction factionDeclaresPeace, IFaction factionDeclaredPeace, IFaction evaluatingClan, out TextObject peaceReason)
        {
            ExplainedNumber result = new ExplainedNumber(base.GetScoreOfDeclaringPeace(factionDeclaresPeace, 
                factionDeclaredPeace, evaluatingClan, out peaceReason));

            War war = Campaign.Current.GetCampaignBehavior<BKDiplomacyBehavior>().GetWar(factionDeclaresPeace,factionDeclaredPeace);
            if (war != null)
            {
                BKExplainedNumber fatigue = BannerKingsConfig.Instance.WarModel.CalculateFatigue(war, Hero.MainHero.MapFaction, true);
                result.AddFactor(fatigue.ResultNumber);
            }

            return result.ResultNumber;
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
            if (stance.GetDailyTributePaid(factionDeclaredWar) < 0)
            {
                return new ExplainedNumber(-50000f);
            }

            WarStats attackerStats = CalculateWarStats(factionDeclaresWar, factionDeclaredWar);
            float attackerScore = attackerStats.Strength + attackerStats.ValueOfSettlements - (attackerStats.TotalStrengthOfEnemies * 1.25f);

            if (factionDeclaredWar.IsKingdomFaction && factionDeclaresWar.IsKingdomFaction)
            {
                var attackerKingdom = (Kingdom)factionDeclaresWar;
                var defenderKingdom = (Kingdom)factionDeclaredWar;

                TextObject reason;
                bool warAllowed = Campaign.Current.Models.KingdomDecisionPermissionModel
                    .IsWarDecisionAllowedBetweenKingdoms(attackerKingdom, defenderKingdom, out reason);
                if (!warAllowed)
                {
                    return new ExplainedNumber(-50000f);
                }

                var tributes = factionDeclaresWar.Stances.ToList().FindAll(x => x.GetDailyTributePaid(x.Faction2) > 0);
                result.AddFactor(-0.15f * tributes.Count);
   
                KingdomDiplomacy diplomacy = Campaign.Current.GetCampaignBehavior<BKDiplomacyBehavior>().GetKingdomDiplomacy(attackerKingdom);
                if (diplomacy != null)
                {
                    if (diplomacy.HasTradePact(defenderKingdom))
                    {
                        result.AddFactor(-0.25f);
                    }

                    foreach (var casusBelli in diplomacy.GetAvailableCasusBelli(defenderKingdom))
                    {
                        result.Add(casusBelli.DeclareWarScore);
                    }
                }

                foreach (Kingdom enemyKingdom in FactionManager.GetEnemyKingdoms(attackerKingdom))
                {
                    WarStats enemyStats = CalculateWarStats(factionDeclaredWar, factionDeclaresWar);
                    float enemyScore = enemyStats.Strength + enemyStats.ValueOfSettlements - (enemyStats.TotalStrengthOfEnemies * 1.25f);
                    float proportion = MathF.Clamp((attackerScore / (enemyScore * 4f)) - 1f, -1f, 0f);
                    result.AddFactor(proportion);
                }
            }     
           
            WarStats defenderStats = CalculateWarStats(factionDeclaredWar, factionDeclaresWar);
            float defenderScore = defenderStats.Strength + defenderStats.ValueOfSettlements - (defenderStats.TotalStrengthOfEnemies * 1.25f);
            float scoreProportion = (attackerScore / defenderScore) - 1f;
            result.AddFactor(scoreProportion);

            float relations = attackerStats.RulingClan.GetRelationWithClan(defenderStats.RulingClan);
            result.AddFactor(relations * -0.003f);

            DefaultDiplomacyModel model = new DefaultDiplomacyModel();
            var getDistanceMethod = model.GetType()
                .GetMethod("GetDistance", BindingFlags.Instance | BindingFlags.NonPublic);

            float distanceFactor = (float)getDistanceMethod.Invoke(model, new object[] { factionDeclaresWar, factionDeclaredWar });
            float averageDistance = Campaign.AverageDistanceBetweenTwoFortifications;

            result.Add(MathF.Clamp(averageDistance / averageDistance, -0.9f, 0f));

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


