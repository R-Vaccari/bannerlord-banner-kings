using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Campaign
{
    public class BKVillageTypes : DefaultTypeInitializer<BKVillageTypes, VillageType>
    {
        public VillageType SpiceFarm { get; set; }
        public VillageType Limestone { get; set; }
        public VillageType Marble { get; set; }
        public VillageType Whale { get; set; }
        public VillageType Garum { get; set; }
        public VillageType Papyrus { get; set; }
        public VillageType PurpleDye { get; set; }

        public override IEnumerable<VillageType> All => throw new NotImplementedException();

        public override void Initialize()
        {
            SpiceFarm = Game.Current.ObjectManager.RegisterPresumedObject(new VillageType("SpiceFarm"));
            SpiceFarm.Initialize(new TextObject("{=JnKofObL}Spice Farm"),
                "spice_sack",
                "silk_plant_ucon",
                "silk_plant_burned",
                new ValueTuple<ItemObject, float>[]
                {
                    new ValueTuple<ItemObject, float>(DefaultItems.Grain, 3f)
                });

            AddProductions(SpiceFarm,
                new ValueTuple<string, float>[]
                {
                    new ValueTuple<string, float>("spice", 4f)
                });

            Papyrus = Game.Current.ObjectManager.RegisterPresumedObject(new VillageType("Papyrus"));
            Papyrus.Initialize(new TextObject("{=RVm9TTU6}Damarian Farm"),
                "wheat_farm", "wheat_farm_ucon", "wheat_farm_burned",
                new ValueTuple<ItemObject, float>[]
                {
                    new ValueTuple<ItemObject, float>(DefaultItems.Grain, 50f)
                });

            AddProductions(Papyrus,
                new ValueTuple<string, float>[]
                {
                    new ValueTuple<string, float>("cow", 0.2f),
                    new ValueTuple<string, float>("sheep", 0.4f),
                    new ValueTuple<string, float>("Papyrus", 5f)
                });

            Whale = Game.Current.ObjectManager.RegisterPresumedObject(new VillageType("Whale"));
            Whale.Initialize(new TextObject("{=T0oGcX47}Whaler"),
                "fisherman", "fisherman_ucon", "fisherman_burned",
                new ValueTuple<ItemObject, float>[]
                {
                    new ValueTuple<ItemObject, float>(DefaultItems.Grain, 3f)
                });

            AddProductions(Whale,
                new ValueTuple<string, float>[]
                {
                    new ValueTuple<string, float>("fish", 28f),
                    new ValueTuple<string, float>("WhaleMeat", 6f)
                });

            PurpleDye = Game.Current.ObjectManager.RegisterPresumedObject(new VillageType("PurpleDye"));
            PurpleDye.Initialize(new TextObject("{=dDruSq5J}Perassic Fishers"),
                "fisherman", "fisherman_ucon", "fisherman_burned",
                new ValueTuple<ItemObject, float>[]
                {
                    new ValueTuple<ItemObject, float>(DefaultItems.Grain, 3f)
                });

            AddProductions(PurpleDye,
                new ValueTuple<string, float>[]
                {
                    new ValueTuple<string, float>("fish", 28f),
                    new ValueTuple<string, float>("PurpleDye", 0.2f)
                });

            Limestone = Game.Current.ObjectManager.RegisterPresumedObject(new VillageType("Limestone"));
            Limestone.Initialize(new TextObject("{=nSLhrHWX}Limestone Quarry"),
                "spice_sack",
                "silk_plant_ucon",
                "silk_plant_burned",
                new ValueTuple<ItemObject, float>[]
                {
                    new ValueTuple<ItemObject, float>(DefaultItems.Grain, 3f)
                });

            AddProductions(Limestone,
                new ValueTuple<string, float>[]
                {
                    new ValueTuple<string, float>("limestone", 20f)
                });

            Marble = Game.Current.ObjectManager.RegisterPresumedObject(new VillageType("Marble"));
            Marble.Initialize(new TextObject("{=4DcWaY9q}Marble Quarry"),
                "spice_sack",
                "silk_plant_ucon",
                "silk_plant_burned",
                new ValueTuple<ItemObject, float>[]
                {
                    new ValueTuple<ItemObject, float>(DefaultItems.Grain, 3f)
                });

            AddProductions(Marble,
                new ValueTuple<string, float>[]
                {
                    new ValueTuple<string, float>("marble", 8f)
                });
        }

        private void AddProductions(VillageType villageType, ValueTuple<string, float>[] productions)
        {
            villageType.AddProductions(from p in productions
                                       select new ValueTuple<ItemObject, float>(Game.Current.ObjectManager.GetObject<ItemObject>(p.Item1), p.Item2));
        }
    }
}
