using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;

namespace BannerKings.Models.Vanilla
{
    public class BKTroopUpgradeModel : DefaultPartyTroopUpgradeModel
    {

        public override int GetXpCostForUpgrade(PartyBase party, CharacterObject characterObject, CharacterObject upgradeTarget) =>
            (int)(base.GetXpCostForUpgrade(party, characterObject, upgradeTarget) * 2f);
        
    }
}
