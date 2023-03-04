using BannerKings.Utils.Extensions;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings.Extensions
{
    public static class ClanExtensions
    {
        public static List<Village> GetActualVillages(this Clan clan)
        {
            var list = new List<Village>();
            foreach (var member in clan.Lords)
            {
                list.AddRange(member.GetVillages());
            }

            return list;
        }
    }
}
