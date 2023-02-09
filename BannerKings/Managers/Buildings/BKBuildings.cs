using System;
using System.Collections.Generic;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Buildings
{
    public class BKBuildings : DefaultTypeInitializer<BKBuildings, BuildingType>
    {

        public static MBReadOnlyList<BuildingType> AllBuildings
        {
            get
            {
                var buildings = Campaign.Current.GetType()
                    .GetProperty("AllBuildingTypes", BindingFlags.Instance | BindingFlags.NonPublic);
                return (MBReadOnlyList<BuildingType>)buildings.GetValue(Campaign.Current);
            }
        }

        public BuildingType Mines { get; private set; }
        public BuildingType CastleMines { get; private set; }
        public BuildingType CastleRetinue { get; private set; }
        public BuildingType Theater { get; private set; }
        public BuildingType Armory { get; private set; }
        public BuildingType CourtHouse { get; private set; }
        public BuildingType WarhorseStuds { get; private set; }
        public BuildingType DailyAssimilation { get; private set; }

        public override IEnumerable<BuildingType> All
        {
            get
            {
                yield return Mines;
                yield return CastleMines;
                yield return CastleRetinue;
                yield return Theater;
                yield return Armory;
                yield return CourtHouse;
                yield return WarhorseStuds;
                yield return DailyAssimilation;
                foreach (var item in ModAdditions)
                {
                    yield return item;
                }
            }
        }

        public override void Initialize()
        {

            Mines = Game.Current.ObjectManager.RegisterPresumedObject(new BuildingType("building_town_mines"));
            Mines.Initialize(new TextObject("{=iGYstgoo}Mines"),
                new TextObject("{=q3PH022A}Dig mines for local exploration of mineral resources. Ores will be limited to the local resources available and richness of the ground. Output will be sold to market when possible, or stored in Stash otherwise. Levels increase output of ores."),
                new[]
                {
                    1500,
                    2500,
                    4000
                },
                BuildingLocation.Settlement,
                new Tuple<BuildingEffectEnum, float, float, float>[]
                {
                    new Tuple<BuildingEffectEnum, float, float, float>(BuildingEffectEnum.Construction, 0.5f, 1f, 1.5f)
                });

            CastleMines = Game.Current.ObjectManager.RegisterPresumedObject(new BuildingType("building_castle_mines"));
            CastleMines.Initialize(new TextObject("{=iGYstgoo}Mines"),
                new TextObject("{=q3PH022A}Dig mines for local exploration of mineral resources. Ores will be limited to the local resources available and richness of the ground. Output will be sold to market when possible, or stored in Stash otherwise. Levels increase output of ores."),
                new[]
                {
                    1500,
                    2500,
                    4000
                },
                BuildingLocation.Castle,
                new Tuple<BuildingEffectEnum, float, float, float>[]
                {
                    new Tuple<BuildingEffectEnum, float, float, float>(BuildingEffectEnum.Construction, 0.5f, 1f, 1.5f)
                });


            CastleRetinue = Game.Current.ObjectManager.RegisterPresumedObject(new BuildingType("building_castle_retinue"));
            CastleRetinue.Initialize(new TextObject("{=6HgSqiDc}Retinue Barracks"),
                new TextObject("{=UNLMYRGm}Barracks for the castle retinue, a group of elite soldiers. The retinue is added to the garrison over time, up to a limit of 20, 40 or 60 (building level)."),
                new[]
                {
                    1000,
                    2000,
                    3000
                }, 
                BuildingLocation.Castle,
                new Tuple<BuildingEffectEnum, float, float, float>[]
                {
                });

            Theater = Game.Current.ObjectManager.RegisterPresumedObject(new BuildingType("bk_building_theater"));
            Theater.Initialize(new TextObject("{=rQmTMDxF}Theater"),
                new TextObject("{=SrAXS0oM}A common place for congregation, the theater serves the purpose of strengthening the culture's presence through it's art and stories. Increases cultural presence of owner's culture."),
                new[]
                {
                    2000,
                    3000,
                    4000
                },
                BuildingLocation.Settlement,
                new Tuple<BuildingEffectEnum, float, float, float>[]
                {
                    new Tuple<BuildingEffectEnum, float, float, float>(BuildingEffectEnum.Loyalty, 0.2f, 0.5f, 1f)
                });

            Armory = Game.Current.ObjectManager.RegisterPresumedObject(new BuildingType("bk_building_armory"));
            Armory.Initialize(new TextObject("{=sBTMZdyq}Armory"),
                new TextObject("{=UoZbZaaT}The armory is used to stock and preserve equipment for the town militia. Raises militia quality and provides garrison trainning."),
                new[]
                {
                    1500,
                    2000,
                    2500
                },
                BuildingLocation.Settlement,
                new Tuple<BuildingEffectEnum, float, float, float>[]
                {
                    new Tuple<BuildingEffectEnum, float, float, float>(BuildingEffectEnum.Experience, 1f, 1f, 1f)
                });

            CourtHouse = Game.Current.ObjectManager.RegisterPresumedObject(new BuildingType("bk_building_courthouse"));
            CourtHouse.Initialize(new TextObject("{=4CTz9MRe}Court House"),
                new TextObject("{=USZBgRW0}The court house is where townsfolk legally settle their disputes. Conflicts such as property disputes or insults are dealt with by local administration. Increases stability."),
                new[]
                {
                    2000,
                    2600,
                    3200
                },
                BuildingLocation.Settlement,
                new Tuple<BuildingEffectEnum, float, float, float>[]
                {
                });

            WarhorseStuds = Game.Current.ObjectManager.RegisterPresumedObject(new BuildingType("bk_building_castle_studs"));
            WarhorseStuds.Initialize(new TextObject("{=PCayirkO}Warhorse Studs"),
                new TextObject("{=b58ioVr2}Warhorse studs allow the raising of warhorses in the castle demesne. Horses will be added to the Stash. Their limit is based on local pastureland and studs level."),
                new[]
                {
                    1000,
                    1800,
                    2400
                },
                BuildingLocation.Castle,
                new Tuple<BuildingEffectEnum, float, float, float>[]
                {
                });


            DailyAssimilation = Game.Current.ObjectManager.RegisterPresumedObject(new BuildingType("bk_building_daily_assimilation"));
            DailyAssimilation.Initialize(new TextObject("{=rZOM0Jit}Cultural assimilation"),
                new TextObject("{=QrcPgzMf}Focus efforts on assimilating local pouplace to your culture. Increases Cultural Presence."),
                new int[3],
                BuildingLocation.Daily,
                new Tuple<BuildingEffectEnum, float, float, float>[]
                {
                });
        }
    }
}
