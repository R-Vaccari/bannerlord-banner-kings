using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Innovations
{
    public class Innovation : BannerKingsObject
    {
        [SaveableField(100)]
        private float currentProgress;
        private float requiredProgress;
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

        public void AddProgress(float points) => currentProgress += points;

        public bool Finished => currentProgress >= requiredProgress;
        public Innovation Requirement => requirement;
        public float RequiredProgress => requiredProgress;
        public float CurrentProgress => currentProgress;
        public CultureObject Culture => culture;
        public TextObject Effects => effects;
    }
}
