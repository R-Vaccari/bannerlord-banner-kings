using BannerKings.Behaviours.Diplomacy.Barterable;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Election;

namespace BannerKings.Behaviours.Diplomacy.Wars
{
    public class BKDeclareWarDecision : DeclareWarDecision
    {
        public CasusBelli CasusBelli { get; private set; }
        public BKDeclareWarDecision(CasusBelli casusBelli, Clan proposerClan, IFaction factionToDeclareWarOn) : base(proposerClan, factionToDeclareWarOn)
        {
            CasusBelli = casusBelli;
        }

        public override float DetermineSupport(Clan clan, DecisionOutcome possibleOutcome)
        {
            _ = clan.Leader;
            DeclareWarDecisionOutcome obj = (DeclareWarDecisionOutcome)possibleOutcome;
            float num = new BKWarBarterable(CasusBelli, Kingdom, FactionToDeclareWarOn).GetValueForFaction(clan);

            return obj.ShouldWarBeDeclared ? num : -num;
        }

        public override void ApplyChosenOutcome(DecisionOutcome chosenOutcome)
        {
            base.ApplyChosenOutcome(chosenOutcome);
            DeclareWarDecisionOutcome outcome = (DeclareWarDecisionOutcome)chosenOutcome;
            if (outcome.ShouldWarBeDeclared)
            {
                TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKDiplomacyBehavior>()
                    .TriggerJustifiedWar(CasusBelli, Kingdom, FactionToDeclareWarOn as Kingdom);
            }
        }
    }
}
