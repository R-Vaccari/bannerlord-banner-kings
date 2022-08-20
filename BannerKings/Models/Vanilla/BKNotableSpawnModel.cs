using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings.Models.Vanilla
{
    public class BKNotableSpawnModel : DefaultNotableSpawnModel
    {
        public override int GetTargetNotableCountForSettlement(Settlement settlement, Occupation occupation)
        {
            return base.GetTargetNotableCountForSettlement(settlement, occupation);
        }
    }
}