using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Education.Languages
{
    public class DefaultLanguages : DefaultTypeInitializer<DefaultLanguages, Language>
    {
        public Language AngloNorman { get; private set; }
        public Language French { get; private set; }
        public Language Sicilian { get; private set; }
        public Language Swedish { get; private set; }
        public Language Arabic { get; private set; }
        public Language Polish { get; private set; }
        public Language Latin { get; private set; }
        public Language German { get; private set; }
        public Language Kipchak { get; private set; }
        public Language Castillian { get; private set; }
        public Language English { get; private set; }
        public Language Vakken { get; private set; }

        public override IEnumerable<Language> All
        {
            get
            {
                yield return AngloNorman;
                yield return French;
                yield return Sicilian;
                yield return Swedish;
                yield return Arabic;
                yield return Polish;
                yield return Vakken;
                foreach (Language item in ModAdditions)
                {
                    yield return item;
                }
            }
        }

        public override void Initialize()
        {
            var cultures = Game.Current.ObjectManager.GetObjectTypeList<CultureObject>();
            AngloNorman = new Language("language_battanian");
            Swedish = new Language("language_sturgian");
            Polish = new Language("language_khuzait");
            French = new Language("language_vlandic");
            Sicilian = new Language("language_calradian");
            Arabic = new Language("language_aseran");
            Vakken = new Language("language_vakken");

            Latin.Initialize(new TextObject("{=!}Medieval Latin"), 
                new TextObject("{=!}Medieval Latin is the current form of literary Latin used in Western Europe. It serves as the primary written language, though local languages are also written to varying degrees. Latin functions as the main medium of scholarly exchange, as the liturgical language of the Church, and as the working language of science, literature, law, and administration.  Thus, it is an invaluable one for any aspiring scholar to learn."),
                null, 
                GetIntelligibles(Latin));

            English.Initialize(new TextObject("{=!}Middle English"),
                new TextObject("{=!}The natural evolution of Old English, heavily influenced by Old French as a result of the Norman Conquest in 1066. This is the language of the common folk of England."),
                cultures.First(x => x.StringId == "battania"),
                GetIntelligibles(English));

            German.Initialize(new TextObject("{=!}Middle High German"),
                new TextObject("{=!}Middle High German or Mittelhochdeutsch is the current German spoken across the German people of the Holy Roman Empire. It evolved from the older Old High German. While texts in this language are written in Latin script, there is no real standardised spelling."),
                cultures.First(x => x.StringId == "germanic"),
                GetIntelligibles(German));

            Kipchak.Initialize(new TextObject("{=!}Mamluk-Kipchak"),
                new TextObject("{=!}Mamluk-Kipchak is a Kipchak language that is spoken in Egypt and Syria by the Mamluk Sultanate ruling elite, since most of the Mamluk rulers are monolingual Turkic speakers. The language is also used as literary language and several Arabic and Persian works have been translated to Kipchak by Mamluks. It is written in Arabic script."),
                cultures.First(x => x.StringId == "germanic"),
                GetIntelligibles(German));

            AngloNorman.Initialize(new TextObject("{=!}Anglo Norman"),
                new TextObject("{=!}The language of the Anglo Normans can be considered a dialect of Old French, Specifically, it is the amalgam of a range of northern Old French dialects that was spoken by William the Conqueror and his nobles and is today mainly used in the noble courts."),
                cultures.First(x => x.StringId == "battania"),
                GetIntelligibles(AngloNorman));

            Swedish.Initialize(new TextObject("{=!}Old Swedish"), 
                new TextObject("{=!}The writing of the Westrogothic law marked the beginning of Early Old Swedish, which developed from Old East Norse, the eastern dialect of Old Norse. "),
                cultures.First(x => x.StringId == "sturgia"), 
                GetIntelligibles(Swedish));
            Polish.Initialize(new TextObject("{=!}Old Polish"), 
                new TextObject("{=!}The language mainly spoken in the territories of the Polish principalities under the Piasts. It is however purely the vernacular language, as the written state language is Latin."),
                cultures.First(x => x.StringId == "khuzait"), 
                GetIntelligibles(Polish));
            French.Initialize(new TextObject("{=!}Old French"), 
                new TextObject("{=!}Old French is the language spoken in most of  the French kingdom. Rather than a unified language, Old French is a linkage of Romance dialects, mutually intelligible yet diverse."),
                cultures.First(x => x.StringId == "vlandia"), 
                GetIntelligibles(French));
            Sicilian.Initialize(new TextObject("{=!}Sicilian"), 
                new TextObject("{=!}Because Sicily is the largest island in the Mediterranean Sea and many peoples such as Carthaginians, Arabs, Normans, Swabians have passed through it, Sicilian displays a rich and varied influence from several languages in its lexical stock and grammar and is considered distinct enough from Italian to be a separate language."),
                cultures.First(x => x.StringId == "empire"), 
                GetIntelligibles(Sicilian));
            Arabic.Initialize(new TextObject("{=!}Classical Arabic"), 
                new TextObject("{=!}The Arabic of the current Islamic world is also known as Quranic Arabic and is the standardized literary form of Arabic used since the 7th century, most notably in Umayyad and Abbasid literary texts such as poetry, elevated prose and oratory, and is also the liturgical language of Islam."),
                cultures.First(x => x.StringId == "aserai"),
                GetIntelligibles(Arabic));
        }

        public Dictionary<Language, float> GetIntelligibles(Language language)
        {
            return language.StringId switch
            {
                "language_battanian" => new Dictionary<Language, float> {{Vakken, 0.15f}, {Sicilian, 0.1f}},
                "language_vlandic" => new Dictionary<Language, float> {{Sicilian, 0.15f}},
                "language_sturgian" => new Dictionary<Language, float> {{Vakken, 0.1f}},
                "language_calradian" => new Dictionary<Language, float> {{French, 0.1f}, {AngloNorman, 0.1f}},
                "language_vakken" => new Dictionary<Language, float> {{AngloNorman, 0.15f}, {Swedish, 0.1f}},
                _ => new Dictionary<Language, float>()
            };
        }
    }
}