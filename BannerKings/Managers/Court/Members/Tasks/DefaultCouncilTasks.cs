using System.Collections.Generic;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Court.Members.Tasks
{
    public class DefaultCouncilTasks : DefaultTypeInitializer<DefaultCouncilTasks, CouncilTask>
    {
        public CouncilTask OrganizeMiltia { get; } = new CouncilTask("OrganizeMilitia");
        public CouncilTask EncourageMilitarism { get; } = new CouncilTask("EncourageMilitarism");
        public CouncilTask SuperviseGarrisons { get; } = new CouncilTask("SuperviseGarrisons");

        public CouncilTask OverseeSecurity { get; } = new CouncilTask("OverseeSecurity");
        public CouncilTask RepressCriminality { get; } = new CouncilTask("RepressCriminality");

        public CouncilTask DevelopEconomy { get; } = new CouncilTask("DevelopEconomy");
        public CouncilTask OverseeProduction { get; } = new CouncilTask("OverseeProduction");
        public CouncilTask PromoteCulture { get; } = new CouncilTask("PromoteCulture");

        public CouncilTask ManageVassals { get; } = new CouncilTask("ManageVassals");
        public CouncilTask OverseeDignataries { get; } = new CouncilTask("OverseeDignataries");
        public CouncilTask ArbitrateRelations { get; } = new CouncilTask("ArbitrateRelations");
        public CouncilTask IntegrateTitles { get; } = new CouncilTask("IntegrateTitles");

        public CouncilTask PromoteFaith { get; } = new CouncilTask("PromoteFaith");
        public CouncilTask CultivatePiety { get; } = new CouncilTask("CultivatePiety");

        public CouncilTask ManageDemesne { get; } = new CouncilTask("ManageDemesne");

        public CouncilTask EntertainFeastsMusician { get; } = new CouncilTask("EntertainFeastsMusician");

        public CouncilTask SmithWeapons { get; } = new CouncilTask("SmithWeapons");
        public CouncilTask SmithArmors { get; } = new CouncilTask("SmithArmors");
        public CouncilTask SmithBardings { get; } = new CouncilTask("SmithBardings");

        public CouncilTask EducateFamilyAntiquarian { get; } = new CouncilTask("EducateFamilyAntiquarian");

        public CouncilTask OverseeBaronies { get; } = new CouncilTask("OverseeBaronies");

        public CouncilTask EnforceLaw { get; } = new CouncilTask("EnforceLaw");

        public OverseeSanitation OverseeSanitation { get; } = new OverseeSanitation();

        public override IEnumerable<CouncilTask> All
        {
            get
            {
                yield return OrganizeMiltia;
                yield return EncourageMilitarism;
                yield return DevelopEconomy;
                yield return OverseeProduction;
                yield return PromoteCulture;
                yield return OverseeSecurity;
                yield return RepressCriminality;
                yield return OverseeDignataries;
                yield return ArbitrateRelations;
                yield return ManageVassals;
                yield return PromoteFaith;
                yield return CultivatePiety;
                yield return ManageDemesne;
                yield return EntertainFeastsMusician;
                yield return SmithBardings;
                yield return SmithArmors;
                yield return SmithWeapons;
                yield return OverseeSanitation;
                yield return EducateFamilyAntiquarian;
                foreach (CouncilTask item in ModAdditions)
                {
                    yield return item;
                }
            }
        }

        public override void Initialize()
        {
            OrganizeMiltia.Initialize(new TextObject("{=!}Organize Militia"),
                new TextObject("{=!}Enact militia trainning drills and the obligation of households owning arms. Through stimulation of the state, more workforce members become active part of fief militias, as well as with generally improved trainning and equipment."),
                new TextObject("{=!}Increased militia production\nImproved militia quality"),
                0f);

            EncourageMilitarism.Initialize(new TextObject("{=!}Encourage Militarism"),
                new TextObject("{=!}Encourage militarism within the populace. More militarism means a larger portion of the population will serve as manpower, leading to bigger manpower pools that replenish faster. Notables will also replenish their volunteer slots quicker."),
                new TextObject("{=!}Increased militarism & draft efficiency"),
                1f);

            DevelopEconomy.Initialize(new TextObject("{=!}Develop Economy"),
                new TextObject("{=!}Foster long-term prosperity within your demesne. Increased caravan attractiveness outputs more trade and tariff revenue."),
                new TextObject("{=!}Increased prosperity\nIncreased caravan attractiveness"),
                0f);

            OverseeProduction.Initialize(new TextObject("{=!}Oversee Production"),
                new TextObject("{=!}Organize production methods and quality standarts Increased production means the population and workshops outputs more products, while increased quality means these products are, on average, of superior quality."),
                new TextObject("{=!}Increased production efficiency & quality"),
                0f);

            PromoteCulture.Initialize(new TextObject("{=!}Promote Culture"),
                new TextObject("{=!}Promote the presence of your culture throughout your fiefs. A populace that follows your culture is more stable, less rebellious and pays more taxes."),
                new TextObject("{=!}Increased cultural presence\nRandomly convert notables to your culture"),
                0f);
            
            OverseeSecurity.Initialize(new TextObject("{=!}Oversee Security"),
                new TextObject("{=!}Tighen up security through repression of criminal activity within your fiefs."),
                new TextObject("{=!}Increased security"),
                1f);

            RepressCriminality.Initialize(new TextObject("{=!}Repress Criminality"),
                new TextObject("{=!}Seek out and destroy criminal networks operating ouside of town boundaries, exterminating sources of criminals that threaten traders and villagers."),
                new TextObject("{=!}Hideouts may be automatically destroyed"),
                1f); 

            ManageVassals.Initialize(new TextObject("{=!}Manage Vassals"),
                new TextObject("{=!}Delegate the handling of vassal relationships, allowing you to maintain more vassals without repercussions."),
                new TextObject("{=!}Increased vassal limit"),
                1f);

            OverseeDignataries.Initialize(new TextObject("{=!}Oversee Dignataries"),
                new TextObject("{=!}Delegate relations with local authorities. Stronger bonds with dignataries such as notables lead to loyal fiefs."),
                new TextObject("{=!}Increased settlement loyalty\nRandomly gain relations with notables"),
                0f);

            ArbitrateRelations.Initialize(new TextObject("{=!}Arbitrate Relations"),
                new TextObject("{=!}Delegate relations with Peers. Improve standing with other families and the clan is more influential in the realm."),
                new TextObject("{=!}Increased influence limit\nRandomly increase relations with clans"),
                0f);

            PromoteFaith.Initialize(new TextObject("{=!}Promote Faith"),
                new TextObject("{=!}Promote the presence of your faith throughout your fiefs."),
                new TextObject("{=!}Increased faith presence\nRandomly convert notables to your faith"),
                0f);

            CultivatePiety.Initialize(new TextObject("{=!}Cultivate Piety"),
                new TextObject("{=!}Foster piety within your family. Increases piety of clan members, may convert members of other faiths."),
                new TextObject("{=!}Increased piety\nRandomly convert clan members to your faith"),
                0f);

            ManageDemesne.Initialize(new TextObject("{=!}Manage Demesne"),
               new TextObject("{=!}Promote the presence of your faith throughout your fiefs."),
               new TextObject("{=!}Fiefs will be automatically assigned adequate governors\nIssues handled by governors will give you the rewards"),
               1f);

            EntertainFeastsMusician.Initialize(new TextObject("{=!}Entertain Feasts"),
               new TextObject("{=!}Entertain your feast guests with music."),
               new TextObject("{=!}Feast guests will have better opinions of your feasts"),
               1f);

            OverseeSanitation.Initialize(new TextObject("{=!}Oversee Sanitation"),
               new TextObject("{=!}Promote sanitation standarts that improve life quality in town. Improved health quality improves population growth."),
               new TextObject("{=!}Improved population growth"),
               1f);

            SmithArmors.Initialize(new TextObject("{=!}Smith Armors"),
               new TextObject("{=!}Forge armor pieces. Items will follow your culture. Their quality is based on the smith's skill - the higher their crafting capability, the better items they can make."),
               new TextObject("{=!}Randomly forge armor pieces"),
               1f);

            SmithBardings.Initialize(new TextObject("{=!}Smith Bardings"),
               new TextObject("{=!}Forge horse bardings. Items will follow your culture. Their quality is based on the smith's skill - the higher their crafting capability, the better items they can make."),
               new TextObject("{=!}Randomly forge horse bardings"),
               1f);

            SmithWeapons.Initialize(new TextObject("{=!}Smith Weapons"),
               new TextObject("{=!}Forge melee weapons. Items will follow your culture. Their quality is based on the smith's skill - the higher their crafting capability, the better items they can make."),
               new TextObject("{=!}Randomly forge melee weapons"),
               1f);

            EducateFamilyAntiquarian.Initialize(new TextObject("{=!}Educate Family"),
               new TextObject("{=!}Educate family members on subjects of history, literature and the sciences."),
               new TextObject("{=!}Scholarship xp for all family members"),
               1f);

            OverseeBaronies.Initialize(new TextObject("{=!}Oversee Baronies"),
               new TextObject("{=!}The castellan is effectively a lord's official in the castellany, their area of jurisdiction. They are responsible for the upkeep and defences of castles, as well as enforcing and passing judgement if necessary."),
               new TextObject("{=!}Improved prosperity for castles and attached villages"),
               1f);

            EnforceLaw.Initialize(new TextObject("{=!}Oversee Baronies"),
               new TextObject("{=!}The castellan is effectively a lord's official in the castellany, their area of jurisdiction. They are responsible for the upkeep and defences of castles, as well as enforcing and passing judgement if necessary."),
               new TextObject("{=!}Improved prosperity for castles and attached villages"),
               1f);
        }
    }
}
