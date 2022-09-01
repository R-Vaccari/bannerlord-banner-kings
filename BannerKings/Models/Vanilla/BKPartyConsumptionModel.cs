using BannerKings.Managers.Education.Lifestyles;
using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;

namespace BannerKings.Models.Vanilla
{
    public class BKPartyConsumptionModel : DefaultMobilePartyFoodConsumptionModel
    {
        public override ExplainedNumber CalculateDailyFoodConsumptionf(MobileParty party, ExplainedNumber baseConsumption)
        {
            var result = base.CalculateDailyFoodConsumptionf(party, baseConsumption);
            if (party.Army != null && party.SiegeEvent != null)
            {
                var leader = party.Army.LeaderParty.LeaderHero;
                if (leader != null)
                {
                    var data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(leader);
                    if (data.HasPerk(BKPerks.Instance.SiegeOverseer))
                    {
                        result.AddFactor(-0.15f, BKPerks.Instance.SiegeOverseer.Name);
                    }

                    var faceTerrainType = Campaign.Current.MapSceneWrapper.GetFaceTerrainType(party.CurrentNavigationFace);
                    if (faceTerrainType == TaleWorlds.Core.TerrainType.Desert && data.Lifestyle != null &&
                        data.Lifestyle.Equals(DefaultLifestyles.Instance.Jawwal))
                    {
                        result.AddFactor(-0.3f, DefaultLifestyles.Instance.Jawwal.Name);
                    }
                }
            }

            return result;
        }
    }
}