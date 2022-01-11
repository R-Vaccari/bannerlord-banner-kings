using Populations.Managers;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using static Populations.Managers.TitleManager;
using static Populations.PolicyManager;
using static Populations.PopulationManager;

namespace Populations
{
    public class PopulationConfig
    {

        public PopulationManager PopulationManager;
        public PolicyManager PolicyManager;
        public TitleManager TitleManager;

        public void InitManagers(Dictionary<Settlement, PopulationData> pops, List<MobileParty> caravans, Dictionary<Settlement, List<PolicyElement>> policies,
            Dictionary<Settlement, TaxType> taxes, Dictionary<Settlement, MilitiaPolicy> militias, Dictionary<Settlement, WorkforcePolicy> workforce, 
            Dictionary<Settlement, TariffType> tariffs, Dictionary<Settlement, CriminalPolicy> criminal, 
            HashSet<FeudalTitle> titles, Dictionary<Hero, HashSet<FeudalTitle>> titleHolders, Dictionary<Kingdom, FeudalTitle> kingdoms)
        {
            this.PopulationManager = new PopulationManager(pops, caravans);
            this.PolicyManager = new PolicyManager(policies, taxes, militias, workforce, tariffs, criminal);
            this.TitleManager = new TitleManager(titles, titleHolders, kingdoms);
        }

        public void InitManagers(PopulationManager populationManager, PolicyManager policyManager, TitleManager titleManager)
        {
            this.PopulationManager = populationManager;
            this.PolicyManager = policyManager;
            this.TitleManager = titleManager != null ? titleManager : new TitleManager(new HashSet<FeudalTitle>(), new Dictionary<Hero, HashSet<FeudalTitle>>(),
                new Dictionary<Kingdom, FeudalTitle>());
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
