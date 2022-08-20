using BannerKings.Managers.Institutions.Guilds;
using BannerKings.Models;
using BannerKings.Models.Vanilla;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.SaveSystem;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Managers.Populations
{
    public class EconomicData : BannerKingsData
    {
        public EconomicData(Settlement settlement,
            Guild guild = null)
        {
            this.settlement = settlement;
            this.guild = guild;
            satisfactions = new[] {0.5f, 0.5f, 0.5f, 0.5f};
            stateSlaves = MBRandom.RandomFloatRanged(0.4f, 0.6f);
        }

        [SaveableProperty(1)] private Settlement settlement { get; }

        private Guild guild { get; set; }

        [SaveableProperty(2)] private float[] satisfactions { get; }

        [SaveableProperty(3)] private float stateSlaves { get; set; }

        public Guild Guild => guild;

        public float Corruption => 1f;

        public float Tariff => new BKTaxModel().GetTownTaxRatio(settlement.Town);

        public float StateSlaves
        {
            get
            {
                if (float.IsNaN(stateSlaves))
                {
                    stateSlaves = 0f;
                }

                return MBMath.ClampFloat(stateSlaves, 0f, 1f);
            }

            set => stateSlaves = MBMath.ClampFloat(value, 0f, 1f);
        }

        public float[] Satisfactions => satisfactions;

        public ExplainedNumber AdministrativeCost =>
            BannerKingsConfig.Instance.AdministrativeModel.CalculateEffect(settlement);

        public float MerchantRevenue => settlement.Town != null
            ? BannerKingsConfig.Instance.EconomyModel.GetMerchantIncome(settlement.Town)
            : 0f;

        public ExplainedNumber CaravanAttraction =>
            BannerKingsConfig.Instance.EconomyModel.CalculateCaravanAttraction(settlement);

        public ExplainedNumber Mercantilism => BannerKingsConfig.Instance.EconomyModel.CalculateEffect(settlement);

        public ExplainedNumber ProductionEfficiency =>
            BannerKingsConfig.Instance.EconomyModel.CalculateProductionEfficiency(settlement);

        public ExplainedNumber ProductionQuality =>
            BannerKingsConfig.Instance.EconomyModel.CalculateProductionQuality(settlement);

        public void RemoveGuild()
        {
            if (guild != null)
            {
                guild = null;
            }
        }

        public void UpdateSatisfaction(ConsumptionType type, float value)
        {
            var current = satisfactions[(int) type];
            satisfactions[(int) type] = MathF.Clamp(current + value, 0f, 1f);
        }

        internal override void Update(PopulationData data)
        {
            /*
            if (guild == null && settlement.IsTown)
            {
                Hero notable = Guild.EvaluateNewLeader(settlement);
                if (notable != null && Guild.IsSuitable(notable))
                {
                    guild = new Guild(settlement, notable, Guild.GetSuitableTrade(settlement, notable));
                    notable.AddPower(-100f);
                    InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=!}{HERO} has formed a {GUILD} at {TOWN}.")
                        .SetTextVariable("HERO", notable.Name)
                        .SetTextVariable("GUILD", guild.GuildType.Name)
                        .SetTextVariable("TOWN", settlement.Name)
                        .ToString()));
                }
            }*/
        }
    }
}