using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Buildings
{
    internal class BKBuildings : DefaultTypeInitializer<BKBuildings, BuildingType>
    {

        public BuildingType CastleRetinue { get; private set; }
        public BuildingType Theater { get; private set; }
        public BuildingType Armory { get; private set; }

        public override IEnumerable<BuildingType> All => throw new NotImplementedException();

        public override void Initialize()
        {
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
            CastleRetinue.Initialize(new TextObject("{=!}Theater"),
                new TextObject("{=!}A common place for congregation, the theater serves the purpose of strengthening the culture's presence through it's art and stories. Increases cultural presence of owner's culture."),
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
        }
    }
}
