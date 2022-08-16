using BannerKings.Behaviours;
using BannerKings.Managers.CampaignStart;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;

namespace BannerKings.Models.Vanilla
{
    public class BKCrimeModel : DefaultCrimeModel
    {

        public override ExplainedNumber GetDailyCrimeRatingChange(IFaction faction, bool includeDescriptions = false)
        {
            ExplainedNumber result = base.GetDailyCrimeRatingChange(faction, includeDescriptions);
            if (Campaign.Current.GetCampaignBehavior<BKCampaignStartBehavior>().HasDebuff(DefaultStartOptions.Instance.Outlaw))
                return new ExplainedNumber(0f, includeDescriptions, DefaultStartOptions.Instance.Outlaw.Name);

            return result;
        }
    }
}
