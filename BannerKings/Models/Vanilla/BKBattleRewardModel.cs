using BannerKings.Managers.Education;
using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;

namespace BannerKings.Models.Vanilla
{
    public class BKBattleRewardModel : DefaultBattleRewardModel
    {

        public override ExplainedNumber CalculateRenownGain(PartyBase party, float renownValueOfBattle, float contributionShare)
        {
            var result = base.CalculateRenownGain(party, renownValueOfBattle, contributionShare);

            var leader = party.LeaderHero;
            if (leader != null)
            {
                var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(leader);
                if (education.HasPerk(BKPerks.Instance.MercenaryFamousSellswords))
                {
                    result.AddFactor(0.2f, BKPerks.Instance.MercenaryFamousSellswords.Name);
                }
            }

            return result;
        }
    }
}
