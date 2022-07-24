using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Models.Vanilla
{
    public class BKLearningModel : DefaultCharacterDevelopmentModel
    {

        public override float CalculateLearningRate(Hero hero, SkillObject skill)
        {
            int level = hero.Level;
            int attributeValue = hero.GetAttributeValue(skill.CharacterAttribute);
            int focus = hero.HeroDeveloper.GetFocus(skill);
            int skillValue = hero.GetSkillValue(skill);
            return CalculateLearningRate(hero, attributeValue, focus, skillValue, level, skill.CharacterAttribute.Name, false).ResultNumber;
        }

        public ExplainedNumber CalculateLearningRate(Hero hero, int attributeValue, int focusValue, int skillValue, int characterLevel, TextObject attributeName, bool includeDescriptions = false)
        {
            ExplainedNumber result = new ExplainedNumber(1.25f, includeDescriptions, null);
            result.AddFactor(0.4f * (float)attributeValue, attributeName);
            result.AddFactor((float)focusValue * 1f, new TextObject("{=MRktqZwu}Skill Focus", null));
            int num = MathF.Round(CalculateLearningLimit(hero, attributeValue, focusValue, null, false).ResultNumber);
            if (skillValue > num)
            {
                int num2 = skillValue - num;
                result.AddFactor(-1f - 0.1f * (float)num2, new TextObject("{=bcA7ZuyO}Learning Limit Exceeded", null));
            }

            if (hero.GetPerkValue(BKPerks.Instance.ScholarshipMagnumOpus))
                result.Add(0.02f * hero.GetSkillValue(BKSkills.Instance.Scholarship) - 230, BKPerks.Instance.ScholarshipMagnumOpus.Name);

            result.LimitMin(0.05f);
            return result;
        }

        public override ExplainedNumber CalculateLearningRate(int attributeValue, int focusValue, int skillValue, int characterLevel, TextObject attributeName, bool includeDescriptions = false)
        {
            ExplainedNumber baseResult = base.CalculateLearningRate(attributeValue, focusValue, skillValue, characterLevel, attributeName, includeDescriptions);
            baseResult.LimitMin(0.05f);
            return baseResult;
        }

        public ExplainedNumber CalculateLearningLimit(Hero hero, int attributeValue, int focusValue, TextObject attributeName, bool includeDescriptions = false)
        {
            ExplainedNumber baseResult = base.CalculateLearningLimit(attributeValue, focusValue, attributeName, includeDescriptions);
            if (hero.GetPerkValue(BKPerks.Instance.ScholarshipMagnumOpus))
                baseResult.Add(focusValue * 15f, BKPerks.Instance.ScholarshipMagnumOpus.Name);
            

            return baseResult;
        }
    }
}
