using BannerKings.Managers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings.Behaviours
{
    internal class BKGoalBehavior : CampaignBehaviorBase
    {
        private static GoalManager GoalManager => BannerKingsConfig.Instance.GoalManager;

        public override void SyncData(IDataStore dataStore)
        {

        }

        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickHeroEvent.AddNonSerializedListener(this, OnDailyTickHeroEvent);
            CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, OnDailyTickSettlementEvent);
        }

        private void OnDailyTickHeroEvent(Hero hero)
        {
            GoalManager.UpdateHeroGoals();
        }

        private void OnDailyTickSettlementEvent(Settlement settlement)
        {
            GoalManager.UpdateSettlementGoals();
        }
    }
}