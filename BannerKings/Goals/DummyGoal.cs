using System.Collections.Generic;
using BannerKings.Conditions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Goals
{
    internal sealed class DummyGoal : BKGoal
    {
        private readonly HasEnoughGoldCondition hasEnoughGoldCondition;
        private readonly IsOwnerOfSettlementCondition isOwnerOfSettlementEpicroteaCondition;
        private readonly IsOwnerOfSettlementCondition isOwnerOfSettlementDiathmaCondition;
        private readonly IsOwnerOfSettlementCondition isOwnerOfSettlementSaneopaCondition;

        public DummyGoal(Hero hero) : base("goal_dummy")
        {
            Hero = hero;

            hasEnoughGoldCondition = new HasEnoughGoldCondition(5000);
            isOwnerOfSettlementEpicroteaCondition = new IsOwnerOfSettlementCondition("town_EN1");
            isOwnerOfSettlementDiathmaCondition = new IsOwnerOfSettlementCondition("town_EN2");
            isOwnerOfSettlementSaneopaCondition = new IsOwnerOfSettlementCondition("town_EN3");
        }

        public Hero Hero { get; }

        public override void Update()
        {
            
        }

        public override bool IsFulfilled(out List<TextObject> failedReasons)
        {
            failedReasons = new List<TextObject>();

            {
                if (!hasEnoughGoldCondition.Apply(Hero, out var failedReasonText))
                {
                    failedReasons.Add(failedReasonText);
                }
            }

            {
                if (!isOwnerOfSettlementEpicroteaCondition.Apply(Hero, out var failedReasonText))
                {
                    failedReasons.Add(failedReasonText);
                }
            }

            {
                if (!isOwnerOfSettlementDiathmaCondition.Apply(Hero, out var failedReasonText))
                {
                    failedReasons.Add(failedReasonText);
                }
            }

            {
                if (!isOwnerOfSettlementSaneopaCondition.Apply(Hero, out var failedReasonText))
                {
                    failedReasons.Add(failedReasonText);
                }
            }

            if (failedReasons.IsEmpty())
            {
                IsDone = true;
            }

            return failedReasons.IsEmpty();
        }
    }
}