using BannerKings.Managers.Institutions;
using BannerKings.Models;
using BannerKings.Models.Populations;
using BannerKings.Populations;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.SaveSystem;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Managers.Populations
{
    public class EconomicData : BannerKingsData
    {
        [SaveableProperty(1)]
        private Settlement settlement { get; set; }

        private Guild guild { get; set; }

        [SaveableProperty(2)]
        private float[] satisfactions { get; set; }

        [SaveableProperty(3)]
        private float stateSlaves { get; set; }

        public EconomicData(Settlement settlement,
            Guild guild = null)
        {
            this.settlement = settlement;
            this.guild = new Guild(settlement, Managers.Institutions.GuildType.Merchants, null);
            this.satisfactions = new float[] { 0.5f, 0.5f, 0.5f, 0.5f };
            this.stateSlaves = MBRandom.RandomFloatRanged(0.4f, 0.6f);
        }


        public Guild Guild => this.guild;

        public float Corruption => 1f;

        public float Tariff => new BKTaxModel().GetTownTaxRatio(settlement.Town);

        public float StateSlaves => this.stateSlaves;

        public float[] Satisfactions => this.satisfactions;

        public void UpdateSatisfaction(ConsumptionType type, float value)
        {
            float current = this.satisfactions[(int)type];
            this.satisfactions[(int)type] = MathF.Clamp(current + value, 0f, 1f);
        }

        internal override void Update(PopulationData data)
        {

        }

        public ExplainedNumber AdministrativeCost => BannerKingsConfig.Instance.Models
            .First(x => x.GetType() == typeof(BKAdministrativeModel)).CalculateEffect(settlement);
        public float MerchantRevenue => settlement.Town != null ? new BKEconomyModel().GetMerchantIncome(settlement.Town) : 0f;
        public ExplainedNumber CaravanAttraction
        {
            get
            {
                BKCaravanAttractionModel model = (BKCaravanAttractionModel)BannerKingsConfig.Instance.Models.First(x => x.GetType() == typeof(BKCaravanAttractionModel));
                return model.CalculateEffect(settlement);
            }
        }
        public ExplainedNumber Mercantilism
        {
            get
            {
                BKEconomyModel model = (BKEconomyModel)BannerKingsConfig.Instance.Models.First(x => x.GetType() == typeof(BKEconomyModel));
                return model.CalculateEffect(settlement);
            }
        }
        public ExplainedNumber ProductionEfficiency
        {
            get
            {
                BKEconomyModel model = (BKEconomyModel)BannerKingsConfig.Instance.Models.First(x => x.GetType() == typeof(BKEconomyModel));
                return model.CalculateProductionEfficiency(settlement);
            }
        }
        public ExplainedNumber ProductionQuality
        {
            get
            {
                BKEconomyModel model = (BKEconomyModel)BannerKingsConfig.Instance.Models.First(x => x.GetType() == typeof(BKEconomyModel));
                return model.CalculateProductionQuality(settlement);
            }
        }
    }
}
