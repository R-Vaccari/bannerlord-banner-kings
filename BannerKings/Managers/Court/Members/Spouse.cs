using BannerKings.Managers.Court.Members.Tasks;
using BannerKings.Managers.Skills;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Court.Members
{
    public class Spouse : CouncilMember
    {
        public Spouse() : base("Spouse")
        {
            description = new TaleWorlds.Localization.TextObject("");
        }

        public override SkillObject PrimarySkill => BKSkills.Instance.Lordship;

        public override SkillObject SecondarySkill => DefaultSkills.Steward;

        public override IEnumerable<CouncilPrivileges> Privileges
        {
            get
            {
                yield break;
            }
        }

        public override TextObject GetCulturalName()
        {
            return GameTexts.FindText("str_spouse");
        }

        public override IEnumerable<CouncilTask> Tasks
        {
            get
            {
                yield return DefaultCouncilTasks.Instance.OrganizeMiltia.GetCopy();
            }
        }
        public override CouncilMember GetCopy(Clan clan)
        {
            Spouse pos = new Spouse();
            pos.Initialize(clan);
            return pos;
        }

        public override bool IsAdequate(CouncilData data) => true;
        protected override bool IsValidCandidateInternal(Hero candidate) => candidate.Spouse == Clan.Leader;
    }
}
