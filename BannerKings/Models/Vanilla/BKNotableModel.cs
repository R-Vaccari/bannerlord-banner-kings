using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;

namespace BannerKings.Models.Vanilla
{
    public class BKNotableModel : DefaultNotableSpawnModel
    {

        public override int GetTargetNotableCountForSettlement(Settlement settlement, Occupation occupation)
        {
            if (!settlement.IsCastle) return base.GetTargetNotableCountForSettlement(settlement, occupation);

            if (settlement.Prosperity < 3000f)
                return 1;
            if (settlement.Prosperity >= 6000f)
                return 3;
            return 2;
        }
    }
}
