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
using BannerKings.Managers.Titles;
using BannerKings.Models.BKModels;
using BannerKings.Managers.AI;

namespace BannerKings
{
    public class BannerKingsConfig
    {

        public PopulationManager PopulationManager;
        public PolicyManager PolicyManager;
        public TitleManager TitleManager;
        public CourtManager CourtManager;
        public HashSet<IBannerKingsModel> Models = new HashSet<IBannerKingsModel>();
        public AIBehavior AI = new AIBehavior();
        public bool wipeData = false;
        public MBReadOnlyList<BuildingType> VillageBuildings { get; set; }

        public BKInfluenceModel InfluenceModel { get; } = new BKInfluenceModel();

        public void InitManagers()
        {
            DefaultVillageBuildings.Instance.Init();
            this.PopulationManager = new PopulationManager(new Dictionary<Settlement, PopulationData>(), new List<MobileParty>());
            this.PopulationManager.ReInitBuildings();
            this.PolicyManager = new PolicyManager(new Dictionary<Settlement, List<BannerKingsDecision>>(), new Dictionary<Settlement,
            List<BannerKingsPolicy>>());
            this.TitleManager = new TitleManager(new Dictionary<FeudalTitle, Hero>(), new Dictionary<Hero, List<FeudalTitle>>(), new Dictionary<Kingdom, FeudalTitle>());
            this.CourtManager = new CourtManager(new Dictionary<Clan, CouncilData>());
            this.InitModels();
        }

        public void InitManagers(PopulationManager populationManager, PolicyManager policyManager, TitleManager titleManager, CourtManager court)
        {
            DefaultVillageBuildings.Instance.Init();
            this.PopulationManager = populationManager;
            this.PopulationManager.ReInitBuildings();
            this.PolicyManager = policyManager;
            this.TitleManager = titleManager;
            titleManager.RefreshDeJure();
            this.CourtManager = court;
            this.InitModels();
        }

        private void InitModels()
        {
            this.Models.Add(new BKCultureAssimilationModel());
            this.Models.Add(new BKCultureAcceptanceModel());
            this.Models.Add(new BKAdministrativeModel());
            this.Models.Add(new BKLegitimacyModel());
            this.Models.Add(new BKTitleModel());
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
