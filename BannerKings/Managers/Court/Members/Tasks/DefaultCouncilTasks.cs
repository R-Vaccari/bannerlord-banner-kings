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
        public CouncilTask FamilyCare { get; } = new CouncilTask("FamilyCare");
        public OverseeSanitation OverseeSanitation { get; } = new OverseeSanitation();
        public CouncilTask GatherLegion { get; } = new CouncilTask("GatherLegion");

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
                yield return OverseeBaronies;
                yield return EnforceLaw;
                yield return FamilyCare;
                yield return GatherLegion;
                foreach (CouncilTask item in ModAdditions)
                {
                    yield return item;
                }
            }
        }

        public override void Initialize()
        {
            GatherLegion.Initialize(new TextObject("{=!}Gather Legion"),
                new TextObject("{=!}Harness the state-granted power to gather a legion. Under the legions demesne law, legates are capable of gathering armies, a privilege otherwise usually restricted to the ruler and their marshal."),
                new TextObject("{=!}Increased chance of gathering a legion"),
                1f);

            OrganizeMiltia.Initialize(new TextObject("{=WQRibYcW}Organize Militia"),
                new TextObject("{=ZVXnyVN3}Enact militia trainning drills and the obligation of households owning arms. Through stimulation of the state, more workforce members become active part of fief militias, as well as with generally improved trainning and equipment."),
                new TextObject("{=5sazP1rU}Increased militia production\nImproved militia quality"),
                0f);

            EncourageMilitarism.Initialize(new TextObject("{=FXZETOgg}Encourage Militarism"),
                new TextObject("{=DViCQ3L7}Encourage militarism within the populace. More militarism means a larger portion of the population will serve as manpower, leading to bigger manpower pools that replenish faster. Notables will also replenish their volunteer slots quicker."),
                new TextObject("{=zFwDhSTs}Increased militarism & draft efficiency"),
                1f);

            DevelopEconomy.Initialize(new TextObject("{=K8nUXVve}Develop Economy"),
                new TextObject("{=i7cD9XCL}Foster long-term prosperity within your demesne. Increased caravan attractiveness outputs more trade and tariff revenue."),
                new TextObject("{=y1jcxLnC}Increased prosperity\nIncreased caravan attractiveness"),
                0f);

            OverseeProduction.Initialize(new TextObject("{=gdAKEowx}Oversee Production"),
                new TextObject("{=Es4VmNJs}Organize production methods and quality standarts Increased production means the population and workshops outputs more products, while increased quality means these products are, on average, of superior quality."),
                new TextObject("{=kDeTbH8b}Increased production efficiency & quality"),
                0f);

            PromoteCulture.Initialize(new TextObject("{=CmPrt049}Promote Culture"),
                new TextObject("{=Vkx4Vka4}Promote the presence of your culture throughout your fiefs. A populace that follows your culture is more stable, less rebellious and pays more taxes."),
                new TextObject("{=Lp8Rdrjm}Increased cultural presence\nRandomly convert notables to your culture"),
                0f);
            
            OverseeSecurity.Initialize(new TextObject("{=t12urUmZ}Oversee Security"),
                new TextObject("{=fiGcYFEO}Tighen up security through repression of criminal activity within your fiefs."),
                new TextObject("{=x59AZOX9}Increased security"),
                1f);

            RepressCriminality.Initialize(new TextObject("{=ojHwJr7W}Repress Criminality"),
                new TextObject("{=Co828LTB}Seek out and destroy criminal networks operating ouside of town boundaries, exterminating sources of criminals that threaten traders and villagers."),
                new TextObject("{=9YZwe7gF}Hideouts may be automatically destroyed"),
                1f); 

            ManageVassals.Initialize(new TextObject("{=kveiriMe}Manage Vassals"),
                new TextObject("{=e2GHMzQy}Delegate the handling of vassal relationships, allowing you to maintain more vassals without repercussions."),
                new TextObject("{=mvavHNQn}Increased vassal limit"),
                1f);

            OverseeDignataries.Initialize(new TextObject("{=m8BAzkfq}Oversee Dignataries"),
                new TextObject("{=DCMYNZpd}Delegate relations with local authorities. Stronger bonds with dignataries such as notables lead to loyal fiefs."),
                new TextObject("{=beXFEM0K}Increased settlement loyalty\nRandomly gain relations with notables"),
                0f);

            ArbitrateRelations.Initialize(new TextObject("{=HkLuhv7Y}Arbitrate Relations"),
                new TextObject("{=e16thLp4}Delegate relations with Peers. Improve standing with other families and the clan is more influential in the realm."),
                new TextObject("{=XaDKn2mM}Increased influence limit\nRandomly increase relations with clans"),
                0f);

            PromoteFaith.Initialize(new TextObject("{=F6TozKia}Promote Faith"),
                new TextObject("{=J13PZvJS}Promote the presence of your faith throughout your fiefs."),
                new TextObject("{=Ey3Vc66Q}Increased faith presence\nRandomly convert notables to your faith"),
                0f);

            CultivatePiety.Initialize(new TextObject("{=zdtoHqhQ}Cultivate Piety"),
                new TextObject("{=88okbUiV}Foster piety within your family. Increases piety of clan members, may convert members of other faiths."),
                new TextObject("{=TgjqSaem}Increased piety\nRandomly convert clan members to your faith"),
                0f);

            ManageDemesne.Initialize(new TextObject("{=SwEz8RuK}Manage Demesne"),
               new TextObject("{=J13PZvJS}Promote the presence of your faith throughout your fiefs."),
               new TextObject("{=9DyVa4Tc}Fiefs will be automatically assigned adequate governors\nIssues handled by governors will give you the rewards"),
               1f);

            EntertainFeastsMusician.Initialize(new TextObject("{=UoQbHh0v}Entertain Feasts"),
               new TextObject("{=QhB6WR8Z}Entertain your feast guests with music."),
               new TextObject("{=9GWVmtRs}Feast guests will have better opinions of your feasts"),
               1f);

            OverseeSanitation.Initialize(new TextObject("{=PdmYfN4Y}Oversee Sanitation"),
               new TextObject("{=OHWoKDeu}Promote sanitation standarts that improve life quality in town. Improved health quality improves population growth."),
               new TextObject("{=5TiaPyyt}Improved population growth"),
               1f);

            FamilyCare.Initialize(new TextObject("{=EZWTi70m}Family Care"),
               new TextObject("{=XiMo6cPC}Heal family members and educate them into good health practices."),
               new TextObject("{=QCw05MZN}Household members heal faster in settlements\nDaily medicine xp for household members"),
               1f);

            SmithArmors.Initialize(new TextObject("{=nmaSK0ca}Smith Armors"),
               new TextObject("{=q2TF0mzR}Forge armor pieces. Items will follow your culture. Their quality is based on the smith's skill - the higher their crafting capability, the better items they can make."),
               new TextObject("{=RvRk10xc}Randomly forge armor pieces"),
               1f);

            SmithBardings.Initialize(new TextObject("{=EgxmFkyv}Smith Bardings"),
               new TextObject("{=Kar7ghbg}Forge horse bardings. Items will follow your culture. Their quality is based on the smith's skill - the higher their crafting capability, the better items they can make."),
               new TextObject("{=Fv8EKJMB}Randomly forge horse bardings"),
               1f);

            SmithWeapons.Initialize(new TextObject("{=yb6KCpEx}Smith Weapons"),
               new TextObject("{=y7OnC1VF}Forge melee weapons. Items will follow your culture. Their quality is based on the smith's skill - the higher their crafting capability, the better items they can make."),
               new TextObject("{=Ei6Z42ut}Randomly forge melee weapons"),
               1f);

            EducateFamilyAntiquarian.Initialize(new TextObject("{=TveF1tT5}Educate Family"),
               new TextObject("{=Vvxpoosf}Educate family members on subjects of history, literature and the sciences."),
               new TextObject("{=W4JiUzdP}Scholarship xp for all family members"),
               1f);

            OverseeBaronies.Initialize(new TextObject("{=a39kNwH5}Oversee Baronies"),
               new TextObject("{=rN1SK85E}The castellan is effectively a lord's official in the castellany, their area of jurisdiction. They are responsible for the upkeep and defences of castles, as well as enforcing and passing judgement if necessary."),
               new TextObject("{=pTSRDrba}Improved prosperity for castles and attached villages"),
               1f);

            EnforceLaw.Initialize(new TextObject("{=0bDAuMZe}Enforce Law"),
               new TextObject("{=RQq8tssi}Oversee the enforcement of law through force or otherwise. Improved enforcement of laws provides a prosperous habitat for trade."),
               new TextObject("{=o27axaVY}Improved security\nImproved caravan attractiveness"),
               1f);
        }
    }
}
