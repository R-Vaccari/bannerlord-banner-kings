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

        public void Initialize(TextObject name, TextObject description, TextObject effects, Era era, InnovationType type,
            float requiredProgress = 1000f, CultureObject culture = null, Innovation requirement = null)
        {
            Initialize(name, description);
            Effects = effects;
            RequiredProgress = requiredProgress;
            Culture = culture;
            Requirement = requirement;
            Era = era;
            Type = type;
        } 

        public Innovation GetCopy()
        {
            Innovation innovation = DefaultInnovations.Instance.GetById(this);
            var newInnovation = new Innovation(innovation.StringId);
            newInnovation.Initialize(innovation.Name, innovation.Description, innovation.Effects, innovation.Era,
                innovation.Type, innovation.RequiredProgress, innovation.Culture, innovation.Requirement);

            return newInnovation;
        }

        public void PostInitialize()
        {
            Innovation innovation = DefaultInnovations.Instance.GetById(this);
            Initialize(innovation.Name, innovation.Description, innovation.Effects, innovation.Era,
                innovation.Type, innovation.RequiredProgress, innovation.Culture, innovation.Requirement);
        }

        public bool Finished => CurrentProgress >= RequiredProgress;
        public Era Era { get; private set; }
        public InnovationType Type { get; private set; }
        public Innovation Requirement { get; private set; }
        public float RequiredProgress { get; private set; }
        [field: SaveableField(100)] public float CurrentProgress { get; private set; }
        public CultureObject Culture { get; private set; }
        public TextObject Effects { get; private set; }

        public void AddProgress(float points)
        {
            CurrentProgress += points;
        }

        public enum InnovationType
        {
            Civic,
            Agriculture,
            Military,
            Technology,
            Building
        }

        public override bool Equals(object obj)
        {
            if (obj is Innovation)
            {
                return (obj as Innovation).StringId == StringId;
            }
            return base.Equals(obj);
        }
    }
}