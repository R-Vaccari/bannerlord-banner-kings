using BannerKings.Managers.Education;
using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;

namespace BannerKings.Models.Vanilla
{
    public class BKPartyImpairmentModel : DefaultPartyImpairmentModel
    {

        public override float GetDisorganizedStateDuration(MobileParty party, bool isSiegeOrRaid)
        {
            float result = base.GetDisorganizedStateDuration(party, isSiegeOrRaid);
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
