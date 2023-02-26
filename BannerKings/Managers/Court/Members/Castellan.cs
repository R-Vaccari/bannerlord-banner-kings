using BannerKings.Managers.Court.Members.Tasks;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Court.Members
{
    public class Castellan : CouncilMember
    {
        public Castellan() : base("Castellan")
        {
            name = new TextObject("{=!}Castellan");
            description = new TextObject("{=!}The Castellans oversee castles and their administration. They are responsible for their prosperity, assuring constant growth and development of castle demesnes.");
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

        public override TextObject GetCulturalName()
        {
            var culture = Culture;
            return new TextObject("{=!}{?ROYAL}High Steward{?}Steward{\\?}", new Dictionary<string, object>()
            {
                { "ROYAL", IsRoyal }
            });
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
            Castellan pos = new Castellan();
            pos.Initialize(clan);
            return pos;
        }

        public override bool IsAdequate(CouncilData data)
        {
            return data.IsRoyal && data.Clan.Culture == Utils.Helpers.GetCulture("vlandia");
        }

        protected override bool IsValidCandidateInternal(Hero candidate) => true;
    }
}
