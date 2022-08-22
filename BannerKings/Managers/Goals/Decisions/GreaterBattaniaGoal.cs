using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Goals.Decisions
{
    internal class GreaterBattaniaGoal : Goal
    {
        private List<Settlement> settlements;

        public GreaterBattaniaGoal() : base("goal_greater_battania", GoalUpdateType.Settlement)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            var settlementStringIds = new List<string>
            {
                "town_B1",
                "town_B2",
                "town_B3",
                "town_B4",
                "town_B5",
                "castle_B1",
                "castle_B2",
                "castle_B3",
                "castle_B4",
                "castle_B5",
                "castle_B6",
                "castle_B7",
                "castle_B8",
                "town_V2",
                "town_EN1"
            };

            settlements = Campaign.Current.Settlements.Where(s => settlementStringIds.Contains(s.StringId)).ToList();

            if (settlementStringIds.Count != settlements.Count)
            {
                throw new BannerKingsException($"Missing settlements for {StringId} during initialization.");
            }
        }

        internal override bool IsFulfilled()
        {
            var referenceSettlement = settlements.First();
            var referenceHero = referenceSettlement.Owner;

            return referenceHero is not null && settlements.All(s => s.Owner == referenceHero);
        }

        internal override Hero GetFulfiller()
        {
            return settlements.First().Owner;
        }

        internal override TextObject GetDecisionText()
        {
            //TODO: Implement text for decision: Title, Description, Coasts and Rewards.
            var decisionText = new TextObject();

            return decisionText;
        }

        internal override void ApplyGoal()
        {
            //TODO: Apply Goal.
        }
    }
}