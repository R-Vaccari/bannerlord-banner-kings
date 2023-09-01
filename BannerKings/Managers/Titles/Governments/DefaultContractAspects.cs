using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BannerKings.Managers.Titles.Governments
{
    public class DefaultContractAspects : DefaultTypeInitializer<DefaultContractAspects, ContractAspect>
    {
        public ContractAspect ConquestMight { get; } = new ContractAspect("");
        public ContractAspect ConquestClaim { get; } = new ContractAspect("");
        public ContractAspect ConquestDistributed { get; } = new ContractAspect("");

        public ContractAspect RevocationProtected { get; } = new ContractAspect("");
        public ContractAspect RevocationVassalage { get; } = new ContractAspect("");
        public ContractAspect RevocationImperial { get; } = new ContractAspect("");
        public ContractAspect RevocationRepublic { get; } = new ContractAspect("");

        public override IEnumerable<ContractAspect> All => throw new NotImplementedException();

        public override void Initialize()
        {
            throw new NotImplementedException();
        }
    }
}
