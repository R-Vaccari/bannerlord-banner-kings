using TaleWorlds.CampaignSystem;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Rites
{
    public abstract class ContextualRite : Rite
    {
        public abstract RiteType GetRiteType();
        public abstract float GetTimeInterval(Hero hero);
    }

    public enum RiteType
    {
        OFFERING,
        SACRIFICE,
        DONATION,
        TOTEM
    }
}