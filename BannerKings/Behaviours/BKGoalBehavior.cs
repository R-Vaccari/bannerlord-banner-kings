using BannerKings.Managers.Goals;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Behaviours
{
    public class BKGoalBehavior : CampaignBehaviorBase
    {
        public override void SyncData(IDataStore dataStore)
        {

        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnCharacterCreationIsOverEvent.AddNonSerializedListener(this, OnCreationEnded);
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);
            CampaignEvents.DailyTickHeroEvent.AddNonSerializedListener(this, OnDailyTickHeroEvent);
        }

        private void OnCreationEnded()
        {
            
        }

        private void OnGameLoaded(CampaignGameStarter starter)
        {

        }

        private void OnDailyTickHeroEvent(Hero hero)
        {
            bool notable = hero.IsNotable;
            bool clan = hero.Clan != null;
            bool leader = clan && hero.Clan.Leader != hero;
            bool clanMember = clan && !leader;
            foreach (Goal goal in DefaultGoals.Instance.All)
            {
                bool run = false;
                if (notable && goal.TickNotables) run = true;
                else if (clanMember && goal.TickClanMembers) run = true;
                else if (leader && goal.TickClanLeaders) run = true;

                if (run) goal.GetCopy(hero).DoAiDecision();
            }
        }
    }
}