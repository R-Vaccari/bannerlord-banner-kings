using BannerKings.Managers.Populations.Villages;
using BannerKings.Populations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;

namespace BannerKings.Models.Vanilla
{
    class BKRaidModel : DefaultRaidModel
    {

        public override float CalculateHitDamage(MapEventSide attackerSide, float settlementHitPoints)
        {
            float result = base.CalculateHitDamage(attackerSide, settlementHitPoints);

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

            if (attackerSide.LeaderParty.LeaderHero != null)
                if (attackerSide.LeaderParty.LeaderHero.Culture.HasFeat(CalradiaExpandedKingdoms.Feats.CEKFeats.NordlingPositiveFeatOne))
                    result *= CalradiaExpandedKingdoms.Feats.CEKFeats.NordlingPositiveFeatOne.EffectBonus;

            return result;
        }
    }
}
