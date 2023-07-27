using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Eastern
{
    public class SixWinds : Faith
    {
        public override TextObject GetDescriptionHint()
        {
            return new TextObject("{=!}");
        }

        public override Banner GetBanner() => new Banner("1.74.74.1836.1836.764.764.1.0.0.510.84.116.500.136.764.764.1.0.-315.510.84.116.500.136.739.744.1.0.-315.510.84.116.500.136.789.784.1.0.-315.510.84.116.500.136.764.764.1.1.314.510.84.116.500.136.789.744.1.1.314.510.84.116.500.136.739.784.1.0.314.500.84.116.550.550.764.764.1.0.-91.427.22.116.85.85.614.764.1.0.-91.150.22.116.105.125.915.764.1.0.0.521.84.116.81.73.764.604.1.0.0.145.116.116.95.85.764.924.0.0.0");

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

        public override TextObject GetCultsDescription()
        {
            throw new NotImplementedException();
        }

        public override TextObject GetFaithDescription()
        {
            throw new NotImplementedException();
        }

        public override TextObject GetFaithName()
        {
            throw new NotImplementedException();
        }

        public override string GetId()
        {
            throw new NotImplementedException();
        }

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
