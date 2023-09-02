using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using BannerKings.Managers.Titles;
using BannerKings.Managers.Traits;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using static BannerKings.Managers.PopulationManager;
using static TaleWorlds.Core.ItemCategory;

namespace BannerKings.Utils
{
    public static class Helpers
    {
        public static void AddTraitLevel(Hero hero, TraitObject trait, int level, float chance = 1f)
        {
            int current = hero.GetTraitLevel(trait);
            int final = current + level;
            if ((final >= trait.MinValue || final <= trait.MaxValue) && MBRandom.RandomFloat < chance)
            {
                hero.SetTraitLevel(trait, final);
                if (hero == Hero.MainHero)
                {
                    string value = GameTexts.FindText("str_trait_name_" + trait.StringId.ToLower(), 
                        (level + MathF.Abs(trait.MinValue)).ToString())
                        .ToString();

                    if (BKTraits.Instance.PersonalityTraits.Contains(trait))
                    {
                        InformationManager.DisplayMessage(new InformationMessage(
                            new TextObject("{=fmoNZHeO}Your sense of {TRAIT} is now perceived by others as {LEVEL}.")
                            .SetTextVariable("TRAIT", trait.Name)
                            .SetTextVariable("LEVEL", value).ToString(),
                            Color.UIntToColorString(TextHelper.COLOR_LIGHT_YELLOW)));
                    }
                    else if (BKTraits.Instance.MedicalTraits.Contains(trait))
                    {
                        InformationManager.DisplayMessage(new InformationMessage(
                            new TextObject("{=fmoNZHeO}Your sense of {TRAIT} is now perceived by others as {LEVEL}.")
                            .SetTextVariable("TRAIT", trait.Name)
                            .SetTextVariable("LEVEL", value).ToString(),
                            Color.UIntToColorString(TextHelper.COLOR_LIGHT_YELLOW)));
                    }
                }
            }
        }

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
                        text = new TextObject("{=5Ym25L00}Servi");
                        break;
                    case "aserai" or "battania":
                        text = new TextObject("{=vSMPBzue}Commoners");
                        break;
                    case "sturgia":
                        text = new TextObject("{=f9tsDXWf}Kholops");
                        break;
                    default:
                        text = new TextObject("Serfs");
                        break;
                }
            }
            else if (type == PopType.Tenants)
            {
                switch (cultureId)
                {
                    case "empire":
                        text = new TextObject("{=GThkJp2s}Coloni");
                        break;
                    case "khuzait":
                        text = new TextObject("{=tUzhQHAh}Nomads");
                        break;
                    case "sturgia":
                        text = new TextObject("{=VviUbJTS}Smerdy");
                        break;
                    case "battania":
                        text = new TextObject("{=TEYb57Wo}Freemen");
                        break;
                    default:
                        text = new TextObject("{=h9UDWQcM}Tenants");
                        break;
                }
            }
            else if (type == PopType.Slaves)
            {
                switch (cultureId)
                {
                    case "empire":
                        text = new TextObject("{=B9hAxxuo}Sclavi");
                        break;
                    case "sturgia":
                        text = new TextObject("{=j6UDXO39}Thralls");
                        break;
                    case "aserai":
                        text = new TextObject("{=TASERbwx}Mameluke");
                        break;
                    default:
                        text = new TextObject("Slaves");
                        break;
                }
            }
            else if (type == PopType.Craftsmen)
            {
                switch (cultureId)
                {
                    case "empire":
                        text = new TextObject("{=6hrBerHd}Cives");
                        break;
                    case "sturgia" or "battania" or "khuzait":
                        text = new TextObject("{=2ogRjAuf}Artisans");
                        break;
                    default:
                        text = new TextObject("Craftsmen");
                        break;
                }
            }
            else
            {
                switch (cultureId)
                {
                    case "vlandia":
                        text = new TextObject("{=FVuW8Y4j}Ealdormen");
                        break;
                    default:
                        text = new TextObject("Nobles");
                        break;
                }
            }

            return text;
        }

        public static string GetConsumptionHint(ConsumptionType type)
        {
            return type switch
            {
                ConsumptionType.Luxury => new TextObject("{=2wwjFQ2A}Satisfaction over availability of products such as jewelry, velvets and fur.").ToString(),
                ConsumptionType.Industrial => new TextObject("{=irOAqrdy}Satisfaction over availability of manufacturing products such as leather, clay and tools.").ToString(),
                ConsumptionType.General => new TextObject("{=NENnF6oJ}Satisfaction over availability of various products, including military equipment and horses.").ToString(),
                _ => new TextObject("{=QJ1pjKxw}Satisfaction over availability of food types.").ToString()
            };
        }
        public static string GetTitlePrefix(TitleType type, CultureObject culture = null)
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
                    /*case "battania":
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
                    }*/
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