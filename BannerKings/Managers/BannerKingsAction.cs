using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers
{
    public abstract class BannerKingsAction
    {
        public bool Possible { get; set; }
        public TextObject Reason { get; set; }
        public Hero ActionTaker { get; protected set; }
        public float Influence { get; set; }

        public abstract void TakeAction(Hero receiver = null);
    }
}