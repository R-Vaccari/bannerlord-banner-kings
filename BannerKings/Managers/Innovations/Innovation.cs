using BannerKings.Managers.Innovations.Eras;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Innovations
{
    public class Innovation : BannerKingsObject
    {
        public Innovation(string id) : base(id)
        {
        }

        public void Initialize(TextObject name, TextObject description, TextObject effects, Era era, float requiredProgress = 1000f, CultureObject culture = null, Innovation requirement = null)
        {
            Initialize(name, description);
            Effects = effects;
            RequiredProgress = requiredProgress;
            Culture = culture;
            Requirement = requirement;
            Era = era;
        } 

        public bool Finished => CurrentProgress >= RequiredProgress;
        public Era Era { get; private set; }
        public Innovation Requirement { get; private set; }
        public float RequiredProgress { get; private set; }
        [field: SaveableField(100)] public float CurrentProgress { get; private set; }
        public CultureObject Culture { get; private set; }
        public TextObject Effects { get; private set; }

        public void AddProgress(float points)
        {
            CurrentProgress += points;
        }
    }
}