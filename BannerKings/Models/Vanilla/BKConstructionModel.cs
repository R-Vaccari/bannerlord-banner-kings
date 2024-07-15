using System;
using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers.Buildings;
using BannerKings.Managers.Innovations;
using BannerKings.Managers.Items;
using BannerKings.Managers.Populations;
using BannerKings.Managers.Skills;
using BannerKings.Managers.Titles.Laws;
using BannerKings.Settings;
using BannerKings.Utils.Models;
using BannerKings.Utils;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Launcher.Library;
using static BannerKings.Managers.Policies.BKWorkforcePolicy;
using static BannerKings.Managers.PopulationManager;
using static BannerKings.Utils.PerksHelpers;

namespace BannerKings.Models.Vanilla
{
    public class BKConstructionModel : DefaultBuildingConstructionModel
    {
        private const float SLAVE_CONSTRUCTION = 0.02f;
        private const float SERF_CONSTRUCTION = 0.015f;
        private const float CRAFTSMEN_CONSTRUCTION = 0.03f;
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

        public List<ValueTuple<ItemObject, int>> GetMaterialRequirements(Building project)
        {
            List<ValueTuple<ItemObject, int>> list = new List<(ItemObject, int)>();
            if (project.BuildingType.IsDefaultProject)
            {
                return list;
            }

            int totalItems = (int)(project.GetConstructionCost() / 20f);
            float woodProportion = 0f;
            float clayProportion = 0f;
            float ironProportion = 0f;
            float toolsProportion = 0f;
            float limeProportion = 0f;
            float marbleProportion = 0f;

            BuildingType type = project.BuildingType;
            int level = project.CurrentLevel;
            if (type == DefaultBuildingTypes.Wall || type == DefaultBuildingTypes.Fortifications)
            {
                toolsProportion = 0.1f;
                if (level != 0)
                {
                    woodProportion = 0.1f;
                    limeProportion = 0.8f;
                }
            }
            else if (type == DefaultBuildingTypes.CastleBarracks || type == DefaultBuildingTypes.CastleMilitiaBarracks ||
                type == DefaultBuildingTypes.SettlementGarrisonBarracks || type == DefaultBuildingTypes.SettlementMilitiaBarracks ||
                type == DefaultBuildingTypes.CastleFairgrounds || type == DefaultBuildingTypes.SettlementFairgrounds ||
                type == DefaultBuildingTypes.SettlementMarketplace)
            {
                if (level == 0)
                {
                    woodProportion = 0.9f;
                }
                else if (level == 1)
                {
                    woodProportion = 0.7f;
                    clayProportion = 0.15f;
                    toolsProportion = 0.05f;
                }
                else
                {
                    woodProportion = 0.7f;
                    ironProportion = 0.1f;
                    limeProportion = 0.05f;
                    toolsProportion = 0.1f;
                }
            }
            else if (type == DefaultBuildingTypes.SettlementForum || type == DefaultBuildingTypes.SettlementAquaducts ||
                type == BKBuildings.Instance.Theater)
            {
                if (level == 0)
                {
                    limeProportion = 0.4f;
                    woodProportion = 0.2f;
                    clayProportion = 0.2f;
                    toolsProportion = 0.1f;
                }
                else if (level == 1)
                {
                    limeProportion = 0.5f;
                    woodProportion = 0.2f;
                    ironProportion = 0.1f;
                    toolsProportion = 0.15f;
                }
                else
                {
                    marbleProportion = 0.5f;
                    ironProportion = 0.2f;
                    toolsProportion = 0.2f;
                }
            }
            else if (type == BKBuildings.Instance.Mines || type == BKBuildings.Instance.CastleMines)
            {
                if (level == 0)
                {
                    woodProportion = 0.4f;
                    toolsProportion = 0.05f;
                }
                else if (level == 1)
                {
                    woodProportion = 0.5f;
                    toolsProportion = 0.1f;
                }
                else
                {
                    woodProportion = 0.3f;
                    ironProportion = 0.2f;
                    toolsProportion = 0.15f;
                }
            }
            else
            {
                if (level == 0)
                {
                    woodProportion = 0.7f;
                    clayProportion = 0.2f;
                }
                else if (level == 1)
                {
                    woodProportion = 0.5f;
                    clayProportion = 0.2f;
                    ironProportion = 0.1f;
                    toolsProportion = 0.05f;
                }
                else
                {
                    woodProportion = 0.4f;
                    limeProportion = 0.1f;
                    ironProportion = 0.15f;
                    toolsProportion = 0.1f;
                }
            }

            if (woodProportion > 0f)
            {
                list.Add(new(DefaultItems.HardWood, (int)(totalItems * woodProportion)));
            }

            if (clayProportion > 0f)
            {
                list.Add(new(TaleWorlds.CampaignSystem.Campaign.Current.ObjectManager.GetObject<ItemObject>("clay"), (int)(totalItems * clayProportion)));
            }

            if (ironProportion > 0f)
            {
                list.Add(new(DefaultItems.IronOre, (int)(totalItems * ironProportion)));
            }

            if (toolsProportion > 0f)
            {
                list.Add(new(DefaultItems.Tools, (int)(totalItems * toolsProportion)));
            }

            if (limeProportion > 0f)
            {
                list.Add(new(BKItems.Instance.Limestone, (int)(totalItems * limeProportion)));
            }

            if (marbleProportion > 0f)
            {
                list.Add(new(BKItems.Instance.Marble, (int)(totalItems * marbleProportion)));
            }

            return list;
        }

