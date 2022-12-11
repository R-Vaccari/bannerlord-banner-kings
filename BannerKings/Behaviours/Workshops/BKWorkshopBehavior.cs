using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;

namespace BannerKings.Behaviours.Workshops
{
    public class BKWorkshopBehavior : CampaignBehaviorBase
    {
        private Dictionary<Workshop, WorkshopData> inventories = new Dictionary<Workshop, WorkshopData>();

        public WorkshopData GetInventory(Workshop wk)
        {
            if (inventories.ContainsKey(wk))
            {
                return inventories[wk];
            }

            return null;
        }

        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickTownEvent.AddNonSerializedListener(this, OnTownDailyTick);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("bannerkings-workshop-inventories", ref inventories);

            if (inventories == null)
            {
                inventories = new Dictionary<Workshop, WorkshopData>();
            }
        }

        private void OnTownDailyTick(Town town)
        {
            foreach (var workshop in town.Workshops)
            {
                AddInventory(workshop);
                inventories[workshop].Tick();
            }
        }

        private void AddInventory(Workshop workshop)
        {
            if (!inventories.ContainsKey(workshop))
            {
                inventories.Add(workshop, new WorkshopData(workshop));
            }
        }
    }
}

