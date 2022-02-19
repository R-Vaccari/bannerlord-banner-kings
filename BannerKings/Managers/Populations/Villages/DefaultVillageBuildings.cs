using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Populations.Villages
{
    public class DefaultVillageBuildings
    {
        private BuildingType manor, palisade;

        public BuildingType Manor => this.manor;
        public BuildingType Palisade => this.palisade;

        public static DefaultVillageBuildings Instance => ConfigHolder.CONFIG;
        
        internal struct ConfigHolder
        {
            public static DefaultVillageBuildings CONFIG = new DefaultVillageBuildings();
        }

        public void Init()
        {
            List<BuildingType> types = new List<BuildingType>();
            this.manor = new BuildingType("bannerkings_manor");
            this.manor.Initialize(new TextObject("{=!}Manor", null), new TextObject("{=!}Manor house, the lord's home and center of the village. A manor house allows the housing of a small retinue in the village.", null), new int[]
            {
                0,
                8000,
                16000
            }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
            {
                new Tuple<BuildingEffectEnum, float, float, float>(BuildingEffectEnum.GarrisonCapacity, 25f, 50f, 100f)
            }, 1);

            this.palisade = new BuildingType("bannerkings_palisade");
            this.palisade.Initialize(new TextObject("{=!}Palisade", null), new TextObject("{=!}A set of wooden stakes placed around the village like a wall. Palisades significantly reduce the speed of raiders.", null), new int[]
            {
                0,
                8000,
                16000
            }, BuildingLocation.Settlement, new Tuple<BuildingEffectEnum, float, float, float>[]
            {
                new Tuple<BuildingEffectEnum, float, float, float>(BuildingEffectEnum.GarrisonCapacity, 25f, 50f, 100f)
            }, 1);

            BannerKingsConfig.Instance.VillageBuildings = new MBReadOnlyList<BuildingType>(types);
        }

        public static IEnumerable<BuildingType> VillageBuildings
        {
            get
            {
                yield return Instance.Manor;
                yield return Instance.Palisade;
                yield break;
            }
        }
    }
}
