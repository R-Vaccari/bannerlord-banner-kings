using BannerKings.Managers.Innovations;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Behaviours.Innovations
{
    internal class BKTroopAdvancementBehavior : BannerKingsBehavior
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnGameLoadFinishedEvent.AddNonSerializedListener(this, OnGameLoaded);
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void OnGameLoaded()
        {
            foreach (var culture in Campaign.Current.ObjectManager.GetObjectTypeList<CultureObject>())
            {
                InnovationData data = BannerKingsConfig.Instance.InnovationsManager.GetInnovationData(culture);
                if (data == null) continue;

                data.Era.TriggerEra(culture);
            }
        }
    }
}
