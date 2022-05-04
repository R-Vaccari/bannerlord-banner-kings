using System.Collections.Generic;
using TaleWorlds.Localization;
using TaleWorlds.Library;

namespace BannerKings.Managers.Institutions.Religions.Doctrines
{
    public class Doctrine
    {
        private TextObject name { get; set; }
        private TextObject description { get; set; }
        private TextObject effects { get; set; }
        private string id { get; set; }
        private List<string> incompatibleDoctrines { get; set; }

        public Doctrine(string id, TextObject name, TextObject description,
            TextObject effects, List<string> incompatibleDoctrines)
        {
            this.id = id;
            this.name = name;
            this.description = description;
            this.effects = effects;
            this.incompatibleDoctrines = incompatibleDoctrines;
        }

        public string Id => id;
        public TextObject Name => name;
        public TextObject Description => description;
        public TextObject Effects => effects;
        public MBReadOnlyList<string> IncompatibleDoctrines => incompatibleDoctrines.GetReadOnlyList();
    }
}
