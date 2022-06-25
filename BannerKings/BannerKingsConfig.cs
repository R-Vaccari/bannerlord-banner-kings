using BannerKings.Managers;
using BannerKings.Managers.Court;
using BannerKings.Managers.Decisions;
using BannerKings.Managers.Education.Books;
using BannerKings.Managers.Education.Languages;
using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Institutions.Religions.Doctrines;
using BannerKings.Managers.Institutions.Religions.Faiths;
using BannerKings.Managers.Items;
using BannerKings.Managers.Policies;
using BannerKings.Managers.Populations.Villages;
using BannerKings.Managers.Titles;
using BannerKings.Models;
using BannerKings.Models.BKModels;
using BannerKings.Models.Populations;
using BannerKings.Populations;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using BannerKings.Managers.AI;

namespace BannerKings
{
    public class BannerKingsConfig
    {
        public PopulationManager PopulationManager { get; private set; }
        public PolicyManager PolicyManager { get; private set; }
        public TitleManager TitleManager { get; private set; }
        public CourtManager CourtManager { get; private set; }
        public ReligionsManager ReligionsManager { get; private set; }
        public EducationManager EducationManager { get; private set; }


        public BKEducationModel EducationModel { get; private set; } = new BKEducationModel();

        public HashSet<IBannerKingsModel> Models = new HashSet<IBannerKingsModel>();
        public AIBehavior AI = new AIBehavior();
        public bool wipeData = false;

        private void Initialize()
        {
            DefaultVillageBuildings.Instance.Init();
            DefaultDivinities.Instance.Initialize();
            DefaultFaiths.Instance.Initialize();
            DefaultDoctrines.Instance.Initialize();
            BKItemCategories.Instance.Initialize();
            BKItems.Instance.Initialize();
            DefaultLanguages.Instance.Initialize();
            DefaultBookTypes.Instance.Initialize();

            Models.Add(new BKCultureAssimilationModel());
            Models.Add(new BKCultureAcceptanceModel());
            Models.Add(new BKAdministrativeModel());
            Models.Add(new BKLegitimacyModel());
            Models.Add(new BKTitleModel());
            Models.Add(new BKStabilityModel());
            Models.Add(new BKGrowthModel());
            Models.Add(new BKEconomyModel());
            Models.Add(new BKCaravanAttractionModel());
            Models.Add(new BKPietyModel());
            Models.Add(new BKCouncilModel());
        }

        public void InitManagers()
        {
            Initialize();
            PopulationManager = new PopulationManager(new Dictionary<Settlement, PopulationData>(), new List<MobileParty>());
            PopulationManager.ReInitBuildings();
            PolicyManager = new PolicyManager(new Dictionary<Settlement, List<BannerKingsDecision>>(), new Dictionary<Settlement,
            List<BannerKingsPolicy>>());
            TitleManager = new TitleManager(new Dictionary<FeudalTitle, Hero>(), new Dictionary<Hero, List<FeudalTitle>>(), new Dictionary<Kingdom, FeudalTitle>());
            CourtManager = new CourtManager(new Dictionary<Clan, CouncilData>());
            ReligionsManager = new ReligionsManager();
            EducationManager = new EducationManager();
        }

        public void InitManagers(PopulationManager populationManager, PolicyManager policyManager, TitleManager titleManager, CourtManager court,
            ReligionsManager religions, EducationManager educations)
        {
            Initialize();
            PopulationManager = populationManager;
            PopulationManager.ReInitBuildings();
            PolicyManager = policyManager;
            TitleManager = titleManager;
            titleManager.RefreshDeJure();
            CourtManager = court;
            ReligionsManager = religions != null ? religions : new ReligionsManager();
            EducationManager = educations != null ? educations : new EducationManager();
        }

        public static BannerKingsConfig Instance => ConfigHolder.CONFIG;

        internal struct ConfigHolder
        {
             public static BannerKingsConfig CONFIG = new BannerKingsConfig();
        }
    }
}
