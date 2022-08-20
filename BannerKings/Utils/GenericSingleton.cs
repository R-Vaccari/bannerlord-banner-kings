using System;

namespace BannerKings.Utils
{
    public class GenericSingleton<T> : IDisposable where T : class, new()
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new T();
                }

                return _instance;
            }
        }

        public virtual void Dispose()
        {
            _instance = null;
        }
    }
}