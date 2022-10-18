using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings.Models.BKModels
{
    public class BKEstatesModel
    {
        public int MinimumEstateAcreage => 120;

        public float MaximumEstateAcreagePercentage => 0.12f;


        public ExplainedNumber CalculateEstatesMaximum(Settlement settlement, bool explanations = false)
        {
            var result = new ExplainedNumber(0f, explanations);
            if (settlement.IsVillage)
            {
                var landOwners = settlement.Notables.Count(x => x.Occupation == Occupation.RuralNotable);
                result.Add(landOwners);
                result.Add(1);
            }

            return result;
        }
    }
}
