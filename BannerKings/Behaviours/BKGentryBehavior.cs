using BannerKings.Actions;
using BannerKings.Managers.Populations.Estates;
using BannerKings.Utils;
using HarmonyLib;
using Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Behaviours
{
    public class BKGentryBehavior : CampaignBehaviorBase
    {
        private Dictionary<Hero, CampaignTime> heroRecords = new Dictionary<Hero, CampaignTime>();
        public override void RegisterEvents()
        {
            CampaignEvents.OnNewGameCreatedPartialFollowUpEndEvent.AddNonSerializedListener(this, OnGameCreatedFollowUp);
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
            CampaignEvents.DailyTickClanEvent.AddNonSerializedListener(this, OnClanDailyTick);
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void OnGameCreatedFollowUp(CampaignGameStarter starter)
        {
            foreach (Settlement settlement in Settlement.All)
            {
                InitializeGentry(settlement, true);
            }
        }

        private void OnClanDailyTick(Clan clan)
        {
            ExceptionUtils.TryCatch(() =>
            {
                if (clan.IsBanditFaction || clan.IsUnderMercenaryService || clan.Kingdom == null || clan == Clan.PlayerClan)
                {
                    return;
                }

                (bool, Estate) gentryTuple = IsGentryClan(clan);
                if (!gentryTuple.Item1 || gentryTuple.Item2 == null)
                {
                    return;
                }
           
                Kingdom kingdom = clan.Kingdom;
                bool war = FactionManager.GetEnemyKingdoms(kingdom).Count() > 0;
                if (!war)
                {
                    if (clan.WarPartyComponents.Count > 0)
                    {
                        var estate = gentryTuple.Item2;
                        var list = new List<WarPartyComponent>();
                        foreach (var party in clan.WarPartyComponents)
                        {
                            list.Add(party);
                        }

                        foreach (var party in list)
                        {
                            FinishParty(party, estate);
                        }
                    }
                }
                else
                {
                    if (clan.WarPartyComponents.Count > 0)
                    {
                        var estate = gentryTuple.Item2;
                        var list = new List<WarPartyComponent>();
                        foreach (var party in clan.WarPartyComponents)
                        {
                            if (party.Leader != null && party.MobileParty.Army == null)
                            {
                                list.Add(party);
                            }
                        }

                        foreach (var party in list)
                        {
                            FinishParty(party, estate);
                        }
                    }
                }
            },
            GetType().Name);
        }

        private void FinishParty(WarPartyComponent party, Estate estate)
        {
            estate.AddPopulation(PopType.Serfs, party.MobileParty.MemberRoster.TotalRegulars);
            estate.AddManpower(PopType.Serfs, party.MobileParty.MemberRoster.TotalRegulars);

            estate.AddPopulation(PopType.Slaves, party.MobileParty.PrisonRoster.TotalRegulars);
            DestroyPartyAction.Apply(null, party.MobileParty);
        }

        public (bool, Estate) IsGentryClan(Clan clan)
        {
            bool isGentry = false;
            Estate estate = null;

            if (clan.Fiefs.Count > 0)
            {
                return new(false, null);
            }

            var estates = BannerKingsConfig.Instance.PopulationManager.GetEstates(clan.Leader);
            if (estates.Count > 0)
            {
                estate = estates[0];
            }

            var court = BannerKingsConfig.Instance.CourtManager.GetCouncil(clan);
            if (court != null && court.Peerage != null)
            {
                isGentry = court.Peerage.IsLesserPeerage;
            }

            return new(isGentry, estate);
        }

        public void SummonGentry(Clan clan, Army army, Estate estate)
        {
            (bool, Estate) isGentry = IsGentryClan(clan);
            if (isGentry.Item1 && estate == isGentry.Item2)
            {
                MobileParty party = MobilePartyHelper.SpawnLordParty(clan.Leader, estate.EstatesData.Settlement);
                SetPartyAiAction.GetActionForEscortingParty(party, army.LeaderParty);
            }
        }

        public bool IsAvailableForSummoning(Clan clan, Estate estate)
        {
            Hero leader = clan.Leader;
            bool leaderReady = leader.IsAlive && !leader.IsChild &&
                leader.PartyBelongedTo == null && !leader.IsPrisoner && !leader.IsNoncombatant;
            var settlement = estate.EstatesData.Settlement;
            bool villageReady = settlement.Village.VillageState == Village.VillageStates.Normal;

            return leaderReady && villageReady && estate.GetManpower(PopType.Serfs) >= 10;
        }

        private void OnDailyTick()
        {
            foreach (var kingdom in Kingdom.All)
            {
                if (MBRandom.RandomFloat <= 1f)
                {
                    InitializeGentry(kingdom.Settlements.GetRandomElementWithPredicate(x => x.IsVillage), false);
                }
            }
        }

        private void InitializeGentry(Settlement settlement, bool campaignStart = false)
        {
            if (!settlement.IsVillage || BannerKingsConfig.Instance.PopulationManager == null ||
                settlement.OwnerClan == null || !settlement.MapFaction.IsKingdomFaction)
            {
                return;
            }

            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
            if (data == null || data.EstateData == null)
            {
                return;
            }

            Estate vacantEstate = data.EstateData.Estates.FirstOrDefault(x => x.IsDisabled);
            if (vacantEstate == null)
            {
                return;
            }

            if (campaignStart && MBRandom.RandomFloat < 0.4f)
            {
                return;
            }

            Equipment equipment = GetEquipmentIfPossible(settlement.Culture);
            if (equipment == null)
            {
                return;
            }
            var clanName = ClanActions.CanCreateNewClan(settlement.Culture, settlement);
            if (clanName == null)
            {
                return;
            }

            string name = clanName.ToString();
            var applicable = FactionHelper.IsClanNameApplicable(name);
            if (!applicable.Item1)
            {
                return;
            }

            var template = GetTemplate(settlement.Culture);
            if (template == null)
            {
                return;
            }

            int cost = (int)(vacantEstate.EstateValue.ResultNumber * 0.5f);
            if (settlement.Owner == Hero.MainHero)
            {
                InformationManager.ShowInquiry(new InquiryData(new TextObject("{=!}The Gentry of {SETTLEMENT}")
                    .SetTextVariable("SETTLEMENT", settlement.Name)
                    .ToString(),
                    new TextObject("{=!}A local of relative wealth has offered to buy a vacant estate in your domain, {SETTLEMENT}. They offer {GOLD}{GOLD_ICON} for the property, as well as their vassalage. As their suzerain you would be entitled taxing their estate and calling them to war.")
                    .SetTextVariable("SETTLEMENT", settlement.Name)
                    .ToString(),
                    true,
                    true,
                    GameTexts.FindText("str_accept").ToString(),
                    GameTexts.FindText("str_reject").ToString(),
                    () => CreateGentryClan(settlement, vacantEstate, clanName, equipment, campaignStart, cost, template),
                    null));
            }
            else
            {
                CreateGentryClan(settlement, vacantEstate, clanName, equipment, campaignStart, cost, template);
            }
        }

        private CharacterObject GetTemplate(CultureObject culture, bool spouse = false)
        {
            var templates = CharacterObject.All.ToList().FindAll(x =>
                x.Occupation == Occupation.Lord && x.StringId.Contains("bannerkings_gentry_") && (spouse ? x.IsFemale : !x.IsFemale)
                && x.Culture == culture);
            return templates.GetRandomElement();
        }

        private void CreateGentryClan(Settlement settlement, Estate vacantEstate, TextObject clanName, Equipment equipment,
            bool campaignStart, int cost, CharacterObject template)
        {
            var hero = HeroCreator.CreateSpecialHero(template,
                settlement,
                null,
                null,
                MBRandom.RandomInt(25, 65));

            EquipmentHelper.AssignHeroEquipmentFromEquipment(hero, equipment);
            List<Hero> family = CreateFamily(hero, settlement);
            Clan clan = ClanActions.CreateNewClan(hero,
                settlement,
                $"gentryClan_{settlement.Name}_{settlement.Culture}",
                clanName,
                MBRandom.RandomFloatRanged(50f, 200f));
            if (clan != null)
            {
                foreach (var member in family)
                {
                    if (member.Clan == null)
                    {
                        member.Clan = clan;
                    }
                }
                Kingdom kingdom = settlement.MapFaction as Kingdom;
                ChangeKingdomAction.ApplyByJoinToKingdom(clan, kingdom, false);
                if (campaignStart)
                {
                    EstateAction action = BannerKingsConfig.Instance.EstatesModel.GetGrant(vacantEstate, settlement.Owner, hero);
                    action.TakeAction();
                }
                else
                {
                    hero.ChangeHeroGold(1000 + cost);
                    GiveGoldAction.ApplyBetweenCharacters(hero, vacantEstate.Owner, cost);
                    vacantEstate.SetOwner(hero);
                }
            }
            else
            {
                KillCharacterAction.ApplyByRemove(hero);
                foreach (var member in family)
                {
                    KillCharacterAction.ApplyByRemove(member);
                }
            }
        }

        private List<Hero> CreateFamily(Hero leader, Settlement settlement)
        {
            List<Hero> list = new List<Hero>();
            var culture = leader.Culture;
            bool femaleSpouse = !leader.IsFemale;
            float ageDifference = MBRandom.RandomFloatRanged(0f, 7f);

            var template = GetTemplate(settlement.Culture, true);
            var spouse = HeroCreator.CreateSpecialHero(template,
                settlement,
                null,
                null,
                (int)(femaleSpouse ? leader.Age - ageDifference : leader.Age + ageDifference));
            EquipmentHelper.AssignHeroEquipmentFromEquipment(spouse, template.Equipment);
            list.Add(spouse);
            leader.Spouse = spouse;

            int femaleAge = (int)(spouse.IsFemale ? spouse.Age : leader.Age);
            int fertilityYears = femaleAge - 18;
            int maxChildAge = fertilityYears;
            int minChildAge = MathF.Max(0, femaleAge - 45);
            if (fertilityYears > 27)
            {
                fertilityYears = 27;
            }

            int childrenQuantity = MBRandom.RandomInt(0, (int)(fertilityYears / 3f));
            for (int i = 0; i < childrenQuantity; i++)
            {
                bool female = MBRandom.RandomFloat <= Campaign.Current.Models.PregnancyModel.DeliveringFemaleOffspringProbability;
                Equipment childEquipment = GetEquipmentIfPossible(culture, true, female);
                if (childEquipment == null)
                {
                    continue;
                }

                int age = MBRandom.RandomInt(minChildAge, maxChildAge);
                var child = DeliverOffSpring(leader.IsFemale ? leader : spouse,
                leader.IsFemale ? spouse : leader,
                    female,
                    settlement,
                    age
                    );
                list.Add(child);
            }

            return list;
        }

        public static Hero DeliverOffSpring(Hero mother, Hero father, bool isOffspringFemale, Settlement settlement, int age)
        {
            CultureObject culture = settlement.Culture;
            var method = typeof(HeroCreator).GetMethod("CreateNewHero", System.Reflection.BindingFlags.Static |
                BindingFlags.NonPublic);
            Hero hero = (Hero) method.Invoke(null, new object[] { isOffspringFemale ? mother.CharacterObject : father.CharacterObject, age });
            hero.SetNewOccupation(Occupation.Lord);
            hero.CharacterObject.IsFemale = isOffspringFemale;
            hero.Mother = mother;
            hero.Father = father;
            EquipmentFlags customFlags = EquipmentFlags.IsNobleTemplate | EquipmentFlags.IsChildEquipmentTemplate;
            MBEquipmentRoster randomElementInefficiently = MBEquipmentRosterExtensions.GetAppropriateEquipmentRostersForHero(hero, customFlags, true).GetRandomElementInefficiently<MBEquipmentRoster>();
            if (randomElementInefficiently != null)
            {
                Equipment randomElementInefficiently2 = randomElementInefficiently.GetCivilianEquipments().GetRandomElementInefficiently<Equipment>();
                EquipmentHelper.AssignHeroEquipmentFromEquipment(hero, randomElementInefficiently2);
                Equipment equipment = new Equipment(false);
                equipment.FillFrom(randomElementInefficiently2, false);
                EquipmentHelper.AssignHeroEquipmentFromEquipment(hero, equipment);
            }

            hero.BornSettlement = settlement;
            hero.Culture = culture;
            TextObject firstName;
            TextObject fullName;
            NameGenerator.Current.GenerateHeroNameAndHeroFullName(hero, out firstName, out fullName, false);
            hero.SetName(fullName, firstName);
            hero.HeroDeveloper.DeriveSkillsFromTraits(true, null);
            BodyProperties bodyProperties = mother.BodyProperties;
            BodyProperties bodyProperties2 = father.BodyProperties;
            int seed = isOffspringFemale ? mother.CharacterObject.GetDefaultFaceSeed(1) : father.CharacterObject.GetDefaultFaceSeed(1);
            string hairTags = isOffspringFemale ? mother.HairTags : father.HairTags;
            string tattooTags = isOffspringFemale ? mother.TattooTags : father.TattooTags;
            AccessTools.Property(hero.GetType(), "StaticBodyProperties")
                .SetValue(hero, BodyProperties.GetRandomBodyProperties(mother.CharacterObject.Race, isOffspringFemale,
                bodyProperties, bodyProperties2, 1, seed, hairTags, father.BeardTags, tattooTags).StaticProperties);

            CampaignEventDispatcher.Instance.OnHeroCreated(hero, true);
            
            return hero;
        }

        private Equipment GetEquipmentIfPossible(CultureObject culture, bool civillian = false, bool female = false)
        {
            var source = from e in MBObjectManager.Instance.GetObjectTypeList<MBEquipmentRoster>()
                         where e.EquipmentCulture == culture
                         select e;
            if (source == null)
            {
                return null;
            }

            var roster = (from e in source where e.EquipmentCulture == culture select e).ToList()
                .GetRandomElementWithPredicate(x => 
                {
                    bool noble = x.HasEquipmentFlags(EquipmentFlags.IsNobleTemplate);
                    bool genderAppropriate = female ? x.HasEquipmentFlags(EquipmentFlags.IsFemaleTemplate) : !x.HasEquipmentFlags(EquipmentFlags.IsFemaleTemplate);
                    bool adequate = civillian ? x.HasEquipmentFlags(EquipmentFlags.IsCivilianTemplate) : x.HasEquipmentFlags(EquipmentFlags.IsMediumTemplate);
                    return noble && genderAppropriate && adequate;
                });
            if (roster == null)
            {
                return null;
            }

            return roster.AllEquipments.GetRandomElement();
        }
    }
}
