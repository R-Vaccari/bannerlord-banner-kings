using BannerKings.Managers.Skills;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Court.Members
{
    public class Spymaster : CouncilPosition
    {
        public Spymaster() : base("Spymaster")
        {
        }

        public override IEnumerable<CouncilTask> Tasks
        {
            get
            {
                yield return new OverseeSecurityTask();
            }
        }

        public override SkillObject PrimarySkill => DefaultSkills.Roguery;
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
            Spymaster pos = new Spymaster();
            pos.Initialize(null, clan);
            return pos;
        }

        public override bool IsAdequate(CouncilData data) => true;
        protected override bool IsValidCandidateInternal(Hero candidate) => true;

        public class OverseeSecurityTask : CouncilTask
        {
            public override TextObject Name => new TextObject("{=!}Organize Militia");
            public override TextObject Description => new TextObject("{=!}");
            public override TextObject Effects => new TextObject("{=!}");

            public override float StandartBuildUp => 0f;
        }
    }
}
