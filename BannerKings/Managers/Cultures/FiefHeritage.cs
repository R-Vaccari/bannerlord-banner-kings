using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings.Managers.Cultures
{
    public class FiefHeritage : BannerKingsObject
    {
        public FiefHeritage(string stringId) : base(stringId)
        {
        }

        public Settlement Settlement { get; private set; }
        public CultureObject Culture { get; private set; }
    }
}
