using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using static Populations.PopulationManager;

namespace Populations.Models
{
    class FoodModel : DefaultSettlementFoodModel
    {

		public FoodModel()
        {
			DefaultSettlementFoodModel.FoodStocksUpperLimit = 500;
		}

        public override ExplainedNumber CalculateTownFoodStocksChange(Town town, bool includeDescriptions = false)
        {
			if (IsSettlementPopulated(town.Settlement)) return CalculateTownFoodChangeInternal(town, includeDescriptions);
			else return base.CalculateTownFoodStocksChange(town, includeDescriptions);
        }

        public ExplainedNumber CalculateTownFoodChangeInternal(Town town, bool includeDescriptions)
		{
			ExplainedNumber result = new ExplainedNumber(0f, includeDescriptions, null);
			PopulationData data = GetPopData(town.Settlement);
			int citySerfs = data.GetTypeCount(PopType.Serfs);
			if (!town.IsUnderSiege)
            {
				float serfProduction = (float)citySerfs * SERF_FOOD *0.8f;
				result.Add((float)serfProduction, new TextObject("Serfs production)", null));
			} else
            {
				float serfProduction = (float)citySerfs * SERF_FOOD * -1f;
				result.Add((float)serfProduction, new TextObject("Serfs consumption)", null));
			}
			

			int cityNobles = data.GetTypeCount(PopType.Nobles);
			float nobleConsumption = (float)cityNobles * NOBLE_FOOD;
			result.Add((float)nobleConsumption, new TextObject("Nobles consumption)", null));

			int cityCraftsmen = data.GetTypeCount(PopType.Craftsmen);
			float craftsmenConsumption = (float)cityCraftsmen * CRAFTSMEN_FOOD;
			result.Add((float)craftsmenConsumption, new TextObject("Craftsmen consumption)", null));

			float num2 = -town.Owner.Settlement.Prosperity / 40f;
			MobileParty garrisonParty = town.GarrisonParty;
			int num3 = (garrisonParty != null) ? garrisonParty.Party.NumberOfAllMembers : 0;
			num3 = -num3 / 20;
			float num4 = 0f;
			float num5 = 0f;
			if (town.Governor != null)
			{
				if (town.IsUnderSiege)
				{
					if (town.Governor.GetPerkValue(DefaultPerks.Steward.Gourmet))
					{
						num5 += DefaultPerks.Steward.Gourmet.SecondaryBonus;
					}
					if (town.Governor.GetPerkValue(DefaultPerks.Medicine.TriageTent))
					{
						num4 += DefaultPerks.Medicine.TriageTent.SecondaryBonus;
					}
				}
				if (town.Governor.GetPerkValue(DefaultPerks.Steward.MasterOfWarcraft))
				{
					num4 += DefaultPerks.Steward.MasterOfWarcraft.SecondaryBonus;
				}
			}
			num2 += num2 * num4;
			num3 += (int)((float)num3 * (num4 + num5));
			result.Add(num2, new TextObject("Prosperity effect"), null);
			result.Add((float)num3, new TextObject("Garrison consumption"), null);
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
			if (IsSettlementPopulated(village.Settlement))
			{
				PopulationData data = GetPopData(village.Settlement);
				int serfs = data.GetTypeCount(PopType.Serfs);
				int slaves = data.GetTypeCount(PopType.Slaves);
				if (IsVillageProducingFood(village))
					return (float)(serfs + slaves) * SERF_FOOD;
				else if (IsVillageAMine(village))
					return (float)serfs * SERF_FOOD + (float)slaves * SLAVE_MINE_FOOD;
				else
					return ((float)(serfs + slaves) * SERF_FOOD) / 2f;
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

	
		private static void GetSettlementFoodChangeDueToIssues(Town town, ref ExplainedNumber explainedNumber)
		{
			Campaign.Current.Models.IssueModel.GetIssueEffectsOfSettlement(DefaultIssueEffects.SettlementFood, town.Settlement, ref explainedNumber);
		}

	}
}
