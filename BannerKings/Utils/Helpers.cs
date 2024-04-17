using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using BannerKings.Managers.Cultures;
using BannerKings.Managers.Items;
using BannerKings.Managers.Traits;
using BannerKings.Settings;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Party;
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
        internal static XmlDocument CreateDocumentFromXmlFile(string xmlPath)
        {
            var xmlDocument = new XmlDocument();
            var streamReader = new StreamReader(xmlPath);
            var xml = streamReader.ReadToEnd();
            xmlDocument.LoadXml(xml);
            streamReader.Close();
            return xmlDocument;
        }

        public static void ApplyFeat(FeatObject feat, PartyBase party, ref ExplainedNumber result)
        {
            if (PartyBaseHelper.HasFeat(party, feat))
            {
                if (feat.IncrementType == FeatObject.AdditionType.Add)
                    result.Add(feat.EffectBonus, GameTexts.FindText("str_culture"));
                else result.AddFactor(feat.EffectBonus, GameTexts.FindText("str_culture"));
            }
        }

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

        public static TextObject GetClassName(PopType type, CultureObject culture) =>
            DefaultPopulationNames.Instance.GetPopulationName(culture, type).Name;

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

        public static IEnumerable<CharacterObject> GetAllPartyHeros(this MobileParty mobileParty)
        {
            Hero leader = mobileParty.LeaderHero ?? mobileParty.Owner;
            if (leader != null)
            {

                for (var i = 0; i < mobileParty.MemberRoster.Count; i++)
                {
                    var elementCopyAtIndex = mobileParty.MemberRoster.GetElementCopyAtIndex(i);
                    if (elementCopyAtIndex.Character.IsHero)
                    {
                        yield return elementCopyAtIndex.Character;
                    }
                }
            }
        }

        public static List<ItemCategory> LuxuryCategories => new List<ItemCategory>(10)
            {
                DefaultItemCategories.Jewelry,
                DefaultItemCategories.Velvet,
                DefaultItemCategories.WarHorse,
                DefaultItemCategories.RangedWeapons4,
                DefaultItemCategories.MeleeWeapons4,
                DefaultItemCategories.HorseEquipment4,
                DefaultItemCategories.RangedWeapons5,
                DefaultItemCategories.MeleeWeapons5,
                DefaultItemCategories.HorseEquipment5,
                BKItemCategories.Instance.Spice,
                BKItemCategories.Instance.Marble
            };

        public static List<ItemCategory> IndustrialCategories => new List<ItemCategory>(15)
            {
                DefaultItemCategories.Pottery,
                DefaultItemCategories.Linen,
                DefaultItemCategories.Wood,
                DefaultItemCategories.Leather,
                DefaultItemCategories.Oil,
                DefaultItemCategories.RangedWeapons3,
                DefaultItemCategories.MeleeWeapons3,
                DefaultItemCategories.HorseEquipment3,
                DefaultItemCategories.PackAnimal,
                DefaultItemCategories.Fur,
                BKItemCategories.Instance.Ink,
                BKItemCategories.Instance.Limestone,
                BKItemCategories.Instance.Papyrus
            };

        public static ConsumptionType GetTradeGoodConsumptionType(ItemCategory item)
        {
            if (LuxuryCategories.Contains(item)) return ConsumptionType.Luxury;
            else if (IndustrialCategories.Contains(item)) return ConsumptionType.Industrial;
            else if (item.Properties == Property.BonusToFoodStores) return ConsumptionType.Food;
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

        #region PerkHelpers
        public static void AddScaledGovernerPerkBonusForTownWithTownHeros(this PerkObject perk, ref ExplainedNumber bonuses, Town town, SkillObject scaleSkill, float everySkillGoverner, float everySkillOwner, float everySkillMember, float? minValue = null, float? maxValue = null)
        {
            float value = 0;
            if (!(perk.PrimaryRole == SkillEffect.PerkRole.Governor || perk.SecondaryRole == SkillEffect.PerkRole.Governor))
            {
                return;
            }
            var perkbouns = perk.PrimaryRole == SkillEffect.PerkRole.Governor ? perk.PrimaryBonus : perk.SecondaryBonus;
            Hero governor = town.Governor;
            if (BannerKingsSettings.Instance.EnableUsefulPerksFromAllPartyMembers && town.OwnerClan != null && everySkillMember > 0)
            {
                var garrisonHeros = town.Settlement.HeroesWithoutParty.Where(d => d.Clan == town.OwnerClan && d.GetPerkValue(perk) && d != town.Governor && !d.IsClanLeader);
                if (garrisonHeros.Any())
                {
                    value += garrisonHeros.Sum(d => perkbouns * (d.GetSkillValue(scaleSkill) / everySkillMember));
                }
            }

            if (BannerKingsSettings.Instance.EnableUsefulGovernorPerksFromSettlementOwner && town.OwnerClan?.Leader != null && town.OwnerClan.Leader.GetPerkValue(perk))
            {
                if (governor == null && town.OwnerClan.Leader.CurrentSettlement != null && town.OwnerClan.Leader.CurrentSettlement == town.Settlement)
                {
                    if (everySkillGoverner > 0)
                    {
                        value += perkbouns * (town.OwnerClan.Leader.GetSkillValue(scaleSkill) / everySkillGoverner);
                    }
                }
                else
                {
                    if (everySkillOwner > 0)
                    {
                        value += perkbouns * (town.OwnerClan.Leader.GetSkillValue(scaleSkill) / everySkillOwner);
                    }
                }
            }
            if (governor != null && governor.GetPerkValue(perk) && governor.CurrentSettlement != null && governor.CurrentSettlement == town.Settlement && everySkillGoverner > 0)
            {
                value += perkbouns * (town.Governor.GetSkillValue(scaleSkill) / everySkillGoverner);
            }
            if (!value.ApproximatelyEqualsTo(0f))
            {
                if (minValue.HasValue && value < minValue.Value)
                {
                    value = minValue.Value;
                }
                if (maxValue.HasValue && value > maxValue.Value)
                {
                    value = maxValue.Value;
                }
                if (perk.PrimaryRole == SkillEffect.PerkRole.Governor)
                {
                    AddToStat(ref bonuses, perk.PrimaryIncrementType, value, perk.Name);
                }
                else
                {
                    AddToStat(ref bonuses, perk.SecondaryIncrementType, value, perk.Name);
                }
            }
        }

        public enum SkillScale
        {
            None,
            OnlyQuartermaster,
            OnlyPartyLeader,
            QuartermasterFirst,
            PartyLeaderFirst,
            TheGreater,
            Both
        }
        public static void AddScaledPerkBonus(this PerkObject perk, ref ExplainedNumber bonuses, bool isSecondary, MobileParty mobileParty, SkillObject scaleSkill, float everySkillLeader, float everySkillQuartermaster, float everySkillMember, SkillScale skillScale, float? minValue = null, float? maxValue = null)
        {
            var value = 0f;
            var perkbouns = isSecondary ? perk.SecondaryBonus : perk.PrimaryBonus;
            var perkRole = isSecondary ? perk.SecondaryRole : perk.PrimaryRole;

            Hero choosenHero = null;

            if (skillScale != SkillScale.None)
            {
                float? choosenValue = null;
                if (everySkillQuartermaster > 0 && skillScale != SkillScale.OnlyPartyLeader &&
                    (skillScale == SkillScale.Both ||
                    skillScale == SkillScale.TheGreater ||
                    skillScale == SkillScale.QuartermasterFirst ||
                    skillScale == SkillScale.PartyLeaderFirst ||
                    (perkRole == SkillEffect.PerkRole.Quartermaster && skillScale == SkillScale.OnlyQuartermaster))
                )
                {
                    choosenHero = mobileParty.EffectiveQuartermaster;
                    if (choosenHero != null && choosenHero.GetPerkValue(perk))
                    {

                        choosenValue = perkbouns * choosenHero.GetSkillValue(scaleSkill) / everySkillQuartermaster;

                    }
                }

                if (everySkillLeader > 0 && skillScale != SkillScale.OnlyQuartermaster &&
                    (skillScale == SkillScale.Both ||
                    skillScale == SkillScale.TheGreater ||
                    skillScale == SkillScale.QuartermasterFirst ||
                    skillScale == SkillScale.PartyLeaderFirst ||
                    (perkRole == SkillEffect.PerkRole.PartyLeader && skillScale == SkillScale.OnlyPartyLeader))
                )
                {
                    choosenHero = mobileParty.LeaderHero;
                    if (choosenHero != null && choosenHero.GetPerkValue(perk))
                    {

                        var newvalue = perkbouns * choosenHero.GetSkillValue(scaleSkill) / everySkillLeader;
                        if (skillScale == SkillScale.QuartermasterFirst && !choosenValue.HasValue)
                        {
                            choosenValue = newvalue;
                        }
                        else if (skillScale == SkillScale.TheGreater && newvalue > choosenValue)
                        {
                            choosenValue = newvalue;
                        }
                        else if (skillScale == SkillScale.Both && mobileParty.LeaderHero != mobileParty.EffectiveQuartermaster)
                        {
                            if (!choosenValue.HasValue)
                            {
                                choosenValue = newvalue;
                            }
                            else
                            {
                                choosenValue += newvalue;
                            }
                        }
                        else if (skillScale == SkillScale.OnlyPartyLeader || skillScale == SkillScale.PartyLeaderFirst)
                        {
                            choosenValue = newvalue;
                        }


                    }
                }
                if (choosenValue.HasValue)
                {
                    value += choosenValue.Value;
                }

            }
            if (BannerKingsSettings.Instance.EnableUsefulPerksFromAllPartyMembers && everySkillMember > 0)
            {
                var mobilePartyHeros = mobileParty.GetAllPartyHeros();
                var partyHeros = mobilePartyHeros.Where(d => d.GetPerkValue(perk) && d.HeroObject != choosenHero);
                value += partyHeros.Sum(d => perkbouns * (d.GetSkillValue(scaleSkill) / everySkillMember));
            }

            if (!value.ApproximatelyEqualsTo(0f))
            {
                if (minValue.HasValue && value < minValue.Value)
                {
                    value = minValue.Value;
                }
                if (maxValue.HasValue && value > maxValue.Value)
                {
                    value = maxValue.Value;
                }

                if (isSecondary)
                {
                    AddToStat(ref bonuses, perk.SecondaryIncrementType, value, perk.Name);
                }
                else
                {
                    AddToStat(ref bonuses, perk.PrimaryIncrementType, value, perk.Name);
                }
            }
        }
        public static float AddScaledPersonlOrClanLeaderPerkBonusWithClanAndFamilyMembers(this PerkObject perk, ref ExplainedNumber bonuses, bool isSecondary, Hero person, SkillObject scaleSkill, float everySkillPerson, float everySkillFamilyMembers, float everySkillClanMembers, float? minValue = null, float? maxValue = null)
        {
            if (person == null)
            {
                return 0f;
            }
            var value = 0f;
            var perkbouns = isSecondary ? perk.SecondaryBonus : perk.PrimaryBonus;
            var perkRole = isSecondary ? perk.SecondaryRole : perk.PrimaryRole;

            if (BannerKingsSettings.Instance.EnableUsefulPerksFromAllPartyMembers)
            {
                var familyMembers = new List<Hero>() { person.Father, person.Mother, person.Spouse };
                if (person.Siblings != null)
                {
                    familyMembers.AddRange(person.Siblings);
                }
                if (person.Children != null)
                {
                    familyMembers.AddRange(person.Children);
                }

                familyMembers = familyMembers.Where(d => d != null && d.IsAlive).ToList();

                List<Hero> otherClanMembers = (person.Clan?.Companions) ?? new List<Hero>();
                otherClanMembers = otherClanMembers.Union((person.Clan?.Lords) ?? new List<Hero>()).ToList();
                otherClanMembers = otherClanMembers.Where(d => d.IsAlive && !familyMembers.Contains(d)).ToList();

                if (everySkillFamilyMembers > 0)
                {
                    familyMembers = familyMembers.Where(d => d.GetPerkValue(perk) && d != person).ToList();
                    value += familyMembers.Sum(d => perkbouns * (d.GetSkillValue(scaleSkill) / everySkillFamilyMembers));
                }

                if (everySkillClanMembers > 0)
                {
                    otherClanMembers = otherClanMembers.Where(d => d.GetPerkValue(perk) && d != person).ToList();
                    value += otherClanMembers.Sum(d => perkbouns * (d.GetSkillValue(scaleSkill) / everySkillClanMembers));
                }
            }
            if (everySkillPerson > 0 && person.GetPerkValue(perk))
            {
                value += perkbouns * person.GetSkillValue(scaleSkill) / everySkillPerson;
            }


            if (!value.ApproximatelyEqualsTo(0f))
            {
                if (minValue.HasValue && value < minValue.Value)
                {
                    value = minValue.Value;
                }
                if (maxValue.HasValue && value > maxValue.Value)
                {
                    value = maxValue.Value;
                }

                if (isSecondary)
                {
                    AddToStat(ref bonuses, perk.SecondaryIncrementType, value, perk.Name);
                    return value;
                }
                else
                {
                    AddToStat(ref bonuses, perk.PrimaryIncrementType, value, perk.Name);
                    return value;
                }
            }
            return 0f;
        }
        private static void AddToStat(ref ExplainedNumber stat, SkillEffect.EffectIncrementType effectIncrementType, float number, TextObject text)
        {
            switch (effectIncrementType)
            {
                case SkillEffect.EffectIncrementType.Add:
                    stat.Add(number, text);
                    break;
                case SkillEffect.EffectIncrementType.AddFactor:
                    stat.AddFactor(number, text);
                    break;
            }
        }
        #endregion
    }
}