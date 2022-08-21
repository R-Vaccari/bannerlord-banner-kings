using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings.Models.Vanilla
{
    public class BKNotableSpawnModel : DefaultNotableSpawnModel
    {
        public override int GetTargetNotableCountForSettlement(Settlement settlement, Occupation occupation)
        {
            if (settlement.IsCastle && occupation == Occupation.Headman) return 1;
            return base.GetTargetNotableCountForSettlement(settlement, occupation);
        }
    }
}