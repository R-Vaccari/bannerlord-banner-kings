using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings.Models.Vanilla
{
    public class BKWallHitpointModel : DefaultWallHitPointCalculationModel
    {
        public override float CalculateMaximumWallHitPoint(Town town)
        {
            var result = base.CalculateMaximumWallHitPoint(town);

            if (town.OwnerClan != null)
            {
                var leader = town.OwnerClan.Leader;
                if (leader != null)
                {
                    var data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(leader);
                    if (data.HasPerk(BKPerks.Instance.SiegePlanner))
                    {
                        result *= 1.25f;
                    }
                }
            }


            return result;
        }
    }
}