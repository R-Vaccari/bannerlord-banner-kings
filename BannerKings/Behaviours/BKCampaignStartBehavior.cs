using BannerKings.UI;
using System;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Behaviours
{
    public class BKCampaignStartBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnCharacterCreationIsOverEvent.AddNonSerializedListener(this, new Action(OnCharacterCreationOver));
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void OnCharacterCreationOver()
        {
            UIManager.Instance.ShowWindow("campaignStart");
        }
    }
}
