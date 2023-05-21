using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Kingdoms.Peerage
{
    public class PeerageKingdomDecision : KingdomDecision
    {
        [SaveableProperty(99)] public Clan Peer { get; private set; }

        public PeerageKingdomDecision(Clan proposerClan, Clan newPeer) : base(proposerClan)
        {
            Peer = newPeer;
        }

        public float CalculateKingdomSupport(Kingdom kingdom)
        {
            var support = 0f;
            float clans = 0;
            foreach (var supporter in DetermineSupporters())
            {
                var clan = supporter.Clan;
                if (clan == Clan.PlayerClan)
                {
                    support += 100f;
                }
                else
                {
                    support += DetermineSupport(clan, new PeerageKingdomDecisionOutcome(Peer, true));
                }

                clans++;
            }

            return MBMath.ClampFloat(support / clans, 0f, 100f);
        }

        public override void ApplyChosenOutcome(DecisionOutcome chosenOutcome)
        {
            var outcome = chosenOutcome as PeerageKingdomDecisionOutcome;
            if (outcome.Approve)
            {
                var council = BannerKingsConfig.Instance.CourtManager.GetCouncil(Peer);
                council.SetPeerage(new Court.Peerage(new TextObject("{=9OhMK2Wk}Full Peerage"),
                    true,
                    true,
                    true,
                    true,
                    true,
                    false));

                if (Peer == Clan.PlayerClan)
                {
                    var peerage = council.Peerage;
                    InformationManager.ShowInquiry(new InquiryData(
                        peerage.Name.ToString(),
                        new TextObject("{=7Pzp6SQ6}The Peers of the realm now consider the {CLAN} to have {PEERAGE}. {TEXT}")
                        .SetTextVariable("CLAN", Peer.Name)
                        .SetTextVariable("PEERAGE", peerage.Name)
                        .SetTextVariable("TEXT", peerage.PeerageGrantedText())
                        .ToString(),
                        true,
                        false,
                        GameTexts.FindText("str_ok").ToString(),
                        String.Empty,
                        null,
                        null));
                }
            }
        }

        public override void ApplySecondaryEffects(MBReadOnlyList<DecisionOutcome> possibleOutcomes, DecisionOutcome chosenOutcome)
        {

        }

        public override Clan DetermineChooser() => Kingdom.RulingClan;

        public override IEnumerable<DecisionOutcome> DetermineInitialCandidates()
        {
            yield return new PeerageKingdomDecisionOutcome(Peer, true);
            yield return new PeerageKingdomDecisionOutcome(Peer);
        }

        public override void DetermineSponsors(MBReadOnlyList<DecisionOutcome> possibleOutcomes)
        {
            foreach (var decisionOutcome in possibleOutcomes)
            {
                if (((PeerageKingdomDecisionOutcome)decisionOutcome).Approve)
                {
                    decisionOutcome.SetSponsor(Peer);
                }
                else
                {
                    AssignDefaultSponsor(decisionOutcome);
                }
            }
        }

        public override float DetermineSupport(Clan clan, DecisionOutcome possibleOutcome)
        {
            float support = 0f;
            var outcome = possibleOutcome as PeerageKingdomDecisionOutcome;

            float egalitarian = clan.Leader.GetTraitLevel(DefaultTraits.Egalitarian);
            float authoritarian = clan.Leader.GetTraitLevel(DefaultTraits.Authoritarian) * - 1f;
            float oligarchic = (Peer.Tier - 3f) * clan.Leader.GetTraitLevel(DefaultTraits.Oligarchic);

            if (clan == Peer)
            {
                return 100;
            }

            support += (clan.Leader.GetRelation(Peer.Leader) / 5f) * (outcome.Approve ? 1f : -1f);
            egalitarian -= Peer.Leader.GetTraitLevel(DefaultTraits.Authoritarian) / 2f;
            oligarchic -= Peer.Leader.GetTraitLevel(DefaultTraits.Egalitarian) / 2f;
            authoritarian += Peer.Leader.GetTraitLevel(DefaultTraits.Authoritarian) / 2f;

            float property = 0f;
            property += Peer.Leader.OwnedCaravans.Count * 3f;
            property += Peer.Leader.OwnedWorkshops.Count * 3f;

            return ((support + property) * 20f) + ((egalitarian + authoritarian + oligarchic) * 60f);
        }

        public override TextObject GetChooseDescription() => new TextObject("{=dpGtisyQ}As the sovereign of {KINGDOM}, you must decide whether to approve granting full Peerage to {CLAN}.")
                .SetTextVariable("KINGDOM", Kingdom.Name)
                .SetTextVariable("CLAN", Peer.Name);

        public override TextObject GetChooseTitle() => new TextObject("{=DAEq21QX}Vote for granting full Peerage to {CLAN}")
                .SetTextVariable("CLAN", Peer.Name);

        public override TextObject GetChosenOutcomeText(DecisionOutcome chosenOutcome, SupportStatus supportStatus, bool isShortVersion = false)
        {
            var outcome = chosenOutcome as PeerageKingdomDecisionOutcome;
            if (outcome.Approve)
            {
                return new TextObject("{=O2Wi49eq}The Peers of the realm have approved {CLAN} as a new full Peer")
                    .SetTextVariable("CLAN", Peer.Name);
            }
            else
            {
                return new TextObject("{=VOmEWvW5}The Peers of the realm have denied {CLAN} as a new full Peer")
                    .SetTextVariable("CLAN", Peer.Name);
            }
        }

        public override TextObject GetGeneralTitle() => new TextObject("{=9OhMK2Wk}Full Peerage");

        public override int GetProposalInfluenceCost() => 250;

        public override DecisionOutcome GetQueriedDecisionOutcome(MBReadOnlyList<DecisionOutcome> possibleOutcomes)
        {
            possibleOutcomes.Sort((x, y) => x.Merit.CompareTo(y.Merit));
            return possibleOutcomes.First();
        }

        public override TextObject GetSecondaryEffects() => new TextObject("{=bdTS2dAa}All supporters gains some relation with each other.", null);

        public override TextObject GetSupportDescription() => new TextObject("{=6oqeHqiZ}The peers of the realm will decide on granting Peerage to {CLAN}.")
            .SetTextVariable("CLAN", Peer.Name);

        public override TextObject GetSupportTitle() => new TextObject("{=HiXMs6fF}Vote on granting Peerage to {CLAN}")
            .SetTextVariable("CLAN", Peer.Name);

        public override bool IsAllowed() => !Peer.IsUnderMercenaryService;
        

        public class PeerageKingdomDecisionOutcome : DecisionOutcome
        {
            public PeerageKingdomDecisionOutcome(Clan peer, bool approve = false)
            {
                Peer = peer;
                Approve = approve;
            }

            [SaveableProperty(200)] public Clan Peer { get; set; }
            [SaveableProperty(201)] public bool Approve { get; set; }


            public override TextObject GetDecisionTitle() => new TextObject("{=V8eQC16w}{CLAN} Peerage")
                .SetTextVariable("CLAN", Peer.Name);

            public override TextObject GetDecisionDescription()
            {
                if (Approve)
                {
                    return new TextObject("{=9bbE5rCe}We support the grant of Peerage to {CLAN}")
                        .SetTextVariable("CLAN", Peer.Name);
                }

                return new TextObject("{=LaXVFt4S}We oppose the grant of Peerage to {CLAN}")
                     .SetTextVariable("CLAN", Peer.Name);
            }


            public override string GetDecisionLink()
            {
                return null;
            }

            public override ImageIdentifier GetDecisionImageIdentifier()
            {
                return null;
            }
        }
    }
}
