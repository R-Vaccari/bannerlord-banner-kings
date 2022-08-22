using TaleWorlds.CampaignSystem;
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

        [SaveableProperty(1)] private CultureObject culture { get; set; }

        [SaveableProperty(2)] private float assimilation { get; set; }

        [SaveableProperty(3)] private float acceptance { get; set; }

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