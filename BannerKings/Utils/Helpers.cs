using System;
using System.IO;
using System.Linq;
using System.Xml;
using BannerKings.Managers.Titles;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
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
        public static ItemModifierGroup GetItemModifierGroup(ItemObject item)
        {
            ItemModifierGroup modifierGroup = null;
            if (item.ArmorComponent != null)
            {
                modifierGroup = item.ArmorComponent.ItemModifierGroup;
            }
            else if (item.WeaponComponent != null)
            {
                modifierGroup = item.WeaponComponent.ItemModifierGroup;
            }
            else if (item.IsFood)
            {
                modifierGroup = Game.Current.ObjectManager.GetObject<ItemModifierGroup>("consumables");
            }
            else if (item.IsAnimal)
            {
                modifierGroup = Game.Current.ObjectManager.GetObject<ItemModifierGroup>("animals");
            }
            else if (!item.HasHorseComponent && item.ItemCategory != DefaultItemCategories.Iron)
            {
                modifierGroup = Game.Current.ObjectManager.GetObject<ItemModifierGroup>("goods");
            }

            return modifierGroup;
        }

        public static void SetAlliance(IFaction faction1, IFaction faction2)
        {
            var stance = Clan.PlayerClan.GetStanceWith(Hero.OneToOneConversationHero.Clan);
            if (stance.IsNeutral)
            {
                stance.IsAllied = true;
                if (faction1 == Hero.MainHero.MapFaction || faction2 == Hero.MainHero.MapFaction)
                {
                    MBInformationManager.AddQuickInformation(new TextObject("{=gc8D4iH4}The {FACTION1} and {FACTION2} are now allies.")
                        .SetTextVariable("FACTION1", faction1.Name)
                        .SetTextVariable("FACTION2", faction2.Name),
                        100,
                        null,
                        GetKingdomDecisionSound());
                }
            }
        }

        internal static string GetRelationDecisionSound() => "event:/ui/notification/relation";
        internal static string GetKingdomDecisionSound() => "event:/ui/notification/kingdom_decision";

        public static void AddCharacterToKeep(Hero hero, Settlement settlement)
        {
            var agent = new AgentData(new SimpleAgentOrigin(hero.CharacterObject, 0));
            var locCharacter = new LocationCharacter(agent, SandBoxManager.Instance.AgentBehaviorManager.AddFixedCharacterBehaviors, null, true, LocationCharacter.CharacterRelations.Neutral, null, true);

            settlement.LocationComplex.GetLocationWithId("lordshall")
                .AddLocationCharacters(delegate { return locCharacter; }, settlement.Culture, LocationCharacter.CharacterRelations.Neutral, 1);
        }

        public static void AddCharacterToKeep(CharacterObject character, Settlement settlement)
        {
            var agent = new AgentData(new SimpleAgentOrigin(character, 0));
            var locCharacter = new LocationCharacter(agent, SandBoxManager.Instance.AgentBehaviorManager.AddFixedCharacterBehaviors, null, true, LocationCharacter.CharacterRelations.Neutral, null, true);

            settlement.LocationComplex.GetLocationWithId("lordshall")
                .AddLocationCharacters(delegate { return locCharacter; }, settlement.Culture, LocationCharacter.CharacterRelations.Neutral, 1);
        }

        public static void AddNotableToKeep(Hero notable, Settlement settlement)
        {
            if (notable != null && settlement != null)
            {
                var town = settlement.Town;

                LocationCharacter locCharacter = town.Settlement.LocationComplex.GetLocationCharacterOfHero(notable);
                if (locCharacter != null)
                {
                    locCharacter.SpecialTargetTag = null;

                    Location characterLocation = town.Settlement.LocationComplex.GetLocationOfCharacter(notable);
                    if (characterLocation.StringId != "lordshall")
                    {
                        town.Settlement.LocationComplex.ChangeLocation(locCharacter, characterLocation,
                                            settlement.LocationComplex.GetLocationWithId("lordshall"));
                    }
                }
            }
        }

        public static void AddMusicianToKeep(Settlement settlement)
        {
            var agent = new AgentData(new SimpleAgentOrigin(settlement.Culture.Musician, 0));
            var locCharacter = new LocationCharacter(agent,
                new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors),
                "musician", 
                true,
                LocationCharacter.CharacterRelations.Neutral, 
                ActionSetCode.GenerateActionSetNameWithSuffix(agent.AgentMonster, agent.AgentIsFemale, "_musician"),
                true, false, null, false, false, true);

            settlement.LocationComplex.GetLocationWithId("lordshall")
                .AddLocationCharacters(delegate { return locCharacter; },
                settlement.Culture, 
                LocationCharacter.CharacterRelations.Neutral, 1);

            var townsmanSuffix = FaceGen.GetMonsterWithSuffix(settlement.Culture.Townsman.Race, "_settlement");
            var tuple = new Tuple<string, Monster>(ActionSetCode.GenerateActionSetNameWithSuffix(townsmanSuffix, false, "_villager"), townsmanSuffix);
            var townsman = new LocationCharacter(new AgentData(
                new SimpleAgentOrigin(settlement.Culture.Townsman, -1, null, default(UniqueTroopDescriptor)))
                .Monster(tuple.Item2)
                .Age(MBRandom.RandomInt(30, 60)), 
                new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddOutdoorWandererBehaviors), 
                null, 
                false, 
                LocationCharacter.CharacterRelations.Friendly, 
                tuple.Item1, 
                true, false, null, false, false, true);

            settlement.LocationComplex.GetLocationWithId("lordshall")
               .AddLocationCharacters(delegate { return townsman; },
               settlement.Culture,
               LocationCharacter.CharacterRelations.Friendly, 10);
        }


        public static bool IsClanLeader(Hero hero)
        {
            return hero.Clan != null && hero.Clan.Leader == hero;
        }

        public static bool IsCloseFamily(Hero hero, Hero family)
        {
            return hero.Father == family || hero.Mother == family || hero.Children.Contains(family) ||
                   hero.Siblings.Contains(family) || hero.Spouse == family;
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
            string cultureId = culture.StringId;
            TextObject text;

            if (type == PopType.Serfs)
            {
                switch (cultureId) 
                {
                    case "empire":
                        text = new TextObject("{=!}Coloni");
                        break;
                    case "aserai" or "battania":
                        text = new TextObject("{=!}Commoners");
                        break;
                    default:
                        text = new TextObject("{=!}Serfs");
                        break;
                }
            }
            else if (type == PopType.Tenants)
            {
                switch (cultureId)
                {
                    case "khuzait":
                        text = new TextObject("{=!}Nomads");
                        break;
                    case "sturgia" or "battania":
                        text = new TextObject("{=!}Freemen");
                        break;
                    default:
                        text = new TextObject("{=!}Tenants");
                        break;
                }
            }
            else if (type == PopType.Slaves)
            {
                switch (cultureId)
                {
                    case "empire":
                        text = new TextObject("{=!}Sclavi");
                        break;
                    case "sturgia":
                        text = new TextObject("{=!}Thralls");
                        break;
                    case "aserai":
                        text = new TextObject("{=!}Mameluke");
                        break;
                    default:
                        text = new TextObject("{=!}Slaves");
                        break;
                }
            }
            else if (type == PopType.Craftsmen)
            {
                switch (cultureId)
                {
                    case "empire":
                        text = new TextObject("{=!}Cives");
                        break;
                    case "sturgia" or "battania" or "khuzait":
                        text = new TextObject("{=!}Artisans");
                        break;
                    default:
                        text = new TextObject("{=!}Craftsmen");
                        break;
                }
            }
            else
            {
                switch (cultureId)
                {
                    case "empire":
                        text = new TextObject("{=!}Nobiles");
                        break;
                    case "vlandia":
                        text = new TextObject("{=!}Ealdormen");
                        break;
                    default:
                        text = new TextObject("{=!}Nobles");
                        break;
                }
            }

            return text;
        }

        public static string GetGovernmentDescription(GovernmentType type)
        {
            var text = type switch
            {
                GovernmentType.Imperial => new TextObject("{=Z8ZfKuSX}An Imperial government is a highly centralized one. Policies favor the ruling clan at the expense of vassals. A strong leadership that sees it's vassals more as administrators than lords."),
                GovernmentType.Tribal => new TextObject("{=mWKXYs2o}The Tribal association is the most descentralized government. Policies to favor the ruling clan are unwelcome, and every lord is a 'king' or 'queen' in their own right."),
                GovernmentType.Republic => new TextObject("{=v3KydG7F}Republics are firmly setup to avoid the accumulation of power. Every clan is given a chance to rule, and though are able to have a few political advantages, the state is always the priority."),
                _ => new TextObject("{=3bJSgTAD}Feudal societies can be seen as the midway between tribals and imperials. Although the ruling clan accumulates privileges, and often cannot be easily removed from the throne, lords and their rightful property need to be respected.")
            };

            return text.ToString();
        }

        public static string GetSuccessionTypeDescription(SuccessionType type)
        {
            var text = type switch
            {
                SuccessionType.Elective_Monarchy => new TextObject("{=YSZYZZUw}In elective monarchies, the ruler is chosen from the realm's dynasties, and rules until death or abdication. Elections take place and all dynasties are able to vote when a new leader is required."),
                SuccessionType.Hereditary_Monarchy => new TextObject("{=9EjsMFJx}In hereditary monarchies, the monarch is always the ruling dynasty's leader. No election takes place, and the realm does not change leadership without extraordinary measures."),
                SuccessionType.Imperial => new TextObject("{=ag50C9hT}Imperial successions are completely dictated by the emperor/empress. They will choose from most competent members in their family, as well as other family leaders. Imperial succession values age, family prestigy, military and administration skills. No election takes place."),
                SuccessionType.FeudalElective => new TextObject("{=P3EzcNY0}A feudal elective succession is a compromise between a hereditary monarchy and a fully elective monarchy. Votes will be cast by the Peers, however the main candidates are the former ruler's family. However, title claimants will also be considered. If there is a scarcity of candidates available, strong families in the realm will also be considered candidates. If the chosen heir is from the former ruler's family, they will inherit the clan, regardless of Inheritance laws."),
                _ => new TextObject("{=ATmtkA1S}Republican successions ensure the power is never concentrated. Each year, a new ruler is chosen from the realm's dynasties. The previous ruler is strickly forbidden to participate. Age, family prestige and administration skills are sought after in candidates.")
            };

            return text.ToString();
        }

        public static string GetSuccessionTypeName(SuccessionType type)
        {
            var text = type switch
            {
                SuccessionType.Elective_Monarchy => new TextObject("{=WG9FTePW}Elective Monarchy"),
                SuccessionType.Hereditary_Monarchy => new TextObject("{=iYzZgP3y}Hereditary Monarchy"),
                SuccessionType.FeudalElective => new TextObject("{=HZRnRmF8}Feudal Elective"),
                SuccessionType.Imperial => new TextObject("{=SW29YLBZ}Imperial"),
                _ => new TextObject("{=vFXFxkM9}Republican")
            };

            return text.ToString();
        }

        public static string GetInheritanceDescription(InheritanceType type)
        {
            var text = type switch
            {
                InheritanceType.Primogeniture => new TextObject("{=NSWFGCd6}Primogeniture favors blood family of eldest age. Clan members not related by blood are last resort."),
                InheritanceType.Seniority => new TextObject("{=SiRpKHww}Seniority favors those of more advanced age in the clan, regardless of blood connections."),
                _ => new TextObject("{=aybPH14L}Ultimogeniture favors the youngest in the clan, as well as blood family. Clan members not related by blood are last resort.")
            };

            return text.ToString();
        }

        public static string GetGenderLawDescription(GenderLaw type)
        {
            return type == GenderLaw.Agnatic 
                ? new TextObject("{=EjVOGKj7}Agnatic law favors males. Although females are not completely excluded, they will only be chosen in case a male candidate is not present.").ToString() 
                : new TextObject("{=M0MP3ysP}Cognatic law sees no distinction between both genders. Candidates are choosen stricly on their merits, as per the context requires.").ToString();
        }

        public static string GetClassHint(PopType type, CultureObject culture)
        {
            var name = GetClassName(type, culture).ToString();
            TextObject description = type switch
            {
                PopType.Nobles => new TextObject("{=!}The {CLASS} represent the free, wealthy and influential members of society. They pay very high taxes and increase your influence as a lord or lady"),
                PopType.Craftsmen => new TextObject("{=!}The {CLASS} are free people of trade, such as merchants, engineers and blacksmiths. Somewhat wealthy, free but not high status people. Craftsmen pay a significant amount of taxes and their presence boosts economical development. Their skills can also be hired to significantly boost construction projects."),
                PopType.Serfs => new TextObject("{=!}The {CLASS} are the lowest class that possess some sort of freedom. Unable to attain specialized skills such as those of craftsmen, these people represent the agricultural workforce. They also pay tax over the profit of their production excess."),
                PopType.Tenants => new TextObject("{=!}The {CLASS} are a step above the serfs. These peasants are free to move and often have rights to protect themselves. Though less taxable than serfs, {CLASS} are more prone to stable and prosperous fiefs due to such rights and the ability to accumulate more wealth."),
                _ => new TextObject("{=!}The {CLASS} are those destituted: criminals, prisioners unworthy of a ransom, and those unlucky to be born into slavery. Slaves do the hard manual labor across settlements, such as building and mining. They themselves pay no tax as they are unable to have posessions, but their labor generates income gathered as tax from their masters.")
            };

            return description.SetTextVariable("CLASS", name).ToString();
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
                            ? new TextObject("{=25sz3WPn}Ard-Banrigh") 
                            : new TextObject("{=Tmq9K61V}{MALE}Ard-Rìgh{?}Queen{\\?}");

                        break;
                    }
                    case "battania" when type == TitleType.Dukedom:
                    {
                        title = female 
                            ? new TextObject("{=XLAgsQ0J}Banrigh")
                            : new TextObject("{=9UcU5h2m}{MALE}Rìgh{?}Queen{\\?}");

                        break;
                    }
                    case "battania" when type == TitleType.County:
                    {
                        title = female 
                            ? new TextObject("{=FMWNKESs}Bantiarna") 
                            : new TextObject("{=Ut4edjyP}{MALE}Mormaer{?}Queen{\\?}");

                        break;
                    }
                    case "battania" when type == TitleType.Barony:
                    {
                        title = female 
                            ? new TextObject("{=811sxLhn}Thaoiseach") 
                            : new TextObject("{=Fjzj45rw}{MALE}Toisiche{?}Queen{\\?}");

                        break;
                    }
                    case "battania" when female:
                        title = new TextObject("{=tD38koMS}Baintighearna");
                        break;
                    case "battania":
                        title = new TextObject("{=3TM8VVpZ}{MALE}Tighearna{?}Queen{\\?}");
                        break;
                    case "empire" when type == TitleType.Empire:
                    {
                        title = female
                            ? new TextObject("{=b3dURwfW}Imperatrix")
                            : new TextObject("{=uTLJjN8A}Imperator");

                        break;
                    }
                    case "empire" when type == TitleType.Kingdom:
                    {
                        if (government == GovernmentType.Republic)
                        {
                            title = female
                                ? new TextObject("{=gNVEqLz4}Principissa")
                                : new TextObject("{=ouHkQtyZ}Princeps");
                        }
                        else
                        {
                            title = female 
                                ? new TextObject("{=BJvarzpV}Regina") 
                                : new TextObject("{=pjye9jRu}{MALE}Rex{?}Queen{\\?}");
                        }

                        break;
                    }
                    case "empire" when type == TitleType.Dukedom:
                    {
                        title = female 
                            ? new TextObject("{=5aCrjmFi}Ducissa") 
                            : new TextObject("{=O9U0gmCL}{MALE}Dux{?}Queen{\\?}");

                        break;
                    }
                    case "empire" when type == TitleType.County:
                    {
                        title = female 
                            ? new TextObject("{=ex7NjOtr}Cometessa") 
                            : new TextObject("{=iAZGWV5E}{MALE}Conte{?}Queen{\\?}");

                        break;
                    }
                    case "empire" when type == TitleType.Barony:
                    {
                        title = female 
                            ? new TextObject("{=dYq0qGzZ}Baronessa") 
                            : new TextObject("{=CNNsfyzD}{MALE}Baro{?}Queen{\\?}");

                        break;
                    }
                    case "empire" when female:
                        title = new TextObject("{=go9MmDKr}Domina");
                        break;
                    case "empire":
                        title = new TextObject("{=rWJjN9N3}{MALE}Dominus{?}Queen{\\?}");
                        break;
                    case "aserai" when type == TitleType.Kingdom:
                    {
                        title = female 
                            ? new TextObject("{=DQXH6NeY}Sultana") 
                            : new TextObject("{=8hKXBBMu}{MALE}Sultan{?}Queen{\\?}");

                        break;
                    }
                    case "aserai" when type == TitleType.Dukedom:
                    {
                        title = female
                            ? new TextObject("{=MVjsWtcZ}Emira") 
                            : new TextObject("{=O8JU8PND}{MALE}Emir{?}Queen{\\?}");

                        break;
                    }
                    case "aserai" when type == TitleType.County:
                    {
                        title = female
                            ? new TextObject("{=AUoParHT}Shaykah") 
                            : new TextObject("{=KkOE3xqq}{MALE}Sheikh{?}Queen{\\?}");

                        break;
                    }
                    case "aserai" when type == TitleType.Barony:
                    {
                        title = female
                            ? new TextObject("{=LYY1ZegU}Walia") 
                            : new TextObject("{=y847ELZt}{MALE}Wali{?}Queen{\\?}");

                        break;
                    }
                    case "aserai" when female:
                        title = new TextObject("{=VGXsX6Ue}Beghum");
                        break;
                    case "aserai":
                        title = new TextObject("{=NQiveYfa}{MALE}Mawlaa{?}Queen{\\?}");
                        break;
                    case "khuzait" when type == TitleType.Kingdom:
                    {
                        title = female 
                            ? new TextObject("{=SdJk1Vpf}Khatun") 
                            : new TextObject("{=xTbcwLP8}{MALE}Khagan{?}Queen{\\?}");

                        break;
                    }
                    case "khuzait" when type == TitleType.Dukedom:
                    {
                        title = female 
                            ? new TextObject("{=Mfuxa8SP}Bekhi") 
                            : new TextObject("{=E3oJ6wzZ}{MALE}Baghatur{?}Queen{\\?}");

                        break;
                    }
                    case "khuzait" when type == TitleType.County:
                    {
                        title = female 
                            ? new TextObject("{=cepkCz19}Khanum") 
                            : new TextObject("{=c14De2tC}{MALE}Khan{?}Queen{\\?}");

                        break;
                    }
                    case "khuzait" when type == TitleType.Barony:
                    {
                        title = female 
                            ? new TextObject("{=Ajj9ptAU}Begum") 
                            : new TextObject("{=L4g7rfjB}{MALE}Bey{?}Queen{\\?}");

                        break;
                    }
                    case "khuzait" when female:
                        title = new TextObject("{=dB9Rfp6W}Khatagtai");
                        break;
                    case "khuzait":
                        title = new TextObject("{=ouUiDOuN}{MALE}Erxem{?}Queen{\\?}");
                        break;
                    case "sturgia" when type == TitleType.Kingdom:
                    {
                        title = female 
                            ? new TextObject("{=LmHWN0vt}Velikaya Knyaginya") 
                            : new TextObject("{=eVZHT4T6}{MALE}Velikiy Knyaz{?}Queen{\\?}");

                        break;
                    }
                    case "sturgia" when type == TitleType.Dukedom:
                    {
                        title = female 
                            ? new TextObject("{=S3kc2bhW}Knyaginya") 
                            : new TextObject("{=1XDPfDim}Knyaz");

                        break;
                    }
                    case "sturgia" when type == TitleType.County:
                    {
                        title = female 
                            ? new TextObject("{=KTxFYNyo}Boyarina") 
                            : new TextObject("{=QQxLPzDu}{MALE}Boyar{?}Queen{\\?}");

                        break;
                    }
                    case "sturgia" when type == TitleType.Barony:
                    {
                        title = female 
                            ? new TextObject("{=bYQLoRUt}Voivodina") 
                            : new TextObject("{=DvtAe1VC}{MALE}Voivode{?}Queen{\\?}");

                        break;
                    }
                    case "sturgia" when female:
                        title = new TextObject("{=Ec79HNiF}Gospoda");
                        break;
                    case "sturgia":
                        title = new TextObject("{=E6MmoXHy}{MALE}Gospodin{?}Queen{\\?}");
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
                    title = new TextObject("{=JmdALFU2}Queen");
                    break;
                case TitleType.Kingdom:
                    title = new TextObject("{=9DBndyJZ}{MALE}King{?}Queen{\\?}");
                    break;
                case TitleType.Dukedom when female:
                    title = new TextObject("{=5uFw1EmO}Duchess");
                    break;
                case TitleType.Dukedom:
                    title = new TextObject("{=zLFrEtFr}{MALE}Duke{?}Duchess{\\?}");
                    break;
                case TitleType.County when female:
                    title = new TextObject("{=o513XU29}Countess");
                    break;
                case TitleType.County:
                    title = new TextObject("{=JFAEL6mR}{MALE}Count{?}Countess{\\?}");
                    break;
                case TitleType.Barony when female:
                    title = new TextObject("{=yxq4RV7E}Baroness");
                    break;
                case TitleType.Barony:
                    title = new TextObject("{=9PL99nxQ}{MALE}Baron{?}Baroness{\\?}");
                    break;
                case TitleType.Empire:
                case TitleType.Lordship:
                default:
                {
                    title = female 
                        ? new TextObject("{=8V8i6QCm}Lady")
                        : new TextObject("{=2FowDhD3}{MALE}Lord{?}Lady{\\?}");

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
                    title = new TextObject("{=jz2SCLZS}Grand-Principality");
                }
            }

            if (title == null)
            {
                title = type switch
                {
                    GovernmentType.Feudal => new TextObject("{=7x3HJ29f}Kingdom"),
                    GovernmentType.Tribal => new TextObject("{=SuG07DUi}High Kingship"),
                    GovernmentType.Imperial => new TextObject("{=uEBLsMAb}Empire"),
                    _ => new TextObject("{=MSaLufNx}Republic")
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
                        title = new TextObject("{=jz2SCLZS}Grand-Principality");
                        break;
                    case "sturgia" when type == TitleType.Dukedom:
                        title = new TextObject("{=5rmKW4c9}Principality");
                        break;
                    case "sturgia" when type == TitleType.County:
                        title = new TextObject("{=GHeUbN6f}Boyardom");
                        break;
                    case "sturgia" when type == TitleType.Barony:
                        title = new TextObject("{=eUi8JOkv}Voivodeship");
                        break;
                    case "sturgia":
                        title = new TextObject("{=wc51byvw}Gospodin");
                        break;
                    case "aserai" when type == TitleType.Kingdom:
                        title = new TextObject("{=DQXH6NeY}Sultanate");
                        break;
                    case "aserai" when type == TitleType.Dukedom:
                        title = new TextObject("{=MVjsWtcZ}Emirate");
                        break;
                    case "aserai":
                    {
                        if (type == TitleType.County)
                        {
                            title = new TextObject("{=V4ra7Por}Sheikhdom");
                        }

                        break;
                    }
                    case "empire":
                    {
                        if (type == TitleType.Empire)
                        {
                                title = new TextObject("{=dSSX7xRm}Imperium");
                        }
                        break;
                    }
                    case "battania":
                    {
                        if (government == GovernmentType.Tribal)
                        {
                            title = type switch
                            {
                                TitleType.Kingdom => new TextObject("{=F0Y49kiT}High-Kingdom"),
                                TitleType.Dukedom => new TextObject("{=XsyHSqDV}Petty Kingdom"),
                                _ => title
                            };
                        }

                        break;
                    }
                }
            }

            title ??= type switch
            {
                TitleType.Empire => new TextObject("Empire"),
                TitleType.Kingdom => new TextObject("{=7x3HJ29f}Kingdom"),
                TitleType.Dukedom => new TextObject("{=HtWGKBDF}Dukedom"),
                TitleType.County => new TextObject("{=c6ggHVzS}County"),
                TitleType.Barony => new TextObject("{=qOLmvS0B}Barony"),
                _ => new TextObject("{=dwMA32rq}Lordship")
            };


            return title.ToString();
        }

        public static bool IsRetinueTroop(CharacterObject character)
        {
            var nobleRecruit = character?.Culture?.EliteBasicTroop;
            bool result = false;

            if (nobleRecruit == null || nobleRecruit.UpgradeTargets == null)
            {
                return false;
            }

            ExceptionUtils.TryCatch(() =>
            {
                while (nobleRecruit.UpgradeTargets != null && nobleRecruit.UpgradeTargets.Count() > 0)
                {
                    result = character == nobleRecruit || nobleRecruit.UpgradeTargets.Contains(character);
                    if (result)
                    {
                        break;
                    }
                    else
                    {
                        nobleRecruit = nobleRecruit.UpgradeTargets[0];
                    }
                }
            }, 
            Type.GetType("BannerKings.Utils.Helpers").Name,
            false);

            return result;
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