using BannerKings.Managers;
using BannerKings.Managers.Court;
using BannerKings.Managers.Policies;
using BannerKings.Populations;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static BannerKings.Managers.Policies.BKTaxPolicy;
using static BannerKings.Managers.Policies.BKWorkforcePolicy;
using static BannerKings.Managers.PopulationManager;
using static BannerKings.Managers.TitleManager;

namespace BannerKings.Models
{
    class BKEconomyModel : DefaultSettlementEconomyModel, IEconomyModel
    {
        private static readonly float CRAFTSMEN_EFFECT_CAP = 0.4f;

        public override float GetDailyDemandForCategory(Town town, ItemCategory category, int extraProsperity)
        {
            if (BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(town.Settlement)
                && category.IsValid && category.StringId != "banner")
            {
                PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement);
                float nobles = data.GetTypeCount(PopType.Nobles);
                float craftsmen = data.GetTypeCount(PopType.Craftsmen);
                float serfs = data.GetTypeCount(PopType.Serfs);
                ConsumptionType type = Helpers.Helpers.GetTradeGoodConsumptionType(category);

                float prosperity = 0.5f + town.Prosperity * 0.0001f;
                float baseResult = 0f;
                if (type == ConsumptionType.Luxury)
                {
                    baseResult += nobles * 15f;
                    baseResult += craftsmen * 3f;
                } else if (type == ConsumptionType.Industrial)
                {
                    baseResult += craftsmen * 10f;
                    baseResult += serfs * 0.1f;
                } else
                {
                    baseResult += nobles * 1f;
                    baseResult += craftsmen * 1f;
                    baseResult += serfs * 0.12f;
                }
                
                float num = MathF.Max(0f, baseResult * prosperity + extraProsperity);
                float num2 = MathF.Max(0f, baseResult * prosperity);
                float num3 = category.BaseDemand * num;
                float num4 = category.LuxuryDemand * num2;
                float result = num3 + num4;
                if (category.BaseDemand < 1E-08f)
                {
                    result = num * 0.01f;
                }

                return result;
            } else return base.GetDailyDemandForCategory(town, category, extraProsperity);
        }

        public override float GetEstimatedDemandForCategory(Town town, ItemData itemData, ItemCategory category) => 
            this.GetDailyDemandForCategory(town, category, 1000);

        public override float GetDemandChangeFromValue(float purchaseValue)
        {
            float value = base.GetDemandChangeFromValue(purchaseValue);
            return value;
        }

        public override (float, float) GetSupplyDemandForCategory(Town town, ItemCategory category, float dailySupply, float dailyDemand, float oldSupply, float oldDemand)
        {
            ValueTuple<float, float> baseResult = base.GetSupplyDemandForCategory(town, category, dailySupply, dailyDemand, oldSupply, oldDemand);
            return baseResult;
        }

        public override int GetTownGoldChange(Town town)
        {
            if (BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(town.Settlement))
                return GetMerchantIncome(town);
            
            else return base.GetTownGoldChange(town);
        }

        public int GetMerchantIncome(Town town)
        {
            PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement);
            float slaves = data.GetTypeCount(PopType.Slaves);
            float privateSlaves = slaves * (1f - data.EconomicData.StateSlaves);
            float tax = 1f;
            if (BannerKingsConfig.Instance.PolicyManager.IsDecisionEnacted(town.Settlement, "decision_slaves_tax")) 
            {
                TaxType taxtype = (BannerKingsConfig.Instance.PolicyManager.GetPolicy(town.Settlement, "tax") as BKTaxPolicy).Policy;
                if (taxtype == TaxType.Standard)
                    tax = 0.7f;
                else if (taxtype == TaxType.High)
                    tax = 0.65f;
                else tax = 0.85f;
            }
            float efficiency = data.EconomicData.ProductionEfficiency.ResultNumber;
            return (int)(privateSlaves * tax * efficiency);
        }

        public ExplainedNumber CalculateEffect(Settlement settlement)
        {
            ExplainedNumber result = new ExplainedNumber(0.1f);

            FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetSovereignFromSettlement(settlement);
            if (title != null)
            {
                GovernmentType government = title.contract.Government;
                if (government == GovernmentType.Republic)
                    result.Add(0.4f, new TextObject("Government"));
                else if (government == GovernmentType.Feudal)
                    result.Add(0.2f, new TextObject("Government"));
                else if (government == GovernmentType.Tribal)
                    result.Add(0.1f, new TextObject("Government"));
                else if (government == GovernmentType.Imperial)
                    result.Add(0.05f, new TextObject("Government"));
            }

            if (BannerKingsConfig.Instance.PolicyManager.IsDecisionEnacted(settlement, "decision_mercantilism"))
                result.Add(0.1f, new TextObject("{=!}Encourage mercantilism decision"));

            return result;
        }

        public ExplainedNumber CalculateProductionEfficiency(Settlement settlement)
        {
            ExplainedNumber result = new ExplainedNumber();
            if (!settlement.IsVillage) result.Add(new DefaultWorkshopModel().GetPolicyEffectToProduction(settlement.Town));
            else result.Add(0.7f);
            PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
            float craftsmen = data.GetTypeCount(PopType.Craftsmen);
            result.Add(MathF.Min((craftsmen / 250f) * 0.020f, CRAFTSMEN_EFFECT_CAP), new TextObject("Craftsmen"));

            if (BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(settlement, "workforce", (int)WorkforcePolicy.Martial_Law))
                result.Add(-0.30f, new TextObject("Martial Law policy"));

            float mercantilism = data.EconomicData.Mercantilism.ResultNumber;
            result.Add(0.25f * mercantilism, new TextObject("Mercantilism"));

            GovernmentType government = BannerKingsConfig.Instance.TitleManager.GetSettlementGovernment(settlement);
            if (government == GovernmentType.Feudal)
                result.AddFactor(0.15f, new TextObject("{=!}Government"));

            BannerKingsConfig.Instance.CourtManager.ApplyCouncilEffect(ref result, settlement.OwnerClan.Leader, CouncilPosition.Steward, 0.15f, true);

            return result;
        }

        public ExplainedNumber CalculateProductionQuality(Settlement settlement)
        {
            return new ExplainedNumber(1f);
        }
    }
}
