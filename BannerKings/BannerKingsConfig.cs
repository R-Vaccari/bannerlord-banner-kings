using System.Collections.Generic;
using System.Linq;
using BannerKings.Behaviours.Criminality;
using BannerKings.Behaviours.Diplomacy.Groups;
using BannerKings.Behaviours.Diplomacy.Groups.Demands;
using BannerKings.Behaviours.Diplomacy.Wars;
using BannerKings.CampaignContent.Culture;
using BannerKings.CampaignContent.Economy.Markets;
using BannerKings.CampaignContent.Skills;
using BannerKings.Managers;
using BannerKings.Managers.Court;
using BannerKings.Managers.Court.Grace;
using BannerKings.Managers.Court.Members;
using BannerKings.Managers.Court.Members.Tasks;
using BannerKings.Managers.Cultures;
using BannerKings.Managers.Decisions;
using BannerKings.Managers.Education.Books;
using BannerKings.Managers.Education.Languages;
using BannerKings.Managers.Education.Lifestyles;
using BannerKings.Managers.Helpers;
using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Institutions.Religions.Doctrines;
using BannerKings.Managers.Institutions.Religions.Doctrines.Marriage;
using BannerKings.Managers.Institutions.Religions.Faiths;
using BannerKings.Managers.Policies;
using BannerKings.Managers.Populations;
using BannerKings.Managers.Populations.Villages;
using BannerKings.Managers.Recruits;
using BannerKings.Managers.Shipping;
using BannerKings.Managers.Titles.Governments;
using BannerKings.Managers.Titles.Laws;
using BannerKings.Managers.Traits;
using BannerKings.Models.BKModels;
using BannerKings.Models.BKModels.Abstract;
using BannerKings.Models.Vanilla;
using BannerKings.Models.Vanilla.Abstract;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace BannerKings
{
    public class BannerKingsConfig
    {
        public const string VersionNumber = "1.3.0.0";
        public const string VersionEdition = "Release";
        public string VersionName => VersionNumber + VersionEdition;
        public const string EmpireCulture = "empire";
        public const string AseraiCulture = "aserai";
        public const string SturgiaCulture = "sturgia";
        public const string VlandiaCulture = "vlandia";
        public const string KhuzaitCulture = "khuzait";
        public const string BattaniaCulture = "battania";

        private List<ITypeInitializer> modInitializers = new List<ITypeInitializer>();

        public bool FirstUse { get; internal set; } = true;
        public string TitlesGeneratorPath { get; set; } = BasePath.Name + "Modules/BannerKings/ModuleData/titles.xml";
        public string RecruitsXmlPath { get; set; }

        public bool wipeData = false;
        public PopulationManager PopulationManager { get; private set; }
        public PolicyManager PolicyManager { get; private set; }
        public TitleManager TitleManager { get; private set; }
        public CourtManager CourtManager { get; private set; }
        public ReligionsManager ReligionsManager { get; private set; }
        public EducationManager EducationManager { get; private set; }
        public InnovationsManager InnovationsManager { get; private set; }
        public GoalManager GoalManager { get; private set; }

        public BKInterestGroupsModel InterestGroupsModel { get; set; } = new();
        public BKConstructionModel ConstructionModel { get; set; } = new();
        public InfluenceModel InfluenceModel { get; set; } = new BKInfluenceModel();
        public TitleModel TitleModel { get; set; } = new BKTitleModel();
        public BKStabilityModel StabilityModel { get; set; } = new();
        public BKClanFinanceModel ClanFinanceModel { get; set; } = new();
        public BKEducationModel EducationModel { get; set; } = new();
        public BKCouncilModel CouncilModel { get; set; } = new();
        public BKLearningModel LearningModel { get; set; } = new();
        public BKInnovationsModel InnovationsModel { get; } = new();
        public EconomyModel EconomyModel { get; set; } = new BKEconomyModel();
        public BKWorkshopModel WorkshopModel { get; set; } = new();
        public BKAdministrativeModel AdministrativeModel { get; } = new();
        public BKSmithingModel SmithingModel { get; set; } = new();
        public CultureModel CultureModel { get; set; } = new BKCultureModel();
        public ReligionModel ReligionModel { get; set; } = new BKReligionModel();
        public VolunteerModel VolunteerModel { get; set; } = new BKVolunteerModel();
        public LegitimacyModel LegitimacyModel { get; set; } = new BKLegitimacyModel();
        public GrowthModel GrowthModel { get; set; } = new BKGrowthModel();
        public BKVillageProductionModel VillageProductionModel { get; set; } = new();
        public BKProsperityModel ProsperityModel { get; set; } = new();
        public BKTaxModel TaxModel { get; set; } = new();
        public BKEstatesModel EstatesModel { get; set; } = new();
        public MarriageModel MarriageModel { get; set; } = new BKMarriageModel();
        public ArmyModel ArmyManagementModel { get; set; } = new BKArmyManagementModel();
        public BKWarModel WarModel { get; set; } = new();
        public BKCrimeModel CrimeModel { get; set; } = new();
        public BKCompanionPrices CompanionModel { get; set; } = new();
        public IPartyNeedsModel PartyNeedsModel { get; set; } = new BKPartyNeedsModel();
        public DiplomacyModel DiplomacyModel { get; set; } = new BKDiplomacyModel();
        public BKKingdomDecisionModel KingdomDecisionModel { get; set; } = new();
        public MercenaryModel MercenaryModel { get; set; } = new BKMercenaryModel();
        public RelationsModel RelationsModel { get; set; } = new BKRelationsModel();

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

        public void Initialize()
        {
            BKSkillEffects.Instance.AddVanilla();
            DefaultPopulationNames.Instance.Initialize();
            DefaultTitleNames.Instance.Initialize();
            BKTraits.Instance.Initialize();
            DefaultVillageBuildings.Instance.Initialize();
            DefaultDivinities.Instance.Initialize();
            DefaultDoctrines.Instance.Initialize();
            DefaultLanguages.Instance.Initialize();
            DefaultBookTypes.Instance.Initialize();
            DefaultLifestyles.Instance.Initialize();
            DefaultDemesneLaws.Instance.Initialize();
            DefaultFaithGroups.Instance.Initialize();
            DefaultFaiths.Instance.Initialize();
            DefaultReligions.Instance.Initialize();
            DefaultCouncilTasks.Instance.Initialize();
            DefaultCouncilPositions.Instance.Initialize();
            DefaultCasusBelli.Instance.Initialize();
            BKTraits.Instance.Initialize();
            DefaultDemands.Instance.Initialize();
            DefaultRadicalGroups.Instance.Initialize();
            DefaultInterestGroup.Instance.Initialize();
            DefaultCriminalSentences.Instance.Initialize();
            DefaultCrimes.Instance.Initialize();
            DefaultCriminalSentences.Instance.Initialize();
            DefaultCourtExpenses.Instance.Initialize();   
            DefaultSuccessions.Instance.Initialize();
            DefaultInheritances.Instance.Initialize();
            DefaultGenderLaws.Instance.Initialize();    
            DefaultGovernments.Instance.Initialize();
            DefaultContractAspects.Instance.Initialize();
            DefaultShippingLanes.Instance.Initialize();
            DefaultMarketGroups.Instance.Initialize();
            DefaultRecruitSpawns.Instance.Initialize();
            DefaultCulturalStandings.Instance.Initialize();
            DefaultMarriageDoctrines.Instance.Initialize();
            DefaultWarDoctrines.Instance.Initialize();
            foreach (ITypeInitializer init in modInitializers)
            {
                init.Initialize();
            }
        }

        public void InitManagers()
        {
            Initialize();

            ReligionsManager = new ReligionsManager();
            PopulationManager = new PopulationManager(new Dictionary<Settlement, PopulationData>());
            PolicyManager = new PolicyManager(new Dictionary<Settlement, List<BannerKingsDecision>>(), new Dictionary<Settlement, List<BannerKingsPolicy>>());
            TitleManager = new TitleManager();
            TitleGenerator.InitializeTitles();
            CourtManager = new CourtManager(new Dictionary<Clan, CouncilData>());          
            EducationManager = new EducationManager();
            InnovationsManager = new InnovationsManager();
            GoalManager = new GoalManager();
        }

        public void InitManagers(PopulationManager populationManager, PolicyManager policyManager, TitleManager titleManager, CourtManager court, ReligionsManager religions, EducationManager educations, InnovationsManager innovations, GoalManager goals)
        {
            Initialize();

            ReligionsManager = religions ?? new ReligionsManager();
            PopulationManager = populationManager;
            PolicyManager = policyManager;
            TitleManager = titleManager;
            CourtManager = court;     
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