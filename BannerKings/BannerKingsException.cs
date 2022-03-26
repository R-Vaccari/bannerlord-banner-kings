using System;

namespace BannerKings
{
    internal class BannerKingsException : Exception
    {

        internal BannerKingsException(string cause, Exception inner) : base(cause, inner)
        {
            
        }
    }
}
