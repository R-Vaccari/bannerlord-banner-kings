using BannerKings.Behaviours.Mercenary;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace BannerKings.Models.BKModels.Abstract
{
    public abstract class MercenaryModel
    {
        public abstract ExplainedNumber GetDailyCareerPointsGain(Clan clan, MercenaryCareer career, bool explanations = false);
        public abstract bool IsEquipmentAdequate(ItemObject item, CharacterObject troop, bool levy);
        public abstract IEnumerable<ItemCategory> GetLevyCategories();
        public abstract IEnumerable<ItemCategory> GetProCategories();
        public abstract IEnumerable<ItemCategory> GetNonCulturalCategories();
    }
}
