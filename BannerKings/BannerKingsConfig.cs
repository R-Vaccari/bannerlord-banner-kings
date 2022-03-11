using BannerKings.Managers;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using static BannerKings.Managers.TitleManager;
using BannerKings.Populations;
using BannerKings.Models;
using BannerKings.Managers.Policies;
using BannerKings.Managers.Decisions;
using BannerKings.Models.Populations;
using TaleWorlds.Library;
using BannerKings.Managers.Populations.Villages;
using BannerKings.Managers.Court;

namespace BannerKings
{
    public class BannerKingsConfig
    {

        public PopulationManager PopulationManager;
        public PolicyManager PolicyManager;
        public TitleManager TitleManager;
        public CourtManager CourtManager;
        public HashSet<IBannerKingsModel> Models = new HashSet<IBannerKingsModel>();
        public MBReadOnlyList<BuildingType> VillageBuildings { get; set; }

        public void InitManagers()
        {
            DefaultVillageBuildings.Instance.Init();
            this.PopulationManager = new PopulationManager(new Dictionary<Settlement, PopulationData>(), new List<MobileParty>());
            this.PolicyManager = new PolicyManager(new Dictionary<Settlement, HashSet<BannerKingsDecision>>(), new Dictionary<Settlement,
            HashSet<BannerKingsPolicy>>());
            this.TitleManager = new TitleManager(new Dictionary<FeudalTitle, (Hero, Hero)>(), new Dictionary<Hero, List<FeudalTitle>>(), new Dictionary<Kingdom, FeudalTitle>());
            this.CourtManager = new CourtManager(new Dictionary<Hero, Council>());
            this.InitModels();
        }

        public void InitManagers(PopulationManager populationManager, PolicyManager policyManager, TitleManager titleManager, CourtManager court)
        {
            this.PopulationManager = populationManager;
            this.PolicyManager = policyManager;
            this.TitleManager = titleManager != null ? titleManager : new TitleManager(new Dictionary<FeudalTitle, (Hero, Hero)>(), new Dictionary<Hero, List<FeudalTitle>>(),
                new Dictionary<Kingdom, FeudalTitle>());
            this.CourtManager = court;
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
            this.Models.Add(new BKCaravanAttractionModel());
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
