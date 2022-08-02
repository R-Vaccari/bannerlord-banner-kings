using BannerKings.Managers.Education;
using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Party;

namespace BannerKings.Models.Vanilla
{
    public class BKPartyMoraleModel : DefaultPartyMoraleModel
    {

        public override ExplainedNumber GetEffectivePartyMorale(MobileParty mobileParty, bool includeDescription = false)
        {
            ExplainedNumber result = base.GetEffectivePartyMorale(mobileParty, includeDescription);

            if (mobileParty.LeaderHero != null)
            {
                EducationData data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(mobileParty.LeaderHero);
                if (data.Perks.Contains(BKPerks.Instance.AugustCommander))
                    result.Add(3f, BKPerks.Instance.AugustCommander.Name);
            }

            return result;
        }
    }
}
