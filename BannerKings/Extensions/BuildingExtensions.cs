using BannerKings.Managers.Buildings;
using System.Linq;
using TaleWorlds.CampaignSystem.Settlements.Buildings;

namespace BannerKings.Extensions
{
    public static class BuildingExtensions
    {
        public static bool IsBuildingSuitable(this Building building)
        {
            var type = building.BuildingType;
            if (type == BKBuildings.Instance.WarhorseStuds)
            {
                return building.Town.Villages.Any(x => x.IsRanchVillage());
            }

            return true;
        }
    }
}
