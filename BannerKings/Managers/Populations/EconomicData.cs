using BannerKings.Managers.Institutions.Guilds;
using BannerKings.Models.Vanilla;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.SaveSystem;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Managers.Populations
{
    public class EconomicData : BannerKingsData
    {
        public EconomicData(Settlement settlement, Guild guild = null)
        {
            this.settlement = settlement;
            this.guild = guild;
            satisfactions = new[] {0.5f, 0.5f, 0.5f, 0.5f};
            stateSlaves = MBRandom.RandomFloatRanged(0.4f, 0.6f);
        }

        [SaveableProperty(1)] private Settlement settlement { get; set; }

        private Guild guild { get; set; }

        [SaveableProperty(2)] private float[] satisfactions { get; set; }

        [SaveableProperty(3)] private float stateSlaves { get; set; }

        public Guild Guild => guild;

        public float Corruption => 1f;

        public float Tariff => new BKTaxModel().GetTownTaxRatio(settlement.Town);

        public int CaravanFee(MobileParty caravan)
        {
            int result = 0;
            if (caravan.IsCaravan)
            {
                if (caravan.MapFaction != settlement.MapFaction)
                {
                    if (caravan.MapFaction.IsKingdomFaction && settlement.MapFaction.IsKingdomFaction)
                    {
                        var diplomacy = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKDiplomacyBehavior>().GetKingdomDiplomacy(settlement.MapFaction as Kingdom);
                        if (diplomacy.HasTradePact(caravan.MapFaction as Kingdom))
                        {
                            return 0;
                        }
                    }
                    result += caravan.MemberRoster.TotalManCount;
                    result += (int)(200 * Mercantilism.ResultNumber);
                }
            }

            return result;
        }

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
            ? BannerKingsConfig.Instance.EconomyModel.GetMerchantIncome(settlement.Town).ResultNumber
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
                    InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=dbPyRhsd}{HERO} has formed a {GUILD} at {TOWN}.")
                        .SetTextVariable("HERO", notable.Name)
                        .SetTextVariable("GUILD", guild.GuildType.Name)
                        .SetTextVariable("TOWN", settlement.Name)
                        .ToString()));
                }
            }*/
        }
    }
}