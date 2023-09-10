using BannerKings.Managers.Innovations.Eras;
using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Innovations
{
    public class Innovation : BannerKingsObject
    {
        [field: SaveableField(100)] private float currentProgress;
        [field: SaveableField(101)] private CultureObject culture;
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

        public Innovation GetCopy(CultureObject culture)
        {
            Innovation innovation = DefaultInnovations.Instance.GetById(this);
            var newInnovation = new Innovation(innovation.StringId);
            newInnovation.Initialize(innovation.Name, innovation.Description, innovation.Effects, innovation.Era,
                innovation.Type, innovation.RequiredProgress, innovation.Culture, innovation.Requirement);
            newInnovation.culture = culture;
            return newInnovation;
        }

        public void PostInitialize()
        {
            Innovation innovation = DefaultInnovations.Instance.GetById(this);
            Initialize(innovation.Name, innovation.Description, innovation.Effects, innovation.Era,
                innovation.Type, innovation.RequiredProgress, innovation.Culture, innovation.Requirement);
        }

        public SkillObject ResearchSkill
        {
            get
            {
                if (Type == InnovationType.Building) return DefaultSkills.Engineering;
                if (Type == InnovationType.Military) return DefaultSkills.Tactics;
                if (Type == InnovationType.Agriculture) return DefaultSkills.Steward;
                if (Type == InnovationType.Technology) return BKSkills.Instance.Scholarship;
                return BKSkills.Instance.Lordship;
            }
        }

        public bool Finished => CurrentProgress >= RequiredProgress;
        public Era Era { get; private set; }
        public InnovationType Type { get; private set; }
        public Innovation Requirement { get; private set; }
        public float RequiredProgress { get; private set; }
       
        public float CurrentProgress
        {
            get => currentProgress;
            private set
            {
                currentProgress += value;
                currentProgress = MBMath.ClampFloat(currentProgress, 0f, RequiredProgress);
            }
        }
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
                Innovation i = (Innovation)obj;
                return i.StringId == StringId && i.culture == culture;
            }
            return base.Equals(obj);
        }
    }
}