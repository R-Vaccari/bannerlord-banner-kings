using BannerKings.Behaviours.Workshops;
using BannerKings.Extensions;
using BannerKings.Managers.Policies;
using BannerKings.Managers.Skills;
using BannerKings.Settings;
using BannerKings.Utils;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Models.Vanilla
{
    public class BKWorkshopModel : DefaultWorkshopModel
    {
        public override int DefaultWorkshopCountInSettlement => 6;
        public override ExplainedNumber GetEffectiveConversionSpeedOfProduction(Workshop workshop, float speed, bool includeDescription)
        {
            ExplainedNumber result = new ExplainedNumber(speed, includeDescription, new TextObject("{=basevalue}Base", null));
            Settlement settlement = workshop.Settlement;
            if (settlement.OwnerClan.Kingdom != null)
            {
                if (settlement.OwnerClan.Kingdom.ActivePolicies.Contains(DefaultPolicies.ForgivenessOfDebts))
                {
                    result.AddFactor(-0.05f, DefaultPolicies.ForgivenessOfDebts.Name);
                }
                if (settlement.OwnerClan.Kingdom.ActivePolicies.Contains(DefaultPolicies.StateMonopolies))
                {
                    result.AddFactor(-0.1f, DefaultPolicies.StateMonopolies.Name);
                }
            }
            PerkHelper.AddPerkBonusForTown(DefaultPerks.Trade.MercenaryConnections, settlement.Town, ref result);

            #region DefaultPerks.Steward.Sweatshops
            if (BannerKingsSettings.Instance.EnableUsefulPerks && BannerKingsSettings.Instance.EnableUsefulStewardPerks)
            {
                DefaultPerks.Steward.Sweatshops.AddScaledClanLeaderPerkBonusWithClanAndFamilyMembers(ref result, false, workshop.Owner);
            }
            else
            {
                PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Steward.Sweatshops, workshop.Owner.CharacterObject, true, ref result);
            }
            #endregion
            return result;
        }
        public int GetUpgradeCost(Workshop workshop)
        {
            return GetCostForPlayer(workshop) / (4 + workshop.Level());
        }

        public ExplainedNumber GetProductionQuality(Workshop workshop, bool explanations = false)
        {
            var result = new ExplainedNumber(0f, explanations);
            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(workshop.Settlement);
            if (data == null)
            {
                return new ExplainedNumber(1f);
            }

            result.Add(data.EconomicData.ProductionQuality.ResultNumber, new TextObject("{=56S6FhCd}Production quality"));
            result.AddFactor(0.25f, workshop.WorkshopType.Name);
            if (workshop.Owner != null)
            {
                var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(workshop.Owner);
                if (education.HasPerk(BKPerks.Instance.ArtisanCraftsman))
                {
                    result.AddFactor(0.05f, BKPerks.Instance.ArtisanCraftsman.Name);
                }
            }

            if (workshop.Level() > 1)
            {
                result.AddFactor((workshop.Level() - 1) * 0.05f, GameTexts.FindText("str_level"));
            }

            return result;
        }

        public ExplainedNumber GetDailyExpense(Workshop workshop, bool includeDescriptions = false)
        {
            var result = new ExplainedNumber(0f, includeDescriptions);

            var labor = workshop.WorkshopType.StringId switch
            {
                "silversmithy" => 50f,
                "velvet_weavery" => 40f,
                "smithy" => 30f,
                "tannery" or "wool_weavery" or "linen_weavery" => 20f,
                _ => 12f
            };

            result.Add(labor, workshop.WorkshopType.Name);
            result.Add(workshop.Settlement.Town.Prosperity * 0.005f, GameTexts.FindText("str_prosperity"));

            if (workshop.Level() > 1)
            {
                result.AddFactor(workshop.Level() * 0.12f, GameTexts.FindText("str_level"));
            }

            return result;
        }

        public override int GetCostForNotable(Workshop workshop)
        {
            float result = base.GetCostForNotable(workshop);
            result *= BannerKingsConfig.Instance.EconomyModel.CalculateProductionQuality(workshop.Settlement)
                .ResultNumber;

            result += GetInventoryCost(workshop);

            return (int)result;
        }

        public override int GetCostForPlayer(Workshop workshop)
        {
            float result = base.GetCostForPlayer(workshop);
            result += (int)(GetDailyExpense(workshop).ResultNumber * 15f * CampaignTime.DaysInYear);

            if (workshop.Owner.OwnedWorkshops.Count == 1)
            {
                result *= 1.15f;
            }

            if (workshop.Owner.IsNotable && workshop.Owner.OwnedAlleys.Count == 0)
            {
                result *= 1.15f;
            }

            result *= 1f + (Hero.MainHero.OwnedWorkshops.Count * 0.05f);
            return (int) result;
        }

        public ExplainedNumber GetBuyingCostExplained(Workshop workshop, Hero buyer, bool descriptions = false)
        {
            ExplainedNumber result = new ExplainedNumber(base.GetCostForPlayer(workshop), descriptions,
                new TextObject("{=LiC18pJC}Equipment costs"));
            result.Add((int)(GetDailyExpense(workshop).ResultNumber * 15f * CampaignTime.DaysInYear),
                new TextObject("{=hwVR2ej5}Employee wages"));

            result.Add(GetInventoryCost(workshop), new TextObject("{=u5LQxWO5}Workshop inventory"));

            if (workshop.Owner.OwnedWorkshops.Count == 1)
            {
                result.AddFactor(0.15f, new TextObject("{=jiLxCXBW}{OWNER}'s only workshop")
                    .SetTextVariable("OWNER", workshop.Owner.Name));
            }

            if (workshop.Owner.IsNotable && workshop.Owner.OwnedAlleys.Count == 0 && workshop.Owner.OwnedCaravans.Count == 0)
            {
                result.AddFactor(0.15f, new TextObject("{=uNzd25jE}{OWNER} has no other incomes")
                    .SetTextVariable("OWNER", workshop.Owner.Name));
            }

            result.AddFactor(buyer.OwnedWorkshops.Count * 0.05f, new TextObject("{=jhWhrK2B}{BUYER} has {COUNT} workshop(s)")
                .SetTextVariable("BUYER", buyer.Name)
                .SetTextVariable("COUNT", buyer.OwnedWorkshops.Count));
            return result;
        }

        public int GetInventoryCost(Workshop workshop)
        {
            int cost = 0;
            WorkshopData data = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKWorkshopBehavior>().GetInventory(workshop);
            if (data != null)
            {
                foreach (var element in data.Inventory)
                {
                    int price = workshop.Settlement.Town.GetItemPrice(element.EquipmentElement, null, true);
                    cost += (int)(price * (float)element.Amount);
                }
            }

            return cost;
        }

        public ExplainedNumber GetProductionEfficiency(Workshop workshop, bool explanations = false)
        {
            var result = new ExplainedNumber(0f, explanations);
            result.Add(BannerKingsConfig.Instance.EconomyModel.CalculateProductionEfficiency(workshop.Settlement, false, workshop.Settlement.PopulationData()).ResultNumber,
                new TextObject("{=2fCjZALt}Local production efficiency"));

            if (workshop.Level() > 1)
            {
                result.AddFactor((workshop.Level() - 1) * 0.08f, GameTexts.FindText("str_level"));
            }

            return result;
        }

        public ExplainedNumber CalculateWorkshopTax(Settlement settlement, Hero payer)
        {
            var result = new ExplainedNumber();
            result.LimitMin(0f);
            result.LimitMax(0.5f);

            var tax = (BKTaxPolicy) BannerKingsConfig.Instance.PolicyManager.GetPolicy(settlement, "tax");
            if (tax.Policy == BKTaxPolicy.TaxType.High)
            {
                result.Add(0.3f);
            }
            else if (tax.Policy == BKTaxPolicy.TaxType.Standard)
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

            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
            if (data != null)
            {
                result.AddFactor(data.EconomicData.Mercantilism.ResultNumber * -0.5f);
            }

            return result;
        }
    }
}