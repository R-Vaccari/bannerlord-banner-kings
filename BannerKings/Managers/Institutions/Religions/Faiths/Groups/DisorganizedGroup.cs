using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Groups
{
    public class DisorganizedGroup : FaithGroup
    {
        public DisorganizedGroup(string id) : base(id)
        {
        }

        public override bool ShouldHaveLeader => false;
        public override bool IsPreacher => false;
        public override bool IsTemporal => false;
        public override bool IsPolitical => false;

        public override TextObject Explanation => new TextObject("{=!}The {GROUPS} may not have a representative.")
            .SetTextVariable("GROUPS", Name);

        public override void EvaluateMakeNewLeader(Religion religion)
        {
        }

        public override List<Hero> EvaluatePossibleLeaders(Religion religion) => new List<Hero>();
    }
}
