using BannerKings.Extensions;
using BannerKings.Managers.Innovations;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Populations.Villages
{
    public class DefaultVillageBuildings : DefaultTypeInitializer<DefaultVillageBuildings, BuildingType>
    {
        public BuildingType Manor { get; } = new BuildingType("bannerkings_manor");
        public BuildingType Palisade { get; } = new BuildingType("bannerkings_palisade");
        public BuildingType TrainningGrounds { get; } = new BuildingType("bannerkings_trainning");
        public BuildingType Warehouse { get; } = new BuildingType("bannerkings_warehouse");
        public BuildingType Courier { get; } = new BuildingType("bannerkings_courier");
        public BuildingType Bakery { get; } = new BuildingType("bannerkings_bakery");
        public BuildingType Mining { get; } = new BuildingType("bannerkings_mining");
        public BuildingType Farming { get; } = new BuildingType("bannerkings_farming");
        public BuildingType Sawmill { get; } = new BuildingType("bannerkings_sawmill");
        public BuildingType AnimalHousing { get; } = new BuildingType("bannerkings_animal_housing");
        public BuildingType Butter { get; } = new BuildingType("bannerkings_butter");
        public BuildingType FishFarm { get; } = new BuildingType("bannerkings_fishing");
        public BuildingType Tannery { get; } = new BuildingType("bannerkings_tannery");
        public BuildingType Blacksmith { get; } = new BuildingType("bannerkings_blacksmith");
        public BuildingType Mines { get; } = new BuildingType("bannerkings_mines");
        public BuildingType Skeps { get; } = new BuildingType("bannerkings_skeps");
        public BuildingType Marketplace { get; } = new BuildingType("bannerkings_marketplace");
        public BuildingType TaxOffice { get; } = new BuildingType("bannerkings_taxoffice");
        public BuildingType DailyProduction { get; } = new BuildingType("bannerkings_daily_production");
        public BuildingType DailyFarm { get; } = new BuildingType("bannerkings_daily_farm");
        public BuildingType DailyPasture { get; } = new BuildingType("bannerkings_daily_pasture");
        public BuildingType DailyWoods { get; } = new BuildingType("bannerkings_daily_woods");
        public BuildingType Mill { get; } = new BuildingType("bk_Mill");

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
                yield return Mines;
                yield return Skeps;
                yield return Marketplace;
                yield return Bakery;
                yield return TaxOffice;
            }
        }

        public override void Initialize()
        {
            Mill.Initialize(new TextObject("{=zN6bj4QX}Mill"),
                new TextObject("{=ZMcefZex}Construct a mill, used to grind grains for breadmaking. Increases bread production (50%, 100%, 150%)."),
                new[]
                {
                    1800,
                    2600,
                    3500
                }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
                {
                });

            Manor.Initialize(new TextObject("{=UHyznyEy}Manor"),
                new TextObject("{=CAPxKfpx}Manor house, the lord's home and center of the village. A manor house allows the housing of a small retinue in the village (15, 30, 45 men). Increases influence from nobles (15%, 30%, 50%)."),
                new[]
                {
                    4000,
                    6000,
                    8000
                }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
                {
                });

            Palisade.Initialize(new TextObject("{=JV9JXwnJ}Palisade"),
                new TextObject("{=wmcdrgpq}A set of wooden stakes placed around the village like a wall. Reduces raiding speed (12%, 24%, 36%)."),
                new[]
                {
                    3000,
                    5000,
                    7000
                }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
                {
                });

            TrainningGrounds.Initialize(new TextObject("{=AaYbjkxE}Trainning Grounds"),
                new TextObject("{=mXUShQJb}Stablish a zone dedicated for trainning, as well as it's required equipments, where locals can train basic military arts. Increases militia production (0.2, 0.5, 1.0)."),
                new[]
                {
                    1500,
                    2400,
                    3200
                }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
                {
                });

            Warehouse.Initialize(new TextObject("{=xmxcLt9R}Arms Warehouse"),
                new TextObject("{=X7fx1TqB}Construct a warehouse dedicated to keep military equipment as well as provide their maintenance. Improves militia quality (4%, 8%, 12%)."),
                new[]
                {
                    2000,
                    3000,
                    4000
                }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
                {
                });

            Courier.Initialize(new TextObject("{=gxgv5Z2h}Courier Post"),
                new TextObject("{=i5g9bUkg}Set up a dedicate courier post that will inform you of any relevant activity in and around your demesne. Enables information messages regardless of your distance."),
                new[]
                {
                    1000,
                    1800,
                    2500
                }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
                {
                });

            Bakery.Initialize(new TextObject("{=cBqEtNeR}Bakery"),
                new TextObject("{=bHDnFANS}Supply tools and space for a local bakery, allowing serfs to turn wheat into bread. Adds bread as production good."),
                new[]
                {
                    1000,
                    1800,
                    2500
                }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
                {
                });

            Mining.Initialize(new TextObject("{=sTwvtYgY}Mining Infrastructure"),
                new TextObject("{=tP4PM9zC}Build mining equipment and infrastructure to improve working conditions in local mines. Increases ore production (5%, 10%, 15%)."),
                new[]
                {
                    1500,
                    2400,
                    3200
                }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
                {
                });

            Mines.Initialize(new TextObject("{=iGYstgoo}Mines"),
                new TextObject("{=sfF4US9P}Dig mines for local exploration of mineral resources. Ores will be limited to the local resources available and richness of the ground. Levels increase output of ores."),
                new[]
                {
                    2000,
                    3000,
                    4000
                }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
                {
                });

            AnimalHousing.Initialize(new TextObject("{=W9U0nkST}Animal Housing"),
                new TextObject("{=SQUxVPch}Invest on infrastructure for animal housing and grazing, yielding more from your pasture lands. Increases live animals production (5%, 10%, 15%)."),
                new[]
                {
                    1500,
                    2400,
                    3200
                }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
                {
                });

            Farming.Initialize(new TextObject("{=sORHi0xE}Farming Infrastructure"),
                new TextObject("{=fSWeTPFb}Provide farming equipment and stablish systems to maximise land productivity. Increases farm goods production (5%, 10%, 15%)."),
                new[]
                {
                    1500,
                    2400,
                    3200
                }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
                {
                });

            Sawmill.Initialize(new TextObject("{=SBRGqWqH}Sawmill"),
                new TextObject("{=BcddDtDe}Build a sawmill, improving the speed and quality of log cutting into usable hardwood. Increases hardwood production (5%, 10%, 15%)."),
                new[]
                {
                    1500,
                    2400,
                    3200
                }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
                {
                });

            Butter.Initialize(new TextObject("{=xWh7ssef}Butter Mill"),
                new TextObject("{=MQVVOyAC}Construct specialized buildings for churning local cattle milk into butter, a highly sought after food amongst lords."),
                new[]
                {
                    1000,
                    1800,
                    2500
                }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
                {
                });

            Tannery.Initialize(new TextObject("{=K3fU8V2V}Fur Tannery"),
                new TextObject("{=iKGGXTBw}Construct specialized buildings for tanning hides, turning these into leather. Adds leather to production."),
                new[]
                {
                    1500,
                    2400,
                    3200
                }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
                {
                });

            FishFarm.Initialize(new TextObject("{=P1r6sHuk}Fish Farm"),
                new TextObject("{=gv4aBV3O}Build controlled fish growing zones, supplying extra fish to the village. Increases fish production (5%, 10%, 15%)."),
                new[]
                {
                    1500,
                    2400,
                    3200
                }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
                {
                });

            Blacksmith.Initialize(new TextObject("{=etbv7s6N}Smith"),
                new TextObject("{=baTKNtfc}Stablish a local blacksmith, supplying the village with metal products. Adds tools to production."),
                new[]
                {
                    1000,
                    1800,
                    2500
                }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
                {
                });

            Skeps.Initialize(new TextObject("{=mMUsTOUY}Bee Skeps"),
                new TextObject("{=oFhhNR4O}Build skeps for bee colonies. The skeps serve as hives for the bees and allow the farming of honey and wax. Adds honey to production."),
                new[]
                {
                    1000,
                    1800,
                    2500
                }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
                {
                });

            Marketplace.Initialize(new TextObject("{=zLdXCpne}Marketplace"),
                new TextObject("{=tV52TCT8}Allow locals to sell off their excess production in the designated marketplace. Travelling merchants and individuals will stop by to trade. Adds village consumption of town goods and boosts hearth growth."),
                new[]
                {
                    600,
                    1200,
                    2000
                }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
                {
                });

            TaxOffice.Initialize(new TextObject("{=b7njo98X}Tax Office"),
                new TextObject("{=Len9bhQG}Collect denar taxes on local artisans and nobles, creating a new revenue stream for the village. If Marketplace is present, items daily consumed from it are also taxed."),
                new[]
                {
                    1500,
                    2500,
                    3500
                }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
                {
                });

            DailyProduction.Initialize(new TextObject("{=eMZikYJ3}Production"),
                new TextObject("{=PiFWGEUC}Focus the population's effort in productivity, not expanding land or changing it's type. While this option is not active, productivity output is reduced."),
                new int[3], BuildingLocation.Daily, new Tuple<BuildingEffectEnum, float, float, float>[]
                {
                });

            DailyFarm.Initialize(new TextObject("{=K4xNbbUN}Farmland"),
                new TextObject("{=Q3q691if}Focus efforts on creating new arable acres, used for farming purposes. More farming acres increase output of farming goods."),
                new int[3], BuildingLocation.Daily, new Tuple<BuildingEffectEnum, float, float, float>[]
                {
                });

            DailyPasture.Initialize(new TextObject("{=WPcgsL9J}Pastureland"),
                new TextObject("{=nALdPgGX}Focus efforts on creating new acres of pasture, where cattle and animals graze and thrive. More pasture acres increase output of animals and animal products."),
                new int[3], BuildingLocation.Daily, new Tuple<BuildingEffectEnum, float, float, float>[]
                {
                });

            DailyWoods.Initialize(new TextObject("{=FRAS5TAC}Woodland"),
                new TextObject("{=1NTssM4Y}Focus efforts on turning acres into woodlands. Acres of woodland allow more higher yields of logs and berries, as well as help the land not becoming overfarmed or grazed."),
                new int[3], BuildingLocation.Daily, new Tuple<BuildingEffectEnum, float, float, float>[]
                {
                });
        }

        public static IEnumerable<BuildingType> VillageBuildings(Village village)
        {
            var type = village.VillageType;
            yield return Instance.Manor;
            yield return Instance.Palisade;
            yield return Instance.TrainningGrounds;
            yield return Instance.Warehouse;
            yield return Instance.DailyProduction;
            yield return Instance.DailyFarm;
            yield return Instance.DailyPasture;
            yield return Instance.DailyWoods;
            yield return Instance.Bakery;
            yield return Instance.Marketplace;
            yield return Instance.TaxOffice;

            InnovationData data = BannerKingsConfig.Instance.InnovationsManager.GetInnovationData(village.Settlement.Culture);
            if (data != null && data.HasFinishedInnovation(DefaultInnovations.Instance.Mills))
            {
                yield return Instance.Mill;
            }

            if (village.IsMiningVillage())
            {
                yield return Instance.Mining;

                if (type == DefaultVillageTypes.IronMine)
                {
                    yield return Instance.Blacksmith;
                }
            }
            else
            {
                if (village.IsFarmingVillage())
                {
                    yield return Instance.Farming;
                }
                else if (village.IsAnimalVillage())
                {
                    yield return Instance.AnimalHousing;
                }
                else if (type == DefaultVillageTypes.Lumberjack)
                {
                    yield return Instance.Sawmill;
                    yield return Instance.Skeps;
                }
                else if (type == DefaultVillageTypes.Fisherman)
                {
                    yield return Instance.FishFarm;
                }
                else
                {
                    yield return Instance.Tannery;
                    yield return Instance.Skeps;
                }

                yield return Instance.Mines;
            }
        }
    }
}