using BannerKings.Managers.Court.Members.Tasks;
using BannerKings.Managers.Skills;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Court.Members
{
    public class Chancellor : CouncilPosition
    {
        public Chancellor() : base("Chancellor")
        {
        }

        public override SkillObject PrimarySkill => DefaultSkills.Charm;
        public override SkillObject SecondarySkill => BKSkills.Instance.Lordship;

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
            if (IsRoyal)
            {
                if (id == "battania") return new TextObject("{=!}Ard Seansalair");
                if (id == "empire") return new TextObject("{=!}Magister Cancellarius");

                return new TextObject("{=!}High Chancellor");
            }

            if (id == "battania") return new TextObject("{=!}Seansalair");
            if (id == "empire") return new TextObject("{=!}Cancellarius");

            return new TextObject("{=!}Chancellor");
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
            Chancellor pos = new Chancellor();
            pos.Initialize(clan);
            return pos;
        }

        public override bool IsAdequate(CouncilData data) => true;
        protected override bool IsValidCandidateInternal(Hero candidate) => true;
    }
}
