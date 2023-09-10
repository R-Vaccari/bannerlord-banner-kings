using System.Collections.Generic;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Doctrines
{
    public class DefaultDoctrines : DefaultTypeInitializer<DefaultDoctrines, Doctrine>
    {
        public Doctrine Druidism { get; private set; }
        public Doctrine Animism { get; private set; }
        public Doctrine Legalism { get; private set; }
        public Doctrine CommunalFaith { get; private set; }
        public Doctrine Literalism { get; private set; }
        public Doctrine Pastoralism { get; private set; }
        public Doctrine Pacifism { get; private set; }
        public Doctrine HeathenTax { get; private set; }
        public Doctrine Childbirth { get; private set; }
        public Doctrine Sacrifice { get; private set; }
        public Doctrine OsricsVengeance { get; private set; }
        public Doctrine Warlike { get; private set; }
        public Doctrine Reavers { get; private set; }
        public Doctrine Tolerant { get; private set; }
        public Doctrine Shamanism { get; private set; }
        public Doctrine Astrology { get; private set; }
        public Doctrine Esotericism { get; private set; }
        public Doctrine RenovatioImperi { get; private set; }

        public override IEnumerable<Doctrine> All
        {
            get
            {
                yield return Druidism;
                yield return Animism;
                yield return Legalism;
                yield return CommunalFaith;
                yield return Literalism;
                yield return Pastoralism;
                yield return Pacifism;
                yield return HeathenTax;
                yield return Childbirth;
                yield return Sacrifice;
                yield return OsricsVengeance;
                yield return Reavers;
                yield return Tolerant;
                yield return Shamanism;
                yield return Astrology;
                yield return Esotericism;
                yield return RenovatioImperi;
                foreach (Doctrine item in ModAdditions)
                {
                    yield return item;
                }
            }
        }

        public override void Initialize()
        {
            RenovatioImperi = new Doctrine("RenovatioImperi",
                new TextObject("{=!}Renovatio Imperi"),
                new TextObject("{=!}Peace in Calradia can only exist with an united Empire. A kingdom under the teachings of Heaven. The faithful should cast aside their ambitions and work together for the benefit of the gods."),
                new TextObject("{=!}On Imperial fiefs, showing mercy post siege, gain relations with all notables and lose no fief stability"),
                new List<Doctrine>()
                {
                },
                false);

            Esotericism = new Doctrine("Esotericism",
               new TextObject("{=!}Esotericism"),
               new TextObject("{=!}Knowledge and faith are less divided than most think. The cosmos is littered with mysteryes, and to seek their answers is to seek to reach the gods themselves."),
               new TextObject("{=!}Wisdom yields piety\nFinishing education projects yields piety"),
               new List<Doctrine>()
               {
               },
               false);

            Astrology = new Doctrine("Astrology",
               new TextObject("{=!}Astrology"),
               new TextObject("{=!}The study of the stars is the study of the divine itself. Much like the Sun blesses the land with fertility, the stars bless man with wisdom."),
               new TextObject("{=!}Cultural innovations develop faster (when Cultural Head)\nShip travels are faster"),
               new List<Doctrine>()
               {
               },
               false);

            Tolerant = new Doctrine("Tolerant",
               new TextObject("{=!}Tolerant"),
               new TextObject("{=!}A tolerant faith considers all faiths to be different interpretations of the Truth, as different paths to the real god(s). Tolerant faiths do not have hostile opinions toward any other faith, facilitating marraiges and decreasing tensions in multiple faith fiefs."),
               new TextObject("{=!}Every other faith is consedered Tolerated"),
               new List<Doctrine>()
               {
               },
               false);

            Shamanism = new Doctrine("Shamanism",
               new TextObject("{=!}Shamanism"),
               new TextObject("{=!}Shamans are the intermediaries between manking and the spiritual world. They do not adhere to an organized structure, instead relying on oral tradition and local custom. Shamans often live close to nature, where the spirits lie, and perform rituals to access the spiritual world to communicate with them, acquiring knowledge to guide their people in the mundane plane."),
               new TextObject("{=!}Preachers are able to heal diseases and curses"),
               new List<Doctrine>()
               {
                   Druidism
               },
               false);

            OsricsVengeance = new Doctrine("osrics_vengeance", 
                new TextObject("{=!}Osric's Vengeance"),
                new TextObject("{=!}Osric fulfilled his vengeace against the Calradic gods when the captured Pravend, taking away their power and providing bountiful land to his people. As such, the Wilunding should follow in his path of occupying their enemies."),
                new TextObject("{=!}Occupying fiefs yields significant piety"),
                new List<Doctrine>()
                {
                    Warlike
                },
                true);

            Reavers = new Doctrine("Reavers",
                new TextObject("{=!}Reavers"),
                new TextObject("{=!}Raiding and pillaging is understood as a pious practice. This faith"),
                new TextObject("{=!}Piety gain raiding and fief pillaging of different cultures"),
                new List<Doctrine>()
                {
                    OsricsVengeance
                });

            Warlike = new Doctrine("osrics_vengeance",
                new TextObject("{=!}Warlike"),
                new TextObject("{=!}This faith understands combat as a pious practice."),
                new TextObject("{=!}Piety gain as battle reward"),
                new List<Doctrine>()
                {
                    OsricsVengeance
                });

            Druidism = new Doctrine("druidism", 
                new TextObject("{=9kA2mxU8}Druidism"),
                new TextObject("{=5pkaQ17t}Clergy is considered part of the Druid caste, who represent the spiritual power, but are also involved in political, material affairs. In a way, druids are a form of lesser nobility and cannot be excluded from political affairs."),
                new TextObject("{=9TwjtYhb}Preachers provide noble troops\nNo religious council advisor causes daily influence loss"),
                new List<Doctrine>());

            Animism = new Doctrine("animism", 
                new TextObject("{=OZqf2Rab}Animism"),
                new TextObject("{=GxdpgOvT}Spirits inhabit everywhere in this world, hidden in plain sight. Under the earth, flowing along rivers or bound to animals or trees, spirits can be anywhere. It is the duty of the faithful to not harm the balance of the material world with the spiritual world, which are often one and the same."),
                new TextObject("{=FRAS5TAC}Woodland acreage provides piety\nReduced baseline fervor"),
                new List<Doctrine>());

            CommunalFaith = new Doctrine("communal_faith", new TextObject("{=Pj5aVLht}Communal Faith"),
                new TextObject("{=!}"),
                new TextObject("{=!}"),
                new List<Doctrine>());

            Legalism = new Doctrine("legalism", new TextObject("{=A7pNHzFo}Legalism"),
                new TextObject("{=OKTdXTNh}Without laws, man is but beast. The wisdom of previous generations is preserved through law, which must be followed to the letter."),
                new TextObject("{=hhwx3SVn}Heathens can not fill council positions\n+0.5 to vassal limit for each personal virtue"),
                new List<Doctrine>());

            HeathenTax = new Doctrine("heathen_tax", new TextObject("{=opRTVAF6}Heathen Taxation"),
                new TextObject("{=QvoijhLH}Non believers are tolerated, but only through their financial subjugation. They are not trusted for service."),
                new TextObject("{=6tDiRTo6}Heathen population yields extra tax\nReduced militarism in predominantly heathen settlements"),
                new List<Doctrine>());

            Pastoralism = new Doctrine("pastorialism", new TextObject("{=XX2xYuzs}Pastorialism"),
                new TextObject("{=QvoijhLH}Non believers are tolerated, but only through their financial subjugation. They are not trusted for service."),
                new TextObject("{=nBidhxSN}Pasture and farmland acreage are more productive\nReduced drafting efficiency"),
                new List<Doctrine>());

            Childbirth = new Doctrine("childbirth", new TextObject("{=DXNnpp89}Honored Childbirth"),
                new TextObject("{=Xp0KPwWU}Birth of children and the spread of the family are seen as a blessing. The more children we bear, more will defend and honor our ways of life in the future."),
                new TextObject("{=BV57JShf}Clan renown is increased every time a child is born\nIncreased fertility"),
                new List<Doctrine>());

            Pacifism = new Doctrine("pacifism", new TextObject("{=555i9sjK}Pacifism"),
                new TextObject("{=a0e4FBRs}Peace is our most valued treasure. Unlike warmongering beasts, we cherish thriving through cooperation and hard work."),
                new TextObject("{=Z2at3qd1}Increased stability in settlements\nPiety loss after battles"),
                new List<Doctrine>());

            Literalism = new Doctrine("literalism", new TextObject("{=qPcN1VEv}Literalism"),
                new TextObject("{=0srttZAq}It is only through our holy texts that we can uphold our values and faith."),
                new TextObject("{=FrtZRbPA}High scholarship provides piety, illiteracy reduces piety\nAdds Court Philosopher to royal councils"),
                new List<Doctrine>());

            Sacrifice = new Doctrine("sacrifice", new TextObject("{=DOiR2ymQ}Human Sacrifices"),
                new TextObject("{=cE3uPWSF}Worthy opponents are deserving of a better death than commoners in the battlefield. We can prove our devotion by ritually sacrificing them."),
                new TextObject("{=wtEXUh89}Allows the Human Sacrifice rite"),
                new List<Doctrine>());
        }
    }
}