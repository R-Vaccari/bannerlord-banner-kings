using System.Linq;
using BannerKings.Models.BKModels;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings.Models.Vanilla;

public class BKSettlementValueModel : DefaultSettlementValueModel
{
    public override float CalculateSettlementValueForFaction(Settlement settlement, IFaction faction)
    {
        var result = base.CalculateSettlementValueForFaction(settlement, faction);
        if (BannerKingsConfig.Instance.TitleManager != null)
        {
            var model = (BKTitleModel) BannerKingsConfig.Instance.Models.First(x =>
                x.GetType() == typeof(BKTitleModel));
            var title = BannerKingsConfig.Instance.TitleManager.GetTitle(settlement);
            if (title == null)
            {
                return result;
            }

            if (title.deJure == title.DeFacto)
            {
                result += model.GetGoldUsurpCost(title) * 3f;
                if (!settlement.IsVillage)
                {
                    foreach (var village in settlement.BoundVillages)
                    {
                        var villageTitle = BannerKingsConfig.Instance.TitleManager.GetTitle(village.Settlement);
                        if (villageTitle != null && villageTitle.deJure == settlement.Owner)
                        {
                            result += model.GetGoldUsurpCost(villageTitle) * 3f;
                        }
                    }
                }
            }
        }

        return result;
    }
}