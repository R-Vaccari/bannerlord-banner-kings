using BannerKings.Managers.Policies;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Localization;
using static BannerKings.Managers.Policies.BKGarrisonPolicy;

namespace BannerKings.Models.Vanilla
{
    public class BKGarrisonModel : DefaultSettlementGarrisonModel
    {

        public override ExplainedNumber CalculateGarrisonChangeAutoRecruitment(Settlement settlement, bool includeDescriptions = false)
        {
            ExplainedNumber baseResult = base.CalculateGarrisonChangeAutoRecruitment(settlement, includeDescriptions);
            if (BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
            {
                GarrisonPolicy garrison = ((BKGarrisonPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(settlement, "garrison")).Policy;
                if (garrison == GarrisonPolicy.Dischargement)
                    baseResult.Add(-1f, new TextObject("{=!}Garrison policy"));
                else if (garrison == GarrisonPolicy.Enlistment)
                    baseResult.Add(1f, new TextObject("{=!}Garrison policy"));
            }

            return baseResult;
        }
    }
}
