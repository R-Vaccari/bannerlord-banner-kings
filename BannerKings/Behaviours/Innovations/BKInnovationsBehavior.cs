using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings.Behaviours.Innovations
{
    public class BKInnovationsBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void OnDailyTick()
        {
            BannerKingsConfig.Instance.InnovationsManager.UpdateInnovations();
        }

        private void OnGameLoaded(CampaignGameStarter campaignGameStarter)
        {

        }
    }
}