using BannerKings.Managers.Skills;
using System.Collections.Generic;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using BannerKings.Managers.Traits;
using BannerKings.Settings;

namespace BannerKings.Models.Vanilla
{
    public class BKLearningModel : DefaultCharacterDevelopmentModel
    {
        private readonly int[] bkRequireXp = new int[1024];
        public BKLearningModel()
        {
            InitializeXpRequiredForSkillLevel();
        }
        private void InitializeXpRequiredForSkillLevel()
        {
            int num = 4000;
            bkRequireXp[0] = num;
            for (int i = 1; i < 1024; i++)
            {
                bkRequireXp[i] = bkRequireXp[i - 1] + (int)(20 * (1 + (i * 0.02f)));
            }
        }

        public override int GetXpRequiredForSkillLevel(int skillLevel)
        {
            if (BannerKingsSettings.Instance.AlternateLeveling)
            {
                if (skillLevel > 1024)
                {
                    skillLevel = 1024;
                }
                if (skillLevel <= 0)
                {
                    return 0;
                }
                return bkRequireXp[skillLevel - 1];
            }

            return base.GetXpRequiredForSkillLevel(skillLevel);
        }

        public override List<Tuple<SkillObject, int>> GetSkillsDerivedFromTraits(Hero hero, CharacterObject templateCharacter = null, bool isByNaturalGrowth = false)
        {
            List <Tuple<SkillObject, int>> list =  base.GetSkillsDerivedFromTraits(hero, templateCharacter, isByNaturalGrowth);
            if (hero == null)
            {
                return list;
            }

            float scholarship = 0;
            float lordship = 0;
            float theology = 0;

            if (hero.IsPreacher)
            {
                theology += 100;
                scholarship += 50;
            }

            if (templateCharacter != null)
            {
                int politician = templateCharacter.GetTraitLevel(DefaultTraits.Politician);
                int surgery = templateCharacter.GetTraitLevel(DefaultTraits.Surgery);
                int manager = templateCharacter.GetTraitLevel(DefaultTraits.Manager);

                scholarship += surgery * 10f;
                scholarship += manager * 8f;
                lordship += politician * 15f;
            }

            list.Add(new Tuple<SkillObject, int>(BKSkills.Instance.Scholarship, (int)scholarship));
            list.Add(new Tuple<SkillObject, int>(BKSkills.Instance.Lordship, (int)lordship));
            list.Add(new Tuple<SkillObject, int>(BKSkills.Instance.Theology, (int)theology));
            return list;
        }

        public override float CalculateLearningRate(Hero hero, SkillObject skill)
        {
            ExplainedNumber result = CalculateLearningRate(hero, 
                hero.GetAttributeValue(skill.CharacterAttribute), 
                hero.HeroDeveloper.GetFocus(skill), hero.GetSkillValue(skill), 
                skill.CharacterAttribute.Name);

            if (skill.CharacterAttribute == DefaultCharacterAttributes.Vigor || skill.CharacterAttribute == DefaultCharacterAttributes.Control)
            {
                result.AddFactor(hero.GetTraitLevel(BKTraits.Instance.AptitudeViolence) * 0.6f);
            }
            else if (skill.CharacterAttribute == DefaultCharacterAttributes.Social)
            {
                result.AddFactor(hero.GetTraitLevel(BKTraits.Instance.AptitudeSocializing) * 0.6f);
            }
            else if (skill.CharacterAttribute == DefaultCharacterAttributes.Intelligence || skill.CharacterAttribute == BKAttributes.Instance.Wisdom)
            {
                result.AddFactor(hero.GetTraitLevel(BKTraits.Instance.AptitudeErudition) * 0.6f);
            }

            return result.ResultNumber;
        }

        public ExplainedNumber CalculateLearningRate(Hero hero, int attributeValue, int focusValue, int skillValue, TextObject attributeName, bool includeDescriptions = false)
        {
            if (skillValue >= 500)
            {
                return new ExplainedNumber(0f);
            }
            var result = new ExplainedNumber(1.25f, includeDescriptions);
            result.AddFactor(0.4f * attributeValue, attributeName);
            result.AddFactor(focusValue * 1f, new TextObject("{=fa3Dmxdo}Skill Focus"));

            var num = MathF.Round(CalculateLearningLimit(hero, attributeValue, focusValue, null).ResultNumber);
            if (skillValue > num)
            {
                var num2 = skillValue - num;
                result.AddFactor(-1f - 0.1f * num2, new TextObject("{=fTKqtNxB}Learning Limit Exceeded"));
            }

            if (hero.GetPerkValue(BKPerks.Instance.ScholarshipMagnumOpus))
            {
                result.Add(0.02f * hero.GetSkillValue(BKSkills.Instance.Scholarship) - 230, BKPerks.Instance.ScholarshipMagnumOpus.Name);
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

        public ExplainedNumber CalculateLearningLimit(Hero hero, int attributeValue, int focusValue, TextObject attributeName, bool includeDescriptions = false)
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