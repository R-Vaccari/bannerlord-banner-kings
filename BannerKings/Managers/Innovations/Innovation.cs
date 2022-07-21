using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Innovations
{
    public class Innovation : BannerKingsObject
    {
        private float requiredProgress;
        private float currentProgress;
        private CultureObject culture;
        private TextObject effects;
        private Innovation requirement;

        public Innovation(string id) : base(id)
        {

        }

        public void Initialize(TextObject name, TextObject description, TextObject effects, float requiredProgress = 1000f,
            CultureObject culture = null, Innovation requirement = null)
        {
            Initialize(name, description);
            this.effects = effects;
            this.requiredProgress = requiredProgress;
            this.culture = culture;
            this.requirement = requirement;
        }

        public Innovation Requirement => requirement;
        public float RequiredProgress => requiredProgress;
        public float CurrentProgress => currentProgress;
        public CultureObject Culture => culture;
        public TextObject Effects => effects;
    }
}
