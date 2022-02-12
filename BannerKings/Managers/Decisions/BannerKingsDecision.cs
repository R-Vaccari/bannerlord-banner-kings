using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Decisions
{
    public abstract class BannerKingsDecision
    {
        [SaveableProperty(1)]
        public Settlement Settlement { get; private set; }

        [SaveableProperty(2)]
        public bool Enabled { get; protected set; }

        public BannerKingsDecision(Settlement settlement, bool enabled)
        {
            this.Settlement = settlement;
            this.Enabled = enabled;
        }

        public abstract string GetHint();
        public abstract string GetName();
        public abstract string GetIdentifier();
        public void OnChange(bool value)
        {
            this.Enabled = value;
        }
    }
}
