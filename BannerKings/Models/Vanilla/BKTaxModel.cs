using BannerKings.Managers.Court;
using BannerKings.Managers.Policies;
using BannerKings.Managers.Populations;
using BannerKings.Managers.Titles;
using BannerKings.Models.BKModels;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static BannerKings.Managers.Policies.BKTaxPolicy;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Models.Vanilla
{
    public class BKTaxModel : DefaultSettlementTaxModel
    {
        public static readonly float NOBLE_OUTPUT = 4.2f;
        public static readonly float CRAFTSMEN_OUTPUT = 1.2f;
        public static readonly float SERF_OUTPUT = 0.26f;
        public static readonly float SLAVE_OUTPUT = 0.33f;

        public override ExplainedNumber CalculateTownTax(Town town, bool includeDescriptions = false)
        {
            var baseResult = base.CalculateTownTax(town, includeDescriptions);

            if (BannerKingsConfig.Instance.PopulationManager != null &&
                BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(town.Settlement))
            {
                baseResult.LimitMin(-200000f);
                baseResult.LimitMax(200000f);
                var taxSlaves =
                    BannerKingsConfig.Instance.PolicyManager.IsDecisionEnacted(town.Settlement, "decision_slaves_tax");
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement);
                float nobles = data.GetTypeCount(PopType.Nobles);
                float craftsmen = data.GetTypeCount(PopType.Nobles);
                float serfs = data.GetTypeCount(PopType.Nobles);
                float slaves = data.GetTypeCount(PopType.Slaves);

                if (craftsmen > 0)
                {
                    craftsmen *= 1f - data.EconomicData.Mercantilism.ResultNumber;
                }

                if (slaves > 0)
                {
                    slaves *= taxSlaves ? 1f : 1f - data.EconomicData.StateSlaves;
                }

                if (nobles > 0f)
                {
                    baseResult.Add(MBMath.ClampFloat(nobles * NOBLE_OUTPUT, 0f, 50000f),
                        new TextObject("{=vEqydn0ZD}{CLASS} output").SetTextVariable("CLASS", "Nobles"));
                }

                if (craftsmen > 0f)
                {
                    baseResult.Add(MBMath.ClampFloat(craftsmen * CRAFTSMEN_OUTPUT, 0f, 50000f),
                        new TextObject("{=vEqydn0ZD}{CLASS} output").SetTextVariable("CLASS", "Craftsmen"));
                }

                if (serfs > 0f)
                {
                    baseResult.Add(MBMath.ClampFloat(serfs * SERF_OUTPUT, 0f, 50000f),
                        new TextObject("{=vEqydn0ZD}{CLASS} output").SetTextVariable("CLASS", "Serfs"));
                }

                if (slaves > 0f)
                {
                    baseResult.Add(MBMath.ClampFloat(slaves * SLAVE_OUTPUT, 0f, 50000f),
                        new TextObject("{=vEqydn0ZD}{CLASS} output").SetTextVariable("CLASS", "Slaves"));
                }

                var taxType = ((BKTaxPolicy) BannerKingsConfig.Instance.PolicyManager.GetPolicy(town.Settlement, "tax"))
                    .Policy;
                switch (taxType)
                {
                    case TaxType.Low:
                        baseResult.AddFactor(-0.15f, new TextObject("Tax policy"));
                        break;
                    case TaxType.High:
                        baseResult.AddFactor(0.15f, new TextObject("Tax policy"));
                        break;
                }

                var legitimacy = (LegitimacyType) new BKLegitimacyModel().CalculateEffect(town.Settlement).ResultNumber;
                if (legitimacy == LegitimacyType.Lawful)
                {
                    baseResult.AddFactor(0.05f, new TextObject("{=zFrQddkvj}Legitimiacy"));
                }

                var admCost = new BKAdministrativeModel().CalculateEffect(town.Settlement).ResultNumber;
                baseResult.AddFactor(admCost * -1f, new TextObject("Administrative costs"));

                if (baseResult.ResultNumber > 0f)
                {
                    baseResult.AddFactor(-0.6f * data.Autonomy, new TextObject("{=Rn8j5tCnH}Autonomy"));
                }

                CalculateDueTax(data, baseResult.ResultNumber);
                CalculateDueWages(BannerKingsConfig.Instance.CourtManager.GetCouncil(town.Settlement.OwnerClan),
                    baseResult.ResultNumber);
                baseResult.AddFactor(admCost * -1f, new TextObject("Administrative costs"));
            }

            return baseResult;
        }

        public override int CalculateVillageTaxFromIncome(Village village, int marketIncome)
        {
            var baseResult = marketIncome * 0.7;
            if (BannerKingsConfig.Instance.PolicyManager != null)
            {
                var taxType = ((BKTaxPolicy) BannerKingsConfig.Instance.PolicyManager.GetPolicy(village.Settlement, "tax"))
                    .Policy;
                switch (taxType)
                {
                    case TaxType.High:
                        baseResult = marketIncome * 9f;
                        break;
                    case TaxType.Low:
                        baseResult = marketIncome * 0.5f;
                        break;
                    case TaxType.Exemption when marketIncome > 0:
                    {
                        baseResult = 0;
                        var random = MBRandom.RandomInt(1, 100);
                        if (random <= 33 && village.Settlement.Notables != null)
                        {
                            var notable = village.Settlement.Notables.GetRandomElement();
                            if (notable != null)
                            {
                                ChangeRelationAction.ApplyRelationChangeBetweenHeroes(village.Settlement.Owner, notable, 1);
                            }
                        }

                        break;
                    }
                }

                if (baseResult > 0)
                {
                    var admCost = new BKAdministrativeModel().CalculateEffect(village.Settlement).ResultNumber;
                    baseResult *= 1f - admCost;
                }

                CalculateDueTax(BannerKingsConfig.Instance.PopulationManager.GetPopData(village.Settlement),
                    (float) baseResult);
                var data = BannerKingsConfig.Instance.CourtManager.GetCouncil(village.Settlement.OwnerClan);
                if (data != null)
                {
                    CalculateDueWages(data, (float) baseResult);
                }
            }

            return (int) baseResult;
        }

        private void CalculateDueWages(CouncilData data, float result)
        {
            foreach (var position in data.GetOccupiedPositions())
            {
                var cost = position.AdministrativeCosts();
                if (cost > 0f)
                {
                    position.DueWage = (int) (result * cost);
                }
            }
        }

        private void CalculateDueTax(PopulationData data, float result)
        {
            var titleData = data.TitleData;
            if (titleData?.Title == null)
            {
                return;
            }

            var contract = titleData.Title.contract;
            if (contract != null && contract.Duties.ContainsKey(FeudalDuties.Taxation))
            {
                var factor = MBMath.ClampFloat(contract.Duties[FeudalDuties.Taxation], 0f, 0.8f);
                titleData.Title.dueTax = result * factor;
            }
        }

        public override float GetTownTaxRatio(Town town)
        {
            if (BannerKingsConfig.Instance.PolicyManager != null)
            {
                if (BannerKingsConfig.Instance.PolicyManager.IsDecisionEnacted(town.Settlement, "decision_tariff_exempt"))
                {
                    return 0f;
                }
            }

            return base.GetTownTaxRatio(town);
        }
    }
}