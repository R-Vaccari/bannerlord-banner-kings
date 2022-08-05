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
                if (data.HasPerk(BKPerks.Instance.FianHighlander))
                    baseResult.AddFactor(0.05f, BKPerks.Instance.FianHighlander.Name);

                if (data.HasPerk(BKPerks.Instance.CaravaneerStrider))
                    baseResult.AddFactor(0.03f, BKPerks.Instance.CaravaneerStrider.Name);

                if (Campaign.Current.IsNight && data.HasPerk(BKPerks.Instance.OutlawNightPredator))
                    baseResult.AddFactor(0.06f, BKPerks.Instance.OutlawNightPredator.Name);
            }

            if (mobileParty.IsCaravan && mobileParty.Owner != null)
            {
                EducationData data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(mobileParty.Owner);
                if (Campaign.Current.IsDay && data.HasPerk(BKPerks.Instance.CaravaneerDealer))
                    baseResult.AddFactor(0.04f, BKPerks.Instance.FianHighlander.Name);
            }

            return baseResult;
        }
    }
}
