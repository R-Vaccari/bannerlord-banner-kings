using BannerKings.Behaviours;
using BannerKings.Extensions;
using BannerKings.Managers.Buildings;
using BannerKings.Managers.Court;
using BannerKings.Managers.Institutions.Religions.Doctrines;
using BannerKings.Managers.Policies;
using BannerKings.Managers.Populations;
using BannerKings.Managers.Populations.Villages;
using BannerKings.Managers.Titles;
using BannerKings.Managers.Titles.Laws;
using BannerKings.Models.BKModels;
using BannerKings.Settings;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static BannerKings.Managers.PopulationManager;
using TaxType = BannerKings.Managers.Policies.BKTaxPolicy.TaxType;

namespace BannerKings.Models.Vanilla
{
    public class BKTaxModel : DefaultSettlementTaxModel
    {
        public static readonly float SERF_OUTPUT = 0.2f;
        public static readonly float TENANT_OUTPUT = 0.17f;

        public float GetNobleOutput(FeudalTitle title)
        {
            float result = 2.2f;

            if (title != null)
            {
                if (title.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.NoblesTaxDuties))
                {
                    result *= 1.25f;
                }
                else if (title.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.NoblesLaxDuties))
                {
                    result *= 0.6f;
                }
            }

            return result;
        }

        public float GetCraftsmenOutput(FeudalTitle title)
        {
            float result = 0.8f;

            if (title != null)
            {
                if (title.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.CraftsmenTaxDuties))
                {
                    result *= 1.35f;
                }
                else if (title.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.CraftsmenLaxDuties))
                {
                    result *= 0.6f;
                }
            }

            return result;
        }

        public float GetSlaveOutput(FeudalTitle title)
        {
            float result = 0.24f;

            if (title != null)
            {
                if (title.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.SlavesDomesticDuties))
                {
                    result *= 1.15f;
                }
            }

            return result;
        }

        public override ExplainedNumber CalculateTownTax(Town town, bool includeDescriptions = false)
        {
            ExplainedNumber baseResult = base.CalculateTownTax(town, includeDescriptions);
            baseResult.LimitMin(0f);
            if (BannerKingsConfig.Instance.PopulationManager != null)
            {
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement);
                if (data != null)
                {
                    baseResult.LimitMin(-200000f);
                    baseResult.LimitMax(200000f);
                    var taxSlaves =
                        BannerKingsConfig.Instance.PolicyManager.IsDecisionEnacted(town.Settlement, "decision_slaves_tax");

                    float nobles = data.GetTypeCount(PopType.Nobles);
                    float craftsmen = data.GetTypeCount(PopType.Craftsmen);
                    float serfs = data.GetTypeCount(PopType.Serfs);
                    float slaves = data.GetTypeCount(PopType.Slaves);
                    float tenants = data.GetTypeCount(PopType.Tenants);

                    if (craftsmen > 0)
                    {
                        craftsmen *= 1f - data.EconomicData.Mercantilism.ResultNumber;
                    }

                    if (slaves > 0)
                    {
                        slaves *= taxSlaves ? 1f : 1f - data.EconomicData.StateSlaves;
                    }

                    var title = BannerKingsConfig.Instance.TitleManager.GetTitle(town.Settlement);

                    if (nobles > 0f)
                    {
                        baseResult.Add(MBMath.ClampFloat(nobles * GetNobleOutput(title), 0f, 50000f) * BannerKingsSettings.Instance.TaxIncome,
                            new TextObject("{=5mCY3JCP}{CLASS} output")
                            .SetTextVariable("CLASS", new TextObject("{=pop_class_nobles}Nobles")));
                    }

                    if (craftsmen > 0f)
                    {
                        baseResult.Add(MBMath.ClampFloat(craftsmen * GetCraftsmenOutput(title), 0f, 50000f) * BannerKingsSettings.Instance.TaxIncome,
                            new TextObject("{=5mCY3JCP}{CLASS} output").SetTextVariable("CLASS", new TextObject("Craftsmen")));
                    }

                    if (serfs > 0f)
                    {
                        baseResult.Add(MBMath.ClampFloat(serfs * SERF_OUTPUT, 0f, 50000f) * BannerKingsSettings.Instance.TaxIncome,
                            new TextObject("{=5mCY3JCP}{CLASS} output").SetTextVariable("CLASS", new TextObject("Serfs")));
                    }

                    if (slaves > 0f)
                    {
                        baseResult.Add(MBMath.ClampFloat(slaves * GetSlaveOutput(title), 0f, 50000f) * BannerKingsSettings.Instance.TaxIncome,
                            new TextObject("{=5mCY3JCP}{CLASS} output").SetTextVariable("CLASS", new TextObject("Slaves")));
                    }

                    if (tenants > 0f)
                    {
                        baseResult.Add(MBMath.ClampFloat(tenants * TENANT_OUTPUT, 0f, 50000f) * BannerKingsSettings.Instance.TaxIncome,
                                                   new TextObject("{=5mCY3JCP}{CLASS} output")
                                                   .SetTextVariable("CLASS", new TextObject("Tenants")));
                    }

                    var buildingBehavior = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKBuildingsBehavior>();
                    int mining = buildingBehavior.GetMiningRevenue(town);
                    if (mining > 0)
                    {
                        baseResult.Add(mining, BKBuildings.Instance.Mines.Name);
                    }

                    int materials = buildingBehavior.GetMaterialExpenses(town);
                    if (materials > 0)
                    {
                        baseResult.Add(-materials, new TextObject("{=a9HMZMAF}Project material expenses"));
                    }

                    var ownerReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(town.OwnerClan.Leader);
                    if (data.ReligionData != null && ownerReligion != null &&
                        ownerReligion.HasDoctrine(DefaultDoctrines.Instance.HeathenTax))
                    {
                        float heathens = data.ReligionData.GetHeathenPercentage(ownerReligion);
                        if (heathens != 0f)
                        {
                            float result = 0f;
                            if (nobles > 0f)
                            {
                                result += MBMath.ClampFloat(nobles * GetNobleOutput(title), 0f, 50000f) * 0.1f;
                            }

                            if (craftsmen > 0f)
                            {
                                result += MBMath.ClampFloat(craftsmen * GetCraftsmenOutput(title), 0f, 50000f) * 0.1f;
                            }

                            if (serfs > 0f)
                            {
                                result += MBMath.ClampFloat(serfs * SERF_OUTPUT, 0f, 50000f) * 0.1f;
                            }

                            baseResult.Add(result * heathens, DefaultDoctrines.Instance.HeathenTax.Name);
                        }
                    }

                    var taxType = ((BKTaxPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(town.Settlement, "tax"))
                            .Policy;
                    switch (taxType)
                    {
                        case TaxType.Low:
                            baseResult.AddFactor(-0.15f, new TextObject("{=L7QhNa6a}Tax policy"));
                            break;
                        case TaxType.High:
                            baseResult.AddFactor(0.15f, new TextObject("{=L7QhNa6a}Tax policy"));
                            break;
                    }

                    if (baseResult.ResultNumber > 0f)
                    {
                        baseResult.Add(baseResult.ResultNumber * -0.6f * data.Autonomy, new TextObject("{=xMsWoSnL}Autonomy"));
                    }

                    CouncilData council = BannerKingsConfig.Instance.CourtManager.GetCouncil(town.Settlement.OwnerClan);
                    CalculateDueTax(data, baseResult.ResultNumber);
                    CalculateDueWages(council,baseResult.ResultNumber);

                    var admCost = new BKAdministrativeModel().CalculateEffect(town.Settlement).ResultNumber;
                    baseResult.AddFactor(admCost * -1f, new TextObject("{=y1sBiOKa}Administrative costs"));
                }
            }

            return baseResult;
        }

        public ExplainedNumber CalculateVillageTaxFromIncome(Village village, bool descriptions = false, bool applyWithdrawal = false)
        {
            var result = new ExplainedNumber(0f, descriptions);
            result.LimitMin(0f);
            result.LimitMax(10000f);
            if (village.VillageState is Village.VillageStates.Looted or Village.VillageStates.BeingRaided)
            {
                return result;
            }

            float factor = 1f;
            if (BannerKingsSettings.Instance.VillageTaxReserves)
            {
                factor = 0.8f;
            }

            if (village.TradeTaxAccumulated > 0)
            {
                result.Add(village.TradeTaxAccumulated * factor, new TextObject("{=ZJePQQpz}Production sold by villagers"));
            }

            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(village.Settlement);
            if (data != null && data.VillageData != null)
            {
                var taxType = ((BKTaxPolicy)BannerKingsConfig.Instance.PolicyManager
                    .GetPolicy(village.Settlement, "tax")).Policy;

                float nobles = data.GetTypeCount(PopType.Nobles);
                float craftsmen = data.GetTypeCount(PopType.Craftsmen);

                float taxOffice = data.VillageData.GetBuildingLevel(DefaultVillageBuildings.Instance.TaxOffice);
                if (taxOffice > 0)
                {
                    AddVillagePopulationTaxes(ref result, village.Settlement, nobles, craftsmen, taxOffice, taxType);
                }

                var admCost = BannerKingsConfig.Instance.AdministrativeModel.CalculateEffect(village.Settlement).ResultNumber;
                result.AddFactor(admCost * -1f, new TextObject("{=y1sBiOKa}Administrative costs"));
            }

            var clan = village.GetActualOwner().Clan;

            if (clan.Kingdom != null && clan.Kingdom.RulingClan != clan && clan.Kingdom.ActivePolicies.Contains(DefaultPolicies.LandTax))
            {
                result.AddFactor(-0.05f, DefaultPolicies.LandTax.Name);
            }

            var governor = village.Bound.Town.Governor;
            if (governor != null)
            {
                if (governor.GetPerkValue(DefaultPerks.Scouting.ForestKin))
                {
                    result.AddFactor(0.1f, DefaultPerks.Scouting.ForestKin.Name);
                }

                if (governor.GetPerkValue(DefaultPerks.Steward.Logistician))
                {
                    result.AddFactor(0.1f, DefaultPerks.Steward.Logistician.Name);
                }
            }

            var council = BannerKingsConfig.Instance.CourtManager.GetCouncil(village.Settlement.OwnerClan);
            if (council != null)
            {
                CalculateDueWages(council, (float)result.ResultNumber);
            }

            if (data != null)
            {
                CalculateDueTax(data, (float)result.ResultNumber);
            }

            return result;
        }


        public override int CalculateVillageTaxFromIncome(Village village, int marketIncome)
        {
            var factor = 0.7f;
            var taxType = ((BKTaxPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(village.Settlement, "tax"))
                   .Policy;
            switch (taxType)
            {
                case TaxType.Low:
                    factor = 0.5f;
                    break;
                case TaxType.High:
                    factor = 0.9f;
                    break;
            }
            return (int)(marketIncome * factor);
        }

        public void AddVillagePopulationTaxes(ref ExplainedNumber result, Settlement settlement, float nobles, float craftsmen, float taxOfficeLevel, TaxType taxType)
        {
            float taxFactor = 1f;
            switch (taxType)
            {
                case TaxType.Low:
                    taxFactor = 0.8f;
                    break;
                case TaxType.High:
                    taxFactor = 1.2f;
                    break;
            }

            var title = BannerKingsConfig.Instance.TitleManager.GetTitle(settlement);
            if (nobles > 0f)
            {
                result.Add(MBMath.ClampFloat(nobles * GetNobleOutput(title) * taxFactor * (0.33f * taxOfficeLevel), 0f, 50000f),
                    new TextObject("{=5mCY3JCP}{CLASS} output")
                    .SetTextVariable("CLASS", new TextObject("{=pop_class_nobles}Nobles")));
            }

            if (craftsmen > 0f)
            {
                result.Add(MBMath.ClampFloat(craftsmen * GetCraftsmenOutput(title) * taxFactor * (0.33f * taxOfficeLevel), 0f, 50000f),
                    new TextObject("{=5mCY3JCP}{CLASS} output")
                    .SetTextVariable("CLASS", new TextObject("{=pop_class_craftsmen}Craftsmen")));
            }
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

            var contract = titleData.Title.Contract;
            if (contract != null && contract.Duties.ContainsKey(FeudalDuties.Taxation))
            {
                var factor = MBMath.ClampFloat(contract.Duties[FeudalDuties.Taxation], 0f, 0.8f);
                titleData.Title.DueTax = result * factor;
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