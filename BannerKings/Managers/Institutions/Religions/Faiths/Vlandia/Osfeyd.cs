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
        public override Banner GetBanner() => new Banner("11.141.145.1836.1836.768.774.1.0.0.535.40.149.175.155.864.924.0.1.225.530.84.149.175.155.574.764.0.0.89.525.40.149.175.155.619.608.0.0.45.510.6.149.69.83.770.602.0.0.-52.510.6.149.59.83.766.563.0.0.44.510.6.149.59.83.766.534.0.0.320.510.6.149.133.87.748.572.0.0.-90.533.145.149.175.155.614.904.0.0.315.510.145.149.133.87.885.605.0.0.235.510.145.149.59.83.922.583.0.0.284.131.40.149.207.208.764.764.0.1.0.510.84.149.133.87.944.764.0.0.0.510.84.149.115.87.944.764.0.0.-50.510.6.149.133.87.756.963.0.0.90.510.6.149.59.83.741.1007.0.0.135.510.6.149.59.83.770.1008.0.0.45");

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
            return new TextObject("{=!}Take heed: you need not look for answers too long. Ask yourself: what would your father do? Your grandfather? Your ancestors? There lies the answer. Great feats have they acomplished. Riding the sea, mounting their warhorses and flourishing their fields. If they have not done so - then do them yourself, each teach your sons to do so.");
        }

        public override TextObject GetClergyGreeting(int rank)
        {
            if (IsCultureNaturalFaith(Hero.MainHero.Culture)) return new TextObject("{=!}Hail, {?PLAYER.GENDER}madam{?}sir{\\?}. I see you are one of us, yet you do not follow our ancestors? A strange choice.. which I can help you remedy. As the local Hestawick, I speak four our peoples here, we who came from west-over-seas.");
            else return new TextObject("{=!}Hail, foreigner. Know that I speak for those of us here - the Wilunding - and not for your kind. The good folk here trust me to speak for them.");
        }

        public override TextObject GetClergyGreetingInducted(int rank) => new TextObject("{=!}Be welcome, {?PLAYER.GENDER}sister{?}brother{\\?}. We have a community of goods folks here such as yourself, living off the land. As their Hestawick, I guide folks on living according to our ancestors.");

        public override TextObject GetClergyInduction(int rank)
        {
            if (GetInductionAllowed(Hero.MainHero, rank).Item1)
            {
                return new TextObject("{=!}Why have you not come sooner, {?PLAYER.GENDER}madam{?}sir{\\?}? It is a shame for one us to be outside our faith. Take heed not of the Calradoi lies and other strangers that inhabit this land. For we have carved this piece for ourselves, and 'tis our duty, in this land, to defend one another.");
            }
            else
            {
                return new TextObject("{=!}Induct you? I think not.");
            }
        }

        public override TextObject GetClergyInductionLast(int rank)
        {
            if (GetInductionAllowed(Hero.MainHero, rank).Item1)
            {
                return new TextObject("{=!}Why have you not come sooner, {?PLAYER.GENDER}madam{?}sir{\\?}? It is a shame for one us to be outside our faith. Take heed not of the Calradoi lies and other strangers that inhabit this land. For we have carved this piece for ourselves, and 'tis our duty to defend one another.");
            }
            else
            {
                return new TextObject("{=!}Induct you? I think not.");
            }
        }

        public override TextObject GetClergyPreachingAnswer(int rank)
        {
            return new TextObject("{=!}I preach the canticles of our peoples, the Wilunding. From west-over-sea we came, with the promise to seize ourselves new fertile land. Our ancestors, along our hallowed gods, have so achieved, as Horse, the prophet, has foretold.");
        }

        public override TextObject GetClergyPreachingAnswerLast(int rank)
        {
            return new TextObject("{=!}The Calradoi will tell you we are usurpers, for they are arrogant and ignorant. We have merely taken ourselves what was promised us. In their ways of silk and wine, they tried to shame and fool us. We have taken up the horse and lance, and taken it for ourselves.");
        }

        public override TextObject GetClergyProveFaith(int rank) => new TextObject("{=!}");

        public override TextObject GetClergyProveFaithLast(int rank)
        {
            return new TextObject("{=!}Mount your horse and ready your lance. That is the way of the Wilunding. We shall plant the land with our spears and carve from it our kingdoms. The wights will be witness to your resolve.");
        }

        public override TextObject GetCultsDescription() => new TextObject("{=!}Gods");

        public override TextObject GetDescriptionHint()
        {
            return new TextObject("{=!}Osfeyd, or 'faith in the gods' in the Vlandic tongue, is the combined beliefs brought by the various Vlandic peoples, woven together by the prophecy of Horsa, made true by Osric Iron-Arm. In search of new fertile farmland, the Wilunding have killed the Calradic gods in the west.");
        }

        public override TextObject GetFaithDescription() => new TextObject("{=!}Osfeyd, or 'faith in the gods' in the Vlandic tongue, is the combined beliefs brought by the various Vlandic peoples, woven together by the prophecy of Horsa, made true by Osric Iron-Arm. In search of new fertile farmland, the Wilunding have killed the Calradic gods in the west. Their gods make sure they are not but mere farmers - many a man have died, pierced by their lances and bolts, such that their kingdom may thrive. This is the tale of their mighty canticle.");

        public override TextObject GetFaithName() => new TextObject("{=!}Osfeyd");

        public override string GetId() => "osfeyd";

        public override int GetIdealRank(Settlement settlement, bool isCapital) => 1;

        public override (bool, TextObject) GetInductionAllowed(Hero hero, int rank)
        {
            TextObject text = new TextObject("{=aSkNfvzG}Induction is possible.");
            bool possible =  IsCultureNaturalFaith(hero.Culture);
            if (!possible)
            {
                text = new TextObject("{=!}Your culture is not accepted.");
            }

            return new ValueTuple<bool, TextObject>(possible, text);
        }

        public override TextObject GetInductionExplanationText() => new TextObject("{=!}You need to be of a Wilunding culture (Vlandia, Osrickin, Swedaz, Rhuthuk)");

        public override Divinity GetMainDivinity() => mainGod;

        public override int GetMaxClergyRank() => 1;

        public override TextObject GetRankTitle(int rank) => new TextObject("{=!}Hestawick");

        public override MBReadOnlyList<Divinity> GetSecondaryDivinities() => new MBReadOnlyList<Divinity>(pantheon);

        public override TextObject GetZealotsGroupName() => new TextObject("{=!}Knights of Osric");

        public override bool IsCultureNaturalFaith(CultureObject culture)
        {
            string id = culture.StringId;
            return id == "vlandia" || id == "swadia" || id == "osrickin" || id == "rhodok";
        }

        public override bool IsHeroNaturalFaith(Hero hero) => IsCultureNaturalFaith(hero.Culture);
    }
}
