using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using System.Linq;

namespace BannerKings.Managers.Education.Languages
{
    public class DefaultLanguages : DefaultTypeInitializer<DefaultLanguages, Language>
    {
        private Language battanian, vlandic, calradian, sturgian, aseran, khuzait, vakken;
        public Language Battanian => battanian;
        public Language Vlandic => vlandic;
        public Language Calradian => calradian;
        public Language Sturgian => sturgian;
        public Language Aseran => aseran;
        public Language Khuzait => khuzait;
        public Language Vakken => vakken;
        public override void Initialize()
        {
            MBReadOnlyList<CultureObject> cultures = Game.Current.ObjectManager.GetObjectTypeList<CultureObject>();
            battanian = new Language("language_battanian");
            sturgian = new Language("language_sturgian");
            khuzait = new Language("language_khuzait");
            vlandic = new Language("language_vlandic");
            calradian = new Language("language_calradian");
            aseran = new Language("language_aseran");
            vakken = new Language("language_vakken");

            battanian.Initialize(new TextObject("{=ZvoyX6DX}Battanian"), new TextObject(), cultures.First(x => x.StringId == "battania"), GetIntelligibles(battanian));
            sturgian.Initialize(new TextObject("{=OxiaKT0G}Sturgian"), new TextObject(), cultures.First(x => x.StringId == "sturgia"), GetIntelligibles(sturgian));
            khuzait.Initialize(new TextObject("{=sZLd6VHi}Khuzait"), new TextObject(), cultures.First(x => x.StringId == "khuzait"), GetIntelligibles(khuzait));
            vlandic.Initialize(new TextObject("{=!}Vlandic"), new TextObject(), cultures.First(x => x.StringId == "vlandia"), GetIntelligibles(vlandic));
            calradian.Initialize(new TextObject("{=jKviMpbP}Calradian"), new TextObject(), cultures.First(x => x.StringId == "empire"), GetIntelligibles(calradian));
            aseran.Initialize(new TextObject("{=!}Aseran"), new TextObject(), cultures.First(x => x.StringId == "aserai"), GetIntelligibles(aseran));
            vakken.Initialize(new TextObject("{=bHh03ob0}Vakken"), new TextObject(), cultures.First(x => x.StringId == "vakken"), GetIntelligibles(vakken));
        }

        public Dictionary<Language, float> GetIntelligibles(Language language)
        {
            switch (language.StringId)
            {
                case "language_battanian":
                    return new Dictionary<Language, float>() { { vakken, 0.15f }, { calradian, 0.1f } };
                case "language_vlandic":
                    return new Dictionary<Language, float>() { { calradian, 0.15f } };
                case "language_sturgian":
                    return new Dictionary<Language, float>() { { vakken, 0.1f } };
                case "language_calradian":
                    return new Dictionary<Language, float>() { { vlandic, 0.1f }, { battanian, 0.1f } };
                case "language_vakken":
                    return new Dictionary<Language, float>() { { battanian, 0.15f }, { sturgian, 0.1f } };
                default:
                    return new Dictionary<Language, float>();
                    
            }
        }

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
            }
        }
    }
}
