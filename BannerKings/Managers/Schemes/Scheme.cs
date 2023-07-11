using TaleWorlds.CampaignSystem;

namespace BannerKings.Managers.Schemes
{
    public class Scheme : BannerKingsObject
    {
        public Scheme(string stringId) : base(stringId)
        {
        }

        public bool IsSecret { get; private set; }
        public Hero Agent { get; private set; }

        public void PostInitialize()
        {

        }

        public enum SchemeType
        {
            Diplomatic,
            Roguery
        }
    }
}
