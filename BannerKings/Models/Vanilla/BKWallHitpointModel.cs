using BannerKings.Managers.Education;
using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings.Models.Vanilla
{
    public class BKWallHitpointModel : DefaultWallHitPointCalculationModel
    {

        public override float CalculateMaximumWallHitPoint(Town town)
        {
            float result = base.CalculateMaximumWallHitPoint(town);

            if (town.OwnerClan != null)
            {
                Hero leader = town.OwnerClan.Leader;
                if (leader != null)
                {
                    EducationData data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(leader);
                    if (data.HasPerk(BKPerks.Instance.SiegePlanner))
                        result *= 1.25f;
                }
            }
            

            return result;
        }
    }
}
