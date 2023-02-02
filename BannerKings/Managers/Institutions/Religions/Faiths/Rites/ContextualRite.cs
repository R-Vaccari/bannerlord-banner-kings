using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Rites
{
    public abstract class ContextualRite : Rite
    {
        public abstract RiteType GetRiteType();
        public abstract float GetTimeInterval(Hero hero);
        public abstract void SetDialogue();
        public abstract void Complete(Hero actionTaker);
        public abstract TextObject GetRequirementsText(Hero hero);
    }

    public enum RiteType
    {
        OFFERING,
        SACRIFICE,
        DONATION
    }
}