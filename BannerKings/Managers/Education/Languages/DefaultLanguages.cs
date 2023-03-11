using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Education.Languages
{
    public class DefaultLanguages : DefaultTypeInitializer<DefaultLanguages, Language>
    {
        public Language Battanian { get; private set; }
        public Language Vlandic { get; private set; }
        public Language Calradian { get; private set; }
        public Language Sturgian { get; private set; }
        public Language Aseran { get; private set; }
        public Language Khuzait { get; private set; }
        public Language Vakken { get; private set; }

        public override IEnumerable<Language> All
        {
            get
            {
                yield return Battanian;
                yield return Vlandic;
                yield return Calradian;
                yield return Sturgian;
                yield return Aseran;
                yield return Khuzait;
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
            Battanian = new Language("language_battanian");
            Sturgian = new Language("language_sturgian");
            Khuzait = new Language("language_khuzait");
            Vlandic = new Language("language_vlandic");
            Calradian = new Language("language_calradian");
            Aseran = new Language("language_aseran");
            Vakken = new Language("language_vakken");

            Battanian.Initialize(new TextObject("{=tRp08jyH}Battanian"), 
                new TextObject("{=hNK8XTow}The language of the Battanian peoples has long since been a cultural divide between Battanians and other kingdoms in the continent. Given that Palaic, its sister language and culture, has mostly been erased by Imperials, the Battanian language is almost completely unintelligible to all other cultures present in Calradia. However, with the increased presence of Calradian as the lingua franca of trade, Battanians have slowly adopted some Imperial vocabulary. The language is still somewhat similar to Vakken, another natural culture of the continent, though recently the Vakken culture has mostly been replaced by Sturgians."),
                cultures.First(x => x.StringId == "battania"), GetIntelligibles(Battanian));
            Sturgian.Initialize(new TextObject("{=VtNL32g2}Sturgian"), 
                new TextObject("{=QT3S6XkQ}Natural to the northern ends of Calradia, the Sturgians have an ancient linguistic tradition. Though this tradition has been recently shaken by the large mixing of nords, the Sturgian culture itself is native to the continent, as well as it's sister culture, Vakken, with which the Sturgian language still finds similarities."),
                cultures.First(x => x.StringId == "sturgia"), GetIntelligibles(Sturgian));
            Khuzait.Initialize(new TextObject("{=ZdFBNgoJ}Khuzait"), 
                new TextObject("{=rYVgj513}The langauge of the steppe is often described by foreigners as curt, but effective. Due to the near absence of scholarly research in the Khuzait culture, it often lacks terms for technical, or more abstract concepts, and as such it is certain those will be adopted from the Imperial language."),
                cultures.First(x => x.StringId == "khuzait"), GetIntelligibles(Khuzait));
            Vlandic.Initialize(new TextObject("{=6FGQ31TM}Vlandic"), 
                new TextObject("{=!}"),
                cultures.First(x => x.StringId == "vlandia"), GetIntelligibles(Vlandic));
            Calradian.Initialize(new TextObject("{=NWqkTdMt}Calradian"), 
                new TextObject("{=GmqBFSgN}The Imperial language of the Calradian empire. Though scholars have made efforts into keeping the language pure, centuries of contact with local cultures have made Calradian adopt small quantities of local vocabularies. Being a language of prestige, Calradian vocabulary are also often adopted by foreign languages, due to it's usefulness in the continent as a Lingua Franca, often used by traders, nobles during their education or peasants looking for a better life within the Empire."),
                cultures.First(x => x.StringId == "empire"), 
                GetIntelligibles(Calradian));
            Aseran.Initialize(new TextObject("{=UAeorLSO}Aseran"), 
                new TextObject("{=gM4s1KQf}Although the Aserai peoples speak a multitude of dialects, scattered across the oases, springs and coasts of the Nahasa, a distinct tradition of literalism and poety has established a common variation that has been embraced by the higher Aserai classes."), 
                cultures.First(x => x.StringId == "aserai"),
                GetIntelligibles(Aseran));
            Vakken.Initialize(new TextObject("{=brxz2SmN}Vakken"), 
                new TextObject("{=bXUwFrCF}The Vakken, sometimes called 'children of the forest', are a group native to northern Calradia. Vakken and Sturgian cultures have ancient connections, as both have lived and traded for centuries before the Imperials arrived in the continent. However, with the prevailment of the Sturgia kingdom and culture, the Vakken tongue and tradition is being increasingly forgotten about in the north."),
                cultures.First(x => x.StringId == "vakken"), GetIntelligibles(Vakken));
        }

        public Dictionary<Language, float> GetIntelligibles(Language language)
        {
            return language.StringId switch
            {
                "language_battanian" => new Dictionary<Language, float> {{Vakken, 0.15f}, {Calradian, 0.1f}},
                "language_vlandic" => new Dictionary<Language, float> {{Calradian, 0.15f}},
                "language_sturgian" => new Dictionary<Language, float> {{Vakken, 0.1f}},
                "language_calradian" => new Dictionary<Language, float> {{Vlandic, 0.1f}, {Battanian, 0.1f}},
                "language_vakken" => new Dictionary<Language, float> {{Battanian, 0.15f}, {Sturgian, 0.1f}},
                _ => new Dictionary<Language, float>()
            };
        }
    }
}