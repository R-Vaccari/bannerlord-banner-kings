using System.Linq;
using BannerKings.Managers.Court;
using BannerKings.Managers.Policies;
using BannerKings.Managers.Titles;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static BannerKings.Managers.Policies.BKCriminalPolicy;
using static BannerKings.Managers.Policies.BKTaxPolicy;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Models.Vanilla
{
    internal class BKLoyaltyModel : DefaultSettlementLoyaltyModel
    {
        private static readonly float SLAVE_LOYALTY = -0.00035f;
        private static readonly float LOYALTY_FACTOR = 4f;

        private static readonly TextObject StarvingText = GameTexts.FindText("str_starving");
        private static readonly TextObject CultureText = new("{=LHFoaUGo}Owner Culture");
        private static readonly TextObject NotableText = GameTexts.FindText("str_notable_relations");
        private static readonly TextObject ParadePerkBonus = new("{=fZYeszid}Parade perk bonus");
        private static readonly TextObject GovernorCultureText = new("{=fVeD8UC2}Governor's Culture");
        private static readonly TextObject SecurityText = GameTexts.FindText("str_security");
        private static readonly TextObject LoyaltyDriftText = GameTexts.FindText("str_loyalty_drift");

        public override ExplainedNumber CalculateLoyaltyChange(Town town, bool includeDescriptions = false)
        {
            if (BannerKingsConfig.Instance.PopulationManager != null &&
                BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(town.Settlement))
            {
                var baseResult = CalculateLoyaltyChangeInternal(town, true);
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement);
                var slaves = data.GetTypeCount(PopType.Slaves);
                var surplusExists =
                    BannerKingsConfig.Instance.PopulationManager.PopSurplusExists(town.Settlement, PopType.Slaves, true);
                baseResult.Add(slaves * SLAVE_LOYALTY, new TextObject("{=FJSfBwzp}Slave population"));

                var tax = (BannerKingsConfig.Instance.PolicyManager.GetPolicy(town.Settlement, "tax") as BKTaxPolicy)
                    .Policy;
                switch (tax)
                {
                    case TaxType.Low:
                    {
                        var fraction1 = data.GetCurrentTypeFraction(PopType.Craftsmen);
                        var fraction2 = data.GetCurrentTypeFraction(PopType.Serfs) * 0.8f;
                        baseResult.Add((fraction1 + fraction2) * LOYALTY_FACTOR, new TextObject("{=j6AoAS6n}Low tax policy"));
                        break;
                    }
                    case TaxType.High:
                    {
                        var fraction1 = data.GetCurrentTypeFraction(PopType.Craftsmen);
                        var fraction2 = data.GetCurrentTypeFraction(PopType.Serfs) * 0.8f;
                        baseResult.Add((fraction1 + fraction2) * LOYALTY_FACTOR * -1f, new TextObject("{=EhHXS8PN}High tax policy"));
                        break;
                    }
                }

                if (BannerKingsConfig.Instance.PolicyManager.IsDecisionEnacted(town.Settlement, "decision_slaves_tax"))
                {
                    var factor = tax == TaxType.Low ? 1.5f : tax == TaxType.Standard ? 2f : 2.5f;
                    var privateSlaves = 1f - data.EconomicData.StateSlaves;
                    baseResult.Add(privateSlaves * -factor, new TextObject("{=AQDh5jHA}Tax private slaves decision"));
                }

                var crime =
                    (BannerKingsConfig.Instance.PolicyManager.GetPolicy(town.Settlement, "criminal") as BKCriminalPolicy)
                    .Policy;
                switch (crime)
                {
                    case CriminalPolicy.Execution:
                    {
                        var value = 0f;
                        if (data.CultureData.DominantCulture == town.Owner.Culture)
                        {
                            value = 0.3f;
                        }
                        else
                        {
                            value = -0.3f;
                        }

                        baseResult.Add(value, new TextObject("{=qyjqPWxJ}Criminal policy"));
                        break;
                    }
                    case CriminalPolicy.Forgiveness:
                    {
                        var value = 0f;
                        if (data.CultureData.DominantCulture != town.Owner.Culture)
                        {
                            value = 0.3f;
                        }
                        else
                        {
                            value = -0.3f;
                        }

                        baseResult.Add(value, new TextObject("{=qyjqPWxJ}Criminal policy"));
                        break;
                    }
                }

                if (BannerKingsConfig.Instance.PolicyManager.IsDecisionEnacted(town.Settlement, "decision_ration"))
                {
                    baseResult.Add(town.IsUnderSiege || town.FoodStocks >= town.FoodStocksUpperLimit() * 0.1f ? -2f : -4f,
                        new TextObject("{=w6bLP4DB}Enforce rations decision"));
                }

                var government = BannerKingsConfig.Instance.TitleManager.GetSettlementGovernment(town.Settlement);
                if (government == GovernmentType.Republic)
                {
                    baseResult.Add(1f, new TextObject("{=PSrEtF5L}Government"));
                }

                baseResult.Add(2f * data.Autonomy, new TextObject("Autonomy"));

                BannerKingsConfig.Instance.CourtManager.ApplyCouncilEffect(ref baseResult, town.OwnerClan.Leader,
                    CouncilPosition.Chancellor, 1f, false);
                return baseResult;
            }

            return base.CalculateLoyaltyChange(town, includeDescriptions);
        }


        private ExplainedNumber CalculateLoyaltyChangeInternal(Town town, bool includeDescriptions = false)
        {
            var result = new ExplainedNumber(0f, includeDescriptions);
            GetSettlementLoyaltyChangeDueToFoodStocks(town, ref result);
            GetSettlementLoyaltyChangeDueToGovernorCulture(town, ref result);
            GetSettlementLoyaltyChangeDueToOwnerCulture(town, ref result);
            GetSettlementLoyaltyChangeDueToPolicies(town, ref result);
            GetSettlementLoyaltyChangeDueToProjects(town, ref result);
            GetSettlementLoyaltyChangeDueToIssues(town, ref result);
            GetSettlementLoyaltyChangeDueToSecurity(town, ref result);
            GetSettlementLoyaltyChangeDueToNotableRelations(town, ref result);
            GetSettlementLoyaltyChangeDueToGovernorPerks(town, ref result);
            GetSettlementLoyaltyChangeDueToLoyaltyDrift(town, ref result);
            return result;
        }

        // Token: 0x06002D67 RID: 11623 RVA: 0x000B46D4 File Offset: 0x000B28D4
        private void GetSettlementLoyaltyChangeDueToGovernorPerks(Town town, ref ExplainedNumber explainedNumber)
        {
            PerkHelper.AddPerkBonusForTown(DefaultPerks.Leadership.HeroicLeader, town, ref explainedNumber);
            PerkHelper.AddPerkBonusForTown(DefaultPerks.Charm.NaturalLeader, town, ref explainedNumber);
            PerkHelper.AddPerkBonusForTown(DefaultPerks.Medicine.PhysicianOfPeople, town, ref explainedNumber);
            PerkHelper.AddPerkBonusForTown(DefaultPerks.Athletics.HealthyCitizens, town, ref explainedNumber);
            PerkHelper.AddPerkBonusForTown(DefaultPerks.Bow.Discipline, town, ref explainedNumber);
            if (town.Settlement.Parties.Any(x =>
                    x.LeaderHero != null && x.LeaderHero.Clan == town.Settlement.OwnerClan &&
                    x.HasPerk(DefaultPerks.Charm.Parade)))
            {
                explainedNumber.Add(DefaultPerks.Charm.Parade.PrimaryBonus, ParadePerkBonus);
            }
        }

        // Token: 0x06002D68 RID: 11624 RVA: 0x000B477C File Offset: 0x000B297C
        private void GetSettlementLoyaltyChangeDueToNotableRelations(Town town, ref ExplainedNumber explainedNumber)
        {
            var num = 0f;
            foreach (var hero in town.Settlement.Notables)
            {
                if (hero.SupporterOf != null)
                {
                    if (hero.SupporterOf == town.Settlement.OwnerClan)
                    {
                        num += 0.5f;
                    }
                    else if (town.Settlement.OwnerClan.IsAtWarWith(hero.SupporterOf))
                    {
                        num += -0.5f;
                    }
                }
            }

            if (num > 0f)
            {
                explainedNumber.Add(num, NotableText);
            }
        }

        private void GetSettlementLoyaltyChangeDueToOwnerCulture(Town town, ref ExplainedNumber explainedNumber)
        {
            if (BannerKingsConfig.Instance.PopulationManager != null &&
                BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(town.Settlement))
            {
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement);
                var assim = data.CultureData.GetAssimilation(town.Owner.Culture);
                var factor = assim - 1f + assim;
                var result = LOYALTY_FACTOR * factor;
                explainedNumber.Add(result, new TextObject("{=D3trXTDz}Cultural Assimilation"));

                if (town.Governor != null)
                {
                    explainedNumber.Add(result * (town.Governor.Culture == town.Culture ? 0.1f : -0.1f),
                        GovernorCultureText);
                }
            }
            else if (town.Settlement.OwnerClan.Culture != town.Settlement.Culture) // vanilla behavior
            {
                explainedNumber.Add(-3f, CultureText);
            }
        }

        private void GetSettlementLoyaltyChangeDueToPolicies(Town town, ref ExplainedNumber explainedNumber)
        {
            var kingdom = town.Owner.Settlement.OwnerClan.Kingdom;
            if (kingdom != null)
            {
                if (kingdom.ActivePolicies.Contains(DefaultPolicies.Citizenship))
                {
                    if (town.Settlement.OwnerClan.Culture == kingdom.RulingClan.Culture)
                    {
                        explainedNumber.Add(0.5f, DefaultPolicies.Citizenship.Name);
                    }

                    else
                    {
                        explainedNumber.Add(-0.5f, DefaultPolicies.Citizenship.Name);
                    }
                }

                if (kingdom.ActivePolicies.Contains(DefaultPolicies.HuntingRights))
                {
                    explainedNumber.Add(-0.2f, DefaultPolicies.HuntingRights.Name);
                }

                if (kingdom.ActivePolicies.Contains(DefaultPolicies.GrazingRights))
                {
                    explainedNumber.Add(0.5f, DefaultPolicies.GrazingRights.Name);
                }

                if (kingdom.ActivePolicies.Contains(DefaultPolicies.TrialByJury))
                {
                    explainedNumber.Add(0.5f, DefaultPolicies.TrialByJury.Name);
                }

                if (kingdom.ActivePolicies.Contains(DefaultPolicies.ImperialTowns))
                {
                    if (kingdom.RulingClan == town.Settlement.OwnerClan)
                    {
                        explainedNumber.Add(1f, DefaultPolicies.ImperialTowns.Name);
                    }

                    else
                    {
                        explainedNumber.Add(-0.3f, DefaultPolicies.ImperialTowns.Name);
                    }
                }

                if (kingdom.ActivePolicies.Contains(DefaultPolicies.ForgivenessOfDebts))
                {
                    explainedNumber.Add(2f, DefaultPolicies.ForgivenessOfDebts.Name);
                }

                if (kingdom.ActivePolicies.Contains(DefaultPolicies.TribunesOfThePeople))
                {
                    explainedNumber.Add(1f, DefaultPolicies.TribunesOfThePeople.Name);
                }

                if (kingdom.ActivePolicies.Contains(DefaultPolicies.DebasementOfTheCurrency))
                {
                    explainedNumber.Add(-1f, DefaultPolicies.DebasementOfTheCurrency.Name);
                }
            }
        }

        private void GetSettlementLoyaltyChangeDueToGovernorCulture(Town town, ref ExplainedNumber explainedNumber)
        {
            if (BannerKingsConfig.Instance.PopulationManager != null &&
                BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(town.Settlement))
            {
                // Ignore if populated. Governor effect is calculated in GetSettlementLoyaltyChangeDueToOwnerCulture
            }
            else if (town.Governor != null)
            {
                explainedNumber.Add(town.Governor.Culture == town.Culture ? 1f : -1f, GovernorCultureText);
            }
        }

        private void GetSettlementLoyaltyChangeDueToFoodStocks(Town town, ref ExplainedNumber explainedNumber)
        {
            var foodLimitForBonus = (int) (town.FoodStocksUpperLimit() * 0.8f);
            if (town.FoodStocks >= foodLimitForBonus)
            {
                explainedNumber.Add(0.5f, new TextObject("{=9Jyv5XNX}Well fed populace"));
            }
            else if (town.Settlement.IsStarving)
            {
                explainedNumber.Add(-2f, StarvingText);
            }
        }

        private void GetSettlementLoyaltyChangeDueToSecurity(Town town, ref ExplainedNumber explainedNumber)
        {
            var value = town.Security > 50f
                ? MBMath.Map(town.Security, 50f, 100f, 0f, 1f)
                : MBMath.Map(town.Security, 0f, 50f, -2f, 0f);
            explainedNumber.Add(value, SecurityText);
        }

        private void GetSettlementLoyaltyChangeDueToProjects(Town town, ref ExplainedNumber explainedNumber)
        {
            if (town.BuildingsInProgress.IsEmpty() &&
                town.CurrentDefaultBuilding.BuildingType == DefaultBuildingTypes.FestivalsAndGamesDaily)
            {
                BuildingHelper.AddDefaultDailyBonus(town, BuildingEffectEnum.LoyaltyDaily, ref explainedNumber);
            }

            foreach (var building in town.Buildings)
            {
                var buildingEffectAmount = building.GetBuildingEffectAmount(BuildingEffectEnum.Loyalty);
                if (!building.BuildingType.IsDefaultProject && buildingEffectAmount > 0f)
                {
                    explainedNumber.Add(buildingEffectAmount, building.Name);
                }
            }
        }


        private void GetSettlementLoyaltyChangeDueToIssues(Town town, ref ExplainedNumber explainedNumber)
        {
            Campaign.Current.Models.IssueModel.GetIssueEffectsOfSettlement(DefaultIssueEffects.SettlementLoyalty,
                town.Settlement, ref explainedNumber);
        }

        private void GetSettlementLoyaltyChangeDueToLoyaltyDrift(Town town, ref ExplainedNumber explainedNumber)
        {
            explainedNumber.Add(-1f * (town.Loyalty - 50f) * 0.1f, LoyaltyDriftText);
        }
    }
}