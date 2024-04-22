using BannerKings.Managers.Court.Members;
using BannerKings.Managers.Court.Members.Tasks;
using BannerKings.Managers.Institutions.Religions.Doctrines;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Court
{
    public class CouncilMember : BannerKingsObject
    {
        private Func<CouncilData, bool> isAdequate;
        private Func<CouncilMember, Hero, ValueTuple<bool, TextObject>> isValidCandidateInternal;
        private Func<CouncilMember, TextObject> getCulturalName;

        public CouncilMember(string id) : base(id)
        {
            DueWage = 0;
            Traits = new Dictionary<TraitObject, float>();
            LastChange = CampaignTime.Zero;
        }

        public void Initialize(SkillObject primary, SkillObject secondary,
            List<CouncilTask> tasks, IEnumerable<CouncilPrivileges> privileges,
            Func<CouncilData, bool> isAdequate, Func<CouncilMember, Hero, ValueTuple<bool, TextObject>> isValidCandidateInternal,
            Func<CouncilMember, TextObject> getCulturalName, Dictionary<TraitObject, float> traits = null,
            bool isAiPriority = false)
        {
            PrimarySkill = primary;
            SecondarySkill = secondary;
            Tasks = tasks;
            Privileges = privileges;
            this.isAdequate = isAdequate;
            this.isValidCandidateInternal = isValidCandidateInternal;
            this.getCulturalName = getCulturalName;
            if (traits != null)
            {
                Traits = traits;
            }
            IsAIPriority = isAiPriority;
        }

        public void PostInitialize()
        {
            CouncilMember c = DefaultCouncilPositions.Instance.GetById(this);

            Initialize(c.PrimarySkill, c.SecondarySkill, c.Tasks, c.Privileges,
                c.isAdequate, c.isValidCandidateInternal, c.getCulturalName);
            SetStrings();
            foreach (var task in Tasks)
            {
                task.PostInitialize();
            }
            CurrentTask?.PostInitialize();
            if (!Tasks.Any(x => x.StringId == CurrentTask?.StringId))
            {
                SetTask(Tasks[0]);
            }

            if (LastChange == null)
            {
                LastChange = CampaignTime.Zero;
            }

            IsAIPriority = c.IsAIPriority;
        }

        public CouncilMember GetCopy(Clan clan)
        {
            CouncilMember member = new CouncilMember(StringId);
            member.Initialize(PrimarySkill, SecondarySkill,
                Tasks, Privileges, isAdequate, isValidCandidateInternal,
                getCulturalName,
                Traits,
                IsAIPriority);
            member.Clan = clan;
            member.SetStrings();
            member.CurrentTask = member.Tasks.FirstOrDefault();
            return member;
        }

        public void Tick(List<Hero> courtiers)
        {
            if (Member != null)
            {
                if (Member.IsDead || Member.IsDisabled || !courtiers.Contains(Member) || !IsValidCandidate(Member).Item1)
                {
                    Member = null;
                    return;
                }

                CurrentTask.Tick();
                if (SecondarySkill != null)
                {
                    Member.AddSkillXp(PrimarySkill, 10);
                }
                   
                if (SecondarySkill != null)
                {
                    Member.AddSkillXp(SecondarySkill, 5);
                }
            }
        }

        public bool IsAIPriority { get; private set; }

        [SaveableProperty(100)] public Hero Member { get; private set; }
        [SaveableProperty(101)] public bool IsRoyal { get; private set; }
        [SaveableProperty(102)] public Clan Clan { get; private set; }
        [SaveableProperty(103)] public int DueWage { get; set; }
        [SaveableProperty(104)] public CouncilTask CurrentTask { get; private set; }
        [SaveableProperty(105)] public CampaignTime LastChange { get; private set; }

        public bool CanMemberChange() => LastChange.ElapsedYearsUntilNow >= 1f;

        public void SetTask(CouncilTask task)
        {
            CurrentTask = task.GetCopy();
        }

        public List<CouncilTask> Tasks { get; private set; }

        public IEnumerable<CouncilPrivileges> Privileges { get; private set; }

        public  SkillObject PrimarySkill { get; private set; }
        public SkillObject SecondarySkill { get; private set; }
        public Dictionary<TraitObject, float> Traits { get; private set; }
        public TextObject GetCulturalName() => getCulturalName(this);

        protected ValueTuple<bool, TextObject> IsValidCandidateInternal(Hero candidate) => isValidCandidateInternal(this, candidate);
        public bool IsAdequate(CouncilData data) => isAdequate(data);

        public void SetMember(Hero hero)
        {
            if (Member != null)
            {
                BannerKingsConfig.Instance.CourtManager.RemoveCache(hero);
            }

            if (hero != null)
            {
                BannerKingsConfig.Instance.CourtManager.AddCache(hero, this);
            }
            Member = hero;

        }
        public void SetIsRoyal(bool isRoyal)
        {
            IsRoyal = isRoyal;
            SetStrings();
        }

        public CultureObject Culture
        {
            get
            {
                if (Clan.Kingdom != null)
                {
                    return Clan.Kingdom.Culture;
                }

                return Clan.Culture;
            }
        }

        public ExplainedNumber Competence => BannerKingsConfig.Instance.CouncilModel.CalculateHeroCompetence(Member, this);
        public ExplainedNumber ProjectedCompetence => BannerKingsConfig.Instance.CouncilModel.CalculateHeroCompetence(Member, this, true, true);
        public ExplainedNumber CalculateCandidateCompetence(Hero candidate, bool projected = true) => BannerKingsConfig.Instance.CouncilModel
            .CalculateHeroCompetence(candidate, this, projected);

        public ValueTuple<bool, TextObject> IsValidCandidate(Hero candidate)
        {
            if (AllPrivileges.Contains(CouncilPrivileges.NOBLE_EXCLUSIVE) && candidate.Occupation != Occupation.Lord)
            {
                return new(false, new TextObject("{=f3yxaXA3}This privy council position requires a noble."));
            }

            if (Clan.IsUnderMercenaryService)
            {
                return new (false, new TextObject("{=CY32Qr0W}The clan is under mercenary service."));
            }

            var clanReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(Clan.Leader);
            if (clanReligion != null && clanReligion.HasDoctrine(DefaultDoctrines.Instance.Legalism))
            {
                var candidateReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(candidate);
                if (candidateReligion == null || candidateReligion != clanReligion)
                {
                    return new(false, new TextObject("{=MZuuw80X}The {FAITH} requires councilors of same faith due to it's Legalism.")
                        .SetTextVariable("FAITH", clanReligion.Faith.GetFaithName()));
                }
            }

            return IsValidCandidateInternal(candidate);
        }

        private void SetStrings()
        {
            name = GetCulturalName();
            description = GameTexts.FindText("str_bk_council_description_" + StringId.ToString().ToLower())
                    .SetTextVariable("NAME", Name);
        }

        public TextObject GetEffects()
        {
            return GameTexts.FindText("str_bk_council_" + StringId.ToString().ToLower() + "_effects")
                .SetTextVariable("BREAK", "\n");
        }

        public float AdministrativeCosts()
        {
            var cost = 0.01f;
            if (IsCorePosition(StringId))
            {
                cost = IsRoyal ? 0.045f : 0.0275f;
            }

            return cost;
        }

        public float InfluenceCosts()
        {
            var cost = 0f;
            if (IsCorePosition(StringId))
            {
                cost = IsRoyal ? 0.12f : 0.03f;
            }

            return cost;
        }

        public bool IsCorePosition(string id)
        {
            return id == "Marshall" || id == "Steward" || id == "Spymaster" || id == "Chancellor" || id == "Spiritual" ||
                id == "Spouse";
        }

        public IEnumerable<CouncilPrivileges> AllPrivileges
        {
            get
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

                if (IsCorePosition(StringId) && IsRoyal && StringId != "Spiritual")
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

                foreach (CouncilPrivileges privilege in Privileges)
                {
                    yield return privilege;
                }
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is CouncilMember)
            {
                return StringId == (obj as CouncilMember).StringId && Clan == (obj as CouncilMember).Clan;
            }
            return base.Equals(obj);
        }
    }
}
