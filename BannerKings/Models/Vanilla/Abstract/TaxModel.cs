using BannerKings.Managers.Policies;
using BannerKings.Managers.Populations;
using BannerKings.Managers.Titles;
using TaleWorlds.CampaignSystem.GameComponents;

namespace BannerKings.Models.Vanilla.Abstract
{
    public abstract class TaxModel : DefaultSettlementTaxModel
    {
        public abstract float NobleTaxOutput { get; }
        public abstract float CraftsmanTaxOutput { get; }
        public abstract float TenantTaxOutput { get; }
        public abstract float SerfTaxOutput { get; }
        public abstract float SlaveTaxOutput { get; }
        public abstract float GetNobleTaxRate(FeudalTitle title, PopulationData data, BKTaxPolicy policy);
        public abstract float GetCraftsmenTaxRate(FeudalTitle title, PopulationData data, BKTaxPolicy policy);
        public abstract float GetSlaveTaxRate(FeudalTitle title, PopulationData data, BKTaxPolicy policy);
        public abstract float GetSerfTaxRate(FeudalTitle title, PopulationData data, BKTaxPolicy policy);
        public abstract float GetTenantTaxRate(FeudalTitle title, PopulationData data, BKTaxPolicy policy);
    }
}
