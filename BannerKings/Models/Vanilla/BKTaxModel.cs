using BannerKings.Behaviours;
using BannerKings.Extensions;
using BannerKings.Managers.Buildings;
using BannerKings.Managers.Court;
using BannerKings.Managers.Cultures;
using BannerKings.Managers.Policies;
using BannerKings.Managers.Populations;
using BannerKings.Managers.Populations.Villages;
using BannerKings.Managers.Titles;
using BannerKings.Managers.Titles.Laws;
using BannerKings.Models.BKModels;
using BannerKings.Models.Vanilla.Abstract;
using BannerKings.Settings;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static BannerKings.Managers.PopulationManager;
using TaxType = BannerKings.Managers.Policies.BKTaxPolicy.TaxType;

namespace BannerKings.Models.Vanilla
{
    public class BKTaxModel : TaxModel
    {
        public override float NobleTaxOutput => 1.2f;

        public override float CraftsmanTaxOutput => 0.3f;

        public override float TenantTaxOutput => 0.12f;

        public override float SerfTaxOutput => 0.07f;

        public override float SlaveTaxOutput => 0.10f;
        public override float SettlementCommissionRateTown => 0.1f;

        public override float GetNobleTaxRate(FeudalTitle title, PopulationData data, BKTaxPolicy policy)
        {
            float result = NobleTaxOutput;
            float nobles = data.GetTypeCount(PopType.Nobles);
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

            switch (policy.Policy)
            {
                case TaxType.Low:
                    result *= 0.85f;
                    break;
                case TaxType.High:
                    result *= 1.15f;
                    break;
            }

            return nobles * result * BannerKingsSettings.Instance.TaxIncome;
        }

        public override float GetCraftsmenTaxRate(FeudalTitle title, PopulationData data, BKTaxPolicy policy)
        {
            float result = CraftsmanTaxOutput;
            float craftsmen = data.GetTypeCount(PopType.Craftsmen);
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

            switch (policy.Policy)
            {
                case TaxType.Low:
                    result *= 0.85f;
                    break;
                case TaxType.High:
                    result *= 1.15f;
                    break;
            }

            return craftsmen * result * BannerKingsSettings.Instance.TaxIncome * data.EconomicData.Mercantilism.ResultNumber;
        }

        public override float GetSlaveTaxRate(FeudalTitle title, PopulationData data, BKTaxPolicy policy)
        {
            float result = SlaveTaxOutput;
            float slaves = data.GetTypeCount(PopType.Slaves);
            if (title != null)
            {
                if (title.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.SlavesDomesticDuties))
                {
                    result *= 1.15f;
                }
            }

            switch (policy.Policy)
            {
                case TaxType.Low:
                    result *= 0.85f;
                    break;
                case TaxType.High:
                    result *= 1.15f;
                    break;
            }

            var taxSlaves = BannerKingsConfig.Instance.PolicyManager.IsDecisionEnacted(data.Settlement, "decision_slaves_tax");
            return slaves * result * BannerKingsSettings.Instance.TaxIncome * (taxSlaves ? 1f : 1f - data.EconomicData.StateSlaves);
        }

        public override float GetTenantTaxRate(FeudalTitle title, PopulationData data, BKTaxPolicy policy)
        {
            float result = TenantTaxOutput;
            float tenants = data.GetTypeCount(PopType.Tenants);
            if (title != null)
            {
                if (title.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.SlavesDomesticDuties))
                {
                    result *= 1.15f;
                }
            }

            switch (policy.Policy)
            {
                case TaxType.Low:
                    result *= 0.85f;
                    break;
                case TaxType.High:
                    result *= 1.15f;
                    break;
            }

