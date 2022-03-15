using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Localization;
using static BannerKings.Managers.PopulationManager;
using static BannerKings.Managers.PolicyManager;
using BannerKings.Populations;
using BannerKings.Managers.Populations.Villages;

namespace BannerKings.Models
{
    class BKInfluenceModel : DefaultClanPoliticsModel
    {
        public override ExplainedNumber CalculateInfluenceChange(Clan clan, bool includeDescriptions = false)
        {
            ExplainedNumber baseResult = base.CalculateInfluenceChange(clan, includeDescriptions);

            foreach (Settlement settlement in clan.Settlements)
            {
                if (BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
                {
                    PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                    int nobles = data.GetTypeCount(PopType.Nobles);
                    if (BannerKingsConfig.Instance.PopulationManager.PopSurplusExists(settlement, PopType.Nobles, true))
                    {
                        int extra = BannerKingsConfig.Instance.PopulationManager.GetPopCountOverLimit(settlement, PopType.Nobles);
                        baseResult.Add(((float)extra * -2f) * 0.01f, new TextObject(string.Format("Excess noble population at {0}", settlement.Name)));
                    }
                    baseResult.Add((float)nobles * 0.01f, new TextObject(string.Format("Nobles influence from {0}", settlement.Name)));

                    VillageData villageData = data.VillageData;
                    if (villageData != null)
                    {
                        float manor = villageData.GetBuildingLevel(DefaultVillageBuildings.Instance.TrainningGrounds);
                        if (manor > 0)
                            baseResult.AddFactor(manor == 3 ? 0.5f : manor * 0.15f, new TextObject("{=!}Manor"));
                    }
                }
            }
                
            return baseResult;
        }
    }
}
