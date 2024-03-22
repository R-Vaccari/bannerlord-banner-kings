using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings.Models.Vanilla
{
    public abstract class VolunteerModel : DefaultVolunteerModel
    {
        public abstract ExplainedNumber GetDraftEfficiency(Hero hero, Settlement settlement);
    }
}
