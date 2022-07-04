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

        public abstract float GetPietyReward();
        public bool MeetsCondition(Hero hero) 
        {
            FaithfulData data = BannerKingsConfig.Instance.ReligionsManager.GetFaithfulData(hero);
            return hero.IsAlive && !hero.IsChild && !hero.IsPrisoner && hero.PartyBelongedTo != null &&
                data != null && data.HasTimePassedForRite(GetRiteType(), GetTimeInterval());
        } 
        protected abstract void Complete(Hero actionTaker);
    }

    public enum RiteType
    {
        OFFERING,
        SACRIFICE
    }
}
