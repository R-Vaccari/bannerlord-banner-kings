using System.Collections.Generic;
using BannerKings.Managers.Titles;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Managers.Helpers
{
    public static class PolicyHelper
    {
        public static List<PolicyObject> GetForbiddenGovernmentPolicies(GovernmentType government)
        {
            var list = new List<PolicyObject>();
            switch (government)
            {
                case GovernmentType.Imperial:
                    list.Add(DefaultPolicies.WarTax);
                    list.Add(DefaultPolicies.LordsPrivyCouncil);
                    list.Add(DefaultPolicies.FeudalInheritance);
                    list.Add(DefaultPolicies.CastleCharters);
                    list.Add(DefaultPolicies.Bailiffs);
                    list.Add(DefaultPolicies.HuntingRights);
                    list.Add(DefaultPolicies.GrazingRights);
                    list.Add(DefaultPolicies.Peerage);
                    list.Add(DefaultPolicies.Marshals);
                    list.Add(DefaultPolicies.Cantons);
                    list.Add(DefaultPolicies.CouncilOfTheCommons);
                    list.Add(DefaultPolicies.NobleRetinues);
                    break;
                case GovernmentType.Republic:
                    list.Add(DefaultPolicies.SacredMajesty);
                    list.Add(DefaultPolicies.CrownDuty);
                    list.Add(DefaultPolicies.ImperialTowns);
                    list.Add(DefaultPolicies.RoyalCommissions);
                    list.Add(DefaultPolicies.RoyalGuard);
                    list.Add(DefaultPolicies.RoyalPrivilege);
                    list.Add(DefaultPolicies.WarTax);
                    list.Add(DefaultPolicies.KingsMercenaries);
                    list.Add(DefaultPolicies.CastleCharters);
                    list.Add(DefaultPolicies.StateMonopolies);
                    list.Add(DefaultPolicies.DebasementOfTheCurrency);
                    list.Add(DefaultPolicies.LandTax);
                    break;
                case GovernmentType.Feudal:
                    list.Add(DefaultPolicies.ImperialTowns);
                    list.Add(DefaultPolicies.PrecarialLandTenure);
                    list.Add(DefaultPolicies.WarTax);
                    list.Add(DefaultPolicies.StateMonopolies);
                    list.Add(DefaultPolicies.Senate);
                    list.Add(DefaultPolicies.CouncilOfTheCommons);
                    list.Add(DefaultPolicies.Citizenship);
                    list.Add(DefaultPolicies.TribunesOfThePeople);
                    list.Add(DefaultPolicies.Cantons);
                    list.Add(DefaultPolicies.Magistrates);
                    break;
                default:
                    list.Add(DefaultPolicies.SacredMajesty);
                    list.Add(DefaultPolicies.DebasementOfTheCurrency);
                    list.Add(DefaultPolicies.CrownDuty);
                    list.Add(DefaultPolicies.ImperialTowns);
                    list.Add(DefaultPolicies.RoyalCommissions);
                    list.Add(DefaultPolicies.RoyalGuard);
                    list.Add(DefaultPolicies.RoyalPrivilege);
                    list.Add(DefaultPolicies.KingsMercenaries);
                    list.Add(DefaultPolicies.CastleCharters);
                    list.Add(DefaultPolicies.Senate);
                    list.Add(DefaultPolicies.Citizenship);
                    list.Add(DefaultPolicies.Magistrates);
                    list.Add(DefaultPolicies.FeudalInheritance);
                    list.Add(DefaultPolicies.PrecarialLandTenure);
                    break;
            }

            return list;
        }
    }
}