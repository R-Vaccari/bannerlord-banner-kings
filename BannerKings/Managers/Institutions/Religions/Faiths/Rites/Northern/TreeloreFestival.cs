using BannerKings.Managers.Goals.Decisions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Rites.Northern
{
    public class TreeloreFestival : Festival
    {
        public override int DayOfTheSeason => 1;
        public override int SeasonOfTheYear => 0;

        public override void Complete(Hero actionTaker)
        {
            var decision = new OrganizeFeastDecision(Hero.MainHero, Behaviours.Feasts.Feast.FeastType.Treelore);
            decision.ShowInquiry();
        }

        public override void Execute(Hero executor)
        {
            base.Execute(executor);
        }

        public override TextObject GetDescription() => new TextObject("{=c6cFhmH9}The Festival of Pérkos is celebrated on the evennight of spring. To the children of the forest, spring represents the creation of the world of men. It is said no world existed between the heavenly home of the gods and birds and the underworld of snakes and worms, until Pérkos, the Tunder Wielder, struck the bark of the Great Oak and, from it's sap, mankind blossomed.");

        public override TextObject GetName() => new TextObject("{=0cs7e6h1}Festival of Pérkos");

        public override float GetPietyReward()
        {
            return 100;
        }

        public override void SetDialogue()
        {
            MBTextManager.SetTextVariable("CLERGYMAN_RITE_CONFIRM",
                new TextObject("{=!}{?PLAYER.GENDER}My lady{?}My lord{\\?}, a feast to honor Pérkos would be a wise choice. Call in all our siblings of the forest so we may honor the Gods on the sacred canopy!"));
        }
    }
}
