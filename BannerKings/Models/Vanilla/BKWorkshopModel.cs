using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using System.Linq;
using BannerKings.Populations;
using BannerKings.Managers.Policies;
using TaleWorlds.Core;
using TaleWorlds.Library;
using BannerKings.Managers.Education;
using BannerKings.Managers.Skills;

namespace BannerKings.Models
{
    public class BKWorkshopModel : DefaultWorkshopModel
    {
        public override int GetSellingCost(Workshop workshop)
        {


            return (int)(GetBuyingCostForPlayer(workshop) * 0.5f);
        }

        public override int GetBuyingCostForPlayer(Workshop workshop)
        {
            float result = base.GetSellingCost(workshop) + 10000;

            if (workshop.Settlement != null)
            {
                Town town = workshop.Settlement.Town;
                int costs = 0;
                int sellValue = 0;
                MBReadOnlyList<ItemObject> items = Game.Current.ObjectManager.GetObjectTypeList<ItemObject>();

                foreach (WorkshopType.Production production in workshop.WorkshopType.Productions)
                {
                    foreach ((ItemCategory, int) input in production.Inputs)
                        costs += GetCost(items, town, input.Item1, input.Item2);

                    foreach ((ItemCategory, int) output in production.Outputs)
                        sellValue += GetCost(items, town, output.Item1, output.Item2);
                }

                result += (int)((sellValue - costs) * (float)CampaignTime.DaysInYear);
                result *= BannerKingsConfig.Instance.EconomyModel.CalculateProductionQuality(workshop.Settlement).ResultNumber;
                
            }

            if (workshop.Owner.OwnedWorkshops.Count == 1) result *= 1.2f;
            if (workshop.Owner.OwnedCommonAreas.Count == 0) result *= 1.2f;

            return (int)result;
        }

        private int GetCost(MBReadOnlyList<ItemObject> items, Town town, ItemCategory category, int quantity)
        {
            float cost = 0;
            ItemObject item = items.FirstOrDefault(x => x.ItemCategory == category);
            if (item != null) cost += town.GetItemPrice(item) * (float)quantity;

            return (int)cost;
        }

        public override float GetPolicyEffectToProduction(Town town)
        {
            if (BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(town.Settlement))
            {
                BKEconomyModel model = (BKEconomyModel)BannerKingsConfig.Instance.Models.First(x => x.GetType() == typeof(BKEconomyModel));
                return model.CalculateProductionEfficiency(town.Settlement).ResultNumber;
            } else return base.GetPolicyEffectToProduction(town); 
           
        }

        public ExplainedNumber CalculateWorkshopTax(Settlement settlement, Hero payer)
        {
            ExplainedNumber result = new ExplainedNumber();
            result.LimitMin(0f);
            result.LimitMax(0.5f);

            PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
            BKTaxPolicy tax = (BKTaxPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(settlement, "tax");
            if (tax.Policy == BKTaxPolicy.TaxType.High) result.Add(0.3f);
            else if (tax.Policy == BKTaxPolicy.TaxType.High) result.Add(0.2f);
            else result.Add(0.1f);

            EducationData payerEducation = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(payer);
            if (payerEducation.HasPerk(BKPerks.Instance.CaravaneerEntrepeneur))
                result.AddFactor(-0.2f, BKPerks.Instance.CaravaneerEntrepeneur.Name);

            if (settlement.OwnerClan != null)
            {
                EducationData ownerEducation = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(settlement.Owner);
                if (ownerEducation.HasPerk(BKPerks.Instance.CaravaneerEntrepeneur))
                    result.AddFactor(0.2f, BKPerks.Instance.CaravaneerEntrepeneur.Name);
            }

            result.AddFactor(data.EconomicData.Mercantilism.ResultNumber * -0.5f);

            return result;
        }
    }
}
