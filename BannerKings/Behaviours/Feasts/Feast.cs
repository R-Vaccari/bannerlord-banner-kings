using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings.Behaviours.Feasts
{
    public class Feast
    {
        public Feast(Hero host, List<Clan> guests, Town town, CampaignTime endDate)
        {
            Host = host;
            Guests = guests;
            Town = town;
            EndDate = endDate;
        }

        public Hero Host { get; private set; }
        public List<Clan> Guests { get; private set; }
        public Town Town { get; private set; }
        public CampaignTime EndDate { get; private set; }
    }
}
