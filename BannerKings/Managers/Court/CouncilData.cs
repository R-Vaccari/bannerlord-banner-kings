using System;
using System.Collections.Generic;
using System.Linq;
using BannerKings.Extensions;
using BannerKings.Managers.Court.Grace;
using BannerKings.Managers.Court.Members;
using BannerKings.Managers.Populations;
using BannerKings.Managers.Skills;
using BannerKings.Managers.Titles;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Court
{
    public class CouncilData : BannerKingsData
    {
        public CouncilData(Clan clan)
        {
            this.Clan = clan;
            Guests = new List<Hero>();
            Positions = new List<CouncilMember>();
            Peerage = Peerage.GetAdequatePeerage(clan);
            foreach (var pos in DefaultCouncilPositions.Instance.All)
            {
                if (pos.IsAdequate(this))
                {
                    if (!Positions.Any(x => x.StringId == pos.StringId))
                    {
                        Positions.Add(pos.GetCopy(Clan));
                    }
                }
            }

            foreach (var pos in Positions)
            {
                pos.PostInitialize();
            }

            CourtGrace = new CourtGrace(this);
        }

        public void PostInitialize()
        {
            if (Positions == null)
            {
                Positions = new List<CouncilMember>();
            }

            if (Guests == null)
            {
                Guests = new List<Hero>();
            }

            if (CourtGrace == null)
            {
                CourtGrace = new CourtGrace(this);
            }

            if (Location == null)
            {
                float prosp = 0f;
                Town town = null;
                foreach (var fief in Clan.Fiefs)
                {
                    if (fief.Prosperity > prosp && fief.Culture == Clan.Culture)
                    {
                        prosp = fief.Prosperity;
                        town = fief;
                    }
                }

                SetCourtLocation(town);
            }

            CourtGrace.PostInitialize();
            foreach (var pos in Positions)
            {
                pos.PostInitialize();
            }
        }

        [SaveableProperty(1)] public Clan Clan { get; private set; }
        [SaveableProperty(2)] public Town Location { get; private set; }
        [SaveableProperty(3)] public List<Hero> Guests { get; private set; }
        [SaveableProperty(5)] public List<CouncilMember> Positions { get; private set; }
        [SaveableProperty(4)] public Peerage Peerage { get; private set; }
        [SaveableProperty(6)] public CourtGrace CourtGrace { get; private set; }


        public void SetCourtLocation(Town town, bool notify = true)
        {
            Location = town;
            if (Clan == Clan.PlayerClan && notify)
            {
                TextObject text = new TextObject("{=CJbZf05V}Your court no longer has a place to gather!");
                if (town != null)
                {
                    text = new TextObject("{=FVzfdaxP}Your court will now gather at {TOWN}!")
                        .SetTextVariable("TOWN", town.Name);
                }

                InformationManager.DisplayMessage(new InformationMessage(text.ToString()));
            }
        }

        public void SetPeerage(Peerage peerage)
        {
            Peerage = peerage;
        }

        public Hero Owner => Clan.Leader;
        public bool IsRoyal => BannerKingsConfig.Instance.CouncilModel.IsCouncilRoyal(Clan).Item1;
        public ExplainedNumber AdministrativeCosts => BannerKingsConfig.Instance.CouncilModel.CalculateAdmCosts(this);

        public float GetCompetence(Hero hero, CouncilMember position)
        {
            if (hero != null && position != null)
            {
                return position.CalculateCandidateCompetence(hero, false).ResultNumber;
            }

            return 0f;
        }

        public float GetCompetence(CouncilMember position) => position.Competence.ResultNumber;
        public CouncilMember GetCouncilPosition(CouncilMember position) => Positions.FirstOrDefault(x => x.StringId == position.StringId);

        public void AddGuest(Hero hero)
        {
            if (hero == null || hero.CurrentSettlement == null || hero.CurrentSettlement.Town == null ||
                hero.CurrentSettlement.Town != Location)
            {
                return;
            }

            Guests.Add(hero);
            if (Clan == Clan.PlayerClan)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    new TextObject("{=3M6VwcXC}{HERO} is now a guest in your court at {TOWN}.")
                    .SetTextVariable("TOWN", Location.Name)
                    .SetTextVariable("HERO", hero.Name)
                    .ToString()));
            }
        }

        public void RemoveGuest(Hero hero)
        {
            if (hero == null || !Guests.Contains(hero))
            {
                return;
            }

            Guests.Remove(hero);
            if (hero.CompanionOf == null && hero.Clan == null && hero.IsWanderer)
            {
                KillCharacterAction.ApplyByRemove(hero);
            }
        }

        internal override void Update(PopulationData data)
        {
            var courtiers = GetCourtMembers();
            foreach (var pos in DefaultCouncilPositions.Instance.All)
            {
                if (pos.IsAdequate(this))
                {
                    if (!Positions.Any(x => x.StringId == pos.StringId))
                    {
                        Positions.Add(pos.GetCopy(Clan));
                    }
                }
                else if (Positions.Contains(pos))
                {
                    Positions.Remove(pos);
                }
            }

            if (Location != null && Location.OwnerClan != Clan)
            {
                SetCourtLocation(null, false);
            }

            if (Location != null && MBRandom.RandomFloat < 0.02f)
            {
                var template = Clan.Culture.NotableAndWandererTemplates.GetRandomElementWithPredicate(x => x.Occupation == Occupation.Wanderer);
                Hero guest = HeroCreator.CreateSpecialHero(template, 
                    Location.Settlement, 
                    null, 
                    null, 
                    TaleWorlds.CampaignSystem.Campaign.Current.Models.AgeModel.HeroComesOfAge + 5 + MBRandom.RandomInt(27));
                EnterSettlementAction.ApplyForCharacterOnly(guest, Location.Settlement);
                AddGuest(guest);
            }

            List<Hero> toRemove = new List<Hero>();
            foreach (Hero guest in Guests)
            {
                if (guest.IsDead || guest.CurrentSettlement == null || guest.CurrentSettlement != Location?.Settlement)
                {
                    toRemove.Add(guest);
                }
                else if (MBRandom.RandomFloat < 0.14f && MBRandom.RandomFloat < 0.1f)
                {
                    toRemove.Add(guest);
                }
                else if (Guests.Count > 5)
                {
                    toRemove.Add(guest);
                }
            }

            foreach (Hero guest in toRemove)
            {
                RemoveGuest(guest);
            }

            foreach (var position in Positions)
            {
                position.SetIsRoyal(IsRoyal);
                position.Tick(courtiers);
            }

            if (MBRandom.RandomFloat <= 0.02f)
            {
                var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(Owner);
                if (education.HasPerk(BKPerks.Instance.AugustDeFacto))
                {
                    var positions = GetOccupiedPositions();
                    if (positions.Count > 0) 
                    {
                        var random = positions.GetRandomElement().Member;
                        ChangeRelationAction.ApplyRelationChangeBetweenHeroes(Owner, random, 1, false);
                        if (Owner == Hero.MainHero)
                        {
                            InformationManager.DisplayMessage(new InformationMessage(
                                new TextObject("{=rLgCZZDO}You improved relations with {HERO} due to {PERK} lifestyle perk.")
                                    .SetTextVariable("HERO", random.Name)
                                    .SetTextVariable("PERK", BKPerks.Instance.AugustDeFacto.Name)
                                    .ToString()));
                        }
                    }
                }
            }

            Owner.AddSkillXp(BKSkills.Instance.Lordship, Positions.Count * 5);
            if (Clan.IsUnderMercenaryService)
            {
                return;
            }

            CourtGrace?.Update();

            if (Owner == Hero.MainHero)
            {
                return;
            }

            CouncilMember vacant = Positions.FirstOrDefault(x => x.IsAIPriority && x.Member == null);
            if (vacant == null)
            {
                vacant = Positions.FirstOrDefault(x => !x.IsAIPriority && x.Member == null);
                if (vacant == null) return;
            }

            var hero = MBRandom.ChooseWeighted(GetHeroesForPosition(vacant));
            if (hero != null)
            {
                var action =
                    BannerKingsConfig.Instance.CouncilModel.GetAction(CouncilActionType.REQUEST, this, hero, vacant, null,
                        true);
                if (BannerKingsConfig.Instance.CouncilModel.WillAcceptAction(action, Owner))
                {
                    action.TakeAction();
                }
                else
                {
                    action.Reject(Owner);
                }
            }
        }

        public List<Hero> GetAvailableHeroes(CouncilMember position = null, bool lordsOnly = false)
        {
            var available = new List<Hero>();
            foreach (var hero in GetCourtMembers())
            {
                if (hero == null || !hero.IsAlive || hero.IsChild) continue;
                if (lordsOnly && !hero.IsLord) continue;

                if (position == null)
                {
                    if (!GetOccupiedPositions().Any(x => x.Member == hero))
                        available.Add(hero);
                }
                else
                {
                    CouncilMember conflicting = GetHeroCurrentConflictingPosition(position, hero);
                    if (conflicting == null)
                    {
                        available.Add(hero);
                    }
                }
            }

            return available;
        }

        public List<ValueTuple<Hero, float>> GetHeroesForPosition(CouncilMember position)
        {
            var list = new List<ValueTuple<Hero, float>>();
            foreach (var hero in GetAvailableHeroes())
            {
                if (position.IsValidCandidate(hero).Item1)
                {
                    list.Add((hero, GetCompetence(hero, position) +
                        Clan.Leader.GetRelation(hero) * 0.001f));
                }
            }

            return list;
        }

        public List<Hero> GetCourtMembers()
        {
            var heroes = new List<Hero>();
            var members = Clan.Heroes;
            if (members is { Count: > 0 })
            {
                foreach (var member in members)
                {
                    if (member != Clan.Leader && member.IsAlive && !member.IsChild && !heroes.Contains(member))
                    {
                        heroes.Add(member);
                    }
                }
            }

            var vassals = BannerKingsConfig.Instance.TitleManager.GetVassals(Owner);
            if (vassals is { Count: > 0 })
            {
                foreach (var vassal in vassals)
                {
                    if (vassal.deJure != Clan.Leader && !heroes.Contains(vassal.deJure))
                    {
                        heroes.Add(vassal.deJure);
                    }
                }
            }

            if (Clan.Kingdom != null && Clan == Clan.Kingdom.RulingClan)
            {
                foreach (var clan in Clan.Kingdom.Clans)
                {
                    if (clan.Leader != Owner && !heroes.Contains(clan.Leader))
                    {
                        heroes.Add(clan.Leader);
                    }
                }
            }

            var rel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(Clan.Leader);
            var titles = BannerKingsConfig.Instance.TitleManager.GetAllDeJure(Owner);
            foreach (var title in titles)
            {
                if (title.TitleType == TitleType.Lordship && title.Fief.MapFaction == Owner.MapFaction)
                {
                    foreach (var notable in title.Fief.Notables)
                    {
                        if (!heroes.Contains(notable))
                        {
                            if (notable.IsPreacher)
                            {
                                var clergyman =
                                    BannerKingsConfig.Instance.ReligionsManager.GetClergymanFromHeroHero(notable);
                                if (clergyman != null &&
                                    BannerKingsConfig.Instance.ReligionsManager.GetClergymanReligion(clergyman) !=
                                    rel)
                                {
                                    continue;
                                }
                            }
                            heroes.Add(notable);
                        }
                    }
                }
            }

            var towns = Clan.Fiefs;
            if (towns is { Count: > 0 })
            {
                foreach (var town in towns)
                {
                    var notables = town.Settlement.Notables;
                    if (notables is { Count: > 0 })
                    {
                        foreach (var notable in notables)
                        {
                            if (!heroes.Contains(notable))
                            {
                                heroes.Add(notable);
                            }
                        }
                    }

                    foreach (var village in town.Villages)
                    {
                        var villageNotables = village.Settlement.Notables;
                        if (villageNotables is { Count: > 0 } && village.GetActualOwner() == Owner)
                        {
                            foreach (var notable in villageNotables)
                            {
                                if (!heroes.Contains(notable))
                                {
                                    if (notable.IsPreacher)
                                    {
                                        var clergyman =
                                            BannerKingsConfig.Instance.ReligionsManager.GetClergymanFromHeroHero(notable);
                                        if (clergyman != null &&
                                            BannerKingsConfig.Instance.ReligionsManager.GetClergymanReligion(clergyman) !=
                                            rel)
                                        {
                                            continue;
                                        }
                                    }

                                    heroes.Add(notable);
                                }
                            }
                        }
                    }
                }
            }

            return heroes;
        }

        public List<CouncilMember> GetOccupiedPositions()
        {
            var heroes = new List<CouncilMember>();
            foreach (var councilMember in Positions)
            {
                if (councilMember.Member != null)
                {
                    heroes.Add(councilMember);
                }
            }

            return heroes;
        }

        public CouncilMember GetHeroCurrentConflictingPosition(CouncilMember desiredPosition, Hero hero)
        {
            if (desiredPosition.IsCorePosition(desiredPosition.StringId))
                return GetHeroPositions(hero).FirstOrDefault(x => x.IsCorePosition(x.StringId));

            return GetHeroPositions(hero).FirstOrDefault(x => !x.IsCorePosition(x.StringId));
        }

        public List<CouncilMember> GetHeroPositions(Hero hero)
        {
            List<CouncilMember> positions = new List<CouncilMember>();
            foreach (var councilMember in Positions)
            {
                if (councilMember.Member == hero)
                {
                    positions.Add(councilMember);
                }
            }

            return positions;
        }
    }

    public enum CouncilPrivileges
    {
        LOW_WAGE,
        MID_WAGE,
        HIGH_WAGE,
        INFLUENCE,
        HIGH_INFLUENCE,
        CLERGYMEN_EXCLUSIVE,
        NOBLE_EXCLUSIVE,
        ARMY_PRIVILEGE,
        REVOKE_PRIVILEGE
    }
}