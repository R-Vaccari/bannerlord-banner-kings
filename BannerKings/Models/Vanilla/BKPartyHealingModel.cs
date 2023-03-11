using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Models.Vanilla
{
    public class BKPartyHealingModel : DefaultPartyHealingModel
    {
        private static readonly TextObject _starvingText = new TextObject("{=jZYUdkXF}Starving");
        public override ExplainedNumber GetDailyHealingForRegulars(MobileParty party, bool includeDescriptions = false)
        {
            ExplainedNumber bonuses = base.GetDailyHealingForRegulars(party, includeDescriptions);
            Boolean isInBesiegedStarvingCity = party.CurrentSettlement != null && party.CurrentSettlement.IsUnderSiege && party.CurrentSettlement.IsStarving;
            if (isInBesiegedStarvingCity && !party.IsGarrison)
            {
                int num = MBRandom.RoundRandomized((float)party.MemberRoster.TotalRegulars * 0.1f);
                bonuses.Add(-num, _starvingText);
            }
            return bonuses;
        }
    }
}
