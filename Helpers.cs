

using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using static Populations.PopulationManager;

namespace Populations.Helpers
{
    public static class Helpers
    {
        public static int GetRosterCount(TroopRoster roster)
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

        public static string GetCulturalClassName(PopulationClass popClass, CultureObject culture)
        {
            if (popClass.type == PopType.Serfs)
            {
                if (culture.StringId == "sturgia")
                    return "Lowmen";
            } else if (popClass.type == PopType.Slaves)
            {
                if (culture.StringId == "sturgia")
                    return "Thralls";
            }
            return popClass.type.ToString();
        }
       
    }
}
