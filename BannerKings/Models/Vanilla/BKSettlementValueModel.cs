using BannerKings.Managers.Titles;
using BannerKings.Models.BKModels;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;

namespace BannerKings.Models.Vanilla
{
    public class BKSettlementValueModel : DefaultSettlementValueModel
    {

        public override float CalculateValueForFaction(Settlement settlement, IFaction faction)
        {
            float result = base.CalculateValueForFaction(settlement, faction);
            if (BannerKingsConfig.Instance.TitleManager != null)
            {
                BKTitleModel model = (BKTitleModel)BannerKingsConfig.Instance.Models.First(x => x.GetType() == typeof(BKTitleModel));
                FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetTitle(settlement);
                if (title.deJure == title.DeFacto)
                {
                    result += model.GetGoldUsurpCost(title) * 3f;
                    if (!settlement.IsVillage)
                    {
                        foreach (Village village in settlement.BoundVillages)
                        {
                            FeudalTitle villageTitle = BannerKingsConfig.Instance.TitleManager.GetTitle(village.Settlement);
                            if (villageTitle.deJure == settlement.Owner) result += model.GetGoldUsurpCost(villageTitle) * 3f;
                        }   
                    }
                }
            }

            return result;
        }
    }
}
