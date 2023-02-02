using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Rites
{
    public abstract class Rite
    {
        public abstract TextObject GetName();
        public abstract TextObject GetDescription();
        public abstract float GetPietyReward();
        public abstract bool MeetsCondition(Hero hero);
        public abstract void Execute(Hero executor);
    }
}
