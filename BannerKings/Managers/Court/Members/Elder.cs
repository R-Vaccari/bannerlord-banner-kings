using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Court.Members
{
    public class Elder : CouncilPosition
    {
        public Elder() : base("Elder")
        {
        }

        public override IEnumerable<CouncilTask> Tasks
        {
            get
            {
                yield return new OverseeCastlesTask();
            }
        }

        public override SkillObject PrimarySkill => DefaultSkills.Charm;
        public override SkillObject SecondarySkill => DefaultSkills.Leadership;

        public override IEnumerable<CouncilPrivileges> Privileges
        {
            get
            {
                yield break;
            }
        }

        public override CouncilPosition GetCopy(Clan clan)
        {
            Elder pos = new Elder();
            pos.Initialize(null, clan);
            return pos;
        }

        public override bool IsAdequate(CouncilData data)
        {
            return data.IsRoyal && data.Clan.Culture == Utils.Helpers.GetCulture("battania");
        }

        protected override bool IsValidCandidateInternal(Hero candidate) => candidate.Age > 50;

        public class OverseeCastlesTask : CouncilTask
        {
            public override TextObject Name => new TextObject("{=!}Organize Militia");
            public override TextObject Description => new TextObject("{=!}");
            public override TextObject Effects => new TextObject("{=!}");

            public override float StandartBuildUp => 0f;
        }
    }
}
