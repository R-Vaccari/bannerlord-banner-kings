using BannerKings.Extensions;
using BannerKings.Managers.Populations;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Groups
{
    public class LandedPreacherGroup : FaithGroup
    {
        public LandedPreacherGroup(string id) : base(id)
        {
        }

        public override bool ShouldHaveLeader => true;
        public override bool IsPreacher => true;
        public override bool IsTemporal => false;
        public override bool IsPolitical => false;

        public override TextObject Explanation => new TextObject("{=!}The representative of the {GROUPS} must be a preacher of one the faiths represented by the faith group. These preachers must represent their faith in their respect seat of faiths, meaning only 1 preacher per faith may qualify.")
            .SetTextVariable("GROUPS", Name);

        public override void EvaluateMakeNewLeader(Religion religion)
        {
            if (Leader == null || Leader.IsAlive) return;

            Hero hero = null;
            Settlement faithSeat = religion.Faith.FaithSeat;
            PopulationData data = faithSeat.PopulationData();
            if (data.ReligionData != null && data.ReligionData.DominantReligion != null &&
                data.ReligionData.DominantReligion.Equals(religion))
            {
                foreach (var clergy in religion.Clergy)
                {
                    if (clergy.Key == faithSeat)
                    {
                        hero = clergy.Value.Hero;
                        break;
                    }
                }
            }

            if (hero != null) MakeHeroLeader(religion, hero);
        }

        public override List<Hero> EvaluatePossibleLeaders(Religion religion)
        {
            Hero result = null;
            Settlement faithSeat = religion.Faith.FaithSeat;
            foreach (var clergy in religion.Clergy)
            {
                if (clergy.Key == faithSeat)
                {
                    result = clergy.Value.Hero; 
                    break;
                } 
            }

            return new List<Hero>() { result };
        }
    }
}
