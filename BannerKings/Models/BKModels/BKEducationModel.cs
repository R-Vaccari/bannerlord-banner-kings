using BannerKings.Managers.Education.Books;
using BannerKings.Managers.Education.Languages;
using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
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

            float teaching = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(instructor).GetLanguageFluency(language) - 1f;
            if (!float.IsNaN(teaching) && teaching != 0f) result.AddFactor(teaching,   new TextObject("{=!}Instructor fluency"));
            
            Language native = BannerKingsConfig.Instance.EducationManager.GetNativeLanguage(student);
            MBReadOnlyDictionary<Language, float> dic = native.Inteligible;
            if (dic.ContainsKey(language))
                result.Add(dic[language], new TextObject("{=!}Intelligibility with {LANGUAGE}").SetTextVariable("LANGUAGE", native.Name));


            return result;
        }

        public ExplainedNumber CalculateBookReadingRate(BookType book, Hero reader)
        {
            ExplainedNumber result = new ExplainedNumber(0f, true);
            float fluency = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(reader).GetLanguageFluency(book.Language);
            result.Add(fluency, new TextObject("{=!}{LANGUAGE} fluency").SetTextVariable("LANGUAGE", book.Language.Name));

            MBReadOnlyList<BookType> books = BannerKingsConfig.Instance.EducationManager.GetAvailableBooks(reader.PartyBelongedTo);
            if (books.Contains(DefaultBookTypes.Instance.Dictionary)) result.Add(0.2f, DefaultBookTypes.Instance.Dictionary.Name);

            return result;
        }
    }
}
