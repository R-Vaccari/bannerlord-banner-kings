using BannerKings.Behaviours.Diplomacy;
using BannerKings.Utils.Models;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Models.BKModels
{
    public abstract class LegitimacyModel
    {
        public abstract BKExplainedNumber CalculateKingdomLegitimacy(KingdomDiplomacy diplomacy, bool explanations = false);
        public abstract BKExplainedNumber CalculateLegitimacy(Hero hero, bool compareToRuler, KingdomDiplomacy diplomacy, bool explanations = false);
    }
}
