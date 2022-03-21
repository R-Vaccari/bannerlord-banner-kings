using TaleWorlds.CampaignSystem;
using static BannerKings.Managers.TitleManager;

namespace BannerKings.Managers.Duties
{
    public abstract class BannerKingsDuty
    {

        public float Completion { get; private set; }

        public CampaignTime DueTime { get; protected set; }

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
