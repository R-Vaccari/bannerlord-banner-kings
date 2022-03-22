using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;
using static BannerKings.Managers.TitleManager;

namespace BannerKings.Managers.Duties
{
    public abstract class BannerKingsDuty
    {
        [SaveableProperty(1)]
        public float Completion { get; private set; }

        [SaveableProperty(2)]
        public CampaignTime DueTime { get; protected set; }

        [SaveableProperty(3)]
        public FeudalDuties Type { get; private set; }

        public BannerKingsDuty(CampaignTime dueTime, FeudalDuties type, float completion = 0f)
        {
            this.Completion = completion;
            this.DueTime = dueTime;
            this.Type = type;
        }

        public abstract void Tick();
        public abstract void Finish();
    }
}
