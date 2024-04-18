using BannerKings.Managers.Education.Lifestyles;
using BannerKings.Managers.Skills;
using BannerKings.Settings;
using BannerKings.Utils;
using Helpers;
using SandBox.GameComponents;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using static BannerKings.Utils.PerksHelpers;

namespace BannerKings.Models.Vanilla
{
    public class BKItemDiscardModel : DefaultItemDiscardModel
    {
        public override bool PlayerCanDonateItem(ItemObject item)
        {
            bool result = false;
            if (item.HasWeaponComponent)
            {
                if (MobileParty.MainParty != null && BannerKingsSettings.Instance.EnableUsefulPerks && BannerKingsSettings.Instance.EnableUsefulStewardPerks)
                {
                    result = (MobileParty.MainParty.EffectiveQuartermaster?.GetPerkValue(DefaultPerks.Steward.GivingHands) ?? false) || (MobileParty.MainParty.LeaderHero?.GetPerkValue(DefaultPerks.Steward.GivingHands) ?? false) || MobileParty.MainParty.GetAllPartyHeros().Any(d => d.GetPerkValue(DefaultPerks.Steward.GivingHands));
                }
                else
                {
                    result = MobileParty.MainParty.HasPerk(DefaultPerks.Steward.GivingHands);
                }
            }
            else if (item.HasArmorComponent)
            {
                if (MobileParty.MainParty != null && BannerKingsSettings.Instance.EnableUsefulPerks && BannerKingsSettings.Instance.EnableUsefulStewardPerks)
                {
                    result = (MobileParty.MainParty.EffectiveQuartermaster?.GetPerkValue(DefaultPerks.Steward.PaidInPromise) ?? false) || (MobileParty.MainParty.LeaderHero?.GetPerkValue(DefaultPerks.Steward.PaidInPromise) ?? false) || MobileParty.MainParty.GetAllPartyHeros().Any(d => d.GetPerkValue(DefaultPerks.Steward.PaidInPromise));
                }
                else
                {
                    result = MobileParty.MainParty.HasPerk(DefaultPerks.Steward.PaidInPromise, checkSecondaryRole: true);
                }
            }

            return result;
        }

        public int GetXpBonusForDiscardingItem(ItemRosterElement item)
        {
            if (item.EquipmentElement.Item != null && !item.IsEmpty && PlayerCanDonateItem(item.EquipmentElement.Item))
            {
                int num;
                switch (item.EquipmentElement.Item.Tier)
                {
                    case ItemObject.ItemTiers.Tier1:
                        num = 100;
                        break;
                    case ItemObject.ItemTiers.Tier2:
                        num = 150;
                        break;
                    case ItemObject.ItemTiers.Tier3:
                        num = 200;
                        break;
                    case ItemObject.ItemTiers.Tier4:
                        num = 250;
                        break;
                    case ItemObject.ItemTiers.Tier5:
                        num = 300;
                        break;
                    case ItemObject.ItemTiers.Tier6:
                        num = 350;
                        break;
                    default:
                        num = 35;
                        break;
                }
                if (item.EquipmentElement.ItemModifier!=null&& num >35)
                {
                    if ((int)item.EquipmentElement.ItemModifier?.ItemQuality == -1)
                    {
                        num = 0;
                    }
                    else
                    {
                        switch (item.EquipmentElement.ItemModifier?.ItemQuality)
                        {
                            case ItemQuality.Poor:
                                num = num / 10;
                                break;
                            case ItemQuality.Inferior:
                                num = num / 5;
                                break;
                            case ItemQuality.Common:
                                break;
                            case ItemQuality.Fine:
                                num = num * 2;
                                break;
                            case ItemQuality.Masterwork:
                                num = num * 4;
                                break;
                            case ItemQuality.Legendary:
                                num = num * 10;
                                break;
                            default:
                                break;
                        }
                    }
                }             
                #region DefaultPerks.Steward.PaidInPromise && DefaultPerks.Steward.GivingHands
                if (MobileParty.MainParty != null && BannerKingsSettings.Instance.EnableUsefulPerks && BannerKingsSettings.Instance.EnableUsefulStewardPerks)
                {
                    ExplainedNumber bonuses = new ExplainedNumber(num * item.Amount);
                    if (item.EquipmentElement.Item.HasArmorComponent)
                    {
                        PerksHelpers.AddScaledPerkBonus(DefaultPerks.Steward.PaidInPromise, ref bonuses, true, MobileParty.MainParty, DefaultSkills.Steward, 10, 10, 50, SkillScale.Both, minValue: 0, maxValue: 1.5f);
                    }
                    else if (item.EquipmentElement.Item.HasWeaponComponent)
                    {
                        PerksHelpers.AddScaledPerkBonus(DefaultPerks.Steward.GivingHands, ref bonuses, false, MobileParty.MainParty, DefaultSkills.Steward, 10, 10, 50, SkillScale.Both, minValue: 0, maxValue: 1.5f);
                    }
                    return (int)bonuses.ResultNumber;
                }
                else
                {
                    return num * item.Amount;
                }
                #endregion
            }

            return 0;
        }

        public override int GetXpBonusForDiscardingItems(ItemRoster itemRoster)
        {
            float num = 0f;
            foreach (var item in itemRoster)
            {
                num += GetXpBonusForDiscardingItem(item);
            }
            return MathF.Floor(num);
        }
    }
}