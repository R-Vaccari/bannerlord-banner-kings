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

        public override IEnumerable<VillageType> All => throw new NotImplementedException();
 
        public override void Initialize()
        {
            SpiceFarm = Game.Current.ObjectManager.RegisterPresumedObject(new VillageType("SpiceFarm"));
            SpiceFarm.Initialize(new TextObject("{=!}Spice Farm"),
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
        }

        private void AddProductions(VillageType villageType, ValueTuple<string, float>[] productions)
        {
            villageType.AddProductions(from p in productions
                                       select new ValueTuple<ItemObject, float>(Game.Current.ObjectManager.GetObject<ItemObject>(p.Item1), p.Item2));
        }
    }
}
