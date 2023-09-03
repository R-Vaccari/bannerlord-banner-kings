using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BannerKings.Managers.Titles.Governments
{
    public class ContractAspect : BannerKingsObject
    {
        private Func<FeudalContract, bool> isAdequateForContract;
        public ContractAspect(string stringId) : base(stringId)
        {
        }

        public float Factor { get; private set; }
        public bool IsAdequateForContract(FeudalContract contract) => isAdequateForContract(contract);

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
