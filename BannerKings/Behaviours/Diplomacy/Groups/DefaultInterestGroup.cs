using BannerKings.Behaviours.Diplomacy.Groups.Demands;
using BannerKings.Behaviours.Diplomacy.Wars;
using BannerKings.Managers.Court.Members;
using BannerKings.Managers.Titles.Laws;
using BannerKings.Managers.Traits;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Diplomacy.Groups
{
    public class DefaultInterestGroup : DefaultTypeInitializer<DefaultInterestGroup, InterestGroup>
    {
        public InterestGroup Royalists { get; } = new InterestGroup("royalists");
        public InterestGroup Traditionalists { get; } = new InterestGroup("traditionalists");
        public InterestGroup Oligarchists { get; } = new InterestGroup("oligarchists");
        public InterestGroup Zealots { get; } = new InterestGroup("zealots");
        public InterestGroup Gentry { get; } = new InterestGroup("gentry");
        public InterestGroup Guilds { get; } = new InterestGroup("guilds");
        public InterestGroup Commoners { get; } = new InterestGroup("commoners");
        public override IEnumerable<InterestGroup> All
        {
            get
            {
                yield return Royalists;
                yield return Traditionalists;
                yield return Oligarchists;
                yield return Zealots;
                yield return Commoners;
            }
        }

        public override void Initialize()
        {
            Royalists.Initialize(new TextObject("{=!}Royalists"),
                new TextObject("{=!}The royalists are those who support the royal administration. They favor the sovereign even over themselves, be for true loyalty or for the prospect of compensation. Royalists support everything that benefits the royal administration regardless of impacts on other groups."),
                DefaultTraits.Authoritarian,
                true,
                true,
                true,
                new List<Occupation>()
                {
                },
                new List<PolicyObject>()
                {
                    DefaultPolicies.SacredMajesty,
                    DefaultPolicies.StateMonopolies,
                    DefaultPolicies.CrownDuty,
                    DefaultPolicies.RoyalCommissions,
                    DefaultPolicies.RoyalGuard,
                    DefaultPolicies.RoyalPrivilege,
                    DefaultPolicies.PrecarialLandTenure,
                    DefaultPolicies.LandTax,
                    DefaultPolicies.ImperialTowns
                },
                new List<PolicyObject>()
                {
                    DefaultPolicies.Marshals,
                    DefaultPolicies.FeudalInheritance,
                    DefaultPolicies.WarTax,
                    DefaultPolicies.Peerage
                },
                new List<DemesneLaw>()
                {
                    DefaultDemesneLaws.Instance.SlaveryManumission
                },
                new List<DemesneLaw>()
                {
                    DefaultDemesneLaws.Instance.SlaveryAserai
                },
                new List<CasusBelli>()
                {
                    DefaultCasusBelli.Instance.CulturalLiberation
                },
                new List<Demand>()
                {
                    DefaultDemands.Instance.CouncilPosition,
                },
                null);

            Traditionalists.Initialize(new TextObject("{=!}Traditionalists"),
               new TextObject("{=!}Traditionalists support the status quo and the way of the ancestors. More than anything, they believe in strength. While they recognize the necessity for a strong ruler, they also do the necessity of liberty for the noble classes. They aim for a balance of power between the sovereign and their vassals, a continuation of old traditions and exerting power through force."),
               BKTraits.Instance.Diligent,
               true,
               true,
               true,
               new List<Occupation>()
               {
                   Occupation.Lord
               },
               new List<PolicyObject>()
               {
                    DefaultPolicies.CrownDuty,
                    DefaultPolicies.RoyalGuard,
                    DefaultPolicies.FeudalInheritance,
                    DefaultPolicies.RoadTolls,
                    DefaultPolicies.Lawspeakers,
                    DefaultPolicies.MilitaryCoronae,
                    DefaultPolicies.LandGrantsForVeterans,
                    DefaultPolicies.Serfdom
               },
               new List<PolicyObject>()
               {
                    DefaultPolicies.Marshals,
                    DefaultPolicies.PrecarialLandTenure,
                    DefaultPolicies.WarTax,
                    DefaultPolicies.Magistrates,
                    DefaultPolicies.ForgivenessOfDebts,
                    DefaultPolicies.TribunesOfThePeople,
                    DefaultPolicies.TrialByJury
               },
               new List<DemesneLaw>()
               {
                    DefaultDemesneLaws.Instance.NoblesMilitaryServiceDuties,
                    DefaultDemesneLaws.Instance.CraftsmenMilitaryServiceDuties,
                    DefaultDemesneLaws.Instance.SerfsMilitaryServiceDuties
               },
               new List<DemesneLaw>()
               {
                    DefaultDemesneLaws.Instance.SlaveryManumission,
                    DefaultDemesneLaws.Instance.NoblesLaxDuties,
                    DefaultDemesneLaws.Instance.CraftsmenLaxDuties,
                    DefaultDemesneLaws.Instance.SerfsLaxDuties
               },
               new List<CasusBelli>()
               {
                    DefaultCasusBelli.Instance.CulturalLiberation,
                    DefaultCasusBelli.Instance.Invasion,
                    DefaultCasusBelli.Instance.GreatRaid
               },
               new List<Demand>()
               {
                   DefaultDemands.Instance.CouncilPosition,
               },
               null);

            Oligarchists.Initialize(new TextObject("{=!}Oligarchists"),
               new TextObject("{=!}Oligarchists are noble people of influence that are concerned, first and foremost, with their own advantage. They continuously seek benefits for themselves or their peers, understanding they are part of the same class. Thus, their interests are often misaligned both with the ruler's, and with the common people."),
               DefaultTraits.Oligarchic,
               true,
               false,
               true,
               new List<Occupation>()
               {
                   Occupation.Lord
               },
               new List<PolicyObject>()
               {
                   DefaultPolicies.Senate,
                   DefaultPolicies.FeudalInheritance,
                   DefaultPolicies.NobleRetinues,
                   DefaultPolicies.Peerage,
                   DefaultPolicies.Marshals,
                   DefaultPolicies.Serfdom,
                   DefaultPolicies.WarTax,
                   DefaultPolicies.LordsPrivyCouncil,
                   DefaultPolicies.MilitaryCoronae
               },
               new List<PolicyObject>()
               {
                    DefaultPolicies.SacredMajesty,
                    DefaultPolicies.StateMonopolies,
                    DefaultPolicies.RoyalCommissions,
                    DefaultPolicies.RoyalGuard,
                    DefaultPolicies.RoyalPrivilege,
                    DefaultPolicies.PrecarialLandTenure,
                    DefaultPolicies.LandTax,
                    DefaultPolicies.KingsMercenaries,
                    DefaultPolicies.ImperialTowns,
                    DefaultPolicies.TrialByJury,
                    DefaultPolicies.HuntingRights
               },
               new List<DemesneLaw>()
               {
                    DefaultDemesneLaws.Instance.SlaveryManumission
               },
               new List<DemesneLaw>()
               {
                    DefaultDemesneLaws.Instance.SlaveryAserai
               },
               new List<CasusBelli>()
               {
                    DefaultCasusBelli.Instance.CulturalLiberation
               },
               new List<Demand>()
               {
                   DefaultDemands.Instance.CouncilPosition,
               },
               null);

            Zealots.Initialize(new TextObject("{=!}Zealots"),
                new TextObject(),
                BKTraits.Instance.Zealous,
                true,
                true,
                true,
                new List<Occupation>()
                {
                    Occupation.Preacher
                },
                new List<PolicyObject>()
                {
                    DefaultPolicies.Citizenship,
                    DefaultPolicies.ForgivenessOfDebts,
                    DefaultPolicies.GrazingRights,
                    DefaultPolicies.TribunesOfThePeople,
                    DefaultPolicies.LandGrantsForVeterans,
                    DefaultPolicies.CouncilOfTheCommons,
                    DefaultPolicies.HuntingRights,
                    DefaultPolicies.TrialByJury
                },
                new List<PolicyObject>()
                {
                    DefaultPolicies.WarTax,
                    DefaultPolicies.RoadTolls,
                    DefaultPolicies.Serfdom
                },
                new List<DemesneLaw>()
                {
                    DefaultDemesneLaws.Instance.SlaveryManumission,
                    DefaultDemesneLaws.Instance.SerfsLaxDuties,
                    DefaultDemesneLaws.Instance.CraftsmenLaxDuties
                },
                new List<DemesneLaw>()
                {
                    DefaultDemesneLaws.Instance.SlaveryAserai,
                    DefaultDemesneLaws.Instance.SlaveryStandard,
                    DefaultDemesneLaws.Instance.SlaveryVlandia,
                    DefaultDemesneLaws.Instance.SerfsAgricultureDuties,
                    DefaultDemesneLaws.Instance.SerfsMilitaryServiceDuties,
                    DefaultDemesneLaws.Instance.CraftsmenMilitaryServiceDuties,
                    DefaultDemesneLaws.Instance.CraftsmenTaxDuties
                },
                new List<CasusBelli>()
                {
                    DefaultCasusBelli.Instance.CulturalLiberation
                },
                new List<Demand>()
                {
                    DefaultDemands.Instance.CouncilPosition,
                },
                DefaultCouncilPositions.Instance.Spiritual);

            Commoners.Initialize(new TextObject("{=!}Commoners"),
                new TextObject(),
                BKTraits.Instance.Just,
                false,
                true,
                false,
                new List<Occupation>()
                {
                    Occupation.Headman
                },
                new List<PolicyObject>()
                {
                    DefaultPolicies.Citizenship,
                    DefaultPolicies.ForgivenessOfDebts,
                    DefaultPolicies.GrazingRights,
                    DefaultPolicies.TribunesOfThePeople,
                    DefaultPolicies.LandGrantsForVeterans,
                    DefaultPolicies.CouncilOfTheCommons,
                    DefaultPolicies.HuntingRights,
                    DefaultPolicies.TrialByJury
                },
                new List<PolicyObject>()
                {
                    DefaultPolicies.WarTax,
                    DefaultPolicies.RoadTolls,
                    DefaultPolicies.Serfdom
                },
                new List<DemesneLaw>()
                {
                    DefaultDemesneLaws.Instance.SlaveryManumission,
                    DefaultDemesneLaws.Instance.SerfsLaxDuties,
                    DefaultDemesneLaws.Instance.CraftsmenLaxDuties
                },
                new List<DemesneLaw>()
                {
                    DefaultDemesneLaws.Instance.SlaveryAserai,
                    DefaultDemesneLaws.Instance.SlaveryStandard,
                    DefaultDemesneLaws.Instance.SlaveryVlandia,
                    DefaultDemesneLaws.Instance.SerfsAgricultureDuties,
                    DefaultDemesneLaws.Instance.SerfsMilitaryServiceDuties,
                    DefaultDemesneLaws.Instance.CraftsmenMilitaryServiceDuties,
                    DefaultDemesneLaws.Instance.CraftsmenTaxDuties
                },
                new List<CasusBelli>()
                {
                    DefaultCasusBelli.Instance.CulturalLiberation
                },
                new List<Demand>()
                {
                    DefaultDemands.Instance.CouncilPosition,
                },
                null);
        }
    }
}
