using BannerKings.Managers.Buildings;
using BannerKings.Managers.Populations.Villages;
using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;

namespace BannerKings.Behaviours
{
    public class BKBuildingsBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);
        }

        public override void SyncData(IDataStore dataStore)
        {

        }

        private void OnGameLoaded(CampaignGameStarter starter)
        {
            if (BannerKingsConfig.Instance.PopulationManager == null)
            {
                return;
            }

            foreach (var settlement in Settlement.All)
            {
                if (settlement.Town != null)
                {
                    var buildings = settlement.Town.Buildings;
                    foreach (var type in BKBuildings.Instance.All)
                    {
                        if (settlement.IsTown && type.BuildingLocation == BuildingLocation.Settlement &&
                            buildings.FirstOrDefault(x => x.BuildingType == type) == null)
                        {
                            buildings.Add(new Building(type, settlement.Town));
                        }
                        else if (settlement.IsCastle && type.BuildingLocation == BuildingLocation.Castle &&
                            buildings.FirstOrDefault(x => x.BuildingType == type) == null)
                        {
                            buildings.Add(new Building(type, settlement.Town));
                        }
                    }
                }
            }
        }
    }
}
