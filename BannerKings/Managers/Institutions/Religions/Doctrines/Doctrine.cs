using System.Collections.Generic;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Doctrines
{
    public class Doctrine : BannerKingsObject
    {
        public Doctrine(string id, TextObject name, TextObject description, TextObject effects, 
            List<Doctrine> incompatibleDoctrines, bool permanent = false) : base(id)
        {
            Initialize(name, description);
            Effects = effects;
            IncompatibleDoctrines = incompatibleDoctrines;
            Permanent = permanent;
        }

        public bool Permanent { get; private set; }
        private List<Doctrine> IncompatibleDoctrines { get; }
        public TextObject Effects { get; private set; }

        public override bool Equals(object obj)
        {
            if (obj is Doctrine)
            {
                return (obj as Doctrine).StringId == StringId;
            }
            return base.Equals(obj);
        }
    }
}