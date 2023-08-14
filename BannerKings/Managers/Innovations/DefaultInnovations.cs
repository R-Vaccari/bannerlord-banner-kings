using BannerKings.Managers.Innovations.Eras;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Innovations
{
    public class DefaultInnovations : DefaultTypeInitializer<DefaultInnovations, Innovation>
    {
        public Innovation HeavyPlough { get; } = new Innovation("innovation_heavy_plough");
        public Innovation ThreeFieldsSystem { get; } = new Innovation("innovation_three_field_system");
        public Innovation PublicWorks { get; } = new Innovation("innovation_public_works");
        public Innovation Cranes { get; } = new Innovation("innovation_cranes");
        public Innovation Wheelbarrow { get; } = new Innovation("innovation_wheelbarrow");
        public Innovation BlastFurnace { get; } = new Innovation("innovation_blast_furnace");
        public Innovation Astrolabe { get; } = new Innovation("innovation_astrolabe");
        public Innovation Stirrups { get; } = new Innovation("innovation_stirrups");
        public Innovation Compass { get; } = new Innovation("innovation_compass");
        public Innovation HorseCollar { get; } = new Innovation("innovation_horse_collar");
        public Innovation HorseShoe { get; } = new Innovation("innovation_horse_show");
        public Innovation Cogs { get; } = new Innovation("innovation_cogs");
        public Innovation WaterMill { get; } = new Innovation("innovation_water_mill");

        public override IEnumerable<Innovation> All
        {
            get
            {
                yield return HeavyPlough;
                yield return ThreeFieldsSystem;
                yield return PublicWorks;
                yield return Cogs;
                yield return WaterMill;
                yield return Stirrups;
                yield return Compass;
                yield return HorseCollar;
                yield return HorseShoe;
                yield return Cranes;
                yield return Wheelbarrow;
                yield return BlastFurnace;
            }
        }

        public override void Initialize()
        {
            var cultures = Game.Current.ObjectManager.GetObjectTypeList<CultureObject>();

            Stirrups.Initialize(new TextObject("{=!}Cogs"),
                new TextObject("{=!}These large devices were often made of massive strong wooden blocks shaped into almost perfect circular discs. Metal teeth were then bolted into the disc such that an array of cogs can produce complex axes of motion. They were mainly used in sophisticated mill designs such as watermills or windmills. Such mechanical mills facilitate the process of milling grains to make bread, an extremely important item in the continent's general diet. However, other mills could be essential parts in industrial military forges for producing armour, weapons and horse equipment fit for massive imperial armies."),
                new TextObject("{=!}Increases production efficiency (6%)"),
                DefaultEras.Instance.SecondEra,
                1000f);

            Compass.Initialize(new TextObject("{=!}Cogs"),
                new TextObject("{=!}These large devices were often made of massive strong wooden blocks shaped into almost perfect circular discs. Metal teeth were then bolted into the disc such that an array of cogs can produce complex axes of motion. They were mainly used in sophisticated mill designs such as watermills or windmills. Such mechanical mills facilitate the process of milling grains to make bread, an extremely important item in the continent's general diet. However, other mills could be essential parts in industrial military forges for producing armour, weapons and horse equipment fit for massive imperial armies."),
                new TextObject("{=!}Increases production efficiency (6%)"),
                DefaultEras.Instance.SecondEra,
                1000f);

            Cogs.Initialize(new TextObject("{=!}Cogs"),
                new TextObject("{=!}These large devices were often made of massive strong wooden blocks shaped into almost perfect circular discs. Metal teeth were then bolted into the disc such that an array of cogs can produce complex axes of motion. They were mainly used in sophisticated mill designs such as watermills or windmills. Such mechanical mills facilitate the process of milling grains to make bread, an extremely important item in the continent's general diet. However, other mills could be essential parts in industrial military forges for producing armour, weapons and horse equipment fit for massive imperial armies."),
                new TextObject("{=!}Increases production efficiency (6%)"),
                DefaultEras.Instance.SecondEra,
                1000f);

            WaterMill.Initialize(new TextObject("{=!}Tidal Mill"),
                new TextObject("{=!}A large contraption moved by the flow of water. Water mills largely facilitate the process of milling grains in order to make bread. Being moved only by a natural water flow, such mills can be constructed in many a village, so long they have a body of water with consistent current."),
                new TextObject("{=!}"),
                DefaultEras.Instance.SecondEra,
                1500f,
                null,
                Cogs);

            HeavyPlough.Initialize(new TextObject("{=AQr3XGhg}Heavy Plough"),
                new TextObject("{=GAADsQQs}Heavy plough used for tougher terrain."),
                new TextObject("{=NEdwbi0L}Increased farmland acreage output by 8%"),
                DefaultEras.Instance.SecondEra,
                800f);

            HorseCollar.Initialize(new TextObject("{=!}Horse Collar"),
               new TextObject("{=!}A padded collar specifically made to fit horses allows the animal to comfortably pull ploughs with maximum efficiency of its strength. This in turn allows to the replacement of oxen teams for horse teams as plough pullers, a significant improvement due to the horses' increased speed and endurance over oxen."),
               new TextObject("{=!}Increased farmland acreage output by 20%"),
               DefaultEras.Instance.SecondEra,
               2000f,
               null,
               HeavyPlough);

            HorseShoe.Initialize(new TextObject("{=!}Horse Shoes"),
                new TextObject("{=!}Metallic horse shows protect the hooves of the animal against the deterioration caused by intense labor. With the advent of teams of horses instead of oxen to pull ploughs, the shoes amplify the animal's advantage of working longer by protecting it moving ability."),
                new TextObject("{=NEdwbi0L}Increased farmland acreage output by 8%"),
                DefaultEras.Instance.SecondEra,
                1200f,
                null,
                HorseCollar);

            ThreeFieldsSystem.Initialize(new TextObject("{=M316iXhm}3 Fields System"),
                new TextObject("{=aft44fJb}Crop rotation system using 3 separate types of crops simultaneously."),
                new TextObject("{=REqhxzYi}Increased farmland acreage output by 25%"),
                DefaultEras.Instance.SecondEra,
                2000f);

            PublicWorks.Initialize(new TextObject("{=d3aY0Bbb}Public Works"),
                new TextObject("{=ZgrePk7u}Focused efforts on public infrastructure by the state allow further development of new and existing buildings."),
                new TextObject("{=cZnMgVyr}Expands Infrastructure limit in settlements by flat 3"),
                null,
                2000f);

            Cranes.Initialize(new TextObject("{=5BfW3TXX}Cranes"),
                new TextObject("{=eTdq7KvZ}Mechanismis capable of vertically carrying high volumes or weight of material. Cranes significantly increase production output by adding productivity to construction sites and trade hubs."),
                new TextObject("{=ESfXzpNq}Improves construction projects speed (12%)\nIncreases production efficiency (6%)"),
                DefaultEras.Instance.SecondEra,
                3000f,
                null,
                Cogs);

            Wheelbarrow.Initialize(new TextObject("{=H5EXMMCH}Wheelbarrow"),
                new TextObject("{=EtdzfFiF}The wheelbarrow is a goods transporting tool that allows a person to carry bigger weights with less efforts. It can be applied in a variety of situations, such as carrying ore out of mines, building material to constructions and grain sacks out of farms."),
                new TextObject("{=JU96GSxT}Increases production efficiency (6%)"),
                DefaultEras.Instance.SecondEra,
                1500f);

            BlastFurnace.Initialize(new TextObject("{=pOHP0a2R}Blast Furnace"),
                new TextObject("{=zzP8O9LS}Blast furnaces efficiently transform iron ore into cast iron. This flexibe alloy can be easily shaped into different forms and products, thus making the furnaces an essential industrial appliance."),
                new TextObject("{=QVkOr639}Increases production efficiency (15%)"),
                DefaultEras.Instance.SecondEra,
                5000f);
        }
    }
}