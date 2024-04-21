using BannerKings.Managers.Education.Lifestyles;
using BannerKings.Managers.Items;
using BannerKings.Managers.Skills;
using BannerKings.Settings;
using BannerKings.Utils;
using BannerKings.Utils.Models;
using Helpers;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace BannerKings.Models.Vanilla
{
    public class BKPriceFactorModel : DefaultTradeItemPriceFactorModel
    {
        public  float GetTradePenaltyInternal(ItemObject item, MobileParty clientParty, PartyBase merchant, bool isSelling, float inStore, float supply, float demand)
        {
            Settlement settlement = merchant?.Settlement;
            float num = 0.06f;
            bool flag = clientParty?.IsCaravan ?? false;
            bool num2 = merchant != null && merchant.MobileParty?.IsCaravan == true;
            if (clientParty != null && merchant != null && clientParty.MapFaction.IsAtWarWith(merchant.MapFaction))
            {
                num += 0.5f;
            }

            if (!item.IsTradeGood && !item.IsAnimal && !item.HasHorseComponent && !flag && isSelling)
            {
                ExplainedNumber explainedNumber = new ExplainedNumber(1.5f + Math.Max(0f, item.Tierf - 1f) * 0.25f);
                if (item.IsCraftedWeapon && item.IsCraftedByPlayer && clientParty != null && clientParty.HasPerk(DefaultPerks.Crafting.ArtisanSmith))
                {
                    explainedNumber.AddFactor(DefaultPerks.Crafting.ArtisanSmith.PrimaryBonus);
                }

                num += explainedNumber.ResultNumber;
            }

            if (item.HasHorseComponent && item.HorseComponent.IsPackAnimal && !flag && isSelling)
            {
                num += 0.8f;
            }

            if (item.HasHorseComponent && item.HorseComponent.IsMount && !flag && isSelling)
            {
                num += 0.8f;
            }

            if (settlement != null && settlement.IsVillage)
            {
                num += (isSelling ? 1f : 0.1f);
            }

            if (num2)
            {
                if (item.ItemCategory == DefaultItemCategories.PackAnimal && !isSelling)
                {
                    num += 2f;
                }

                num += (isSelling ? 1f : 0.1f);
            }

            bool flag2 = clientParty == null;
            if (flag)
            {
                num *= 0.5f;
            }
            else if (flag2)
            {
                num *= 0.2f;
            }

            float num3 = ((clientParty != null) ? TaleWorlds.CampaignSystem.Campaign.Current.Models.PartyTradeModel.GetTradePenaltyFactor(clientParty) : 1f);
            num *= num3;
            ExplainedNumber stat = new ExplainedNumber(num);
            if (clientParty != null)
            {
                if (settlement != null && clientParty.MapFaction == settlement.MapFaction)
                {
                    if (settlement.IsVillage)
                    {
                        PerkHelper.AddPerkBonusForParty(DefaultPerks.Scouting.VillageNetwork, clientParty, isPrimaryBonus: true, ref stat);
                    }
                    else if (settlement.IsTown)
                    {
                        PerkHelper.AddPerkBonusForParty(DefaultPerks.Scouting.RumourNetwork, clientParty, isPrimaryBonus: true, ref stat);
                    }
                }

                if (item.IsTradeGood)
                {
                    if (clientParty.HasPerk(DefaultPerks.Trade.WholeSeller) && isSelling)
                    {
                        PerkHelper.AddPerkBonusForParty(DefaultPerks.Trade.WholeSeller, clientParty, isPrimaryBonus: true, ref stat);
                    }

                    if (isSelling && item.IsFood && clientParty.LeaderHero != null)
                    {
                        PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Trade.GranaryAccountant, clientParty.LeaderHero.CharacterObject, isPrimaryBonus: true, ref stat);
                    }
                }
                else if (!item.IsTradeGood && clientParty.HasPerk(DefaultPerks.Trade.Appraiser) && isSelling)
                {
                    PerkHelper.AddPerkBonusForParty(DefaultPerks.Trade.Appraiser, clientParty, isPrimaryBonus: true, ref stat);
                }

                if (PartyBaseHelper.HasFeat(clientParty.Party, DefaultCulturalFeats.AseraiTraderFeat))
                {
                    stat.AddFactor(-0.1f);
                }

                if (item.WeaponComponent != null && isSelling)
                {
                    PerkHelper.AddPerkBonusForParty(DefaultPerks.Roguery.ArmsDealer, clientParty, isPrimaryBonus: true, ref stat);
                }

                if (!isSelling && item.IsFood)
                {
                    PerkHelper.AddPerkBonusForParty(DefaultPerks.Trade.InsurancePlans, clientParty, isPrimaryBonus: false, ref stat);
                }

                #region DefaultPerks.Steward.ArenicosMules
                if (BannerKingsSettings.Instance.EnableUsefulPerks && BannerKingsSettings.Instance.EnableUsefulStewardPerks)
                {
                    if (item.HorseComponent != null && item.HorseComponent.IsPackAnimal)
                    {
                        DefaultPerks.Steward.ArenicosMules.AddScaledPerkBonus(ref stat, true, clientParty);
                    }
                }
                else
                {
                    if (item.HorseComponent != null && item.HorseComponent.IsPackAnimal && clientParty.HasPerk(DefaultPerks.Steward.ArenicosMules, checkSecondaryRole: true))
                    {
                        PerkHelper.AddPerkBonusForParty(DefaultPerks.Steward.ArenicosMules, clientParty, isPrimaryBonus: false, ref stat);
                    }
                }
                #endregion


                if (item.IsMountable)
                {
                    if (clientParty.HasPerk(DefaultPerks.Riding.DeeperSacks, checkSecondaryRole: true))
                    {
                        stat.AddFactor(DefaultPerks.Riding.DeeperSacks.SecondaryBonus, DefaultPerks.Riding.DeeperSacks.Name);
                    }

                    #region DefaultPerks.Steward.ArenicosHorses
                    if (BannerKingsSettings.Instance.EnableUsefulPerks && BannerKingsSettings.Instance.EnableUsefulStewardPerks)
                    {
                        DefaultPerks.Steward.ArenicosHorses.AddScaledPerkBonus(ref stat, true, clientParty);
                    }
                    else
                    {
                        if (clientParty.EffectiveQuartermaster != null && clientParty.EffectiveQuartermaster.GetPerkValue(DefaultPerks.Steward.ArenicosHorses))
                        {
                            PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Steward.ArenicosHorses, clientParty.LeaderHero.CharacterObject, isPrimaryBonus: false, ref stat);
                        }
                    }
                    #endregion

                }

                if (clientParty.IsMainParty && Hero.MainHero.GetPerkValue(DefaultPerks.Roguery.SmugglerConnections) && merchant?.MapFaction != null && merchant.MapFaction.MainHeroCrimeRating > 0f)
                {
                    PerkHelper.AddPerkBonusForParty(DefaultPerks.Roguery.SmugglerConnections, clientParty, isPrimaryBonus: false, ref stat);
                }

                if (!isSelling && merchant != null && merchant.IsSettlement && merchant.Settlement.IsVillage && clientParty.HasPerk(DefaultPerks.Trade.DistributedGoods, checkSecondaryRole: true))
                {
                    PerkHelper.AddPerkBonusForParty(DefaultPerks.Trade.DistributedGoods, clientParty, isPrimaryBonus: false, ref stat);
                }

                if (isSelling && item.HasHorseComponent && clientParty.HasPerk(DefaultPerks.Trade.LocalConnection, checkSecondaryRole: true))
                {
                    stat.AddFactor(DefaultPerks.Trade.LocalConnection.SecondaryBonus, DefaultPerks.Trade.LocalConnection.Name);
                }

                if (isSelling && (item.ItemCategory == DefaultItemCategories.Pottery || item.ItemCategory == DefaultItemCategories.Tools || item.ItemCategory == DefaultItemCategories.Jewelry || item.ItemCategory == DefaultItemCategories.Cotton) && clientParty.LeaderHero != null)
                {
                    PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Trade.TradeyardForeman, clientParty.LeaderHero.CharacterObject, isPrimaryBonus: true, ref stat);
                }

                if (!isSelling && (item.ItemCategory == DefaultItemCategories.Clay || item.ItemCategory == DefaultItemCategories.Iron || item.ItemCategory == DefaultItemCategories.Silver || item.ItemCategory == DefaultItemCategories.Cotton) && clientParty.HasPerk(DefaultPerks.Trade.RapidDevelopment))
                {
                    PerkHelper.AddPerkBonusForParty(DefaultPerks.Trade.RapidDevelopment, clientParty, isPrimaryBonus: false, ref stat);
                }
            }

            return stat.ResultNumber;
        }

        public override float GetTradePenalty(ItemObject item, MobileParty clientParty, PartyBase merchant, bool isSelling, float inStore, float supply, float demand)
        {

            var result = GetTradePenaltyInternal(item, clientParty, merchant, isSelling, inStore, supply, demand);

            if (clientParty != null && clientParty.LeaderHero != null)
            {
                var leader = clientParty.LeaderHero;
                var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(leader);
                if (education.Lifestyle != null && education.Lifestyle.Equals(DefaultLifestyles.Instance.Gladiator))
                {
                    result *= 0.8f;
                }
            }

            if (clientParty != null && clientParty.IsCaravan)
            {
                var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(clientParty.Owner);
                if (education.HasPerk(BKPerks.Instance.CaravaneerOutsideConnections))
                {
                    result *= 0.95f;
                }
            }

            Settlement settlement = merchant?.Settlement;
            if (settlement != null && settlement.IsCastle) result *= 3f;

            if (item.HasWeaponComponent || item.HasArmorComponent || item.HasSaddleComponent) result *= 5f;


            return result;
        }

        public override int GetPrice(EquipmentElement itemRosterElement, MobileParty clientParty, PartyBase merchant, bool isSelling, float inStoreValue, float supply, float demand)
        {
            if (merchant != null && merchant.Settlement != null)
            {
                Settlement settlement = merchant.Settlement;
                if (settlement.Town != null && settlement.Town.CurrentBuilding != null)
                {
                    ItemCategory category = itemRosterElement.Item.GetItemCategory();
                    if (category == DefaultItemCategories.Wood || category == DefaultItemCategories.Clay || category == BKItemCategories.Instance.Limestone ||
                        category == DefaultItemCategories.Iron || category == BKItemCategories.Instance.Marble)
                    {
                        foreach (var requirement in BannerKingsConfig.Instance.ConstructionModel.GetMaterialRequirements(settlement.Town.CurrentBuilding))
                        {
                            if (requirement.Item1.ItemCategory == category)
                            {
                                demand += requirement.Item2;
                            }
                        }
                    }
                }

                if (settlement.IsVillage)
                {
                    inStoreValue += merchant.ItemRoster.GetItemNumber(itemRosterElement.Item) * 20f;
                }

                if (settlement.IsCastle)
                {
                    inStoreValue += merchant.ItemRoster.GetItemNumber(itemRosterElement.Item) * 10f;
                }
            }

            int price = base.GetPrice(itemRosterElement, clientParty, merchant, isSelling, inStoreValue, supply, demand);
            ItemObject item = itemRosterElement.Item;
            if (item.HasHorseComponent)
            {
                if (item.HorseComponent.MeatCount > 0)
                {
                    ItemObject meat = DefaultItems.Meat;
                    price += (int)(meat.Value * 0.5f * GetPriceFactor(meat, clientParty, merchant, inStoreValue, supply, demand, isSelling));
                }

                if (item.HorseComponent.HideCount > 0)
                {
                    ItemObject hides = DefaultItems.Hides;
                    price += (int)(hides.Value * 0.5f * GetPriceFactor(hides, clientParty, merchant, inStoreValue, supply, demand, isSelling));
                }
            }

            return price;
        }

        private float GetPriceFactor(ItemObject item, MobileParty tradingParty, PartyBase merchant, float inStoreValue, float supply, float demand, bool isSelling)
        {
            float basePriceFactor = GetBasePriceFactor(item.GetItemCategory(), inStoreValue, supply, demand, isSelling, item.Value);
            float tradePenalty = GetTradePenalty(item, tradingParty, merchant, isSelling, inStoreValue, supply, demand);
            if (!isSelling)
            {
                return basePriceFactor * (1f + tradePenalty);
            }

            return basePriceFactor * 1f / (1f + tradePenalty);
        }

        public override float GetBasePriceFactor(ItemCategory itemCategory, float inStoreValue, float supply, float demand,
            bool isSelling, int transferValue)
        {
            if (isSelling)
            {
                inStoreValue += (float)transferValue;
            }

            float value = MathF.Pow(demand / (0.1f * supply + inStoreValue * 0.04f + 2f), itemCategory.IsAnimal ? 0.3f : 0.6f);
            if (itemCategory.Properties == ItemCategory.Property.BonusToFoodStores)
            {
                return MathF.Clamp(value, 0.3f, 3f);
            }

            if (itemCategory.IsTradeGood)
            {
                return MathF.Clamp(value, 0.3f, 10f);
            }

            return MathF.Clamp(value, 0.7f, 1.3f);
        }
    }
}