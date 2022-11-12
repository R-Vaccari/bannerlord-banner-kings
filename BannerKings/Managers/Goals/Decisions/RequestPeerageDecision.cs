using BannerKings.Managers.Kingdoms.Peerage;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Goals.Decisions
{
    internal class RequestPeerageDecision : Goal
    {

        public RequestPeerageDecision() : base("goal_request_peerage_decision", GoalUpdateType.Manual)
        {
            var name = new TextObject("{=!}Request Full Peerage");
            var description = new TextObject("{=!}Request the recognition of your family as a full Peer of the realm. A full Peer does not have legal restrictions on voting, starting elections, granting knighthood, hosting a council or being awarded fiefs. They are the very top of the realm's nobility. Successfully requesting Peerage will require renown (clan tier 4 minimum is recommended) and having good relations with full Peers. Holding property (caravans, workshops, estates, lordships) is a good positive factor as well.\n");

            Initialize(name, description);
        }

        internal override bool IsAvailable()
        {
            var council = BannerKingsConfig.Instance.CourtManager.GetCouncil(Clan.PlayerClan);
            return Clan.PlayerClan.Kingdom != null && Clan.PlayerClan.Kingdom.Leader != Hero.MainHero &&
                (council.Peerage == null || !council.Peerage.CanStartElection);
        }

        internal override bool IsFulfilled(out List<TextObject> failedReasons)
        {
            failedReasons = new List<TextObject>();

            if (!IsAvailable())
            {
                return false;
            }

            if (Clan.PlayerClan.IsUnderMercenaryService)
            {
                failedReasons.Add(new TextObject("{=!}Mercenaries cannot request Peerage"));
            }

            if (FactionManager.GetEnemyKingdoms(Clan.PlayerClan.Kingdom).Count() > 0)
            {
                failedReasons.Add(new TextObject("{=!}Cannot request Peerage during wars"));
            }

            var decision = new PeerageKingdomDecision(Clan.PlayerClan.Kingdom.RulingClan, Clan.PlayerClan);
            if (Clan.PlayerClan.Influence < decision.GetProposalInfluenceCost())
            {
                failedReasons.Add(GameTexts.FindText("str_decision_not_enough_influence"));
            }

            return failedReasons.Count == 0;
        }

        internal override Hero GetFulfiller()
        {
            return Hero.MainHero;
        }

        internal override void ShowInquiry()
        {
            ApplyGoal();
        }

        internal override void ApplyGoal()
        {
            var decision = new PeerageKingdomDecision(Clan.PlayerClan.Kingdom.RulingClan, Clan.PlayerClan);
            InformationManager.ShowInquiry(new InquiryData(new TextObject("{=!}Request Full Peerage").ToString(),
                new TextObject("{=!}Request full rights of Peerage. The any existing Peer with voting power may participate in the decision. Current support for the approval of {CLAN}: {SUPPORT}%.")
                .SetTextVariable("CLAN", GetFulfiller().Clan.Name)
                .SetTextVariable("SUPPORT", decision.CalculateKingdomSupport(GetFulfiller().Clan.Kingdom) * 100f)
                .ToString(), 
                true,
                true,
                GameTexts.FindText("str_selection_widget_accept").ToString(),
                GameTexts.FindText("str_selection_widget_cancel").ToString(),
                () =>
                {
                    GainKingdomInfluenceAction.ApplyForDefault(GetFulfiller(), -decision.GetProposalInfluenceCost());
                    Clan.PlayerClan.Kingdom.AddDecision(decision, true);

                    MBInformationManager.AddQuickInformation(new TextObject("{=!}The Peers of {KINGDOM} will now vote on your request.")
                        .SetTextVariable("KINGDOM", Clan.PlayerClan.Kingdom.Name),
                        0,
                        null,
                        "event:/ui/notification/relation");
                },
                null));
        }

        public override void DoAiDecision()
        {
            throw new NotImplementedException();
        }
    }
}