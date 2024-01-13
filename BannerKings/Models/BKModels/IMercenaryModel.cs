using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace BannerKings.Models.BKModels
{
    public abstract class IMercenaryModel
    {
        public abstract bool IsEquipmentAdequate(ItemObject item, CharacterObject troop, bool levy);
        public abstract IEnumerable<ItemCategory> GetLevyCategories();
        public abstract IEnumerable<ItemCategory> GetProCategories();
    }
}
