using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Innovations
{
    public class DefaultInnovations : DefaultTypeInitializer<DefaultInnovations, Innovation>
    {
        public Innovation HeavyPlough { get; private set; }
        public Innovation ThreeFieldsSystem { get; private set; }
        public Innovation PublicWorks { get; private set; }
        public Innovation Cranes { get; private set; }
        public Innovation Wheelbarrow { get; private set; }
        public Innovation BlastFurnace { get; private set; }
        public Innovation Astrolabe { get; } = new Innovation("innovation_astrolabe");
        public Innovation Stirrups { get; } = new Innovation("innovation_stirrups");
        public Innovation Compass { get; } = new Innovation("innovation_compass");
        public Innovation TidalMill { get; } = new Innovation("innovation_tidal_mill");

        public override IEnumerable<Innovation> All
        {
            get
            {
                yield return HeavyPlough;
                yield return ThreeFieldsSystem;
                yield return PublicWorks;
                yield return Cranes;
                yield return Wheelbarrow;
                yield return BlastFurnace;
            }
        }

        public override void Initialize()
        {
            var cultures = Game.Current.ObjectManager.GetObjectTypeList<CultureObject>();

            HeavyPlough = new Innovation("innovation_heavy_plough");
            HeavyPlough.Initialize(new TextObject("{=AQr3XGhg}Heavy Plough"),
                new TextObject("{=GAADsQQs}Heavy plough used for tougher terrain."),
                new TextObject("{=NEdwbi0L}Increased farmland acreage output by 8%"),
                null,
                800f);

            ThreeFieldsSystem = new Innovation("innovation_three_field_system");
            ThreeFieldsSystem.Initialize(new TextObject("{=M316iXhm}3 Fields System"),
                new TextObject("{=aft44fJb}Crop rotation system using 3 separate types of crops simultaneously."),
                new TextObject("{=REqhxzYi}Increased farmland acreage output by 25%"),
                null,
                1200f);

            PublicWorks = new Innovation("innovation_public_works");
            PublicWorks.Initialize(new TextObject("{=d3aY0Bbb}Public Works"),
                new TextObject("{=ZgrePk7u}Focused efforts on public infrastructure by the state allow further development of new and existing buildings."),
                new TextObject("{=cZnMgVyr}Expands Infrastructure limit in settlements by flat 3"),
                null,
                2000f);

            Cranes = new Innovation("innovation_cranes");
            Cranes.Initialize(new TextObject("{=5BfW3TXX}Cranes"),
                new TextObject("{=eTdq7KvZ}Mechanismis capable of vertically carrying high volumes or weight of material. Cranes significantly increase production output by adding productivity to construction sites and trade hubs."),
                new TextObject("{=ESfXzpNq}Improves construction projects speed (12%)\nIncreases production efficiency (6%)"),
                null,
                3000f);

            Wheelbarrow = new Innovation("innovation_wheelbarrow");
            Wheelbarrow.Initialize(new TextObject("{=H5EXMMCH}Wheelbarrow"),
                new TextObject("{=EtdzfFiF}The wheelbarrow is a goods transporting tool that allows a person to carry bigger weights with less efforts. It can be applied in a variety of situations, such as carrying ore out of mines, building material to constructions and grain sacks out of farms."),
                new TextObject("{=JU96GSxT}Increases production efficiency (6%)"),
                null,
                1500f);

            BlastFurnace = new Innovation("innovation_blast_furnace");
            BlastFurnace.Initialize(new TextObject("{=pOHP0a2R}Blast Furnace"),
                new TextObject("{=zzP8O9LS}Blast furnaces efficiently transform iron ore into cast iron. This flexibe alloy can be easily shaped into different forms and products, thus making the furnaces an essential industrial appliance."),
                new TextObject("{=QVkOr639}Increases production efficiency (15%)"),
                null,
                5000f);
        }
    }
}