            return tenants * result * BannerKingsSettings.Instance.TaxIncome;
        }

        public override float GetSerfTaxRate(FeudalTitle title, PopulationData data, BKTaxPolicy policy)
        {
            float result = SerfTaxOutput;
            float serfs = data.GetTypeCount(PopType.Serfs);
            if (title != null)
            {
                if (title.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.SlavesDomesticDuties))
                {
                    result *= 1.15f;
                }
            }

            switch (policy.Policy)
            {
                case TaxType.Low:
                    result *= 0.85f;
                    break;
                case TaxType.High:
                    result *= 1.15f;
                    break;
            }

            return serfs * result * BannerKingsSettings.Instance.TaxIncome;
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
                    var title = BannerKingsConfig.Instance.TitleManager.GetTitle(town.Settlement);
                    var taxType = (BKTaxPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(town.Settlement, "tax");
                    baseResult.Add(GetNobleTaxRate(title, data, taxType), new TextObject("{=26yWZwHo}{CLASS} taxes")
                        .SetTextVariable("CLASS", DefaultPopulationNames.Instance.GetPopulationName(town.Culture, PopType.Nobles).Name));

                    baseResult.Add(GetCraftsmenTaxRate(title, data, taxType), new TextObject("{=26yWZwHo}{CLASS} taxes")
                        .SetTextVariable("CLASS", DefaultPopulationNames.Instance.GetPopulationName(town.Culture, PopType.Craftsmen).Name));

                    baseResult.Add(GetTenantTaxRate(title, data, taxType), new TextObject("{=26yWZwHo}{CLASS} taxes")
                        .SetTextVariable("CLASS", DefaultPopulationNames.Instance.GetPopulationName(town.Culture, PopType.Tenants).Name));
                    
                    baseResult.Add(GetSerfTaxRate(title, data, taxType), new TextObject("{=26yWZwHo}{CLASS} taxes")
                        .SetTextVariable("CLASS", DefaultPopulationNames.Instance.GetPopulationName(town.Culture, PopType.Serfs).Name));

                    baseResult.Add(GetSlaveTaxRate(title, data, taxType),new TextObject("{=26yWZwHo}{CLASS} taxes")
                        .SetTextVariable("CLASS", DefaultPopulationNames.Instance.GetPopulationName(town.Culture, PopType.Slaves).Name));

                    foreach (Workshop wk in town.Workshops)
                    {
                        if (wk.Owner != town.OwnerClan.Leader)
                        {
                            baseResult.Add(BannerKingsConfig.Instance.ClanFinanceModel.GetWorkshopTaxes(wk),
                                new TextObject("{=GKtDoLCd}Taxes from {WORKSHOP} ({OWNER})")
                                .SetTextVariable("WORKSHOP", wk.Name)
                                .SetTextVariable("OWNER", wk.Owner.Name));
                        }
                    }

                    float tariff = data.EconomicData.Tariff;
                    baseResult.Add(data.EconomicData.ConsumedValue * tariff, new TextObject("{=2vBXHPsS}Goods consumed by the populace ({TARIFF}% tariffs)")
                        .SetTextVariable("TARIFF", (tariff * 100f).ToString("0.00")));

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

                    /*var ownerReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(town.OwnerClan.Leader);
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
                    }*/

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
                float taxOffice = data.VillageData.GetBuildingLevel(DefaultVillageBuildings.Instance.TaxOffice);
                if (taxOffice > 0)
                {
                    AddVillagePopulationTaxes(ref result, data, taxOffice);
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

        public void AddVillagePopulationTaxes(ref ExplainedNumber result, PopulationData data, float taxOfficeLevel)
        {
            var title = BannerKingsConfig.Instance.TitleManager.GetTitle(data.Settlement);
            var taxType = (BKTaxPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(data.Settlement, "tax");

            result.Add(MBMath.ClampFloat(GetNobleTaxRate(title, data, taxType) * (0.33f * taxOfficeLevel), 0f, 50000f),
                new TextObject("{=5mCY3JCP}{CLASS} output")
                .SetTextVariable("CLASS", new TextObject("{=pop_class_nobles}Nobles")));

            result.Add(MBMath.ClampFloat(GetCraftsmenTaxRate(title, data, taxType) * (0.33f * taxOfficeLevel), 0f, 50000f),
                new TextObject("{=5mCY3JCP}{CLASS} output")
                .SetTextVariable("CLASS", new TextObject("{=pop_class_craftsmen}Craftsmen")));
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
            if (BannerKingsConfig.Instance.PolicyManager.IsDecisionEnacted(town.Settlement, "decision_tariff_exempt"))
            {
                return 0f;
            }

            float result = base.GetTownTaxRatio(town);
            var taxType = ((BKTaxPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(town.Settlement, "tax")).Policy;
            switch (taxType)
            {
                case TaxType.Low:
                    result -= 0.04f;
                    break;
                case TaxType.High:
                    result += 0.04f;
                    break;
                case TaxType.Exemption:
                    result = 0f;
                    break;
            }

            return result;
        }
    }
}