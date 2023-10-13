using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BannerKings.Managers.Institutions.Religions.Leaderships
{
    public class HeadOfFaith : BannerKingsObject
    {
        public HeadOfFaith(string stringId) : base(stringId)
        {
        }

        public override bool Equals(object obj)
        {
            if (obj is HeadOfFaith)
            {
                return (obj as HeadOfFaith).StringId == StringId;
            }
            return base.Equals(obj);
        }
    }
}
