using System;
using System.Linq;
using BannerKings.Managers.Innovations;
using BannerKings.Managers.Skills;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static BannerKings.Managers.Policies.BKWorkforcePolicy;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Models.Vanilla
{
    public class BKConstructionModel : DefaultBuildingConstructionModel
    {
        private const float SLAVE_CONSTRUCTION = 0.015f;
        private const float SERF_CONSTRUCTION = 0.010f;
        private readonly TextObject CultureText = GameTexts.FindText("str_culture");
        private readonly TextObject HighLoyaltyBonusText = new("{=fNiANUo4}High Loyalty");
        private readonly TextObject LowLoyaltyPenaltyText = new("{=fETzfZzS}Low Loyalty");

        private readonly TextObject ProductionFromMarketText = new("{=usk5UNRE}Construction from Market");
        private readonly TextObject VeryLowLoyaltyPenaltyText = new("{=2KOc0Wmu}Very Low Loyalty");

        public ExplainedNumber CalculateInfrastructureLimit(Settlement settlement)
        {
            var result = new ExplainedNumber(0f, true);
            result.LimitMin(0f);
            result.LimitMax(50f);
            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
            result.Add(data.TotalPop / 1200f, new TextObject("{=bLbvfBnb}Total population"));

            if (settlement.OwnerClan != null)
            {
                var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(settlement.Owner);
                if (education.Perks.Contains(BKPerks.Instance.CivilEngineer))
                {
                    result.Add(5f, BKPerks.Instance.CivilEngineer.Name);
                }
            }

            var innovations = BannerKingsConfig.Instance.InnovationsManager.GetInnovationData(settlement.Culture);
            if (innovations != null)
            {
                if (innovations.HasFinishedInnovation(DefaultInnovations.Instance.PublicWorks))
                {
                    result.Add(3f, DefaultInnovations.Instance.PublicWorks.Name);
                }
            }

            return result;
        }

        public ExplainedNumber CalculateVillageConstruction(Settlement settlement)
        {
            var result = new ExplainedNumber(0f, true);
            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
            var serfs = data.GetTypeCount(PopType.Serfs);
            result.Add(serfs * SERF_CONSTRUCTION, new TextObject("{=jH7cWD5r}Serfs"));

            var slaves = data.GetTypeCount(PopType.Slaves);
            result.Add(slaves * SLAVE_CONSTRUCTION, new TextObject("{=8xhVr4rK}Slaves"));

            return result;
        }

        public override ExplainedNumber CalculateDailyConstructionPower(Town town, bool includeDescriptions = false)
        {
            if (BannerKingsConfig.Instance.PopulationManager != null &&
                BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(town.Settlement))
            {
                var result = new ExplainedNumber(0f, includeDescriptions);
                CalculateDailyConstructionPowerInternal(town, ref result, includeDescriptions);
                return result;
            }

            return base.CalculateDailyConstructionPower(town, includeDescriptions);
        }

        public override int CalculateDailyConstructionPowerWithoutBoost(Town town)
        {
            if (BannerKingsConfig.Instance.PopulationManager != null &&
                BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(town.Settlement))
            {
                var result = new ExplainedNumber(0f);
                return CalculateDailyConstructionPowerInternal(town, ref result, true);
                ;
            }

            return base.CalculateDailyConstructionPowerWithoutBoost(town);
        }

        public override int GetBoostCost(Town town)
        {
            if (BannerKingsConfig.Instance.PopulationManager != null &&
                BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(town.Settlement))
            {
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement);
                var craftsmen = data.GetTypeCount(PopType.Craftsmen);
                return town.IsCastle ? (int) (craftsmen * 2f) : (int) (craftsmen / 2f);
            }

            return base.GetBoostCost(town);
        }

        public override int GetBoostAmount(Town town)
        {
            if (BannerKingsConfig.Instance.PopulationManager != null &&
                BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(town.Settlement))
            {
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement);
                var craftsmen = data.GetTypeCount(PopType.Craftsmen);
                var slaves = data.GetTypeCount(PopType.Slaves);

                if (slaves <= 0)
                {
                    slaves = 1;
                }

                var proportion = craftsmen / (float) slaves;
                var finalProportion = Math.Min(proportion, town.IsCastle ? 0.4f : 0.1f);
                var result = (int) (GetWorkforce(town.Settlement) * (finalProportion * 8f));
                return MBMath.ClampInt(result, 0, 100);
            }

            return base.GetBoostAmount(town);
        }

        private float GetWorkforce(Settlement settlement)
        {
            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
            var construction = BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(data.Settlement, "workforce",
                (int) WorkforcePolicy.Construction);
            var slaves = data.GetTypeCount(PopType.Slaves) * data.EconomicData.StateSlaves * (construction ? 1f : 0.5f);
            var serfs = data.GetTypeCount(PopType.Slaves) * (construction ? 0.15f : 0.1f);
            var slaveTotal = slaves > 0 ? slaves * SLAVE_CONSTRUCTION : 0f;
            var serfTotal = serfs * SERF_CONSTRUCTION;
            return slaveTotal + serfTotal;
        }

        private int CalculateDailyConstructionPowerInternal(Town town, ref ExplainedNumber result, bool omitBoost = false)
        {
            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement);
            result.Add(GetWorkforce(town.Settlement), new TextObject("{=8EX6VriS}Workforce"));
            result.Add(3f, new TextObject("{=AaNeOd9n}Base"));

            var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(town.OwnerClan.Leader);
            if (education.Perks.Contains(BKPerks.Instance.CivilEngineer))
            {
                result.AddFactor(0.2f, BKPerks.Instance.CivilEngineer.Name);
            }

            var innovations = BannerKingsConfig.Instance.InnovationsManager.GetInnovationData(town.Culture);
            if (innovations != null)
            {
                if (innovations.HasFinishedInnovation(DefaultInnovations.Instance.Cranes))
                {
                    result.AddFactor(0.12f, DefaultInnovations.Instance.Cranes.Name);
                }
            }

            if (town.IsCastle && town.Security >= 50)
            {
                if (town.GarrisonParty != null)
                {
                    var garrisonNumber = town.GarrisonParty.Party.NumberOfAllMembers;
                    if (garrisonNumber > 0)
                    {
                        var garrisonResult = garrisonNumber * (town.Security * 0.01f - 0.49f) * 0.1f;
                        result.Add(garrisonResult, new TextObject("Idle garrison"));
                    }
                }
            }


            if (!omitBoost && town.BoostBuildingProcess > 0)
            {
                var num = town.IsCastle ? 250 : 500;
                var num2 = GetBoostAmount(town);
                var num3 = Math.Min(1f, town.BoostBuildingProcess / (float) num);
                var num4 = 0f;
                if (town.IsTown && town.Governor != null && town.Governor.GetPerkValue(DefaultPerks.Engineering.Clockwork))
                {
                    num4 += DefaultPerks.Engineering.Clockwork.SecondaryBonus;
                }

                num2 += MathF.Round(num2 * num4);
                result.Add(num2 * num3, new TextObject("Craftsmen services"));
            }

            if (town.Governor != null)
            {
                var currentSettlement = town.Governor.CurrentSettlement;

                if (currentSettlement?.Town == town)
                {
                    SkillHelper.AddSkillBonusForTown(DefaultSkills.Engineering,
                        DefaultSkillEffects.TownProjectBuildingBonus, town, ref result);
                    PerkHelper.AddPerkBonusForTown(DefaultPerks.Steward.ForcedLabor, town, ref result);

                    if (!town.BuildingsInProgress.IsEmpty())
                    {
                        if (town.IsCastle && town.Governor.GetPerkValue(DefaultPerks.Engineering.MilitaryPlanner))
                        {
                            result.AddFactor(DefaultPerks.Engineering.MilitaryPlanner.SecondaryBonus,
                                DefaultPerks.Engineering.MilitaryPlanner.Name);
                        }

                        else if (town.IsTown && town.Governor.GetPerkValue(DefaultPerks.Engineering.Carpenters))
                        {
                            result.AddFactor(DefaultPerks.Engineering.Carpenters.SecondaryBonus,
                                DefaultPerks.Engineering.Carpenters.Name);
                        }

                        var building = town.BuildingsInProgress.Peek();
                        if ((building.BuildingType == DefaultBuildingTypes.Fortifications ||
                             building.BuildingType == DefaultBuildingTypes.CastleBarracks ||
                             building.BuildingType == DefaultBuildingTypes.CastleMilitiaBarracks ||
                             building.BuildingType == DefaultBuildingTypes.SettlementGarrisonBarracks ||
                             building.BuildingType == DefaultBuildingTypes.SettlementMilitiaBarracks ||
                             building.BuildingType == DefaultBuildingTypes.SettlementAquaducts) &&
                            town.Governor.GetPerkValue(DefaultPerks.Engineering.Stonecutters))
                        {
                            result.AddFactor(DefaultPerks.Engineering.Stonecutters.PrimaryBonus,
                                DefaultPerks.Engineering.Stonecutters.Name);
                        }
                    }
                }
            }

            var num5 = town.SoldItems.Sum(delegate(Town.SellLog x)
            {
                if (x.Category.Properties != ItemCategory.Property.BonusToProduction)
                {
                    return 0;
                }

                return x.Number;
            });
            if (num5 > 0)
            {
                result.Add(0.25f * num5, ProductionFromMarketText);
            }


            var buildingType = town.BuildingsInProgress.IsEmpty() ? null : town.BuildingsInProgress.Peek().BuildingType;
            if (DefaultBuildingTypes.MilitaryBuildings.Contains(buildingType))
            {
                PerkHelper.AddPerkBonusForTown(DefaultPerks.TwoHanded.Confidence, town, ref result);
            }

            if (buildingType == DefaultBuildingTypes.SettlementMarketplace ||
                buildingType == DefaultBuildingTypes.SettlementAquaducts ||
                buildingType == DefaultBuildingTypes.SettlementLimeKilns)
            {
                PerkHelper.AddPerkBonusForTown(DefaultPerks.Trade.SelfMadeMan, town, ref result);
            }

            var effectOfBuildings = town.GetEffectOfBuildings(BuildingEffectEnum.Construction);
            if (effectOfBuildings > 0f)
            {
                result.Add(effectOfBuildings, GameTexts.FindText("str_building_bonus"));
            }

            if (town.OwnerClan.Leader.Culture.HasFeat(DefaultCulturalFeats.BattanianConstructionFeat))
            {
                result.AddFactor(DefaultCulturalFeats.BattanianConstructionFeat.EffectBonus, CultureText);
            }

            switch (town.Loyalty)
            {
                case >= 75f:
                {
                    var num6 = MBMath.Map(town.Loyalty, 75f, 100f, 0f, 20f);
                    var value2 = result.ResultNumber * (num6 / 100f);
                    result.Add(value2, HighLoyaltyBonusText);
                    break;
                }
                case > 25f and <= 50f:
                {
                    var num7 = MBMath.Map(town.Loyalty, 25f, 50f, 50f, 0f);
                    var num8 = result.ResultNumber * (num7 / 100f);
                    result.Add(-num8, LowLoyaltyPenaltyText);
                    break;
                }
                case <= 25f:
                    result.Add(-result.ResultNumber, VeryLowLoyaltyPenaltyText);
                    break;
            }

            result.LimitMin(0f);
            return (int) result.ResultNumber;
        }
    }
}