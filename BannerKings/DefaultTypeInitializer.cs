
namespace BannerKings
{
    public abstract class DefaultTypeInitializer<T> where T : new()
    {
        public abstract void Initialize();
        public static T Instance => ConfigHolder.CONFIG;
        internal struct ConfigHolder
        {
            public static T CONFIG = new T();
        }
    }
}
