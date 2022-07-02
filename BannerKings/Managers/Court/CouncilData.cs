using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using BannerKings.Populations;
using TaleWorlds.SaveSystem;
using BannerKings.Managers.Titles;
using System;
using BannerKings.Managers.Institutions.Religions;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Court
{
    public class CouncilData : BannerKingsData
    {
        [SaveableProperty(1)]
        private Clan clan { get; set; }

        [SaveableProperty(2)]
        private List<CouncilMember> members { get; set; }

        [SaveableProperty(3)]
        private List<CouncilMember> royalMembers { get; set; }

        public CouncilData(Clan clan, Hero marshall = null, Hero chancellor = null, Hero steward = null, Hero spymaster = null,
            Hero spiritual = null)
        {
            this.clan = clan;
            members = new List<CouncilMember>();
            members.Add(new CouncilMember(marshall, CouncilPosition.Marshall, clan));
            members.Add(new CouncilMember(chancellor, CouncilPosition.Chancellor, clan));
            members.Add(new CouncilMember(steward, CouncilPosition.Steward, clan));
            members.Add(new CouncilMember(spymaster, CouncilPosition.Spymaster, clan));
            members.Add(new CouncilMember(spiritual, CouncilPosition.Spiritual, clan));
            royalMembers = new List<CouncilMember>();
        }

        public Hero Owner => clan.Leader;

        public CouncilMember GetMemberFromPosition(CouncilPosition position)
        {
            CouncilMember member = members.FirstOrDefault(x => x.Position == position);
            if (member != null) return member;

            CouncilMember royalMember = royalMembers.FirstOrDefault(x => x.Position == position);
            return royalMember;
        }

        public MBReadOnlyList<CouncilMember> Positions
        {
            get
            {
                if (members == null)
                    members = new List<CouncilMember>();
                return members.GetReadOnlyList();
            }
        }

        public MBReadOnlyList<CouncilMember> RoyalPositions
        {
            get
            {
                if (royalMembers == null)
                    royalMembers = new List<CouncilMember>();
                return royalMembers.GetReadOnlyList();
            }
        }

        public MBReadOnlyList<CouncilMember> AllPositions
        {
            get
            {
                List<CouncilMember> all = new List<CouncilMember>();
                all.AddRange(members);
                if (royalMembers != null) all.AddRange(royalMembers);
                return all.GetReadOnlyList();
            }
        }

        public bool IsRoyal => BannerKingsConfig.Instance.CouncilModel.IsCouncilRoyal(clan).Item1;

        internal override void Update(PopulationData data)
        {

            if (royalMembers == null) royalMembers = new List<CouncilMember>();
            List<Hero> courtiers = GetCourtMembers();
            if (GetMemberFromPosition(CouncilPosition.Spiritual) == null) members.Add(new CouncilMember(null, CouncilPosition.Spiritual, clan));
            foreach (CouncilMember member in this.members)
            {
                if (member.Member != null && (member.Member.IsDead || member.Member.IsDisabled || !courtiers.Contains(member.Member)))
                    member.Member = null;
                if (member.Clan == null) member.Clan = clan;
            }

            foreach (CouncilMember member in this.royalMembers)
            {
                if (member.Member != null && (member.Member.IsDead || member.Member.IsDisabled || !courtiers.Contains(member.Member)))
                    member.Member = null;
                if (member.Clan == null) member.Clan = clan;
            }

            if (clan.IsUnderMercenaryService) return;

            if (IsRoyal)
            {
                foreach (CouncilMember position in members)
                    if (!position.IsRoyal)
                    {
                        position.IsRoyal = true;
                        if (position.Member != null && !position.IsValidCandidate(position.Member))
                            position.Member = null;
                    }
                    

                List<CouncilMember> royal = GetIdealRoyalPositions();
                foreach (CouncilMember position in royal)
                    if (royalMembers.FirstOrDefault(x => x.Position == position.Position) == null)
                        royalMembers.Add(position);

                List<CouncilMember> toRemove = new List<CouncilMember>();
                foreach (CouncilMember position in royalMembers)
                    if (royal.FirstOrDefault(x => x.Position == position.Position) == null)
                        toRemove.Add(position);

                if (toRemove.Count > 0)
                    foreach (CouncilMember position in toRemove)
                        royalMembers.Remove(position);
            }
            else
            {
                if (royalMembers.Count > 0) royalMembers.Clear();
                foreach (CouncilMember position in members)
                    if (position.IsRoyal)
                        position.IsRoyal = false;
            }

            if (Owner == Hero.MainHero || MBRandom.RandomInt(1, 100) >= 5) return;

            CouncilMember vacant = members.FirstOrDefault(x => x.Member == null);
            if (vacant == null)
            {
                vacant = royalMembers.GetRandomElementWithPredicate(x => x.Member == null);
                if (vacant == null) return;
            }

            Hero hero = MBRandom.ChooseWeighted(GetHeroesForPosition(vacant));
            if (hero != null)
            {
                CouncilAction action = BannerKingsConfig.Instance.CouncilModel.GetAction(CouncilActionType.REQUEST, this, hero, vacant, null, true);
                if (BannerKingsConfig.Instance.CouncilModel.WillAcceptAction(action, Owner)) action.TakeAction();
                else action.Reject(Owner);
            }

            /* bool answer = false;
                   InformationManager.ShowInquiry(new InquiryData(new TextObject("{=!}Council Position Request").ToString(),
                       new TextObject("{=!}{REQUESTER} requests the position of {POSITION} in your council.")
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
            List<CouncilMember> positions = new List<CouncilMember>();
            if (clan.Kingdom == null) return positions;

            FeudalTitle sovereign = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(clan.Kingdom);
            if (sovereign == null || sovereign.contract == null) return positions;
            GovernmentType government = sovereign.contract.Government;

            if (government == GovernmentType.Imperial)
                positions.Add(new CouncilMember(null, CouncilPosition.Prince, clan));
            else if (government == GovernmentType.Feudal)
                positions.Add(new CouncilMember(null, CouncilPosition.Constable, clan));

            if (clan.Kingdom.Culture == Utils.Helpers.GetCulture("vlandia"))
                positions.Add(new CouncilMember(null, CouncilPosition.Castellan, clan));

            if (clan.Kingdom.Culture == Utils.Helpers.GetCulture("battania"))
                positions.Add(new CouncilMember(null, CouncilPosition.Elder, clan));

            return positions;
        }

        public List<Hero> GetAvailableHeroes(bool lordsOnly = false)
        {
            List<Hero> currentMembers = GetMembers();
            List<Hero> available = new List<Hero>();
            foreach (Hero hero in GetCourtMembers())
            {
                if (hero == null) continue;
                if (!currentMembers.Contains(hero) && hero.IsAlive && !hero.IsChild)
                {
                    if (lordsOnly && hero.IsNoble)
                        available.Add(hero);
                    else available.Add(hero);
                }   
            }
            return available;
        }

        public List<ValueTuple<Hero, float>> GetHeroesForPosition(CouncilMember position)
        {
            List<ValueTuple<Hero, float>> list = new List<ValueTuple<Hero, float>>();
            foreach (Hero hero in GetAvailableHeroes())
                if (position.IsValidCandidate(hero))
                    list.Add((hero, GetCompetence(hero, position.Position) + ((float)clan.Leader.GetRelation(hero) * 0.001f)));

            return list;
        }

        public List<Hero> GetCourtMembers()
        {
            List<Hero> heroes = new List<Hero>();

            MBReadOnlyList<Hero> members = clan.Heroes;
            if (members != null && members.Count > 0)
                foreach (Hero member in members)
                    if (member != this.clan.Leader && member.IsAlive && !member.IsChild && !heroes.Contains(member))
                        heroes.Add(member);

            if (BannerKingsConfig.Instance.TitleManager.IsHeroTitleHolder(Owner))
            {
                List<FeudalTitle> vassals = BannerKingsConfig.Instance.TitleManager.GetVassals(Owner);
                if (vassals != null && vassals.Count > 0)
                    foreach (FeudalTitle vassal in vassals)
                        if (vassal.deJure != this.clan.Leader && !heroes.Contains(vassal.deJure))
                            heroes.Add(vassal.deJure);

                FeudalTitle highest = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(Owner);
                if (highest != null && highest.IsSovereignLevel && clan.Kingdom != null)
                    foreach (Clan clan in clan.Kingdom.Clans)
                        if (clan.Leader != Owner && !heroes.Contains(clan.Leader))
                            heroes.Add(clan.Leader);
            }

            MBReadOnlyList<Town> towns = this.clan.Fiefs;
            if (towns != null && towns.Count > 0)
            {
                Religion rel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(clan.Leader);
                foreach (Town town in towns)
                {
                    MBReadOnlyList<Hero> notables = town.Settlement.Notables;
                    if (notables != null && notables.Count > 0)
                        foreach (Hero notable in notables)
                            if (!heroes.Contains(notable))
                                heroes.Add(notable);

                    foreach (Village village in town.Villages)
                    {
                        MBReadOnlyList<Hero> villageNotables = village.Settlement.Notables;
                        if (villageNotables != null && villageNotables.Count > 0)
                            foreach (Hero notable in villageNotables)
                                if (!heroes.Contains(notable))
                                {
                                    if (notable.IsPreacher)
                                    {
                                        Clergyman clergyman = BannerKingsConfig.Instance.ReligionsManager.GetClergymanFromHeroHero(notable);
                                        if (clergyman != null && BannerKingsConfig.Instance.ReligionsManager.GetClergymanReligion(clergyman) != rel)
                                            continue;
                                    }
                                    heroes.Add(notable);
                                }
                                    
                    }
                }
            }

            return heroes;
        }

        public List<CouncilMember> GetOccupiedPositions()
        {
            List<CouncilMember> heroes = new List<CouncilMember>();
            foreach (CouncilMember councilMember in members)
                if (councilMember.Member != null) heroes.Add(councilMember);

            return heroes;
        }

        public List<Hero> GetMembers()
        {
            List<Hero> heroes = new List<Hero>();
            foreach (CouncilMember councilMember in members)
                if (councilMember.Member != null) heroes.Add(councilMember.Member);

            return heroes;
        }

        public CouncilMember GetHeroPosition(Hero hero)
        {
            foreach (CouncilMember councilMember in this.members)
                if (councilMember.Member == hero)
                    return councilMember;

            return null;
        }

        public float GetCompetence(Hero hero, CouncilPosition position)
        {
            float competence = 0f;
            bool found = false;
            foreach (CouncilMember member in this.members)
                if (member.Member == hero)
                {
                    competence = member.Competence;
                    found = true;
                    break;
                }

            if (!found)
                competence = new CouncilMember(hero, position, clan).Competence;
            return competence;
        }


        public float GetCompetence(CouncilPosition position)
        {
            float competence = 0f;
            foreach (CouncilMember member in this.members)
                if (member.Position == position)
                {
                    competence = member.Competence;
                    break;
                }
            return competence;
        }

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
                CouncilMember position = members.FirstOrDefault(x => x.Position == CouncilPosition.Spiritual);
                if (position == null)
                {
                    position = new CouncilMember(null, CouncilPosition.Spiritual, clan);
                    members.Add(position);
                }

                return position.Member;
            }
            set => this.members.First(x => x.Position == CouncilPosition.Spiritual).Member = value;
        }

        public CouncilMember GetCouncilMember(CouncilPosition position)
        {
            CouncilMember result = null;
            result = members.FirstOrDefault(x => x.Position == position);
            if (result == null) result = royalMembers.FirstOrDefault(x => x.Position == position);
            return result;
        }

        public float AdministrativeCosts
        {
            get
            {
                float costs = 0f;
                foreach (CouncilMember councilMember in members)
                    if (councilMember.Member != null)
                        costs += councilMember.AdministrativeCosts();
                return costs;
            }
        }
    }

    public class CouncilMember
    {
        [SaveableProperty(1)]
        private Hero member { get; set; }

        [SaveableProperty(2)]
        private CouncilPosition position { get; set; }

        [SaveableProperty(3)]
        private bool isRoyal { get; set; } = false;

        [SaveableProperty(4)]
        private Clan clan { get; set; } = null;

        [SaveableProperty(5)]
        private int dueWage { get; set; } = 0;

        public CouncilMember(Hero member, CouncilPosition position, Clan clan)
        {
            this.member = member;
            this.position = position;
            this.clan = clan;
            dueWage = 0;
        }

        public Hero Member
        {
            get => this.member;
            set => this.member = value;
        }
        public CouncilPosition Position => this.position;
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

        public TextObject GetName()
        {
            
            return GameTexts.FindText("str_bk_council_" + position.ToString().ToLower() + (isRoyal ? "_royal" : ""),Culture.StringId);
            /*
            Religion rel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(clan.Leader);
            if (rel != null)
            {
                if (rel.Leadership is KinshipLeadership)
                    return rel.Faith.GetRankTitle(rel.Faith.GetMaxClergyRank());

            }

            return GameTexts.FindText("str_bk_spiritual_guide");*/
        }

        public TextObject GetDescription() => GameTexts.FindText("str_bk_council_description_" + position.ToString().ToLower())
            .SetTextVariable("NAME", GetName());

        public bool IsValidCandidate(Hero candidate)
        {
            if (candidate.Clan != null && candidate.Clan.IsUnderMercenaryService) return false;

            if (position == CouncilPosition.Spiritual)
                return BannerKingsConfig.Instance.ReligionsManager.IsPreacher(candidate);
            else if (position == CouncilPosition.Elder)
                return candidate.Culture == Culture && candidate.Age >= 50;
            else if (IsRoyal && IsCorePosition(position))
                return candidate.Occupation == Occupation.Lord;

            return true;
        }

        public float AdministrativeCosts()
        {
            float cost = 0.01f;
            if (position == CouncilPosition.Spiritual) cost = 0f;
            else if (IsCorePosition(position)) cost = IsRoyal ? 0.045f : 0.0275f;

            return cost;
        }

        public float InfluenceCosts()
        {
            float cost = 0f;
            if (IsCorePosition(position) && position != CouncilPosition.Spiritual)
                cost = IsRoyal ? 0.05f : 0.03f;

            return cost;
        }

        public CultureObject Culture
        {
            get
            {
                if (clan.Kingdom != null) return clan.Kingdom.Culture;
                else return clan.Culture;
            }
        }

        public bool IsCorePosition(CouncilPosition position) => position == CouncilPosition.Marshall || position == CouncilPosition.Steward ||
            position == CouncilPosition.Spymaster || position == CouncilPosition.Chancellor;

        public float Competence
        {
            get
            {
                if (this.member != null)
                {
                    int targetCap = 300;
                    float primarySkill = 0f;
                    float secondarySkill = 0f;

                    targetCap += 15 * (member.GetAttributeValue(DefaultCharacterAttributes.Intelligence) - 5);
                    if (this.position == CouncilPosition.Marshall)
                    {
                        primarySkill = member.GetSkillValue(DefaultSkills.Leadership);
                        secondarySkill = member.GetSkillValue(DefaultSkills.Tactics);
                    }
                    else if (this.position == CouncilPosition.Chancellor)
                    {
                        primarySkill = member.GetSkillValue(DefaultSkills.Charm);
                        secondarySkill = member.GetSkillValue(DefaultSkills.Charm);
                    }
                    else if (this.position == CouncilPosition.Steward)
                    {
                        primarySkill = member.GetSkillValue(DefaultSkills.Steward);
                        secondarySkill = member.GetSkillValue(DefaultSkills.Trade);
                    }
                    else if (this.position == CouncilPosition.Spymaster)
                    {
                        primarySkill = member.GetSkillValue(DefaultSkills.Roguery);
                        secondarySkill = member.GetSkillValue(DefaultSkills.Scouting);
                    }

                    return MBMath.ClampFloat((primarySkill + (secondarySkill / 2)) / targetCap, 0f, 1f);
                }
                return 0f;
            }
        }

        public IEnumerable<CouncilPrivileges> GetPrivileges()
        {
            float adm = AdministrativeCosts();
            if (adm > 0.03f) yield return CouncilPrivileges.HIGH_WAGE;
            else if (adm > 0.01f) yield return CouncilPrivileges.MID_WAGE;
            else if (adm > 0f) yield return CouncilPrivileges.LOW_WAGE;

            if (position == CouncilPosition.Spiritual) yield return CouncilPrivileges.CLERGYMEN_EXCLUSIVE;

            if (IsCorePosition(position) && IsRoyal) yield return CouncilPrivileges.NOBLE_EXCLUSIVE;

            float influence = InfluenceCosts();
            if (influence >= 0.05f) yield return CouncilPrivileges.HIGH_INFLUENCE;
            else if (influence > 0f) yield return CouncilPrivileges.INFLUENCE;

            if (position == CouncilPosition.Marshall && IsRoyal) yield return CouncilPrivileges.ARMY_PRIVILEGE;
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