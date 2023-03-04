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
        public CouncilTask IntegrateTitles { get; } = new CouncilTask("IntegrateTitles");

        public CouncilTask PromoteFaith { get; } = new CouncilTask("PromoteFaith");

        public CouncilTask ManageDemesne { get; } = new CouncilTask("ManageDemesne");

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
                yield return ManageVassals;
                yield return PromoteFaith;
                yield return ManageDemesne;
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

            PromoteFaith.Initialize(new TextObject("{=!}Promote Faith"),
               new TextObject("{=!}Promote the presence of your faith throughout your fiefs."),
               new TextObject("{=!}Increased faith presence\nRandomly convert notables to your faith"),
               0f);

            ManageDemesne.Initialize(new TextObject("{=!}Manage Demesne"),
              new TextObject("{=!}Promote the presence of your faith throughout your fiefs."),
              new TextObject("{=!}Fiefs will be automatically assigned adequate governors\nIssues handled by governors will give you the rewards"),
              1f);
        }
    }
}
