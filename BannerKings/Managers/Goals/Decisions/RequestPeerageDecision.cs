using BannerKings.Managers.Kingdoms.Peerage;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Goals.Decisions
{
    internal class RequestPeerageDecision : Goal
    {
        public RequestPeerageDecision(Hero fulfiller = null) : base("goal_request_peerage_decision", GoalCategory.Kingdom, GoalUpdateType.Manual)
        {
            var name = new TextObject("{=sdpM1PD3}Request Full Peerage");
            var description = new TextObject("{=O7LLRFEX}Request the recognition of your family as a full Peer of the realm. A full Peer does not have legal restrictions on voting, starting elections, granting knighthood, hosting a council or being awarded fiefs. They are the very top of the realm's nobility. Successfully requesting Peerage will require renown (clan tier 4 minimum is recommended) and having good relations with full Peers. Holding property (caravans, workshops, estates, lordships) is a good positive factor as well.\n");
            Fulfiller = fulfiller;
            Initialize(name, description);
        }

        public override bool IsAvailable()
        {
            var council = BannerKingsConfig.Instance.CourtManager.GetCouncil(Clan.PlayerClan);
            return Clan.PlayerClan.Kingdom != null && Clan.PlayerClan.Kingdom.Leader != Hero.MainHero &&
                (council.Peerage == null || !council.Peerage.CanStartElection);
        }

        public override bool IsFulfilled(out List<TextObject> failedReasons)
        {
            failedReasons = new List<TextObject>();

            Clan clan = GetFulfiller().Clan;
            if (clan.IsUnderMercenaryService)
            {
                failedReasons.Add(new TextObject("{=SjBky9Op}Mercenaries cannot request Peerage"));
            }

            var decision = new PeerageKingdomDecision(clan.Kingdom.RulingClan, clan);
            if (clan.Influence < decision.GetProposalInfluenceCost())
            {
                failedReasons.Add(GameTexts.FindText("str_decision_not_enough_influence"));
            }

            return failedReasons.Count == 0;
        }

        public override void ShowInquiry()
        {
            ApplyGoal();
        }

        public override void ApplyGoal()
        {
            var decision = new PeerageKingdomDecision(Clan.PlayerClan.Kingdom.RulingClan, Clan.PlayerClan);
            InformationManager.ShowInquiry(new InquiryData(new TextObject("{=sdpM1PD3}Request Full Peerage").ToString(),
                new TextObject("{=HCMiSysD}Request full rights of Peerage. The any existing Peer with voting power may participate in the decision. Current support for the approval of {CLAN}: {SUPPORT}%.")
                .SetTextVariable("CLAN", GetFulfiller().Clan.Name)
                .SetTextVariable("SUPPORT", new KingdomElection(decision).GetLikelihoodForOutcome(0) * 100f)
                .ToString(), 
                true,
                true,
                GameTexts.FindText("str_selection_widget_accept").ToString(),
                GameTexts.FindText("str_selection_widget_cancel").ToString(),
                () =>
                {
                    GainKingdomInfluenceAction.ApplyForDefault(GetFulfiller(), -decision.GetProposalInfluenceCost());
                    Clan.PlayerClan.Kingdom.AddDecision(decision, true);

                    MBInformationManager.AddQuickInformation(new TextObject("{=5YsS2g7T}The Peers of {KINGDOM} will now vote on your request.")
                        .SetTextVariable("KINGDOM", Clan.PlayerClan.Kingdom.Name),
                        0,
                        null,
                        "event:/ui/notification/relation");
                },
                null));
        }

        public override void DoAiDecision()
        {
            List<TextObject> reasons;
            if (!IsFulfilled(out reasons))
            {
                return;
            }

            Clan clan = GetFulfiller().Clan;
            var decision = new PeerageKingdomDecision(clan.Kingdom.RulingClan, clan);
            var election = new KingdomElection(decision);
            if (election.GetLikelihoodForOutcome(0) < 0.4f)
            {
                return;
            }

            clan.Kingdom.AddDecision(decision, false);
        }
    }
}