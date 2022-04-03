using BannerKings.Components;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Party;
using TaleWorlds.Core;

namespace BannerKings.Models
{
    class BKPartyLimitModel : DefaultPartySizeLimitModel
    {
        public override int GetAssumedPartySizeForLordParty(Hero leaderHero, IFaction partyMapFaction, Clan actualClan) =>
            base.GetAssumedPartySizeForLordParty(leaderHero, partyMapFaction, actualClan);
        

        public override ExplainedNumber GetPartyMemberSizeLimit(PartyBase party, bool includeDescriptions = false)
        {
            ExplainedNumber baseResult = base.GetPartyMemberSizeLimit(party, includeDescriptions);

            if (!party.IsMobile || party.MobileParty.IsGarrison)
                return baseResult;

            if (party.MobileParty.LeaderHero != null)
                if (party.MobileParty.PartyComponent.Leader.Culture.HasFeat(CalradiaExpandedKingdoms.Feats.CEKFeats.ApolssalianPositiveFeatFour))
                    baseResult.Add(CalradiaExpandedKingdoms.Feats.CEKFeats.ApolssalianPositiveFeatFour.EffectBonus, GameTexts.FindText("str_culture", null));

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
