using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using static Populations.PolicyManager;
using static Populations.PopulationManager;

namespace Populations
{
    public class PopulationConfig
    {

        public PopulationManager PopulationManager;
        public PolicyManager PolicyManager;

        public void InitManagers(Dictionary<Settlement, PopulationData> pops, List<MobileParty> caravans, Dictionary<Settlement, List<PolicyElement>> policies,
            Dictionary<Settlement, TaxType> taxes, Dictionary<Settlement, MilitiaPolicy> militias, Dictionary<Settlement, WorkforcePolicy> workforce, 
            Dictionary<Settlement, TariffType> tariffs, Dictionary<Settlement, CriminalPolicy> criminal)
        {
            this.PopulationManager = new PopulationManager(pops, caravans);
            this.PolicyManager = new PolicyManager(policies, taxes, militias, workforce, tariffs, criminal);
        }

        public void InitManagers(PopulationManager populationManager, PolicyManager policyManager)
        {
            this.PopulationManager = populationManager;
            this.PolicyManager = policyManager;
        }

        public static PopulationConfig Instance
        {
            get => ConfigHolder.CONFIG;
        }

        internal struct ConfigHolder
        {
             public static PopulationConfig CONFIG = new PopulationConfig();
        }
    }
}
