using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;
using System.Linq;
using TaleWorlds.Core;
using static Populations.PopulationManager;
using TaleWorlds.SaveSystem;
using System.Collections.Generic;
using TaleWorlds.ObjectSystem;

namespace Populations.Components
{
    class PopulationPartyComponent : PartyComponent
    {
        [SaveableProperty(1)]
        public Settlement _target { get; set; }

        [SaveableProperty(2)]
        public Settlement _origin { get; set; }

        [SaveableProperty(3)]
        public string _name { get; set; }

        [SaveableProperty(4)]
        public bool slaveCaravan { get; set; }

        [SaveableProperty(5)]
        public PopType popType { get; set; }

        [SaveableProperty(6)]
        public bool supplyCaravan { get; set; }



        public PopulationPartyComponent(Settlement target, Settlement origin, string name, bool slaveCaravan, bool supplyCaravan, PopType popType) : base()
        {
            _target = target;
            _name = name;
            _origin = origin;
            this.slaveCaravan = slaveCaravan;
            this.supplyCaravan = supplyCaravan;
            this.popType = popType;
        }

        private static MobileParty CreateParty(string id, Settlement origin, bool slaveCaravan, bool supplyCaravan, Settlement target, string name, PopType popType)
        {
            return MobileParty.CreateParty(id + origin + target.Name.ToString(), new PopulationPartyComponent(target, origin, String.Format(name, origin.Name.ToString()), slaveCaravan, supplyCaravan, popType),
                delegate (MobileParty mobileParty)
            {
                mobileParty.SetPartyUsedByQuest(true);
                mobileParty.Party.Visuals.SetMapIconAsDirty();
                mobileParty.SetInititave(0f, 1f, float.MaxValue);
                mobileParty.ShouldJoinPlayerBattles = false;
                mobileParty.Aggressiveness = 0f;
                mobileParty.SetMoveGoToSettlement(target);
            });
        }

        public static void CreateSlaveCaravan(string id, Settlement origin, Settlement target, string name, int slaves)
        {
            MobileParty caravan = CreateParty(id, origin, true, false, target, name, PopType.None);
            caravan.AddPrisoner(CharacterObject.All.FirstOrDefault(x => x.StringId == "looter"), slaves);
            caravan.InitializeMobileParty(origin.Culture.EliteCaravanPartyTemplate, origin.GatePosition, 0f, 0f, -1);
            GiveMounts(ref caravan);
            GiveFood(ref caravan);
            PopulationConfig.Instance.PopulationManager.AddParty(caravan);
        }

        public static void CreateTravellerParty(string id, Settlement origin, Settlement target, string name, int count, PopType type, CharacterObject civilian)
        {
            MobileParty party = CreateParty(id, origin, false, false, target, name, type);
            PopulationData data = PopulationConfig.Instance.PopulationManager.GetPopData(origin);
            data.UpdatePopType(type, count);
            TroopRoster roster = new TroopRoster(party.Party);
            roster.AddToCounts(civilian, count);
            if (type == PopType.Serfs)
            {
                if (origin.Culture.MilitiaPartyTemplate != null)
                    foreach (PartyTemplateStack stack in origin.Culture.MilitiaPartyTemplate.Stacks)
                    {
                        CharacterObject soldier = stack.Character;
                        if (soldier != null)
                            roster.AddToCounts(soldier, GetCountToAdd(roster.TotalRegulars, soldier.Tier, soldier.IsRanged));
                    }  
                
            } else if (type == PopType.Craftsmen)
            {
                if (origin.Culture.CaravanPartyTemplate != null)
                    foreach (PartyTemplateStack stack in origin.Culture.MilitiaPartyTemplate.Stacks)
                    {
                        CharacterObject soldier = stack.Character;
                        if (soldier != null)
                            roster.AddToCounts(soldier, GetCountToAdd(roster.TotalRegulars, soldier.Tier, soldier.IsRanged));
                    }
            } else if (type == PopType.Nobles)
            {
                PartyTemplateObject template = MBObjectManager.Instance.GetObjectTypeList<PartyTemplateObject>().FirstOrDefault(x => x.StringId == "populations_mercenary_generic_elite");
                if (template != null)
                    foreach (PartyTemplateStack stack in template.Stacks)
                    {
                        CharacterObject soldier = stack.Character;
                        if (soldier != null)
                            roster.AddToCounts(soldier, GetCountToAdd(roster.TotalRegulars, soldier.Tier, soldier.IsRanged));
                    }

            }
            
            party.InitializeMobileParty(roster, new TroopRoster(party.Party), origin.GatePosition, 0f);
            GivePackAnimals(ref party);
            GiveFood(ref party);
            GiveItems(ref party, type);
            PopulationConfig.Instance.PopulationManager.AddParty(party);
        }

