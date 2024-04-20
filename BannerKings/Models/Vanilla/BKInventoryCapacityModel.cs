using BannerKings.Managers.Skills;
using BannerKings.Settings;
using BannerKings.Utils;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using static BannerKings.Utils.PerksHelpers;
using TaleWorlds.Core;

namespace BannerKings.Models.Vanilla
{
    public class BKInventoryCapacityModel : DefaultInventoryCapacityModel
    {
        public override ExplainedNumber CalculateInventoryCapacity(MobileParty mobileParty,
            bool includeDescriptions = false, int additionalTroops = 0, int additionalSpareMounts = 0,
            int additionalPackAnimals = 0, bool includeFollowers = false)
        {
            var result = base.CalculateInventoryCapacity(mobileParty, includeDescriptions, additionalTroops,
                additionalSpareMounts, additionalPackAnimals, includeFollowers);


            #region DefaultPerks.Steward.ForcedLabo
            if (BannerKingsSettings.Instance.EnableUsefulPerks && BannerKingsSettings.Instance.EnableUsefulStewardPerks)
            {
                if (mobileParty.HasPerk(DefaultPerks.Steward.ForcedLabor, false))
                {
                    // remove the original perk bonus
                    result.Add(-mobileParty.PrisonRoster.TotalHealthyCount);
                }
                ExplainedNumber prisonersCarryingCapacity = new ExplainedNumber(mobileParty.PrisonRoster.TotalHealthyCount, includeDescriptions, null);
                DefaultPerks.Steward.ForcedLabor.AddScaledPerkBonus(ref prisonersCarryingCapacity, false, mobileParty);
                result.Add(prisonersCarryingCapacity.ResultNumber, DefaultPerks.Steward.ForcedLabor.Name);
            }

            #endregion


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
}