using BannerKings.Managers.Education;
using BannerKings.Managers.Populations.Villages;
using BannerKings.Managers.Skills;
using BannerKings.Populations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;

namespace BannerKings.Models.Vanilla
{
    public class BKRaidModel : DefaultRaidModel
    {

        public override float CalculateHitDamage(MapEventSide attackerSide, float settlementHitPoints)
        {
            float result = base.CalculateHitDamage(attackerSide, settlementHitPoints);
            PartyBase attacker = attackerSide.LeaderParty;
            if (attacker != null && attacker.LeaderHero != null)
            {
                EducationData education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(attacker.LeaderHero);
                if (education.HasPerk(BKPerks.Instance.OutlawPlunderer))
                    result *= 1.15f;
            }


            MapEventSide defender = attackerSide.MapEvent.DefenderSide;
            Settlement settlement = null;
            if (defender.Parties != null && defender.Parties.Count > 0)
                settlement = defender.Parties[0].Party.Settlement;

            if (settlement != null)
                if (BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
                {
                    VillageData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement).VillageData;
                    int palisade = data.GetBuildingLevel(DefaultVillageBuildings.Instance.Palisade);
                    if (palisade > 0)
                        result *= (1f - (0.12f * palisade));
                }

            return result;
        }
    }
}
