using BannerKings.Managers.Populations;
using BannerKings.Managers.Titles.Laws;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Models.Vanilla
{
    public class BKFoodModel : IFoodModel
    {
        public override float NobleFood => -0.1f;
        public override float CraftsmanFood => -0.044f;
        public override float SerfFood => -0.025f;
        public override float TenantFood => -0.035f;
        public override float SlaveFood => -0.0015f;
        public override int FoodStocksUpperLimit => 500;
        public override int NumberOfProsperityToEatOneFood => 40;
        public override int NumberOfMenOnGarrisonToEatOneFood => 20;
        public override int CastleFoodStockUpperLimitBonus => 250;

        public override ExplainedNumber CalculateTownFoodStocksChange(Town town, bool includeMarketStocks = true,
            bool includeDescriptions = false)
        {
            if (BannerKingsConfig.Instance.PopulationManager != null)
            {
                return CalculateTownFoodChangeInternal(town, includeMarketStocks, includeDescriptions);
            }

            return base.CalculateTownFoodStocksChange(town, includeMarketStocks, includeDescriptions);
        }

        public ExplainedNumber CalculateTownFoodChangeInternal(Town town, bool includeMarketStocks, bool includeDescriptions)
        {
            var result = new ExplainedNumber(0f, includeDescriptions);
            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement);

            if (data == null)
            {
                return base.CalculateTownFoodStocksChange(town, includeMarketStocks, includeDescriptions);
            }

            result.Add(GetPopulationFoodConsumption(data).ResultNumber, new TextObject("{=3JzB3jVw}Population Consumption"));

            float foodProduction = GetPopulationFoodProduction(data, town).ResultNumber;
            result.Add(foodProduction, new TextObject("{=AOdwTPTa}Population Production"));

            float season = (float)CampaignTime.Now.GetSeasonOfYear;
            switch (season)
            {
                case 3f:
                    result.Add(-0.5f * foodProduction, new TextObject("{=nwqUFaU8}Winter"));
                    break;
                case 1f:
                    result.Add(0.5f * foodProduction, new TextObject("{=f7vOVQb7}Summer"));
                    break;
            }

            var garrisonParty = town.GarrisonParty;
            var garrisonConsumption = garrisonParty != null ? garrisonParty.Party.NumberOfAllMembers : 0;
            result.Add(-garrisonConsumption / NumberOfMenOnGarrisonToEatOneFood, new TextObject("{=o7W9qvHw}Garrison consumption"));

            var prisoners = town.Settlement.Party.NumberOfPrisoners;
            result.Add(-prisoners / (NumberOfMenOnGarrisonToEatOneFood * 2), new TextObject("{=S3ntMGX6}Prisoner rations"));

            if (BannerKingsConfig.Instance.PolicyManager.IsDecisionEnacted(town.Settlement, "decision_militia_encourage"))
            {
                result.AddFactor(-0.25f, new TextObject("{=1aq83aPr}Conscription policy"));
            }

            var ownerClan = town.Settlement.OwnerClan;
            if (ownerClan?.Kingdom != null &&
                town.Settlement.OwnerClan.Kingdom.ActivePolicies.Contains(DefaultPolicies.HuntingRights))
            {
                result.Add(2f, DefaultPolicies.HuntingRights.Name);
            }

            var marketConsumption = 0;
            foreach (var sellLog in town.SoldItems)
            {
                if (sellLog.Category.Properties == ItemCategory.Property.BonusToFoodStores)
                {
                    marketConsumption += sellLog.Number;
                }
            }

            result.Add(marketConsumption, new TextObject("{=3hEk7Bdk}Market consumption"));

            if (town.Governor != null)
            {
                if (town.IsUnderSiege)
                {
                    if (town.Governor.GetPerkValue(DefaultPerks.Steward.Gourmet))
                    {
                        result.AddFactor(DefaultPerks.Steward.Gourmet.SecondaryBonus, DefaultPerks.Steward.Gourmet.Name);
                    }

                    if (town.Governor.GetPerkValue(DefaultPerks.Medicine.TriageTent))
                    {
                        result.AddFactor(DefaultPerks.Medicine.TriageTent.SecondaryBonus,
                            DefaultPerks.Medicine.TriageTent.Name);
                    }
                }

                if (town.Governor.GetPerkValue(DefaultPerks.Roguery.DirtyFighting))
                {
                    result.Add(DefaultPerks.Roguery.DirtyFighting.SecondaryBonus, DefaultPerks.Roguery.DirtyFighting.Name);
                }

                if (result.ResultNumber > 0f && town.Governor.GetPerkValue(DefaultPerks.Steward.MasterOfWarcraft))
                {
                    result.AddFactor(-DefaultPerks.Steward.MasterOfWarcraft.SecondaryBonus,
                        DefaultPerks.Steward.MasterOfWarcraft.Name);
                }
            }

            GetSettlementFoodChangeDueToIssues(town, ref result);
            return result;
        }

        public int GetFoodEstimate(Settlement settlement, int maxStocks)
        {
            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
            var result = GetPopulationFoodConsumption(data);
            var finalResult = (int) (maxStocks / (result.ResultNumber * -1f));
            return finalResult;
        }

        public override ExplainedNumber GetPopulationFoodConsumption(PopulationData data)
        {
            var result = new ExplainedNumber();
            result.LimitMin(-3000f);
            result.LimitMax(0f);
            var citySerfs = data.GetTypeCount(PopType.Serfs);
            if (citySerfs > 0)
            {
                var serfConsumption = citySerfs * SerfFood;
                result.Add(serfConsumption, new TextObject("{=jH7cWD5r}Serfs consumption"));
            }

            var citySlaves = data.GetTypeCount(PopType.Slaves);
            if (citySlaves > 0)
            {
                var slaveConsumption = citySlaves * SlaveFood;
                result.Add(slaveConsumption, new TextObject("{=8xhVr4rK}Slaves consumption"));
            }

            var cityNobles = data.GetTypeCount(PopType.Nobles);
            if (cityNobles > 0)
            {
                var nobleConsumption = cityNobles * NobleFood;
                result.Add(nobleConsumption, new TextObject("{=myyYr6BO}Nobles consumption"));
            }

            var cityCraftsmen = data.GetTypeCount(PopType.Craftsmen);
            if (cityCraftsmen > 0)
            {
                var craftsmenConsumption = cityCraftsmen * CraftsmanFood;
                result.Add(craftsmenConsumption, new TextObject("{=!}Craftsmen consumption"));
            }

            var cityTenants = data.GetTypeCount(PopType.Tenants);
            if (cityTenants > 0)
            {
                var tenantsConsumption = cityTenants * TenantFood;
                result.Add(tenantsConsumption, new TextObject("{=!}Tenants consumption"));
            }

            if (BannerKingsConfig.Instance.PolicyManager.IsDecisionEnacted(data.Settlement, "decision_ration"))
            {
                result.AddFactor(-0.4f, new TextObject("{=w6bLP4DB}Enforce rations decision"));
            }

            if (data.Settlement.IsCastle)
            {
                result.AddFactor(-0.1f, new TextObject("{=j0bzPuzg}Castle rations"));
            }

            return result;
        }

        public override ExplainedNumber GetPopulationFoodProduction(PopulationData data, Town town)
        {
            var result = new ExplainedNumber();
            result.LimitMin(0f);
            if (!town.IsUnderSiege)
            {
                var landData = data.LandData;
                var serfName = Utils.Helpers.GetClassName(PopType.Serfs, town.Culture);
                var slaveName = Utils.Helpers.GetClassName(PopType.Slaves, town.Culture);
                var tenantsName = Utils.Helpers.GetClassName(PopType.Tenants, town.Culture);

                float serfs = data.LandData.AvailableSerfsWorkForce;
                float slaves = data.LandData.AvailableSlavesWorkForce;
                float tenants = data.LandData.AvailableTenantsWorkForce;
                float totalWorkforce = serfs + slaves + tenants;

                float serfProportion = serfs / totalWorkforce;
                float slaveProportion = slaves / totalWorkforce;
                float tenantsProportion = tenants / totalWorkforce;

                result.Add((landData.Farmland * serfProportion) * landData.GetAcreClassOutput("farmland", PopType.Serfs),
                    new TextObject("{=8Wuxnwnf}Farmlands ({CLASS})")
                    .SetTextVariable("CLASS", serfName));

                result.Add((landData.Farmland * slaveProportion) * landData.GetAcreClassOutput("farmland", PopType.Slaves),
                    new TextObject("{=8Wuxnwnf}Farmlands ({CLASS})")
                    .SetTextVariable("CLASS", slaveName));

                result.Add((landData.Farmland * tenantsProportion) * landData.GetAcreClassOutput("farmland", PopType.Tenants),
                   new TextObject("{=8Wuxnwnf}Farmlands ({CLASS})")
                   .SetTextVariable("CLASS", tenantsName));


                result.Add(landData.Pastureland * landData.GetAcreOutput("pasture"), new TextObject("{=ngRhXYj1}Pasturelands"));
                result.Add(landData.Woodland * landData.GetAcreOutput("wood"), new TextObject("{=qPQ7HKgG}Woodlands"));
                var fertility = landData.Fertility - 1f;
                if (fertility != 0f)
                {
                    var toDeduce = result.ResultNumber * fertility;
                    result.Add(toDeduce, new TextObject("{=KcNcxeMK}Fertility"));
                }

                var efficiency = data.EconomicData.ProductionEfficiency.ResultNumber - 1f;
                if (efficiency != 0f)
                {
                    var toDeduce = result.ResultNumber * efficiency;
                    result.Add(toDeduce, new TextObject("{=Q0AgGuB0}Production efficiency"));
                }

                result.AddFactor(MathF.Clamp(data.LandData.WorkforceSaturation - 1f, -1f, 0f), new TextObject("{=LohssChh}Workforce saturation"));

                Building b = null;
                foreach (var building in town.Buildings)
                {
                    if (building.BuildingType == DefaultBuildingTypes.CastleGardens ||
                        building.BuildingType == DefaultBuildingTypes.SettlementWorkshop)
                    {
                        b = building;
                        break;
                    }
                }

                if (b is {CurrentLevel: > 0})
                {
                    result.AddFactor(b.CurrentLevel * (town.IsCastle ? 0.5f : 0.3f), b.Name);
                }

                if (data.TitleData != null && data.TitleData.Title != null)
                {
                    var title = data.TitleData.Title;
                    if (title.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.SerfsLaxDuties))
                    {
                        result.AddFactor(-0.05f, DefaultDemesneLaws.Instance.SerfsLaxDuties.Name);
                    }
                    else if (title.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.SerfsAgricultureDuties))
                    {
                        result.AddFactor(0.1f, DefaultDemesneLaws.Instance.SerfsAgricultureDuties.Name);
                    }
                }   
            }

            return result;
        }

        private static void GetSettlementFoodChangeDueToIssues(Town town, ref ExplainedNumber explainedNumber)
        {
            TaleWorlds.CampaignSystem.Campaign.Current.Models.IssueModel.GetIssueEffectsOfSettlement(DefaultIssueEffects.SettlementFood,
                town.Settlement, ref explainedNumber);
        }
    }
}