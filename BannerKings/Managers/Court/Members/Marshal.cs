using BannerKings.Managers.Skills;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Court.Members
{
    public class Marshal : CouncilPosition
    {
        public Marshal() : base("Marshall")
        {
        }

        public override SkillObject PrimarySkill => DefaultSkills.Leadership;
        public override SkillObject SecondarySkill => BKSkills.Instance.Lordship;

        public override IEnumerable<CouncilPrivileges> Privileges
        {
            get
            {
                if (IsRoyal)
                {
                    yield return CouncilPrivileges.ARMY_PRIVILEGE;
                }
            }
        }

        public override IEnumerable<CouncilTask> Tasks
        {
            get
            {
                yield return new OrganizeMilitiaTask();
                yield return new EncourageMilitarismTask();
            }
        }

        public override CouncilPosition GetCopy(Clan clan)
        {
            Marshal pos = new Marshal();
            pos.Initialize(null, clan);
            return pos;
        }

        public override bool IsAdequate(CouncilData data) => true;

        protected override bool IsValidCandidateInternal(Hero candidate) => true;

        public class OrganizeMilitiaTask : CouncilTask
        {
            public override TextObject Name => new TextObject("{=!}Organize Militia");
            public override TextObject Description => new TextObject("{=!}");
            public override TextObject Effects => new TextObject("{=!}");

            public override float StandartBuildUp => 0f;
        }

        public class EncourageMilitarismTask : CouncilTask
        {
            public override TextObject Name => new TextObject("{=!}Encourage Militarism");
            public override TextObject Description => new TextObject("{=!}");
            public override TextObject Effects => new TextObject("{=!}");

            public override float StandartBuildUp => 0f;
        }
    }
}
