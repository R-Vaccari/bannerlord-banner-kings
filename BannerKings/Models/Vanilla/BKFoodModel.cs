using BannerKings.Managers;
using BannerKings.Populations;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Models
{
    class BKFoodModel : SettlementFoodModel
	{

		private static readonly float SLAVE_MINE_FOOD = 0f;
		private static readonly float NOBLE_FOOD = -0.01f;
		private static readonly float CRAFTSMEN_FOOD = -0.005f;
		private static readonly float SERF_FOOD = 0.006f;

		public override int FoodStocksUpperLimit => 500;
        public override int NumberOfProsperityToEatOneFood => 40;
        public override int NumberOfMenOnGarrisonToEatOneFood => 20;
        public override int CastleFoodStockUpperLimitBonus => 150;

        public override ExplainedNumber CalculateTownFoodStocksChange(Town town, bool includeDescriptions = false)
        {
			if (BannerKingsConfig.Instance.PopulationManager != null && 
				BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(town.Settlement)) 
				return CalculateTownFoodChangeInternal(town, includeDescriptions);
			else return new DefaultSettlementFoodModel().CalculateTownFoodStocksChange(town, includeDescriptions);
        }

        public ExplainedNumber CalculateTownFoodChangeInternal(Town town, bool includeDescriptions)
		{
			//InformationManager.DisplayMessage(new InformationMessage("Food model running..."));
			ExplainedNumber result = new ExplainedNumber(0f, includeDescriptions, null);

			// ------- Pops / Prosperity consumption ---------
			PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement);
			int citySerfs = data.GetTypeCount(PopType.Serfs);
			if (!town.IsUnderSiege)
            {
				float serfProduction = (float)citySerfs * SERF_FOOD * (town.IsCastle ? 1f : 0.4f);
				if (BannerKingsConfig.Instance.PolicyManager.GetSettlementWork(town.Settlement) == PolicyManager.WorkforcePolicy.Construction)
					serfProduction *= 0.85f;
					
				result.Add((float)serfProduction, new TextObject("Serfs production", null));
			} else
            {
				float serfConsumption = (float)citySerfs * SERF_FOOD * -1f;
				result.Add((float)serfConsumption, new TextObject("Serfs consumption (siege)", null));

				int citySlaves = data.GetTypeCount(PopType.Slaves);
				float slaveConsumption = (float)citySlaves * SERF_FOOD * -0.5f;
				result.Add((float)slaveConsumption, new TextObject("Slaves consumption (siege)", null));
			}
			

			int cityNobles = data.GetTypeCount(PopType.Nobles);
			float nobleConsumption = (float)cityNobles * NOBLE_FOOD;
			result.Add((float)nobleConsumption, new TextObject("Nobles consumption", null));

			int cityCraftsmen = data.GetTypeCount(PopType.Craftsmen);
			float craftsmenConsumption = (float)cityCraftsmen * CRAFTSMEN_FOOD;
			result.Add((float)craftsmenConsumption, new TextObject("Craftsmen consumption", null));

			float prosperityImpact = -town.Owner.Settlement.Prosperity / (town.IsCastle ? 100f : 40f);
			result.Add(prosperityImpact, new TextObject("Prosperity effect"), null);

			MobileParty garrisonParty = town.GarrisonParty;
			int garrisonConsumption = (garrisonParty != null) ? garrisonParty.Party.NumberOfAllMembers : 0;
			result.Add((float)(-garrisonConsumption / NumberOfMenOnGarrisonToEatOneFood), new TextObject("Garrison consumption"), null);

			int prisoners = town.Settlement.Party.NumberOfPrisoners;
			result.Add((float)(-prisoners / (NumberOfMenOnGarrisonToEatOneFood * 2)), new TextObject("Prisoner rations"), null);

			if (BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(town.Settlement, PolicyManager.PolicyType.CONSCRIPTION))
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

			if (BannerKingsConfig.Instance.PolicyManager.GetSettlementWork(town.Settlement) == PolicyManager.WorkforcePolicy.Martial_Law)
            {
				float militia = town.Militia;
				result.Add(militia * -0.05f, new TextObject("Martial Law policy"));
            }

			// ------- Other factors ---------
			Clan ownerClan = town.Settlement.OwnerClan;
			if (((ownerClan != null) ? ownerClan.Kingdom : null) != null && town.Settlement.OwnerClan.Kingdom.ActivePolicies.Contains(DefaultPolicies.HuntingRights))
			{
				result.Add(2f, DefaultPolicies.HuntingRights.Name, null);
			}
			if (!town.Owner.Settlement.IsUnderSiege)
			{
				foreach (Village village in town.Owner.Settlement.BoundVillages)
				{
					if (village.VillageState == Village.VillageStates.Normal)
					{
						float villageFood = CalculateVillageProduction(village);
						result.Add((float)villageFood, village.Name, null);
					}
					else
					{
						int num6 = 0;
						result.Add((float)num6, village.Name, null);
					}
				}
				using (List<Building>.Enumerator enumerator2 = town.Buildings.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						Building building = enumerator2.Current;
						float buildingEffectAmount = building.GetBuildingEffectAmount(BuildingEffectEnum.FoodProduction);
						if (buildingEffectAmount > 0f)
						{
							result.Add(buildingEffectAmount, building.Name, null);
						}
					}
					goto IL_296;
				}
			}
			if (town.Governor != null && town.Governor.GetPerkValue(DefaultPerks.Roguery.DirtyFighting))
			{
				result.Add(DefaultPerks.Roguery.DirtyFighting.SecondaryBonus, DefaultPerks.Roguery.DirtyFighting.Name, null);
			}
			else
			{
				result.Add(0f, new TextObject("Village not producing"), null);
			}
			IL_296:
			foreach (Town.SellLog sellLog in town.SoldItems)
			{
				if (sellLog.Category.Properties == ItemCategory.Property.BonusToFoodStores)
				{
					result.Add((float)sellLog.Number, sellLog.Category.GetName(), null);
				}
			}
			GetSettlementFoodChangeDueToIssues(town, ref result);
			return result;
		}

		private float CalculateVillageProduction(Village village)
        {
			if (BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(village.Settlement))
			{
				PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(village.Settlement);
				float food = 0;
				int nobles = data.GetTypeCount(PopType.Nobles);
				int serfs = data.GetTypeCount(PopType.Serfs);
				int slaves = data.GetTypeCount(PopType.Slaves);
				if (IsVillageProducingFood(village))
					food += (float)(serfs + slaves) * SERF_FOOD * 4f;
				else if (IsVillageAMine(village))
					food += (float)serfs * SERF_FOOD;
				else
					food += ((float)(serfs + slaves) * SERF_FOOD) * 1.2f;

				food += (float)nobles * NOBLE_FOOD;

				return food;
			}
			else return 10;
		}

		private int CalculateFoodPurchasedFromMarket(Town town)
		{
			return Enumerable.Sum<Town.SellLog>(town.SoldItems, delegate (Town.SellLog x)
			{
				if (x.Category.Properties != ItemCategory.Property.BonusToFoodStores)
				{
					return 0;
				}
				return x.Number;
			});
		}

		public int GetFoodEstimate(Town town, bool includeDescriptions, int maxStocks)
		{
			//InformationManager.DisplayMessage(new InformationMessage("Food model running..."));
			ExplainedNumber result = new ExplainedNumber(0f, includeDescriptions, null);

			// ------- Pops / Prosperity consumption ---------
			PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement);
			int citySerfs = data.GetTypeCount(PopType.Serfs);

			float serfConsumption = (float)citySerfs * SERF_FOOD * -1f;
			result.Add((float)serfConsumption, new TextObject("Serfs consumption (siege)", null));

			int citySlaves = data.GetTypeCount(PopType.Slaves);
			float slaveConsumption = (float)citySlaves * SERF_FOOD * -0.5f;
			result.Add((float)slaveConsumption, new TextObject("Slaves consumption (siege)", null));
			

			int cityNobles = data.GetTypeCount(PopType.Nobles);
			float nobleConsumption = (float)cityNobles * NOBLE_FOOD;
			result.Add((float)nobleConsumption, new TextObject("Nobles consumption", null));

			int cityCraftsmen = data.GetTypeCount(PopType.Craftsmen);
			float craftsmenConsumption = (float)cityCraftsmen * CRAFTSMEN_FOOD;
			result.Add((float)craftsmenConsumption, new TextObject("Craftsmen consumption", null));

			float prosperityImpact = -town.Owner.Settlement.Prosperity / (town.IsCastle ? 100f : 40f);
			result.Add(prosperityImpact, new TextObject("Prosperity effect"), null);

			MobileParty garrisonParty = town.GarrisonParty;
			int garrisonConsumption = (garrisonParty != null) ? garrisonParty.Party.NumberOfAllMembers : 0;
			result.Add((float)(-garrisonConsumption / NumberOfMenOnGarrisonToEatOneFood), new TextObject("Garrison consumption"), null);

			if (BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(town.Settlement, PolicyManager.PolicyType.CONSCRIPTION))
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
				result.Add(2f, DefaultPolicies.HuntingRights.Name, null);
			
			
			if (town.Governor != null && town.Governor.GetPerkValue(DefaultPerks.Roguery.DirtyFighting))
			
				result.Add(DefaultPerks.Roguery.DirtyFighting.SecondaryBonus, DefaultPerks.Roguery.DirtyFighting.Name, null);
			
			else
			{
				result.Add(0f, new TextObject("Village not producing"), null);
			}
		IL_296:
			foreach (Town.SellLog sellLog in town.SoldItems)
			{
				if (sellLog.Category.Properties == ItemCategory.Property.BonusToFoodStores)
				{
					result.Add((float)sellLog.Number, sellLog.Category.GetName(), null);
				}
			}
			GetSettlementFoodChangeDueToIssues(town, ref result);

			int finalResult = (int)((float)maxStocks / result.ResultNumber);
			return -finalResult;
		}


		private static void GetSettlementFoodChangeDueToIssues(Town town, ref ExplainedNumber explainedNumber)
		{
			Campaign.Current.Models.IssueModel.GetIssueEffectsOfSettlement(DefaultIssueEffects.SettlementFood, town.Settlement, ref explainedNumber);
		}

	}
}
