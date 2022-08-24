using System;
using System.IO;
using System.Linq;
using System.Xml;
using BannerKings.Managers.Titles;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.CampaignSystem.Settlements.Locations;
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
            var agent = new AgentData(new SimpleAgentOrigin(seller.CharacterObject, 0));
            var locCharacter = new LocationCharacter(agent, SandBoxManager.Instance.AgentBehaviorManager.AddFixedCharacterBehaviors, null, true, LocationCharacter.CharacterRelations.Neutral, null, true);

            settlement.LocationComplex.GetLocationWithId("lordshall")
                .AddLocationCharacters(delegate { return locCharacter; }, settlement.Culture, LocationCharacter.CharacterRelations.Neutral, 1);
        }


        public static bool IsClanLeader(Hero hero)
        {
            return hero.Clan != null && hero.Clan.Leader == hero;
        }

        public static bool IsCloseFamily(Hero hero, Hero family)
        {
            return hero.Father == family || hero.Mother == family || hero.Children.Contains(family) ||
                   hero.Siblings.Contains(family);
        }

        public static int GetRosterCount(TroopRoster roster, string filter = null)
        {
            var rosters = roster.GetTroopRoster();
            var count = 0;

            rosters.ForEach(rosterElement =>
            {
                if (filter == null)
                {
                    if (!rosterElement.Character.IsHero)
                    {
                        count += rosterElement.Number + rosterElement.WoundedNumber;
                    }
                }
                else if (!rosterElement.Character.IsHero && rosterElement.Character.StringId.Contains(filter))
                {
                    count += rosterElement.Number + rosterElement.WoundedNumber;
                }
            });

            return count;
        }

        public static TextObject GetClassName(PopType type, CultureObject culture)
        {
            var cultureModifier = '_' + culture.StringId;
            var id = $"pop_class_{type.ToString().ToLower()}{cultureModifier}";
            var text = type.ToString();
            switch (type)
            {
                case PopType.Serfs when culture.StringId == "sturgia":
                    text = "Lowmen";
                    break;
                case PopType.Serfs when culture.StringId is "empire" or "aserai":
                    text = "Commoners";
                    break;
                case PopType.Serfs when culture.StringId == "battania":
                    text = "Freemen";
                    break;
                case PopType.Serfs:
                {
                    if (culture.StringId == "khuzait")
                    {
                        text = "Nomads";
                    }

                    break;
                }
                case PopType.Slaves when culture.StringId == "sturgia":
                    text = "Thralls";
                    break;
                case PopType.Slaves:
                {
                    if (culture.StringId == "aserai")
                    {
                        text = "Mameluke";
                    }

                    break;
                }
                case PopType.Craftsmen:
                {
                    if (culture.StringId is "khuzait" or "battania")
                    {
                        text = "Artisans";
                    }

                    break;
                }
                case PopType.Nobles when culture.StringId == "empire":
                    text = "Nobiles";
                    break;
                case PopType.Nobles when culture.StringId == "sturgia":
                    text = "Knyaz";
                    break;
                case PopType.Nobles:
                {
                    if (culture.StringId == "vlandia")
                    {
                        text = "Ealdormen";
                    }

                    break;
                }
                case PopType.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            var finalResult = $"{{={id}}}{text}";
            return new TextObject(finalResult);
        }

        public static string GetGovernmentDescription(GovernmentType type)
        {
            var text = type switch
            {
                GovernmentType.Imperial => new TextObject("{=jazWnee9d}An Imperial government is a highly centralized one. Policies favor the ruling clan at the expense of vassals. A strong leadership that sees it's vassals more as administrators than lords."),
                GovernmentType.Tribal => new TextObject("{=uvCfG9NpK}The Tribal association is the most descentralized government. Policies to favor the ruling clan are unwelcome, and every lord is a 'king' or 'queen' in their own right."),
                GovernmentType.Republic => new TextObject("{=521WG0UQx}Republics are firmly setup to avoid the accumulation of power. Every clan is given a chance to rule, and though are able to have a few political advantages, the state is always the priority."),
                _ => new TextObject("{=Hz4iKnfFy}Feudal societies can be seen as the midway between tribals and imperials. Although the ruling clan accumulates privileges, and often cannot be easily removed from the throne, lords and their rightful property need to be respected.")
            };

            return text.ToString();
        }

        public static string GetSuccessionTypeDescription(SuccessionType type)
        {
            var text = type switch
            {
                SuccessionType.Elective_Monarchy => new TextObject("{=QmekWoADm}In elective monarchies, the ruler is chosen from the realm's dynasties, and rules until death or abdication. Elections take place and all dynasties are able to vote when a new leader is required."),
                SuccessionType.Hereditary_Monarchy => new TextObject("{=a5tfVEib2}In hereditary monarchies, the monarch is always the ruling dynasty's leader. No election takes place, and the realm does not change leadership without extraordinary measures."),
                SuccessionType.Imperial => new TextObject("{=898cgUpLp}Imperial successions are completely dictated by the emperor/empress. They will choose from most competent members in their family, as well as other family leaders. Imperial succession values age, family prestigy, military and administration skills. No election takes place."),
                _ => new TextObject("{=1vdfcNJZh}Republican successions ensure the power is never concentrated. Each year, a new ruler is chosen from the realm's dynasties. The previous ruler is strickly forbidden to participate. Age, family prestige and administration skills are sought after in candidates.")
            };

            return text.ToString();
        }

        public static string GetSuccessionTypeName(SuccessionType type)
        {
            var text = type switch
            {
                SuccessionType.Elective_Monarchy => new TextObject("{=ZBmvcKQgy}Elective Monarchy"),
                SuccessionType.Hereditary_Monarchy => new TextObject("{=CEJcse1Z5}Hereditary Monarchy"),
                SuccessionType.Imperial => new TextObject("{=ip4mtSgEq}Imperial"),
                _ => new TextObject("{=XhC6c44O6}Republican")
            };

            return text.ToString();
        }

        public static string GetInheritanceDescription(InheritanceType type)
        {
            var text = type switch
            {
                InheritanceType.Primogeniture => new TextObject("{=3oEyqMYDh}Primogeniture favors blood family of eldest age. Clan members not related by blood are last resort."),
                InheritanceType.Seniority => new TextObject("{=Or0T9kVDY}Seniority favors those of more advanced age in the clan, regardless of blood connections."),
                _ => new TextObject("{=7QdCduTzB}Ultimogeniture favors the youngest in the clan, as well as blood family. Clan members not related by blood are last resort.")
            };

            return text.ToString();
        }

        public static string GetGenderLawDescription(GenderLaw type)
        {
            return type == GenderLaw.Agnatic 
                ? new TextObject("{=usaOJjnXE}Agnatic law favors males. Although females are not completely excluded, they will only be chosen in case a male candidate is not present.").ToString() 
                : new TextObject("{=TL0YKqx6S}Cognatic law sees no distinction between both genders. Candidates are choosen stricly on their merits, as per the context requires.").ToString();
        }

        public static string GetClassHint(PopType type, CultureObject culture)
        {
            var name = GetClassName(type, culture).ToString();
            var description = type switch
            {
                PopType.Nobles => " represent the free, wealthy and influential members of society. They pay very high taxes and increase your influence as a lord.",
                PopType.Craftsmen => " are free people of trade, such as merchants, engineers and blacksmiths. Somewhat wealthy, free but not high status people. Craftsmen pay a significant amount of taxes and their presence boosts economical development. Their skills can also be hired to significantly boost construction projects.",
                PopType.Serfs => " are the lowest class that possess some sort of freedom. Unable to attain specialized skills such as those of craftsmen, these people represent the agricultural workforce. They also pay tax over the profit of their production excess.",
                _ => " are those destituted: criminals, prisioners unworthy of a ransom, and those unlucky to be born into slavery. Slaves do the hard manual labor across settlements, such as building and mining. They themselves pay no tax as they are unable to have posessions, but their labor generates income gathered as tax from their masters."
            };

            return name + description;
        }

        public static string GetConsumptionHint(ConsumptionType type)
        {
            return type switch
            {
                ConsumptionType.Luxury => "Satisfaction over availability of products such as jewelry, velvets and fur.",
                ConsumptionType.Industrial => "Satisfaction over availability of manufacturing products such as leather, clay and tools.",
                ConsumptionType.General => "Satisfaction over availability of various products, including military equipment and horses.",
                _ => "Satisfaction over availability of food types."
            };
        }

        public static string GetTitleHonorary(TitleType type, GovernmentType government, bool female, CultureObject culture = null)
        {
            TextObject title = null;
            if (culture != null)
            {
                switch (culture.StringId)
                {
                    case "battania" when type == TitleType.Kingdom:
                    {
                        title = female 
                            ? new TextObject("{=By688mFkm}Ard-Banrigh") 
                            : new TextObject("{=zqfcSxaeL}{MALE}Ard-Rìgh{?}Queen{\\?}");

                        break;
                    }
                    case "battania" when type == TitleType.Dukedom:
                    {
                        title = female 
                            ? new TextObject("{=mELLuQnWw}Banrigh")
                            : new TextObject("{=FYMKf2hwa}{MALE}Rìgh{?}Queen{\\?}");

                        break;
                    }
                    case "battania" when type == TitleType.County:
                    {
                        title = female 
                            ? new TextObject("{=ars2EBEYh}Bantiarna") 
                            : new TextObject("{=KRvOtUwuJ}{MALE}Mormaer{?}Queen{\\?}");

                        break;
                    }
                    case "battania" when type == TitleType.Barony:
                    {
                        title = female 
                            ? new TextObject("{=59dJ4nfFL}Thaoiseach") 
                            : new TextObject("{=HMi9UUMxe}{MALE}Toisiche{?}Queen{\\?}");

                        break;
                    }
                    case "battania" when female:
                        title = new TextObject("{=52X0VrD8b}Baintighearna");
                        break;
                    case "battania":
                        title = new TextObject("{=68Td3qmiX}{MALE}Tighearna{?}Queen{\\?}");
                        break;
                    case "empire" when type == TitleType.Kingdom:
                    {
                        if (government == GovernmentType.Republic)
                        {
                            title = female
                                ? new TextObject("{=6ujRpaq5E}Principissa")
                                : new TextObject("{=dvJGLCZYA}Princeps");
                        }
                        else
                        {
                            title = female 
                                ? new TextObject("{=0NR774BCZ}Regina") 
                                : new TextObject("{=fNdiGARWw}{MALE}Rex{?}Queen{\\?}");
                        }

                        break;
                    }
                    case "empire" when type == TitleType.Dukedom:
                    {
                        title = female 
                            ? new TextObject("{=EoaFhUHvR}Ducissa") 
                            : new TextObject("{=c7ponMAcO}{MALE}Dux{?}Queen{\\?}");

                        break;
                    }
                    case "empire" when type == TitleType.County:
                    {
                        title = female 
                            ? new TextObject("{=7efB0cmgF}Cometessa") 
                            : new TextObject("{=VkKoz0R1P}{MALE}Conte{?}Queen{\\?}");

                        break;
                    }
                    case "empire" when type == TitleType.Barony:
                    {
                        title = female 
                            ? new TextObject("{=gxqEEYPPm}Baronessa") 
                            : new TextObject("{=JoTV5hnR1}{MALE}Baro{?}Queen{\\?}");

                        break;
                    }
                    case "empire" when female:
                        title = new TextObject("{=AbsNid71c}Domina");
                        break;
                    case "empire":
                        title = new TextObject("{=miYCBgzYw}{MALE}Dominus{?}Queen{\\?}");
                        break;
                    case "aserai" when type == TitleType.Kingdom:
                    {
                        title = female 
                            ? new TextObject("{=gmbkEEyMt}Sultana") 
                            : new TextObject("{=zg38KDYqP}{MALE}Sultan{?}Queen{\\?}");

                        break;
                    }
                    case "aserai" when type == TitleType.Dukedom:
                    {
                        title = female
                            ? new TextObject("{=1M6esqcaH}Emira") 
                            : new TextObject("{=WMQZLhMRT}{MALE}Emir{?}Queen{\\?}");

                        break;
                    }
                    case "aserai" when type == TitleType.County:
                    {
                        title = female
                            ? new TextObject("{=oVHCBmab9}Shaykah") 
                            : new TextObject("{=6RFtsWTpY}{MALE}Sheikh{?}Queen{\\?}");

                        break;
                    }
                    case "aserai" when type == TitleType.Barony:
                    {
                        title = female
                            ? new TextObject("{=Mi4NP1Utk}Walia") 
                            : new TextObject("{=Ab6UJFuHB}{MALE}Wali{?}Queen{\\?}");

                        break;
                    }
                    case "aserai" when female:
                        title = new TextObject("{=aLu8G8dzN}Beghum");
                        break;
                    case "aserai":
                        title = new TextObject("{=bGuhjsgmr}{MALE}Mawlaa{?}Queen{\\?}");
                        break;
                    case "khuzait" when type == TitleType.Kingdom:
                    {
                        title = female 
                            ? new TextObject("{=un9uYYazd}Khatun") 
                            : new TextObject("{=kLQRmXhok}{MALE}Khagan{?}Queen{\\?}");

                        break;
                    }
                    case "khuzait" when type == TitleType.Dukedom:
                    {
                        title = female 
                            ? new TextObject("{=mGC7s5CXb}Bekhi") 
                            : new TextObject("{=APbSMMCPE}{MALE}Baghatur{?}Queen{\\?}");

                        break;
                    }
                    case "khuzait" when type == TitleType.County:
                    {
                        title = female 
                            ? new TextObject("{=FTLCoRRd8}Khanum") 
                            : new TextObject("{=RVJ8xCUkS}{MALE}Khan{?}Queen{\\?}");

                        break;
                    }
                    case "khuzait" when type == TitleType.Barony:
                    {
                        title = female 
                            ? new TextObject("{=eDHD5z51Y}Begum") 
                            : new TextObject("{=sktcGny95}{MALE}Bey{?}Queen{\\?}");

                        break;
                    }
                    case "khuzait" when female:
                        title = new TextObject("{=ZdMvLGrew}Khatagtai");
                        break;
                    case "khuzait":
                        title = new TextObject("{=ZjTEWzBtt}{MALE}Erxem{?}Queen{\\?}");
                        break;
                    case "sturgia" when type == TitleType.Kingdom:
                    {
                        title = female 
                            ? new TextObject("{=rbiFSSitE}Velikaya Knyaginya") 
                            : new TextObject("{=PLqcZ5JC9}{MALE}Velikiy Knyaz{?}Queen{\\?}");

                        break;
                    }
                    case "sturgia" when type == TitleType.Dukedom:
                    {
                        title = female 
                            ? new TextObject("{=h8XFgYZ8w}Knyaginya") 
                            : new TextObject("{=NUkqVeNDV}{MALE}Knyaz{?}Queen{\\?}");

                        break;
                    }
                    case "sturgia" when type == TitleType.County:
                    {
                        title = female 
                            ? new TextObject("{=z21AAfxa5}Boyarina") 
                            : new TextObject("{=yk0p0g7qh}{MALE}Boyar{?}Queen{\\?}");

                        break;
                    }
                    case "sturgia" when type == TitleType.Barony:
                    {
                        title = female 
                            ? new TextObject("{=Wmpnwz0km}Voivodina") 
                            : new TextObject("{=tpAW4hJO0}{MALE}Voivode{?}Queen{\\?}");

                        break;
                    }
                    case "sturgia" when female:
                        title = new TextObject("{=7ce8HwGWZ}Gospoda");
                        break;
                    case "sturgia":
                        title = new TextObject("{=w9fFZWqMf}{MALE}Gospodin{?}Queen{\\?}");
                        break;
                }
            }

            if (title != null)
            {
                return title.ToString();
            }

            switch (type)
            {
                case TitleType.Kingdom when female:
                    title = new TextObject("{=GsybwTbY3}Queen");
                    break;
                case TitleType.Kingdom:
                    title = new TextObject("{=yD0hZdqVo}{MALE}King{?}Queen{\\?}");
                    break;
                case TitleType.Dukedom when female:
                    title = new TextObject("{=XS93LYssn}Duchess");
                    break;
                case TitleType.Dukedom:
                    title = new TextObject("{=OV9HrGhsT}{MALE}Duke{?}Duchess{\\?}");
                    break;
                case TitleType.County when female:
                    title = new TextObject("{=AnHcBrjsv}Countess");
                    break;
                case TitleType.County:
                    title = new TextObject("{=7xBJkwgwt}{MALE}Count{?}Countess{\\?}");
                    break;
                case TitleType.Barony when female:
                    title = new TextObject("{=LugfnFJ7y}Baroness");
                    break;
                case TitleType.Barony:
                    title = new TextObject("{=vZh21py3O}{MALE}Baron{?}Baroness{\\?}");
                    break;
                case TitleType.Empire:
                case TitleType.Lordship:
                default:
                {
                    title = female 
                        ? new TextObject("{=pweu26x50}Lady")
                        : new TextObject("{=J4giLpLfK}{MALE}Lord{?}Lady{\\?}");

                    break;
                }
            }

            return title.ToString();
        }

        public static string GetGovernmentString(GovernmentType type, CultureObject culture = null)
        {
            TextObject title = null;

            if (culture is {StringId: "sturgia"})
            {
                if (type == GovernmentType.Tribal)
                {
                    title = new TextObject("{=FFPOoVNTf}Grand-Principality");
                }
            }

            if (title == null)
            {
                title = type switch
                {
                    GovernmentType.Feudal => new TextObject("{=5KqcnQH5o}Kingdom"),
                    GovernmentType.Tribal => new TextObject("{=eMxqdXOhe}High Kingship"),
                    GovernmentType.Imperial => new TextObject("{=LQZ3dzeM6}Empire"),
                    _ => new TextObject("{=rXmKj8DZP}Republic")
                };
            }

            return title.ToString();
        }

        public static string GetTitlePrefix(TitleType type, GovernmentType government, CultureObject culture = null)
        {
            TextObject title = null;

            if (culture != null)
            {
                switch (culture.StringId)
                {
                    case "sturgia" when type == TitleType.Kingdom:
                        title = new TextObject("{=FFPOoVNTf}Grand-Principality");
                        break;
                    case "sturgia" when type == TitleType.Dukedom:
                        title = new TextObject("{=BJpEPWBX5}Principality");
                        break;
                    case "sturgia" when type == TitleType.County:
                        title = new TextObject("{=Wn4WvCrmq}Boyardom");
                        break;
                    case "sturgia" when type == TitleType.Barony:
                        title = new TextObject("{=UQdcSYYNA}Voivodeship");
                        break;
                    case "sturgia":
                        title = new TextObject("{=KfU7ixePQ}Gospodin");
                        break;
                    case "aserai" when type == TitleType.Kingdom:
                        title = new TextObject("{=gmbkEEyMt}Sultanate");
                        break;
                    case "aserai" when type == TitleType.Dukedom:
                        title = new TextObject("{=1M6esqcaH}Emirate");
                        break;
                    case "aserai":
                    {
                        if (type == TitleType.County)
                        {
                            title = new TextObject("{=CeAAnvH2n}Sheikhdom");
                        }

                        break;
                    }
                    case "battania":
                    {
                        if (government == GovernmentType.Tribal)
                        {
                            title = type switch
                            {
                                TitleType.Kingdom => new TextObject("{=ubXqM8Tb5}High-Kingdom"),
                                TitleType.Dukedom => new TextObject("{=zrWGgnY4g}Petty Kingdom"),
                                _ => title
                            };
                        }

                        break;
                    }
                }
            }

            title ??= type switch
            {
                TitleType.Kingdom => new TextObject("{=5KqcnQH5o}Kingdom"),
                TitleType.Dukedom => new TextObject("{=RwBtqusG6}Dukedom"),
                TitleType.County => new TextObject("{=FWQ74Loqa}County"),
                TitleType.Barony => new TextObject("{=RRcJQc1aa}Barony"),
                _ => new TextObject("{=87RxZ0NMK}Lordship")
            };


            return title.ToString();
        }

        public static bool IsRetinueTroop(CharacterObject character)
        {
            var nobleRecruit = character.Culture.EliteBasicTroop;
            if (nobleRecruit.UpgradeTargets == null)
            {
                return false;
            }

            if (character == nobleRecruit)
            {
                return true;
            }

            if (nobleRecruit.UpgradeTargets != null)
            {
                var currentUpgrades = nobleRecruit.UpgradeTargets;
                while (currentUpgrades != null && currentUpgrades.Any())
                {
                    var upgrade = currentUpgrades[0];
                    if (upgrade == character)
                    {
                        return true;
                    }

                    currentUpgrades = upgrade.UpgradeTargets;
                }
            }

            return false;
        }

        public static bool IsRetinueTroop(CharacterObject character, CultureObject settlementCulture)
        {
            var culture = character.Culture;
            var nobleRecruit = culture.EliteBasicTroop;

            if (nobleRecruit.UpgradeTargets == null)
            {
                return false;
            }


            if (culture == settlementCulture)
            {
                if (character == nobleRecruit || nobleRecruit.UpgradeTargets.Contains(character))
                {
                    return true;
                }
            }

            return false;
        }

        public static CultureObject GetCulture(string id)
        {
            var culture = MBObjectManager.Instance.GetObjectTypeList<CultureObject>().FirstOrDefault(x => x.StringId == id);
            return culture;
        }

        public static ConsumptionType GetTradeGoodConsumptionType(ItemCategory item)
        {
            var id = item.StringId;
            if (item.Properties == Property.BonusToFoodStores)
            {
                return ConsumptionType.Food;
            }

            if (id is "silver" or "jewelry" or "spice" or "velvet" or "war_horse" ||
                id.EndsWith("4") || id.EndsWith("5"))
            {
                return ConsumptionType.Luxury;
            }

            if (id is "wool" or "pottery" or "cotton" or "flax" or "linen" or "leather" or "tools" 
                || id.EndsWith("3") || id.Contains("horse"))
            {
                return ConsumptionType.Industrial;
            }

            return ConsumptionType.General;
        }

        public static ConsumptionType GetTradeGoodConsumptionType(ItemObject item)
        {
            var id = item.StringId;
            switch (id)
            {
                case "silver" or "jewelry" or "spice" or "velvet" or "fur":
                    return ConsumptionType.Luxury;
                case "wool" or "pottery" or "cotton" or "flax" or "linen" or "leather" or "tools":
                    return ConsumptionType.Industrial;
            }

            if (item.IsFood)
            {
                return ConsumptionType.Food;
            }

            if (item.IsInitialized && !item.IsBannerItem &&
                (item.HasArmorComponent || item.HasWeaponComponent || item.IsAnimal ||
                 item.IsTradeGood || item.HasHorseComponent) && item.StringId != "undefined")
            {
                return ConsumptionType.General;
            }

            return ConsumptionType.None;
        }

        public static XmlDocument CreateDocumentFromXmlFile(string xmlPath)
        {
            var xmlDocument = new XmlDocument();
            var streamReader = new StreamReader(xmlPath);
            var xml = streamReader.ReadToEnd();
            xmlDocument.LoadXml(xml);
            streamReader.Close();
            return xmlDocument;
        }
    }
}