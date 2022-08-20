using System.Linq;
using BannerKings.Managers.Policies;
using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace BannerKings.Models.Vanilla
{
    public class BKWorkshopModel : DefaultWorkshopModel
    {
        public override int GetSellingCost(Workshop workshop)
        {
            return base.GetSellingCost(workshop);
        }

        public override int GetBuyingCostForPlayer(Workshop workshop)
        {
            float result = base.GetSellingCost(workshop) + 10000;

            if (workshop.Settlement != null)
            {
                var town = workshop.Settlement.Town;
                var costs = 0;
                var sellValue = 0;
                var items = Game.Current.ObjectManager.GetObjectTypeList<ItemObject>();

                foreach (var production in workshop.WorkshopType.Productions)
                {
                    foreach (var input in production.Inputs)
                    {
                        costs += GetCost(items, town, input.Item1, input.Item2);
                    }

                    var outputCost = 10000;
                    foreach (var output in production.Outputs)
                    {
                        var cost = GetCost(items, town, output.Item1, output.Item2);
                        if (cost < outputCost)
                        {
                            outputCost = cost;
                        }
                    }

                    sellValue += outputCost;
                }

                result += (int) ((sellValue - costs) * (float) CampaignTime.DaysInYear);
                result *= BannerKingsConfig.Instance.EconomyModel.CalculateProductionQuality(workshop.Settlement)
                    .ResultNumber;
            }

            if (workshop.Owner.OwnedWorkshops.Count == 1)
            {
                result *= 1.2f;
            }

            if (workshop.Owner.OwnedCommonAreas.Count == 0)
            {
                result *= 1.2f;
            }

            return (int) result;
        }

        private int GetCost(MBReadOnlyList<ItemObject> items, Town town, ItemCategory category, int quantity)
        {
            float cost = 0;
            var item = items.FirstOrDefault(x => x.ItemCategory == category);
            if (item != null)
            {
                cost += town.GetItemPrice(item) * (float) quantity;
            }

            return (int) cost;
        }

        public override float GetPolicyEffectToProduction(Town town)
        {
            if (BannerKingsConfig.Instance.PopulationManager != null &&
                BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(town.Settlement))
            {
                var model = (BKEconomyModel) BannerKingsConfig.Instance.Models.First(x =>
                    x.GetType() == typeof(BKEconomyModel));
                return model.CalculateProductionEfficiency(town.Settlement).ResultNumber;
            }

            return base.GetPolicyEffectToProduction(town);
        }

        public ExplainedNumber CalculateWorkshopTax(Settlement settlement, Hero payer)
        {
            var result = new ExplainedNumber();
            result.LimitMin(0f);
            result.LimitMax(0.5f);

            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
            var tax = (BKTaxPolicy) BannerKingsConfig.Instance.PolicyManager.GetPolicy(settlement, "tax");
            if (tax.Policy == BKTaxPolicy.TaxType.High)
            {
                result.Add(0.3f);
            }
            else if (tax.Policy == BKTaxPolicy.TaxType.High)
            {
                result.Add(0.2f);
            }
            else
            {
                result.Add(0.1f);
            }

            var payerEducation = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(payer);
            if (payerEducation.HasPerk(BKPerks.Instance.ArtisanEntrepeneur))
            {
                result.AddFactor(-0.2f, BKPerks.Instance.ArtisanEntrepeneur.Name);
            }

            if (settlement.OwnerClan != null)
            {
                var ownerEducation = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(settlement.Owner);
                if (ownerEducation.HasPerk(BKPerks.Instance.ArtisanEntrepeneur))
                {
                    result.AddFactor(0.2f, BKPerks.Instance.ArtisanEntrepeneur.Name);
                }
            }

            result.AddFactor(data.EconomicData.Mercantilism.ResultNumber * -0.5f);

            return result;
        }
    }
}