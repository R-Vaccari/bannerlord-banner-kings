using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Rites
{
    public abstract class Rite<T>
    {
        public abstract TextObject GetName();
        public abstract TextObject GetDescription();
        public abstract RiteType GetRiteType();

        public abstract void Complete(Hero actionTaker, T input);

        

        public enum RiteType
        {
            OFFERING,
            SACRIFICE
        }
    }
}
