using System.Collections.Generic;
using System.Linq;
using BannerKings.Behaviours.Criminality;
using BannerKings.Managers;
using BannerKings.Managers.AI;
using BannerKings.Managers.Court;
using BannerKings.Managers.Court.Grace;
using BannerKings.Managers.Court.Members;
using BannerKings.Managers.Court.Members.Tasks;
using BannerKings.Managers.Cultures;
using BannerKings.Managers.Decisions;
using BannerKings.Managers.Education.Books;
using BannerKings.Managers.Education.Languages;
using BannerKings.Managers.Education.Lifestyles;
using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Institutions.Religions.Doctrines;
using BannerKings.Managers.Institutions.Religions.Faiths;
using BannerKings.Managers.Policies;
using BannerKings.Managers.Populations;
using BannerKings.Managers.Populations.Villages;
using BannerKings.Managers.Titles;
using BannerKings.Managers.Titles.Governments;
using BannerKings.Managers.Titles.Laws;
using BannerKings.Managers.Traits;
using BannerKings.Models.BKModels;
using BannerKings.Models.Vanilla;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings
{
    public class BannerKingsConfig
    {
        public const string VersionNumber = "1.2.7.6";
        public const string VersionEdition = "Standard";
        public string VersionName => VersionNumber + VersionEdition;
        public const string EmpireCulture = "empire";
        public const string AseraiCulture = "aserai";
        public const string SturgiaCulture = "sturgia";
        public const string VlandiaCulture = "vlandia";
        public const string KhuzaitCulture = "khuzait";
        public const string BattaniaCulture = "battania";

        private List<ITypeInitializer> modInitializers = new List<ITypeInitializer>();

        public bool FirstUse { get; private set; } = true;

        public AIBehavior AI = new();

        public bool wipeData = false;
        public PopulationManager PopulationManager { get; private set; }
        public PolicyManager PolicyManager { get; private set; }
        public TitleManager TitleManager { get; private set; }
        public CourtManager CourtManager { get; private set; }
        public ReligionsManager ReligionsManager { get; private set; }
        public EducationManager EducationManager { get; private set; }
        public InnovationsManager InnovationsManager { get; private set; }
        public GoalManager GoalManager { get; private set; }

        public BKConstructionModel ConstructionModel { get; } = new();
        public BKInfluenceModel InfluenceModel { get; } = new();
        public BKTitleModel TitleModel { get; } = new();
        public BKStabilityModel StabilityModel { get; } = new();
        public BKClanFinanceModel ClanFinanceModel { get; } = new();
        public BKEducationModel EducationModel { get; } = new();
        public BKCouncilModel CouncilModel { get; } = new();
        public BKLearningModel LearningModel { get; } = new();
        public BKInnovationsModel InnovationsModel { get; } = new();
        public BKEconomyModel EconomyModel { get; } = new();
        public BKWorkshopModel WorkshopModel { get; } = new();
        public BKAdministrativeModel AdministrativeModel { get; } = new();
        public BKSmithingModel SmithingModel { get; } = new();
        public BKCultureModel CultureModel { get; } = new();
        public BKReligionModel ReligionModel { get; } = new();
        public BKPietyModel PietyModel { get; } = new();
        public BKVolunteerModel VolunteerModel { get; } = new();
        public BKLegitimacyModel LegitimacyModel { get; } = new();
        public BKGrowthModel GrowthModel { get; } = new();
        public BKVillageProductionModel VillageProductionModel { get; } = new();
        public BKProsperityModel ProsperityModel { get; } = new();
        public BKTaxModel TaxModel { get; } = new();
        public BKEstatesModel EstatesModel { get; } = new();
        public BKMarriageModel MarriageModel { get; } = new();
        public BKArmyManagementModel ArmyManagementModel { get; } = new();
        public BKCrimeModel CrimeModel { get; } = new();
        public BKCompanionPrices CompanionModel { get; } = new();
        public BKKingodmDecsionModel KingdomDecisionModel { get; } = new();
        public IPartyNeedsModel PartyNeedsModel { get; } = new BKPartyNeedsModel();

        static BannerKingsConfig()
        {
            ConfigHolder.CONFIG = new();
        }

        public static BannerKingsConfig Instance => ConfigHolder.CONFIG;

        public void AddInitializer(ITypeInitializer init)
        {
            if (init != null)
            {
                modInitializers.Add(init);
            }
        }

        public void InitializeManagersFirstTime()
        {
            InitManagers();
            foreach (var settlement in Settlement.All.Where(settlement => settlement.IsVillage || settlement.IsTown || settlement.IsCastle))
            {
                PopulationManager.InitializeSettlementPops(settlement);
            }

            foreach (var clan in Clan.All.Where(clan => !clan.IsEliminated && !clan.IsBanditFaction))
            {
                CourtManager.CreateCouncil(clan);
            }

            foreach (var hero in Hero.AllAliveHeroes)
            {
                EducationManager.InitHeroEducation(hero);
            }

            FirstUse = false;
        }

        private void Initialize()
        {
            BKTraits.Instance.Initialize();
            DefaultVillageBuildings.Instance.Initialize();
            DefaultDivinities.Instance.Initialize();
            DefaultFaiths.Instance.Initialize();
            DefaultDoctrines.Instance.Initialize();
            DefaultLanguages.Instance.Initialize();
            DefaultBookTypes.Instance.Initialize();
            DefaultLifestyles.Instance.Initialize();
            DefaultDemesneLaws.Instance.Initialize();
            DefaultReligions.Instance.Initialize();
            DefaultCouncilTasks.Instance.Initialize();
            DefaultCouncilPositions.Instance.Initialize();
            DefaultCrimes.Instance.Initialize();
            DefaultCriminalSentences.Instance.Initialize();
            DefaultCourtExpenses.Instance.Initialize();
            DefaultPopulationNames.Instance.Initialize();
            DefaultTitleNames.Instance.Initialize();
            DefaultSuccessions.Instance.Initialize();
            DefaultGovernments.Instance.Initialize();
            foreach (ITypeInitializer init in modInitializers)
            {
                init.Initialize();
            }
        }

        public void InitManagers()
        {
            Initialize();

            PopulationManager = new PopulationManager(new Dictionary<Settlement, PopulationData>(), new List<MobileParty>());
            PolicyManager = new PolicyManager(new Dictionary<Settlement, List<BannerKingsDecision>>(), new Dictionary<Settlement, List<BannerKingsPolicy>>());
            TitleManager = new TitleManager(new Dictionary<FeudalTitle, Hero>(), new Dictionary<Kingdom, FeudalTitle>());
            CourtManager = new CourtManager(new Dictionary<Clan, CouncilData>());
            ReligionsManager = new ReligionsManager();
            EducationManager = new EducationManager();
            InnovationsManager = new InnovationsManager();
            GoalManager = new GoalManager();
        }

        public void InitManagers(PopulationManager populationManager, PolicyManager policyManager, TitleManager titleManager, CourtManager court, ReligionsManager religions, EducationManager educations, InnovationsManager innovations, GoalManager goals)
        {
            Initialize();

            PopulationManager = populationManager;
            PolicyManager = policyManager;
            TitleManager = titleManager;
            titleManager.RefreshCaches();
            CourtManager = court;
            ReligionsManager = religions ?? new ReligionsManager();
            EducationManager = educations ?? new EducationManager();
            InnovationsManager = innovations ?? new InnovationsManager();
            GoalManager = goals ?? new GoalManager();
            FirstUse = false;
        }

        private struct ConfigHolder
        {
            public static BannerKingsConfig CONFIG = new();
        }
    }
}