using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using static Populations.Population;

namespace Populations.Models
{
    public class MilitiaModel : DefaultSettlementMilitiaModel
    {
        public override void CalculateMilitiaSpawnRate(Settlement settlement, out float meleeTroopRate, out float rangedTroopRate)
        {
            base.CalculateMilitiaSpawnRate(settlement, out meleeTroopRate, out rangedTroopRate);
        }

        public override ExplainedNumber CalculateMilitiaChange(Settlement settlement, bool includeDescriptions = false)
        {
            return base.CalculateMilitiaChange(settlement, includeDescriptions);
        }

        public override float CalculateEliteMilitiaSpawnChance(Settlement settlement)
        {
            return base.CalculateEliteMilitiaSpawnChance(settlement);
        }
    }
}
