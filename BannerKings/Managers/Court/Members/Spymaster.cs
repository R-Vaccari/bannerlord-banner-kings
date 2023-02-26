using BannerKings.Managers.Court.Members.Tasks;
using BannerKings.Managers.Skills;
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

        public override SkillObject PrimarySkill => DefaultSkills.Roguery;
        public override SkillObject SecondarySkill => BKSkills.Instance.Lordship;
        public override TextObject GetCulturalName()
        {
            var id = Culture.StringId;
            if (IsRoyal)
            {
                if (id == "battania") return new TextObject("{=!}Ard Treòraiche");
                if (id == "empire") return new TextObject("{=!}Magister Officiorum");
                if (id == "khuzait") return new TextObject("{=!}Cherbi");

                return new TextObject("{=!}Grand Spymaster");
            }

            if (id == "battania") return new TextObject("{=!}Treòraiche");
            if (id == "khuzait") return new TextObject("{=!}Khevtuul");
            if (id == "empire") return new TextObject("{=!}Custodis");

            return new TextObject("{=!}Spymaster");
        }

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
            Spymaster pos = new Spymaster();
            pos.Initialize(clan);
            return pos;
        }

        public override bool IsAdequate(CouncilData data) => true;
        protected override bool IsValidCandidateInternal(Hero candidate) => true;
    }
}
