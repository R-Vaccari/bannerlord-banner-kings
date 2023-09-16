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
                new TextObject("{=!}Republics are a Calradoi government with the purpose of preventing authoritarian regimes of power. Before the Calradoi organized themselves into the empire, they were a republic, government by a body of peers called the Senate. They were traditionally quite resistant to any attempts of power concentration at the hands of one or another senator, lest they turn into a monarchical regime... Yet, at times of war, strong, unquestioned leadership was needed, wich could be granted to a temporary senator holding all the power - the Diktator. History teaches that one Diktator too influential is all it takes to topple a republic."),
                new TextObject("{=!}- Every year, an election for ruler takes place (Republican succession){newline}- Settlement production quality +10%{newline}- Settlement loyalty +1{newline}- Settlement mercantilism +50%"),
                0.5f,
                -1f,
                0.6f,
                1f,
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
                new TextObject("{=!}- Settlement stability +5%{newline}- Settlement security +1{newline}- Settlement mercantilism +20%"),
                0.2f,
                1f,
                -0.2f,
                -0.5f,
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
                new TextObject("{=!}Tribal"),
                new TextObject("{=!}- Settlement draft efficiency +20%{newline}- Settlement mercantilism +30%"),
                0.3f,
                -0.7f,
                0.5f,
                0.4f,
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
                new TextObject("{=!}- +15% settlement production efficiency{newline}- Settlement mercantilism +15%"),
                0.15f,
                -0.4f,
                0.7f,
                -0.2f,
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
