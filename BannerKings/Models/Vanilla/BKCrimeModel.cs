using BannerKings.Behaviours;
using BannerKings.Behaviours.Criminality;
using BannerKings.Managers.CampaignStart;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;

namespace BannerKings.Models.Vanilla
{
    public class BKCrimeModel : DefaultCrimeModel
    {
        public ExplainedNumber GetMonetaryFine(Crime crime, bool explanations = false)
        {
            var result = new ExplainedNumber(0, explanations);

            return result;
        }

        public override ExplainedNumber GetDailyCrimeRatingChange(IFaction faction, bool includeDescriptions = false)
        {
            var result = base.GetDailyCrimeRatingChange(faction, includeDescriptions);
            if (TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKCampaignStartBehavior>()
                .HasDebuff(DefaultStartOptions.Instance.Outlaw))
            {
                return new ExplainedNumber(0f, includeDescriptions, DefaultStartOptions.Instance.Outlaw.Name);
            }

            return result;
        }
    }
}