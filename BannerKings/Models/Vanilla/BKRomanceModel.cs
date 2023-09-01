using BannerKings.Managers.Traits;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;

namespace BannerKings.Models.Vanilla
{
    public class BKRomanceModel : DefaultRomanceModel
    {
        public override int GetAttractionValuePercentage(Hero potentiallyInterestedCharacter, Hero heroOfInterest)
        {
            int result = base.GetAttractionValuePercentage(potentiallyInterestedCharacter, heroOfInterest);

            result += (int)(heroOfInterest.GetTraitLevel(BKTraits.Instance.Seductive) * 15f);
            result += (int)(heroOfInterest.GetTraitLevel(BKTraits.Instance.CongenitalAttractive) * 15f);

            return result;
        }
    }
}
