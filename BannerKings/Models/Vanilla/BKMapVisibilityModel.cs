using BannerKings.Managers.Education;
using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;

namespace BannerKings.Models.Vanilla
{
    public class BKMapVisibilityModel : DefaultMapVisibilityModel
    {

        public override float GetPartySpottingDifficulty(MobileParty spottingParty, MobileParty party)
        {
            float result = base.GetPartySpottingDifficulty(spottingParty, party);

            if (party != null && party.LeaderHero != null && Campaign.Current.MapSceneWrapper.GetFaceTerrainType(party.CurrentNavigationFace) == TerrainType.Forest)
            {
                EducationData education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(party.LeaderHero);
                if (education.HasPerk(BKPerks.Instance.OutlawNightPredator))
                    result *= 1.5f;
            }

            return result;
        }
    }
}
