using BannerKings.Managers;
using BannerKings.Managers.Goals;
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
            CampaignEvents.OnCharacterCreationIsOverEvent.AddNonSerializedListener(this, OnCreationEnded);
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);
            //CampaignEvents.DailyTickHeroEvent.AddNonSerializedListener(this, OnDailyTickHeroEvent);
            //CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, OnDailyTickSettlementEvent);
        }

        private void OnCreationEnded()
        {
            DefaultGoals.Instance.Initialize();
        }

        private void OnGameLoaded(CampaignGameStarter starter)
        {
            DefaultGoals.Instance.Initialize();
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