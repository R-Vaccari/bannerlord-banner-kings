using BannerKings.Managers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings.Behaviours
{
    public class BKManagerBehavior : BannerKingsBehavior
    {
        private CourtManager courtManager;
        private EducationManager educationsManager;
        private InnovationsManager innovationsManager;
        private PolicyManager policyManager;
        private PopulationManager populationManager;
        private ReligionsManager religionsManager;
        private TitleManager titleManager;
        private GoalManager goalsManager;
        private bool firstUse = BannerKingsConfig.Instance.FirstUse;

        public override void RegisterEvents()
        {
            CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, OnGameCreated);
            CampaignEvents.OnGameEarlyLoadedEvent.AddNonSerializedListener(this, OnGameEarlyLoaded);
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnGameCreated);
        }

        public override void SyncData(IDataStore dataStore)
        {
            if (dataStore.IsSaving)
            {
                populationManager = BannerKingsConfig.Instance.PopulationManager;
                policyManager = BannerKingsConfig.Instance.PolicyManager;
                titleManager = BannerKingsConfig.Instance.TitleManager;
                courtManager = BannerKingsConfig.Instance.CourtManager;
                religionsManager = BannerKingsConfig.Instance.ReligionsManager;
                educationsManager = BannerKingsConfig.Instance.EducationManager;
                innovationsManager = BannerKingsConfig.Instance.InnovationsManager;
                goalsManager = BannerKingsConfig.Instance.GoalManager;
                firstUse = BannerKingsConfig.Instance.FirstUse;

                educationsManager.CleanEntries();
                religionsManager.CleanEntries();
            }

            if (BannerKingsConfig.Instance.wipeData)
            {
                populationManager = null;
                policyManager = null;
                titleManager = null;
                courtManager = null;
                religionsManager = null;
                educationsManager = null;
                innovationsManager = null;
                goalsManager = null;
            }

            dataStore.SyncData("bannerkings-populations", ref populationManager);
            dataStore.SyncData("bannerkings-titles", ref titleManager);
            dataStore.SyncData("bannerkings-courts", ref courtManager);
            dataStore.SyncData("bannerkings-policies", ref policyManager);
            dataStore.SyncData("bannerkings-religions", ref religionsManager);
            dataStore.SyncData("bannerkings-educations", ref educationsManager);
            dataStore.SyncData("bannerkings-innovations", ref innovationsManager);
            dataStore.SyncData("bannerkings-goals", ref goalsManager);
            dataStore.SyncData("bannerkings-first-use", ref firstUse);

            if (dataStore.IsLoading)
            {
                if (firstUse || populationManager == null)
                {
                    BannerKingsConfig.Instance.InitializeManagersFirstTime();
                }
                else
                {
                    BannerKingsConfig.Instance.InitManagers(populationManager, policyManager, titleManager, courtManager, religionsManager, educationsManager, innovationsManager, goalsManager);
                }
            }
        }

        internal void NullManagers()
        {
            populationManager = null;
            titleManager = null;
            courtManager = null;
            policyManager = null;
            religionsManager = null;
            educationsManager = null;
            innovationsManager = null;
            goalsManager = null;
            firstUse = true;
            BannerKingsConfig.Instance.FirstUse = true;
        }

        private void OnGameCreated(CampaignGameStarter starter)
        {
            if (firstUse)
            {
                BannerKingsConfig.Instance.InitializeManagersFirstTime();
                BannerKingsConfig.Instance.TitleManager.PostInitialize();
                BannerKingsConfig.Instance.ReligionsManager.PostInitialize();
                BannerKingsConfig.Instance.InnovationsManager.PostInitialize();
            }
        }

        private void OnGameLoaded(CampaignGameStarter starter)
        {
            if (firstUse)
            {
                BannerKingsConfig.Instance.InitializeManagersFirstTime();
                BannerKingsConfig.Instance.InnovationsManager.PostInitialize();
                BannerKingsConfig.Instance.TitleManager.PostInitialize();
                BannerKingsConfig.Instance.ReligionsManager.PostInitialize();
            }

            foreach (var settlement in Settlement.All)
            {
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                if (data != null)
                {
                    var dominant = data.CultureData.DominantCulture;
                    if (dominant.BasicTroop != null)
                    {
                        data.Settlement.Culture = dominant;
                    }
                }
            }
        }

        private void OnGameEarlyLoaded(CampaignGameStarter starter)
        {
            BannerKingsConfig.Instance.Initialize();
            if (!firstUse)
            {
                BannerKingsConfig.Instance.PopulationManager.PostInitialize();
                BannerKingsConfig.Instance.EducationManager.PostInitialize();
                BannerKingsConfig.Instance.ReligionsManager.PostInitialize();
                BannerKingsConfig.Instance.CourtManager.PostInitialize();
                BannerKingsConfig.Instance.GoalManager.PostInitialize();
                BannerKingsConfig.Instance.InnovationsManager.PostInitialize();
                BannerKingsConfig.Instance.TitleManager.PostInitialize();
                BannerKingsConfig.Instance.ReligionsManager.PostInitialize();
            } 
        }
    }
}