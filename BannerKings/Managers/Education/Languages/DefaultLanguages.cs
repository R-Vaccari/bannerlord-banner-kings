using System.Collections.Generic;
using TaleWorlds.Localization;

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
            battanian = new Language(new TextObject("{=ZvoyX6DX}Battanian"), new TextObject(), null);
            sturgian = new Language(new TextObject("{=OxiaKT0G}Sturgian"), new TextObject(), null);
            khuzait = new Language(new TextObject("{=sZLd6VHi}Khuzait"), new TextObject(), null);
            vlandic = new Language(new TextObject("{=!}Vlandic"), new TextObject(), null);
            calradian = new Language(new TextObject("{=jKviMpbP}Calradian"), new TextObject(), null);
            aseran = new Language(new TextObject("{=!}Aseran"), new TextObject(), null);
            vakken = new Language(new TextObject("{=bHh03ob0}Vakken"), new TextObject(), null);

            battanian.Initialize(new Dictionary<Language, float>() { { vakken, 0.15f }, { calradian, 0.1f } });
            sturgian.Initialize(new Dictionary<Language, float>() { { vakken, 0.1f } });
            vlandic.Initialize(new Dictionary<Language, float>() { { calradian, 0.15f } });
            calradian.Initialize(new Dictionary<Language, float>() { { vlandic, 0.1f }, { battanian, 0.1f } });
            aseran.Initialize(new Dictionary<Language, float>());
            khuzait.Initialize(new Dictionary<Language, float>());
            vakken.Initialize(new Dictionary<Language, float>() { { battanian, 0.15f }, { sturgian, 0.1f } });
        }
    }
}
