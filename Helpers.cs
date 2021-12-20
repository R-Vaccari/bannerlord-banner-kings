using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using static Populations.PopulationManager;

namespace Populations.Helpers
{
    public static class Helpers
    {
        public static BuildingType _buildingCastleRetinue = Game.Current.ObjectManager.RegisterPresumedObject<BuildingType>(new BuildingType("building_castle_retinue"));
        public static int GetRosterCount(TroopRoster roster, string filter = null)
        {
            List<TroopRosterElement> rosters = roster.GetTroopRoster();
            int count = 0;
            rosters.ForEach(rosterElement =>
            {
                if (filter == null)
                {
                    if (!rosterElement.Character.IsHero)
                        count += rosterElement.Number + rosterElement.WoundedNumber;
                } else if (!rosterElement.Character.IsHero && rosterElement.Character.StringId.Contains(filter))
                        count += rosterElement.Number + rosterElement.WoundedNumber;

            });
            return count;
        }

        public static TextObject GetClassName(PopType type, CultureObject culture)
        {
            string cultureModifier = '_' + culture.StringId;
            string formatted = string.Format("pop_class_{0}{1}", type.ToString().ToLower(), cultureModifier, type.ToString());
            
            return GameTexts.FindText(formatted); 
        }

        public static string GetClassHint(PopType type, CultureObject culture)
        {
            string name = GetClassName(type, culture).ToString();
            string description;
            if (type == PopType.Nobles)
                description = " represent the free, wealthy and influential members of society. They pay very high taxes and increase your influence as a lord.";
            else if (type == PopType.Craftsmen)
                description = " are free people of trade, such as merchants, engineers and blacksmiths. Somewhat wealthy, free but not high status people. Craftsmen pay a significant amount of taxes and their presence boosts economical development. Their skills can also be hired to significantly boost construction projects.";
            else if (type == PopType.Serfs)
                description = " are the lowest class that possess some sort of freedom. Unable to attain specialized skills such as those of craftsmen, these people represent the agricultural workforce. They also pay tax over the profit of their production excess.";
            else description = " are those destituted: criminals, prisioners unworthy of a ransom, and those unlucky to be born into slavery. Slaves do the hard manual labor across settlements, such as building and mining. They themselves pay no tax as they are unable to have posessions, but their labor generates income gathered as tax from their masters.";
            return name + description;
        }

        public static bool IsRetinueTroop(CharacterObject character, CultureObject settlementCulture)
        {
            CultureObject culture = character.Culture;
            CharacterObject nobleRecruit = culture.EliteBasicTroop;

            if (nobleRecruit.UpgradeTargets == null)
                return false;
            

            if (culture == settlementCulture)
                if (character == nobleRecruit || nobleRecruit.UpgradeTargets.Contains(character))
                    return true;

            return false;
        }
    }
}
