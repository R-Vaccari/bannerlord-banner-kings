using BannerKings.Components;
using BannerKings.Managers.Education;
using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Party;

namespace BannerKings.Models
{
    class BKPartyLimitModel : DefaultPartySizeLimitModel
    {
        public override int GetAssumedPartySizeForLordParty(Hero leaderHero, IFaction partyMapFaction, Clan actualClan) =>
            base.GetAssumedPartySizeForLordParty(leaderHero, partyMapFaction, actualClan);
        

        public override ExplainedNumber GetPartyMemberSizeLimit(PartyBase party, bool includeDescriptions = false)
        {
            ExplainedNumber baseResult = base.GetPartyMemberSizeLimit(party, includeDescriptions);
            if (party.MobileParty == null) return baseResult;

            Hero leader = party.MobileParty.LeaderHero;
            if (leader != null)
            {
                EducationData data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(leader);
                if (data.Perks.Contains(BKPerks.Instance.AugustCommander))
                    baseResult.Add(5f, BKPerks.Instance.AugustCommander.Name);
            }


            if (BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsPopulationParty(party.MobileParty))
            {
                if (party.MobileParty.PartyComponent != null && party.MobileParty.PartyComponent is PopulationPartyComponent)
                    baseResult.Add(50f);
            }

            return baseResult;
        }

        public override ExplainedNumber GetPartyPrisonerSizeLimit(PartyBase party, bool includeDescriptions = false) => 
            base.GetPartyPrisonerSizeLimit(party, includeDescriptions);
        
        public override int GetTierPartySizeEffect(int tier) => base.GetTierPartySizeEffect(tier);
        
    }
}
