using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using static BannerKings.Managers.PopulationManager;
using static TaleWorlds.Core.ItemCategory;
using BannerKings.Managers.Titles;

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

        public static string GetGovernmentDescription(GovernmentType type)
        {
            TextObject text = null;
            if (type == GovernmentType.Imperial)
                text = new TextObject("{=!}An Imperial government is a highly centralized one. Policies favor the ruling clan at the expense of vassals. A strong leadership that sees it's vassals more as administrators than lords.");
            else if (type == GovernmentType.Tribal)
                text = new TextObject("{=!}The Tribal association is the most descentralized government. Policies to favor the ruling clan are unwelcome, and every lord is a 'king' or 'queen' in their own right.");
            else if (type == GovernmentType.Republic)
                text = new TextObject("{=!}Republics are firmly setup to avoid the accumulation of power. Every clan is given a chance to rule, and though are able to have a few political advantages, the state is always the priority.");
            else text = new TextObject("{=!}Feudal societies can be seen as the midway between tribals and imperials. Although the ruling clan accumulates privileges, and often cannot be easily removed from the throne, lords and their rightful property need to be respected.");

            return text.ToString();
        }

        public static string GetSuccessionTypeDescription(SuccessionType type)
        {
            TextObject text = null;
            if (type == SuccessionType.Elective_Monarchy)
                text = new TextObject("{=!}In elective monarchies, the ruler is chosen from the realm's dynasties, and rules until death or abdication. Elections take place and all dynasties are able to vote when a new leader is required.");
            else if (type == SuccessionType.Hereditary_Monarchy)
                text = new TextObject("{=!}In hereditary monarchies, the monarch is always the ruling dynasty's leader. No election takes place, and the realm does not change leadership without extraordinary measures.");
            else if (type == SuccessionType.Imperial)
                text = new TextObject("{=!}Imperial successions are completely dictated by the emperor/empress. They will choose from most competent members in their family, as well as other family leaders. Imperial succession values age, family prestigy, military and administration skills. No election takes place.");
            else text = new TextObject("{=!}Republican successions ensure the power is never concentrated. Each year, a new ruler is chosen from the realm's dynasties. The previous ruler is strickly forbidden to participate. Age, family prestige and administration skills are sought after in candidates.");

            return text.ToString();
        }

        public static string GetSuccessionTypeName(SuccessionType type)
        {
            TextObject text = null;
            if (type == SuccessionType.Elective_Monarchy)
                text = new TextObject("{=!}Elective Monarchy");
            else if (type == SuccessionType.Hereditary_Monarchy)
                text = new TextObject("{=!}Hereditary Monarchy");
            else if (type == SuccessionType.Imperial)
                text = new TextObject("{=!}Imperial");
            else text = new TextObject("{=!}Republican");

            return text.ToString();
        }

        public static string GetInheritanceDescription(InheritanceType type)
        {
            TextObject text = null;
            if (type == InheritanceType.Primogeniture)
                text = new TextObject("{=!}Primogeniture favors blood family of eldest age. Clan members not related by blood are last resort.");
            else if (type == InheritanceType.Seniority)
                text = new TextObject("{=!}Seniority favors those of more advanced age in the clan, regardless of blood connections.");
            else text = new TextObject("{=!}Ultimogeniture favors the youngest in the clan, as well as blood family. Clan members not related by blood are last resort.");

            return text.ToString();
        }

        public static string GetGenderLawDescription(GenderLaw type)
        {
            TextObject text = null;
            if (type == GenderLaw.Agnatic)
                text = new TextObject("{=!}Agnatic law favors males. Although females are not completely excluded, they will only be chosen in case a male candidate is not present.");
            else text = new TextObject("{=!}Cognatic law sees no distinction between both genders. Candidates are choosen stricly on their merits, as per the context requires.");

            return text.ToString();
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
                else if (culture.StringId == "khuzait")
                {
                    if (type == TitleType.Kingdom)
                        title = new TextObject("{=!}Khagan");
                    else if (type == TitleType.Dukedom)
                        title = new TextObject("{=!}Baghatur");
                    else if (type == TitleType.County)
                        title = new TextObject("{=!}Khan");
                    else if (type == TitleType.Barony)
                        title = new TextObject("{=!}Bey");
                    else title = new TextObject("{=!}Erxem");
                }
                else if (culture.StringId == "sturgia")
                {
                    if (type == TitleType.Kingdom)
                        title = new TextObject("{=!}Velikiy Knyaz");
                    else if (type == TitleType.Dukedom)
                        title = new TextObject("{=!}Knyaz");
                    else if (type == TitleType.County)
                        title = new TextObject("{=!}Boyar");
                    else if (type == TitleType.Barony)
                        title = new TextObject("{=!}Voivode");
                    else title = new TextObject("{=!}Gospodin");
                }
            }

            if (title == null)
            {
                if (type == TitleType.Kingdom)
                    title = new TextObject("{=!}King");
                else if (type == TitleType.Dukedom)
                    title = new TextObject("{=!}Duke");
                else if (type == TitleType.County)
                    title = new TextObject("{=!}Count");
                else if (type == TitleType.Barony)
                    title = new TextObject("{=!}Baron");
                else title = new TextObject("{=!}Lord");
            }

            return title.ToString();
        }

        public static string GetGovernmentString(GovernmentType type, CultureObject culture = null)
        {
            TextObject title = null;

            if (culture != null)
            {
                if (culture.StringId == "sturgia")
                {
                    if (type == GovernmentType.Tribal)
                        title = new TextObject("{=!}Grand-Principality");
                }
            }

            if (title == null)
            {
                if (type == GovernmentType.Feudal)
                    title = new TextObject("{=!}Kingdom");
                else if (type == GovernmentType.Tribal)
                    title = new TextObject("{=!}High Kingship");
                else if (type == GovernmentType.Imperial)
                    title = new TextObject("{=!}Empire");
                else title = new TextObject("{=!}Republic");
            }

            return title.ToString();
        }

        public static string GetTitlePrefix(TitleType type, CultureObject culture = null)
        {
            TextObject title = null;

            if (culture != null)
            {
                if (culture.StringId == "sturgia")
                {
                    if (type == TitleType.Kingdom)
                        title = new TextObject("{=!}Grand-Principality");
                    else if (type == TitleType.Dukedom)
                        title = new TextObject("{=!}Principality");
                    else if (type == TitleType.County)
                        title = new TextObject("{=!}Boyardom");
                    else if (type == TitleType.Barony)
                        title = new TextObject("{=!}Voivodeship");
                    else title = new TextObject("{=!}Gospodin");
                }
            }

            if (title == null)
            {
                if (type == TitleType.Kingdom)
                    title = new TextObject("{=!}Kingdom");
                else if (type == TitleType.Dukedom)
                    title = new TextObject("{=!}Dukedom");
                else if (type == TitleType.County)
                    title = new TextObject("{=!}County");
                else if (type == TitleType.Barony)
                    title = new TextObject("{=!}Barony");
                else title = new TextObject("{=!}Lordship");
            }
            

            return title.ToString();
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
