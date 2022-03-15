using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Populations.Villages
{
    public class DefaultVillageBuildings
    {
        private BuildingType manor, palisade, trainning, bakery, mining, courier,
            animalHousing, farming, sawmill, butter, tannery, fishing, blacksmith,
            warehouse, dailyProduction, dailyFarm, dailyPasture, dailyWoods;

        public BuildingType Manor => this.manor;
        public BuildingType Palisade => this.palisade;
        public BuildingType TrainningGrounds => this.trainning;
        public BuildingType Warehouse => this.warehouse;
        public BuildingType Courier => this.courier;
        public BuildingType Bakery => this.bakery;
        public BuildingType Mining => this.mining;
        public BuildingType Farming => this.farming;
        public BuildingType Sawmill => this.sawmill;
        public BuildingType AnimalHousing => this.animalHousing;
        public BuildingType Butter => this.butter;
        public BuildingType FishFarm => this.fishing;
        public BuildingType Tannery => this.tannery;
        public BuildingType Blacksmith => this.blacksmith;
        public BuildingType DailyProduction => this.dailyProduction;
        public BuildingType DailyFarm => this.dailyFarm;
        public BuildingType DailyPasture => this.dailyPasture;
        public BuildingType DailyWoods => this.dailyWoods;

        public static DefaultVillageBuildings Instance => ConfigHolder.CONFIG;
        
        internal struct ConfigHolder
        {
            public static DefaultVillageBuildings CONFIG = new DefaultVillageBuildings();
        }

        public void Init()
        {
            this.manor = new BuildingType("bannerkings_manor");
            this.manor.Initialize(new TextObject("{=!}Manor"), 
                new TextObject("{=!}Manor house, the lord's home and center of the village. A manor house allows the housing of a small retinue in the village (15, 30, 45 men). Increases influence from nobles (15%, 30%, 50%)."), new int[]
            {
                4000,
                6000,
                8000
            }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
            {
   
            }, 0);

            this.palisade = new BuildingType("bannerkings_palisade");
            this.palisade.Initialize(new TextObject("{=!}Palisade"), 
                new TextObject("{=!}A set of wooden stakes placed around the village like a wall. Reduces raiding speed (12%, 24%, 36%)."), new int[]
            {
                3000,
                5000,
                7000
            }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
            {

            }, 0);

            this.trainning = new BuildingType("bannerkings_trainning");
            this.trainning.Initialize(new TextObject("{=!}Trainning Grounds"), 
                new TextObject("{=!}Stablish a zone dedicated for trainning, as well as it's required equipments, where locals can train basic military arts. Increases militia production (0.2, 0.5, 1.0)."), new int[]
            {
                1500,
                2400,
                3200
            }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
            {

            }, 0);

            this.warehouse = new BuildingType("bannerkings_warehouse");
            this.warehouse.Initialize(new TextObject("{=!}Arms Warehouse"), 
                new TextObject("{=!}Construct a warehouse dedicated to keep military equipment as well as provide their maintenance. Improves militia quality (4%, 8%, 12%)."), new int[]
            {
                2000,
                3000,
                4000
            }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
            {

            }, 0);

            this.courier = new BuildingType("bannerkings_courier");
            this.courier.Initialize(new TextObject("{=!}Courier Post"), 
                new TextObject("{=!}Set up a dedicate courier post that will inform you of any relevant activity in and around your demesne. Enables information messages regardless of your distance."), new int[]
            {
                1000,
                1800,
                2500
            }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
            {
   
            }, 0);

            this.bakery = new BuildingType("bannerkings_bakery");
            this.bakery.Initialize(new TextObject("{=!}Bakery"), 
                new TextObject("{=!}Supply tools and space for a local bakery, allowing serfs to turn wheat into bread. Adds bread as production good."), new int[]
            {
                1000,
                1800,
                2500
            }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
            {
  
            }, 0);

            this.mining = new BuildingType("bannerkings_mining");
            this.mining.Initialize(new TextObject("{=!}Mining Infrastructure"), 
                new TextObject("{=!}Build mining equipment and infrastructure to improve working conditions in local mines. Increases ore production (5%, 10%, 15%)."), new int[]
            {
                1500,
                2400,
                3200
            }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
            {
 
            }, 0);

            this.animalHousing = new BuildingType("bannerkings_animal_housing");
            this.animalHousing.Initialize(new TextObject("{=!}Animal Housing"), 
                new TextObject("{=!}Invest on infrastructure for animal housing and grazing, yielding more from your pasture lands. Increases live animals production (5%, 10%, 15%)."), new int[]
            {
                1500,
                2400,
                3200
            }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
            {
  
            }, 0);

            this.farming = new BuildingType("bannerkings_farming");
            this.farming.Initialize(new TextObject("{=!}Farming Infrastructure"), 
                new TextObject("{=!}Provide farming equipment and stablish systems to maximise land productivity. Increases farm goods production (5%, 10%, 15%)."), new int[]
            {
                1500,
                2400,
                3200
            }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
            {
    
            }, 0);

            this.sawmill = new BuildingType("bannerkings_sawmill");
            this.sawmill.Initialize(new TextObject("{=!}Sawmill"), 
                new TextObject("{=!}Build a sawmill, improving the speed and quality of log cutting into usable hardwood. Increases hardwood production (5%, 10%, 15%)."), new int[]
            {
                1500,
                2400,
                3200
            }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
            {

            }, 0);

            this.butter = new BuildingType("bannerkings_butter");
            this.butter.Initialize(new TextObject("{=!}Butter Mill"), 
                new TextObject("{=!}Construct specialized buildings for churning local cattle milk into butter, a highly sought after food amongst lords."), new int[]
            {
                1000,
                1800,
                2500
            }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
            {
   
            }, 0);

            this.tannery = new BuildingType("bannerkings_tannery");
            this.tannery.Initialize(new TextObject("{=!}Fur Tannery"), 
                new TextObject("{=!}Construct specialized buildings for tanning hides, turning these into leather. Adds leather to production."), new int[]
            {
                1500,
                2400,
                3200
            }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
            {
    
            }, 0);

            this.fishing = new BuildingType("bannerkings_fishing");
            this.fishing.Initialize(new TextObject("{=!}Fish Farm"), 
                new TextObject("{=!}Build controlled fish growing zones, supplying extra fish to the village. Increases fish production (5%, 10%, 15%)."), new int[]
            {
                1500,
                2400,
                3200
            }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
            {
           
            }, 0);

            this.blacksmith = new BuildingType("bannerkings_blacksmith");
            this.blacksmith.Initialize(new TextObject("{=!}Smith"), 
                new TextObject("{=!}Stablish a local blacksmith, supplying the village with metal products. Adds tools to production."), new int[]
            {
                1000,
                1800,
                2500
            }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
            {
           
            }, 0);


            this.dailyProduction = new BuildingType("bannerkings_daily_production");
            this.dailyProduction.Initialize(new TextObject("{=!}Production", null), new TextObject("{=!}Focus the population's effort in productivity, not expanding land or changing it's type. While this option is not active, productivity output is reduced.", null), new int[3], BuildingLocation.Daily, new Tuple<BuildingEffectEnum, float, float, float>[]
            {

            }, 0);

            this.dailyFarm = new BuildingType("bannerkings_daily_farm");
            this.dailyFarm.Initialize(new TextObject("{=!}Farmland", null), new TextObject("{=!}Focus efforts on creating new arable acres, used for farming purposes. More farming acres increase output of farming goods.", null), new int[3], BuildingLocation.Daily, new Tuple<BuildingEffectEnum, float, float, float>[]
            {

            }, 0);

            this.dailyPasture = new BuildingType("bannerkings_daily_pasture");
            this.dailyPasture.Initialize(new TextObject("{=!}Pastureland", null), new TextObject("{=!}Focus efforts on creating new acres of pasture, where cattle and animals graze and thrive. More pasture acres increase output of animals and animal products.", null), new int[3], BuildingLocation.Daily, new Tuple<BuildingEffectEnum, float, float, float>[]
            {

            }, 0);

            this.dailyWoods = new BuildingType("bannerkings_daily_woods");
            this.dailyWoods.Initialize(new TextObject("{=!}Woodland", null), new TextObject("{=!}Focus efforts on turning acres into woodlands. Acres of woodland allow more higher yields of logs and berries, as well as help the land not becoming overfarmed or grazed.", null), new int[3], BuildingLocation.Daily, new Tuple<BuildingEffectEnum, float, float, float>[]
            {

            }, 0);
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
            yield break;
            
        }
    }
}
