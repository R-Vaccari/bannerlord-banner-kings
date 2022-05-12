using BannerKings.Managers.Policies;
using BannerKings.Managers.Titles;
using BannerKings.Populations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static BannerKings.Managers.Policies.BKTaxPolicy;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Models
{
    class BKTaxModel : CalradiaExpandedKingdoms.Models.CEKSettlementTaxModel
    {
        public static readonly float NOBLE_OUTPUT = 2.2f;
        public static readonly float CRAFTSMEN_OUTPUT = 0.82f;
        public static readonly float SERF_OUTPUT = 0.22f;
        public static readonly float SLAVE_OUTPUT = 0.33f;

        public override ExplainedNumber CalculateTownTax(Town town, bool includeDescriptions = false)
        {
            ExplainedNumber baseResult = base.CalculateTownTax(town, includeDescriptions);

            if (BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(town.Settlement))
            {
                baseResult.LimitMin(-200000f);
                baseResult.LimitMax(200000f);
                bool taxSlaves = BannerKingsConfig.Instance.PolicyManager.IsDecisionEnacted(town.Settlement, "decision_slaves_tax");
                PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement);
                float nobles = data.GetTypeCount(PopType.Nobles);
                float craftsmen = data.GetTypeCount(PopType.Nobles);
                float serfs = data.GetTypeCount(PopType.Nobles);
                float slaves = data.GetTypeCount(PopType.Slaves);

                if (craftsmen > 0)
                    craftsmen *= (1f - data.EconomicData.Mercantilism.ResultNumber);

                if (slaves > 0)
                    slaves *= (taxSlaves ? 1f : 1f - data.EconomicData.StateSlaves);

                if (nobles > 0f) baseResult.Add(MBMath.ClampFloat(nobles * NOBLE_OUTPUT, 0f, 50000f), new TextObject("{=!}{CLASS} output").SetTextVariable("CLASS", "Nobles"));
                if (craftsmen > 0f) baseResult.Add(MBMath.ClampFloat(craftsmen * CRAFTSMEN_OUTPUT, 0f, 50000f), new TextObject("{=!}{CLASS} output").SetTextVariable("CLASS", "Craftsmen"));
                if (serfs > 0f) baseResult.Add(MBMath.ClampFloat(serfs * SERF_OUTPUT, 0f, 50000f), new TextObject("{=!}{CLASS} output").SetTextVariable("CLASS", "Serfs"));
                if (slaves > 0f) baseResult.Add(MBMath.ClampFloat(slaves * SLAVE_OUTPUT, 0f, 50000f), new TextObject("{=!}{CLASS} output").SetTextVariable("CLASS", "Slaves"));

                TaxType taxType = ((BKTaxPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(town.Settlement, "tax")).Policy;
                if (taxType == TaxType.Low)
                    baseResult.AddFactor(-0.15f, new TextObject("Tax policy"));
                else if (taxType == TaxType.High)
                    baseResult.AddFactor(0.15f, new TextObject("Tax policy"));


                float admCost = new BKAdministrativeModel().CalculateEffect(town.Settlement).ResultNumber;
                baseResult.AddFactor(admCost * -1f, new TextObject("Administrative costs"));

                if (baseResult.ResultNumber > 0f)
                    baseResult.AddFactor(-0.6f * data.Autonomy, new TextObject("{=!}Autonomy"));

                CalculateDueTax(data, baseResult.ResultNumber);
            }

            return baseResult;
        }

        public override int CalculateVillageTaxFromIncome(Village village, int marketIncome)
        {
            double baseResult = marketIncome * 0.7;
            if (BannerKingsConfig.Instance.PolicyManager != null)
            {
                TaxType taxType = ((BKTaxPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(village.Settlement, "tax")).Policy;
                if (taxType == TaxType.High)
                    baseResult = marketIncome * 9f;
                else if (taxType == TaxType.Low) baseResult = marketIncome * 0.5f;
                else if (taxType == TaxType.Exemption && marketIncome > 0)
                {
                    baseResult = 0;
                    int random = MBRandom.RandomInt(1, 100);
                    if (random <= 33 && village.Settlement.Notables != null)
                    {
                        Hero notable = village.Settlement.Notables.GetRandomElement();
                        if (notable != null) ChangeRelationAction.ApplyRelationChangeBetweenHeroes(village.Settlement.Owner, notable, 1);
                    }
                }
                if (baseResult > 0)
                {
                    float admCost = new BKAdministrativeModel().CalculateEffect(village.Settlement).ResultNumber;
                    baseResult *= 1f - admCost;
                }

                CalculateDueTax(BannerKingsConfig.Instance.PopulationManager.GetPopData(village.Settlement), (float)baseResult);
            }

            return (int)baseResult;
        }

        private void CalculateDueTax(PopulationData data, float result)
        {
            TitleData titleData = data.TitleData;
            if (titleData == null || titleData.Title == null) return;
            FeudalContract contract = titleData.Title.contract;
            if (contract != null && contract.Duties.ContainsKey(FeudalDuties.Taxation))
            {
                float factor = MBMath.ClampFloat(contract.Duties[FeudalDuties.Taxation], 0f, 0.8f);
                titleData.Title.dueTax = result * factor;
            }
        }

        public override float GetTownCommissionChangeBasedOnSecurity(Town town, float commission)
        {
            return commission;
        }

        public override float GetTownTaxRatio(Town town) {
            if (BannerKingsConfig.Instance.PolicyManager != null)
                if (BannerKingsConfig.Instance.PolicyManager.IsDecisionEnacted(town.Settlement, "decision_tariff_exempt"))
                    return 0f;
            
            return base.GetTownTaxRatio(town);
        }

        public override float GetVillageTaxRatio() => base.GetVillageTaxRatio();
        
    }
}
