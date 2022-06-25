using BannerKings.Managers.Titles;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using static BannerKings.Managers.PopulationManager;
using static TaleWorlds.Core.ItemCategory;

namespace BannerKings.Utils
{
    public static class Helpers
    {
        public static BuildingType _buildingCastleRetinue = Game.Current.ObjectManager.RegisterPresumedObject(new BuildingType("building_castle_retinue"));


        public static void AddSellerToKeep(Hero seller, Settlement settlement)
        {
            AgentData agent = new AgentData(new SimpleAgentOrigin(seller.CharacterObject, 0, null, default(UniqueTroopDescriptor)));
            LocationCharacter locCharacter = new LocationCharacter(agent,
                new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddFixedCharacterBehaviors),
                null, true, LocationCharacter.CharacterRelations.Neutral, null, true, false, null, false, false, true);

            settlement.LocationComplex.GetLocationWithId("lordshall")
                .AddLocationCharacters(delegate { return locCharacter; }, settlement.Culture,
                LocationCharacter.CharacterRelations.Neutral, 1);
        }


        public static bool IsClanLeader(Hero hero) => hero.Clan != null && hero.Clan.Leader == hero;

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
            if (type == ConsumptionType.Industrial)
                return "Satisfaction over availability of manufacturing products such as leather, clay and tools.";
            if (type == ConsumptionType.General)
                return "Satisfaction over availability of various products, including military equipment and horses.";
            return "Satisfaction over availability of food types.";
        }

        public static string GetTitleHonorary(TitleType type, GovernmentType government, bool female, CultureObject culture = null)
        {
            TextObject title = null;
            if (culture != null)
            {
                if (culture.StringId == "battania")
                {
                    if (type == TitleType.Kingdom)
                    {
                        if (female)
                            title = new TextObject("{=!}Ard-Banrigh");
                        else title = new TextObject("{=!}{MALE}Ard-Rìgh{?}Queen{\\?}");
                    }
                    else if (type == TitleType.Dukedom)
                    {
                        if (female)
                            title = new TextObject("{=!}Banrigh");
                        else title = new TextObject("{=!}{MALE}Rìgh{?}Queen{\\?}");
                    }
                    else if (type == TitleType.County)
                    {
                        if (female)
                            title = new TextObject("{=!}Bantiarna");
                        else title = new TextObject("{=!}{MALE}Mormaer{?}Queen{\\?}");
                    }
                    else if (type == TitleType.Barony)
                    {
                        if (female)
                            title = new TextObject("{=!}Thaoiseach");
                        else title = new TextObject("{=!}{MALE}Toisiche{?}Queen{\\?}");
                    }
                    else
                    {
                        if (female)
                            title = new TextObject("{=!}Baintighearna");
                        else title = new TextObject("{=!}{MALE}Tighearna{?}Queen{\\?}");
                    }
                } else if (culture.StringId == "empire")
                {

                    if (type == TitleType.Kingdom)
                    {
                        if (government == GovernmentType.Republic)
                        {
                            if (female)
                                title = new TextObject("{=!}Principissa");
                            else title = new TextObject("{=!}Princeps");
                        } else
                        {
                            if (female)
                                title = new TextObject("{=!}Regina");
                            else title = new TextObject("{=!}{MALE}Rex{?}Queen{\\?}");
                        }
                    }
                    else if (type == TitleType.Dukedom)
                    {
                        if (female)
                            title = new TextObject("{=!}Ducissa");
                        else title = new TextObject("{=!}{MALE}Dux{?}Queen{\\?}");
                    }
                    else if (type == TitleType.County)
                    {
                        if (female)
                            title = new TextObject("{=!}Cometessa");
                        else title = new TextObject("{=!}{MALE}Conte{?}Queen{\\?}");
                    }
                    else if (type == TitleType.Barony)
                    {
                        if (female)
                            title = new TextObject("{=!}Baronessa");
                        else title = new TextObject("{=!}{MALE}Baro{?}Queen{\\?}");
                    }
                    else
                    {
                        if (female)
                            title = new TextObject("{=!}Domina");
                        else title = new TextObject("{=!}{MALE}Dominus{?}Queen{\\?}");
                    }
                }
                else if (culture.StringId == "aserai")
                {
                    if (type == TitleType.Kingdom)
                    {
                        if (female)
                            title = new TextObject("{=!}Sultana");
                        else title = new TextObject("{=!}{MALE}Sultan{?}Queen{\\?}");
                    }
                    else if (type == TitleType.Dukedom)
                    {
                        if (female)
                            title = new TextObject("{=!}Emira");
                        else title = new TextObject("{=!}{MALE}Emir{?}Queen{\\?}");
                    }
                    else if (type == TitleType.County)
                    {
                        if (female)
                            title = new TextObject("{=!}Shaykah");
                        else title = new TextObject("{=!}{MALE}Sheikh{?}Queen{\\?}");
                    }
                    else if (type == TitleType.Barony)
                    {
                        if (female)
                            title = new TextObject("{=!}Walia");
                        else title = new TextObject("{=!}{MALE}Wali{?}Queen{\\?}");
                    }
                    else
                    {
                        if (female)
                            title = new TextObject("{=!}Beghum");
                        else title = new TextObject("{=!}{MALE}Mawlaa{?}Queen{\\?}");
                    }
                }
                else if (culture.StringId == "khuzait")
                {
                    if (type == TitleType.Kingdom)
                    {
                        if (female)
                            title = new TextObject("{=!}Khatun");
                        else title = new TextObject("{=!}{MALE}Khagan{?}Queen{\\?}");
                    }
                    else if (type == TitleType.Dukedom)
                    {
                        if (female)
                            title = new TextObject("{=!}Bekhi");
                        else title = new TextObject("{=!}{MALE}Baghatur{?}Queen{\\?}");
                    }
                    else if (type == TitleType.County)
                    {
                        if (female)
                            title = new TextObject("{=!}Khanum");
                        else title = new TextObject("{=!}{MALE}Khan{?}Queen{\\?}");
                    }
                    else if (type == TitleType.Barony)
                    {
                        if (female)
                            title = new TextObject("{=!}Begum");
                        else title = new TextObject("{=!}{MALE}Bey{?}Queen{\\?}");
                    }
                    else
                    {
                        if (female)
                            title = new TextObject("{=!}Khatagtai");
                        else title = new TextObject("{=!}{MALE}Erxem{?}Queen{\\?}");
                    }
                }
                else if (culture.StringId == "sturgia")
                {
                    if (type == TitleType.Kingdom)
                    {
                        if (female)
                            title = new TextObject("{=!}Velikaya Knyaginya");
                        else title = new TextObject("{=!}{MALE}Velikiy Knyaz{?}Queen{\\?}");
                    }
                    else if (type == TitleType.Dukedom)
                    {
                        if (female)
                            title = new TextObject("{=!}Knyaginya");
                        else title = new TextObject("{=!}{MALE}Knyaz{?}Queen{\\?}");
                    }
                    else if (type == TitleType.County)
                    {
                        if (female)
                            title = new TextObject("{=!}Boyarina");
                        else title = new TextObject("{=!}{MALE}Boyar{?}Queen{\\?}");
                    }
                    else if (type == TitleType.Barony)
                    {
                        if (female)
                            title = new TextObject("{=!}Voivodina");
                        else title = new TextObject("{=!}{MALE}Voivode{?}Queen{\\?}");
                    }
                    else
                    {
                        if (female)
                            title = new TextObject("{=!}Gospoda");
                        else title = new TextObject("{=!}{MALE}Gospodin{?}Queen{\\?}");
                    }
                }
            }

            if (title == null)
            {
                if (type == TitleType.Kingdom)
                {
                    if (female)
                        title = new TextObject("{=!}Queen");
                    else title = new TextObject("{=!}{MALE}King{?}Queen{\\?}");
                }
                else if (type == TitleType.Dukedom)
                {
                    if (female)
                        title = new TextObject("{=!}Duchess");
                    else title = new TextObject("{=!}{MALE}Duke{?}Duchess{\\?}");
                }
                else if (type == TitleType.County)
                {
                    if (female)
                        title = new TextObject("{=!}Countess");
                    else title = new TextObject("{=!}{MALE}Count{?}Countess{\\?}");
                }
                else if (type == TitleType.Barony)
                {
                    if (female)
                        title = new TextObject("{=!}Baroness");
                    else title = new TextObject("{=!}{MALE}Baron{?}Baroness{\\?}");
                }
                else
                {
                    if (female)
                    title = new TextObject("{=!}Lady");
                    else title = new TextObject("{=!}{MALE}Lord{?}Lady{\\?}");
                }
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

        public static string GetTitlePrefix(TitleType type, GovernmentType government, CultureObject culture = null)
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
                } else if (culture.StringId == "sturgia")
                {
                    if (type == TitleType.Kingdom)
                    {
                        if (government == GovernmentType.Republic)
                            title = new TextObject("{=!}Princedom");
                    }
                        
                }
                else if (culture.StringId == "sturgia")
                {
                    if (type == TitleType.Kingdom)
                        title = new TextObject("{=!}Sultanate");
                    else if (type == TitleType.Dukedom)
                        title = new TextObject("{=!}Emirate");
                    else if (type == TitleType.County)
                        title = new TextObject("{=!}Sheikhdom");
                } else if (culture.StringId == "battania")
                {
                    if (type == TitleType.Kingdom)
                    {
                        if (government == GovernmentType.Tribal)
                            title = new TextObject("{=!}High-Kingdom");
                    }
                } else if (culture.StringId == "khuzait")
                {

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

        public static bool IsRetinueTroop(CharacterObject character)
        {
            CharacterObject nobleRecruit = character.Culture.EliteBasicTroop;
            if (nobleRecruit.UpgradeTargets == null)
                return false;

            if (character == nobleRecruit)
                return true;

            if (nobleRecruit.UpgradeTargets != null)
            {
                CharacterObject[] currentUpgrades = nobleRecruit.UpgradeTargets;
                while (currentUpgrades != null && currentUpgrades.Count() > 0)
                {
                    CharacterObject upgrade = currentUpgrades[0];
                    if (upgrade == character)
                        return true;
                    else
                        currentUpgrades = upgrade.UpgradeTargets;
                }           
            }

            return false;
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

        public static CultureObject GetCulture(string id)
        {
            CultureObject culture = MBObjectManager.Instance.GetObjectTypeList<CultureObject>().FirstOrDefault(x => x.StringId == id);
            return culture;
        }

        public static ConsumptionType GetTradeGoodConsumptionType(ItemCategory item)
        {

            string id = item.StringId;
            if (item.Properties == Property.BonusToFoodStores)
                return ConsumptionType.Food;
            else  if (id == "silver" || id == "jewelry" || id == "spice" || id == "velvet" || id == "war_horse" || id.EndsWith("4") || id.EndsWith("5"))
                return ConsumptionType.Luxury;
            else if (id == "wool" || id == "pottery" || id == "cotton" || id == "flax" || id == "linen" || id == "leather" || id == "tools"
                || id.EndsWith("3") || id.Contains("horse"))
                return ConsumptionType.Industrial;
            else return ConsumptionType.General;
        }

        public static ConsumptionType GetTradeGoodConsumptionType(ItemObject item)
        {
            string id = item.StringId;
            if (id == "silver" || id == "jewelry" || id == "spice" || id == "velvet" || id == "fur")
                return ConsumptionType.Luxury;
            if (id == "wool" || id == "pottery" || id == "cotton" || id == "flax" || id == "linen" || id == "leather" || id == "tools")
                return ConsumptionType.Industrial;
            if (item.IsFood)
                return ConsumptionType.Food;
            if (item.IsInitialized && !item.IsBannerItem && (item.HasArmorComponent || item.HasWeaponComponent || item.IsAnimal ||
                                                             item.IsTradeGood || item.HasHorseComponent) && item.StringId != "undefined")
                return ConsumptionType.General;
            return ConsumptionType.None;
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
