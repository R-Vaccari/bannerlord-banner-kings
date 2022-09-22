using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors.AiBehaviors;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.CraftingSystem;
using TaleWorlds.CampaignSystem.Party;
using static TaleWorlds.CampaignSystem.CampaignBehaviors.CraftingCampaignBehavior;

namespace BannerKings.Behaviours
{
    public class BKCraftingOrderSlots : CraftingOrderSlots
    {
        internal void RemoveTownOrder(CraftingOrder craftingOrder)
        {
            Slots[craftingOrder.DifficultyLevel] = null;
        }



    }

    public class BKCraftingBehavior : CraftingCampaignBehavior
    {
        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, DailyTick);
        }

        private Dictionary<Town, CraftingOrderSlots> _craftingOrders = new Dictionary<Town, CraftingOrderSlots>();

        private void DailyTick()
        {
            
        }

        private new void CreateTownOrder(Hero orderOwner, int orderSlot)
        {

        }

        private void ReplaceCraftingOrder(Town town, CraftingOrder order)
        {
            List<Hero> list = new List<Hero>();
            Settlement settlement = town.Settlement;
            list.AddRange(settlement.Notables);
            list.AddRange(settlement.HeroesWithoutParty);
            foreach (MobileParty party in settlement.Parties)
            {
                if (party.LeaderHero != null && !party.IsMainParty)
                {
                    list.Add(party.LeaderHero);
                }
            }

            int difficultyLevel = order.DifficultyLevel;
            //object value = _craftingOrders[town].RemoveTownOrder(order);
            if (list.Any())
            {
             //   CreateTownOrder(list.GetRandomElement(), difficultyLevel);
            }

            list = null;
        }


        public override void SyncData(IDataStore dataStore)
        {

        }
    }
}