        private static int GetCountToAdd(int partySize, int tier, bool ranged) => (int)((float)partySize / (float)(tier + (ranged ? 3 : 2)))  + MBRandom.RandomInt(-2, 3);

        private static void GiveMounts(ref MobileParty party)
        {
            int lacking = party.Party.NumberOfRegularMembers - party.Party.NumberOfMounts;
            ItemObject horse = Items.All.FirstOrDefault(x => x.StringId == "sumpter_horse");
            party.ItemRoster.AddToCounts(horse, lacking);
        }

        private static void GivePackAnimals(ref MobileParty party)
        {
            ItemObject itemObject = null;
            foreach (ItemObject itemObject2 in Items.All)
                if (itemObject2.ItemCategory == DefaultItemCategories.PackAnimal && !itemObject2.NotMerchandise)
                    itemObject = itemObject2;

            if (itemObject != null)
                party.ItemRoster.Add(new ItemRosterElement(itemObject, (int)((float)party.MemberRoster.TotalManCount * 0.25f), null));
            
        }

        private static void GiveFood(ref MobileParty party)
        {
            foreach (ItemObject itemObject in Items.All)
            {
                if (itemObject.IsFood)
                {
                    int num2 = MBRandom.RoundRandomized((float)party.MemberRoster.TotalManCount * 
                        (1f / (float)itemObject.Value) * (float)16 * MBRandom.RandomFloat * 
                        MBRandom.RandomFloat * MBRandom.RandomFloat * MBRandom.RandomFloat);
                    if (num2 > 0)
                        party.ItemRoster.AddToCounts(itemObject, num2);  
                }
            }
        }

        private static void GiveItems(ref MobileParty party, PopType type)
        {
            int partySize = party.MemberRoster.Count;
            int totalValue = 0;
            int valueMax = party.MemberRoster.Count * (type == PopType.Serfs ? 30 : (type == PopType.Craftsmen ? 100 : 300));

            while (party.ItemRoster.TotalWeight < party.InventoryCapacity && totalValue < valueMax)
            {

                if (type == PopType.Craftsmen)
                {
                    List<ItemObject> list = new List<ItemObject>();
                    foreach (CraftingMaterials material in Materials)
                        list.Add(Campaign.Current.Models.SmithingModel.GetCraftingMaterialItem(material));

                    ItemObject materialItem = MBRandom.ChooseWeighted(list, delegate (ItemObject item)
                    {
                        return item.Value * MBRandom.RandomFloat;
                    });
                    totalValue += materialItem.Value;
                    party.ItemRoster.AddToCounts(materialItem, 1);
                }

                ItemObject good = MBRandom.ChooseWeighted(Items.AllTradeGoods, delegate (ItemObject item)
                {
                    if (item.IsFood) 
                        return 0f;

                    if (type == PopType.Nobles)
                    {
                        if (item.StringId == "silver" || item.StringId == "jewelry" || item.StringId == "spice"
                        || item.StringId == "velvet" || item.StringId == "fur") return 1f + (10f / partySize) / (float)item.Value;
                    }
                    else if (type == PopType.Craftsmen)
                    {
                        if (item.StringId == "wool" || item.StringId == "pottery" || item.StringId == "cotton" ||
                        item.StringId == "flax" || item.StringId == "linen" || item.StringId == "leather")
                            return 1f / (float)item.Value * (10f / partySize);
                    }
                    return 1f / (float)item.Value;
                });
                totalValue += good.Value;
                party.ItemRoster.AddToCounts(good, 1);
            }
        }

        private static IEnumerable<CraftingMaterials> Materials
        {
            get
            {
                yield return CraftingMaterials.Charcoal;
                yield return CraftingMaterials.Iron1;
                yield return CraftingMaterials.Iron2;
                yield return CraftingMaterials.Iron3;
                yield return CraftingMaterials.Iron4;
                yield return CraftingMaterials.Iron5;
                yield return CraftingMaterials.Iron6;
                yield return CraftingMaterials.IronOre;
                yield return CraftingMaterials.Wood;
                yield break;
            } 
        }

        public override Hero PartyOwner => HomeSettlement.OwnerClan.Leader;

        public override TextObject Name
        {
            get
            {
                return new TextObject(String.Format(_name, HomeSettlement.Name.ToString()));
            }
        }

        public override Settlement HomeSettlement
        {
            get => _target;
        }

        public Settlement OriginSettlement
        {
            get => _origin;
        }
    }
}
