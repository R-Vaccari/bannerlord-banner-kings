using System;
using BannerKings.Extensions;
using BannerKings.Managers;
using BannerKings.Managers.Populations;
using BannerKings.Managers.Populations.Villages;
using BannerKings.Managers.Skills;
using BannerKings.Managers.Titles.Laws;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace BannerKings.Models.Vanilla
{
    public class BKVillageProductionModel : DefaultVillageProductionCalculatorModel
    {
        private static readonly float PRODUCTION = 0.005f;
        private static readonly float BOOSTED_PRODUCTION = 0.008f;

        public ExplainedNumber CalculateProductionsExplained(Village village)
        {
            var explainedNumber = new ExplainedNumber(0f, true);
            var productions = BannerKingsConfig.Instance.PopulationManager
                .GetProductions(BannerKingsConfig.Instance.PopulationManager.GetPopData(village.Settlement));
            foreach (var production in productions)
            {
                explainedNumber.Add(CalculateDailyProductionAmount(village, production.Item1), production.Item1.Name);
            }

            return explainedNumber;
        }

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
                var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(village.Settlement.OwnerClan.Leader);

                var productions = BannerKingsConfig.Instance.PopulationManager.GetProductions(data);
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
                        explainedNumber.Add(GetWorkforceOutput(serfs * weight, slaves * weight, item, data)
                            .ResultNumber);

                        if (item.IsMountable && item.Tier == ItemObject.ItemTiers.Tier2 &&
                            PerkHelper.GetPerkValueForTown(DefaultPerks.Riding.Shepherd, village.Bound.Town) &&
                            MBRandom.RandomFloat < DefaultPerks.Riding.Shepherd.SecondaryBonus * 0.01f)
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

                        if (village.Bound.IsCastle && education.HasPerk(BKPerks.Instance.RitterIronHorses))
                        {
                            explainedNumber.AddFactor(0.1f, BKPerks.Instance.RitterIronHorses.Name);
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

                        if (villageData.CurrentBuilding.BuildingType.StringId == DefaultVillageBuildings.Instance.DailyProduction.StringId)
                        {
                            explainedNumber.AddFactor(0.15f, DefaultVillageBuildings.Instance.DailyProduction.Name);
                        }

                        explainedNumber.AddFactor(data.EconomicData.ProductionEfficiency.ResultNumber - 1f);
                    }
                }

                return explainedNumber.ResultNumber;
            }

            return base.CalculateDailyProductionAmount(village, item);
        }

        private ExplainedNumber GetWorkforceOutput(float serfs, float slaves, ItemObject item, PopulationData data)
        {
            var result = new ExplainedNumber();
            result.LimitMin(0f);
            result.LimitMax(200f);
            if (serfs is < 0f or float.NaN)
            {
                serfs = 1f;
            }

            if (slaves is < 0f or float.NaN)
            {
                slaves = 1f;
            }

            bool woodland = AddWoodlandProcution(ref result, serfs, slaves, item, data.LandData);
            bool animal = AddAnimalProcution(ref result, item, data.LandData);
            bool farm = AddFarmProcution(ref result, serfs, slaves, item, data);
            if (!woodland && !animal && !farm)
            {
                AddGeneralProcution(ref result, serfs, slaves, item, data);
            }

            return result;
        }

        private void AddGeneralProcution(ref ExplainedNumber result, float serfs, float slaves, ItemObject item, PopulationData data)
        {
            result.Add(serfs * PRODUCTION);

            if (item.IsMineral())
            {
                if (data.TitleData != null && data.TitleData.Title != null)
                {
                    var title = data.TitleData.Title;
                    if (title.contract.IsLawEnacted(DefaultDemesneLaws.Instance.SlavesHardLabor))
                    {
                        result.Add(slaves * BOOSTED_PRODUCTION);
                        return;
                    }
                }
            }

            result.Add(slaves * PRODUCTION);
        }

        private bool AddFarmProcution(ref ExplainedNumber result, float serfs, float slaves, ItemObject item, PopulationData data)
        {
            bool valid = item.IsFood && item.StringId != "fish";
            if (valid)
            {
                var acres = data.LandData.Farmland;
                var maxWorkforce = (int)(acres * data.LandData.GetRequiredLabor("farmland"));
                if (maxWorkforce < (serfs + slaves))
                {
                    // TODO
                }

                result.Add(serfs * data.LandData.GetAcreClassOutput("farmland", PopulationManager.PopType.Serfs));
                result.Add(slaves * data.LandData.GetAcreClassOutput("farmland", PopulationManager.PopType.Slaves));
            }

            return valid;
        }

        private bool AddAnimalProcution(ref ExplainedNumber result, ItemObject item, LandData data)
        {
            bool valid = item.IsAnimal || item.IsMountable;
            if (valid)
            {
                var acres = data.Pastureland;
                result.Add((acres * data.GetAcreOutput("pasture")) / Math.Max(item.HorseComponent.MeatCount, 1));
                if (item.IsMountable)
                {
                    result.AddFactor(item.Tierf * -0.12f);
                }
            }

            return valid;
        }

        private bool AddWoodlandProcution(ref ExplainedNumber result, float serfs, float slaves, ItemObject item, LandData data)
        {
            bool valid = item.StringId is "hardwood" or "fur";
            if (valid)
            {
                var acres = data.Woodland;
                var maxWorkforce = acres * data.GetRequiredLabor("wood");
                var workforce = Math.Min(maxWorkforce, serfs + slaves);
                result.Add(workforce * data.GetAcreOutput("wood") * (item.StringId is "hardwood" ? 30f : 15f));
                var serfFactor = serfs / slaves;
                if (serfFactor > 0f)
                {
                    result.AddFactor(serfFactor * 0.5f);
                }
            }

            return valid;
        }
    }
}