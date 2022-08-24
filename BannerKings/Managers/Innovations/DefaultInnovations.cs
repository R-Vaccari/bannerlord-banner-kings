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
            HeavyPlough.Initialize(new TextObject("{=JtVSLxTM}Heavy Plough"),
                new TextObject("{=WOyvGkUe}Heavy plough used for tougher terrain."),
                new TextObject("{=3jFTYjJK}Increased farmland acreage output by 8%"),
                800f);

            ThreeFieldsSystem = new Innovation("innovation_three_field_system");
            ThreeFieldsSystem.Initialize(new TextObject("{=WdO39AEL}3 Fields System"),
                new TextObject("{=gQyepV7c}Crop rotation system using 3 separate types of crops simultaneously."),
                new TextObject("{=DvnWuSsn}Increased farmland acreage output by 25%"),
                1200f);

            PublicWorks = new Innovation("innovation_public_works");
            PublicWorks.Initialize(new TextObject("{=d7ytP0ej}Public Works"),
                new TextObject(
                    "{=jM5siaNV}Focused efforts on public infrastructure by the state allow further development of new and existing buildings."),
                new TextObject("{=KoP0wzzi}Expands Infrastructure limit in settlements by flat 3"),
                2000f);

            Cranes = new Innovation("innovation_cranes");
            Cranes.Initialize(new TextObject("{=moPn6wZD}Cranes"),
                new TextObject(
                    "{=vkaBMV5q}Mechanismis capable of vertically carrying high volumes or weight of material. Cranes significantly increase production output by adding productivity to construction sites and trade hubs."),
                new TextObject("{=wP4dZP4o}Improves construction projects speed (12%)\nIncreases production efficiency (6%)"),
                3000f);

            Wheelbarrow = new Innovation("innovation_wheelbarrow");
            Wheelbarrow.Initialize(new TextObject("{=hRNGD11o}Wheelbarrow"),
                new TextObject(
                    "{=HPGmk1oy}The wheelbarrow is a goods transporting tool that allows a person to carry bigger weights with less efforts. It can be applied in a variety of situations, such as carrying ore out of mines, building material to constructions and grain sacks out of farms."),
                new TextObject("{=w267ALJy}Increases production efficiency (6%)"),
                1500f);

            BlastFurnace = new Innovation("innovation_blast_furnace");
            BlastFurnace.Initialize(new TextObject("{=siEg4WJX}Blast Furnace"),
                new TextObject(
                    "{=tmFpiT94}Blast furnaces efficiently transform iron ore into cast iron. This flexibe alloy can be easily shaped into different forms and products, thus making the furnaces an essential industrial appliance."),
                new TextObject("{=uw46OTE4}Increases production efficiency (15%)"),
                5000f);
        }
    }
}