using System;
using System.Collections.Generic;
using System.Linq;
using BannerKings.Extensions;
using BannerKings.Managers.Court.Members;
using BannerKings.Managers.Populations;
using BannerKings.Managers.Skills;
using BannerKings.Managers.Titles;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
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
            Positions = new List<CouncilMember>();
            Peerage = Peerage.GetAdequatePeerage(clan);
        }

        public void PostInitialize()
        {
            if (Positions == null)
            {
                Positions = new List<CouncilMember>();
            }

            foreach (var pos in Positions)
            {
                pos.PostInitialize();
            }
        }

        [SaveableProperty(1)] public Clan Clan { get; private set; }
        [SaveableProperty(5)] public List<CouncilMember> Positions { get; private set; }
        [SaveableProperty(4)] public Peerage Peerage { get; private set; }

        public void SetPeerage(Peerage peerage)
        {
            Peerage = peerage;
        }

        public Hero Owner => Clan.Leader;
        public bool IsRoyal => BannerKingsConfig.Instance.CouncilModel.IsCouncilRoyal(Clan).Item1;

        public float AdministrativeCosts
        {
            get
            {
                var costs = 0f;
                foreach (var councilMember in Positions)
                {
                    if (councilMember.Member != null)
                    {
                        costs += councilMember.AdministrativeCosts();
                    }
                }

                return costs;
            }
        }

        public float GetCompetence(Hero hero, CouncilMember position)
        {
            if (hero != null && position != null)
            {
                return position.CalculateCandidateCompetence(hero).ResultNumber;
            }

            return 0f;
        }

        public float GetCompetence(CouncilMember position) => position.Competence.ResultNumber;
        public CouncilMember GetCouncilPosition(CouncilMember position) => Positions.FirstOrDefault(x => x.Equals(position));

        internal override void Update(PopulationData data)
        {
            var courtiers = GetCourtMembers();
            List<CouncilMember> positions = new List<CouncilMember>();
            foreach (var pos in DefaultCouncilPositions.Instance.All)
            {
                if (pos.IsAdequate(this))
                {
                    if (!Positions.Contains(pos))
                    {
                        Positions.Add(pos.GetCopy(Clan));
                    }
                }
                else if (Positions.Contains(pos))
                {
                    Positions.Remove(pos);
                }
            }

            foreach (var position in Positions)
            {
                if (position.Member != null &&
                    (position.Member.IsDead || position.Member.IsDisabled || !courtiers.Contains(position.Member)))
                {
                    position.SetMember(null);
                }

                position.SetIsRoyal(IsRoyal);
                if (position.Member != null && !position.IsValidCandidate(position.Member))
                {
                    position.SetMember(null);
                }
            }

            if (MBRandom.RandomFloat <= 0.02f)
            {
                var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(Owner);
                if (education.HasPerk(BKPerks.Instance.AugustDeFacto))
                {
                    var random = GetOccupiedPositions().GetRandomElement().Member;
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

            Owner.AddSkillXp(BKSkills.Instance.Lordship, Positions.Count * 5);
            if (Clan.IsUnderMercenaryService)
            {
                return;
            }
           
            if (Owner == Hero.MainHero)
            {
                return;
            }

            var vacant = Positions.FirstOrDefault(x => x.Member == null);
            if (vacant == null)
            {
                return;
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

        public List<Hero> GetAvailableHeroes(bool lordsOnly = false)
        {
            var currentMembers = GetMembers();
            var available = new List<Hero>();
            foreach (var hero in GetCourtMembers())
            {
                if (hero == null)
                {
                    continue;
                }

                if (!currentMembers.Contains(hero) && hero.IsAlive && !hero.IsChild)
                {
                    if (lordsOnly && hero.IsLord)
                    {
                        available.Add(hero);
                    }
                    else
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
                if (position.IsValidCandidate(hero))
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

            var highest = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(Owner);
            if (highest is { IsSovereignLevel: true } && Clan.Kingdom != null)
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
                if (title.type == TitleType.Lordship && title.fief.MapFaction == Owner.MapFaction)
                {
                    foreach (var notable in title.fief.Notables)
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
            PostInitialize();
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

        public List<Hero> GetMembers()
        {
            PostInitialize();
            var heroes = new List<Hero>();
            foreach (var councilMember in Positions)
            {
                if (councilMember.Member != null)
                {
                    heroes.Add(councilMember.Member);
                }
            }

            return heroes;
        }

        public CouncilMember GetHeroPosition(Hero hero)
        {
            PostInitialize();
            foreach (var councilMember in Positions)
            {
                if (councilMember.Member == hero)
                {
                    return councilMember;
                }
            }

            return null;
        }
    }

    public enum CouncilPosition
    {
        Marshall,
        Chancellor,
        Steward,
        Spymaster,
        Spiritual,
        Prince,
        Castellan,
        Druzina,
        Elder,
        Constable,
        Philosopher,
        None
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