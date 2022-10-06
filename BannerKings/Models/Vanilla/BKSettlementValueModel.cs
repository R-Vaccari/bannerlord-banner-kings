using BannerKings.Utils;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings.Models.Vanilla
{
    public class BKSettlementValueModel : DefaultSettlementValueModel
    {
        public override float CalculateSettlementValueForFaction(Settlement settlement, IFaction faction)
        {
            var result = base.CalculateSettlementValueForFaction(settlement, faction);
            ExceptionUtils.TryCatch(() =>
            {
                if (BannerKingsConfig.Instance.TitleManager != null)
                {
                    var model = BannerKingsConfig.Instance.TitleModel;
                    var title = BannerKingsConfig.Instance.TitleManager.GetTitle(settlement);
                    if (title == null || title.deJure == null || title.DeFacto == null)
                    {
                        return;
                    }

                    if (title.deJure == title.DeFacto)
                    {
                        result += model.GetGoldUsurpCost(title) * 3f;
                        if (!settlement.IsVillage && settlement.BoundVillages != null)
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
            },
            GetType().Name);
           

            return result;
        }
    }
}