using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Kingdoms
{
    public class BKSettlementClaimantDecision : SettlementClaimantDecision
    {
        [SaveableProperty(200)]
        private List<Clan> participants { get; set; }

        [SaveableProperty(201)]
        private bool conquestRights { get; set; }
        public BKSettlementClaimantDecision(Clan proposerClan, Settlement settlement, Hero capturerHero, Clan clanToExclude, List<Clan> participants, bool conquestRights) : base(proposerClan, settlement, capturerHero, clanToExclude)
        {
            this.participants = participants;
            this.conquestRights = conquestRights;
        }

        public override IEnumerable<DecisionOutcome> DetermineInitialCandidates()
        {
            if (conquestRights)
            {
                List<ClanAsDecisionOutcome> list = new List<ClanAsDecisionOutcome>();
                foreach (Clan clan in participants)
                    if (clan != ClanToExclude && !clan.IsUnderMercenaryService && !clan.IsEliminated && !clan.Leader.IsDead)
                        list.Add(new ClanAsDecisionOutcome(clan));

                return list;
            }
            return base.DetermineInitialCandidates();
        }     
    }
}
