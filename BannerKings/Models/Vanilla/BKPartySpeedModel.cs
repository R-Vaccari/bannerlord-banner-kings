using BannerKings.Behaviours;
using BannerKings.Managers.CampaignStart;
using BannerKings.Managers.Education;
using BannerKings.Managers.Education.Lifestyles;
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

                if (data.Lifestyle != null && data.Lifestyle.Equals(DefaultLifestyles.Instance.Outlaw))
                {
                    int count = 0;
                    foreach (TroopRosterElement element in mobileParty.MemberRoster.GetTroopRoster())
                        if (element.Character.IsHero || element.Character.Occupation == Occupation.Bandit)
                            count += element.Number;

                    baseResult.AddFactor((count / mobileParty.MemberRoster.TotalManCount) * 0.1f, data.Lifestyle.Name);
                }
            }

            if (mobileParty.IsCaravan && mobileParty.Owner != null)
            {
                EducationData data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(mobileParty.Owner);
                if (Campaign.Current.IsDay && data.HasPerk(BKPerks.Instance.CaravaneerDealer))
                    baseResult.AddFactor(0.04f, BKPerks.Instance.FianHighlander.Name);
            }

            if (mobileParty.LeaderHero == Hero.MainHero && Campaign.Current.GetCampaignBehavior<BKCampaignStartBehavior>().HasDebuff(DefaultStartOptions.Instance.Caravaneer))
                baseResult.AddFactor(-0.05f, DefaultStartOptions.Instance.Caravaneer.Name);

            return baseResult;
        }
    }
}
