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
        public override Banner GetBanner() => new Banner("11.148.145.1836.1836.768.774.1.0.0.535.40.149.175.155.874.918.0.1.225.530.22.149.175.155.584.758.0.0.89.525.40.149.175.155.629.602.0.0.45.510.22.149.69.83.780.596.0.0.-52.510.22.149.59.83.776.557.0.0.44.510.22.149.59.83.776.528.0.0.320.510.22.149.133.87.758.566.0.0.-90.533.22.149.175.155.624.898.0.0.315.510.22.149.133.87.895.599.0.0.235.510.22.149.59.83.932.577.0.0.284.131.40.149.207.208.774.758.0.1.0.510.22.149.133.87.954.758.0.0.0.510.22.149.115.87.954.758.0.0.-50.510.22.149.133.87.766.957.0.0.90.510.22.149.59.83.751.1001.0.0.135.510.22.149.59.83.780.1002.0.0.45");

        public override TextObject GetBlessingAction() => new TextObject("{=!}I would like to pledge myself to one of the gods.");

        public override TextObject GetBlessingActionName() => new TextObject("{=!}pray to.");

        public override TextObject GetBlessingConfirmQuestion() => new TextObject("{=!}Confirm to me your devotion, lest have your name forgotten.");

        public override TextObject GetBlessingQuestion() => new TextObject("{=!}And to which of our hallowed, renowned gods to you pledge yourself today?");
        
        public override TextObject GetBlessingQuickInformation() => new TextObject("{=!}{HERO} is now pledged to {DIVINITY}.");

        public override TextObject GetClergyForbiddenAnswer(int rank) => new TextObject("{=!}Forbidden? Cravenness, of course. In the light of our ancestors, among which rank our gods - Wilund, the Smith; Horsa, the Prophet; Osric, the Conqueror - what kind of man is one who escapes in the face of the enemy? No man, I say.");
        
        public override TextObject GetClergyForbiddenAnswerLast(int rank) => new TextObject("{=!}Take heed: you need not look for answers too long. Ask yourself: what would your father do? Your grandfather? Your ancestors? There lies the answer. Great feats have they acomplished. Riding the sea, mounting their warhorses and flourishing their fields. If they have not done so - then do them yourself, each teach your sons to do so.");

        public override TextObject GetClergyGreeting(int rank)
        {
            if (IsCultureNaturalFaith(Hero.MainHero.Culture)) return new TextObject("{=!}Hail, {?PLAYER.GENDER}madam{?}sir{\\?}.");
            else return new TextObject("{=!}Hail, foreigner.");
        }

        public override TextObject GetClergyGreetingInducted(int rank) => new TextObject("{=!}Be welcome, {?PLAYER.GENDER}sister{?}brother{\\?}. We have a community here of good folks such as yourself.");

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

        public override TextObject GetClergyPreachingAnswer(int rank) => new TextObject("{=!}I preach the canticles of our peoples, the Wilunding. From west-over-sea we came, with the promise to seize ourselves new fertile land. Our ancestors, along our hallowed gods, have so achieved, as Horsa, the prophet, has foretold.");   

        public override TextObject GetClergyPreachingAnswerLast(int rank) => new TextObject("{=!}The Calradoi will tell you we are usurpers, for they are arrogant and ignorant. We have merely taken ourselves what was promised us. In their ways of silk and wine, they tried to shame and fool us. We have taken up the horse and lance, and conquered it for ourselves."); 

        public override TextObject GetClergyProveFaith(int rank) => new TextObject("{=!}Find a barrow. At evening,leave your unshod horses or a worthy blade. Sleep upon the grave mound and on the morrow your hands will be blessed with craft to create kingdoms. Or to pull them down.");

        public override TextObject GetClergyProveFaithLast(int rank) => new TextObject("{=!}Mount your horse and ready your lance. That is the way of the Wilunding. We shall plant the land with our spears and carve from it our kingdoms. The wights will be witness to your resolve.");

        public override TextObject GetCultsDescription() => new TextObject("{=!}Gods");

        public override TextObject GetDescriptionHint() => new TextObject("{=!}Osfeyd, or 'faith in the gods' in the Vlandic tongue, is the combined beliefs brought by the various Vlandic peoples, woven together by the prophecy of Horsa, made true by Osric Iron-Arm. In search of new fertile farmland, the Wilunding have killed the Calradic gods in the west.");  

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

        public override bool IsCultureNaturalFaith(CultureObject culture)
        {
            string id = culture.StringId;
            return id == "vlandia" || id == "swadia" || id == "osrickin" || id == "rhodok";
        }

        public override bool IsHeroNaturalFaith(Hero hero) => IsCultureNaturalFaith(hero.Culture);
    }
}
