using Helpers;
using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static Populations.PopulationManager;

namespace Populations.Models
{
    class ConstructionModel : DefaultBuildingConstructionModel
    {

        public override ExplainedNumber CalculateDailyConstructionPower(Town town, bool includeDescriptions = false)
        {
			if (IsSettlementPopulated(town.Settlement))
            {
				ExplainedNumber result = new ExplainedNumber(0f, includeDescriptions, null);
				this.CalculateDailyConstructionPowerInternal(town, ref result, includeDescriptions);
				return result;
			}
			else return base.CalculateDailyConstructionPower(town, includeDescriptions);
        }

        public override int CalculateDailyConstructionPowerWithoutBoost(Town town)
        {
			if (IsSettlementPopulated(town.Settlement))
			{
				ExplainedNumber result = new ExplainedNumber(0f, false, null);
				return this.CalculateDailyConstructionPowerInternal(town, ref result, true); ;
			}
			else return base.CalculateDailyConstructionPowerWithoutBoost(town);
        }

        public override int GetBoostCost(Town town)
        {
			if (IsSettlementPopulated(town.Settlement))
			{
				PopulationData data = GetPopData(town.Settlement);
				int craftsmen = data.GetTypeCount(PopType.Craftsmen);
				return (int)((float)craftsmen / 2f);
			}
			else return base.GetBoostCost(town);
        }

        public override int GetBoostAmount(Town town)
        {
			if (IsSettlementPopulated(town.Settlement))
			{
				PopulationData data = GetPopData(town.Settlement);
				int craftsmen = data.GetTypeCount(PopType.Craftsmen);
				int slaves = data.GetTypeCount(PopType.Slaves);

				float proportion = (float)craftsmen / (float)slaves;
				float finalProportion = Math.Min(proportion, 0.1f);
				return (int)(GetSlaveWorkforce(town.Settlement) * (finalProportion * 10f));
			}
			else return base.GetBoostAmount(town);
        }

		private float GetSlaveWorkforce(Settlement settlement)
        {
			PopulationData data = GetPopData(settlement);
			int slaves = data.GetTypeCount(PopType.Slaves);
			return (float)slaves * SLAVE_CONSTRUCTION;
		}

