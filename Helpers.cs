using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using static Populations.Managers.TitleManager;
using static Populations.PopulationManager;
using static TaleWorlds.Core.ItemCategory;

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
            string id = string.Format("pop_class_{0}{1}", type.ToString().ToLower(), cultureModifier);
            string text = type.ToString();
            if (type == PopType.Serfs)
            {
                if (culture.StringId == "sturgia")
                    text = "Lowmen";
                else if (culture.StringId == "empire" || culture.StringId == "aserai")
                    text = "Commoners";
                else if (culture.StringId == "battania")
                    text = "Freemen";
                else if (culture.StringId == "khuzait")
                    text = "Nomads";
            }
            else if (type == PopType.Slaves)
            {
                if (culture.StringId == "sturgia")
                    text = "Thralls";
                else if (culture.StringId == "aserai")
                    text = "Mameluke";
            }
            else if (type == PopType.Craftsmen)
            {
                if (culture.StringId == "khuzait" || culture.StringId == "battania")
                    text = "Artisans";
            }
            else if (type == PopType.Nobles)
            {
                if (culture.StringId == "empire")
                    text = "Nobiles";
                else if (culture.StringId == "sturgia")
                    text = "Knyaz";
                else if (culture.StringId == "vlandia")
                    text = "Ealdormen";
            }
            string finalResult = string.Format("{{={0}}}{1}", id, text);
            return new TextObject(finalResult);
            
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

        public static string GetTitleHonorary(TitleType type, bool female)
        {
            if (type == TitleType.Kingdom)
                return "King";
            else if (type == TitleType.Dukedom)
                return "Duke";
            else if  (type == TitleType.County)
                return "Count";
            else if (type == TitleType.Barony)
                return "Baron";
            else return "Lord";
        }

        public static string GetTitlePrefix(TitleType type)
        {
            if (type == TitleType.Kingdom)
                return "Kingdom";
            else if (type == TitleType.Dukedom)
                return "Dukedom";
            else if (type == TitleType.County)
                return "County";
            else if (type == TitleType.Barony)
                return "Barony";
            else return "Lordship";
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

        public static ConsumptionType GetTradeGoodConsumptionType(ItemCategory item)
        {
            string id = item.StringId;
            if (id == "silver" || id == "jewelry" || id == "spice" || id == "velvet" || id == "fur")
                return ConsumptionType.Luxury;
            else if (id == "wool" || id == "pottery" || id == "cotton" || id == "flax" || id == "linen" || id == "leather" || id == "tools")
                return ConsumptionType.Industrial;
            else if (item.Properties == Property.BonusToFoodStores)
                return ConsumptionType.Food;
            else return ConsumptionType.General;
        }

        public static XmlDocument CreateDocumentFromXmlFile(string xmlPath)
        {
            XmlDocument xmlDocument = new XmlDocument();
            StreamReader streamReader = new StreamReader(xmlPath);
            string xml = streamReader.ReadToEnd();
             xmlDocument.LoadXml(xml);
            streamReader.Close();
            return xmlDocument;
        }
    }
}
