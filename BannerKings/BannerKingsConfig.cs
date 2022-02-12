using BannerKings.Managers;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using static BannerKings.Managers.TitleManager;
using static BannerKings.Managers.PolicyManager;
using BannerKings.Populations;
using TaleWorlds.Core;
using BannerKings.Models;
using BannerKings.Managers.Policies;
using BannerKings.Managers.Decisions;

namespace BannerKings
{
    public class BannerKingsConfig
    {

        public PopulationManager PopulationManager;
        public PolicyManager PolicyManager;
        public TitleManager TitleManager;
        public HashSet<IBannerKingsModel> Models = new HashSet<IBannerKingsModel>();

        public void InitManagers(Dictionary<Settlement, PopulationData> pops, List<MobileParty> caravans, Dictionary<Settlement,
            HashSet<BannerKingsDecision>> DECISIONS, Dictionary<Settlement, 
            HashSet<BannerKingsPolicy>> POLICIES, 
            HashSet<FeudalTitle> titles, Dictionary<Hero, HashSet<FeudalTitle>> titleHolders, Dictionary<Kingdom, FeudalTitle> kingdoms)
        {
            this.PopulationManager = new PopulationManager(pops, caravans);
            this.PolicyManager = new PolicyManager(DECISIONS, POLICIES);
            this.TitleManager = new TitleManager(titles, titleHolders, kingdoms);
            this.InitModels();
        }

        public void InitManagers(PopulationManager populationManager, PolicyManager policyManager, TitleManager titleManager)
        {
            this.PopulationManager = populationManager;
            this.PolicyManager = policyManager;
            this.TitleManager = titleManager != null ? titleManager : new TitleManager(new HashSet<FeudalTitle>(), new Dictionary<Hero, HashSet<FeudalTitle>>(),
                new Dictionary<Kingdom, FeudalTitle>());
            this.InitModels();
        }

        private void InitModels()
        {
            this.Models.Add(new BKCultureAssimilationModel());
            this.Models.Add(new BKCultureAcceptanceModel());
            this.Models.Add(new AdministrativeModel());
            this.Models.Add(new BKLegitimacyModel());
            this.Models.Add(new BKUsurpationModel());
            this.Models.Add(new BKStabilityModel());
            this.Models.Add(new BKGrowthModel());
            this.Models.Add(new BKEconomyModel());
        }

        public static BannerKingsConfig Instance
        {
            get => ConfigHolder.CONFIG;
        }

        internal struct ConfigHolder
        {
             public static BannerKingsConfig CONFIG = new BannerKingsConfig();
        }
    }
}
