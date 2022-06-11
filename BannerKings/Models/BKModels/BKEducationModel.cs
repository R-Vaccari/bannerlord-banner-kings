using BannerKings.Managers.Education.Languages;
using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;

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

            return result;
        }
    }
}
