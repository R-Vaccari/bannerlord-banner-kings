using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions
{
    public class DefaultDivinities
    {
        private Divinity asera, amraMain, amraSecondary1, amraSecondary2;

        public static DefaultDivinities Instance => ConfigHolder.CONFIG;

        internal struct ConfigHolder
        {
            public static DefaultDivinities CONFIG = new DefaultDivinities();
        }

        public void Initialize()
        {
            asera = new Divinity(new TextObject("{=!}Asera"), 
                new TextObject("{=!}The god of the Aserai."), 
                new TextObject());

            amraMain = new Divinity(new TextObject("{=!}Sluagh Aos’An"), 
                new TextObject("{=!}Constituting the major heavenly divine of the Battanians are those known as the Slaugh Aos’An - the Host of Noble Folk who reign between darkened clouds and watch over humanity with starlight torches. Seldom petitioned, as they are viewed as capricious entities; the Slaugh Aos’An are said to visit Battania during the changing of the seasons and to witness the birth of those ordained by fate to bring about weal and doom to the land. To make an oath under the auspices of the Slaugh Aos’An is to be bound to the letter or the spirit of one’s words; never more and never both. To break such an oath is to invite all of fate to conspire towards your end, and to know no peace in Heaven nor Hell."),
                new TextObject());

            amraSecondary1 = new Divinity(new TextObject("{=!}Na Sidhfir"),
                new TextObject("{=!}Those deemed to have won the favor of the Slaugh Aos’An and the love of the Battanian people for more than a generation may be vaunted into the ranks of the Na Sidhfir - the Immortal Men of the Woods. Occupying a position equally heroic and tragic, the grand figures of the Na Sidhfir are claimed to be tireless and exhausted entities - unable to rest so long as they are remembered, but too self-absorbed to allow their songs to go unsung. Derwyddon practitioners claim the Na Sidhfir possess the bodies of Wolfskins, allowing them to rest and ravage away from the heavenly realms."),
                new TextObject());

            amraSecondary2 = new Divinity(new TextObject("{=!}Dymhna Sidset"),
                new TextObject("{=!} Patient devils, the Dymhna Sidset are the stuff of children’s parables and ill told tales around campfires. They are the spittal on a rabid dog’s lips, the rage of a mother bear seeking a misplaced cub, the cold biting steel that strikes only in betrayal. Though the attempted Calradification of the Uchalion Plateau could not purge this pagan belief set entirely, it did compartmentalize and mangle its body of rituals. Giants, ghosts, and many an unseen shade were changed from beings of tale and legend to “patient devils” by the whims of the Empire. In recent years, some have sought to venerate the Dymhna Sidset; viewing them instead as aspects of rebellion and irredentism."),
                new TextObject());
        }

        public Divinity Asera => asera;
    }
}
