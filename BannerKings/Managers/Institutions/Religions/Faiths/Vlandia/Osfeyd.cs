using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Vlandia
{
    public class Osfeyd : PolytheisticFaith
    {
        public override Banner GetBanner()
        {
            throw new NotImplementedException();
        }

        public override TextObject GetBlessingAction() => new TextObject("{=!}I would like to pledge myself to one of the gods.");

        public override TextObject GetBlessingActionName() => new TextObject("{=!}pray to.");

        public override TextObject GetBlessingConfirmQuestion() => new TextObject("{=!}Confirm to me your devotion, lest have your name forgotten.");

        public override TextObject GetBlessingQuestion()
        {
            return new TextObject("{=!}And to which of our hallowed, renowned gods to you pledge yourself today?");
        }

        public override TextObject GetBlessingQuickInformation()
        {
            return new TextObject("{=!}{HERO} is now pledged to {DIVINITY}.");
        }

        public override TextObject GetClergyForbiddenAnswer(int rank)
        {
            return new TextObject("{=!}Forbidden? Cravenness, of course. In the light of our ancestors, among which rank our gods - Wilund, the Smith; Horsa, the Prophet; Osric, the Conqueror - what kind of man is one who escapes in the face of the enemy? No man, I say.");
        }

        public override TextObject GetClergyForbiddenAnswerLast(int rank)
        {
            return new TextObject();
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

        public override TextObject GetClergyProveFaith(int rank) => new TextObject("{=!}");

        public override TextObject GetClergyProveFaithLast(int rank)
        {
            throw new NotImplementedException();
        }

        public override TextObject GetCultsDescription() => new TextObject("{=!}Gods");

        public override TextObject GetDescriptionHint()
        {
            throw new NotImplementedException();
        }

        public override TextObject GetFaithDescription() => new TextObject("{=!}Osfeyd, or 'faith in the gods' in the Vlandic tongue, is the combined beliefs brought by the various Vlandic peoples, woven together by the prophecy of Horsa, made true by Osric Iron-Arm. The Wilunding have come in search of new fertile farmland, yet their gods make sure they are not but mere farmers - many a man have died, pierced by their lances and bolts, such that their kingdom may thrive. With such might, the Wilunding have killed the Calradic gods in the west, and will kill any others possible as they write their mighty canticle.");

        public override TextObject GetFaithName() => new TextObject("{=!}Osfeyd");

        public override string GetId() => "osfeyd";

        public override int GetIdealRank(Settlement settlement, bool isCapital) => 1;

        public override (bool, TextObject) GetInductionAllowed(Hero hero, int rank)
        {
            throw new NotImplementedException();
        }

        public override TextObject GetInductionExplanationText()
        {
            throw new NotImplementedException();
        }

        public override Divinity GetMainDivinity() => mainGod;

        public override int GetMaxClergyRank() => 1;

        public override TextObject GetRankTitle(int rank) => new TextObject("{=!}Hestawick");

        public override MBReadOnlyList<Divinity> GetSecondaryDivinities() => new MBReadOnlyList<Divinity>(pantheon);

        public override bool IsCultureNaturalFaith(CultureObject culture)
        {
            string id = culture.StringId;
            return id == "vlandia" || id == "swadia" || id == "osrickin" || id == "rhodok";
        }

        public override bool IsHeroNaturalFaith(Hero hero) => IsCultureNaturalFaith(hero.Culture);
    }
}
