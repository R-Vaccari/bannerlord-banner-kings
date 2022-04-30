using BannerKings.Managers.Titles;
using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

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
            Completion = completion;
            DueTime = dueTime;
            Type = type;
        }

        public abstract void Tick();
        public abstract void Finish();
    }
}
