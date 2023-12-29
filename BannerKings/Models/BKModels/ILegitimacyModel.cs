using BannerKings.Behaviours.Diplomacy;
using BannerKings.Utils.Models;

namespace BannerKings.Models.BKModels
{
    public abstract class LegitimacyModel
    {
        public abstract BKExplainedNumber CalculateEffect(KingdomDiplomacy diplomacy, bool explanations = false);
    }
}
