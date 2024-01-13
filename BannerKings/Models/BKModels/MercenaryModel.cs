using BannerKings.Campaign.Economy.Markets;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace BannerKings.Models.BKModels
{
    public class MercenaryModel : IMercenaryModel
    {
        public override IEnumerable<ItemCategory> GetLevyCategories()
        {
            yield return DefaultItemCategories.Garment;
            yield return DefaultItemCategories.LightArmor;
            yield return DefaultItemCategories.MeleeWeapons2;
            yield return DefaultItemCategories.RangedWeapons2;
            yield return DefaultItemCategories.Horse;
            yield return DefaultItemCategories.Shield2;
        }

        public override IEnumerable<ItemCategory> GetProCategories()
        {
            yield return DefaultItemCategories.MediumArmor;
            yield return DefaultItemCategories.LightArmor;
            yield return DefaultItemCategories.MeleeWeapons3;
            yield return DefaultItemCategories.RangedWeapons3;
            yield return DefaultItemCategories.Horse;
            yield return DefaultItemCategories.Shield3;
        }

        public override bool IsEquipmentAdequate(ItemObject item, CharacterObject troop, bool levy)
        {
            MarketGroup group = DefaultMarketGroups.Instance.GetMarket(troop.Culture);
            if (item.IsCraftedByPlayer)
            {
                return false;
            }

            if (group != null && group.GetSpawn((CultureObject)item.Culture) <= 0f)
            {
                return false;
            }
            else if (item.Culture != troop.Culture)
            {
                return false;
            }

            if (levy)
            {
                if (GetLevyCategories().Contains(item.ItemCategory))
                {
                    return true;
                }
            }
            else
            {
                if (GetProCategories().Contains(item.ItemCategory)) 
                {
                    return true;
                }
            }

            return false;
        }
    }
}
