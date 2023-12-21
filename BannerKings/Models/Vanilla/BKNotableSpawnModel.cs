using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings.Models.Vanilla
{
    public class BKNotableSpawnModel : DefaultNotableSpawnModel
    {
        public override int GetTargetNotableCountForSettlement(Settlement settlement, Occupation occupation)
        {
            int result = base.GetTargetNotableCountForSettlement(settlement, occupation);
            if (settlement.IsCastle)
            {
                if (occupation == Occupation.Merchant)
                {
                    return 1;
                }

                if (occupation == Occupation.Artisan)
                {
                    return 2;
                }
            }
            else
            {
                if (settlement.IsVillage)
                {
                    Village village = settlement.Village;
                    if (occupation == Occupation.RuralNotable)
                    {
                        if (village.Hearth >= 1000f) result += 1;
                        if (village.Hearth >= 400f) result += 1;
                    }
                }
            }

            if (settlement.Town != null)
            {
                if (occupation == Occupation.Merchant)
                {
                    if (settlement.Town.Prosperity >= 8000)
                        result += 1;

                    if (settlement.Town.Prosperity >= 15000)
                        result += 1;
                }
                else if (occupation == Occupation.Artisan)
                {
                    if (settlement.Town.Prosperity >= 10000)
                        result += 1;
                }
            }

            return result;
        }
    }
}