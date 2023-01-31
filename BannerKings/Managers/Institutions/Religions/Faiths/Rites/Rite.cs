using TaleWorlds.CampaignSystem;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Rites
{
    public abstract class Rite
    {
        public abstract float GetPietyReward();
        public abstract bool MeetsCondition(Hero hero);
    }
}
