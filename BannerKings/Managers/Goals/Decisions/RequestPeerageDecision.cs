using BannerKings.Managers.Court;
using BannerKings.Managers.Kingdoms.Peerage;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static System.Collections.Specialized.BitVector32;

namespace BannerKings.Managers.Goals.Decisions
{
    public class RequestPeerageDecision : Goal
    {
        public RequestPeerageDecision(Hero fulfiller = null) : base("goal_request_peerage_decision", fulfiller)
        {
        }

        public override bool TickClanLeaders => true;

        public override bool TickClanMembers => false;

        public override bool TickNotables => false;

        public override GoalCategory Category => GoalCategory.Kingdom;

        public override Goal GetCopy(Hero fulfiller)
        {
            RequestPeerageDecision copy = new RequestPeerageDecision(fulfiller);
            copy.Initialize(Name, Description);
            return copy;
        }

        public override bool IsAvailable()
        {
            Clan clan = GetFulfiller().Clan;
            var council = BannerKingsConfig.Instance.CourtManager.GetCouncil(clan);
            return clan.Kingdom != null && clan.Kingdom.Leader != Hero.MainHero &&
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
                failedReasons.Add(new TextObject("{=!}You do not have enough influence ({INFLUENCE}{INFLUENCE_ICON})")
                    .SetTextVariable("INFLUENCE", decision.GetProposalInfluenceCost())
                    .SetTextVariable("INFLUENCE_ICON", Utils.TextHelper.INFLUENCE_ICON));
            }

            return failedReasons.Count == 0;
        }

        public override void ShowInquiry()
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
                () => ApplyGoal(),
                null));
        }

        public override void ApplyGoal()
        {
            Clan clan = GetFulfiller().Clan;
            var decision = new PeerageKingdomDecision(clan.Kingdom.RulingClan, clan);
            if (clan != Clan.PlayerClan)
            {
                var election = new KingdomElection(decision);
                if (election.GetLikelihoodForOutcome(0) < 0.4f) return;
            }

            clan.Kingdom.AddDecision(decision, false);
            GainKingdomInfluenceAction.ApplyForDefault(GetFulfiller(), -decision.GetProposalInfluenceCost());

            if (clan == Clan.PlayerClan)
            {
                MBInformationManager.AddQuickInformation(new TextObject("{=5YsS2g7T}The Peers of {KINGDOM} will now vote on your request.")
                .SetTextVariable("KINGDOM", Clan.PlayerClan.Kingdom.Name),
                0,
                null,
                Utils.Helpers.GetKingdomDecisionSound());
            }
        }

        public override void DoAiDecision()
        {
            List<TextObject> reasons;
            if (!IsAvailable() || !IsFulfilled(out reasons))
            {
                return;
            }

            ApplyGoal();
        }
    }
}