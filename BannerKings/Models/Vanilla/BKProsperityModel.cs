using System.Linq;
using BannerKings.Extensions;
using BannerKings.Managers.Court;
using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Policies;
using BannerKings.Managers.Populations;
using BannerKings.Managers.Populations.Villages;
using BannerKings.Managers.Skills;
using BannerKings.Managers.Titles.Laws;
using CalradiaExpandedKingdoms.Models;
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
using static BannerKings.Managers.Policies.BKTaxPolicy;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Models.Vanilla
{
    public class BKProsperityModel : CEKSettlementProsperityModel
    {
        private static readonly float STABILITY_FACTOR = 5f;
        private static readonly TextObject FoodShortageText = new("{=7Ttux0dr}Food Shortage");
        private static readonly TextObject ProsperityFromMarketText = new("{=3kMgpxc0}Goods From Market");
        private static readonly TextObject Governor = new("{=DyZdcwa4}Governor");
        private static readonly TextObject HousingCostsText = new("{=zYjK6Kzb}Housing Costs");

        public override ExplainedNumber CalculateHearthChange(Village village, bool includeDescriptions = false)
        {
            var baseResult = base.CalculateHearthChange(village, includeDescriptions);
            //if (BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(village.Settlement))
            // new BKGrowthModel().CalculateHearthGrowth(village, ref baseResult);

            var owner = village.GetActualOwner();
            var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(owner);
            if (education.HasPerk(BKPerks.Instance.CivilCultivator))
            {
                baseResult.Add(1f, BKPerks.Instance.CivilCultivator.Name);
            }

            if (education.HasPerk(BKPerks.Instance.RitterPettySuzerain))
            {
                baseResult.Add(0.1f, BKPerks.Instance.RitterPettySuzerain.Name);
            }

            if (owner.Culture.StringId == "battania")
            {
                BannerKingsConfig.Instance.CourtManager.ApplyCouncilEffect(ref baseResult, owner,
                          CouncilPosition.Elder, 0.2f, false);
            }

            var rel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(owner);
            if ((village.VillageType == DefaultVillageTypes.DateFarm || village.VillageType == DefaultVillageTypes.DesertHorseRanch) 
                && BannerKingsConfig.Instance.ReligionsManager.HasBlessing(owner, DefaultDivinities.Instance.AseraSecondary3, rel)) 
            {
                baseResult.Add(0.1f, DefaultDivinities.Instance.AseraSecondary3.Name);
            }

            if (BannerKingsConfig.Instance.ReligionsManager.HasBlessing(owner, DefaultDivinities.Instance.AmraMain, rel) 
                && (village.VillageType == DefaultVillageTypes.HogFarm || village.VillageType == DefaultVillageTypes.CattleRange ||
                village.VillageType == DefaultVillageTypes.Lumberjack)) 
            {
                baseResult.Add(0.2f, DefaultDivinities.Instance.AmraMain.Name);
            }

            if (BannerKingsConfig.Instance.ReligionsManager.HasBlessing(owner, DefaultDivinities.Instance.TreeloreMoon, rel))
            {
                if (rel.FavoredCultures.Contains(village.Settlement.Culture))
                {
                    baseResult.Add(0.08f, DefaultDivinities.Instance.TreeloreMoon.Name);
                }
            }

            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(village.Settlement);
            if (data != null)
            {
                var villageData = data.VillageData;
                var marketplace = villageData.GetBuildingLevel(DefaultVillageBuildings.Instance.Marketplace);
                if (marketplace > 0)
                {
                    baseResult.Add(0.075f * marketplace, DefaultVillageBuildings.Instance.Marketplace.Name);
                }
            }

            var tax = (BKTaxPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(village.Settlement, "tax");
            if (tax.Policy != TaxType.Standard)
            {
                if (tax.Policy == TaxType.High)
                {
                    baseResult.AddFactor(-0.15f, new TextObject("{=EhHXS8PN}High tax policy"));
                }
                else if (tax.Policy == TaxType.Low)
                {
                    baseResult.AddFactor(0.1f, new TextObject("{=j6AoAS6n}Low tax policy"));
                }
                else
                {
                    baseResult.AddFactor(0.2f, new TextObject("{=HMao8su6}Tax exemption policy"));
                }
            }

            AddDemesneLawEffect(data, ref baseResult);
            return baseResult;
        }

        public override ExplainedNumber CalculateProsperityChange(Town fortification, bool includeDescriptions = false)
        {
            var baseResult = base.CalculateProsperityChange(fortification, includeDescriptions);
            if (BannerKingsConfig.Instance.PopulationManager != null &&
                BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(fortification.Settlement))
            {
                var explainedNumber = new ExplainedNumber(0f, true);
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(fortification.Settlement);
                var craftsmen = data.GetTypeCount(PopType.Craftsmen);
                explainedNumber.Add(craftsmen * 0.0005f, new TextObject("Craftsmen output"));
                var slaves = data.GetTypeCount(PopType.Slaves);
                explainedNumber.Add(slaves * -0.0001f, new TextObject("{=FJSfBwzp}Slave population"));

                if (BannerKingsConfig.Instance.PopulationManager.PopSurplusExists(fortification.Settlement, PopType.Slaves,
                        true))
                {
                    explainedNumber.Add(slaves * -0.0003f, new TextObject("{=y9jGiPQw}Slave surplus"));
                }

                var factor = data.Stability - 1f + data.Stability;
                var stabilityImpact = STABILITY_FACTOR * factor;
                explainedNumber.Add(stabilityImpact, new TextObject("Stability impact"));

                var foodLimitForBonus = (int) (fortification.FoodStocksUpperLimit() * 0.8f);
                if (fortification.FoodStocks >= foodLimitForBonus)
                {
                    explainedNumber.Add(0.5f, new TextObject("{=9Jyv5XNX}Well fed populace"));
                }
                else if (fortification.Settlement.IsStarving)
                {
                    var starvation = stabilityImpact;
                    if (starvation > 0f)
                    {
                        starvation *= -0.5f;
                    }

                    if (stabilityImpact is <= 0f and > -1f)
                    {
                        starvation = -1f;
                    }

                    explainedNumber.Add(starvation, FoodShortageText);
                }

                var houseCost = fortification.Prosperity < 1500f
                    ? 6f - (fortification.Prosperity / 250f - 1f)
                    : fortification.Prosperity >= 6000f
                        ? -1f + fortification.Prosperity / 3000f * -1f
                        : 0f;
                explainedNumber.Add(houseCost, HousingCostsText);

                if (fortification.IsTown)
                {
                    var num3 = fortification.SoldItems.Sum(delegate(Town.SellLog x)
                    {
                        if (x.Category.Properties != ItemCategory.Property.BonusToProsperity)
                        {
                            return 0;
                        }

                        return x.Number;
                    });
                    if (num3 > 0)
                    {
                        explainedNumber.Add(num3 * 0.1f, ProsperityFromMarketText);
                    }

                    float merchantGold = fortification.Gold;
                    var merchantEffect = merchantGold < 20000f ? merchantGold / 10000f - 2f :
                        merchantGold >= 200000f ? MathF.Min(200000f * 0.000005f - 1f, 2f) : 0f;
                    explainedNumber.Add(merchantEffect, new TextObject("{=Crsf0YLd}Merchants wealth"));
                } 
                else if (fortification.IsCastle)
                {
                    var owner = fortification.OwnerClan.Leader;
                    if (owner.Culture.StringId == "vlandia")
                    {
                        BannerKingsConfig.Instance.CourtManager.ApplyCouncilEffect(ref explainedNumber, fortification.OwnerClan.Leader,
                            CouncilPosition.Castellan, 1.5f, false);
                    }
                }

                if (fortification.Governor != null)
                {
                    float skill = fortification.Governor.GetSkillValue(DefaultSkills.Steward);
                    explainedNumber.Add(MathF.Min(skill * 0.001f, 1.5f), Governor);
                }

                PerkHelper.AddPerkBonusForTown(DefaultPerks.Medicine.PristineStreets, fortification, ref explainedNumber);
                PerkHelper.AddPerkBonusForTown(DefaultPerks.Riding.Veterinary, fortification, ref explainedNumber);
                if (PerkHelper.GetPerkValueForTown(DefaultPerks.Engineering.Apprenticeship, fortification))
                {
                    var num4 = 0f;
                    foreach (var building in from x in fortification.Buildings
                             where !x.BuildingType.IsDefaultProject && x.CurrentLevel > 0
                             select x)
                    {
                        num4 += DefaultPerks.Engineering.Apprenticeship.SecondaryBonus;
                    }

                    if (num4 > 0f && explainedNumber.ResultNumber > 0f)
                    {
                        explainedNumber.AddFactor(num4, DefaultPerks.Engineering.Apprenticeship.Name);
                    }
                }

                if (fortification.BuildingsInProgress.IsEmpty())
                {
                    BuildingHelper.AddDefaultDailyBonus(fortification, BuildingEffectEnum.ProsperityDaily,
                        ref explainedNumber);
                }

                foreach (var building2 in fortification.Buildings)
                {
                    var buildingEffectAmount = building2.GetBuildingEffectAmount(BuildingEffectEnum.Prosperity);
                    if (!building2.BuildingType.IsDefaultProject && buildingEffectAmount > 0f)
                    {
                        explainedNumber.Add(buildingEffectAmount, building2.Name);
                    }

                    if (building2.BuildingType == DefaultBuildingTypes.SettlementAquaducts ||
                        building2.BuildingType == DefaultBuildingTypes.CastleGranary ||
                        building2.BuildingType == DefaultBuildingTypes.SettlementGranary)
                    {
                        PerkHelper.AddPerkBonusForTown(DefaultPerks.Medicine.CleanInfrastructure, fortification,
                            ref explainedNumber);
                    }
                }

                if (fortification.IsTown && !fortification.CurrentBuilding.IsCurrentlyDefault &&
                    fortification.Governor != null && fortification.Governor.GetPerkValue(DefaultPerks.Trade.TrickleDown))
                {
                    explainedNumber.Add(DefaultPerks.Trade.TrickleDown.SecondaryBonus, DefaultPerks.Trade.TrickleDown.Name);
                }

                if (fortification.Settlement.OwnerClan.Kingdom != null)
                {
                    if (fortification.Settlement.OwnerClan.Kingdom.ActivePolicies.Contains(DefaultPolicies.RoadTolls))
                    {
                        explainedNumber.Add(-0.2f, DefaultPolicies.RoadTolls.Name);
                    }

                    if (fortification.Settlement.OwnerClan.Kingdom.RulingClan == fortification.Settlement.OwnerClan &&
                        fortification.Settlement.OwnerClan.Kingdom.ActivePolicies.Contains(DefaultPolicies.ImperialTowns))
                    {
                        explainedNumber.Add(1f, DefaultPolicies.ImperialTowns.Name);
                    }

                    if (fortification.Settlement.OwnerClan.Kingdom.ActivePolicies.Contains(DefaultPolicies.CrownDuty))
                    {
                        explainedNumber.Add(-1f, DefaultPolicies.CrownDuty.Name);
                    }

                    if (fortification.Settlement.OwnerClan.Kingdom.ActivePolicies.Contains(DefaultPolicies.WarTax))
                    {
                        explainedNumber.Add(-1f, DefaultPolicies.WarTax.Name);
                    }
                }

                GetSettlementProsperityChangeDueToIssues(fortification.Settlement, ref explainedNumber);

                BannerKingsConfig.Instance.CourtManager.ApplyCouncilEffect(ref explainedNumber,
                    fortification.OwnerClan.Leader, CouncilPosition.Steward, 1f, false);

                AddDemesneLawEffect(data, ref explainedNumber);
                return explainedNumber;
            }

            return baseResult;
        }

        private void GetSettlementProsperityChangeDueToIssues(Settlement settlement, ref ExplainedNumber result)
        {
            Campaign.Current.Models.IssueModel.GetIssueEffectsOfSettlement(DefaultIssueEffects.SettlementProsperity,
                settlement, ref result);
        }

        private void AddDemesneLawEffect(PopulationData data, ref ExplainedNumber result)
        {
            if (data != null && data.TitleData != null && data.TitleData.Title != null)
            {
                var title = data.TitleData.Title;
                if (title.contract != null)
                {
                    if (title.contract.IsLawEnacted(DefaultDemesneLaws.Instance.SerfsLaxDuties))
                    {
                        float proportion = data.GetCurrentTypeFraction(PopType.Serfs);
                        result.AddFactor(proportion * 0.05f, DefaultDemesneLaws.Instance.SerfsLaxDuties.Name);
                    }

                    if (title.contract.IsLawEnacted(DefaultDemesneLaws.Instance.CraftsmenLaxDuties))
                    {
                        float proportion = data.GetCurrentTypeFraction(PopType.Craftsmen);
                        result.AddFactor(proportion * 0.08f, DefaultDemesneLaws.Instance.SerfsLaxDuties.Name);
                    }
                }
            }
        }
    }
}