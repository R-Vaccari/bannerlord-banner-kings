using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Innovations
{
    public class DefaultInnovations : DefaultTypeInitializer<DefaultInnovations, Innovation>
    {

        private Innovation heavyPlough, threeFieldsSystem, sewers, publicWorks, cranes, wheelbarrow, blastFurnace;
        public Innovation HeavyPlough => heavyPlough;
        public Innovation ThreeFieldsSystem => threeFieldsSystem;
        public Innovation PublicWorks => publicWorks;
        public Innovation Cranes => cranes;
        public Innovation Wheelbarrow => wheelbarrow;
        public Innovation BlastFurnace => blastFurnace;

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
            MBReadOnlyList<CultureObject> cultures = Game.Current.ObjectManager.GetObjectTypeList<CultureObject>();

            heavyPlough = new Innovation("innovation_heavy_plough");
            heavyPlough.Initialize(new TextObject("{=!}Heavy Plough"),
                new TextObject("{=!}Heavy plough used for tougher terrain."),
                new TextObject("{=!}Increased farmland acreage output by 8%"),
                800f,
                null,
                null);

            threeFieldsSystem = new Innovation("innovation_three_field_system");
            threeFieldsSystem.Initialize(new TextObject("{=!}3 Fields System"),
                new TextObject("{=!}Crop rotation system using 3 separate types of crops simultaneously."),
                new TextObject("{=!}Increased farmland acreage output by 25%"),
                1200f,
                null,
                null);

            publicWorks = new Innovation("innovation_public_works");
            publicWorks.Initialize(new TextObject("{=!}Public Works"),
                new TextObject("{=!}Focused efforts on public infrastructure by the state allow further development of new and existing buildings."),
                new TextObject("{=!}Expands Infrastructure limit in settlements by flat 3"),
                2000f,
                null,
                null);

            cranes = new Innovation("innovation_cranes");
            cranes.Initialize(new TextObject("{=!}Cranes"),
                new TextObject("{=!}Mechanismis capable of vertically carrying high volumes or weight of material. Cranes significantly increase production output by adding productivity to construction sites and trade hubs."),
                new TextObject("{=!}Improves construction projects speed (12%)\nIncreases production efficiency (6%)"),
                3000f,
                null,
                null);

            wheelbarrow = new Innovation("innovation_wheelbarrow");
            wheelbarrow.Initialize(new TextObject("{=!}Wheelbarrow"),
                new TextObject("{=!}The wheelbarrow is a goods transporting tool that allows a person to carry bigger weights with less efforts. It can be applied in a variety of situations, such as carrying ore out of mines, building material to constructions and grain sacks out of farms."),
                new TextObject("{=!}Increases production efficiency (6%)"),
                1500f,
                null,
                null);

            blastFurnace = new Innovation("innovation_blast_furnace");
            blastFurnace.Initialize(new TextObject("{=!}Blast Furnace"),
                new TextObject("{=!}Blast furnaces efficiently transform iron ore into cast iron. This flexibe alloy can be easily shaped into different forms and products, thus making the furnaces an essential industrial appliance."),
                new TextObject("{=!}Increases production efficiency (15%)"),
                5000f,
                null,
                null);
        }
    }
}
