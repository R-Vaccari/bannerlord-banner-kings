using BannerKings.Managers.Populations.Villages;
using BannerKings.Populations;
using Helpers;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;
using TaleWorlds.Core;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Models
{
    class BKVillageProductionModel : DefaultVillageProductionCalculatorModel
    {
        private static readonly float SERF_PRODUCTION = 0.0005f;
        private static readonly float SLAVE_PRODUCTION = 0.00125f;
        public override float CalculateDailyProductionAmount(Village village, ItemObject item)
        {
            if (village.Settlement != null && village.VillageState == Village.VillageStates.Normal && BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(village.Settlement))
            {
                ExplainedNumber explainedNumber = new ExplainedNumber(0f, false, null);
                PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(village.Settlement);
				VillageData villageData = data.VillageData;
                int serfs = data.GetTypeCount(PopType.Serfs);
                int slaves = data.GetTypeCount(PopType.Slaves);

				bool building = villageData.IsCurrentlyBuilding;
				bool production = !building && villageData.CurrentDefault.BuildingType == DefaultVillageBuildings.Instance.DailyProduction;
				explainedNumber.AddFactor(production ? 0.15f : -0.25f);

				List<(ItemObject, float)> productions = BannerKingsConfig.Instance.PopulationManager.GetProductions(villageData);
				foreach (System.ValueTuple<ItemObject, float> valueTuple in productions)
				{
					ItemObject item2 = valueTuple.Item1;
					float num = valueTuple.Item2;
					if (item2 == item)
					{
						float num2 = (float)(village.GetHearthLevel() + 1) * 0.5f;
						if (item.IsMountable && item.Tier == ItemObject.ItemTiers.Tier2 && PerkHelper.GetPerkValueForTown(DefaultPerks.Riding.Horde, village.TradeBound.Town) && MBRandom.RandomFloat < DefaultPerks.Riding.Horde.SecondaryBonus * 0.01f)
							num += 1f;
						
						explainedNumber.Add((float)serfs * SERF_PRODUCTION + (float)slaves * SLAVE_PRODUCTION + num, null, null);
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
                        {
							PerkHelper.AddPerkBonusForTown(DefaultPerks.Medicine.PerfectHealth, village.TradeBound.Town, ref explainedNumber);
							int animalHousing = villageData.GetBuildingLevel(DefaultVillageBuildings.Instance.AnimalHousing);
							if (animalHousing > 0)
								explainedNumber.AddFactor(0.05f * animalHousing);
						} else if (item.ItemCategory == DefaultItemCategories.Clay || item.ItemCategory == DefaultItemCategories.Iron || item.ItemCategory == DefaultItemCategories.Silver
							|| item.ItemCategory == DefaultItemCategories.Salt)
						{
							int mining = villageData.GetBuildingLevel(DefaultVillageBuildings.Instance.Mining);
							if (mining > 0)
								explainedNumber.AddFactor(0.05f * mining);
						} else if (item.ItemCategory == DefaultItemCategories.Wood)
                        {
							int sawmill = villageData.GetBuildingLevel(DefaultVillageBuildings.Instance.Sawmill);
							if (sawmill > 0)
								explainedNumber.AddFactor(0.05f * sawmill);
						}
						else if (item.ItemCategory == DefaultItemCategories.Fish)
						{
							int fishing = villageData.GetBuildingLevel(DefaultVillageBuildings.Instance.FishFarm);
							if (fishing > 0)
								explainedNumber.AddFactor(0.05f * fishing);
						}
						else if (item.ItemCategory == DefaultItemCategories.Grain || item.ItemCategory == DefaultItemCategories.DateFruit ||
							item.ItemCategory == DefaultItemCategories.Grape || item.ItemCategory == DefaultItemCategories.Olives ||
							item.ItemCategory == DefaultItemCategories.Flax)
						{
							int farming = villageData.GetBuildingLevel(DefaultVillageBuildings.Instance.Farming);
							if (farming > 0)
								explainedNumber.AddFactor(0.05f * farming);
						}


						Hero leader = village.Settlement.OwnerClan.Leader;
						CharacterObject characterObject = leader.CharacterObject;
						if (characterObject != null)
						{
							if (characterObject.Culture.HasFeat(DefaultCulturalFeats.KhuzaitAnimalProductionFeat) && (item.ItemCategory == DefaultItemCategories.Sheep || item.ItemCategory == DefaultItemCategories.Cow || item.ItemCategory == DefaultItemCategories.WarHorse || item.ItemCategory == DefaultItemCategories.Horse || item.ItemCategory == DefaultItemCategories.PackAnimal))
								explainedNumber.AddFactor(DefaultCulturalFeats.KhuzaitAnimalProductionFeat.EffectBonus, GameTexts.FindText("str_culture", null));
							
							if (village.Bound.IsCastle && characterObject.Culture.HasFeat(DefaultCulturalFeats.VlandianCastleVillageProductionFeat))
								explainedNumber.AddFactor(DefaultCulturalFeats.VlandianCastleVillageProductionFeat.EffectBonus, GameTexts.FindText("str_culture", null));
							
						}
					}
				}

				float eff = data.EconomicData.ProductionEfficiency.ResultNumber - 1f;
				explainedNumber.AddFactor(eff);

				return explainedNumber.ResultNumber;
            } else return base.CalculateDailyProductionAmount(village, item);
        }
    }
}
