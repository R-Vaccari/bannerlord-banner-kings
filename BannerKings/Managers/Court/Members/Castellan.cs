using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Court.Members
{
    public class Castellan : CouncilPosition
    {
        public Castellan() : base("Castellan")
        {
        }

        public override IEnumerable<CouncilTask> Tasks
        {
            get
            {
                yield return new OverseeCastlesTask();
            }
        }

        public override SkillObject PrimarySkill => DefaultSkills.Steward;
        public override SkillObject SecondarySkill => DefaultSkills.Engineering;

        public override IEnumerable<CouncilPrivileges> Privileges
        {
            get
            {
                yield break;
            }
        }

        public override CouncilPosition GetCopy(Clan clan)
        {
            Castellan pos = new Castellan();
            pos.Initialize(null, clan);
            return pos;
        }

        public override bool IsAdequate(CouncilData data)
        {
            return data.IsRoyal && data.Clan.Culture == Utils.Helpers.GetCulture("vlandia");
        }

        protected override bool IsValidCandidateInternal(Hero candidate) => true;

        public class OverseeCastlesTask : CouncilTask
        {
            public override TextObject Name => new TextObject("{=!}Organize Militia");
            public override TextObject Description => new TextObject("{=!}");
            public override TextObject Effects => new TextObject("{=!}");

            public override float StandartBuildUp => 0f;
        }
    }
}
