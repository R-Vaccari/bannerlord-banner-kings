using System.Collections.Generic;
using BannerKings.Managers.Education.Languages;
using BannerKings.Managers.Items;
using BannerKings.Managers.Skills;
using BannerKings.Managers.Traits;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
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
        public BookType SkullsEleftheroi { get; } = new BookType("book_SkullsEleftheroi");
        public BookType WestGemynt { get; } = new BookType("book_WestGemynt");
        public BookType IrkBitig { get; } = new BookType("book_IrkBitig");
        public BookType FabulaeAquilae { get; } = new BookType("book_FabulaeAquilae");
        public BookType NakedFingers { get; } = new BookType("book_NakedFingers");
        public BookType GardenArgument { get; } = new BookType("book_GardenArgument");
        public BookType HelgeredKara { get; } = new BookType("book_HelgeredKara");
        public BookType KaisLayala { get; } = new BookType("book_KaisLayala");
        public BookType LoveCastle { get; } = new BookType("book_LoveCastle");
        public BookType GreenKnight { get; } = new BookType("book_GreenKnight");

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
                yield return SkullsEleftheroi;
                yield return WestGemynt;
                yield return IrkBitig;
                yield return FabulaeAquilae;
                yield return NakedFingers;
                yield return GardenArgument;
                yield return HelgeredKara;
                yield return KaisLayala;
                yield return LoveCastle;
                yield return GreenKnight;
                foreach (BookType item in ModAdditions)
                {
                    yield return item;
                }
            }
        }

        public override void Initialize()
        {
            SkullsEleftheroi.Initialize(BKItems.Instance.BookSkullsEleftheroi,
                new TextObject("{=F9468GN7}Subtitled “slaves, servants, soldiers”, this book styles itself as a scientific study of the skull shapes and sizes of the Eleftheroi. Its anonymous author claims that the form of the skulls shows that they are perfect “human livestock”. He argues that runaway slaves have lost all rights and the fact that they return to Imperial lands only to enlist as servants and soldiers is proof that they are less than human. As a biological study, its methods of measuring body parts and collecting data has some limited merits."),
                DefaultLanguages.Instance.Khuzait,
                BookUse.Focusbook,
                DefaultSkills.Medicine);

            WestGemynt.Initialize(BKItems.Instance.BookWestGemynt,
                new TextObject("{=QxZBqSj8}Documented interviews with the last surviving immigrants from the Wilunding homeland. The book is over a century old and has become a major source for both romantic tales and academic debate. Yet, for many of the current Wilunding, a generation was entirely born within Calradia, their homeland has since become a distant memory, a myth."),
                DefaultLanguages.Instance.Vlandic,
                BookUse.Focusbook,
                BKSkills.Instance.Scholarship);

            FabulaeAquilae.Initialize(BKItems.Instance.BookFabulaeAquilae,
                new TextObject("{=UOKVvnfo}Collected by several great theologians, poets and even senator scholars - “tales of the Eagle”, or “the books of tales” as it is colloquially called has become the standard popular versions of old folklore. Nearly all the stories are historical allegories or degraded myths of Calrados and other gods on their shapeshifting adventures as eagle, wolf or dragon. For the Calradoi, these tales are their history, faith and nation."),
                DefaultLanguages.Instance.Calradian,
                BookUse.Focusbook,
                BKSkills.Instance.Theology);

            NakedFingers.Initialize(BKItems.Instance.BookNakedFingers,
                new TextObject("{=XEkfTzb4}Written in prison by a former boss and informant in the Hidden Hand criminal organization, the book claims to expose tactics and methods of the Hidden Hand mafia. It also exposes many of the weaknesses in the imperial justice system."),
                DefaultLanguages.Instance.Calradian,
                BookUse.Focusbook,
                DefaultSkills.Roguery);

            IrkBitig.Initialize(BKItems.Instance.BookIrkBitig,
                new TextObject("{=bbox1mnG}The Book of Visions is a collection of tales from the Devseg peoples, written down in poetic form. The book describes various small stories that serve to depict omens, be them beningn or not. The poems provide invaluable insight into the Devseg culture and theology."),
                DefaultLanguages.Instance.Khuzait,
                BookUse.Focusbook,
                BKSkills.Instance.Theology);

            GardenArgument.Initialize(BKItems.Instance.BookGardenArgument,
                new TextObject("{=J3SKKSfk}A comedic Vlandic story that criticizes the conventions of courtly love. A lover steals into a garden in Sargot, and plies her with lots of witty lines to persuade his lover to submit to his embraces. She shoots down all of his advances one by one, them when he is downcast, she takes him in her arms and tell him that she wanted him all along, except on her terms, not his. “All the silks of Sargot, all the furs of Varcheg...”"),
                DefaultLanguages.Instance.Vlandic,
                BookUse.Skillbook,
                DefaultSkills.Charm,
                BKTraits.Instance.Diligent);

            HelgeredKara.Initialize(BKItems.Instance.BookHelgeredKara,
                new TextObject("{=CN8pxctd}An action story, full of battle. The shieldmaiden Kara chooses the warrior Helgered as her lover, as he is the only man who can defeat her in combat. Her father, who pledged her to another, comes with his sons and his huscarls to kill Helgered. They fight, and Helgered and Kars slaughter the entire host except for Kara's beloved younger -- who, alas, growing up to avenge his father by slaying Helgered. “A light pierced the gloom over Varcheg cliffs...”"),
                DefaultLanguages.Instance.Sturgian,
                BookUse.Skillbook,
                DefaultSkills.OneHanded,
                DefaultTraits.Valor);

            KaisLayala.Initialize(BKItems.Instance.BookKaisLayala,
                new TextObject("{=eQi3froa}A sad story that reminds many of their powerlesness towards the ways of the world. The shepherd boy Kais and the nobleman's daughter Layala love each other, but they can never marry. The poem is Kais's lament as he wanders alone, unwilling to forget his true love, driving himself mad with longing. “The wind that blows the dry steppe dust...”"),
                DefaultLanguages.Instance.Khuzait,
                BookUse.Skillbook,
                DefaultSkills.Charm,
                BKTraits.Instance.Humble);

            LoveCastle.Initialize(BKItems.Instance.BookLoveCastle,
                new TextObject("{=WfjPoxGs}An allegoric Vlandic tale. It describes how a brave but rough warrior wins the heart of his lady by learning the virtues of chivalry, becoming a true and noble knight."),
                DefaultLanguages.Instance.Calradian,
                BookUse.Skillbook,
                BKSkills.Instance.Lordship,
                DefaultTraits.Honor);

            HeatsDesire = new BookType("book_heartsDesire");
            HeatsDesire.Initialize(BKItems.Instance.BookHeartsDesire,
                new TextObject("{=kDM2UWeE}A vlandic tale of love that can be interpreted either erotically or spiritually. The lover realizes the majesty of the divine by gazing upon the body of his beloved. The appeal to love through the naked body is offensive to the highly moralistic, yet compelling to those of more relaxed morals. “You are the first and the last...”"),
                DefaultLanguages.Instance.Vlandic,
                BookUse.Skillbook,
                DefaultSkills.Charm,
                BKTraits.Instance.Seductive);

            Siege = new BookType("book_siege");
            Siege.Initialize(BKItems.Instance.BookSiege,
                new TextObject("{=!}A Calradic, illustrated treatise on siege warfare, also known as poliorcetics. The manuscript goes over all the main topics of sieges: building appropriate engines, bringing men upon the enemy's walls, as well as keeping the enemy off yours. All those in Calradia can attest that none can match the Calradoi in terms of siege warfare."),
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

            GreenKnight.Initialize(BKItems.Instance.BookGreenKnight,
                new TextObject("{=!}A Vlandic tale that explores the themes of chivalry, honor and sacrifice. A knight is made an offer by this mysterious, green knight figure, at the possible cost of his life. Believing the danger to be absurd and the offer purely benefitial to himself, he makes a pledge. A year later, the terms of the offer come due, and the knight's honor and integrity are challenged: will he keep his word and risk his life?"),
                DefaultLanguages.Instance.Vlandic,
                BookUse.Skillbook,
                DefaultSkills.Riding,
                DefaultTraits.Honor);

            Mounted = new BookType("book_riding");
            Mounted.Initialize(BKItems.Instance.BookArtHorsemanship,
                new TextObject("{=!}An ancient Calradic treatise concerning the trainning, care and art of mounting horses. The treatise is the staple manuscript on the long Calradic equestrian tradition, an art developed by its nobility and culminated into the cataphract doctrice."),
                DefaultLanguages.Instance.Calradian,
                BookUse.Focusbook,
                DefaultSkills.Riding);

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