using BannerKings.Behaviours;
using BannerKings.Managers.Court;
using BannerKings.Managers.Court.Members;
using BannerKings.Managers.Court.Members.Tasks;
using BannerKings.Managers.Innovations;
using BannerKings.Managers.Policies;
using BannerKings.Managers.Skills;
using BannerKings.Managers.Titles;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static BannerKings.Managers.Policies.BKTaxPolicy;
using static BannerKings.Managers.Policies.BKWorkforcePolicy;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Models.Vanilla
{
    public class BKEconomyModel : DefaultSettlementEconomyModel, IEconomyModel
    {
        private static readonly float CRAFTSMEN_EFFECT_CAP = 0.4f;

        public ExplainedNumber CalculateEffect(Settlement settlement)
        {
            var result = new ExplainedNumber(0.1f, true);
            result.LimitMin(0f);
            result.LimitMax(1f);

            var title = BannerKingsConfig.Instance.TitleManager.GetSovereignFromSettlement(settlement);
            if (title != null)
            {
                var government = title.Contract.Government;
                switch (government)
                {
                    case GovernmentType.Republic:
                        result.Add(0.4f, new TextObject("Government"));
                        break;
                    case GovernmentType.Feudal:
                        result.Add(0.2f, new TextObject("Government"));
                        break;
                    case GovernmentType.Tribal:
                        result.Add(0.1f, new TextObject("Government"));
                        break;
                    case GovernmentType.Imperial:
                        result.Add(0.05f, new TextObject("Government"));
                        break;
                }
            }

            if (BannerKingsConfig.Instance.PolicyManager.IsDecisionEnacted(settlement, "decision_mercantilism"))
            {
                result.Add(0.1f, new TextObject("{=OdpuFusm}Encourage mercantilism decision"));
            }

            return result;
        }

        public ExplainedNumber CalculateProductionEfficiency(Settlement settlement)
        {
            var result = new ExplainedNumber(0, true);
            if (!settlement.IsVillage)
            {
                result.Add(new DefaultWorkshopModel().GetPolicyEffectToProduction(settlement.Town));
            }
            else
            {
                result.Add(0.7f);
            }

            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
            float craftsmen = data.GetTypeCount(PopType.Craftsmen);
            result.Add(MathF.Min(craftsmen / 250f * 0.020f, CRAFTSMEN_EFFECT_CAP), new TextObject("Craftsmen"));

            if (BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(settlement, "workforce",
                    (int) WorkforcePolicy.Martial_Law))
            {
                result.Add(-0.30f, new TextObject("{=7cFbhefJ}Martial Law policy"));
            }

            var mercantilism = data.EconomicData.Mercantilism.ResultNumber;
            result.Add(0.25f * mercantilism, new TextObject("Mercantilism"));

            var government = BannerKingsConfig.Instance.TitleManager.GetSettlementGovernment(settlement);
            if (government == GovernmentType.Feudal)
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
                    result.AddFactor(0.06f, DefaultInnovations.Instance.Wheelbarrow.Name);
                }

                if (innovations.HasFinishedInnovation(DefaultInnovations.Instance.BlastFurnace))
                {
                    result.AddFactor(0.15f, DefaultInnovations.Instance.BlastFurnace.Name);
                }

                if (innovations.HasFinishedInnovation(DefaultInnovations.Instance.Cranes))
                {
                    result.AddFactor(0.06f, DefaultInnovations.Instance.Cranes.Name);
                }
            }

            BannerKingsConfig.Instance.CourtManager.ApplyCouncilEffect(ref result, settlement.OwnerClan.Leader,
                DefaultCouncilPositions.Instance.Steward,
                DefaultCouncilTasks.Instance.OverseeProduction,
                .15f, true);

            return result;
        }

        public ExplainedNumber CalculateProductionQuality(Settlement settlement)
        {
            var result = new ExplainedNumber(1f, true);
            result.LimitMin(0f);
            result.LimitMax(2f);

            result.Add((CalculateEffect(settlement).ResultNumber - 0.4f) * 0.5f, new TextObject("{=5eHCGMEK}Mercantilism"));

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

            BannerKingsConfig.Instance.CourtManager.ApplyCouncilEffect(ref result, settlement.OwnerClan.Leader,
                DefaultCouncilPositions.Instance.Steward,
                DefaultCouncilTasks.Instance.OverseeProduction,
                .085f, true);

            return result;
        }

        public ExplainedNumber GetCaravanPrice(Settlement settlement, Hero buyer, bool isLarge = false)
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

        public ExplainedNumber CalculateCaravanAttraction(Settlement settlement)
        {
            var result = new ExplainedNumber(1f, true);

            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
            result.Add(data.EconomicData.Mercantilism.ResultNumber / 2f, new TextObject("{=5eHCGMEK}Mercantilism"));
            result.AddFactor(data.MilitaryData.Militarism.ResultNumber * -1f, new TextObject("{=m66LFb9g}Militarism"));

            BannerKingsConfig.Instance.CourtManager.ApplyCouncilEffect(ref result, settlement.OwnerClan.Leader,
                DefaultCouncilPositions.Instance.Steward,
                DefaultCouncilTasks.Instance.DevelopEconomy, 
                0.15f, false);

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
                
                var capital = Campaign.Current.GetCampaignBehavior<BKCapitalBehavior>().GetCapital(settlement.OwnerClan.Kingdom);
                if (capital == settlement.Town)
                {
                    result.AddFactor(0.4f, new TextObject("{=fQVyeiJb}Capital"));
                }

                BannerKingsConfig.Instance.CourtManager.ApplyCouncilEffect(ref result,
                   settlement.OwnerClan.Leader, 
                   DefaultCouncilPositions.Instance.Constable,
                   DefaultCouncilTasks.Instance.EnforceLaw,
                   0.05f, 
                   true);
            }

            return result;
        }

        public override float GetDailyDemandForCategory(Town town, ItemCategory category, int extraProsperity) => 
            GetCategoryDemand(town.Settlement, category, extraProsperity);

        public float GetCategoryDemand(Settlement settlement, ItemCategory category, int extraProsperity)
        {
            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
            if (data == null)
            {
                return base.GetDailyDemandForCategory(settlement.Town, category, extraProsperity);
            }

            float nobles = data.GetTypeCount(PopType.Nobles);
            float craftsmen = data.GetTypeCount(PopType.Craftsmen);
            float serfs = data.GetTypeCount(PopType.Serfs);
            var type = Utils.Helpers.GetTradeGoodConsumptionType(category);

            var baseResult = 0f;
            switch (type)
            {
                case ConsumptionType.Luxury:
                    baseResult += nobles * 8f;
                    baseResult += craftsmen * 1f;
                    break;
                case ConsumptionType.Industrial:
                    baseResult += craftsmen * 1.2f;
                    baseResult += serfs * 0.2f;
                    break;
                default:
                    baseResult += nobles * 1f;
                    baseResult += craftsmen * 1f;
                    baseResult += serfs * 0.20f;
                    break;
            }

            var prosperity = settlement.IsVillage ? settlement.Village.TradeBound.Prosperity : settlement.Town.Prosperity;
            var num = MathF.Max(0f, baseResult + (prosperity / 3) + extraProsperity);
            var num2 = MathF.Max(0f, baseResult + (prosperity / 2));

            if (!category.IsTradeGood && category.StringId != "noble_horse")
            {
                num *= 3f;
            }
            var num3 = category.BaseDemand * num;
            var num4 = category.LuxuryDemand * num2;
            var result = num3 + num4;
            if (category.BaseDemand < 1E-08f)
            {
                result = num * 0.01f;
            }        

            return result;
        }

        public override float GetEstimatedDemandForCategory(Town town, ItemData itemData, ItemCategory category)
        {
            return GetDailyDemandForCategory(town, category, 1000);
        }

        public override float GetDemandChangeFromValue(float purchaseValue)
        {
            var value = base.GetDemandChangeFromValue(purchaseValue);
            return value;
        }

        public override (float, float) GetSupplyDemandForCategory(Town town, ItemCategory category, float dailySupply,
            float dailyDemand, float oldSupply, float oldDemand)
        {
            var baseResult =
                base.GetSupplyDemandForCategory(town, category, dailySupply, dailyDemand, oldSupply, oldDemand);
            return baseResult;
        }

        public override int GetTownGoldChange(Town town)
        {
            if (BannerKingsConfig.Instance.PopulationManager != null &&
                BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(town.Settlement))
            {
                return GetMerchantIncome(town);
            }

            return base.GetTownGoldChange(town);
        }

        public int GetMerchantIncome(Town town)
        {
            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement);
            float slaves = data.GetTypeCount(PopType.Slaves);
            var privateSlaves = slaves * (1f - data.EconomicData.StateSlaves);
            var tax = 1f;
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

            var efficiency = data.EconomicData.ProductionEfficiency.ResultNumber * 1.25f;
            if (privateSlaves > 0f)
            {
                return (int) ((privateSlaves * tax * efficiency) + 5000 + (town.Prosperity / 2f));
            }

            return 0;
        }
    }
}