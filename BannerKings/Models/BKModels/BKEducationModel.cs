using BannerKings.Managers.Education;
using BannerKings.Managers.Education.Books;
using BannerKings.Managers.Education.Languages;
using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Models.BKModels
{
    public class BKEducationModel : IBannerKingsModel
    {
        public ExplainedNumber CalculateEffect(Settlement settlement) => new ExplainedNumber();

        public ExplainedNumber CalculateLessionCost(Hero student, Hero instructor)
        {
            ExplainedNumber result = new ExplainedNumber(1000f, true);

            return result;
        }

        public ExplainedNumber CalculateLanguageLimit(Hero learner)
        {
            ExplainedNumber result = new ExplainedNumber(2f, true);
            if (learner.GetPerkValue(BKPerks.Instance.ScholarshipAvidLearner)) result.Add(2f, BKPerks.Instance.ScholarshipAvidLearner.Name);
            if (learner.GetPerkValue(BKPerks.Instance.ScholarshipBookWorm)) result.Add(1f, BKPerks.Instance.ScholarshipBookWorm.Name);
            if (learner.GetPerkValue(BKPerks.Instance.ScholarshipPolyglot)) result.Add(2f, BKPerks.Instance.ScholarshipPolyglot.Name);

            return result;
        }

        public ExplainedNumber CalculateLanguageLearningRate(Hero student, Hero instructor, Language language)
        {
            ExplainedNumber result = new ExplainedNumber(1f, true);
            result.LimitMin(0f);
            result.LimitMax(5f);

            result.Add(student.GetSkillValue(BKSkills.Instance.Scholarship) * 0.01f, BKSkills.Instance.Scholarship.Name);

            EducationData data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(instructor);
            float teaching = data.GetLanguageFluency(language) - 1f;
            if (!float.IsNaN(teaching) && teaching != 0f) result.AddFactor(teaching, new TextObject("{=!}Instructor fluency"));
            
            Language native = BannerKingsConfig.Instance.EducationManager.GetNativeLanguage(student);
            MBReadOnlyDictionary<Language, float> dic = native.Inteligible;
            if (dic.ContainsKey(language)) result.Add(dic[language], new TextObject("{=!}Intelligibility with {LANGUAGE}")
                .SetTextVariable("LANGUAGE", native.Name));

            if (student.GetPerkValue(BKPerks.Instance.ScholarshipAvidLearner)) result.AddFactor(0.2f, BKPerks.Instance.ScholarshipAvidLearner.Name);

            float overLimit = data.Languages.Count - CalculateLanguageLimit(student).ResultNumber;
            if (overLimit > 0f) result.AddFactor(-0.33f * overLimit, new TextObject("{=!}Over languages limit"));

            return result;
        }

        public ExplainedNumber CalculateBookReadingRate(BookType book, Hero reader)
        {
            ExplainedNumber result = new ExplainedNumber(0f, true);
            float fluency = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(reader).GetLanguageFluency(book.Language);
            result.Add(fluency, new TextObject("{=!}{LANGUAGE} fluency").SetTextVariable("LANGUAGE", book.Language.Name));

            MBReadOnlyList<BookType> books = BannerKingsConfig.Instance.EducationManager.GetAvailableBooks(reader.PartyBelongedTo);
            if (books.Contains(DefaultBookTypes.Instance.Dictionary)) result.Add(0.2f, DefaultBookTypes.Instance.Dictionary.Name);

            if (reader.GetPerkValue(BKPerks.Instance.ScholarshipWellRead)) result.AddFactor(0.12f, BKPerks.Instance.ScholarshipWellRead.Name);
            if (reader.GetPerkValue(BKPerks.Instance.ScholarshipBookWorm)) result.AddFactor(0.20f, BKPerks.Instance.ScholarshipBookWorm.Name);

            return result;
        }
    }
}
