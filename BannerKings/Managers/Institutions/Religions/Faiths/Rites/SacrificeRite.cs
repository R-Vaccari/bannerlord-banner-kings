using TaleWorlds.CampaignSystem;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Rites
{
    public abstract class SacrificeRite : ContextualRite
    {
        public abstract bool CanHeroBeSacrificed(Hero executor, Hero hero);
        public abstract int CalculateSacrificeReward(Hero sacrifice);
    }
}
