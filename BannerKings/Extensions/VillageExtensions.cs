using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings.Extensions
{
    public static class VillageExtensions
    {
        public static Hero GetActualOwner(this Village village)
        {
            var owner = village.Settlement.Owner;
            var title = BannerKingsConfig.Instance.TitleManager.GetTitle(village.Settlement);
            if (title != null && title.deJure != null)
            {
                if (village.Settlement.MapFaction == title.deJure.MapFaction)
                {
                    owner = title.deJure;
                }
            }

            return owner;
        }

        public static bool IsMiningVillage(this Village village)
        {
            var type = village.VillageType;
            return type == DefaultVillageTypes.SilverMine || type == DefaultVillageTypes.IronMine ||
                type == DefaultVillageTypes.SaltMine || type == DefaultVillageTypes.ClayMine;
        }

        public static bool IsFarmingVillage(this Village village)
        {
            var type = village.VillageType;
            return type == DefaultVillageTypes.WheatFarm || type == DefaultVillageTypes.DateFarm ||
                type == DefaultVillageTypes.FlaxPlant || type == DefaultVillageTypes.SilkPlant || 
                type == DefaultVillageTypes.OliveTrees || type == DefaultVillageTypes.VineYard;
        }

        public static bool IsAnimalVillage(this Village village)
        {
            var type = village.VillageType;
            return type == DefaultVillageTypes.CattleRange || type == DefaultVillageTypes.HogFarm ||
                     type == DefaultVillageTypes.SheepFarm || type == DefaultVillageTypes.BattanianHorseRanch || 
                     type == DefaultVillageTypes.DesertHorseRanch || type == DefaultVillageTypes.EuropeHorseRanch || 
                     type == DefaultVillageTypes.SteppeHorseRanch || type == DefaultVillageTypes.SturgianHorseRanch ||
                     type == DefaultVillageTypes.VlandianHorseRanch;
        }

        public static bool IsRanchVillage(this Village village)
        {
            var type = village.VillageType;
            return type == DefaultVillageTypes.BattanianHorseRanch ||
                     type == DefaultVillageTypes.DesertHorseRanch || type == DefaultVillageTypes.EuropeHorseRanch ||
                     type == DefaultVillageTypes.SteppeHorseRanch || type == DefaultVillageTypes.SturgianHorseRanch ||
                     type == DefaultVillageTypes.VlandianHorseRanch;
        }
    }
}
