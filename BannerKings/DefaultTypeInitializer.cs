using System.Collections.Generic;
using System.Linq;
using TaleWorlds.ObjectSystem;

namespace BannerKings
{
    public abstract class DefaultTypeInitializer<T, X> 
        where T : new()
        where X : MBObjectBase
    {
        public abstract void Initialize();
        public static T Instance => ConfigHolder.CONFIG;
        internal struct ConfigHolder
        {
            public static T CONFIG = new T();
        }

        public abstract IEnumerable<X> All { get; }

        public X GetById(X input) => All.FirstOrDefault(x => x.StringId == input.StringId);
    }
}
