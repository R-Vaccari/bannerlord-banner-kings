using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using System.Linq;

namespace BannerKings.Managers.Education.Languages
{
    public class DefaultLanguages : DefaultTypeInitializer<DefaultLanguages>
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
            battanian = new Language(new TextObject("{=ZvoyX6DX}Battanian"), new TextObject(), cultures.First(x => x.StringId == "battania"));
            sturgian = new Language(new TextObject("{=OxiaKT0G}Sturgian"), new TextObject(), cultures.First(x => x.StringId == "sturgia"));
            khuzait = new Language(new TextObject("{=sZLd6VHi}Khuzait"), new TextObject(), cultures.First(x => x.StringId == "khuzait"));
            vlandic = new Language(new TextObject("{=!}Vlandic"), new TextObject(), cultures.First(x => x.StringId == "vlandia"));
            calradian = new Language(new TextObject("{=jKviMpbP}Calradian"), new TextObject(), cultures.First(x => x.StringId == "empire"));
            aseran = new Language(new TextObject("{=!}Aseran"), new TextObject(), cultures.First(x => x.StringId == "aserai"));
            vakken = new Language(new TextObject("{=bHh03ob0}Vakken"), new TextObject(), cultures.First(x => x.StringId == "vakken"));

            battanian.Initialize(new Dictionary<Language, float>() { { vakken, 0.15f }, { calradian, 0.1f } });
            sturgian.Initialize(new Dictionary<Language, float>() { { vakken, 0.1f } });
            vlandic.Initialize(new Dictionary<Language, float>() { { calradian, 0.15f } });
            calradian.Initialize(new Dictionary<Language, float>() { { vlandic, 0.1f }, { battanian, 0.1f } });
            aseran.Initialize(new Dictionary<Language, float>());
            khuzait.Initialize(new Dictionary<Language, float>());
            vakken.Initialize(new Dictionary<Language, float>() { { battanian, 0.15f }, { sturgian, 0.1f } });
        }

        public IEnumerable<Language> All
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
