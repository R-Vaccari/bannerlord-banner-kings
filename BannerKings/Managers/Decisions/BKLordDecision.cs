using TaleWorlds.CampaignSystem;

namespace BannerKings.Managers.Decisions
{
    public abstract class BKLordDecision : BKDecision<Hero>
    {
        protected BKLordDecision(Hero hero, bool enabled) : base(hero, enabled)
        {

        }
    }
}