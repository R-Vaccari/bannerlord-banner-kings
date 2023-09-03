using BannerKings.Managers.Kingdoms.Policies;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Titles.Governments
{
    public class DefaultGovernments : DefaultTypeInitializer<DefaultGovernments, Government>
    {
        public Government Republic { get; } = new Government("Republic");
        public Government Imperial { get; } = new Government("Imperial");
        public Government Feudal { get; } = new Government("Feudal");
        public Government Tribal { get; } = new Government("Tribal");

        public override IEnumerable<Government> All
        {
            get
            {
                yield return Feudal;
                yield return Tribal;
                yield return Imperial;
                yield return Republic;
            }
        }

        public Government GetKingdomIdealSuccession(Kingdom kingdom)
        {
            string id = kingdom.StringId;
            if (id.Contains("empire"))
            {
                if (id == "empire")
                {
                    return Republic;
                }

                return Imperial;
            }

            if (id == "aserai" || id == "vlandia")
            {
                return Feudal;
            }

            return Tribal;
        }

        public override void Initialize()
        {
            Republic.Initialize(new TextObject("{=!}Republic"),
                new TextObject(),
                new TextObject(),
                0.5f,
                new List<PolicyObject>()
                {
                    DefaultPolicies.SacredMajesty,
                    DefaultPolicies.CrownDuty,
                    DefaultPolicies.ImperialTowns,
                    DefaultPolicies.RoyalCommissions,
                    DefaultPolicies.RoyalGuard,
                    DefaultPolicies.RoyalPrivilege,
                    DefaultPolicies.WarTax,
                    DefaultPolicies.KingsMercenaries,
                    DefaultPolicies.CastleCharters,
                    DefaultPolicies.StateMonopolies,
                    DefaultPolicies.DebasementOfTheCurrency,
                    DefaultPolicies.LandTax,
                    BKPolicies.Instance.LimitedArmyPrivilege
                },
                new List<Succession>()
                {
                    DefaultSuccessions.Instance.Republic,
                    DefaultSuccessions.Instance.Dictatorship
                });

            Imperial.Initialize(new TextObject("{=!}Imperial"),
                new TextObject(),
                new TextObject(),
                0.5f,
                new List<PolicyObject>()
                {
                    DefaultPolicies.WarTax,
                    DefaultPolicies.LordsPrivyCouncil,
                    DefaultPolicies.FeudalInheritance,
                    DefaultPolicies.CastleCharters,
                    DefaultPolicies.Bailiffs,
                    DefaultPolicies.HuntingRights,
                    DefaultPolicies.GrazingRights,
                    DefaultPolicies.Peerage,
                    DefaultPolicies.Marshals,
                    DefaultPolicies.Cantons,
                    DefaultPolicies.CouncilOfTheCommons,
                    DefaultPolicies.NobleRetinues,
                    BKPolicies.Instance.LimitedArmyPrivilege
                },
                new List<Succession>()
                {
                    DefaultSuccessions.Instance.Imperial,
                    DefaultSuccessions.Instance.Dictatorship
                });

            Tribal.Initialize(new TextObject("{=!}Tribal"),
                new TextObject(),
                new TextObject(),
                0.5f,
                new List<PolicyObject>()
                {
                    DefaultPolicies.SacredMajesty,
                    DefaultPolicies.DebasementOfTheCurrency,
                    DefaultPolicies.CrownDuty,
                    DefaultPolicies.ImperialTowns,
                    DefaultPolicies.RoyalCommissions,
                    DefaultPolicies.RoyalGuard,
                    DefaultPolicies.RoyalPrivilege,
                    DefaultPolicies.KingsMercenaries,
                    DefaultPolicies.CastleCharters,
                    DefaultPolicies.Senate,
                    DefaultPolicies.Citizenship,
                    DefaultPolicies.Magistrates,
                    DefaultPolicies.FeudalInheritance,
                    DefaultPolicies.PrecarialLandTenure,
                    BKPolicies.Instance.LimitedArmyPrivilege,
                },
                new List<Succession>()
                {
                    DefaultSuccessions.Instance.TribalElective,
                    DefaultSuccessions.Instance.WilundingElective,
                    DefaultSuccessions.Instance.BattanianElective
                });

            Feudal.Initialize(new TextObject("{=!}Feudal"),
                new TextObject(),
                new TextObject(),
                0.5f,
                new List<PolicyObject>()
                {
                    DefaultPolicies.ImperialTowns,
                    DefaultPolicies.PrecarialLandTenure,
                    DefaultPolicies.WarTax,
                    DefaultPolicies.StateMonopolies,
                    DefaultPolicies.Senate,
                    DefaultPolicies.CouncilOfTheCommons,
                    DefaultPolicies.Citizenship,
                    DefaultPolicies.TribunesOfThePeople,
                    DefaultPolicies.Cantons,
                    DefaultPolicies.Magistrates
                },
                new List<Succession>()
                {
                    DefaultSuccessions.Instance.FeudalElective,
                    DefaultSuccessions.Instance.WilundingElective,
                    DefaultSuccessions.Instance.TheocraticElective,
                    DefaultSuccessions.Instance.Hereditary
                });
        }
    }
}
