using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Localization;

namespace BannerKings.CampaignContent.Traits
{
    public class TraitEffect : BannerKingsObject
    {
        public TraitEffect(string stringId) : base(stringId)
        {
        }

        public void Initialize(TextObject description, TraitObject trait, SkillEffect.PerkRole role, float bonusPerPoint,
            bool addFactor = false)
        {
            this.description = description;
            Trait = trait;
            Role = role;
            BonusPerPoint = bonusPerPoint;
            AddFactor = addFactor;
        }

        public TextObject GetDescription(float traitLevel)
        {
            float result = traitLevel * BonusPerPoint;
            if (AddFactor) result *= 100f;
            string s = result.ToString("0.0");
            return Description.SetTextVariable("EFFECT", result > 0 ? '+' + s : s);
        }  

        public TraitObject Trait { get; set; }
        public SkillEffect.PerkRole Role { get; set; }
        public float BonusPerPoint { get; set; }
        public bool AddFactor {  get; set; }    
    }
}
