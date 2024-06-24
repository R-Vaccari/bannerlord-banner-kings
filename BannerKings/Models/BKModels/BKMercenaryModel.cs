using BannerKings.CampaignContent.Economy.Markets;
using BannerKings.Models.BKModels.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace BannerKings.Models.BKModels
{
    public class BKMercenaryModel : MercenaryModel
    {
        public override IEnumerable<ItemCategory> GetLevyCategories()
        {
            yield return DefaultItemCategories.Garment;
            yield return DefaultItemCategories.LightArmor;
            yield return DefaultItemCategories.MeleeWeapons2;
            yield return DefaultItemCategories.RangedWeapons2;
            yield return DefaultItemCategories.Horse;
            yield return DefaultItemCategories.Shield2;
            yield return DefaultItemCategories.HorseEquipment;
            yield return DefaultItemCategories.HorseEquipment2;
            yield return DefaultItemCategories.Arrows;
        }

        public override IEnumerable<ItemCategory> GetNonCulturalCategories()
        {
            yield return DefaultItemCategories.Arrows;
            yield return DefaultItemCategories.RangedWeapons1;
            yield return DefaultItemCategories.RangedWeapons2;
            yield return DefaultItemCategories.RangedWeapons3;
            yield return DefaultItemCategories.RangedWeapons4;
            yield return DefaultItemCategories.RangedWeapons5;
        }

        public override IEnumerable<ItemCategory> GetProCategories()
        {
            yield return DefaultItemCategories.MediumArmor;
            yield return DefaultItemCategories.LightArmor;
            yield return DefaultItemCategories.Garment;
            yield return DefaultItemCategories.MeleeWeapons3;
            yield return DefaultItemCategories.RangedWeapons3;
            yield return DefaultItemCategories.MeleeWeapons4;
            yield return DefaultItemCategories.RangedWeapons4;
            yield return DefaultItemCategories.Horse;
            yield return DefaultItemCategories.WarHorse;
            yield return DefaultItemCategories.Shield3;
            yield return DefaultItemCategories.Shield4;
            yield return DefaultItemCategories.HorseEquipment2;
            yield return DefaultItemCategories.HorseEquipment3;
            yield return DefaultItemCategories.HorseEquipment4;
            yield return DefaultItemCategories.Arrows;
        }

        public override bool IsEquipmentAdequate(ItemObject item, CharacterObject troop, bool levy)
        {
            MarketGroup group = DefaultMarketGroups.Instance.GetMarket(troop.Culture);
            if (item.IsCraftedByPlayer) return false;

            if (item.Culture == null) return true;

            bool culture = false;
            bool adequate = false;
            if (GetNonCulturalCategories().Contains(item.ItemCategory)) culture = true; 
            else
            {
                if (item.Culture == troop.Culture) culture = true;
                else if (group != null && group.GetSpawn((CultureObject)item.Culture) > 0f) culture = true; 
            }
            
            if (levy && GetLevyCategories().Contains(item.ItemCategory)) adequate = true;
            else if (!levy && GetProCategories().Contains(item.ItemCategory)) adequate = true;

            return adequate && culture;
        }
    }
}
