using TaleWorlds.Core;

namespace BannerKings.Extensions
{
    public static class ItemExtensions
    {

        public static bool IsMineral(this ItemObject item)
        {
            return item.StringId is "clay" or "iron" or "salt" or "silver" or "goldore";
        }
    }
}
