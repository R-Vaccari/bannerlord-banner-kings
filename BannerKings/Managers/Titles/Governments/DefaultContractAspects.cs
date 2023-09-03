using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Titles.Governments
{
    public class DefaultContractAspects : DefaultTypeInitializer<DefaultContractAspects, ContractAspect>
    {
        public ContractAspect ConquestMight { get; } = new ContractRight("");
        public ContractAspect ConquestClaim { get; } = new ContractRight("");
        public ContractAspect ConquestDistributed { get; } = new ContractRight("");

        public ContractAspect RevocationProtected { get; } = new ContractRight("RevocationProtected");
        public ContractAspect RevocationVassalage { get; } = new ContractRight("RevocationVassalage");
        public ContractAspect RevocationImperial { get; } = new ContractRight("RevocationImperial");
        public ContractAspect RevocationRepublic { get; } = new ContractRight("RevocationRepublic");

        public override IEnumerable<ContractAspect> All
        {
            get
            {
                yield return RevocationRepublic;
                yield return RevocationVassalage;
                yield return RevocationProtected;
                yield return RevocationImperial;
            }
        }

        public List<ContractAspect> GetIdealKingdomAspects(Kingdom kingdom, Government government)
        {
            List<ContractAspect> result = new List<ContractAspect>(4);
            if (government == DefaultGovernments.Instance.Republic)
            {
                result.Add(RevocationRepublic);
            }
            else if (government == DefaultGovernments.Instance.Imperial)
            {
                result.Add(RevocationImperial);
            }
            else if (government == DefaultGovernments.Instance.Tribal)
            {
                result.Add(RevocationProtected);
            }
            else
            {
                result.Add(RevocationVassalage);
            }

            return result;
        }

        public override void Initialize()
        {
            RevocationProtected.Initialize(new TextObject("{=!}Revoking Protection"),
                new TextObject("{=!}Revoking Protection guarantees that all vassals of the realm have their titles secured from revoking."));

            RevocationImperial.Initialize(new TextObject("{=!}Imperial Revoking"),
                new TextObject("{=!}Imperial Revoking concentrates all the revoking power on the ruler, who is said to be the true owner of all land."));

            RevocationRepublic.Initialize(new TextObject("{=!}Republican Revoking"),
                new TextObject("{=!}Republican revoking protects most tites except those of Duke levels, who are often the main contests of elections. Its purpose in theory is to stop lords from accumulating too much power, but many a time has been used to destroy political rivals."));

            RevocationVassalage.Initialize(new TextObject("{=!}Vassalage Revoking"),
                new TextObject("{=!}Vassalage Revoking is the feudal form of revoking. Relying on a strict hierarchy of suzerain and vassal relationships, Vassalage distributes the power of revoking to this hierarchy chain, such that not too much power is concentrated either with the ruler or specific lords."));
        }
    }
}
