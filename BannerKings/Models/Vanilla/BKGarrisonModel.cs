using System.Linq;
using BannerKings.Managers.Policies;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static BannerKings.Managers.Policies.BKGarrisonPolicy;

namespace BannerKings.Models.Vanilla
{
    public class BKGarrisonModel : DefaultSettlementGarrisonModel
    {
        public override ExplainedNumber CalculateGarrisonChange(Settlement settlement, bool includeDescriptions = false)
        {
            var baseResult = base.CalculateGarrisonChange(settlement, includeDescriptions);
            if (BannerKingsConfig.Instance.PopulationManager != null &&
                BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
            {
                var garrison =
                    ((BKGarrisonPolicy) BannerKingsConfig.Instance.PolicyManager.GetPolicy(settlement, "garrison")).Policy;
                switch (garrison)
                {
                    case GarrisonPolicy.Dischargement:
                        baseResult.Add(-1f, new TextObject("{=FS1Wdj52B}Garrison policy"));
                        break;
                    case GarrisonPolicy.Enlistment:
                        baseResult.Add(1f, new TextObject("{=FS1Wdj52B}Garrison policy"));
                        break;
                }
            }

            return baseResult;
        }

        public override int FindNumberOfTroopsToLeaveToGarrison(MobileParty mobileParty, Settlement settlement)
        {
            var result = base.FindNumberOfTroopsToLeaveToGarrison(mobileParty, settlement);
            if (result > 0)
            {
                var kingdom = settlement.OwnerClan.Kingdom;
                if (kingdom != null)
                {
                    float enemies = FactionManager.GetEnemyKingdoms(kingdom).Count();
                    var strength = 0f;
                    if (settlement.Town is {GarrisonParty: { }})
                    {
                        strength = settlement.Town.GarrisonParty.MemberRoster.TotalManCount;
                    }

                    if (strength > 500)
                    {
                        return 0;
                    }

                    if (enemies == 0)
                    {
                        return 0;
                    }

                    return (int) MathF.Max(result / 10f - enemies, 0f);
                }
            }

            return result;
        }
    }
}