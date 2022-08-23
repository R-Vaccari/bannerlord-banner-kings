using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Goals.Decisions
{
    internal class RecruitCompanionDecision : Goal
    {
        private readonly List<CompanionType> companionTypes;
        private CompanionType selectedCompanionType;

        public RecruitCompanionDecision() : base("goal_recruit_companion_decision", GoalUpdateType.Manual)
        {
            companionTypes = new List<CompanionType>
            {
                new("commander", "Commander", "A companion that meets the criteria for a Commander.", 5000, 100),
                new("thief", "Thief", "A companion that meets the criteria for a Thief.", 5000, 100),
                new("surgeon", "Surgeon", "A companion that meets the criteria for a Surgeon.", 5000, 100),
                new("caravaneer", "Caravaneer", "A companion that meets the criteria for a Caravaneer.", 5000, 100)
            };
        }

        internal override bool IsAvailable()
        {
            return true;
        }

        internal override bool IsFulfilled(out List<TextObject> failedReasons)
        {
            failedReasons = new List<TextObject>();

            var gold = GetFulfiller().Gold;
            var influence = GetFulfiller().Clan?.Influence;

            if (companionTypes.All(ct => gold < ct.GoldCost && influence < ct.InfluenceCost))
            {
                failedReasons.AddRange(companionTypes.Select(companionType => 
                    new TextObject("{=!}A {COMPANION_NAME} can't be afforded. {GOLD}{GOLD_ICON} and {INFLUENCE}{INFLUENCE_ICON} is needed.")
                        .SetTextVariable("COMPANION_NAME", companionType.Name)
                        .SetTextVariable("GOLD", companionType.GoldCost)
                        .SetTextVariable("INFLUENCE", companionType.InfluenceCost)));
            }

            return failedReasons.IsEmpty();
        }

        internal override Hero GetFulfiller()
        {
            return Hero.MainHero;
        }

        internal override void ShowInquiry()
        {
            IsFulfilled(out var failedReasons);

            var gold = GetFulfiller().Gold;
            var influence = GetFulfiller().Clan?.Influence;

            var options = new List<InquiryElement>();
            for (var index = 0; index < companionTypes.Count; index++)
            {
                var companionType = companionTypes[index];
                var enabled = gold >= companionType.GoldCost && influence >= companionType.InfluenceCost;

                options.Add(new InquiryElement(companionType,
                    companionType.Name,
                    null,
                    enabled,
                    enabled ? companionType.Description : failedReasons[index].ToString()));
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TextObject("{=!}Companions").ToString(),
                new TextObject("{=!}Choose a companion to recruit.").ToString(),
                options, 
                true, 
                1, 
                GameTexts.FindText("str_done").ToString(),
                GameTexts.FindText("str_cancel").ToString(),
                delegate (List<InquiryElement> x)
                {
                    selectedCompanionType = (CompanionType)x[0].Identifier;
                    ApplyGoal();
                }, null, string.Empty));
        }

        internal override void ApplyGoal()
        {

        }

        public override void DoAiDecision()
        {
            throw new NotImplementedException();
        }

        private class CompanionType
        {
            public CompanionType(string stringId, string name, string description, int goldCost, int influenceCost)
            {
                StringId = stringId;
                Name = name;
                Description = description;
                GoldCost = goldCost;
                InfluenceCost = influenceCost;
            }

            public string StringId { get; set; }

            public string Name { get; set; } 

            public string Description { get; set; } 

            public int GoldCost { get; set; }

            public int InfluenceCost { get; set; }
        }
    }
}