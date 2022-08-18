using BannerKings.Managers.Education;
using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;

namespace BannerKings.Models.Vanilla
{
    public class BKPartyImpairmentModel : DefaultPartyImpairmentModel
    {

        public override float GetDisorganizedStateDuration(MobileParty party)
        {
            float result = base.GetDisorganizedStateDuration(party);
            if (party.LeaderHero != null)
            {
                EducationData data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(party.LeaderHero);
                if (data.HasPerk(BKPerks.Instance.OutlawKidnapper))
                    result *= 0.7f;
            }

            return result;
        }
    }
}
