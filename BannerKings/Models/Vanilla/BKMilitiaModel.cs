using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Localization;
using static BannerKings.Managers.PopulationManager;
using BannerKings.Populations;
using static BannerKings.Managers.Policies.BKMilitiaPolicy;
using BannerKings.Managers.Policies;
using BannerKings.Managers.Populations.Villages;
using BannerKings.Managers.Court;

namespace BannerKings.Models
{
    public class BKMilitiaModel : DefaultSettlementMilitiaModel
    {
        public override void CalculateMilitiaSpawnRate(Settlement settlement, out float meleeTroopRate, out float rangedTroopRate)
        {
            if (BannerKingsConfig.Instance.PolicyManager != null)
            {
                MilitiaPolicy policy = ((BKMilitiaPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(settlement, "militia")).Policy;
                if (policy == MilitiaPolicy.Melee)
                {
                    meleeTroopRate = 0.75f;
                    rangedTroopRate = 0.25f;
                } else if (policy == MilitiaPolicy.Ranged)
                {
                    meleeTroopRate = 0.25f;
                    rangedTroopRate = 0.75f;
                } else base.CalculateMilitiaSpawnRate(settlement, out meleeTroopRate, out rangedTroopRate);
            } else
                base.CalculateMilitiaSpawnRate(settlement, out meleeTroopRate, out rangedTroopRate);
        }

        public override ExplainedNumber CalculateMilitiaChange(Settlement settlement, bool includeDescriptions = false)
        {
            ExplainedNumber baseResult = base.CalculateMilitiaChange(settlement, includeDescriptions);
            if (BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
            {
                PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                int serfs = data.GetTypeCount(PopType.Serfs);
                float maxMilitia = GetMilitiaLimit(data, settlement);
                float filledCapacity = settlement.IsVillage ? settlement.Village.Militia / maxMilitia : settlement.Town.Militia / maxMilitia;
                float baseGrowth = (float)serfs * 0.0025f;

                if (BannerKingsConfig.Instance.PolicyManager.IsDecisionEnacted(settlement, "decision_militia_encourage"))
                    baseResult.Add(baseGrowth * (1f - 1f * filledCapacity), new TextObject("Conscription policy"));
                else if (filledCapacity > 1f)
                    baseResult.Add(baseGrowth * -1f * filledCapacity, new TextObject("Over supported limit"));

                VillageData villageData = data.VillageData;
                if (villageData != null)
                {
                    float trainning = villageData.GetBuildingLevel(DefaultVillageBuildings.Instance.TrainningGrounds);
                    if (trainning > 0)
                        baseResult.Add(trainning == 1 ? 0.2f : (trainning == 2 ? 0.5f : 1f), new TextObject("{=BkTiRPT4}Training Fields"));
                }

                BannerKingsConfig.Instance.CourtManager.ApplyCouncilEffect(ref baseResult, settlement.OwnerClan.Leader, CouncilPosition.Marshall, 1f, false);
            }

            return baseResult;
        }

        public float GetMilitiaLimit(PopulationData data, Settlement settlement)
        {
            if (settlement.IsCastle)
                return (float)data.TotalPop * 0.1f + 200f;
            else if (settlement.IsVillage)
                return (float)data.TotalPop * 0.05f + 20f;
            else return (float)data.TotalPop * 0.02f + 100f;
        }

        public override float CalculateEliteMilitiaSpawnChance(Settlement settlement)
        {
            float baseResult = base.CalculateEliteMilitiaSpawnChance(settlement) + (settlement.IsTown ? 0.12f : 0.20f);
            if (BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
            {
                PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                if (BannerKingsConfig.Instance.PolicyManager.IsDecisionEnacted(settlement, "decision_militia_subsidize"))
                    baseResult += 0.12f;

                VillageData villageData = data.VillageData;
                if (villageData != null)
                {
                    float warehouse = villageData.GetBuildingLevel(DefaultVillageBuildings.Instance.Warehouse);
                    if (warehouse > 0)
                        baseResult += 0.04f * warehouse;
                }
            }
                
            return baseResult;
        }
    }
}
