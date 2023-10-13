using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings.Models.Vanilla
{
    public class BKNotableSpawnModel : DefaultNotableSpawnModel
    {
        public override int GetTargetNotableCountForSettlement(Settlement settlement, Occupation occupation)
        {
            if (settlement.IsCastle)
            {
                if (occupation == Occupation.Headman)
                {
                    return 1;
                }
            }
            else
            {
                int result = base.GetTargetNotableCountForSettlement(settlement, occupation);
                if (settlement.IsTown)
                {
                    if (occupation == Occupation.Merchant)
                    {
                        if (settlement.Prosperity >= 8000)
                            result += 1;

                        if (settlement.Prosperity >= 15000)
                            result += 1;
                    }
                    else if (occupation == Occupation.Artisan)
                    {
                        if (settlement.Prosperity >= 10000)
                            result += 1;
                    }
                }
                
                if (settlement.IsVillage)
                {
                    Village village = settlement.Village;
                    if (occupation == Occupation.RuralNotable)
                    {
                        if (village.Hearth >= 1000f) result += 1;
                        if (village.Hearth >= 200f) result += 1;
                    }
                }

                return result;
            }

            return base.GetTargetNotableCountForSettlement(settlement, occupation);
        }
    }
}