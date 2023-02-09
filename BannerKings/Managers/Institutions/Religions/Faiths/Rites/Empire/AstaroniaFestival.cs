using BannerKings.Managers.Goals.Decisions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Rites.Empire
{
    public class AstaroniaFestival : Festival
    {
        public override int DayOfTheSeason => 1;

        public override int SeasonOfTheYear => 2;

        public override void Complete(Hero actionTaker)
        {
            var decision = new OrganizeFeastDecision(Hero.MainHero, Behaviours.Feasts.Feast.FeastType.Astaronia);
            decision.ShowInquiry();
        }

        public override TextObject GetDescription() => new TextObject("{=pQmosHxT}Astaronia, bride of Iovis and mother of Calradia. Though the Imperials are not native to the continent, that is how commonfolk think of her - the mother of all, regardless of 'native' faiths to the continent, and the Imperials as their legitimate children. On the equinox of Autumnus, she is celebrated as a symbol of motherhood, fertility and prosperity.");

        public override TextObject GetName() => new TextObject("{=oL7Lkkj4}Dies Astaroniae");

        public override void SetDialogue()
        {
            MBTextManager.SetTextVariable("CLERGYMAN_RITE_CONFIRM",
                new TextObject("{=ZNU4rSxp}{?PLAYER.GENDER}My lady{?}My lord{\\?}, a feast for the Goddess would be a most pious choice. Let her ease the coming autumn and winter and bring us a bountiful spring."));
        }
    }
}
