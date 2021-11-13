

using System.Collections.Generic;
using TaleWorlds.CampaignSystem;

namespace Populations.Helpers
{
    public static class Helpers
    {
        public static int GetPrisionerCount(TroopRoster roster)
        {
            List<TroopRosterElement> rosters = roster.GetTroopRoster();
            int count = 0;
            rosters.ForEach(rosterElement =>
            {
                if (!rosterElement.Character.IsHero)
                    count += rosterElement.Number + rosterElement.WoundedNumber;
            });
            return count;
        }
       
    }
}
