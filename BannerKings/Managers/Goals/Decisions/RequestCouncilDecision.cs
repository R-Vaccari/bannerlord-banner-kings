using BannerKings.Managers.Court;
using BannerKings.Models.BKModels;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Goals.Decisions
{
    internal class RequestCouncilDecision : Goal
    {
        private CouncilAction chosenAction;

        public RequestCouncilDecision() : base("goal_request_council_decision", GoalUpdateType.Manual)
        {
            var name = new TextObject("{=!}Request Council Position");
            var description = new TextObject("{=!}Request a position in your suzerain's council.");

            Initialize(name, description);
        }

        internal override bool IsAvailable()
        {
            return Clan.PlayerClan.Kingdom != null && Clan.PlayerClan.Kingdom.Leader != Hero.MainHero;
        }

        internal override bool IsFulfilled(out List<TextObject> failedReasons)
        {
            failedReasons = new List<TextObject>();
            return true;
        }

        internal override Hero GetFulfiller()
        {
            return Hero.MainHero;
        }

        internal override void ShowInquiry()
        {
            IsFulfilled(out var failedReasons);
            var options = new List<InquiryElement>();
            var leadingClan = Clan.PlayerClan.Kingdom.RulingClan;
            var council = BannerKingsConfig.Instance.CourtManager.GetCouncil(leadingClan);

            foreach (var member in council.AllPositions)
            {
                TextObject name = null;
                TextObject hint = new TextObject("{=!}{DESCRIPTION}\n\n{REASON}");
                CouncilAction action;
                var model = (BKCouncilModel)BannerKingsConfig.Instance.Models.First(x => x is BKCouncilModel);
                if (member.Member == Hero.MainHero)
                {
                    action = model.GetAction(CouncilActionType.RELINQUISH, council, Hero.MainHero, member);
                    hint = hint.SetTextVariable("DESCRIPTION", 
                        new TextObject("{=yYVE3O3p}Relinquish your position in the council. It will cost no influence and exempt you of any council privileges."));
                    name = new TextObject("{=!}Relinquish {POSITION}")
                       .SetTextVariable("POSITION", member.GetName());
                }
                else if (council.GetHeroPosition(Hero.MainHero) == null || member.Member == null)
                {
                    action = model.GetAction(CouncilActionType.REQUEST, council, Hero.MainHero, member);
                    hint = hint.SetTextVariable("DESCRIPTION", new TextObject("{=dcDs5auK}Request your liege to grant you this position in the council. This action will cost {INFLUENCE}{INFLUENCE_ICON}.\n\n{ACCEPT}")
                                .SetTextVariable("INFLUENCE", action.Influence));
                    name = new TextObject("{=!}Request {POSITION}")
                       .SetTextVariable("POSITION", member.GetName());
                }
                else
                {
                    action = model.GetAction(CouncilActionType.SWAP, council, Hero.MainHero, member,
                        council.GetHeroPosition(Hero.MainHero));
                    hint = hint.SetTextVariable("DESCRIPTION", new TextObject("{=ZYyxmOv9}Request to swap your current position with {COUNCILMAN} position of {POSITION}. This action will cost {INFLUENCE}{INFLUENCE_ICON}.")
                                .SetTextVariable("COUNCILMAN", action.TargetPosition.Member.Name)
                                .SetTextVariable("POSITION", action.TargetPosition.GetName())
                                .SetTextVariable("INFLUENCE", action.Influence));
                    name = new TextObject("{=!}Swap to {POSITION}")
                        .SetTextVariable("POSITION", member.GetName());
                }

                if (!action.Possible)
                {
                    hint = hint.SetTextVariable("REASON", action.Reason);
                }
                

                options.Add(new InquiryElement(action, 
                    name.ToString(), 
                    null, 
                    action.Possible,
                    hint.ToString()));
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TextObject("{=!}Request Council Position").ToString(),
                new TextObject("{=!}Choose a council position to fill. Different positions have different criteria for accepting candidates - some will be entirely blocked off, such as religious positions. Swapping with an existing lord will incur relations penalties.").ToString(),
                options, 
                true, 
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

        internal override void ApplyGoal()
        {
            chosenAction.TakeAction();
        }

        public override void DoAiDecision()
        {
            throw new NotImplementedException();
        }
    }
}