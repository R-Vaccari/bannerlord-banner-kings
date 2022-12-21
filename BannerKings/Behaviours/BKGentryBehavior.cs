using BannerKings.Actions;
using BannerKings.Managers.Populations.Estates;
using BannerKings.Utils;
using Helpers;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
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
            //CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, OnSettlementDailyTick);
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
                        foreach (var party in clan.WarPartyComponents)
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
                        foreach (var party in clan.WarPartyComponents)
                        {
                            if (party.Leader != null && party.MobileParty.Army == null)
                            {
                                FinishParty(party, estate);
                            }
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

        public int GetGentryPartySize(Clan clan, Estate estate)
        {
            var leader = clan.Leader;
            var manpower = estate.GetManpower(PopType.Serfs);
            return MathF.Min(manpower, 20);
        }

        private void OnSettlementDailyTick(Settlement settlement)
        {
            InitializeGentry(settlement);
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

            if (MBRandom.RandomFloat < (campaignStart ? 0.4f : 0.0001f))
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
                    () => CreateGentryClan(settlement, vacantEstate, clanName, equipment, campaignStart, cost),
                    null));
            }
            else
            {
                CreateGentryClan(settlement, vacantEstate, clanName, equipment, campaignStart, cost);
            }
        }

        private void CreateGentryClan(Settlement settlement, Estate vacantEstate, TextObject clanName, Equipment equipment,
            bool campaignStart, int cost)
        {
            var hero = HeroCreator.CreateSpecialHero(settlement.Culture.LordTemplates.GetRandomElement(),
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
            leader.SetNewOccupation(Occupation.Lord);
            var culture = leader.Culture;
            bool femaleSpouse = !leader.IsFemale;
            Equipment spouseEquipment = GetEquipmentIfPossible(culture, femaleSpouse);
            if (spouseEquipment == null)
            {
                return list;
            }
          
            float ageDifference = MBRandom.RandomFloatRanged(0f, 7f);

            var spouse = HeroCreator.CreateSpecialHero(culture.Townswoman,
                settlement,
                null,
                null,
                (int)(femaleSpouse ? leader.Age - ageDifference : leader.Age + ageDifference));
            list.Add(spouse);
            spouse.SetNewOccupation(Occupation.Lord);
            EquipmentHelper.AssignHeroEquipmentFromEquipment(spouse, spouseEquipment);
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
                var child = HeroCreator.DeliverOffSpring(leader.IsFemale ? leader : spouse,
                leader.IsFemale ? spouse : leader,
                    female,
                    culture
                    );
                child.SetBirthDay(CampaignTime.Now - CampaignTime.Years(age));
                list.Add(child);
                child.SetNewOccupation(Occupation.Lord);
                EquipmentHelper.AssignHeroEquipmentFromEquipment(child, childEquipment);
            }

            return list;
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
