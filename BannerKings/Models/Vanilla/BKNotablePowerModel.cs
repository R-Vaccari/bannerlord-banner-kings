using BannerKings.Extensions;
using BannerKings.Managers.Populations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;

namespace BannerKings.Models.Vanilla
{
    public class BKNotablePowerModel : DefaultNotablePowerModel
    {
        public override ExplainedNumber CalculateDailyPowerChangeForHero(Hero hero, bool includeDescriptions = false)
        {
            ExplainedNumber result = base.CalculateDailyPowerChangeForHero(hero, includeDescriptions);
            if (hero.CurrentSettlement != null && hero.CurrentSettlement.Town != null && hero.GovernorOf == hero.CurrentSettlement.Town)
                result.Add(0.3f);

            if (1000f < hero.Power) result.Add((hero.Power / 1000f) * -3f);
            if (hero.IsPreacher && hero.OwnedWorkshops.Count == 0)
            {
                result.Add(0.1f);
                PopulationData data = hero.CurrentSettlement.PopulationData();
                if (data.ReligionData != null && data.ReligionData.DominantReligion == BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(hero))
                    result.Add(0.1f);
            }

            return result;
        }
    }
}