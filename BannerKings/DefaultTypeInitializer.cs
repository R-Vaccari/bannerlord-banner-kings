using System.Collections.Generic;
using System.Linq;
using TaleWorlds.ObjectSystem;

namespace BannerKings;

public abstract class DefaultTypeInitializer<T, X>
    where T : new()
    where X : MBObjectBase
{
    public static T Instance => ConfigHolder.CONFIG;

    public abstract IEnumerable<X> All { get; }
    public abstract void Initialize();

    public X GetById(X input)
    {
        if (input != null)
        {
            return All.FirstOrDefault(x => x.StringId == input.StringId);
        }

        return null;
    }

    public X GetById(string input)
    {
        if (input != null)
        {
            return All.FirstOrDefault(x => x.StringId == input);
        }

        return null;
    }

    internal struct ConfigHolder
    {
        public static T CONFIG = new();
    }
}