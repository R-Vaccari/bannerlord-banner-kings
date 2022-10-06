using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Library;

namespace BannerKings.Extensions
{
    public static class WorkshopExtensions
    {
        public static int TaxExpenses(this Workshop workshop)
        {
            return (int)(MathF.Max(0, workshop.ProfitMade) * BannerKingsConfig.Instance.WorkshopModel
                .CalculateWorkshopTax(workshop.Settlement, workshop.Owner).ResultNumber);
        }
    }
}

