using BannerKings.Managers.Court.Members.Tasks;
using BannerKings.Managers.Skills;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Court.Members
{
    public class Physician : CouncilPosition
    {
        public Physician(string id) : base(id)
        {
        }

        public override IEnumerable<CouncilTask> Tasks
        {
            get
            {
                yield return DefaultCouncilTasks.Instance.OrganizeMiltia.GetCopy();
            }
        }
        public override SkillObject PrimarySkill => DefaultSkills.Medicine;

        public override SkillObject SecondarySkill => BKSkills.Instance.Scholarship;

        public override IEnumerable<CouncilPrivileges> Privileges => throw new NotImplementedException();

        public override CouncilPosition GetCopy(Clan clan)
        {
            throw new NotImplementedException();
        }

        public override TextObject GetCulturalName()
        {
            throw new NotImplementedException();
        }

        public override bool IsAdequate(CouncilData data)
        {
            throw new NotImplementedException();
        }

        protected override bool IsValidCandidateInternal(Hero candidate)
        {
            throw new NotImplementedException();
        }
    }
}
