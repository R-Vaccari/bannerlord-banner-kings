using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Empire
{
    public class LegionariesSarapios : MonotheisticFaith
    {
        public override Banner GetBanner()
        {
            throw new NotImplementedException();
        }

        public override TextObject GetBlessingAction()
        {
            throw new NotImplementedException();
        }

        public override TextObject GetBlessingActionName()
        {
            throw new NotImplementedException();
        }

        public override TextObject GetBlessingConfirmQuestion()
        {
            throw new NotImplementedException();
        }

        public override TextObject GetBlessingQuestion()
        {
            throw new NotImplementedException();
        }

        public override TextObject GetBlessingQuickInformation()
        {
            throw new NotImplementedException();
        }

        public override TextObject GetClergyForbiddenAnswer(int rank)
        {
            throw new NotImplementedException();
        }

        public override TextObject GetClergyForbiddenAnswerLast(int rank)
        {
            throw new NotImplementedException();
        }

        public override TextObject GetClergyGreeting(int rank)
        {
            throw new NotImplementedException();
        }

        public override TextObject GetClergyGreetingInducted(int rank)
        {
            throw new NotImplementedException();
        }

        public override TextObject GetClergyInduction(int rank)
        {
            throw new NotImplementedException();
        }

        public override TextObject GetClergyInductionLast(int rank)
        {
            throw new NotImplementedException();
        }

        public override TextObject GetClergyPreachingAnswer(int rank)
        {
            throw new NotImplementedException();
        }

        public override TextObject GetClergyPreachingAnswerLast(int rank)
        {
            throw new NotImplementedException();
        }

        public override TextObject GetClergyProveFaith(int rank)
        {
            throw new NotImplementedException();
        }

        public override TextObject GetClergyProveFaithLast(int rank)
        {
            throw new NotImplementedException();
        }

        public override TextObject GetCultsDescription() => new TextObject("{=!}Cults");

        public override TextObject GetDescriptionHint()
        {
            throw new NotImplementedException();
        }

        public override TextObject GetFaithDescription() => new TextObject("{=!}Sarapios Divus was a commander of exceptional performance, to whom the founding of Epicrotea was honored. He was acclaimed Invictus in his lifetime - undefeated, a most honorable title - and Divus in his afterlife - a divinity. Though the rite becoming Divus is otherwise reserved to emperors and their families, the Senate had no choice but to concede to the demands of the deceased Sarapios' followers, thousands of legionaries and officers. So it is that rather than a state formality or political manoeuvre, his worship was a natural development, as the practice first began.");

        public override TextObject GetFaithName() => new TextObject("{=!}Legionaries of Sarapios Invictus");

        public override string GetId() => "legionaries";

        public override int GetIdealRank(Settlement settlement, bool isCapital)
        {
            throw new NotImplementedException();
        }

        public override (bool, TextObject) GetInductionAllowed(Hero hero, int rank)
        {
            throw new NotImplementedException();
        }

        public override TextObject GetInductionExplanationText()
        {
            throw new NotImplementedException();
        }

        public override Divinity GetMainDivinity()
        {
            throw new NotImplementedException();
        }

        public override int GetMaxClergyRank()
        {
            throw new NotImplementedException();
        }

        public override TextObject GetRankTitle(int rank)
        {
            throw new NotImplementedException();
        }

        public override MBReadOnlyList<Divinity> GetSecondaryDivinities()
        {
            throw new NotImplementedException();
        }

        public override bool IsCultureNaturalFaith(CultureObject culture)
        {
            throw new NotImplementedException();
        }

        public override bool IsHeroNaturalFaith(Hero hero)
        {
            throw new NotImplementedException();
        }
    }
}
