using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;

namespace BannerKings.Models.Vanilla;

public class BKInventoryCapacityModel : DefaultInventoryCapacityModel
{
    public override ExplainedNumber CalculateInventoryCapacity(MobileParty mobileParty,
        bool includeDescriptions = false, int additionalTroops = 0, int additionalSpareMounts = 0,
        int additionalPackAnimals = 0, bool includeFollowers = false)
    {
        var result = base.CalculateInventoryCapacity(mobileParty, includeDescriptions, additionalTroops,
            additionalSpareMounts, additionalPackAnimals, includeFollowers);

        var leader = mobileParty.LeaderHero;
        if (leader != null)
        {
            var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(leader);
            if (education.HasPerk(BKPerks.Instance.CaravaneerStrider))
            {
                result.Add(mobileParty.Party.NumberOfPackAnimals * 20f, BKPerks.Instance.CaravaneerStrider.Name);
            }
        }


        return result;
    }
}