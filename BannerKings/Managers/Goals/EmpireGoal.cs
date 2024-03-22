using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings.Managers.Goals
{
    public abstract class EmpireGoal : Goal
    {
        protected EmpireGoal(string stringId, GoalCategory goalType, GoalUpdateType goalUpdateType, Hero fulfiller = null) : base(stringId, goalType, goalUpdateType, fulfiller)
        {
        }

        public abstract List<string> SettlementIds { get; }
        public List<Settlement> Settlements => TaleWorlds.CampaignSystem.Campaign.Current.Settlements.Where(s => SettlementIds.Contains(s.StringId)).ToList();
        public abstract string TitleId { get; }

        public abstract (float Gold, float Influence) GetCosts(Hero hero);
    }
}
