using BannerKings.Managers.Skills;
using BannerKings.Settings;
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

            if (party is {LeaderHero: { }} &&
                TaleWorlds.CampaignSystem.Campaign.Current.MapSceneWrapper.GetFaceTerrainType(party.CurrentNavigationFace) == TerrainType.Forest)
            {
                var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(party.LeaderHero);
                if (education.HasPerk(BKPerks.Instance.OutlawNightPredator))
                {
                    result *= 1.5f;
                }
            }

            return result;
        }

        public override ExplainedNumber GetPartySpottingRange(MobileParty party, bool includeDescriptions = false)
        {
            ExplainedNumber result = base.GetPartySpottingRange(party, includeDescriptions);
            if (TaleWorlds.CampaignSystem.Campaign.Current.MapSceneWrapper.GetFaceTerrainType(party.CurrentNavigationFace) == TerrainType.Forest)
            {
                result.AddFactor(-0.4f);
            }

            return result;
        }

        public override float GetHideoutSpottingDistance()
        {
            return base.GetHideoutSpottingDistance() / BannerKingsSettings.Instance.HideoutSpotDifficulty;
        }
    }
}