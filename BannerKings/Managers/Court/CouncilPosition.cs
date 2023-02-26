using BannerKings.Managers.Court.Members.Tasks;
using BannerKings.Managers.Institutions.Religions.Doctrines;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Court
{
    public abstract class CouncilPosition : BannerKingsObject
    {
        public CouncilPosition(string id) : base(id)
        {
            DueWage = 0;   
        }

        public void Initialize(Clan clan)
        {
            Clan = clan;
            SetStrings();
            CurrentTask = Tasks.First();
        }

        public void PostInitialize()
        {
            SetStrings();
            CurrentTask.PostInitialize();
        }

        public abstract CouncilPosition GetCopy(Clan clan);

        [SaveableProperty(100)] public Hero Member { get; private set; }
        [SaveableProperty(101)] public bool IsRoyal { get; private set; }
        [SaveableProperty(102)] public Clan Clan { get; private set; }
        [SaveableProperty(103)] public int DueWage { get; set; }
        [SaveableProperty(104)] public CouncilTask CurrentTask { get; private set; }

        public abstract IEnumerable<CouncilTask> Tasks { get; }

        public void SetMember(Hero hero) => Member = hero;
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
        public ExplainedNumber CalculateCandidateCompetence(Hero candidate) => BannerKingsConfig.Instance.CouncilModel
            .CalculateHeroCompetence(candidate, this);
        
        public abstract bool IsAdequate(CouncilData data);
        public bool IsValidCandidate(Hero candidate)
        {
            if (candidate.Clan is { IsUnderMercenaryService: true })
            {
                return false;
            }

            var clanReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(Clan.Leader);
            if (clanReligion != null && clanReligion.HasDoctrine(DefaultDoctrines.Instance.Legalism))
            {
                var candidateReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(candidate);
                if (candidateReligion == null || candidateReligion != clanReligion)
                {
                    return false;
                }
            }

            if (IsRoyal && IsCorePosition(StringId))
            {
                return candidate.Occupation == Occupation.Lord;
            }

            return IsValidCandidateInternal(candidate);
        }

        protected abstract bool IsValidCandidateInternal(Hero candidate);

        public abstract SkillObject PrimarySkill { get; }
        public abstract SkillObject SecondarySkill { get; }
        public abstract TextObject GetCulturalName();

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
            if (IsCorePosition(StringId) && StringId != "Spiritual")
            {
                cost = IsRoyal ? 0.05f : 0.03f;
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

                if (IsCorePosition(StringId) && IsRoyal)
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

        public abstract IEnumerable<CouncilPrivileges> Privileges { get; }

        public override bool Equals(object obj)
        {
            if (obj is CouncilPosition)
            {
                return StringId == (obj as CouncilPosition).StringId;
            }
            return base.Equals(obj);
        }
    }
}
