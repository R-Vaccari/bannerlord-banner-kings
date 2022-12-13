using Helpers;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using HarmonyLib;
using TaleWorlds.SaveSystem;

namespace BannerKings.Behaviours.Workshops
{
    public class WorkshopData
    {
        public WorkshopData(Workshop workshop)
        {
            Workshop = workshop;
            Inventory = new ItemRoster();
        }
    
        [SaveableProperty(1)] public Workshop Workshop { get; private set; }
        [SaveableProperty(2)] public ItemRoster Inventory { get; private set; }
        [SaveableProperty(3)] public bool IsRunningOnInventory { get; set; }

        public void Tick()
        {
            IsRunningOnInventory = false;
            var town = Workshop.Settlement.Town;
            if (Workshop.IsRunning && Workshop.Capital > 10000 && Workshop.WorkshopType.StringId != "artisans")
            {
                var inputs = GetAvailableInputs();
                int maxCapacity = GetInventoryCapacity();
                int currentCapacity = 0;
                foreach (var element in Inventory)
                {
                    currentCapacity += element.Amount;
                }

                foreach (var input in inputs)
                {
                    if (currentCapacity < maxCapacity)
                    {
                        int price = town.GetItemPrice(input);
                        if ((Workshop.Capital - Workshop.InitialCapital) > price)
                        {
                            Workshop.ChangeGold(-price);
                            town.ChangeGold(price);
                            town.Owner.ItemRoster.AddToCounts(input, -1);
                            Inventory.AddToCounts(input, 1);
                        }
                    }
                    else break;
                }
            }
            else if (Workshop.NotRunnedDays > 0 && Workshop.IsRunning)
            {
                IsRunningOnInventory = true;
                for (int i = 0; i < Workshop.WorkshopType.Productions.Count; i++)
                {
                    var production = Workshop.WorkshopType.Productions[i];
                    bool runningPossible = HasEnoughInputs(production);

                    if (!runningPossible)
                    {
                        continue;
                    }

                    float policyEffectToProduction = Campaign.Current.Models.WorkshopModel.GetPolicyEffectToProduction(town);
                    ExplainedNumber explainedNumber = new ExplainedNumber(production.ConversionSpeed * policyEffectToProduction, 
                        false, null);

                    if (town.Governor != null && town.Governor.GetPerkValue(DefaultPerks.Trade.MercenaryConnections))
                    {
                        PerkHelper.AddPerkBonusForTown(DefaultPerks.Trade.MercenaryConnections, town, ref explainedNumber);
                    }

                    if (town.Owner != null)
                    {
                        PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Steward.Sweatshops, Workshop.Owner.CharacterObject, 
                            true, ref explainedNumber);
                    }

                    float num = Workshop.ProductionProgress[i];
                    if (num > 1f)
                    {
                        num = 1f;
                    }

                    num += explainedNumber.ResultNumber;
                    if (num >= 1f)
                    {
                        bool isRunning = true;
                        while (isRunning && num >= 1f)
                        {
                            isRunning = DoProduction(production);
                            num -= 1f;

                            if (isRunning)
                            {
                                Workshop.ResetNotRunnedDays();
                            }
                        }
                    }
                    Workshop.SetProgress(i, num);
                }
            }
        }

        private bool DoProduction(WorkshopType.Production production) 
        {
            if (HasEnoughInputs(production))
            {
                foreach (var input in production.Inputs)
                {
                    var category = input.Item1;
                    int totalToConsume = input.Item2;
                    foreach (var element in Inventory)
                    {
                        if (totalToConsume <= 0)
                        {
                            break;
                        }

                        if (element.EquipmentElement.Item.ItemCategory == category)
                        {
                            int toConsume = Math.Min(element.Amount, totalToConsume);
                            totalToConsume -= toConsume;
                            Inventory.AddToCounts(element.EquipmentElement, -toConsume);
                        }
                    }
                }

                Town town = Workshop.Settlement.Town;
                WorkshopsCampaignBehavior vanillaBehavior = Campaign.Current.GetCampaignBehavior<WorkshopsCampaignBehavior>();
                foreach (var output in production.Outputs)
                {
                    ItemCategory category = output.Item1;
                    EquipmentElement element = (EquipmentElement)AccessTools.Method(vanillaBehavior.GetType(), "GetRandomItem")
                    .Invoke(vanillaBehavior, new object[] { category, town });

                    if (element.Item != null)
                    {
                        float count = output.Item2;
                        int itemPrice = town.GetItemPrice(element, null, false);
                        town.Owner.ItemRoster.AddToCounts(element, (int)count);
                        if (Campaign.Current.GameStarted)
                        {
                            int num = (int)(MathF.Min(1000, itemPrice) * count);
                            Workshop.ChangeGold(num);
                            town.ChangeGold(-num);
                        }
                        CampaignEventDispatcher.Instance.OnItemProduced(element.Item, town.Owner.Settlement, (int)count);
                    }
                }

                return true;
            }
            else return false;
        }

        public int GetInventoryCount()
        {
            int count = 0;
            foreach (var element in Inventory)
            {
                count += element.Amount;
            }

            return count;
        }

        public int GetInventoryCapacity() => (int)(Workshop.Level * 20f);

        private bool HasEnoughInputs(WorkshopType.Production production)
        {
            bool enough = true;
            foreach (var input in production.Inputs)
            {
                var category = input.Item1;
                int currentCount = 0;
                foreach (var element in Inventory)
                {
                    if (element.EquipmentElement.Item.ItemCategory == category)
                    {
                        currentCount += element.Amount;
                        if (currentCount >= input.Item2) break;
                    }
                }

                if (currentCount < input.Item2)
                {
                    enough = false;
                }
            }

            return enough;
        }

        public List<ItemObject> GetAvailableInputs()
        {
            var list = new List<ItemObject>();
            ItemRoster itemRoster = Workshop.Settlement.Town.Owner.ItemRoster;
            foreach (var production in Workshop.WorkshopType.Productions)
            {
                foreach (ValueTuple<ItemCategory, int> valueTuple in production.Inputs)
                {
                    ItemCategory category = valueTuple.Item1;
                    for (int i = 0; i < itemRoster.Count; i++)
                    {
                        ItemObject itemAtIndex = itemRoster.GetItemAtIndex(i);
                        if (itemAtIndex.ItemCategory == category)
                        {
                            list.Add(itemAtIndex);
                            break;
                        }
                    }
                }
            }

            return list;
        }
    }
}
