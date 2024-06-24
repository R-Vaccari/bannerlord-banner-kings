using BannerKings.Behaviours;
using BannerKings.CampaignContent.Skills;
using BannerKings.Extensions;
using BannerKings.Managers.Buildings;
using BannerKings.Managers.Court.Members;
using BannerKings.Managers.Court.Members.Tasks;
using BannerKings.Managers.Innovations;
using BannerKings.Managers.Policies;
using BannerKings.Managers.Populations;
using BannerKings.Managers.Shipping;
using BannerKings.Managers.Skills;
using BannerKings.Managers.Titles.Governments;
using BannerKings.Models.Vanilla.Abstract;
using Helpers;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static BannerKings.Managers.Policies.BKTaxPolicy;
using static BannerKings.Managers.Policies.BKWorkforcePolicy;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Models.Vanilla
{
    public class BKEconomyModel : EconomyModel
    {
        private static readonly float CRAFTSMEN_EFFECT_CAP = 0.4f;

        public override ExplainedNumber CalculateMercantilism(PopulationData data, bool descriptions = false)
        {
            var result = new ExplainedNumber(0.1f, descriptions);
            result.LimitMin(0f);
            result.LimitMax(1f);

            Settlement settlement = data.Settlement;
            var titleData = data.TitleData;
            if (titleData != null)
            {
                var title = titleData.Title;
                if (title != null)
                {
                    Government government = title.Contract.Government;
                    result.Add(government.Mercantilism, new TextObject("Government"));
                }
            }

            if (BannerKingsConfig.Instance.PolicyManager.IsDecisionEnacted(settlement, "decision_mercantilism"))
            {
                result.Add(0.1f, new TextObject("{=OdpuFusm}Encourage mercantilism decision"));
            }

            return result;
        }

        public override ExplainedNumber CalculateMercantilism(Settlement settlement, bool descriptions = false) =>
            CalculateMercantilism(settlement.PopulationData(), descriptions);

        public override ExplainedNumber CalculateProductionEfficiency(Settlement settlement, bool explanations = false, PopulationData data = null)
        {
            var result = new ExplainedNumber(!settlement.IsVillage ? 0.7f : 0.9f, explanations);
            data ??= BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);

            float craftsmen = data.GetTypeCount(PopType.Craftsmen);
            result.Add(MathF.Min(craftsmen / 250f * 0.020f, CRAFTSMEN_EFFECT_CAP), Utils.Helpers.GetClassName(PopType.Craftsmen, settlement.Culture));

            if (BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(settlement, "workforce",
                    (int) WorkforcePolicy.Martial_Law))
            {
                result.Add(-0.30f, new TextObject("{=7cFbhefJ}Martial Law policy"));
            }

            var mercantilism = data.EconomicData.Mercantilism.ResultNumber;
            result.Add(0.25f * mercantilism, new TextObject("Mercantilism"));

            var government = BannerKingsConfig.Instance.TitleManager.GetSettlementGovernment(settlement);
            if (government.Equals(DefaultGovernments.Instance.Feudal))
            {
                result.AddFactor(0.15f, new TextObject("{=PSrEtF5L}Government"));
            }

            if (settlement.OwnerClan != null)
            {
                var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(settlement.Owner);
                if (education.HasPerk(BKPerks.Instance.CivilManufacturer))
                {
                    result.Add(0.15f, BKPerks.Instance.CivilManufacturer.Name);
                }

                if (settlement.Owner.GetPerkValue(BKPerks.Instance.LordshipEconomicAdministration))
                {
                    result.Add(0.1f, BKPerks.Instance.LordshipEconomicAdministration.Name);
                }

                if (education.HasPerk(BKPerks.Instance.ArtisanEntrepeneur))
                {
                    result.Add(0.1f, BKPerks.Instance.ArtisanEntrepeneur.Name);
                }
            }

            var innovations = BannerKingsConfig.Instance.InnovationsManager.GetInnovationData(settlement.Culture);
            if (innovations != null)
            {
                if (innovations.HasFinishedInnovation(DefaultInnovations.Instance.Wheelbarrow))
                {
                    result.AddFactor(0.1f, DefaultInnovations.Instance.Wheelbarrow.Name);
                }

                if (innovations.HasFinishedInnovation(DefaultInnovations.Instance.BlastFurnace))
                {
                    result.AddFactor(0.2f, DefaultInnovations.Instance.BlastFurnace.Name);
                }

                if (innovations.HasFinishedInnovation(DefaultInnovations.Instance.Stirrups))
                {
                    result.AddFactor(0.06f, DefaultInnovations.Instance.Stirrups.Name);
                }

                if (innovations.HasFinishedInnovation(DefaultInnovations.Instance.Cogs))
                {
                    result.AddFactor(0.06f, DefaultInnovations.Instance.Cogs.Name);
                }

                if (innovations.HasFinishedInnovation(DefaultInnovations.Instance.Cranes))
                {
                    result.AddFactor(0.12f, DefaultInnovations.Instance.Cranes.Name);
                }
            }

            BannerKingsConfig.Instance.CourtManager.ApplyCouncilEffect(ref result, settlement.OwnerClan.Leader,
                DefaultCouncilPositions.Instance.Steward,
                DefaultCouncilTasks.Instance.OverseeProduction,
                .15f, true);

            if (settlement.Town != null)
            {
                SkillHelper.AddSkillBonusForTown(DefaultSkills.Crafting,
                        BKSkillEffects.Instance.ProductionEfficiency,
                        settlement.Town,
                        ref result);
            }

            return result;
        }

        public override ExplainedNumber CalculateProductionQuality(Settlement settlement)
        {
            var result = new ExplainedNumber(1f, true);
            result.LimitMin(0f);
            result.LimitMax(2f);

            result.Add((CalculateMercantilism(settlement).ResultNumber - 0.4f) * 0.5f, new TextObject("{=5eHCGMEK}Mercantilism"));

            var lordshipEconomicAdministration = BKPerks.Instance.LordshipEconomicAdministration;
            if (settlement.Owner.GetPerkValue(lordshipEconomicAdministration))
            {
                result.AddFactor(0.05f, lordshipEconomicAdministration.Name);
            }

            if (settlement.OwnerClan == null)
            {
                return result;
            }

            var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(settlement.Owner);
            if (education.Perks.Contains(BKPerks.Instance.CivilManufacturer))
            {
                result.Add(0.1f, BKPerks.Instance.CivilManufacturer.Name);
            }

            var government = BannerKingsConfig.Instance.TitleManager.GetSettlementGovernment(settlement);
            if (government.Equals(DefaultGovernments.Instance.Republic))
            {
                result.AddFactor(0.1f, new TextObject("{=PSrEtF5L}Government"));
            }

            BannerKingsConfig.Instance.CourtManager.ApplyCouncilEffect(ref result, settlement.OwnerClan.Leader,
                DefaultCouncilPositions.Instance.Steward,
                DefaultCouncilTasks.Instance.OverseeProduction,
                .085f, true);

            if (settlement.Town != null)
            {
                SkillHelper.AddSkillBonusForTown(DefaultSkills.Crafting,
                       BKSkillEffects.Instance.ProductionQuality,
                       settlement.Town,
                       ref result);
            }
           
            return result;
        }

        public override ExplainedNumber GetCaravanPrice(Settlement settlement, Hero buyer, bool isLarge = false)
        {
            var cost = new ExplainedNumber(isLarge ? 22500 : 15000, true);

            if (buyer.Culture.HasFeat(DefaultCulturalFeats.AseraiTraderFeat))
            {
                cost.AddFactor(-DefaultCulturalFeats.AseraiTraderFeat.EffectBonus,
                    DefaultCulturalFeats.AseraiTraderFeat.Name);
            }

            /*if (settlement == null)
            {
                settlement = Hero.OneToOneConversationHero.CurrentSettlement;
            }

            if (settlement != null)
            {
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                if (data != null)
                {
                    cost.AddFactor(1f - data.EconomicData.Mercantilism.ResultNumber, new TextObject("{=ciXU8Ews}Mecantilism"));
                    cost.AddFactor(data.EconomicData.CaravanAttraction.ResultNumber - 1f,
                        new TextObject("{=FK7QzVtM}Caravan attraction"));
                }
            }*/

            return cost;
        }

        public override ExplainedNumber CalculateTradePower(PopulationData data, bool descriptions = false)
        {
            ExplainedNumber result = new ExplainedNumber(1f, descriptions);
            result.LimitMin(0f);

            Settlement settlement = data.Settlement;
            result.Add(data.EconomicData.Mercantilism.ResultNumber / 2f, new TextObject("{=5eHCGMEK}Mercantilism"));
            result.AddFactor(data.MilitaryData.Militarism.ResultNumber * -1f, new TextObject("{=m66LFb9g}Militarism"));

            BannerKingsConfig.Instance.CourtManager.ApplyCouncilEffect(ref result, settlement.OwnerClan.Leader,
                DefaultCouncilPositions.Instance.Steward,
                DefaultCouncilTasks.Instance.DevelopEconomy,
                0.15f, false);

            foreach (var lane in DefaultShippingLanes.Instance.GetSettlementLanes(settlement))
            {
                float laneResult = 0f;
                foreach (Settlement port in lane.Ports)
                {
                    if (port.IsTown)
                    {
                        laneResult += 0.1f;
                    }
                }

                result.Add(laneResult, lane.Name);
            }

            if (settlement.Town != null)
            {
                if (settlement.Town.Gold < 50000)
                {
                    float factor = MathF.Clamp(-1f + (settlement.Town.Gold * 0.00001f), -0.8f, -0.2f);
                    result.AddFactor(factor, new TextObject("{=s2gxPA2Q}Market gold"));
                }
                else if (settlement.Town.Gold >= 1000000)
                {
                    float factor = MathF.Clamp(settlement.Town.Gold / 10000000f, 0.1f, 0.5f);
                    result.AddFactor(factor, new TextObject("{=s2gxPA2Q}Market gold"));
                }

                var capital = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKCapitalBehavior>().GetCapital(settlement.OwnerClan.Kingdom);
                if (capital == settlement.Town)
                {
                    result.AddFactor(0.4f, new TextObject("{=fQVyeiJb}Capital"));
                }

                Building building = settlement.Town.Buildings.FirstOrDefault(x => x.BuildingType.StringId == BKBuildings.Instance.Harbor.StringId ||
                                        x.BuildingType.StringId == BKBuildings.Instance.Port.StringId);
                if (building != null && building.CurrentLevel > 0)
                {
                    bool harbor = building.BuildingType.StringId == BKBuildings.Instance.Harbor.StringId;
                    result.AddFactor((harbor ? 0.12f : 0.7f) * building.CurrentLevel, building.Name);
                }

                BannerKingsConfig.Instance.CourtManager.ApplyCouncilEffect(ref result,
                   settlement.OwnerClan.Leader,
                   DefaultCouncilPositions.Instance.Constable,
                   DefaultCouncilTasks.Instance.EnforceLaw,
                   0.05f,
                   true);

                Hero governor = settlement.Town.Governor;
                if (governor != null)
                {
                    SkillHelper.AddSkillBonusForTown(DefaultSkills.Trade,
                       BKSkillEffects.Instance.TradePower,
                       settlement.Town,
                       ref result);
                }

                if (settlement.IsCastle)
                {
                    result.AddFactor(-0.25f, new TextObject("{=UPhMZ859}Castle"));
                }
            }

            return result;
        }

        public override ExplainedNumber CalculateTradePower(Settlement settlement, bool descriptions = false) =>
            CalculateTradePower(settlement.PopulationData(), descriptions);

        public override float GetDailyDemandForCategory(Town town, ItemCategory category, int extraProsperity)
        {
            var data = town.Settlement.PopulationData();
            if (data == null)
            {
                return base.GetDailyDemandForCategory(town, category, extraProsperity);
            }

            float nobles = data.GetTypeCount(PopType.Nobles);
            float craftsmen = data.GetTypeCount(PopType.Craftsmen);
            float serfs = data.GetTypeCount(PopType.Serfs);
            float tenants = data.GetTypeCount(PopType.Tenants);
            ConsumptionType type = Utils.Helpers.GetTradeGoodConsumptionType(category);

            float baseResult = 0f;
            switch (type)
            {
                case ConsumptionType.Luxury:
                    baseResult += nobles * 0.2f;
                    baseResult += craftsmen * 0.05f;
                    break;
                case ConsumptionType.Industrial:
                    baseResult += craftsmen * 0.06f;
                    baseResult += serfs * 0.01f;
                    baseResult += tenants * 0.02f;
                    break;
                default:
                    baseResult += nobles * 0.05f;
                    baseResult += craftsmen * 0.04f;
                    baseResult += serfs * 0.01f;
                    baseResult += tenants * 0.02f;
                    break;
            }

            float prosperity = town.Prosperity;
            float num = MathF.Max(0f, baseResult + (prosperity / 2) + extraProsperity);
            float num2 = MathF.Max(0f, baseResult + (prosperity));

            float baseDemand = category.BaseDemand;
            float num3 = baseDemand * num;
            float num4 = category.LuxuryDemand * num2;
            float result = num3 + num4;
            if (baseDemand < 1E-08f)
            {
                result = num * 0.01f;
            }

            return result;
        }

        public override float GetEstimatedDemandForCategory(Town town, ItemData itemData, ItemCategory category) =>
            GetDailyDemandForCategory(town, category, 1000);

        public override int GetTownGoldChange(Town town) => (int)GetMerchantIncome(town).ResultNumber;

        public override ExplainedNumber GetMerchantIncome(Town town, bool explanations = false)
        {
            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement);
            ExplainedNumber result = new ExplainedNumber(town.Prosperity / 1.5f, explanations);
            float slaves = data.GetTypeCount(PopType.Slaves);
            var privateSlaves = slaves * (1f - data.EconomicData.StateSlaves);
            var tax = 0.05f;
            if (BannerKingsConfig.Instance.PolicyManager.IsDecisionEnacted(town.Settlement, "decision_slaves_tax"))
            {
                var taxtype = (BannerKingsConfig.Instance.PolicyManager.GetPolicy(town.Settlement, "tax") as BKTaxPolicy)
                    .Policy;
                tax = taxtype switch
                {
                    TaxType.Standard => 0.7f,
                    TaxType.High => 0.65f,
                    _ => 0.85f
                };
            }

            result.AddFactor(data.EconomicData.ProductionEfficiency.ResultNumber, new TextObject("Production efficiency"));
            if (privateSlaves > 0f)
            {
                result.Add(privateSlaves * tax, new TextObject("{=yjbfHwog}Private slaves"));
            }

            if (town.IsCastle) result.AddFactor(-0.5f, new TextObject("{=kyB8tkgY}Castle"));
            return result;
        }

        public override int GetNotableCaravanLimit(Hero notable)
        {
            Occupation occupation = notable.Occupation;
            if (occupation == Occupation.Merchant) return 2;
            else if (occupation == Occupation.Artisan) return 1;

            return 0;
        }

        public override int GetSettlementMarketGoldLimit(Settlement settlememt)
        {
            int result = 0;
            if (settlememt.IsVillage)
            {
                result += 10000;
                Village village = settlememt.Village;
                result += (int)(village.Hearth * 3f);
            }
            else if (settlememt.Town != null)
            {
                result += 50000;
                Town town = settlememt.Town;
                if (settlememt.IsCastle) result += (int)(town.Prosperity * 12f);
                else result += (int)(town.Prosperity * 25f);
            }

            return result;
        }
    }
}