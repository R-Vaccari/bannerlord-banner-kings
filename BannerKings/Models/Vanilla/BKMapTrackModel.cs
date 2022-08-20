using BannerKings.Managers.Education;
using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;

namespace BannerKings.Models.Vanilla
{
    public class BKMapTrackModel : DefaultMapTrackModel
    {

        public override int GetTrackLife(MobileParty mobileParty)
        {
            float baseResult = base.GetTrackLife(mobileParty);
            if (mobileParty.LeaderHero != null)
            {
                EducationData data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(mobileParty.LeaderHero);
                if (data.Perks.Contains(BKPerks.Instance.FianRanger)) baseResult *= 0.2f;
            }
            
            return (int)baseResult;
        }
    }
}
