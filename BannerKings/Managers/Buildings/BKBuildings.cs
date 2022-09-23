using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Buildings
{
    internal class BKBuildings : DefaultTypeInitializer<BKBuildings, BuildingType>
    {

        public BuildingType Mines { get; private set; }
        public BuildingType CastleMines { get; private set; }
        public BuildingType CastleRetinue { get; private set; }
        public BuildingType Theater { get; private set; }
        public BuildingType Armory { get; private set; }

        public override IEnumerable<BuildingType> All
        {
            get
            {
                yield return Mines;
                yield return CastleMines;
                yield return CastleRetinue;
                yield return Theater;
                //yield return Armory;
            }
        }

        public override void Initialize()
        {

            Mines = Game.Current.ObjectManager.RegisterPresumedObject(new BuildingType("building_town_mines"));
            Mines.Initialize(new TextObject("{=!}Mines"),
                new TextObject("{=!}Dig mines for local exploration of mineral resources. Ores will be limited to the local resources available and richness of the ground. Levels increase output of ores."),
                new[]
                {
                    2500,
                    3500,
                    5000
                },
                BuildingLocation.Settlement,
                new Tuple<BuildingEffectEnum, float, float, float>[]
                {
                });

            CastleMines = Game.Current.ObjectManager.RegisterPresumedObject(new BuildingType("building_castle_mines"));
            CastleMines.Initialize(new TextObject("{=!}Mines"),
                new TextObject("{=!}Dig mines for local exploration of mineral resources. Ores will be limited to the local resources available and richness of the ground. Levels increase output of ores."),
                new[]
                {
                    2500,
                    3500,
                    5000
                },
                BuildingLocation.Castle,
                new Tuple<BuildingEffectEnum, float, float, float>[]
                {
                });


            CastleRetinue = Game.Current.ObjectManager.RegisterPresumedObject(new BuildingType("building_castle_retinue"));
            CastleRetinue.Initialize(new TextObject("{=6HgSqiDc}Retinue Barracks"),
                new TextObject("{=UNLMYRGm}Barracks for the castle retinue, a group of elite soldiers. The retinue is added to the garrison over time, up to a limit of 20, 40 or 60 (building level)."),
                new[]
                {
                    1000,
                    1500,
                    2000
                }, 
                BuildingLocation.Castle,
                new Tuple<BuildingEffectEnum, float, float, float>[]
                {
                });

            Theater = Game.Current.ObjectManager.RegisterPresumedObject(new BuildingType("bk_building_theater"));
            Theater.Initialize(new TextObject("{=!}Theater"),
                new TextObject("{=!}A common place for congregation, the theater serves the purpose of strengthening the culture's presence through it's art and stories. Increases cultural presence of owner's culture."),
                new[]
                {
                    1000,
                    1500,
                    2000
                },
                BuildingLocation.Settlement,
                new Tuple<BuildingEffectEnum, float, float, float>[]
                {
                });
        }
    }
}
