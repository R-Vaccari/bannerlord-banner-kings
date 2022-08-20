using BannerKings.Managers.Populations.Villages;
using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings.Models.Vanilla
{
    public class BKRaidModel : DefaultRaidModel
    {
        public override float CalculateHitDamage(MapEventSide attackerSide, float settlementHitPoints)
        {
            var result = base.CalculateHitDamage(attackerSide, settlementHitPoints);
            var attacker = attackerSide.LeaderParty;
            if (attacker != null && attacker.LeaderHero != null)
            {
                var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(attacker.LeaderHero);
                if (education.HasPerk(BKPerks.Instance.OutlawPlunderer))
                {
                    result *= 1.15f;
                }
            }


            var defender = attackerSide.MapEvent.DefenderSide;
            Settlement settlement = null;
            if (defender.Parties != null && defender.Parties.Count > 0)
            {
                settlement = defender.Parties[0].Party.Settlement;
            }

            if (settlement != null)
            {
                if (BannerKingsConfig.Instance.PopulationManager != null &&
                    BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
                {
                    var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement).VillageData;
                    var palisade = data.GetBuildingLevel(DefaultVillageBuildings.Instance.Palisade);
                    if (palisade > 0)
                    {
                        result *= 1f - 0.12f * palisade;
                    }
                }
            }

            return result;
        }
    }
}