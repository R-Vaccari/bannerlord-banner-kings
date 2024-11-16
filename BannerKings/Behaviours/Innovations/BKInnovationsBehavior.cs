using BannerKings.Managers.Innovations;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Behaviours.Innovations
{
    public class BKInnovationsBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
            CampaignEvents.OnGameLoadFinishedEvent.AddNonSerializedListener(this, () =>
            {
                foreach (var culture in Campaign.Current.ObjectManager.GetObjectTypeList<CultureObject>())
                {
                    InnovationData data = BannerKingsConfig.Instance.InnovationsManager.GetInnovationData(culture);
                    if (data == null) continue;

                    data.Era.TriggerEra(culture);
                }
            });
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