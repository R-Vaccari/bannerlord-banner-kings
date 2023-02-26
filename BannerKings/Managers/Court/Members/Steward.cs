using BannerKings.Managers.Court.Members.Tasks;
using BannerKings.Managers.Skills;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Court.Members
{
    public class Steward : CouncilPosition
    {
        public Steward() : base("Steward")
        {
        }

        public override SkillObject PrimarySkill => DefaultSkills.Steward;
        public override SkillObject SecondarySkill => BKSkills.Instance.Lordship;

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
            Steward pos = new Steward();
            pos.Initialize(clan);
            return pos;
        }

        public override TextObject GetCulturalName()
        {
            var id = Culture.StringId;
            if (IsRoyal)
            {
                if (id == "battania") return new TextObject("{=!}Ard Seansalair");
                if (id == "empire") return new TextObject("{=!}Magister Sacrarum Largitionum");

                return new TextObject("{=!}High Steward");
            }

            if (id == "battania") return new TextObject("{=!}Seansalair");
            if (id == "empire") return new TextObject("{=!}Praefectus Largitionum");

            return new TextObject("{=!}Steward");
        }

        public override bool IsAdequate(CouncilData data) => true;
        protected override bool IsValidCandidateInternal(Hero candidate) => true;
    }
}
