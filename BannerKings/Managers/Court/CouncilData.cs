using System;
using System.Collections.Generic;
using System.Linq;
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
        public CouncilData(Clan clan, Hero marshall = null, Hero chancellor = null, Hero steward = null,
            Hero spymaster = null,
            Hero spiritual = null)
        {
            this.clan = clan;
            members = new List<CouncilMember>
            {
                new(marshall, CouncilPosition.Marshall, clan),
                new(chancellor, CouncilPosition.Chancellor, clan),
                new(steward, CouncilPosition.Steward, clan),
                new(spymaster, CouncilPosition.Spymaster, clan),
                new(spiritual, CouncilPosition.Spiritual, clan)
            };
            royalMembers = new List<CouncilMember>();
        }

        [SaveableProperty(1)] private Clan clan { get; set; }

        [SaveableProperty(2)] private List<CouncilMember> members { get; set; }

        [SaveableProperty(3)] private List<CouncilMember> royalMembers { get; set; }

        public Hero Owner => clan.Leader;

        public MBReadOnlyList<CouncilMember> Positions
        {
            get
            {
                if (members == null)
                {
                    members = new List<CouncilMember>();
                }

                return members.GetReadOnlyList();
            }
        }

        public MBReadOnlyList<CouncilMember> RoyalPositions
        {
            get
            {
                if (royalMembers == null)
                {
                    royalMembers = new List<CouncilMember>();
                }

                return royalMembers.GetReadOnlyList();
            }
        }

        public MBReadOnlyList<CouncilMember> AllPositions
        {
            get
            {
                var all = new List<CouncilMember>();
                all.AddRange(members);
                if (royalMembers != null)
                {
                    all.AddRange(royalMembers);
                }

                return all.GetReadOnlyList();
            }
        }

        public bool IsRoyal => BannerKingsConfig.Instance.CouncilModel.IsCouncilRoyal(clan).Item1;

        public Hero Marshall
        {
            get => members.First(x => x.Position == CouncilPosition.Marshall).Member;
            set => members.First(x => x.Position == CouncilPosition.Marshall).Member = value;
        }

        public Hero Chancellor
        {
            get => members.First(x => x.Position == CouncilPosition.Chancellor).Member;
            set => members.First(x => x.Position == CouncilPosition.Chancellor).Member = value;
        }

        public Hero Steward
        {
            get => members.First(x => x.Position == CouncilPosition.Steward).Member;
            set => members.First(x => x.Position == CouncilPosition.Steward).Member = value;
        }

        public Hero Spymaster
        {
            get => members.First(x => x.Position == CouncilPosition.Spymaster).Member;
            set => members.First(x => x.Position == CouncilPosition.Spymaster).Member = value;
        }

        public Hero Spiritual
        {
            get
            {
                var position = members.FirstOrDefault(x => x.Position == CouncilPosition.Spiritual);
                if (position == null)
                {
                    position = new CouncilMember(null, CouncilPosition.Spiritual, clan);
                    members.Add(position);
                }

                return position.Member;
            }
            set => members.First(x => x.Position == CouncilPosition.Spiritual).Member = value;
        }

        public float AdministrativeCosts
        {
            get
            {
                var costs = 0f;
                foreach (var councilMember in members)
                {
                    if (councilMember.Member != null)
                    {
                        costs += councilMember.AdministrativeCosts();
                    }
                }

                return costs;
            }
        }

        public CouncilMember GetMemberFromPosition(CouncilPosition position)
        {
            var member = members.FirstOrDefault(x => x.Position == position);
            if (member != null)
            {
                return member;
            }

            var royalMember = royalMembers.FirstOrDefault(x => x.Position == position);
            return royalMember;
        }

        internal override void Update(PopulationData data)
        {
            if (royalMembers == null)
            {
                royalMembers = new List<CouncilMember>();
            }

            var courtiers = GetCourtMembers();
            if (GetMemberFromPosition(CouncilPosition.Spiritual) == null)
            {
                members.Add(new CouncilMember(null, CouncilPosition.Spiritual, clan));
            }

            foreach (var member in members)
            {
                if (member.Member != null &&
                    (member.Member.IsDead || member.Member.IsDisabled || !courtiers.Contains(member.Member)))
                {
                    member.Member = null;
                }

                if (member.Clan == null)
                {
                    member.Clan = clan;
                }
            }

            foreach (var member in royalMembers)
            {
                if (member.Member != null &&
                    (member.Member.IsDead || member.Member.IsDisabled || !courtiers.Contains(member.Member)))
                {
                    member.Member = null;
                }

                if (member.Clan == null)
                {
                    member.Clan = clan;
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
                            new TextObject("{=M6K8ooUFc}You improved relations with {HERO} due to {PERK} lifestyle perk.")
                                .SetTextVariable("HERO", random.Name)
                                .SetTextVariable("PERK", BKPerks.Instance.AugustDeFacto.Name)
                                .ToString()));
                    }
                }
            }

            if (clan.IsUnderMercenaryService)
            {
                return;
            }

            if (IsRoyal)
            {
                foreach (var position in members)
                {
                    if (!position.IsRoyal)
                    {
                        position.IsRoyal = true;
                        if (position.Member != null && !position.IsValidCandidate(position.Member))
                        {
                            position.Member = null;
                        }
                    }
                }


                var royal = GetIdealRoyalPositions();
                foreach (var position in royal)
                {
                    if (royalMembers.FirstOrDefault(x => x.Position == position.Position) == null)
                    {
                        royalMembers.Add(position);
                    }
                }

                var toRemove = new List<CouncilMember>();
                foreach (var position in royalMembers)
                {
                    if (royal.FirstOrDefault(x => x.Position == position.Position) == null)
                    {
                        toRemove.Add(position);
                    }
                }

                if (toRemove.Count > 0)
                {
                    foreach (var position in toRemove)
                    {
                        royalMembers.Remove(position);
                    }
                }
            }
            else
            {
                if (royalMembers.Count > 0)
                {
                    royalMembers.Clear();
                }

                foreach (var position in members)
                {
                    if (position.IsRoyal)
                    {
                        position.IsRoyal = false;
                    }
                }
            }

            if (Owner == Hero.MainHero || MBRandom.RandomInt(1, 100) >= 5)
            {
                return;
            }

            var vacant = members.FirstOrDefault(x => x.Member == null);
            if (vacant == null)
            {
                vacant = royalMembers.GetRandomElementWithPredicate(x => x.Member == null);
                if (vacant == null)
                {
                    return;
                }
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

            /* bool answer = false;
                   InformationManager.ShowInquiry(new InquiryData(new TextObject("{=O2vyPfZFJ}Council Position Request").ToString(),
                       new TextObject("{=0DTwFByMk}{REQUESTER} requests the position of {POSITION} in your council.")
                       .SetTextVariable("REQUESTER", action.ActionTaker.EncyclopediaLinkWithName)
                       .SetTextVariable("POSITION", action.TargetPosition.GetName()).ToString(),
                   action.Possible, true, GameTexts.FindText("str_selection_widget_accept").ToString(),
                   GameTexts.FindText("str_selection_widget_cancel").ToString(), 
                   () => action.TakeAction(),
                   () => action.Reject(Owner), string.Empty));
                  */
        }

        public List<CouncilMember> GetIdealRoyalPositions()
        {
            var positions = new List<CouncilMember>();
            if (clan.Kingdom == null)
            {
                return positions;
            }

            var sovereign = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(clan.Kingdom);
            if (sovereign?.contract == null)
            {
                return positions;
            }

            var government = sovereign.contract.Government;

            switch (government)
            {
                case GovernmentType.Imperial:
                    positions.Add(new CouncilMember(null, CouncilPosition.Prince, clan));
                    break;
                case GovernmentType.Feudal:
                    positions.Add(new CouncilMember(null, CouncilPosition.Constable, clan));
                    break;
            }

            if (clan.Kingdom.Culture == Utils.Helpers.GetCulture("vlandia"))
            {
                positions.Add(new CouncilMember(null, CouncilPosition.Castellan, clan));
            }

            if (clan.Kingdom.Culture == Utils.Helpers.GetCulture("battania"))
            {
                positions.Add(new CouncilMember(null, CouncilPosition.Elder, clan));
            }

            return positions;
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
                    list.Add((hero, GetCompetence(hero, position.Position) + clan.Leader.GetRelation(hero) * 0.001f));
                }
            }

            return list;
        }

        public List<Hero> GetCourtMembers()
        {
            var heroes = new List<Hero>();

            var members = clan.Heroes;
            if (members is {Count: > 0})
            {
                foreach (var member in members)
                {
                    if (member != clan.Leader && member.IsAlive && !member.IsChild && !heroes.Contains(member))
                    {
                        heroes.Add(member);
                    }
                }
            }

            if (BannerKingsConfig.Instance.TitleManager.IsHeroTitleHolder(Owner))
            {
                var vassals = BannerKingsConfig.Instance.TitleManager.GetVassals(Owner);
                if (vassals is {Count: > 0})
                {
                    foreach (var vassal in vassals)
                    {
                        if (vassal.deJure != clan.Leader && !heroes.Contains(vassal.deJure))
                        {
                            heroes.Add(vassal.deJure);
                        }
                    }
                }

                var highest = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(Owner);
                if (highest is {IsSovereignLevel: true} && clan.Kingdom != null)
                {
                    foreach (var clan in clan.Kingdom.Clans)
                    {
                        if (clan.Leader != Owner && !heroes.Contains(clan.Leader))
                        {
                            heroes.Add(clan.Leader);
                        }
                    }
                }
            }

            var towns = this.clan.Fiefs;
            if (towns is {Count: > 0})
            {
                var rel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(clan.Leader);
                foreach (var town in towns)
                {
                    var notables = town.Settlement.Notables;
                    if (notables is {Count: > 0})
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
                        if (villageNotables is {Count: > 0})
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
            foreach (var councilMember in members)
            {
                if (councilMember.Member != null)
                {
                    heroes.Add(councilMember);
                }
            }

            foreach (var councilMember in royalMembers)
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
            var heroes = new List<Hero>();
            foreach (var councilMember in members)
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
            foreach (var councilMember in members)
            {
                if (councilMember.Member == hero)
                {
                    return councilMember;
                }
            }

            return null;
        }

        public float GetCompetence(Hero hero, CouncilPosition position)
        {
            var competence = 0f;
            var found = false;
            foreach (var member in members)
            {
                if (member.Member == hero)
                {
                    competence = member.Competence;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                competence = new CouncilMember(hero, position, clan).Competence;
            }

            return competence;
        }


        public float GetCompetence(CouncilPosition position)
        {
            var competence = 0f;
            foreach (var member in members)
            {
                if (member.Position == position)
                {
                    competence = member.Competence;
                    break;
                }
            }

            return competence;
        }

        public CouncilMember GetCouncilMember(CouncilPosition position)
        {
            return  members.FirstOrDefault(x => x.Position == position) ?? royalMembers.FirstOrDefault(x => x.Position == position);
        }
    }

    public class CouncilMember
    {
        public CouncilMember(Hero member, CouncilPosition position, Clan clan)
        {
            this.member = member;
            this.position = position;
            this.clan = clan;
            dueWage = 0;
        }

        [SaveableProperty(1)] private Hero member { get; set; }

        [SaveableProperty(2)] private CouncilPosition position { get; set; }

        [SaveableProperty(3)] private bool isRoyal { get; set; }

        [SaveableProperty(4)] private Clan clan { get; set; }

        [SaveableProperty(5)] private int dueWage { get; set; }

        public Hero Member
        {
            get => member;
            set => member = value;
        }

        public CouncilPosition Position => position;

        public bool IsRoyal
        {
            get => isRoyal;
            set => isRoyal = value;
        }

        public Clan Clan
        {
            get => clan;
            set => clan = value;
        }

        public int DueWage
        {
            get => dueWage;
            set => dueWage = value;
        }

        public CultureObject Culture
        {
            get
            {
                if (clan.Kingdom != null)
                {
                    return clan.Kingdom.Culture;
                }

                return clan.Culture;
            }
        }

        public float Competence
        {
            get
            {
                if (member != null)
                {
                    var targetCap = 300;
                    var primarySkill = 0f;
                    var secondarySkill = 0f;

                    targetCap += 15 * (member.GetAttributeValue(DefaultCharacterAttributes.Intelligence) - 5);
                    switch (position)
                    {
                        case CouncilPosition.Marshall:
                            primarySkill = member.GetSkillValue(DefaultSkills.Leadership);
                            secondarySkill = member.GetSkillValue(DefaultSkills.Tactics);
                            break;
                        case CouncilPosition.Chancellor:
                            primarySkill = member.GetSkillValue(DefaultSkills.Charm);
                            secondarySkill = member.GetSkillValue(DefaultSkills.Charm);
                            break;
                        case CouncilPosition.Steward:
                            primarySkill = member.GetSkillValue(DefaultSkills.Steward);
                            secondarySkill = member.GetSkillValue(DefaultSkills.Trade);
                            break;
                        case CouncilPosition.Spymaster:
                            primarySkill = member.GetSkillValue(DefaultSkills.Roguery);
                            secondarySkill = member.GetSkillValue(DefaultSkills.Scouting);
                            break;
                    }

                    return MBMath.ClampFloat((primarySkill + secondarySkill / 2) / targetCap, 0f, 1f);
                }

                return 0f;
            }
        }

        public TextObject GetName()
        {
            return GameTexts.FindText("str_bk_council_" + position.ToString().ToLower() + (isRoyal ? "_royal" : ""),
                Culture.StringId);
        }

        public TextObject GetDescription()
        {
            return GameTexts.FindText("str_bk_council_description_" + position.ToString().ToLower())
                .SetTextVariable("NAME", GetName());
        }

        public TextObject GetEffects()
        {
            return GameTexts.FindText("str_bk_council_" + position.ToString().ToLower() + "_effects")
                .SetTextVariable("BREAK", "\n");
        }

        public bool IsValidCandidate(Hero candidate)
        {
            if (candidate.Clan is {IsUnderMercenaryService: true})
            {
                return false;
            }

            switch (position)
            {
                case CouncilPosition.Spiritual:
                    return BannerKingsConfig.Instance.ReligionsManager.IsPreacher(candidate);
                case CouncilPosition.Elder:
                    return candidate.Culture == Culture && candidate.Age >= 50;
            }

            if (IsRoyal && IsCorePosition(position))
            {
                return candidate.Occupation == Occupation.Lord;
            }

            return true;
        }

        public float AdministrativeCosts()
        {
            var cost = 0.01f;
            if (position == CouncilPosition.Spiritual)
            {
                cost = 0f;
            }
            else if (IsCorePosition(position))
            {
                cost = IsRoyal ? 0.045f : 0.0275f;
            }

            return cost;
        }

        public float InfluenceCosts()
        {
            var cost = 0f;
            if (IsCorePosition(position) && position != CouncilPosition.Spiritual)
            {
                cost = IsRoyal ? 0.05f : 0.03f;
            }

            return cost;
        }

        public bool IsCorePosition(CouncilPosition position)
        {
            return position is CouncilPosition.Marshall or CouncilPosition.Steward or CouncilPosition.Spymaster or CouncilPosition.Chancellor;
        }

        public IEnumerable<CouncilPrivileges> GetPrivileges()
        {
            var adm = AdministrativeCosts();
            switch (adm)
            {
                case > 0.03f:
                    yield return CouncilPrivileges.HIGH_WAGE;
                    break;
                case > 0.01f:
                    yield return CouncilPrivileges.MID_WAGE;
                    break;
                case > 0f:
                    yield return CouncilPrivileges.LOW_WAGE;
                    break;
            }

            if (position == CouncilPosition.Spiritual)
            {
                yield return CouncilPrivileges.CLERGYMEN_EXCLUSIVE;
            }

            if (IsCorePosition(position) && IsRoyal)
            {
                yield return CouncilPrivileges.NOBLE_EXCLUSIVE;
            }

            var influence = InfluenceCosts();
            switch (influence)
            {
                case >= 0.05f:
                    yield return CouncilPrivileges.HIGH_INFLUENCE;
                    break;
                case > 0f:
                    yield return CouncilPrivileges.INFLUENCE;
                    break;
            }

            if (position == CouncilPosition.Marshall && IsRoyal)
            {
                yield return CouncilPrivileges.ARMY_PRIVILEGE;
            }
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