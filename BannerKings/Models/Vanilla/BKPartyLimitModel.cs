using BannerKings.Components;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Party;
using TaleWorlds.Core;

namespace BannerKings.Models
{
    class BKPartyLimitModel : CalradiaExpandedKingdoms.Models.CEKPartySizeLimitModel
    {
        public override int GetAssumedPartySizeForLordParty(Hero leaderHero, IFaction partyMapFaction, Clan actualClan) =>
            base.GetAssumedPartySizeForLordParty(leaderHero, partyMapFaction, actualClan);
        

        public override ExplainedNumber GetPartyMemberSizeLimit(PartyBase party, bool includeDescriptions = false)
        {
            ExplainedNumber baseResult = base.GetPartyMemberSizeLimit(party, includeDescriptions);
            if (party.MobileParty == null || !party.IsMobile || party.MobileParty.IsGarrison)
                return baseResult;

            if (BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsPopulationParty(party.MobileParty))
            {
                if (party.MobileParty.PartyComponent is PopulationPartyComponent)
                    baseResult.Add(50f);
            }

            return baseResult;
        }

        public override ExplainedNumber GetPartyPrisonerSizeLimit(PartyBase party, bool includeDescriptions = false) => 
            base.GetPartyPrisonerSizeLimit(party, includeDescriptions);
        
        public override int GetTierPartySizeEffect(int tier) => base.GetTierPartySizeEffect(tier);
        
    }
}
