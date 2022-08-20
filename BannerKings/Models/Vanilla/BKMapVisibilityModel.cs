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
            var result = base.GetPartySpottingDifficulty(spottingParty, party);

            if (party != null && party.LeaderHero != null &&
                Campaign.Current.MapSceneWrapper.GetFaceTerrainType(party.CurrentNavigationFace) == TerrainType.Forest)
            {
                var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(party.LeaderHero);
                if (education.HasPerk(BKPerks.Instance.OutlawNightPredator))
                {
                    result *= 1.5f;
                }
            }

            return result;
        }
    }
}