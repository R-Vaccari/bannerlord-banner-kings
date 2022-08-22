using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings.Models.Vanilla
{
    public class BKNotableSpawnModel : DefaultNotableSpawnModel
    {
        public override int GetTargetNotableCountForSettlement(Settlement settlement, Occupation occupation)
        {
            return settlement.IsCastle && occupation == Occupation.Headman
                ? 1
                : base.GetTargetNotableCountForSettlement(settlement, occupation);
        }
    }
}