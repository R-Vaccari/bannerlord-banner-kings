using BannerKings.Managers;
using BannerKings.Managers.Court;
using BannerKings.Managers.Decisions;
using BannerKings.Managers.Education.Books;
using BannerKings.Managers.Education.Languages;
using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Institutions.Religions.Doctrines;
using BannerKings.Managers.Institutions.Religions.Faiths;
using BannerKings.Managers.Policies;
using BannerKings.Managers.Populations.Villages;
using BannerKings.Managers.Titles;
using BannerKings.Models;
using BannerKings.Models.BKModels;
using BannerKings.Populations;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using BannerKings.Managers.AI;
using BannerKings.Managers.Education.Lifestyles;
using BannerKings.Models.Vanilla;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Party;

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
        public InnovationsManager InnovationsManager { get; private set; }

        public HashSet<IBannerKingsModel> Models = new HashSet<IBannerKingsModel>();
        public AIBehavior AI = new AIBehavior();
        public bool wipeData = false;

        public BKInfluenceModel InfluenceModel { get; } = new BKInfluenceModel();
        public BKTitleModel TitleModel { get; } = new BKTitleModel();
        public BKStabilityModel StabilityModel { get; } = new BKStabilityModel();
        public BKClanFinanceModel ClanFinanceModel { get; } = new BKClanFinanceModel();
        public BKEducationModel EducationModel { get; } = new BKEducationModel();
        public BKCouncilModel CouncilModel { get; } = new BKCouncilModel();
        public BKLearningModel LearningModel { get; } = new BKLearningModel();
        public BKInnovationsModel InnovationsModel { get; } = new BKInnovationsModel();
        public BKEconomyModel EconomyModel { get; } = new BKEconomyModel();
        public BKWorkshopModel WorkshopModel { get; } = new BKWorkshopModel();
        public BKAdministrativeModel AdministrativeModel { get; } = new BKAdministrativeModel();
        public BKSmithingModel SmithingModel { get; } = new BKSmithingModel();
        public BKCultureAcceptanceModel CultureAcceptanceModel { get; } = new BKCultureAcceptanceModel();
        public BKCultureAssimilationModel CultureAssimilationModel { get; } = new BKCultureAssimilationModel();

        public void Initialize()
        {
            DefaultVillageBuildings.Instance.Initialize();
            DefaultDivinities.Instance.Initialize();
            DefaultFaiths.Instance.Initialize();
            DefaultDoctrines.Instance.Initialize();

            DefaultLanguages.Instance.Initialize();
            DefaultBookTypes.Instance.Initialize();
            DefaultLifestyles.Instance.Initialize();

            Models.Add(new BKCultureAssimilationModel());
            Models.Add(new BKCultureAcceptanceModel());
            Models.Add(new BKAdministrativeModel());
            Models.Add(new BKLegitimacyModel());
            Models.Add(new BKTitleModel());
            Models.Add(new BKStabilityModel());
            Models.Add(new BKGrowthModel());
            Models.Add(new BKEconomyModel());
            Models.Add(new BKPietyModel());
            Models.Add(new BKCouncilModel());
        }

        public void InitManagers()
        {
            Initialize();
            PopulationManager = new PopulationManager(new Dictionary<Settlement, PopulationData>(), new List<MobileParty>());
            PolicyManager = new PolicyManager(new Dictionary<Settlement, List<BannerKingsDecision>>(), new Dictionary<Settlement,
            List<BannerKingsPolicy>>());
            TitleManager = new TitleManager(new Dictionary<FeudalTitle, Hero>(), new Dictionary<Kingdom, FeudalTitle>());
            CourtManager = new CourtManager(new Dictionary<Clan, CouncilData>());
            ReligionsManager = new ReligionsManager();
            EducationManager = new EducationManager();
            InnovationsManager = new InnovationsManager();
        }

        public void InitManagers(PopulationManager populationManager, PolicyManager policyManager, TitleManager titleManager, CourtManager court,
            ReligionsManager religions, EducationManager educations, InnovationsManager innovations)
        {
            Initialize();
            PopulationManager = populationManager;
            PolicyManager = policyManager;
            TitleManager = titleManager;
            titleManager.RefreshCaches();
            CourtManager = court;
            ReligionsManager = religions != null ? religions : new ReligionsManager();
            EducationManager = educations != null ? educations : new EducationManager();
            InnovationsManager = innovations != null ? innovations : new InnovationsManager();
        }

        public static BannerKingsConfig Instance => ConfigHolder.CONFIG;

        internal struct ConfigHolder
        {
             public static BannerKingsConfig CONFIG = new BannerKingsConfig();
        }
    }
}
