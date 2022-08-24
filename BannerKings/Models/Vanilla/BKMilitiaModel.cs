using BannerKings.Managers.Court;
using BannerKings.Managers.Policies;
using BannerKings.Managers.Populations;
using BannerKings.Managers.Populations.Villages;
using BannerKings.Managers.Titles;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;
using static BannerKings.Managers.PopulationManager;
using static BannerKings.Managers.Policies.BKMilitiaPolicy;

namespace BannerKings.Models.Vanilla
{
    public class BKMilitiaModel : DefaultSettlementMilitiaModel
    {
        public override void CalculateMilitiaSpawnRate(Settlement settlement, out float meleeTroopRate,
            out float rangedTroopRate)
        {
            if (BannerKingsConfig.Instance.PolicyManager != null)
            {
                var policy = ((BKMilitiaPolicy) BannerKingsConfig.Instance.PolicyManager.GetPolicy(settlement, "militia"))
                    .Policy;
                switch (policy)
                {
                    case MilitiaPolicy.Melee:
                        meleeTroopRate = 0.75f;
                        rangedTroopRate = 0.25f;
                        break;
                    case MilitiaPolicy.Ranged:
                        meleeTroopRate = 0.25f;
                        rangedTroopRate = 0.75f;
                        break;
                    default:
                        base.CalculateMilitiaSpawnRate(settlement, out meleeTroopRate, out rangedTroopRate);
                        break;
                }
            }
            else
            {
                base.CalculateMilitiaSpawnRate(settlement, out meleeTroopRate, out rangedTroopRate);
            }
        }

        public override ExplainedNumber CalculateMilitiaChange(Settlement settlement, bool includeDescriptions = false)
        {
            var baseResult = base.CalculateMilitiaChange(settlement, includeDescriptions);
            if (BannerKingsConfig.Instance.PopulationManager != null &&
                BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
            {
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                var serfs = data.GetTypeCount(PopType.Serfs);
                var maxMilitia = GetMilitiaLimit(data, settlement).ResultNumber;
                var filledCapacity = settlement.IsVillage
                    ? settlement.Village.Militia / maxMilitia
                    : settlement.Town.Militia / maxMilitia;
                var baseGrowth = serfs * 0.0025f;

                if (BannerKingsConfig.Instance.PolicyManager.IsDecisionEnacted(settlement, "decision_militia_encourage"))
                {
                    baseResult.Add(baseGrowth * (1f - 1f * filledCapacity), new TextObject("{=1aq83aPr}Conscription policy"));
                }
                else if (filledCapacity > 1f)
                {
                    baseResult.Add(baseGrowth * -1f * filledCapacity, new TextObject("{=0atu0kiG}Over supported limit"));
                }

                var villageData = data.VillageData;
                if (villageData != null)
                {
                    float trainning = villageData.GetBuildingLevel(DefaultVillageBuildings.Instance.TrainningGrounds);
                    if (trainning > 0)
                    {
                        baseResult.Add(trainning == 1 ? 0.2f : trainning == 2 ? 0.5f : 1f,
                            new TextObject("{=c6pesaYL}Training Fields"));
                    }
                }

                BannerKingsConfig.Instance.CourtManager.ApplyCouncilEffect(ref baseResult, settlement.OwnerClan.Leader,
                    CouncilPosition.Marshall, 1f, false);
            }

            return baseResult;
        }

        public ExplainedNumber GetMilitiaLimit(PopulationData data, Settlement settlement)
        {
            var result = new ExplainedNumber(0f, true);
            result.Add(data.TotalPop * 0.1f, new TextObject("{=bLbvfBnb}Total population"));

            if (settlement.IsCastle)
            {
                result.Add(200f, new TextObject("{=UPhMZ859}Castle"));
            }
            else if (settlement.IsVillage)
            {
                result.Add(20f, new TextObject("{=esr9rn30}Village"));
            }
            else
            {
                result.Add(100f, new TextObject("{=FO8mvaZJ}Town"));
            }

            return result;
        }

        public override float CalculateEliteMilitiaSpawnChance(Settlement settlement)
        {
            var baseResult = +(settlement.IsTown ? 0.12f : 0.20f);
            if (BannerKingsConfig.Instance.PopulationManager != null &&
                BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
            {
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                if (BannerKingsConfig.Instance.PolicyManager.IsDecisionEnacted(settlement, "decision_militia_subsidize"))
                {
                    baseResult += 0.12f;
                }

                var government = BannerKingsConfig.Instance.TitleManager.GetSettlementGovernment(settlement);
                if (government == GovernmentType.Tribal)
                {
                    baseResult += 0.08f;
                }

                var villageData = data.VillageData;
                if (villageData != null)
                {
                    float warehouse = villageData.GetBuildingLevel(DefaultVillageBuildings.Instance.Warehouse);
                    if (warehouse > 0)
                    {
                        baseResult += 0.04f * warehouse;
                    }
                }
            }

            return baseResult;
        }

        public ExplainedNumber MilitiaSpawnChanceExplained(Settlement settlement)
        {
            var result =
                new ExplainedNumber(base.CalculateEliteMilitiaSpawnChance(settlement) + (settlement.IsTown ? 0.12f : 0.20f),
                    true);
            if (BannerKingsConfig.Instance.PopulationManager != null &&
                BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
            {
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                if (BannerKingsConfig.Instance.PolicyManager.IsDecisionEnacted(settlement, "decision_militia_subsidize"))
                {
                    result.Add(0.12f, new TextObject("{=nPBwLDwE}Subsidize militia"));
                }

                var government = BannerKingsConfig.Instance.TitleManager.GetSettlementGovernment(settlement);
                if (government == GovernmentType.Tribal)
                {
                    result.Add(0.08f, new TextObject("{=PSrEtF5L}Government"));
                }

                var villageData = data.VillageData;
                if (villageData != null)
                {
                    float warehouse = villageData.GetBuildingLevel(DefaultVillageBuildings.Instance.Warehouse);
                    if (warehouse > 0)
                    {
                        result.Add(0.04f * warehouse, DefaultVillageBuildings.Instance.Warehouse.Name);
                    }
                }
            }

            return result;
        }
    }
}