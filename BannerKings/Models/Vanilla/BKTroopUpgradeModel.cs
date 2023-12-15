using BannerKings.Managers.Education.Lifestyles;
using BannerKings.Settings;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;

namespace BannerKings.Models.Vanilla
{
    public class BKTroopUpgradeModel : DefaultPartyTroopUpgradeModel
    {
        public override int GetXpCostForUpgrade(PartyBase party, CharacterObject characterObject,
            CharacterObject upgradeTarget)
        {
            var result = base.GetXpCostForUpgrade(party, characterObject, upgradeTarget) * BannerKingsSettings.Instance.TroopUpgradeXp;
            if (party != null && party.MobileParty != null && party.MobileParty.LeaderHero != null)
            {
                var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(party.MobileParty.LeaderHero);
                if (education != null && education.Lifestyle != null && education.Lifestyle.Equals(DefaultLifestyles.Instance.Cataphract))
                {
                    result *= 1.25f;
                }
            }

            return (int)result;
        }
    }
}