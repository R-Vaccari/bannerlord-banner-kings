using System;

namespace BannerKings
{
    internal class BannerKingsException : Exception
    {

        internal BannerKingsException(string cause) : base(cause) { }
        internal BannerKingsException(string cause, Exception inner) : base(cause, inner)
        {
            
        }
    }
}
