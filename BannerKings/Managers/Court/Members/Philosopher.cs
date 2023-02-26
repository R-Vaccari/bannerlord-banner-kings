using BannerKings.Managers.Court.Members.Tasks;
using BannerKings.Managers.Institutions.Religions.Doctrines;
using BannerKings.Managers.Skills;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Court.Members
{
    public class Philosopher : CouncilPosition
    {
        public Philosopher() : base("Philosopher")
        {
        }

        public override SkillObject PrimarySkill => BKSkills.Instance.Scholarship;
        public override SkillObject SecondarySkill => BKSkills.Instance.Lordship;
        public override TextObject GetCulturalName()
        {
            var id = Culture.StringId;
            if (id == "empire") return new TextObject("{=!}Philosophus");

            return new TextObject("{=!}Philosopher");
        }

        public override IEnumerable<CouncilPrivileges> Privileges
        {
            get
            {
                yield break;
            }
        }

        public override IEnumerable<CouncilTask> Tasks
        {
            get
            {
                yield return DefaultCouncilTasks.Instance.OrganizeMiltia.GetCopy();
            }
        }

        public override CouncilPosition GetCopy(Clan clan)
        {
            Philosopher pos = new Philosopher();
            pos.Initialize(clan);
            return pos;
        }

        public override bool IsAdequate(CouncilData data)
        {
            var religion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(data.Clan.Leader);
            return religion != null && religion.HasDoctrine(DefaultDoctrines.Instance.Literalism);
        }

        protected override bool IsValidCandidateInternal(Hero candidate) => true;
    }
}
