

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

        public static string GetCulturalClassName(PopType type, CultureObject culture)
        {
            if (type == PopType.Serfs)
            {
                if (culture.StringId == "sturgia")
                    return "Lowmen";
                else if (culture.StringId == "empire" || culture.StringId == "aserai")
                    return "Commoners";
                else if (culture.StringId == "battania")
                    return "Freemen";
                else if (culture.StringId == "khuzait")
                    return "Nomads";
            } else if (type == PopType.Slaves)
            {
                if (culture.StringId == "sturgia")
                    return "Thralls";
                else if (culture.StringId == "aserai")
                    return "Mameluke";
            } else if (type == PopType.Craftsmen)
            {
                if (culture.StringId == "khuzait" || culture.StringId == "battania")
                    return "Artisans";
            }
            else if (type == PopType.Nobles)
            {
                if (culture.StringId == "empire")
                    return "Nobiles";
                else if (culture.StringId == "sturgia")
                    return "Knyaz";
                else if (culture.StringId == "vlandia")
                    return "Ealdormen";
            }
            return type.ToString();
        }
    }
}
