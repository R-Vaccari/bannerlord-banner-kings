using BannerKings.Managers.Education;
using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;

namespace BannerKings.Models.Vanilla
{
    public class BKPartySpeedModel : DefaultPartySpeedCalculatingModel
    {

        public override ExplainedNumber CalculateFinalSpeed(MobileParty mobileParty, ExplainedNumber finalSpeed)
        {
            ExplainedNumber baseResult = base.CalculateFinalSpeed(mobileParty, finalSpeed);
            if (mobileParty.LeaderHero != null)
            {
                EducationData data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(mobileParty.LeaderHero);
                if (data.Perks.Contains(BKPerks.Instance.FianHighlander))
                    baseResult.AddFactor(0.05f, BKPerks.Instance.FianHighlander.Name);
            }

            return baseResult;
        }
    }
}
