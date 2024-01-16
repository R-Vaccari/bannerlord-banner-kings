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
        public Innovation Mills { get; } = new Innovation("innovation_water_mill");
        public Innovation Guilds { get; } = new Innovation("Guilds");
        public Innovation Burgage { get; } = new Innovation("Burgage");
        public Innovation HalfPlateArmor { get; } = new Innovation("HalfPlateArmor");
        public Innovation PlateArmor { get; } = new Innovation("PlateArmor");
        public Innovation Forum { get; } = new Innovation("Forum");
        public Innovation Theater { get; } = new Innovation("Theater");
        public Innovation Aqueducts { get; } = new Innovation("Aqueducts");
        public Innovation Crossbows { get; } = new Innovation("Crossbows");
        public Innovation Manorialism { get; } = new Innovation("Manorialism");
        public Innovation Masonry { get; } = new Innovation("Masonry");
        public Innovation AdvancedMasonry { get; } = new Innovation("AdvancedMasonry");

        public Dictionary<string, List<Innovation>> StartingInnovations { get; } = new Dictionary<string, List<Innovation>>(15);

        public override IEnumerable<Innovation> All
        {
            get
            {
                yield return HeavyPlough;
                yield return ThreeFieldsSystem;
                yield return PublicWorks;
                yield return Cogs;
                yield return Mills;
                yield return Stirrups;
                yield return Compass;
                yield return HorseCollar;
                yield return HorseShoe;
                yield return Cranes;
                yield return Wheelbarrow;
                yield return BlastFurnace;
                yield return Guilds;
                yield return Burgage;
                yield return Crossbows;
                yield return Forum;
                yield return Aqueducts;
                yield return HalfPlateArmor;
                yield return Theater;
                yield return Manorialism;
                yield return Masonry;
                yield return AdvancedMasonry;
                foreach (Innovation item in ModAdditions)
                {
                    yield return item;
                }
            }
        }

        public override void Initialize()
        {
            var cultures = Game.Current.ObjectManager.GetObjectTypeList<CultureObject>();

            Manorialism.Initialize(new TextObject("{=kXCkuHkY}Manorialism"),
                new TextObject("{=6G2Oht8M}Originated from the Calradian Villae, manorialism describes the agrarian relationship of a suzerain and their dependents. While each peasant may own or rent certain parts of the fields, the entire fields are collectively worked on, and their productions shared, a significant part of which is paid to the overlord as taxes, who often lives in a large habitation - the manor."),
                new TextObject("{=6WAvHgPj}Enables new village buildings\nReduced slave presence in villages"),
                DefaultEras.Instance.FirstEra,
                Innovation.InnovationType.Civic,
                1000f,
                null);

            ThreeFieldsSystem.Initialize(new TextObject("{=M316iXhm}3 Fields System"),
                new TextObject("{=g0wZ1P2M}A crop rotation system using 3 separate types of crops simultaneously. This system, also called Open Field, is a major improvement the culture of 2 fields, allowing the soil to regenerate as different types of crops provide and consume different nutrients."),
                new TextObject("{=5710Kwo0}Greatly improved farmland output"),
                DefaultEras.Instance.FirstEra,
                Innovation.InnovationType.Agriculture,
                2500f,
                Manorialism);

            Masonry.Initialize(new TextObject("{=rTUgkyMt}Masonry"),
                new TextObject("{=hogsHsHE}The art of molding stone to man's desire. Within the continent, this art was first explored in its depths by the Calradoi and the Darshi. Their imposing stone walls would, at times, make barbarian raiders entirely give up on raiding their towns, for they knew they could not overwhelm them. More often, it striked them with a sense of might - that those who built such walls were unmatched builders. The art of masonry later evolved into other forms of infrastructure, such as aqueducts and forums."),
                new TextObject("{=JdD4VMhr}Enables construction of level 2 walls"),
                DefaultEras.Instance.FirstEra,
                Innovation.InnovationType.Building,
                1500f,
                null);

            Forum.Initialize(new TextObject("{=OqSj1HBK}Forum"),
                new TextObject("{=sLDyMt8s}A Calradian invention, the Forum is the center of town life, where announcements are made, sentences passed, and emperors are named. It is the marble heart of a city, and itself a form of art."),
                new TextObject("{=oD2NsLEO}Enables the construction of forums"),
                DefaultEras.Instance.SecondEra,
                Innovation.InnovationType.Building,
                1000f,
                null);

            Theater.Initialize(new TextObject("Theater"),
                new TextObject("{=0wsqttef}It is unclear whether Calradians or Laconians first invented Theaters. More than a building, theaters are a cultural innovation - common folk gather to watch elaborate, classical works of both tragedy and comedy. In many ways, theaters stablish a cultural foundation and promote cultural values through the stories told in them."),
                new TextObject("{=wnS5gm2Z}Enables the construction of theaters"),
                DefaultEras.Instance.SecondEra,
                Innovation.InnovationType.Building,
                1000f,
                Forum);

            Aqueducts.Initialize(new TextObject("{=mX96pT3Z}Aqueducts"),
                new TextObject("{=fVRSUU9D}A feat of engineering by the Calradoi - aqueducts transport water through a downard gradient from a water source to a town. The reliable supply of water serves to satiate the populace's thirst, but also provide water to industries such as farming, milling and dyeing, keeping food supply cheap and the economy active. It is said among the Calradoi a city without an aqueduct is doomed to failure."),
                new TextObject("{=mEuEaVb8}Enables the construction of aqueducts"),
                DefaultEras.Instance.SecondEra,
                Innovation.InnovationType.Building,
                1000f,
                Forum);

            Burgage.Initialize(new TextObject("{=rhCibDMB}Burgage"),
                new TextObject("{=McmVLHig}A Burgage is a rental property with a town, owned by a lord. Such plots are rented in exchange for monetary payment, providing a steady source of income to their noble overlords."),
                new TextObject("{=3SLXcMMc}Increased population cap in towns"),
                DefaultEras.Instance.SecondEra,
                Innovation.InnovationType.Civic,
                1000f);

            Guilds.Initialize(new TextObject("{=oOTc2x9R}Guilds"),
                new TextObject("{=oOTc2x9R}Guilds are associations of artisans that concern a specific trade. Their main purpose was the regulate a specific craft, trainning artisans, guaranteeing a minimum level of quality and destroying forgeries. This would allow, for example, artisans of a specific town to be renowned in a given craft, driving prices up and thus the guild's profits. Yet, this source of power among the merchants and artisans may prove a source of conflict with their noble overlords."),
                new TextObject("{=KPG1tp2e}Enables the existance of guilds in towns"),
                DefaultEras.Instance.SecondEra,
                Innovation.InnovationType.Civic,
                2000f,
                Burgage);

            PublicWorks.Initialize(new TextObject("{=d3aY0Bbb}Public Works"),
               new TextObject("{=fNLHKDfe}Focused efforts on public infrastructure by the state, such as roads and bridges, allow better development of the economy."),
               new TextObject("{=Wj5OuuKY}Increased town prosperity and adm. costs"),
               DefaultEras.Instance.SecondEra,
               Innovation.InnovationType.Civic,
               2000f,
               Burgage);

            Stirrups.Initialize(new TextObject("{=cUZL1bQj}Stirrups"),
                new TextObject("{=UtOOFUu9}A riding application that greatly helps with rider stability. The stirrup provides support for the rider's feet, reducing the risk of a fall and losing control of the animal. In military terms, it means cavalry can be significantly more effective and reliable. For the economy, it is also quite useful as the horse is widely used in agriculture, transportation and communication."),
                new TextObject("{=Vz6pm1G2}Increased production efficiency\nBetter cavalry efficiency in sim. battles"),
                DefaultEras.Instance.FirstEra,
                Innovation.InnovationType.Military,
                2000f);

            Crossbows.Initialize(new TextObject("{=ewvrYvsZ}Crossbows"),
                new TextObject("{=EsZYQsnE}A weapon developed by the Calradoi - a crossbow works by launching a bolt across a horizontal frame with the force of its string. The string, rather than drawn and held in place by a hand, is locked into place by a mechanism, allowing the crossbowman to aim the weapon, ready to release, with virtually no restrictions, as opposed to a bow, which requires significant strength to hold while ready."),
                new TextObject("{=7mybzSYB}Enables the construction of crossbows"),
                DefaultEras.Instance.FirstEra,
                Innovation.InnovationType.Military,
                2000f,
                Stirrups);

            Cogs.Initialize(new TextObject("{=0624yewk}Cogs"),
                new TextObject("{=OgqFqiLj}These large devices were often made of massive strong wooden blocks shaped into almost perfect circular discs. Metal teeth were then bolted into the disc such that an array of cogs can produce complex axes of motion. They were mainly used in sophisticated mill designs such as watermills or windmills. Such mechanical mills facilitate the process of milling grains to make bread, an extremely important item in the continent's general diet. However, other mills could be essential parts in industrial military forges for producing armour, weapons and horse equipment fit for massive imperial armies."),
                new TextObject("{=BzKY5Wun}Increased production efficiency"),
                DefaultEras.Instance.FirstEra,
                Innovation.InnovationType.Technology,
                1000f);

            Mills.Initialize(new TextObject("{=M2sADqDN}Mills"),
                new TextObject("{=Xu1c2xmk}A large contraption moved by the flow of water, wind or force. Mills largely facilitate the process of milling grains in order to make bread. Different types of mills may be constructed to fit any context - for example, a village with a river stream may use the flow of water to turn the mill. A more complex, but flexible mill could be moved by wind. Ultimately, however, they could simply be moved through slave labor."),
                new TextObject("{=5yKtBhYZ}Enables construction of mills"),
                DefaultEras.Instance.FirstEra,
                Innovation.InnovationType.Building,
                1500f,
                Cogs);

            HeavyPlough.Initialize(new TextObject("{=AQr3XGhg}Heavy Plough"),
                new TextObject("{=bPaAHAFe}A adaptation of the plough, but heavier and used for tougher, moist terrain. This plough allows more soil to be turned over, facilitating agricultural work and recycling nutriets found in deeper soil layers."),
                new TextObject("{=NPt6xwhi}Improved farmland output"),
                DefaultEras.Instance.FirstEra,
                Innovation.InnovationType.Agriculture,
                800f);

            HorseCollar.Initialize(new TextObject("{=dds6strO}Horse Collar"),
               new TextObject("{=NjcHMAmp}A padded collar specifically made to fit horses allows the animal to comfortably pull ploughs with maximum efficiency of its strength. This in turn allows to the replacement of oxen teams for horse teams as plough pullers, a significant improvement due to the horses' increased speed and endurance over oxen."),
               new TextObject("{=dTizPCEX}Significantly improved farmland output"),
               DefaultEras.Instance.FirstEra,
               Innovation.InnovationType.Agriculture,
               2000f,
               HeavyPlough);

            HorseShoe.Initialize(new TextObject("{=oXPnEqxK}Horse Shoes"),
                new TextObject("{=RkQi0bst}Metallic horse shows protect the hooves of the animal against the deterioration caused by intense labor. With the advent of teams of horses instead of oxen to pull ploughs, the shoes amplify the animal's advantage of working longer by protecting it moving ability."),
                new TextObject("{=NPt6xwhi}Improved farmland output"),
                DefaultEras.Instance.FirstEra,
                Innovation.InnovationType.Agriculture,
                1200f,
                HorseCollar);

            Cranes.Initialize(new TextObject("{=5BfW3TXX}Cranes"),
                new TextObject("{=eTdq7KvZ}Mechanismis capable of vertically carrying high volumes or weight of material. Cranes significantly increase production output by adding productivity to construction sites and trade hubs."),
                new TextObject("{=hMyih8SN}Significantly improved construction speed\nIncreased production efficiency"),
                DefaultEras.Instance.SecondEra,
                Innovation.InnovationType.Technology,
                3000f);

            AdvancedMasonry.Initialize(new TextObject("{=oEcdNsTa}Advanced Masonry"),
                new TextObject("{=ZVTUdEKh}The final culmination of the art of masonry. Advanced masonry involves the construction of high, thick stone walls with deep foundations. Such walls are resistant even agains the strongest forms of siege engines, and certainly provide sisgnificant advantage to those under their protection.."),
                new TextObject("{=wT1kuDyC}Enables construction of level 3 walls"),
                DefaultEras.Instance.SecondEra,
                Innovation.InnovationType.Building,
                3000f,
                Cranes);

            Wheelbarrow.Initialize(new TextObject("{=H5EXMMCH}Wheelbarrow"),
                new TextObject("{=EtdzfFiF}The wheelbarrow is a goods transporting tool that allows a person to carry bigger weights with less efforts. It can be applied in a variety of situations, such as carrying ore out of mines, building material to constructions and grain sacks out of farms."),
                new TextObject("{=BzKY5Wun}Increased production efficiency"),
                DefaultEras.Instance.SecondEra,
                Innovation.InnovationType.Technology,
                1500f,
                Cranes);

            BlastFurnace.Initialize(new TextObject("{=pOHP0a2R}Blast Furnace"),
                new TextObject("{=zzP8O9LS}Blast furnaces efficiently transform iron ore into cast iron. This flexibe alloy can be easily shaped into different forms and products, thus making the furnaces an essential industrial appliance."),
                new TextObject("{=m0RDPf0p}Significantly increased production efficiency"),
                DefaultEras.Instance.SecondEra,
                Innovation.InnovationType.Technology,
                5000f,
                Cranes);

            HalfPlateArmor.Initialize(new TextObject("{=FC9AOT6i}Half-Plate Armor"),
                new TextObject("{=!}"),
                new TextObject("{=SzKfJXQd}Enables the production of half-plated and similar armors"),
                DefaultEras.Instance.SecondEra,
                Innovation.InnovationType.Technology,
                2000f,
                BlastFurnace);

            Compass.Initialize(new TextObject("{=CQxPHrcj}Compass"),
               new TextObject("{=QZ2WbQM3}An instrument used to tell the cardinal directions, extensively used in long travels, by ship or land."),
               new TextObject("{=!}"),
               DefaultEras.Instance.ThirdEra,
               Innovation.InnovationType.Technology,
               3000f,
               Astrolabe);

            Astrolabe.Initialize(new TextObject("{=eoPaEKTc}Astrolabe"),
                new TextObject("{=WST44T3P}."),
                new TextObject("{=kyB8tkgY}C"),
                DefaultEras.Instance.ThirdEra,
                Innovation.InnovationType.Technology,
                3000f);

            StartingInnovations["vlandia"] = new List<Innovation>()
            {
                Burgage,
                HeavyPlough,
                HorseCollar,
                Crossbows,
                Mills
            };

            StartingInnovations["empire"] = new List<Innovation>()
            {
                HeavyPlough,
                HorseCollar,
                Guilds,
                Manorialism,
                Crossbows,
                Stirrups,
                Mills,
                Theater,
                Forum,
                Aqueducts
            };

            StartingInnovations["khuzait"] = new List<Innovation>()
            {
                HorseCollar,
                Stirrups
            };

            StartingInnovations["aserai"] = new List<Innovation>()
            {
                Burgage,
                HeavyPlough,
                HorseCollar,
                Crossbows
            };

            StartingInnovations["sturgia"] = new List<Innovation>()
            {
                Burgage,
                HeavyPlough,
                HorseCollar,
            };

            StartingInnovations["battania"] = new List<Innovation>()
            {
                HeavyPlough,
                ThreeFieldsSystem,
                Wheelbarrow
            };
        }

        public List<Innovation> GetCultureDefaultInnovations(CultureObject culture)
        {
            List<Innovation> list = new List<Innovation>(3);
            if (StartingInnovations.ContainsKey(culture.StringId))
            {
                list = StartingInnovations[culture.StringId];
            }

            return list;
        }
    }
}