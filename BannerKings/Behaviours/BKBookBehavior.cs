using BannerKings.Managers.Items;
using System;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Behaviours
{
    public class BKBookBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnSessionLaunched));
        }

        public override void SyncData(IDataStore dataStore)
        {

        }

        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            BKItemCategories.Instance.Initialize();
            BKItems.Instance.Initialize();
        }
    }
}
