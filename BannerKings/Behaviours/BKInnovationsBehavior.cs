using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings.Behaviours
{
    public class BKInnovationsBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
            CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, OnDailyTickSettlement);
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void OnDailyTick()
        {
            BannerKingsConfig.Instance.InnovationsManager.UpdateInnovations();
        }

        private void OnDailyTickSettlement(Settlement settlement)
        {
            BannerKingsConfig.Instance.InnovationsManager.AddSettlementResearch(settlement);
        }

        private void OnGameLoaded(CampaignGameStarter campaignGameStarter)
        {
            
        }
    }
}