using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Rites
{
    public abstract class Rite
    {
        public abstract TextObject GetName();
        public abstract TextObject GetDescription();
        public abstract RiteType GetRiteType();
        public abstract float GetTimeInterval();

        public abstract void Execute(Hero executor);
        public abstract void SetDialogue();
        public abstract float GetPietyReward();
        public abstract bool MeetsCondition(Hero hero);
        public abstract void Complete(Hero actionTaker);
    }

    public enum RiteType
    {
        OFFERING,
        SACRIFICE
    }
}
