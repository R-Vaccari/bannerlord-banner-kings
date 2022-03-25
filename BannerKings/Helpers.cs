using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using static BannerKings.Managers.TitleManager;
using static BannerKings.Managers.PopulationManager;
using static TaleWorlds.Core.ItemCategory;

namespace BannerKings.Helpers
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

        public static string GetConsumptionHint(ConsumptionType type)
        {
            if (type == ConsumptionType.Luxury)
                return "Satisfaction over availability of products such as jewelry, velvets and fur.";
            else if (type == ConsumptionType.Industrial)
                return "Satisfaction over availability of manufacturing products such as leather, clay and tools.";
            else if (type == ConsumptionType.General)
                return "Satisfaction over availability of various products, including military equipment and horses.";
            else return "Satisfaction over availability of food types.";
        }

        public static string GetTitleHonorary(TitleType type, bool female, CultureObject culture = null)
        {
            TextObject title = null;
            if (culture != null)
            {
                if (culture.StringId == "battania")
                {
                    if (type == TitleType.Kingdom)
                        title = new TextObject("{=!}Ard-Righ");
                    else if (type == TitleType.Dukedom)
                        title = new TextObject("{=!}Righ");
                    else if (type == TitleType.County)
                        title = new TextObject("{=!}Mormaer");
                    else if (type == TitleType.Barony)
                        title = new TextObject("{=!}Toisiche");
                    else title = new TextObject("{=!}Tighearna");
                } else if (culture.StringId == "empire")
                {
                    if (type == TitleType.Kingdom)
                        title = new TextObject("{=!}Rex");
                    else if (type == TitleType.Dukedom)
                        title = new TextObject("{=!}Dux");
                    else if (type == TitleType.County)
                        title = new TextObject("{=!}Conte");
                    else if (type == TitleType.Barony)
                        title = new TextObject("{=!}Baro");
                    else title = new TextObject("{=!}Dominus");
                }
                else if (culture.StringId == "aserai")
                {
                    if (type == TitleType.Kingdom)
                        title = new TextObject("{=!}Sultan");
                    else if (type == TitleType.Dukedom)
                        title = new TextObject("{=!}Emir");
                    else if (type == TitleType.County)
                        title = new TextObject("{=!}Sheikh");
                    else if (type == TitleType.Barony)
                        title = new TextObject("{=!}Wali");
                    else title = new TextObject("{=!}Mawlaa");
                }
            }

            if (type == TitleType.Kingdom)
                title = new TextObject("{=!}King");
            else if (type == TitleType.Dukedom)
                title = new TextObject("{=!}Duke");
            else if  (type == TitleType.County)
                title = new TextObject("{=!}Count");
            else if (type == TitleType.Barony)
                title = new TextObject("{=!}Baron");
            title = new TextObject("{=!}Lord");

            return title.ToString();
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

        public static ConsumptionType GetTradeGoodConsumptionType(ItemObject item)
        {
            string id = item.StringId;
            if (id == "silver" || id == "jewelry" || id == "spice" || id == "velvet" || id == "fur")
                return ConsumptionType.Luxury;
            else if (id == "wool" || id == "pottery" || id == "cotton" || id == "flax" || id == "linen" || id == "leather" || id == "tools")
                return ConsumptionType.Industrial;
            else if (item.IsFood)
                return ConsumptionType.Food;
            else if (item.IsInitialized && !item.IsBannerItem && (item.HasArmorComponent || item.HasWeaponComponent || item.IsAnimal ||
                item.IsTradeGood || item.HasHorseComponent) && item.StringId != "undefined")
                return ConsumptionType.General;
            else return ConsumptionType.None;
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
