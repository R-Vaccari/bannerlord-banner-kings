using BannerKings.Managers;
using BannerKings.Populations;
using TaleWorlds.CampaignSystem;
using System;

namespace BannerKings.Behaviours
{
    class BKSaveBehavior : CampaignBehaviorBase
    {
        private PopulationManager populationManager;
        private PolicyManager policyManager;
        private TitleManager titleManager;
        private CourtManager courtManager;
        private ReligionsManager religionsManager;

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnSessionLaunched));
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
            }

            if (BannerKingsConfig.Instance.wipeData)
            {
                populationManager = null;
                policyManager = null;
                titleManager = null;
                courtManager = null;
                religionsManager = null;
            }

            dataStore.SyncData("bannerkings-populations", ref populationManager);
            dataStore.SyncData("bannerkings-titles", ref titleManager);
            dataStore.SyncData("bannerkings-courts", ref courtManager);
            dataStore.SyncData("bannerkings-policies", ref policyManager);
            dataStore.SyncData("bannerkings-religions", ref religionsManager);

            if (dataStore.IsLoading)
            {
                if (populationManager == null && policyManager == null && titleManager == null && courtManager == null)
                    BannerKingsConfig.Instance.InitManagers();

                else BannerKingsConfig.Instance.InitManagers(populationManager, policyManager,
                    titleManager, courtManager, religionsManager);
            }
        }

        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            if (BannerKingsConfig.Instance.PopulationManager != null)
            {
                foreach (Settlement settlement in Settlement.All)
                    if (BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
                    {
                        PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                        settlement.Culture = data.CultureData.DominantCulture;
                    }
            }

            if (BannerKingsConfig.Instance.PolicyManager == null || BannerKingsConfig.Instance.TitleManager == null)
                BannerKingsConfig.Instance.InitManagers();

            BannerKingsConfig.Instance.ReligionsManager.InitializePresets();
        }
    }
}
