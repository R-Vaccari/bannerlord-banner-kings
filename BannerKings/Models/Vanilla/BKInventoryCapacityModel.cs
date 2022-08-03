using BannerKings.Managers.Education;
using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Party;

namespace BannerKings.Models.Vanilla
{
    public class BKInventoryCapacityModel : DefaultInventoryCapacityModel
    {
        public override ExplainedNumber CalculateInventoryCapacity(MobileParty mobileParty, bool includeDescriptions = false, int additionalTroops = 0, int additionalSpareMounts = 0, int additionalPackAnimals = 0, bool includeFollowers = false)
        {
            ExplainedNumber result =  base.CalculateInventoryCapacity(mobileParty, includeDescriptions, additionalTroops, additionalSpareMounts, additionalPackAnimals, includeFollowers);

            Hero leader = mobileParty.LeaderHero;
            if (leader != null)
            {
                EducationData education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(leader);
                if (education.HasPerk(BKPerks.Instance.CaravaneerStrider))
                    result.Add(mobileParty.Party.NumberOfPackAnimals * 20f, BKPerks.Instance.CaravaneerStrider.Name);
            }
            

            return result;
        }
    }
}
