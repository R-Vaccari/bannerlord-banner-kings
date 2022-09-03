using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;

namespace BannerKings.Models.Vanilla
{
    public class BKNotablePowerModel : DefaultNotablePowerModel
    {
        public override ExplainedNumber CalculateDailyPowerChangeForHero(Hero hero, bool includeDescriptions = false)
        {
            var result = base.CalculateDailyPowerChangeForHero(hero, includeDescriptions);
            if (hero.CurrentSettlement is {Town: { }} &&
                hero.GovernorOf == hero.CurrentSettlement.Town)
            {
                result.Add(0.3f);
            }

            return result;
        }
    }
}