using BannerKings.Managers.Policies;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Localization;
using System.Linq;
using static BannerKings.Managers.Policies.BKGarrisonPolicy;
using TaleWorlds.Library;

namespace BannerKings.Models.Vanilla
{
    public class BKGarrisonModel : DefaultSettlementGarrisonModel
    {

        public override ExplainedNumber CalculateGarrisonChange(Settlement settlement, bool includeDescriptions = false)
        {
            ExplainedNumber baseResult = base.CalculateGarrisonChange(settlement, includeDescriptions);
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

        public override int FindNumberOfTroopsToLeaveToGarrison(MobileParty mobileParty, Settlement settlement)
        {
            int result = base.FindNumberOfTroopsToLeaveToGarrison(mobileParty, settlement);
            if (result > 0)
            {
                Kingdom kingdom = settlement.OwnerClan.Kingdom;
                if (kingdom != null)
                {
                    float enemies = FactionManager.GetEnemyKingdoms(kingdom).Count();
                    float strength = 0f;
                    if (settlement.Town != null && settlement.Town.GarrisonParty != null)
                        strength = settlement.Town.GarrisonParty.MemberRoster.TotalManCount;
                    if (strength > 500) return 0;

                    if (enemies == 0) return 0;
                    return (int)MathF.Max(((float)result / 10f - enemies), 0f);
                }
            }

            return result;
        }
    }
}
