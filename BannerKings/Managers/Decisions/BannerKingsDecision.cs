using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Decisions
{
    public abstract class BannerKingsDecision
    {
        protected BannerKingsDecision(Settlement settlement, bool enabled)
        {
            Settlement = settlement;
            Enabled = enabled;
        }

        [SaveableProperty(1)] public Settlement Settlement { get; set; }

        [SaveableProperty(2)] public bool Enabled { get; set; }

        public abstract string GetHint();
        public abstract string GetName();
        public abstract string GetIdentifier();

        public void OnChange(bool value)
        {
            Enabled = value;
        }
    }
}