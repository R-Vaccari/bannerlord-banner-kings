using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Populations
{
    public class CultureDataClass
    {
        public CultureDataClass(CultureObject culture, float assimilation, float acceptance)
        {
            this.culture = culture;
            this.assimilation = assimilation;
            this.acceptance = acceptance;
        }

        public void Tick(Settlement settlement, CultureData cultureData)
        {
            Settlement = settlement;
            acceptance = MathF.Clamp(acceptance + AcceptanceGain.ResultNumber, -1f, 1f);
            assimilation = cultureData.GetWeightPorportion(settlement, Culture);
        }

        [SaveableProperty(1)] private CultureObject culture { get; set; }

        [SaveableProperty(2)] private float assimilation { get; set; }

        [SaveableProperty(3)] private float acceptance { get; set; }

        public Settlement Settlement { get; private set; }

        public ExplainedNumber AcceptanceGain => BannerKingsConfig.Instance.CultureModel.CalculateAcceptanceGain(this);


        internal float Assimilation
        {
            get => assimilation;
            set => assimilation = MBMath.ClampFloat(value, 0f, 1f);
        }

        internal float Acceptance
        {
            get => acceptance;
            set => acceptance = MBMath.ClampFloat(value, 0f, 1f);
        }

        internal CultureObject Culture => culture;
    }
}