using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BannerKings.Managers.Titles.Governments
{
    public class ContractAspect : BannerKingsObject
    {
        public ContractAspect(string stringId) : base(stringId)
        {
        }

        public enum AspectTypes
        {
            Conquest,
            Revocation,
            Taxes,
            Military,
            Religious
        }
    }
}