		private int CalculateDailyConstructionPowerInternal(Town town, ref ExplainedNumber result, bool omitBoost = false)
		{
			PopulationData data = GetPopData(town.Settlement);
			int slaves = data.GetTypeCount(PopType.Slaves);
			result.Add(GetSlaveWorkforce(town.Settlement), new TextObject("Slave workforce"), null);

			if (!omitBoost && town.BoostBuildingProcess > 0)
			{
				int num = town.IsCastle ? 250 : 500;
				int num2 = this.GetBoostAmount(town);
				float num3 = Math.Min(1f, (float)town.BoostBuildingProcess / (float)num);
				float num4 = 0f;
				if (town.IsTown && town.Governor != null && town.Governor.GetPerkValue(DefaultPerks.Engineering.Clockwork))
				{
					num4 += DefaultPerks.Engineering.Clockwork.SecondaryBonus;
				}
				num2 += MBMath.Round((float)num2 * num4);
				result.Add((float)num2 * num3, new TextObject("Craftsmen services"), null);
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
					{
						result.AddFactor(DefaultPerks.Engineering.MilitaryPlanner.SecondaryBonus, DefaultPerks.Engineering.MilitaryPlanner.Name);
					}
					else if (town.IsTown && town.Governor.GetPerkValue(DefaultPerks.Engineering.Carpenters))
					{
						result.AddFactor(DefaultPerks.Engineering.Carpenters.SecondaryBonus, DefaultPerks.Engineering.Carpenters.Name);
					}
					Building building = town.BuildingsInProgress.Peek();
					if ((building.BuildingType == DefaultBuildingTypes.Fortifications || building.BuildingType == DefaultBuildingTypes.CastleBarracks || building.BuildingType == DefaultBuildingTypes.CastleMilitiaBarracks || building.BuildingType == DefaultBuildingTypes.SettlementGarrisonBarracks || building.BuildingType == DefaultBuildingTypes.SettlementMilitiaBarracks || building.BuildingType == DefaultBuildingTypes.SettlementAquaducts) && town.Governor.GetPerkValue(DefaultPerks.Engineering.Stonecutters))
					{
						result.AddFactor(DefaultPerks.Engineering.Stonecutters.PrimaryBonus, DefaultPerks.Engineering.Stonecutters.Name);
					}
				}
			}
			SettlementLoyaltyModel settlementLoyaltyModel = Campaign.Current.Models.SettlementLoyaltyModel;
			int num5 = Enumerable.Sum<Town.SellLog>(town.SoldItems, delegate (Town.SellLog x)
			{
				if (x.Category.Properties != ItemCategory.Property.BonusToProduction)
				{
					return 0;
				}
				return x.Number;
			});
			if (num5 > 0)
			{
				result.Add(0.25f * (float)num5, this.ProductionFromMarketText, null);
			}
			BuildingType buildingType = town.BuildingsInProgress.IsEmpty<Building>() ? null : town.BuildingsInProgress.Peek().BuildingType;
			if (Enumerable.Contains<BuildingType>(DefaultBuildingTypes.MilitaryBuildings, buildingType))
			{
				PerkHelper.AddPerkBonusForTown(DefaultPerks.TwoHanded.Confidence, town, ref result);
			}
			if (buildingType == DefaultBuildingTypes.SettlementMarketplace || buildingType == DefaultBuildingTypes.SettlementAquaducts || buildingType == DefaultBuildingTypes.SettlementLimeKilns)
			{
				PerkHelper.AddPerkBonusForTown(DefaultPerks.Trade.SelfMadeMan, town, ref result);
			}
			float effectOfBuildings = town.GetEffectOfBuildings(BuildingEffectEnum.Construction);
			if (effectOfBuildings > 0f)
			{
				result.Add(effectOfBuildings, GameTexts.FindText("str_building_bonus", null), null);
			}
			if (town.OwnerClan.Leader.CharacterObject.GetFeatValue(DefaultFeats.Cultural.BattanianNegativeConstructionFeat))
			{
				result.AddFactor(DefaultFeats.Cultural.BattanianNegativeConstructionFeat.EffectBonus, this.CultureText);
			}
			if (town.Loyalty >= 75f)
			{
				float num6 = MBMath.Map(town.Loyalty, 75f, 100f, 0f, 20f);
				float value2 = result.ResultNumber * (num6 / 100f);
				result.Add(value2, this.HighLoyaltyBonusText, null);
			}
			else if (town.Loyalty > 25f && town.Loyalty <= 50f)
			{
				float num7 = MBMath.Map(town.Loyalty, 25f, 50f, 50f, 0f);
				float num8 = result.ResultNumber * (num7 / 100f);
				result.Add(-num8, this.LowLoyaltyPenaltyText, null);
			}
			else if (town.Loyalty <= 25f)
			{
				result.Add(-result.ResultNumber, this.VeryLowLoyaltyPenaltyText, null);
			}
			result.LimitMin(0f);
			return (int)result.ResultNumber;
		}

		private const float SLAVE_CONSTRUCTION = 0.01f;

		private const float HammerMultiplier = 0.01f;

		// Token: 0x04000EF0 RID: 3824
		private const int VeryLowLoyaltyValue = 25;

		// Token: 0x04000EF1 RID: 3825
		private const float MediumLoyaltyValue = 50f;

		// Token: 0x04000EF2 RID: 3826
		private const float HighLoyaltyValue = 75f;

		// Token: 0x04000EF3 RID: 3827
		private const float HighestLoyaltyValue = 100f;

		// Token: 0x04000EF4 RID: 3828
		private readonly TextObject ProductionFromMarketText = new TextObject("{=vaZDJGMx}Construction from Market", null);
		private readonly TextObject CultureText = GameTexts.FindText("str_culture", null);
		private readonly TextObject BoostText = new TextObject("{=!}Boost from craftsmen", null);
		private readonly TextObject HighLoyaltyBonusText = new TextObject("{=aSniKUJv}High Loyalty", null);
		private readonly TextObject LowLoyaltyPenaltyText = new TextObject("{=SJ2qsRdF}Low Loyalty", null);
		private readonly TextObject VeryLowLoyaltyPenaltyText = new TextObject("{=CcQzFnpN}Very Low Loyalty", null);
	}
}
