using BannerKings.Components;
using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;

namespace BannerKings.Models;

internal class BKPartyLimitModel : DefaultPartySizeLimitModel
{
    public override int GetAssumedPartySizeForLordParty(Hero leaderHero, IFaction partyMapFaction, Clan actualClan)
    {
        return base.GetAssumedPartySizeForLordParty(leaderHero, partyMapFaction, actualClan);
    }


    public override ExplainedNumber GetPartyMemberSizeLimit(PartyBase party, bool includeDescriptions = false)
    {
        var baseResult = base.GetPartyMemberSizeLimit(party, includeDescriptions);
        if (party.MobileParty == null)
        {
            return baseResult;
        }

        var leader = party.MobileParty.LeaderHero;
        if (leader != null)
        {
            var data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(leader);
            if (data.Perks.Contains(BKPerks.Instance.AugustCommander))
            {
                baseResult.Add(5f, BKPerks.Instance.AugustCommander.Name);
            }
        }


        if (BannerKingsConfig.Instance.PopulationManager != null &&
            BannerKingsConfig.Instance.PopulationManager.IsPopulationParty(party.MobileParty))
        {
            if (party.MobileParty.PartyComponent != null &&
                party.MobileParty.PartyComponent is PopulationPartyComponent)
            {
                baseResult.Add(50f);
            }
        }

        return baseResult;
    }

    public override ExplainedNumber GetPartyPrisonerSizeLimit(PartyBase party, bool includeDescriptions = false)
    {
        return base.GetPartyPrisonerSizeLimit(party, includeDescriptions);
    }

    public override int GetTierPartySizeEffect(int tier)
    {
        return base.GetTierPartySizeEffect(tier);
    }
}