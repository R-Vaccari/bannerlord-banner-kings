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

        public override IEnumerable<CouncilTask> Tasks
        {
            get
            {
                yield return new EducateFamilyTask();
            }
        }

        public override SkillObject PrimarySkill => BKSkills.Instance.Scholarship;
        public override SkillObject SecondarySkill => BKSkills.Instance.Lordship;

        public override IEnumerable<CouncilPrivileges> Privileges
        {
            get
            {
                yield break;
            }
        }

        public override CouncilPosition GetCopy(Clan clan)
        {
            Philosopher pos = new Philosopher();
            pos.Initialize(null, clan);
            return pos;
        }

        public override bool IsAdequate(CouncilData data)
        {
            var religion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(data.Clan.Leader);
            return religion != null && religion.HasDoctrine(DefaultDoctrines.Instance.Literalism);
        }

        protected override bool IsValidCandidateInternal(Hero candidate) => true;

        public class EducateFamilyTask : CouncilTask
        {
            public override TextObject Name => new TextObject("{=!}Organize Militia");
            public override TextObject Description => new TextObject("{=!}");
            public override TextObject Effects => new TextObject("{=!}");

            public override float StandartBuildUp => 0f;
        }
    }
}
