using BannerKings.Managers.Court.Members.Tasks;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Court.Members
{
    public class Elder : CouncilMember
    {
        public Elder() : base("Elder")
        {
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

        public override TextObject GetCulturalName()
        {
            var id = Culture.StringId;
            if (id == "battania") return new TextObject("{=!}Seanar");

            return new TextObject("{=!}Elder");
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
            Elder pos = new Elder();
            pos.Initialize(clan);
            return pos;
        }

        public override bool IsAdequate(CouncilData data)
        {
            return data.IsRoyal && data.Clan.Culture == Utils.Helpers.GetCulture("battania");
        }

        protected override bool IsValidCandidateInternal(Hero candidate) => candidate.Age > 50;
    }
}
