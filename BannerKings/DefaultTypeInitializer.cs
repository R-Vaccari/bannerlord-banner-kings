using System.Collections.Generic;
using System.Linq;
using TaleWorlds.ObjectSystem;

namespace BannerKings
{
    public abstract class DefaultTypeInitializer<TDefaultTypeInitializer, TMBObjectBase> : ITypeInitializer where TDefaultTypeInitializer : new() where TMBObjectBase : MBObjectBase
    {
        public static TDefaultTypeInitializer Instance => ConfigHolder.CONFIG;

        public abstract IEnumerable<TMBObjectBase> All { get; }

        protected List<TMBObjectBase> ModAdditions { get; } = new List<TMBObjectBase>();

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

        public void AddObject(TMBObjectBase toAdd)
        {
            if (toAdd != null)
            {
                ModAdditions.Add(toAdd);
            }
        }

        private struct ConfigHolder
        {
            public static TDefaultTypeInitializer CONFIG = new();
        }
    }
}