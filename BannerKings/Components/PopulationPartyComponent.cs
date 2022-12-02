using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Components
{
    public class PopulationPartyComponent : PartyComponent
    {
        public PopulationPartyComponent(Settlement target, Settlement origin, string name, bool slaveCaravan,
            PopType popType, bool trading = false)
        {
            Target = target;
            nameString = name;
            Origin = origin;
            SlaveCaravan = slaveCaravan;
            PopulationType = popType;
            Trading = trading;
        }

        [SaveableProperty(1)] protected Settlement Target { get; set; }

        [SaveableProperty(2)] protected Settlement Origin { get; set; }

        [SaveableProperty(3)] private string nameString { get; set; }

        [SaveableProperty(4)] public bool SlaveCaravan { get; private set; }

        [SaveableProperty(5)] public PopType PopulationType { get; private set; }

        [SaveableProperty(6)] public bool Trading { get; private set; }

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
            }
        }

        public override Hero PartyOwner => HomeSettlement.OwnerClan.Leader;

        public override TextObject Name => new TextObject(nameString)
            .SetTextVariable("ORIGIN", OriginSettlement.Name);

        public override Settlement HomeSettlement => Target;

        public Settlement OriginSettlement => Origin;

        private static MobileParty CreateParty(string id, Settlement origin, bool slaveCaravan, Settlement target,
            string name, PopType popType, bool trading = false)
        {
            return MobileParty.CreateParty(id + origin + target.Name,
                new PopulationPartyComponent(target, origin, name, slaveCaravan, popType, trading),
                delegate(MobileParty mobileParty)
                {
                    mobileParty.SetPartyUsedByQuest(true);
                    mobileParty.Party.Visuals.SetMapIconAsDirty();
                    mobileParty.SetInitiative(0f, 1f, float.MaxValue);
                    mobileParty.ShouldJoinPlayerBattles = false;
                    mobileParty.Aggressiveness = 0f;
                    mobileParty.Ai.DisableAi();
                    mobileParty.SetMoveGoToSettlement(target);
                });
        }

        public static void CreateSlaveCaravan(string id, Settlement origin, Settlement target, string name, int slaves)
        {
            var caravan = CreateParty(id, origin, true, target, name, PopType.None);
            caravan.AddPrisoner(CharacterObject.All.FirstOrDefault(x => x.StringId == "looter"), slaves);
            caravan.InitializeMobilePartyAtPosition(origin.Culture.EliteCaravanPartyTemplate, origin.GatePosition);
            GiveMounts(ref caravan);
            GiveFood(ref caravan);
            BannerKingsConfig.Instance.PopulationManager.AddParty(caravan);
        }

        public static MobileParty CreateTravellerParty(string id, Settlement origin, Settlement target, string name, int count,
            PopType type, CharacterObject civilian, bool trading = false)
        {
            var party = CreateParty(id, origin, false, target, name, type, trading);
            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(origin);
            data.UpdatePopType(type, count);
            var roster = new TroopRoster(party.Party);
            roster.AddToCounts(civilian, count);
            switch (type)
            {
                case PopType.Serfs:
                {
                    if (origin.Culture.MilitiaPartyTemplate != null)
                    {
                        foreach (var stack in origin.Culture.MilitiaPartyTemplate.Stacks)
                        {
                            var soldier = stack.Character;
                            if (soldier != null)
                            {
                                roster.AddToCounts(soldier,
                                    GetCountToAdd(roster.TotalRegulars, soldier.Tier, soldier.IsRanged));
                            }
                        }
                    }

                    break;
                }
                case PopType.Craftsmen:
                {
                    if (origin.Culture.CaravanPartyTemplate != null)
                    {
                        foreach (var stack in origin.Culture.MilitiaPartyTemplate.Stacks)
                        {
                            var soldier = stack.Character;
                            if (soldier != null)
                            {
                                roster.AddToCounts(soldier,
                                    GetCountToAdd(roster.TotalRegulars, soldier.Tier, soldier.IsRanged));
                            }
                        }
                    }

                    break;
                }
                case PopType.Nobles:
                {
                    var template = MBObjectManager.Instance.GetObjectTypeList<PartyTemplateObject>()
                        .FirstOrDefault(x => x.StringId == "populations_mercenary_generic_elite");
                    if (template != null)
                    {
                        foreach (var stack in template.Stacks)
                        {
                            var soldier = stack.Character;
                            if (soldier != null)
                            {
                                roster.AddToCounts(soldier,
                                    GetCountToAdd(roster.TotalRegulars, soldier.Tier, soldier.IsRanged));
                            }
                        }
                    }

                    break;
                }
            }

            party.InitializeMobilePartyAroundPosition(roster, new TroopRoster(party.Party), origin.GatePosition, 1f);
            if (!trading)
            {
                GivePackAnimals(ref party);
                GiveFood(ref party);
                GiveItems(ref party, type);
            }

            BannerKingsConfig.Instance.PopulationManager.AddParty(party);
            return party;
        }

        private static int GetCountToAdd(int partySize, int tier, bool ranged)
        {
            return (int) (partySize / (float) (tier + (ranged ? 3 : 2))) + MBRandom.RandomInt(-2, 3);
        }

        protected static void GiveMounts(ref MobileParty party)
        {
            var lacking = party.Party.NumberOfRegularMembers - party.Party.NumberOfMounts;
            var horse = Items.All.FirstOrDefault(x => x.StringId == "sumpter_horse");
            party.ItemRoster.AddToCounts(horse, lacking);
        }

        protected static void GivePackAnimals(ref MobileParty party)
        {
            ItemObject itemObject = null;
            foreach (var itemObject2 in Items.All)
            {
                if (itemObject2.ItemCategory == DefaultItemCategories.PackAnimal && !itemObject2.NotMerchandise)
                {
                    itemObject = itemObject2;
                }
            }

            if (itemObject != null)
            {
                party.ItemRoster.Add(new ItemRosterElement(itemObject, (int) (party.Party.NumberOfAllMembers * 0.25f)));
            }
        }

        public static void GiveFood(ref MobileParty party)
        {
            foreach (var itemObject in Items.All)
            {
                if (itemObject.IsFood)
                {
                    var num2 = MBRandom.RoundRandomized(party.Party.NumberOfAllMembers *
                                                        (1f / itemObject.Value) * 16 * MBRandom.RandomFloat *
                                                        MBRandom.RandomFloat * MBRandom.RandomFloat * MBRandom.RandomFloat);
                    if (num2 > 0)
                    {
                        party.ItemRoster.AddToCounts(itemObject, num2);
                    }
                }
            }
        }

        protected static void GiveItems(ref MobileParty party, PopType type)
        {
            var partySize = party.Party.NumberOfAllMembers;
            var totalValue = 0;
            var valueMax = partySize * (type == PopType.Serfs ? 30 : type == PopType.Craftsmen ? 100 : 300);

            while (party.ItemRoster.TotalWeight < party.InventoryCapacity && totalValue < valueMax)
            {
                if (type == PopType.Craftsmen)
                {
                    var list = new List<ValueTuple<ItemObject, float>>();
                    foreach (var material in Materials)
                    {
                        var item = Campaign.Current.Models.SmithingModel.GetCraftingMaterialItem(material);
                        list.Add(new ValueTuple<ItemObject, float>(item, item.Value * MBRandom.RandomFloat));
                    }

                    var materialItem = MBRandom.ChooseWeighted(list);
                    totalValue += materialItem.Value;
                    party.ItemRoster.AddToCounts(materialItem, 1);
                }

                var goods = new List<ValueTuple<ItemObject, float>>();
                foreach (var item in Items.AllTradeGoods)
                {
                    if (item.StringId == "stolen_goods")
                    {
                        continue;
                    }

                    switch (type)
                    {
                        case PopType.Nobles:
                        {
                            if (item.StringId is "silver" or "jewelry" or "spice" or "velvet" or "fur")
                            {
                                goods.Add(new ValueTuple<ItemObject, float>(item, 1f * (10f / partySize) / item.Value));
                            }

                            break;
                        }
                        case PopType.Craftsmen:
                        {
                            if (item.StringId is "wool" or "pottery" or "cotton" or "flax" or "linen" or "leather" or "tools")
                            {
                                goods.Add(new ValueTuple<ItemObject, float>(item, 1f * (10f / partySize) / item.Value));
                            }

                            break;
                        }
                    }

                    goods.Add(new ValueTuple<ItemObject, float>(item, 1f / item.Value));
                }

                var good = MBRandom.ChooseWeighted(goods);
                totalValue += good.Value;
                party.ItemRoster.AddToCounts(good, 1);
            }
        }
    }
}