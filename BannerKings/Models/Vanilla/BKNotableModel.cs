using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;

namespace BannerKings.Models.Vanilla
{
    public class BKNotableModel : DefaultNotableSpawnModel
    {

        public override int GetTargetNotableCountForSettlement(Settlement settlement, Occupation occupation)
        {
            if (!settlement.IsCastle) return base.GetTargetNotableCountForSettlement(settlement, occupation);
            if (occupation == Occupation.Headman)
                return 1;
            return 0;
        }
    }
}
