using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Localization;
using static BannerKings.Managers.PopulationManager;
using static BannerKings.Managers.PolicyManager;
using BannerKings.Populations;
using BannerKings.Managers.Populations.Villages;
using TaleWorlds.Library;

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
                    float nobles = data.GetTypeCount(PopType.Nobles);
                    baseResult.Add(MBMath.ClampFloat(nobles * 0.01f, 0f, 12f), new TextObject(string.Format("Nobles influence from {0}", settlement.Name)));

                    VillageData villageData = data.VillageData;
                    if (villageData != null)
                    {
                        float manor = villageData.GetBuildingLevel(DefaultVillageBuildings.Instance.TrainningGrounds);
                        if (manor > 0)
                            baseResult.AddFactor(manor == 3 ? 0.5f : manor * 0.15f, new TextObject("{=!}Manor"));
                    }

                    if (BannerKingsConfig.Instance.PopulationManager.PopSurplusExists(settlement, PopType.Nobles, true))
                    {
                        float result = baseResult.ResultNumber;
                        float extra = BannerKingsConfig.Instance.PopulationManager.GetPopCountOverLimit(settlement, PopType.Nobles);
                        baseResult.Add(MBMath.ClampFloat(extra * -0.01f, result * -0.5f, -0.1f), new TextObject(string.Format("Excess noble population at {0}", settlement.Name)));
                    }
                }
            }
                
            return baseResult;
        }
    }
}
