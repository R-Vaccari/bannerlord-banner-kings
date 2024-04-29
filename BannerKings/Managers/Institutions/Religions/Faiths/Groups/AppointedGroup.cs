using BannerKings.Managers.Court;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Groups
{
    public class AppointedGroup : FaithGroup
    {
        public AppointedGroup(CouncilMember member, string id) : base(id)
        {
            CouncilMember = member;
        }

        public CouncilMember CouncilMember { get; set; }
        public override bool ShouldHaveLeader => true;
        public override bool IsPreacher => false;
        public override bool IsTemporal => false;
        public override bool IsPolitical => true;

        public override TextObject Explanation => new TextObject("{=!}The representative of the {GROUPS} must be a council member fulfilling the {ROLE} council role. Valid councils are those of realms that follow a religion that is part of this faith group. Additionally, only councils of rulers are valid.")
            .SetTextVariable("GROUPS", Name)
            .SetTextVariable("ROLE", CouncilMember.Name);
    }
}
