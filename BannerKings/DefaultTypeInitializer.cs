using System.Collections.Generic;
using System.Linq;
using TaleWorlds.ObjectSystem;

namespace BannerKings
{
    public abstract class DefaultTypeInitializer<TDefaultTypeInitializer, TMBObjectBase> where TDefaultTypeInitializer : new() where TMBObjectBase : MBObjectBase
    {
        public static TDefaultTypeInitializer Instance => ConfigHolder.CONFIG;

        public abstract IEnumerable<TMBObjectBase> All { get; }

        public abstract void Initialize();

        public TMBObjectBase GetById(TMBObjectBase input)
        {
            return input != null 
                ? All.FirstOrDefault(x => x.StringId == input.StringId) 
                : null;
        }

        public TMBObjectBase GetById(string input)
        {
            return input != null 
                ? All.FirstOrDefault(x => x.StringId == input) 
                : null;
        }

        private struct ConfigHolder
        {
            public static TDefaultTypeInitializer CONFIG = new();
        }
    }
}