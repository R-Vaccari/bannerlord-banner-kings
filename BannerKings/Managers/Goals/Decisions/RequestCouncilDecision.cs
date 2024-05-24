using BannerKings.Managers.Court;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Goals.Decisions
{
    public class RequestCouncilDecision : Goal
    {
        private CouncilAction chosenAction;

        public RequestCouncilDecision(Hero fulfiller = null) : base("goal_request_council_decision", fulfiller)
        {
        }

        public override bool TickClanLeaders => true;

        public override bool TickClanMembers => false;

        public override bool TickNotables => false;

        public override GoalCategory Category => GoalCategory.Kingdom;

        public override Goal GetCopy(Hero fulfiller)
        {
            RequestCouncilDecision copy = new RequestCouncilDecision(fulfiller);
            copy.Initialize(Name, Description);
            return copy;
        }

        public override bool IsAvailable()
        {
            return Clan.PlayerClan.Kingdom != null && Clan.PlayerClan.Kingdom.Leader != Hero.MainHero;
        }

        public override bool IsFulfilled(out List<TextObject> failedReasons)
        {
            failedReasons = new List<TextObject>();
            return true;
        }

        public override void ShowInquiry()
        {
            IsFulfilled(out var failedReasons);
            var options = new List<InquiryElement>();
            Clan leadingClan = Clan.PlayerClan.Kingdom.RulingClan;
            CouncilData council = BannerKingsConfig.Instance.CourtManager.GetCouncil(leadingClan);

            foreach (CouncilMember member in council.Positions)
            {
                TextObject name = null;
                var hint = new TextObject("{=DX1iCyKA}{DESCRIPTION}\n\n{REASON}\n\nYour competence is {COMPETENCE}%:\n{EXPLANATION}");
                CouncilAction action;
                var model = BannerKingsConfig.Instance.CouncilModel;
                if (member.Member == Hero.MainHero)
                {
                    action = model.GetAction(CouncilActionType.RELINQUISH, council, Hero.MainHero, member);
                    hint = hint.SetTextVariable("DESCRIPTION", 
                        new TextObject("{=yYVE3O3p}Relinquish your position in the council. It will cost no influence and exempt you of any council privileges."));
                    name = new TextObject("{=oXv9Zi3y}Relinquish {POSITION}")
                       .SetTextVariable("POSITION", member.Name);
                }
                else if (council.GetHeroCurrentConflictingPosition(member, Hero.MainHero) == null || member.Member == null)
                {
                    action = model.GetAction(CouncilActionType.REQUEST, council, Hero.MainHero, member);
                    hint = hint.SetTextVariable("DESCRIPTION", new TextObject("{=ehHLy3bN}Request your liege to grant you this position in the council. This action will cost {INFLUENCE} influence.")
                                .SetTextVariable("INFLUENCE", action.Influence));
                    name = new TextObject("{=DwfLTc6R}Request {POSITION}")
                       .SetTextVariable("POSITION", member.Name);
                }
                else
                {
                    action = model.GetAction(CouncilActionType.SWAP, council, Hero.MainHero, member,
                        council.GetHeroCurrentConflictingPosition(member, Hero.MainHero));
                    hint = hint.SetTextVariable("DESCRIPTION", new TextObject("{=ZYyxmOv9}Request to swap your current position with {COUNCILMAN} position of {POSITION}. This action will cost {INFLUENCE}{INFLUENCE_ICON}.")
                                .SetTextVariable("COUNCILMAN", action.TargetPosition.Member.Name)
                                .SetTextVariable("POSITION", action.TargetPosition.Name)
                                .SetTextVariable("INFLUENCE", action.Influence));
                    name = new TextObject("{=tQ5eP9n6}Swap to {POSITION}")
                        .SetTextVariable("POSITION", member.Name);
                }

                if (!action.Possible)
                {
                    hint = hint.SetTextVariable("REASON", action.Reason);
                }

                ExplainedNumber competence = BannerKingsConfig.Instance.CouncilModel.CalculateHeroCompetence(Hero.MainHero,
                        member, 
                        true,
                        true);
                hint = hint.SetTextVariable("COMPETENCE", (competence.ResultNumber * 100f).ToString("0.00"))
                    .SetTextVariable("EXPLANATION", competence.GetExplanations());

                options.Add(new InquiryElement(action, 
                    name.ToString(), 
                    null, 
                    action.Possible,
                    hint.ToString()));
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TextObject("{=oBxXQmTb}Request Council Position").ToString(),
                new TextObject("{=bLxGGL9z}Choose a council position to fill. Different positions have different criteria for accepting candidates - some will be entirely blocked off, such as religious positions. Swapping with an existing lord will incur relations penalties.").ToString(),
                options, 
                true, 
                1,
                1, 
                GameTexts.FindText("str_done").ToString(),
                GameTexts.FindText("str_cancel").ToString(),
                delegate (List<InquiryElement> selectedOptions)
                {
                    chosenAction = (CouncilAction)selectedOptions.First().Identifier;
                    ApplyGoal();
                }, 
                null, 
                string.Empty));
        }

        public override void ApplyGoal()
        {
            chosenAction.TakeAction();
        }

        public override void DoAiDecision()
        {
            throw new NotImplementedException();
        }
    }
}