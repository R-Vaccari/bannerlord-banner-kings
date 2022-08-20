using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Populations.Villages
{
    public class DefaultVillageBuildings : DefaultTypeInitializer<DefaultVillageBuildings, BuildingType>
    {
        private BuildingType manor, palisade, trainning, bakery, mining, courier,
            animalHousing, farming, sawmill, butter, tannery, fishing, blacksmith,
            warehouse, dailyProduction, dailyFarm, dailyPasture, dailyWoods;

        public BuildingType Manor => manor;
        public BuildingType Palisade => palisade;
        public BuildingType TrainningGrounds => trainning;

        internal BuildingType GetById(BuildingType buildingType)
        {
            throw new NotImplementedException();
        }

        public BuildingType Warehouse => warehouse;
        public BuildingType Courier => courier;
        public BuildingType Bakery => bakery;
        public BuildingType Mining => mining;
        public BuildingType Farming => farming;
        public BuildingType Sawmill => sawmill;
        public BuildingType AnimalHousing => animalHousing;
        public BuildingType Butter => butter;
        public BuildingType FishFarm => fishing;
        public BuildingType Tannery => tannery;
        public BuildingType Blacksmith => blacksmith;
        public BuildingType DailyProduction => dailyProduction;
        public BuildingType DailyFarm => dailyFarm;
        public BuildingType DailyPasture => dailyPasture;
        public BuildingType DailyWoods => dailyWoods;



        public override void Initialize()
        {
            manor = new BuildingType("bannerkings_manor");
            Game.Current.ObjectManager.RegisterPresumedObject(manor);
            manor.Initialize(new TextObject("{=!}Manor"), 
                new TextObject("{=!}Manor house, the lord's home and center of the village. A manor house allows the housing of a small retinue in the village (15, 30, 45 men). Increases influence from nobles (15%, 30%, 50%)."), 
                new[]
                {
                    4000,
                    6000,
                    8000
                }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
                {
   
                });

            palisade = new BuildingType("bannerkings_palisade");
            Game.Current.ObjectManager.RegisterPresumedObject(palisade);
            palisade.Initialize(new TextObject("{=!}Palisade"), 
                new TextObject("{=!}A set of wooden stakes placed around the village like a wall. Reduces raiding speed (12%, 24%, 36%)."), 
                new[]
                {
                    3000,
                    5000,
                    7000
                }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
                {

                });

            trainning = new BuildingType("bannerkings_trainning");
            Game.Current.ObjectManager.RegisterPresumedObject(trainning);
            trainning.Initialize(new TextObject("{=!}Trainning Grounds"), 
                new TextObject("{=!}Stablish a zone dedicated for trainning, as well as it's required equipments, where locals can train basic military arts. Increases militia production (0.2, 0.5, 1.0)."), 
                new[]
                {
                    1500,
                    2400,
                    3200
                }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
                {

                });

            warehouse = new BuildingType("bannerkings_warehouse");
            Game.Current.ObjectManager.RegisterPresumedObject(warehouse);
            warehouse.Initialize(new TextObject("{=!}Arms Warehouse"), 
                new TextObject("{=!}Construct a warehouse dedicated to keep military equipment as well as provide their maintenance. Improves militia quality (4%, 8%, 12%)."), 
                new[]
                {
                    2000,
                    3000,
                    4000
                }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
                {

                });

            Game.Current.ObjectManager.RegisterPresumedObject(new BuildingType("bannerkings_warehouse"));

            courier = new BuildingType("bannerkings_courier");
            courier.Initialize(new TextObject("{=!}Courier Post"), 
                new TextObject("{=!}Set up a dedicate courier post that will inform you of any relevant activity in and around your demesne. Enables information messages regardless of your distance."), new[]
            {
                1000,
                1800,
                2500
            }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
            {
   
            });

            bakery = new BuildingType("bannerkings_bakery");
            bakery.Initialize(new TextObject("{=!}Bakery"), 
                new TextObject("{=!}Supply tools and space for a local bakery, allowing serfs to turn wheat into bread. Adds bread as production good."), new[]
            {
                1000,
                1800,
                2500
            }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
            {
  
            });

            mining = new BuildingType("bannerkings_mining");
            Game.Current.ObjectManager.RegisterPresumedObject(mining);
            mining.Initialize(new TextObject("{=!}Mining Infrastructure"), 
                new TextObject("{=!}Build mining equipment and infrastructure to improve working conditions in local mines. Increases ore production (5%, 10%, 15%)."), new[]
            {
                1500,
                2400,
                3200
            }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
            {
 
            });

            animalHousing = new BuildingType("bannerkings_animal_housing");
            Game.Current.ObjectManager.RegisterPresumedObject(animalHousing);
            animalHousing.Initialize(new TextObject("{=!}Animal Housing"), 
                new TextObject("{=!}Invest on infrastructure for animal housing and grazing, yielding more from your pasture lands. Increases live animals production (5%, 10%, 15%)."), new[]
            {
                1500,
                2400,
                3200
            }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
            {
  
            });

            farming = new BuildingType("bannerkings_farming");
            Game.Current.ObjectManager.RegisterPresumedObject(farming);
            farming.Initialize(new TextObject("{=!}Farming Infrastructure"), 
                new TextObject("{=!}Provide farming equipment and stablish systems to maximise land productivity. Increases farm goods production (5%, 10%, 15%)."), new[]
            {
                1500,
                2400,
                3200
            }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
            {
    
            });

            sawmill = new BuildingType("bannerkings_sawmill");
            Game.Current.ObjectManager.RegisterPresumedObject(sawmill);
            sawmill.Initialize(new TextObject("{=!}Sawmill"), 
                new TextObject("{=!}Build a sawmill, improving the speed and quality of log cutting into usable hardwood. Increases hardwood production (5%, 10%, 15%)."), new[]
            {
                1500,
                2400,
                3200
            }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
            {

            });

            butter = new BuildingType("bannerkings_butter");
            butter.Initialize(new TextObject("{=!}Butter Mill"), 
                new TextObject("{=!}Construct specialized buildings for churning local cattle milk into butter, a highly sought after food amongst lords."), new[]
            {
                1000,
                1800,
                2500
            }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
            {
   
            });

            tannery = new BuildingType("bannerkings_tannery");
            tannery.Initialize(new TextObject("{=!}Fur Tannery"), 
                new TextObject("{=!}Construct specialized buildings for tanning hides, turning these into leather. Adds leather to production."), new[]
            {
                1500,
                2400,
                3200
            }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
            {
    
            });

            fishing = new BuildingType("bannerkings_fishing");
            fishing.Initialize(new TextObject("{=!}Fish Farm"), 
                new TextObject("{=!}Build controlled fish growing zones, supplying extra fish to the village. Increases fish production (5%, 10%, 15%)."), new[]
            {
                1500,
                2400,
                3200
            }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
            {
           
            });

            blacksmith = new BuildingType("bannerkings_blacksmith");
            blacksmith.Initialize(new TextObject("{=!}Smith"), 
                new TextObject("{=!}Stablish a local blacksmith, supplying the village with metal products. Adds tools to production."), new[]
            {
                1000,
                1800,
                2500
            }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
            {
           
            });


            dailyProduction = new BuildingType("bannerkings_daily_production");
            Game.Current.ObjectManager.RegisterPresumedObject(dailyProduction);
            dailyProduction.Initialize(new TextObject("{=!}Production"), new TextObject("{=!}Focus the population's effort in productivity, not expanding land or changing it's type. While this option is not active, productivity output is reduced."), new int[3], BuildingLocation.Daily, new Tuple<BuildingEffectEnum, float, float, float>[]
            {

            });

            dailyFarm = new BuildingType("bannerkings_daily_farm");
            Game.Current.ObjectManager.RegisterPresumedObject(dailyFarm);
            dailyFarm.Initialize(new TextObject("{=!}Farmland"), new TextObject("{=!}Focus efforts on creating new arable acres, used for farming purposes. More farming acres increase output of farming goods."), new int[3], BuildingLocation.Daily, new Tuple<BuildingEffectEnum, float, float, float>[]
            {

            });

            dailyPasture = new BuildingType("bannerkings_daily_pasture");
            Game.Current.ObjectManager.RegisterPresumedObject(dailyPasture);
            dailyPasture.Initialize(new TextObject("{=!}Pastureland"), new TextObject("{=!}Focus efforts on creating new acres of pasture, where cattle and animals graze and thrive. More pasture acres increase output of animals and animal products."), new int[3], BuildingLocation.Daily, new Tuple<BuildingEffectEnum, float, float, float>[]
            {

            });

            dailyWoods = new BuildingType("bannerkings_daily_woods");
            Game.Current.ObjectManager.RegisterPresumedObject(dailyWoods);
            dailyWoods.Initialize(new TextObject("{=!}Woodland"), new TextObject("{=!}Focus efforts on turning acres into woodlands. Acres of woodland allow more higher yields of logs and berries, as well as help the land not becoming overfarmed or grazed."), new int[3], BuildingLocation.Daily, new Tuple<BuildingEffectEnum, float, float, float>[]
            {

            });
        }

        public override IEnumerable<BuildingType> All
        { 
            get
            {
                yield return Instance.Manor;
                yield return Instance.Palisade;
                yield return Instance.TrainningGrounds;
                yield return Instance.Warehouse;
                yield return Instance.DailyProduction;
                yield return Instance.DailyFarm;
                yield return Instance.DailyPasture;
                yield return Instance.DailyWoods;
                yield return Instance.Farming;
                yield return Instance.Mining;
                yield return Instance.AnimalHousing;
                yield return Instance.Sawmill;
                yield return Instance.FishFarm;
                yield return Instance.Tannery;
                yield return Instance.Blacksmith;
            }
            
        }

        public static IEnumerable<BuildingType> VillageBuildings(Village village)
        {
            VillageType type = village.VillageType;
            yield return Instance.Manor;
            yield return Instance.Palisade;
            yield return Instance.TrainningGrounds;
            yield return Instance.Warehouse;
            yield return Instance.DailyProduction;
            yield return Instance.DailyFarm;
            yield return Instance.DailyPasture;
            yield return Instance.DailyWoods;
            if (type == DefaultVillageTypes.WheatFarm || type == DefaultVillageTypes.DateFarm || type == DefaultVillageTypes.FlaxPlant ||
                type == DefaultVillageTypes.SilkPlant || type == DefaultVillageTypes.OliveTrees || type == DefaultVillageTypes.VineYard)
                yield return Instance.Farming;
            else if (type == DefaultVillageTypes.SilverMine || type == DefaultVillageTypes.IronMine || type == DefaultVillageTypes.SaltMine ||
                type == DefaultVillageTypes.ClayMine)
                yield return Instance.Mining;
            else if (type == DefaultVillageTypes.CattleRange || type == DefaultVillageTypes.HogFarm || type == DefaultVillageTypes.SheepFarm ||
                type == DefaultVillageTypes.BattanianHorseRanch || type == DefaultVillageTypes.DesertHorseRanch || type == DefaultVillageTypes.EuropeHorseRanch
                || type == DefaultVillageTypes.SteppeHorseRanch || type == DefaultVillageTypes.SturgianHorseRanch || type == DefaultVillageTypes.VlandianHorseRanch)
                yield return Instance.AnimalHousing;
            else if (type == DefaultVillageTypes.Lumberjack)
                yield return Instance.Sawmill;
            else if (type == DefaultVillageTypes.Fisherman)
                yield return Instance.FishFarm;
            else yield return Instance.Tannery;

            if (type == DefaultVillageTypes.IronMine)
                yield return Instance.Blacksmith;
        }
    }
}
