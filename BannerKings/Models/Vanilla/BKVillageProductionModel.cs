using BannerKings.Managers.Populations.Villages;
using BannerKings.Populations;
using Helpers;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;
using TaleWorlds.Core;

namespace BannerKings.Models
{
	class BKVillageProductionModel : DefaultVillageProductionCalculatorModel
	{
		private static readonly float PRODUCTION = 0.0005f;
		private static readonly float BOOSTED_PRODUCTION = 0.00125f;
		public override float CalculateDailyProductionAmount(Village village, ItemObject item)
		{
			if (village.Settlement != null && village.VillageState == Village.VillageStates.Normal && BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(village.Settlement))
			{
				ExplainedNumber explainedNumber = new ExplainedNumber(0f);
				PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(village.Settlement);
				VillageData villageData = data.VillageData;
				float serfs = data.GetTypeCount(Managers.PopulationManager.PopType.Serfs) * 0.85f;
				float slaves = data.GetTypeCount(Managers.PopulationManager.PopType.Slaves);

				List<(ItemObject, float)> productions = BannerKingsConfig.Instance.PopulationManager.GetProductions(villageData);
				float totalWeight = 0f;
				foreach (ValueTuple<ItemObject, float> valueTuple in productions)
					totalWeight += valueTuple.Item2;


				foreach (ValueTuple<ItemObject, float> valueTuple in productions)
				{
					ItemObject output = valueTuple.Item1;
					if (output == item)
					{
						float weight = valueTuple.Item2 / totalWeight;
						explainedNumber.Add(GetWorkforceOutput(serfs * weight, slaves * weight, item, data.LandData).ResultNumber);

						if (item.IsMountable && item.Tier == ItemObject.ItemTiers.Tier2 && PerkHelper.GetPerkValueForTown(DefaultPerks.Riding.Horde, village.TradeBound.Town) && MBRandom.RandomFloat < DefaultPerks.Riding.Horde.SecondaryBonus * 0.01f)
							explainedNumber.Add(1f);

						if (item.ItemCategory == DefaultItemCategories.Grain || item.ItemCategory == DefaultItemCategories.Olives || item.ItemCategory == DefaultItemCategories.Fish || item.ItemCategory == DefaultItemCategories.DateFruit)
							PerkHelper.AddPerkBonusForTown(DefaultPerks.Trade.GranaryAccountant, village.TradeBound.Town, ref explainedNumber);

						else if (item.ItemCategory == DefaultItemCategories.Clay || item.ItemCategory == DefaultItemCategories.Iron
							|| item.ItemCategory == DefaultItemCategories.Cotton || item.ItemCategory == DefaultItemCategories.Silver)
							PerkHelper.AddPerkBonusForTown(DefaultPerks.Trade.TradeyardForeman, village.TradeBound.Town, ref explainedNumber);

						if (item.IsTradeGood)
							PerkHelper.AddPerkBonusForTown(DefaultPerks.Athletics.Steady, village.TradeBound.Town, ref explainedNumber);

						if (PerkHelper.GetPerkValueForTown(DefaultPerks.Riding.Breeder, village.TradeBound.Town))
							PerkHelper.AddPerkBonusForTown(DefaultPerks.Riding.Breeder, village.TradeBound.Town, ref explainedNumber);

						if (item.IsAnimal)
							PerkHelper.AddPerkBonusForTown(DefaultPerks.Medicine.PerfectHealth, village.TradeBound.Town, ref explainedNumber);

						BannerKingsConfig.Instance.PopulationManager.ApplyProductionBuildingEffect(ref explainedNumber, output, villageData);

						CharacterObject characterObject = village.Settlement.OwnerClan.Leader.CharacterObject;
						if (characterObject != null)
						{
							if (characterObject.Culture.HasFeat(DefaultCulturalFeats.KhuzaitAnimalProductionFeat) && (item.ItemCategory == DefaultItemCategories.Sheep || item.ItemCategory == DefaultItemCategories.Cow || item.ItemCategory == DefaultItemCategories.WarHorse || item.ItemCategory == DefaultItemCategories.Horse || item.ItemCategory == DefaultItemCategories.PackAnimal))
								explainedNumber.AddFactor(DefaultCulturalFeats.KhuzaitAnimalProductionFeat.EffectBonus, GameTexts.FindText("str_culture"));

							if (village.Bound.IsCastle && characterObject.Culture.HasFeat(DefaultCulturalFeats.VlandianCastleVillageProductionFeat))
								explainedNumber.AddFactor(DefaultCulturalFeats.VlandianCastleVillageProductionFeat.EffectBonus, GameTexts.FindText("str_culture"));
						}

						bool production = !villageData.IsCurrentlyBuilding && villageData.CurrentDefault.BuildingType == DefaultVillageBuildings.Instance.DailyProduction;
						explainedNumber.AddFactor(production ? 0.15f : -0.10f);

						explainedNumber.AddFactor(data.EconomicData.ProductionEfficiency.ResultNumber - 1f);
					}
				}

				return explainedNumber.ResultNumber;
			}

			return base.CalculateDailyProductionAmount(village, item);
		}

		private ExplainedNumber GetWorkforceOutput(float serfs, float slaves, ItemObject item, LandData data)
		{
			ExplainedNumber result = new ExplainedNumber();
			result.LimitMin(0f);
			result.LimitMax(200f);
			if (serfs < 0f || float.IsNaN(serfs))
				serfs = 1f;

			if (slaves < 0f || float.IsNaN(slaves))
				slaves = 1f;

			if (item.StringId == "hardwood" || item.StringId == "fur")
			{
				float acres = data.Woodland;
				float maxWorkforce = acres / data.GetRequiredLabor("wood");
				float workforce = Math.Min(maxWorkforce, serfs + slaves);
				result.Add(workforce * data.GetAcreOutput("wood") * 15f);
			}
			else if ((item.IsAnimal || item.IsMountable) && item.HorseComponent != null)
			{
				float acres = data.Pastureland;
				float maxWorkforce = acres / data.GetRequiredLabor("pasture");
				float workforce = Math.Min(maxWorkforce, serfs + slaves);
				result.Add(workforce * data.GetAcreOutput("pasture") * item.HorseComponent.MeatCount);
			}
			else if (item.IsFood && item.StringId != "fish")
			{
				float acres = data.Farmland;
				float maxWorkforce = acres / data.GetRequiredLabor("farmland");
				float workforce = Math.Min(maxWorkforce, serfs + slaves);
				result.Add(workforce * data.GetAcreOutput("farmland"));
				float serfFactor = serfs / workforce;
				if (serfFactor > 0f) result.AddFactor(serfFactor * 0.5f);
			}
			else
			{
				result.Add(serfs * (item.IsFood ? BOOSTED_PRODUCTION : PRODUCTION));
				result.Add(slaves * (item.StringId == "clay" || item.StringId == "iron" || item.StringId == "salt" || item.StringId == "silver" ?
					BOOSTED_PRODUCTION : PRODUCTION));
			}

			return result;
		}
	}
}