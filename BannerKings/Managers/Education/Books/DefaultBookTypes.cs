using System.Collections.Generic;
using BannerKings.Managers.Education.Languages;
using BannerKings.Managers.Items;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Education.Books
{
    public class DefaultBookTypes : DefaultTypeInitializer<DefaultBookTypes, BookType>
    {
        public BookType HeatsDesire { get; private set; }
        public BookType Siege { get; private set; }
        public BookType Strategikon { get; private set; }
        public BookType Trade { get; private set; }
        public BookType Dictionary { get; private set; }
        public BookType Mounted { get; private set; }
        public BookType Leadership { get; private set; }
        public BookType OneHanded { get; private set; }
        public BookType TwoHanded { get; private set; }
        public BookType Polearm { get; private set; }
        public BookType Crossbow { get; private set; }
        public BookType Bow { get; private set; }
        public BookType Throwing { get; private set; }
        public BookType Medicine { get; private set; }

        public override IEnumerable<BookType> All
        {
            get
            {
                yield return HeatsDesire;
                yield return Siege;
                yield return Strategikon;
                yield return Trade;
                yield return Dictionary;
                yield return Mounted;
                yield return Leadership;
                yield return OneHanded;
                yield return TwoHanded;
                yield return Polearm;
                yield return Crossbow;
                yield return Bow;
                yield return Throwing;
                yield return Medicine;
                foreach (BookType item in ModAdditions)
                {
                    yield return item;
                }
            }
        }

        public override void Initialize()
        {
            HeatsDesire = new BookType("book_heartsDesire");
            HeatsDesire.Initialize(BKItems.Instance.BookHeartsDesire, 
                new TextObject("{=!}"), 
                DefaultLanguages.Instance.Vlandic, BookUse.Skillbook, DefaultSkills.Charm);

            Siege = new BookType("book_siege");
            Siege.Initialize(BKItems.Instance.BookSiege, 
                new TextObject("{=!}"), 
                DefaultLanguages.Instance.Calradian, BookUse.Focusbook, DefaultSkills.Engineering);

            Strategikon = new BookType("book_tactics");
            Strategikon.Initialize(BKItems.Instance.BookStrategikon, 
                new TextObject("{=kGCHAdZG}Assembled for use by the Calradoi dynasty, Strategikon compiles series of treatises on army maneuvers and tatics. The book deals with diverse formations and battle tactics used by opponents of the Empire, since it's inception by Calradios the Great. The book is treated as basic education for military-minded lords in the empire."), 
                DefaultLanguages.Instance.Calradian, BookUse.Focusbook, DefaultSkills.Tactics);

            Trade = new BookType("book_trade");
            Trade.Initialize(BKItems.Instance.BookTrade, 
                new TextObject("{=5qh4CNDP}Written by Dhashwal of the Nahasa, the treatise compiles a series of reflections on trade routes and trading practices the caravaneer deal with in his lifetime. From the Jawwal nomadic traders, through the Imperial bureaucrats up to the sea-faring nothern traders, Dhashwal writes on his experience of mastering the trade through decades of field experience."), 
                DefaultLanguages.Instance.Aseran, BookUse.Focusbook, DefaultSkills.Trade);

            Dictionary = new BookType("book_dictionary");
            Dictionary.Initialize(BKItems.Instance.BookDictionary, 
                new TextObject("{=eLyaVrbS}Dictionarium Calradium is a compendium of basic words and syntax of the languages in the continent. Used to further the former Empire's influence on different cultures, it serves any language by comparing it to Calradian, the Imperial language."), 
                DefaultLanguages.Instance.Calradian, 
                BookUse.Dictionary);

            Mounted = new BookType("book_riding");
            Mounted.Initialize(BKItems.Instance.BookMounted, 
                new TextObject("{=!}"), 
                DefaultLanguages.Instance.Vlandic, BookUse.Focusbook, DefaultSkills.Riding);

            Leadership = new BookType("book_leadership");
            Leadership.Initialize(BKItems.Instance.BookLeadership, 
                new TextObject("{=!}"), 
                DefaultLanguages.Instance.Calradian, BookUse.Focusbook, DefaultSkills.Leadership);

            OneHanded = new BookType("book_oneHanded");
            OneHanded.Initialize(BKItems.Instance.BookOneHanded, 
                new TextObject("{=qJ30OzLa}\"Royal Armouries Ms. I.33\" is the earliest known Calradian combat manual. The text provides guidance on the use of a single-handed swords. The fencing system is based on number of wards which are answered by defensive postures for each one of them. The treatise expound a martial system of defensive and offensive techniques between a master and a pupil, each armed with a sword and a shield, drawn in ink and watercolour and accompanied with text in Calradian, interspersed by Vlandian fencing terms. On the last two pages, the pupil is replaced by a woman called Walpurgis."), 
                DefaultLanguages.Instance.Vlandic, BookUse.Focusbook, DefaultSkills.OneHanded);

            TwoHanded = new BookType("book_twoHanded");
            TwoHanded.Initialize(BKItems.Instance.BookTwoHanded, 
                new TextObject("{=iuQ1bZ8c}The book presents the legend of Llewellyn the Green, mythical ruler of Seonon, who went on a pilgrimage around Llyn Tywal to seek aid from Derwyddon spirits to improve Battanian army. He met Lady of Caldera, who gifted him the Crusher, first two handed sword in Battania. Soon, Llewellyn the Green has found many smithies in his domain, that supplied Battanian archers with two hand swords, creating army of Fian Champions and became first king of the highlands. Fian Champions were a nightmare for Imperial army for centuries, not only because of very well known archery skills, but also because of exceptional swordmanship. Ever since, Llewellyn was since knwon as R�ghf�nnid, king of the Fians."), 
                DefaultLanguages.Instance.Battanian, BookUse.Focusbook, DefaultSkills.TwoHanded);

            Crossbow = new BookType("book_crossbow");
            Crossbow.Initialize(BKItems.Instance.BookCrossbow, 
                new TextObject("{=R5GJQu3Q}Written by Leonhardth of Ocspool, this monumental work is considered a founding text of Vlandia, as it popularized usage of crossbows and siege engines among Vlandian lords during the rule of King Osrac Iron-arm. Thanks to clear descirption of crossbow and balista constuction and simple explanations of physics behind the contraption even lords of a single village were able to supply their peasants with deadly weapons. It is said that Vlandia is build on crossbow and lance and this work is a proof, that Leonhardth of Ocspool deserves at least half of credit for Vlandian indpendence."), 
                DefaultLanguages.Instance.Calradian, BookUse.Focusbook, DefaultSkills.Crossbow);

            Bow = new BookType("book_bow");
            Bow.Initialize
            (
                BKItems.Instance.BookBow,
                new TextObject("{=juJoP0yu}The History of Calradian Archery is a monography of most popular weapon around the continent: bows and arrows. Written by renowned archer, Rovin of Erithrys, the book describes the variety of bows and arrows used across the continent, along with the description of culture favorite variants. Each Calradian culture archery traditions and practical ways to use those in warfare is described in separate chapters. The author also introduces most of bow constructions invented in Calradia, from simplest mountain bows to rather expensive and complicated bows used by the nobility."),
                DefaultLanguages.Instance.Calradian,
                BookUse.Focusbook,
                DefaultSkills.Bow
            );

            Polearm = new BookType("book_polearm");
            Polearm.Initialize
            (
                BKItems.Instance.BookPolearm,
                new TextObject("{=VXKu1ucX}Lycaron debate of 1074 is a record of events that occurred during a tournament of Lycaron in local inn. A group of Khan's Guards, entourage of young Khuzait warrior Monchug (who came to Lycaron to take part in the tournament) has gotten into discussion with Vlandian voulgiers serving in local garrison about whos swingeable polearm armament is better. Soon the present Cataphracts of the same garrison joined the discussion, with intention to prove that long, thrust lances are superior. The debate resulted in 12 deaths, 23 injured and maimed and, as declared unanimously by witnesses, was won by Gareth, a local townfolk, who was passing by the inn and entered carrying his trusty rake, to see what all the ruckus is about."),
                DefaultLanguages.Instance.Calradian,
                BookUse.Focusbook,
                DefaultSkills.Polearm
            );

            Throwing = new BookType("book_throwing");
            Throwing.Initialize
            (
                BKItems.Instance.BookThrowing,
                new TextObject("{=iaGGjVGP}This legend dates to the times of migration of nords from Nordland and Jumne to contemporary sturgian lands and tells story of princess Francesca. Though beautiful, she was so strong, that she was able to cut through enemy shields by throwing an axe at them. The legend tells a story pursuing her dreams despite odds. She wished to sail south, to plentiful sturgian Vanni woods, but her strict Jarl father denied her permission to do so. To prove she is able to make her own decisions she ventured north and managed to kill ice dragon of Jumne with a throw of a single axe, that cut off all three heads of the mighty beast. When she brought the trophy back, her father gave her blessing to cross the Byalic sea. Soon she and her crew set out for the journey and after many adventures they settled near Alebat tribe. She fell in love with young cheftain of that tribe and married him soon after."),
                DefaultLanguages.Instance.Sturgian,
                BookUse.Focusbook,
                DefaultSkills.Throwing
            );

            Medicine = new BookType("book_medicine");
            Medicine.Initialize
            (
                BKItems.Instance.BookMedicine,
                new TextObject("{=7TMGrRS4}Aseran Papyrus is probably oldest known medical written text in Calradia. The book is written as dialogue between legendary Aserai patriarch (Asera) and his physician in which the ruler asks questions and the physician provides the answers. This type of discourse makes possible both a discussion of general ethics and philosophies of life and the inclusion of the prevailing Aserai religious beliefs. In general, the book integrates moral and physical conduct and provides an important relationship between mental and physical states of health. The common theme seems to be moderation in everything, harmony with nature, contentment with oneself, and restraining and reducing one's desires- sensible ways of handling physical and mental stresses and their devastating effects on health and longevity."),
                DefaultLanguages.Instance.Aseran,
                BookUse.Focusbook,
                DefaultSkills.Medicine
            );
        }
    }
}