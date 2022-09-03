using BannerKings.Managers.Titles;
using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Duties
{
    public abstract class BannerKingsDuty
    {
        public BannerKingsDuty(CampaignTime dueTime, FeudalDuties type, float completion = 0f)
        {
            Completion = completion;
            DueTime = dueTime;
            Type = type;
        }

        [SaveableProperty(1)] public float Completion { get; set; }

        [SaveableProperty(2)] public CampaignTime DueTime { get; protected set; }

        [SaveableProperty(3)] public FeudalDuties Type { get; set; }

        public abstract void Tick();
        public abstract void Finish();
    }
}