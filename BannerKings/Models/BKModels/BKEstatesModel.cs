using BannerKings.Managers.Policies;
using BannerKings.Managers.Populations;
using BannerKings.Managers.Populations.Estates;
using BannerKings.Managers.Populations.Villages;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace BannerKings.Models.BKModels
{
    public class BKEstatesModel
    {
        public int MinimumEstateAcreage => 120;

        public float MaximumEstateAcreagePercentage => 0.12f;


        public ExplainedNumber CalculateEstatesMaximum(Settlement settlement, bool explanations = false)
        {
            var result = new ExplainedNumber(0f, explanations);
            if (settlement.IsVillage)
            {
                var landOwners = settlement.Notables.Count(x => x.Occupation == Occupation.RuralNotable);
                result.Add(landOwners);
                result.Add(1);
            }

            return result;
        }

        public ExplainedNumber CalculateEstateIncome(Estate estate, bool explanations = false)
        {
            var result = new ExplainedNumber(0f, explanations);

            var settlement = estate.EstatesData.Settlement;
            if (settlement.IsVillage)
            {
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(estate.EstatesData.Settlement);

                float proportion = GetEstateWorkforceProportion(estate, data);
                result.Add(settlement.Village.TradeTaxAccumulated * proportion, new TextObject("{=!}Production contribution"));

                float taxOffice = data.VillageData.GetBuildingLevel(DefaultVillageBuildings.Instance.TaxOffice);
                if (taxOffice > 0)
                {
                    var taxType = ((BKTaxPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(settlement, "tax")).Policy;
                    BannerKingsConfig.Instance.TaxModel.AddVillagePopulationTaxes(ref result, estate.Nobles, estate.Craftsmen, 
                        taxOffice, taxType);
                }
            }

            return result;
        }

        public float GetEstateWorkforceProportion(Estate estate, PopulationData data)
        {
            float serfs = data.GetTypeCount(Managers.PopulationManager.PopType.Serfs);
            float slaves = data.GetTypeCount(Managers.PopulationManager.PopType.Slaves);

            return (estate.Serfs + estate.Slaves) / (serfs + slaves);
        }
    }
}
