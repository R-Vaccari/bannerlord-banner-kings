using System;

namespace BannerKings.Managers.Titles.Governments
{
    public abstract class ContractAspect : BannerKingsObject
    {
        private Func<FeudalContract, bool> isAdequateForContract;
        public ContractAspect(string stringId) : base(stringId)
        {
        }

        public abstract void PostInitialize();
        public float Authoritarian { get; protected set; }
        public float Oligarchic { get; protected set; }
        public float Egalitarian { get; protected set; }

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
