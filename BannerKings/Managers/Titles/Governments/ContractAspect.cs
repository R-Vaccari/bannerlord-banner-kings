using System;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Managers.Titles.Governments
{
    public abstract class ContractAspect : BannerKingsObject
    {
        protected Func<Kingdom, bool> isAdequateForKingdom;
        public ContractAspect(string stringId) : base(stringId)
        {
        }

        public abstract void PostInitialize();
        public float Authoritarian { get; protected set; }
        public float Oligarchic { get; protected set; }
        public float Egalitarian { get; protected set; }
        public AspectTypes AspectType { get; protected set; }

        public bool IsAdequateForKingdom(Kingdom kingdom)
        {
            if (isAdequateForKingdom != null) isAdequateForKingdom(kingdom);
            return true;
        }

        public override bool Equals(object obj)
        {
            if (obj is ContractAspect)
            {
                return (obj as ContractAspect).StringId == StringId;
            }
            return base.Equals(obj);
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
