using BannerKings.Managers.Court.Members.Tasks;
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
        public override TextObject GetCulturalName()
        {
            var id = Culture.StringId;
            if (IsRoyal)
            {
                if (id == "battania") return new TextObject("{=!}Ard Marasgal");
                if (id == "empire") return new TextObject("{=!}Magister Domesticus");
                if (id == "khuzait") return new TextObject("{=!}Tumetu-iin Noyan");

                return new TextObject("{=!}Grand Marshal");
            }

            if (id == "battania") return new TextObject("{=!}Marasgal");
            if (id == "khuzait") return new TextObject("{=!}Jagutu-iin Darga");
            if (id == "empire") return new TextObject("{=!}Domesticus");

            return new TextObject("{=!}Marshal");
        }

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
                yield return DefaultCouncilTasks.Instance.OrganizeMiltia.GetCopy();
            }
        }

        public override CouncilPosition GetCopy(Clan clan)
        {
            Marshal pos = new Marshal();
            pos.Initialize(clan);
            return pos;
        }

        public override bool IsAdequate(CouncilData data) => true;

        protected override bool IsValidCandidateInternal(Hero candidate) => true;
    }
}
