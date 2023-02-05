using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Northern
{
    public class TreeloreFaith : PolytheisticFaith
    {
        public override bool IsCultureNaturalFaith(CultureObject culture)
        {
            if (culture.StringId == "sturgia" || culture.StringId == "vakken")
            {
                return true;
            }

            return false;
        }
        public override bool IsHeroNaturalFaith(Hero hero) => IsCultureNaturalFaith(hero.Culture);

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

        public override TextObject GetClergyGreeting(int rank) => new TextObject("{=!}Hail to you, foreigner. Know that the spirits watch over all mankind, true believers or otherwise, and that no malice escapes their senses, and no misdeed goes unrecorded by the oak grain.");

        public override TextObject GetClergyGreetingInducted(int rank) => new TextObject("{=!}Hail to you, brethren. How can I help you? Do you wish to hear the truth of the gods, or, perhaps, foretell the future?");

        public override TextObject GetClergyInduction(int rank)
        {
            var induction = GetInductionAllowed(Hero.MainHero, rank);
            if (!induction.Item1)
            {
                return new TextObject("{=!}Alas, one born outside the embrace of the gods, can not choose to be embraced. Though one can be respected for their boldness, only a child of the forest can follow the path of the true gods - it is written in our ancestry. Such is the tale written in the oak grain.");
            }

            return new TextObject("{=!}I ask of you only this - why have you not come before? My brethren of the woods, you have come to your home. The way of Pérkos, Méhns and all the gods of the heavenly canopy, that is your true nature. Your blood and bone.");
        }

        public override TextObject GetClergyInductionLast(int rank)
        {
            var induction = GetInductionAllowed(Hero.MainHero, rank);
            if (!induction.Item1)
            {
                return new TextObject("{=!}Go now, and return to those of your kind, wherever they might be. The children of the forest only accept those born in it.");
            }

            return new TextObject("{=!}Be welcome as a child of the forest. Defend your brethren and your gods - fight our enemies fiercely, but also be kind to those that visit your hearth. Do not try and convince them of our ways - it is not their place. Yet it is ours to keep unharmed.");
        }

        public override TextObject GetClergyPreachingAnswer(int rank) => new TextObject("{=!}When it comes to the gods, there is nothing the words of man can say that the rustling of leaves or burbling of rivers does not tell better. The oaks, offspring of the Great World Tree, are the chronicles of the past. Truth, you see, is embedded within all that which is godly, be it the river flow or the oak grain. I merely interpret it.");

        public override TextObject GetClergyPreachingAnswerLast(int rank) => new TextObject("{=!}But if you insist... We preach the way of the Thunder Wielder. Valor in defending your ancestry, honor in keeping your word, and generosity towards those you take into your hearth.");

        public override TextObject GetClergyProveFaith(int rank)
        {
            throw new NotImplementedException();
        }

        public override TextObject GetClergyProveFaithLast(int rank)
        {
            throw new NotImplementedException();
        }

        public override TextObject GetFaithDescription() => new TextObject("{=!}Pérkenweyd is a native faith of the Calradian continent, stretching from the Kachyar peninsula to the Chertyg mountains. Thus, it is the natural faith of the Vakken and Sturgian peoples. Though the Sturgians have been in contact with different cultures and faiths, the Sturgian populace remains true to their ancestry. Pérkenweyd, or 'tree lore', understands...");

        public override TextObject GetFaithName() => new TextObject("{=!}Pérkenweyd");
        public override string GetId() => "treelore";

        public override int GetIdealRank(Settlement settlement, bool isCapital)
        {
            if (settlement.IsVillage)
            {
                return 1;
            }

            return 0;
        }

        public override (bool, TextObject) GetInductionAllowed(Hero hero, int rank)
        {
            throw new NotImplementedException();
        }

        public override TextObject GetMainDivinitiesDescription() => new TextObject("{=!}Supreme God");

        public override Divinity GetMainDivinity() => mainGod;

        public override int GetMaxClergyRank() => 1;

        public override TextObject GetRankTitle(int rank) => new TextObject("{=!}Elder");

        public override MBReadOnlyList<Divinity> GetSecondaryDivinities() => pantheon.GetReadOnlyList();

        public override TextObject GetSecondaryDivinitiesDescription() => new TextObject("{=J4D4X2XJ}Cults");
    }
}
