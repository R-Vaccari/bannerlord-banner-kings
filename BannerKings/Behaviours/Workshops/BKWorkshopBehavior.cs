using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation;
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
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("bannerkings-workshop-inventories", ref inventories);

            if (inventories == null)
            {
                inventories = new Dictionary<Workshop, WorkshopData>();
            }
        }

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            /*starter.AddPlayerLine("workshop_notable_owner_begin_2", 
                "hero_main_options", 
                "workshop_owner_notable_multiple_response", 
                "{=LuLttpc5}I wish to buy one of your workshops.", 
                new ConversationSentence.OnConditionDelegate(this.workshop_notable_owner_begin_multiple_on_condition), 
                new ConversationSentence.OnConsequenceDelegate(this.workshop_notable_owner_answer_list_workshops_on_condition), 
                100, 
                new ConversationSentence.OnClickableConditionDelegate(this.player_war_status_clickable_condition), 
                null);*/
        }

        /*private void OnGameLoaded(CampaignGameStarter starter)
        {
            foreach (Town town in Town.AllTowns)
            {
                if (town.Workshops.Count() == 4)
                {
                    Workshop[] oldWorkshops = new Workshop[4];
                    for (int i = 0; i < town.Workshops.Count(); i++)
                    {
                        Workshop workshop = town.Workshops[i];
                        oldWorkshops[i] = workshop;
                    }

                    town.InitializeWorkshops(6);
                    for (int i = 0; i < oldWorkshops.Count(); i++)
                    {
                        town.Workshops[i] = oldWorkshops[i];
                    }
                }
            }
        }*/

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

