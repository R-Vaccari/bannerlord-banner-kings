﻿using BannerKings.Populations;
using Helpers;
using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static BannerKings.Managers.Policies.BKWorkforcePolicy;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Models
{
    class BKConstructionModel : DefaultBuildingConstructionModel
    {
		public ExplainedNumber CalculateVillageConstruction(Settlement settlement)
        {
			ExplainedNumber result = new ExplainedNumber(0f, true);
			PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
			int serfs = data.GetTypeCount(PopType.Serfs);
			result.Add(serfs  * SERF_CONSTRUCTION, new TextObject("Serfs"));

			int slaves = data.GetTypeCount(PopType.Slaves);
			result.Add(slaves * SLAVE_CONSTRUCTION, new TextObject("Slaves"));

			return result;
		}

        public override ExplainedNumber CalculateDailyConstructionPower(Town town, bool includeDescriptions = false)
        {
	        if (BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(town.Settlement))
            {
				ExplainedNumber result = new ExplainedNumber(0f, includeDescriptions);
				CalculateDailyConstructionPowerInternal(town, ref result, includeDescriptions);
				return result;
			}

	        return base.CalculateDailyConstructionPower(town, includeDescriptions);
        }

        public override int CalculateDailyConstructionPowerWithoutBoost(Town town)
        {
	        if (BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(town.Settlement))
			{
				ExplainedNumber result = new ExplainedNumber(0f);
				return CalculateDailyConstructionPowerInternal(town, ref result, true); ;
			}

	        return base.CalculateDailyConstructionPowerWithoutBoost(town);
        }

        public override int GetBoostCost(Town town)
        {
	        if (BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(town.Settlement))
			{
				PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement);
				int craftsmen = data.GetTypeCount(PopType.Craftsmen);
				return town.IsCastle ? (int)(craftsmen * 2f) : (int)(craftsmen / 2f);
			}

	        return base.GetBoostCost(town);
        }

        public override int GetBoostAmount(Town town)
        {
	        if (BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(town.Settlement))
			{
				PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement);
				int craftsmen = data.GetTypeCount(PopType.Craftsmen);
				int slaves = data.GetTypeCount(PopType.Slaves);

				if (slaves <= 0)
					slaves = 1;

				float proportion = craftsmen / (float)slaves;
				float finalProportion = Math.Min(proportion, (town.IsCastle ? 0.4f : 0.1f));
				int result = (int)(GetWorkforce(town.Settlement) * (finalProportion * 8f));
				return MBMath.ClampInt(result, 0, 100);
			}

	        return base.GetBoostAmount(town);
        }

		private float GetWorkforce(Settlement settlement)
        {
			PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
			bool construction = BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(data.Settlement, "workforce", (int)WorkforcePolicy.Construction);
			float slaves = data.GetTypeCount(PopType.Slaves) * data.EconomicData.StateSlaves * (construction ? 1f : 0.5f);
			float serfs = data.GetTypeCount(PopType.Slaves) * (construction ? 0.15f : 0.1f);
			float slaveTotal = slaves > 0 ? slaves * SLAVE_CONSTRUCTION : 0f;
			float serfTotal = (serfs * SERF_CONSTRUCTION);
			return slaveTotal + serfTotal;
		}

		private int CalculateDailyConstructionPowerInternal(Town town, ref ExplainedNumber result, bool omitBoost = false)
		{
			PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement);
			result.Add(GetWorkforce(town.Settlement), new TextObject("{=!}Workforce"));
			result.Add(3f, new TextObject("Base"));

			if (town.IsCastle && town.Security >= 50)
				if (town.GarrisonParty != null)
                {
					int garrisonNumber = town.GarrisonParty.Party.NumberOfAllMembers;
					if (garrisonNumber > 0)
					{
						float garrisonResult = (garrisonNumber * ((town.Security * 0.01f) - 0.49f)) * 0.1f;
						result.Add(garrisonResult, new TextObject("Idle garrison"));
					}
				}	
                

			if (!omitBoost && town.BoostBuildingProcess > 0)
			{
				int num = town.IsCastle ? 250 : 500;
				int num2 = GetBoostAmount(town);
				float num3 = Math.Min(1f, town.BoostBuildingProcess / (float)num);
				float num4 = 0f;
				if (town.IsTown && town.Governor != null && town.Governor.GetPerkValue(DefaultPerks.Engineering.Clockwork))
					num4 += DefaultPerks.Engineering.Clockwork.SecondaryBonus;
				
				num2 += MathF.Round(num2 * num4);
				result.Add(num2 * num3, new TextObject("Craftsmen services"));
			}
			if (town.Governor != null)
			{
				Settlement currentSettlement = town.Governor.CurrentSettlement;
				if (((currentSettlement != null) ? currentSettlement.Town : null) == town)
				{
					SkillHelper.AddSkillBonusForTown(DefaultSkills.Engineering, DefaultSkillEffects.TownProjectBuildingBonus, town, ref result);
					PerkHelper.AddPerkBonusForTown(DefaultPerks.Steward.ForcedLabor, town, ref result);
				}
			}
			if (town.Governor != null)
			{
				Settlement currentSettlement2 = town.Governor.CurrentSettlement;
				if (((currentSettlement2 != null) ? currentSettlement2.Town : null) == town && !town.BuildingsInProgress.IsEmpty<Building>())
				{
					if (town.IsCastle && town.Governor.GetPerkValue(DefaultPerks.Engineering.MilitaryPlanner))
						result.AddFactor(DefaultPerks.Engineering.MilitaryPlanner.SecondaryBonus, DefaultPerks.Engineering.MilitaryPlanner.Name);
					
					else if (town.IsTown && town.Governor.GetPerkValue(DefaultPerks.Engineering.Carpenters))
						result.AddFactor(DefaultPerks.Engineering.Carpenters.SecondaryBonus, DefaultPerks.Engineering.Carpenters.Name);
					
					Building building = town.BuildingsInProgress.Peek();
					if ((building.BuildingType == DefaultBuildingTypes.Fortifications || 
						building.BuildingType == DefaultBuildingTypes.CastleBarracks || building.BuildingType == DefaultBuildingTypes.CastleMilitiaBarracks || 
						building.BuildingType == DefaultBuildingTypes.SettlementGarrisonBarracks || 
						building.BuildingType == DefaultBuildingTypes.SettlementMilitiaBarracks || 
						building.BuildingType == DefaultBuildingTypes.SettlementAquaducts) && town.Governor.GetPerkValue(DefaultPerks.Engineering.Stonecutters))
						result.AddFactor(DefaultPerks.Engineering.Stonecutters.PrimaryBonus, DefaultPerks.Engineering.Stonecutters.Name);
					
				}
			}

			int num5 = town.SoldItems.Sum(delegate (Town.SellLog x)
			{
				if (x.Category.Properties != ItemCategory.Property.BonusToProduction)
					return 0;
				
				return x.Number;
			});
			if (num5 > 0)
				result.Add(0.25f * num5, ProductionFromMarketText);
			

			BuildingType buildingType = town.BuildingsInProgress.IsEmpty<Building>() ? null : town.BuildingsInProgress.Peek().BuildingType;
			if (DefaultBuildingTypes.MilitaryBuildings.Contains<BuildingType>(buildingType))

				PerkHelper.AddPerkBonusForTown(DefaultPerks.TwoHanded.Confidence, town, ref result);
			
			if (buildingType == DefaultBuildingTypes.SettlementMarketplace || buildingType == DefaultBuildingTypes.SettlementAquaducts || buildingType == DefaultBuildingTypes.SettlementLimeKilns)
				PerkHelper.AddPerkBonusForTown(DefaultPerks.Trade.SelfMadeMan, town, ref result);
			
			float effectOfBuildings = town.GetEffectOfBuildings(BuildingEffectEnum.Construction);
			if (effectOfBuildings > 0f)
				result.Add(effectOfBuildings, GameTexts.FindText("str_building_bonus"));
			
			if (town.OwnerClan.Leader.Culture.HasFeat(DefaultCulturalFeats.BattanianConstructionFeat))
				result.AddFactor(DefaultCulturalFeats.BattanianConstructionFeat.EffectBonus, CultureText);
			
			if (town.Loyalty >= 75f)
			{
				float num6 = MBMath.Map(town.Loyalty, 75f, 100f, 0f, 20f);
				float value2 = result.ResultNumber * (num6 / 100f);
				result.Add(value2, HighLoyaltyBonusText);
			}
			else if (town.Loyalty > 25f && town.Loyalty <= 50f)
			{
				float num7 = MBMath.Map(town.Loyalty, 25f, 50f, 50f, 0f);
				float num8 = result.ResultNumber * (num7 / 100f);
				result.Add(-num8, LowLoyaltyPenaltyText);
			}
			else if (town.Loyalty <= 25f)
				result.Add(-result.ResultNumber, VeryLowLoyaltyPenaltyText);
			
			result.LimitMin(0f);
			return (int)result.ResultNumber;
		}

		private const float SLAVE_CONSTRUCTION = 0.015f;
		private const float SERF_CONSTRUCTION = 0.010f;

		private readonly TextObject ProductionFromMarketText = new TextObject("{=vaZDJGMx}Construction from Market");
		private readonly TextObject CultureText = GameTexts.FindText("str_culture");
		private readonly TextObject HighLoyaltyBonusText = new TextObject("{=aSniKUJv}High Loyalty");
		private readonly TextObject LowLoyaltyPenaltyText = new TextObject("{=SJ2qsRdF}Low Loyalty");
		private readonly TextObject VeryLowLoyaltyPenaltyText = new TextObject("{=CcQzFnpN}Very Low Loyalty");
	}
}
