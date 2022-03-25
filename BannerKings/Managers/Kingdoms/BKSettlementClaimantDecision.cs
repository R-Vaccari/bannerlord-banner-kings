using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.Library;

namespace BannerKings.Managers.Kingdoms
{
    public class BKSettlementClaimantDecision : SettlementClaimantDecision
    {
        List<Clan> participants;
        bool conquestRights;
        public BKSettlementClaimantDecision(Clan proposerClan, Settlement settlement, Hero capturerHero, Clan clanToExclude, List<Clan> participants, bool conquestRights) : base(proposerClan, settlement, capturerHero, clanToExclude)
        {
            this.participants = participants;
            this.conquestRights = conquestRights;
        }

        public override IEnumerable<DecisionOutcome> DetermineInitialCandidates()
        {
            if (conquestRights)
            {
                List<SettlementClaimantDecision.ClanAsDecisionOutcome> list = new List<SettlementClaimantDecision.ClanAsDecisionOutcome>();
                foreach (Clan clan in participants)
                    if (clan != this.ClanToExclude && !clan.IsUnderMercenaryService && !clan.IsEliminated && !clan.Leader.IsDead)
                        list.Add(new SettlementClaimantDecision.ClanAsDecisionOutcome(clan));

                return list;
            }
            return base.DetermineInitialCandidates();
        }     
    }
}
