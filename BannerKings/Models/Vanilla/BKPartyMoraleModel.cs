using BannerKings.Behaviours;
using BannerKings.Managers.CampaignStart;
using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;

namespace BannerKings.Models.Vanilla
{
    public class BKPartyMoraleModel : DefaultPartyMoraleModel
    {
        public override ExplainedNumber GetEffectivePartyMorale(MobileParty mobileParty, bool includeDescription = false)
        {
            var result = base.GetEffectivePartyMorale(mobileParty, includeDescription);

            if (mobileParty.Owner == Hero.MainHero && Campaign.Current.GetCampaignBehavior<BKCampaignStartBehavior>()
                    .HasDebuff(DefaultStartOptions.Instance.Mercenary))
            {
                result.Add(-20f, DefaultStartOptions.Instance.Mercenary.Name);
            }

            if (mobileParty.LeaderHero != null)
            {
                var data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(mobileParty.LeaderHero);
                if (data.Perks.Contains(BKPerks.Instance.AugustCommander))
                {
                    result.Add(3f, BKPerks.Instance.AugustCommander.Name);
                }
            }

            return result;
        }
    }
}