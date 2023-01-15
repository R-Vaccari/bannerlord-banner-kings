using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.Library;

namespace BannerKings.Behaviours.Diplomacy.Wars
{
    public class BKDeclareWarDecision : DeclareWarDecision
    {
        public CasusBelli CasusBelli { get; private set; }
        public BKDeclareWarDecision(CasusBelli casusBelli, Clan proposerClan, IFaction factionToDeclareWarOn) : base(proposerClan, factionToDeclareWarOn)
        {
            CasusBelli = casusBelli;
        }

        public override void ApplyChosenOutcome(DecisionOutcome chosenOutcome)
        {
            base.ApplyChosenOutcome(chosenOutcome);
            InformationManager.DisplayMessage(new InformationMessage(CasusBelli.WarDeclaredText.ToString()));
        }
    }
}
