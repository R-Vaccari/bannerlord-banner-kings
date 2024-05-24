using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings.Managers.Goals
{
    public abstract class EmpireGoal : Goal
    {
        protected EmpireGoal(string stringId, Hero fulfiller = null) : base(stringId, fulfiller)
        {
        }

        public override bool TickClanLeaders => true;

        public override bool TickClanMembers => false;

        public override bool TickNotables => false;

        public override GoalCategory Category => GoalCategory.Unique;

        public abstract List<string> SettlementIds { get; }
        public List<Settlement> Settlements => TaleWorlds.CampaignSystem.Campaign.Current.Settlements.Where(s => SettlementIds.Contains(s.StringId)).ToList();
        public abstract string TitleId { get; }

        public abstract (float Gold, float Influence) GetCosts(Hero hero);
    }
}
