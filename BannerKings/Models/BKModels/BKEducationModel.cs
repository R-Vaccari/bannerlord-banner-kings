using BannerKings.Managers.Education.Books;
using BannerKings.Managers.Education.Languages;
using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Models.BKModels
{
    public class BKEducationModel : IBannerKingsModel
    {
        public ExplainedNumber CalculateEffect(Settlement settlement) => new ExplainedNumber();

        public ExplainedNumber CalculateLanguageLearningRate(Hero student, Hero instructor, Language language)
        {
            ExplainedNumber result = new ExplainedNumber(1f, true);
            result.LimitMin(1f);
            result.LimitMax(5f);

            result.Add(student.GetSkillValue(BKSkills.Instance.Scholarship) * 0.1f, BKSkills.Instance.Scholarship.Name);
            result.AddFactor(BannerKingsConfig.Instance.EducationManager.GetHeroEducation(instructor).GetLanguageFluency(language), 
                new TextObject("{=!}Instructor fluency"));

            return result;
        }

        public ExplainedNumber CalculateBookReadingRate(BookType book, Hero reader)
        {
            ExplainedNumber result = new ExplainedNumber(0f, true);
            float fluency = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(reader).GetLanguageFluency(book.Language);
            result.Add(fluency, new TextObject("{=!}{LANGUAGE} fluency").SetTextVariable("LANGUAGE", book.Language.Name));

            return result;
        }
    }
}
