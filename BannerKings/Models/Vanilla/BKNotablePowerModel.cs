using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;

namespace BannerKings.Models.Vanilla
{
    public class BKNotablePowerModel : DefaultNotablePowerModel
    {

        public override ExplainedNumber CalculateDailyPowerChangeForHero(Hero hero, bool includeDescriptions = false)
        {
            ExplainedNumber result = base.CalculateDailyPowerChangeForHero(hero, includeDescriptions);
            if (hero.CurrentSettlement != null && hero.CurrentSettlement.Town != null && hero.GovernorOf == hero.CurrentSettlement.Town)
                result.Add(0.3f);

            return result;
        }
    }
}
