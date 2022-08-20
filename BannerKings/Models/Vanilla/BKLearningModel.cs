using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Models.Vanilla
{
    public class BKLearningModel : DefaultCharacterDevelopmentModel
    {
        public override float CalculateLearningRate(Hero hero, SkillObject skill)
        {
            var level = hero.Level;
            var attributeValue = hero.GetAttributeValue(skill.CharacterAttribute);
            var focus = hero.HeroDeveloper.GetFocus(skill);
            var skillValue = hero.GetSkillValue(skill);
            return CalculateLearningRate(hero, attributeValue, focus, skillValue, level, skill.CharacterAttribute.Name)
                .ResultNumber;
        }

        public ExplainedNumber CalculateLearningRate(Hero hero, int attributeValue, int focusValue, int skillValue,
            int characterLevel, TextObject attributeName, bool includeDescriptions = false)
        {
            var result = new ExplainedNumber(1.25f, includeDescriptions);
            result.AddFactor(0.4f * attributeValue, attributeName);
            result.AddFactor(focusValue * 1f, new TextObject("{=MRktqZwu}Skill Focus"));
            var num = MathF.Round(CalculateLearningLimit(hero, attributeValue, focusValue, null).ResultNumber);
            if (skillValue > num)
            {
                var num2 = skillValue - num;
                result.AddFactor(-1f - 0.1f * num2, new TextObject("{=bcA7ZuyO}Learning Limit Exceeded"));
            }

            if (hero.GetPerkValue(BKPerks.Instance.ScholarshipMagnumOpus))
            {
                result.Add(0.02f * hero.GetSkillValue(BKSkills.Instance.Scholarship) - 230,
                    BKPerks.Instance.ScholarshipMagnumOpus.Name);
            }

            result.LimitMin(0.05f);
            return result;
        }

        public override ExplainedNumber CalculateLearningRate(int attributeValue, int focusValue, int skillValue,
            int characterLevel, TextObject attributeName, bool includeDescriptions = false)
        {
            var baseResult = base.CalculateLearningRate(attributeValue, focusValue, skillValue, characterLevel,
                attributeName, includeDescriptions);
            baseResult.LimitMin(0.05f);
            return baseResult;
        }

        public ExplainedNumber CalculateLearningLimit(Hero hero, int attributeValue, int focusValue,
            TextObject attributeName, bool includeDescriptions = false)
        {
            var baseResult = base.CalculateLearningLimit(attributeValue, focusValue, attributeName, includeDescriptions);
            if (hero.GetPerkValue(BKPerks.Instance.ScholarshipMagnumOpus))
            {
                baseResult.Add(focusValue * 15f, BKPerks.Instance.ScholarshipMagnumOpus.Name);
            }


            return baseResult;
        }
    }
}