        public ExplainedNumber CalculateVillageConstruction(Settlement settlement)
        {
            var result = new ExplainedNumber(0f, true);
            result.Add(GetWorkforce(settlement), new TextObject("{=8EX6VriS}Workforce"));
            return result;
        }

        public ExplainedNumber CalculateLandExpansion(PopulationData data, float workforce, bool explanations = false)
        {
            var result = new ExplainedNumber(0f, explanations);
            result.LimitMin(0f);
            result.LimitMax(2f);
            var labor = workforce * 0.010f;
            result.Add(labor / data.LandData.DifficultyFinal, new TextObject("{=8EX6VriS}Workforce"));

            return result;
        }

        public override ExplainedNumber CalculateDailyConstructionPower(Town town, bool includeDescriptions = false)
        {
            if (BannerKingsConfig.Instance.PopulationManager != null)
            {
                var result = new ExplainedNumber(0f, includeDescriptions);
                result.LimitMin(0f);
                result.LimitMax(100f);
                CalculateDailyConstructionPowerInternal(town, ref result, includeDescriptions);
                return result;
            }

            return base.CalculateDailyConstructionPower(town, includeDescriptions);
        }

        public override int CalculateDailyConstructionPowerWithoutBoost(Town town)
        {
            if (BannerKingsConfig.Instance.PopulationManager != null)
            {
                var result = new ExplainedNumber(0f);
                return CalculateDailyConstructionPowerInternal(town, ref result, true);
            }

            return base.CalculateDailyConstructionPowerWithoutBoost(town);
        }

        public override int GetBoostCost(Town town)
        {
            if (BannerKingsConfig.Instance.PopulationManager != null)
            {
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement);
                if (data != null)
                {
                    var craftsmen = data.GetTypeCount(PopType.Craftsmen);
                    return town.IsCastle ? (int)(craftsmen * 2f) : (int)(craftsmen / 2f);
                }
            }

            return base.GetBoostCost(town);
        }

