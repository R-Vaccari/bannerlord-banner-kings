
using BannerKings.Behaviours.Diplomacy.Wars;
using BannerKings.Managers.Titles.Laws;
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
        public override IEnumerable<InterestGroup> All => throw new System.NotImplementedException();

        public override void Initialize()
        {
            Royalists.Initialize(new TextObject(),
                new TextObject(),
                DefaultTraits.Authoritarian,
                false,
                true,
                false,
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
                });

            Traditionalists.Initialize(new TextObject(),
               new TextObject(),
               DefaultTraits.Authoritarian,
               false,
               true,
               false,
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
               });

            Oligarchists.Initialize(new TextObject(),
               new TextObject(),
               DefaultTraits.Oligarchic,
               false,
               true,
               false,
               new List<Occupation>()
               {
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
               });


            Commoners.Initialize(new TextObject(),
                new TextObject(),
                DefaultTraits.Generosity,
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
                    DefaultPolicies.CharterOfLiberties,
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
                });
        }
    }
}
