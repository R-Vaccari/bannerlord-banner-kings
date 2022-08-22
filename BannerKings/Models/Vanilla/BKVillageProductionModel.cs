using System;
using BannerKings.Managers;
using BannerKings.Managers.Populations;
using BannerKings.Managers.Populations.Villages;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace BannerKings.Models.Vanilla
{
    internal class BKVillageProductionModel : DefaultVillageProductionCalculatorModel
    {
        private static readonly float PRODUCTION = 0.00072f;
        private static readonly float BOOSTED_PRODUCTION = 0.0015f;

        public override float CalculateDailyProductionAmount(Village village, ItemObject item)
        {
            if (village.Settlement != null && village.VillageState == Village.VillageStates.Normal &&
                BannerKingsConfig.Instance.PopulationManager != null &&
                BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(village.Settlement))
            {
                var explainedNumber = new ExplainedNumber(0f);
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(village.Settlement);
                var villageData = data.VillageData;
                var serfs = data.GetTypeCount(PopulationManager.PopType.Serfs) * 0.85f;
                float slaves = data.GetTypeCount(PopulationManager.PopType.Slaves);

                var productions = BannerKingsConfig.Instance.PopulationManager.GetProductions(villageData);
                var totalWeight = 0f;
                foreach (var valueTuple in productions)
                {
                    totalWeight += valueTuple.Item2;
                }


                foreach (var valueTuple in productions)
                {
                    var output = valueTuple.Item1;
                    if (output == item)
                    {
                        var weight = valueTuple.Item2 / totalWeight;
                        explainedNumber.Add(GetWorkforceOutput(serfs * weight, slaves * weight, item, data.LandData)
                            .ResultNumber);

                        if (item.IsMountable && item.Tier == ItemObject.ItemTiers.Tier2 &&
                            PerkHelper.GetPerkValueForTown(DefaultPerks.Riding.Horde, village.Bound.Town) &&
                            MBRandom.RandomFloat < DefaultPerks.Riding.Horde.SecondaryBonus * 0.01f)
                        {
                            explainedNumber.Add(1f);
                        }

                        if (item.ItemCategory == DefaultItemCategories.Grain ||
                            item.ItemCategory == DefaultItemCategories.Olives ||
                            item.ItemCategory == DefaultItemCategories.Fish ||
                            item.ItemCategory == DefaultItemCategories.DateFruit)
                        {
                            PerkHelper.AddPerkBonusForTown(DefaultPerks.Trade.GranaryAccountant, village.Bound.Town,
                                ref explainedNumber);
                        }

                        else if (item.ItemCategory == DefaultItemCategories.Clay || item.ItemCategory ==
                                 DefaultItemCategories.Iron
                                 || item.ItemCategory ==
                                 DefaultItemCategories.Cotton ||
                                 item.ItemCategory ==
                                 DefaultItemCategories.Silver)
                        {
                            PerkHelper.AddPerkBonusForTown(DefaultPerks.Trade.TradeyardForeman, village.Bound.Town,
                                ref explainedNumber);
                        }

                        if (item.IsTradeGood)
                        {
                            PerkHelper.AddPerkBonusForTown(DefaultPerks.Athletics.Steady, village.Bound.Town,
                                ref explainedNumber);
                        }

                        if (PerkHelper.GetPerkValueForTown(DefaultPerks.Riding.Breeder, village.Bound.Town))
                        {
                            PerkHelper.AddPerkBonusForTown(DefaultPerks.Riding.Breeder, village.Bound.Town,
                                ref explainedNumber);
                        }

                        if (item.IsAnimal)
                        {
                            PerkHelper.AddPerkBonusForTown(DefaultPerks.Medicine.PerfectHealth, village.Bound.Town,
                                ref explainedNumber);
                        }

                        BannerKingsConfig.Instance.PopulationManager.ApplyProductionBuildingEffect(ref explainedNumber,
                            output, villageData);

                        var characterObject = village.Settlement.OwnerClan.Leader.CharacterObject;
                        if (characterObject != null)
                        {
                            if (characterObject.Culture.HasFeat(DefaultCulturalFeats.KhuzaitAnimalProductionFeat) &&
                                (item.ItemCategory == DefaultItemCategories.Sheep ||
                                 item.ItemCategory == DefaultItemCategories.Cow ||
                                 item.ItemCategory == DefaultItemCategories.WarHorse ||
                                 item.ItemCategory == DefaultItemCategories.Horse ||
                                 item.ItemCategory == DefaultItemCategories.PackAnimal))
                            {
                                explainedNumber.AddFactor(DefaultCulturalFeats.KhuzaitAnimalProductionFeat.EffectBonus,
                                    GameTexts.FindText("str_culture"));
                            }

                            if (village.Bound.IsCastle &&
                                characterObject.Culture.HasFeat(DefaultCulturalFeats.VlandianCastleVillageProductionFeat))
                            {
                                explainedNumber.AddFactor(
                                    DefaultCulturalFeats.VlandianCastleVillageProductionFeat.EffectBonus,
                                    GameTexts.FindText("str_culture"));
                            }
                        }

                        var production = !villageData.IsCurrentlyBuilding && villageData.CurrentDefault.BuildingType ==
                            DefaultVillageBuildings.Instance.DailyProduction;
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
            var result = new ExplainedNumber();
            result.LimitMin(0f);
            result.LimitMax(200f);
            if (serfs is < 0f or Single.NaN)
            {
                serfs = 1f;
            }

            if (slaves is < 0f or Single.NaN)
            {
                slaves = 1f;
            }

            if (item.StringId is "hardwood" or "fur")
            {
                var acres = data.Woodland;
                var maxWorkforce = acres / data.GetRequiredLabor("wood");
                var workforce = Math.Min(maxWorkforce, serfs + slaves);
                result.Add(workforce * data.GetAcreOutput("wood") * 15f);
                var serfFactor = serfs / workforce;
                if (serfFactor > 0f)
                {
                    result.AddFactor(serfFactor * 0.5f);
                }
            }
            else if ((item.IsAnimal || item.IsMountable) && item.HorseComponent != null)
            {
                var acres = data.Pastureland;
                var maxWorkforce = acres / data.GetRequiredLabor("pasture");
                var workforce = Math.Min(maxWorkforce, serfs + slaves);
                result.Add(workforce * data.GetAcreOutput("pasture") * item.HorseComponent.MeatCount);
                var serfFactor = serfs / workforce;
                if (serfFactor > 0f)
                {
                    result.AddFactor(serfFactor * 0.5f);
                }
            }
            else if (item.IsFood && item.StringId != "fish")
            {
                var acres = data.Farmland;
                var maxWorkforce = acres / data.GetRequiredLabor("farmland");
                var workforce = Math.Min(maxWorkforce, serfs + slaves);
                result.Add(workforce * data.GetAcreOutput("farmland"));
                var serfFactor = serfs / workforce;
                if (serfFactor > 0f)
                {
                    result.AddFactor(serfFactor * 1f);
                }
            }
            else
            {
                result.Add(serfs * (item.IsFood ? BOOSTED_PRODUCTION : PRODUCTION));
                result.Add(slaves *
                           (item.StringId is "clay" or "iron" or "salt" or "silver"
                               ? BOOSTED_PRODUCTION
                               : PRODUCTION));
            }

            return result;
        }
    }
}