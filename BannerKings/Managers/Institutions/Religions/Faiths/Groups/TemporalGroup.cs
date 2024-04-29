using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Groups
{
    public class TemporalGroup : FaithGroup
    {
        public TemporalGroup(string id) : base(id)
        {
        }

        public override bool ShouldHaveLeader => true;
        public override bool IsPreacher => false;
        public override bool IsTemporal => true;
        public override bool IsPolitical => false;

        public override TextObject Explanation => new TextObject("{=!}The representative of the {GROUPS} must be a preacher of one the faiths represented by the faith group. These preachers must represent their faith in their respect seat of faiths, meaning only 1 preacher per faith may qualify.")
            .SetTextVariable("GROUPS", Name);
    }
}
