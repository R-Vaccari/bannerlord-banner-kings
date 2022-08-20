using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Kingdoms
{
    public class BKSettlementClaimantDecision : SettlementClaimantDecision
    {
        public BKSettlementClaimantDecision(Clan proposerClan, Settlement settlement, Hero capturerHero, Clan clanToExclude,
            List<Clan> participants, bool conquestRights) : base(proposerClan, settlement, capturerHero, clanToExclude)
        {
            this.participants = participants;
            this.conquestRights = conquestRights;
        }

        [SaveableProperty(200)] private List<Clan> participants { get; }

        [SaveableProperty(201)] private bool conquestRights { get; }

        public override IEnumerable<DecisionOutcome> DetermineInitialCandidates()
        {
            if (conquestRights)
            {
                var list = new List<ClanAsDecisionOutcome>();
                foreach (var clan in participants)
                {
                    if (clan != ClanToExclude && !clan.IsUnderMercenaryService && !clan.IsEliminated && !clan.Leader.IsDead)
                    {
                        list.Add(new ClanAsDecisionOutcome(clan));
                    }
                }

                return list;
            }

            return base.DetermineInitialCandidates();
        }
    }
}