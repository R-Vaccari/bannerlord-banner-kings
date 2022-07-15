using System.Collections.Generic;
using TaleWorlds.Localization;
using TaleWorlds.Library;

namespace BannerKings.Managers.Institutions.Religions.Doctrines
{
    public class Doctrine : BannerKingsObject
    {
        private TextObject effects { get; set; }
        private List<string> incompatibleDoctrines { get; set; }

        public Doctrine(string id, TextObject name, TextObject description,
            TextObject effects, List<string> incompatibleDoctrines) : base(id)
        {
            this.name = name;
            this.description = description;
            this.effects = effects;
            this.incompatibleDoctrines = incompatibleDoctrines;
        }

        public TextObject Effects => effects;
        public MBReadOnlyList<string> IncompatibleDoctrines => incompatibleDoctrines.GetReadOnlyList();
    }
}