        public override int GetBoostAmount(Town town)
        {
            var result = 0;
            if (BannerKingsConfig.Instance.PopulationManager != null)
            {
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement);
                if (data != null)
                {
                    var craftsmen = data.GetTypeCount(PopType.Craftsmen);
                    result = (int)(craftsmen * 0.3f * CRAFTSMEN_CONSTRUCTION);
                    result = MBMath.ClampInt(result, 0, 100);
                }

                #region DefaultPerks.Steward.Relocation
                if (BannerKingsSettings.Instance.EnableUsefulPerks && BannerKingsSettings.Instance.EnableUsefulStewardPerks)
                {
                    var explainedNumber = new ExplainedNumber(0f, false);
                    result += (int)(DefaultPerks.Steward.Relocation.AddScaledGovernerPerkBonusForTownWithTownHeros(ref explainedNumber, true,  town) * 100);
                }
                else
                {
                    if (town.Governor != null && town.Governor.GetPerkValue(DefaultPerks.Steward.Relocation))
                    {
                        result += (int)DefaultPerks.Steward.Relocation.SecondaryBonus;
                    }
                }
                #endregion

                if (town.Governor != null && town.Governor.GetPerkValue(DefaultPerks.Trade.SpringOfGold))
                {
                    result += (int)DefaultPerks.Trade.SpringOfGold.SecondaryBonus;
                }
            }
            else
            {
                result = base.GetBoostAmount(town);
            }
            return result;
        }

        private float GetWorkforce(Settlement settlement)
        {
            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
            float result = 0f;
            if (settlement.Town != null)
            {
                bool construction = BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(data.Settlement, "workforce",
                               (int)WorkforcePolicy.Construction);
                if (!construction)
                {
                    return 0f;
                }
            }
            else if (settlement.IsVillage)
            {
                result += data.GetTypeCount(PopType.Craftsmen) * CRAFTSMEN_CONSTRUCTION / 2f;
            }

            float slaves = data.LandData.SlavesConstructionForce * SLAVE_CONSTRUCTION;
            if (data.TitleData != null && data.TitleData.Title != null && data.TitleData.Title.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.SlavesHardLabor))
            {
                slaves *= 1.2f;
            }

            result += data.LandData.SerfsConstructionForce * SERF_CONSTRUCTION;
            result += data.LandData.TenantsConstructionForce * SERF_CONSTRUCTION;

            return result + slaves;
        }

        public int GetMaterialSupply(ItemObject material, Town town)
        {
            int result = 0;

            foreach (ItemRosterElement element in town.Settlement.Stash)
            {
                if (element.EquipmentElement.Item == material)
                {
                    result += element.Amount;
                }
            }

            foreach (ItemRosterElement element in town.Settlement.ItemRoster)
            {
                if (element.EquipmentElement.Item == material)
                {
                    result += element.Amount;
                }
            }

            return result;
        }

        private int CalculateDailyConstructionPowerInternal(Town town, ref ExplainedNumber result, bool omitBoost = false)
        {
            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement);
            result.Add(GetWorkforce(town.Settlement), new TextObject("{=8EX6VriS}Workforce"));

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
                var num3 = Math.Min(1f, town.BoostBuildingProcess / (float)num);
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

                    if (BannerKingsSettings.Instance.EnableUsefulPerks && BannerKingsSettings.Instance.EnableUsefulStewardPerks)
                    {
                        if (town?.Settlement?.Party?.PrisonRoster != null && town.Settlement.Party.PrisonRoster.TotalManCount > 4)
                        {
                            var factor = (float)(town.Settlement.Party.PrisonRoster.TotalHealthyCount / 5);
                            DefaultPerks.Steward.GivingHands.AddScaledGovernerPerkBonusForTownWithTownHeros(ref result, true, town, factor);
                        }
                    }
                    else
                    {
                        if (town.Governor.GetPerkValue(DefaultPerks.Steward.ForcedLabor) && town.Settlement?.Party?.PrisonRoster?.TotalManCount > 0)
                        {
                            float value2 = MathF.Min(0.3f, (float)(town.Settlement.Party.PrisonRoster.TotalManCount / 3) * DefaultPerks.Steward.ForcedLabor.SecondaryBonus);
                            result.AddFactor(value2, DefaultPerks.Steward.ForcedLabor.Name);
                        }
                    }

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

            var num5 = town.SoldItems.Sum(delegate (Town.SellLog x)
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

            float loyaltyFactor = 0f;
            TextObject loyaltyText = VeryLowLoyaltyPenaltyText;
            switch (town.Loyalty)
            {
                case >= 75f:
                    {
                        loyaltyFactor = MBMath.Map(town.Loyalty, 75f, 100f, 0f, 0.12f);
                        loyaltyText = HighLoyaltyBonusText;
                        break;
                    }
                case > 15f and <= 50f:
                    {
                        loyaltyFactor = MBMath.Map(town.Loyalty, 25f, 50f, -1f, 0f);
                        loyaltyText = LowLoyaltyPenaltyText;
                        break;
                    }
                case <= 15f:
                    loyaltyFactor = -1f;
                    break;
            }

            result.AddFactor(loyaltyFactor, loyaltyText);
            Building project = town.CurrentBuilding;
            if (project != null)
            {
                foreach (var requirement in GetMaterialRequirements(project))
                {
                    if (GetMaterialSupply(requirement.Item1, town) < requirement.Item2)
                    {
                        result.Add(-result.ResultNumber, new TextObject("{=TAfQZOSB}Missing {MATERIAL} for project {PROJECT}")
                            .SetTextVariable("MATERIAL", requirement.Item1.Name)
                            .SetTextVariable("PROJECT", project.Name));
                        break;
                    }
                }
            }

            return (int)result.ResultNumber;
        }
    }
}