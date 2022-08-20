using BannerKings.Managers.Education;
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
            ExplainedNumber result = base.CalculateDailyFoodConsumptionf(party, baseConsumption);
            if (party.Army != null && party.SiegeEvent != null)
            {
                Hero leader = party.Army.LeaderParty.LeaderHero;
                if (leader != null)
                {
                    EducationData data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(leader);
                    if (data.HasPerk(BKPerks.Instance.SiegeOverseer))
                        result.AddFactor(-0.15f, BKPerks.Instance.SiegeOverseer.Name);
                }
            }

            return result;
        }
    }
}
