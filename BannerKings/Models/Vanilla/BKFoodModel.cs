﻿using BannerKings.Populations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Models
{
    class BKFoodModel : SettlementFoodModel
	{
		private static readonly float NOBLE_FOOD = -0.1f;
		private static readonly float CRAFTSMEN_FOOD = -0.05f;
		private static readonly float SERF_FOOD = 0.03f;

		public override int FoodStocksUpperLimit => 500;
        public override int NumberOfProsperityToEatOneFood => 40;
        public override int NumberOfMenOnGarrisonToEatOneFood => 20;
        public override int CastleFoodStockUpperLimitBonus => 150;

        public override ExplainedNumber CalculateTownFoodStocksChange(Town town, bool includeDescriptions = false)
        {
	        if (BannerKingsConfig.Instance.PopulationManager != null && 
	            BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(town.Settlement)) 
				return CalculateTownFoodChangeInternal(town, includeDescriptions);
	        return new DefaultSettlementFoodModel().CalculateTownFoodStocksChange(town, includeDescriptions);
        }

        public ExplainedNumber CalculateTownFoodChangeInternal(Town town, bool includeDescriptions)
		{
			//InformationManager.DisplayMessage(new InformationMessage("Food model running..."));
			ExplainedNumber result = new ExplainedNumber(0f, includeDescriptions);

			// ------- Pops / Prosperity consumption ---------
			PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement);

			result.Add(GetPopulationFoodConsumption(data).ResultNumber, new TextObject("{=!}Population Consumption"));
			result.Add(GetPopulationFoodProduction(data, town).ResultNumber, new TextObject("{=!}Population Production"));

			//float prosperityImpact = -town.Owner.Settlement.Prosperity / (town.IsCastle ? 400f : 120f);
			//result.Add(prosperityImpact, new TextObject("Prosperity effect"), null);

			MobileParty garrisonParty = town.GarrisonParty;
			int garrisonConsumption = (garrisonParty != null) ? garrisonParty.Party.NumberOfAllMembers : 0;
			result.Add(-garrisonConsumption / NumberOfMenOnGarrisonToEatOneFood, new TextObject("Garrison consumption"));

			int prisoners = town.Settlement.Party.NumberOfPrisoners;
			result.Add(-prisoners / (NumberOfMenOnGarrisonToEatOneFood * 2), new TextObject("Prisoner rations"));

			if (BannerKingsConfig.Instance.PolicyManager.IsDecisionEnacted(town.Settlement, "decision_militia_encourage"))
				result.AddFactor(-0.25f, new TextObject("Conscription policy"));
	
			if (town.Governor != null)
			{
				if (town.IsUnderSiege)
				{
					if (town.Governor.GetPerkValue(DefaultPerks.Steward.Gourmet))
						result.AddFactor(DefaultPerks.Steward.Gourmet.SecondaryBonus, DefaultPerks.Steward.Gourmet.Name);
					
					if (town.Governor.GetPerkValue(DefaultPerks.Medicine.TriageTent))
						result.AddFactor(DefaultPerks.Medicine.TriageTent.SecondaryBonus, DefaultPerks.Medicine.TriageTent.Name);
				}
				if (town.Governor.GetPerkValue(DefaultPerks.Steward.MasterOfWarcraft))
					result.AddFactor(DefaultPerks.Steward.MasterOfWarcraft.SecondaryBonus, DefaultPerks.Steward.MasterOfWarcraft.Name);	
			}

			// ------- Other factors ---------
			Clan ownerClan = town.Settlement.OwnerClan;
			if (((ownerClan != null) ? ownerClan.Kingdom : null) != null && town.Settlement.OwnerClan.Kingdom.ActivePolicies.Contains(DefaultPolicies.HuntingRights))
				result.Add(2f, DefaultPolicies.HuntingRights.Name);
			
			if (town.Governor != null && town.Governor.GetPerkValue(DefaultPerks.Roguery.DirtyFighting))
				result.Add(DefaultPerks.Roguery.DirtyFighting.SecondaryBonus, DefaultPerks.Roguery.DirtyFighting.Name);

			int marketConsumption = 0;
			foreach (Town.SellLog sellLog in town.SoldItems)
				if (sellLog.Category.Properties == ItemCategory.Property.BonusToFoodStores)
					marketConsumption += sellLog.Number;

			result.Add(marketConsumption, new TextObject("{=!}Market consumption"));

			GetSettlementFoodChangeDueToIssues(town, ref result);
			return result;
		}

		public int GetFoodEstimate(Settlement settlement, int maxStocks)
		{
			PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
			ExplainedNumber result = GetPopulationFoodConsumption(data);
			int finalResult = (int)(maxStocks / (result.ResultNumber * -1f));
			return finalResult;
		}

		public ExplainedNumber GetPopulationFoodConsumption(PopulationData data)
        {
			ExplainedNumber result = new ExplainedNumber();
			result.LimitMin(-1000f);
			result.LimitMax(0f);
			int citySerfs = data.GetTypeCount(PopType.Serfs);
			float serfConsumption = citySerfs * SERF_FOOD * -1f;
			result.Add(serfConsumption, new TextObject("Serfs consumption"));

			int citySlaves = data.GetTypeCount(PopType.Slaves);
			float slaveConsumption = citySlaves * SERF_FOOD * -0.5f;
			result.Add(slaveConsumption, new TextObject("Slaves consumption"));

			int cityNobles = data.GetTypeCount(PopType.Nobles);
			float nobleConsumption = cityNobles * NOBLE_FOOD;
			result.Add(nobleConsumption, new TextObject("Nobles consumption"));

			int cityCraftsmen = data.GetTypeCount(PopType.Craftsmen);
			float craftsmenConsumption = cityCraftsmen * CRAFTSMEN_FOOD;
			result.Add(craftsmenConsumption, new TextObject("Craftsmen consumption"));

			if (BannerKingsConfig.Instance.PolicyManager.IsDecisionEnacted(data.Settlement, "decision_ration"))
				result.AddFactor(-0.4f, new TextObject("{=!}Enforce rations decision"));

			if (data.Settlement.IsCastle)
				result.AddFactor(-0.1f, new TextObject("{=!}Castle rations"));

			return result;
		}

		public ExplainedNumber GetPopulationFoodProduction(PopulationData data, Town town)
        {
			ExplainedNumber result = new ExplainedNumber();
			result.LimitMin(0f);
			result.LimitMax(1000f);
			LandData landData = data.LandData;
			result.Add(landData.Farmland * 0.018f, new TextObject("{=!}Farmlands"));
			result.Add(landData.Pastureland * 0.005f, new TextObject("{=!}Pasturelands"));
			result.Add(landData.Woodland * 0.001f, new TextObject("{=!}Woodlands"));
			float fertility = landData.Fertility - 1f;
			if (fertility != 0f) result.AddFactor(fertility, new TextObject("{=!}Fertility"));
			float saturation = MBMath.ClampFloat(landData.WorkforceSaturation, 0f, 1f) - 1f;
			if (saturation != 0f) result.AddFactor(saturation, new TextObject("{=!}Workforce Saturation"));

			if (town != null)
			{
				Building b = null;
				foreach (Building building in town.Buildings)
					if (building.BuildingType == DefaultBuildingTypes.CastleGardens ||
						building.BuildingType == DefaultBuildingTypes.SettlementWorkshop)
					{
						b = building;
						break;
					}

				if (b != null && b.CurrentLevel > 0)
					result.AddFactor(b.CurrentLevel * (town.IsCastle ? 0.5f : 0.3f), b.Name);
			}

			return result;
		}

		private static void GetSettlementFoodChangeDueToIssues(Town town, ref ExplainedNumber explainedNumber)
		{
			Campaign.Current.Models.IssueModel.GetIssueEffectsOfSettlement(DefaultIssueEffects.SettlementFood, town.Settlement, ref explainedNumber);
		}
	}
}
