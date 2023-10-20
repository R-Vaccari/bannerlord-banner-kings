using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Vlandia
{
    public class Osfeyd : PolytheisticFaith
    {
        public override Settlement FaithSeat => Settlement.All.First(x => x.StringId == "town_V3");
        public override Banner GetBanner() => new Banner("11.148.145.1836.1836.768.774.1.0.0.535.40.149.175.155.874.918.0.1.225.530.22.22.175.155.584.758.1.0.89.525.40.149.175.155.629.602.0.0.45.510.22.149.69.83.780.596.0.0.-52.510.22.149.59.83.776.557.0.0.44.510.22.149.59.83.776.528.0.0.320.510.22.149.133.87.758.566.0.0.-90.533.22.22.175.155.624.898.1.0.315.510.22.149.133.87.895.599.0.0.235.510.22.149.59.83.932.577.0.0.284.131.40.149.207.208.774.758.0.1.0.510.22.149.133.87.954.758.0.0.0.510.22.149.115.87.954.758.0.0.-50.510.22.149.133.87.766.957.0.0.90.510.22.149.59.83.751.1001.0.0.135.510.22.149.59.83.780.1002.0.0.45");

        public override TextObject GetBlessingAction() => new TextObject("{=Xkj2BLN2}I would like to pledge myself to one of the gods.");

        public override TextObject GetBlessingActionName() => new TextObject("{=sFibPXWh}pray to.");

        public override TextObject GetBlessingConfirmQuestion() => new TextObject("{=nizT4WgK}Confirm to me your devotion, lest have your name forgotten.");

        public override TextObject GetBlessingQuestion()
        {
            return new TextObject("{=H31W0mBz}And to which of our hallowed, renowned gods to you pledge yourself today?");
        }

        public override TextObject GetBlessingQuickInformation()
        {
            return new TextObject("{=bjaNvKOZ}{HERO} is now pledged to {DIVINITY}.");
        }

        public override TextObject GetClergyForbiddenAnswer(int rank)
        {
            return new TextObject("{=vrrmrgwI}Forbidden? Cravenness, of course. In the light of our ancestors, among which rank our gods - Wilund, the Smith; Horsa, the Prophet; Osric, the Conqueror - what kind of man is one who escapes in the face of the enemy? No man, I say.");
        }

        public override TextObject GetClergyForbiddenAnswerLast(int rank)
        {
            return new TextObject("{=3Okkc85v}Take heed: you need not look for answers too long. Ask yourself: what would your father do? Your grandfather? Your ancestors? There lies the answer. Great feats have they acomplished. Riding the sea, mounting their warhorses and flourishing their fields. If they have not done so - then do them yourself, each teach your sons to do so.");
        }

        public override TextObject GetClergyGreeting(int rank)
        {
            if (IsCultureNaturalFaith(Hero.MainHero.Culture)) return new TextObject("{=oHiA5FB1}Hail, {?PLAYER.GENDER}madam{?}sir{\\?}. I see you are one of us, yet you do not follow our ancestors? A strange choice.. which I can help you remedy. As the local Hestawick, I speak four our peoples here, we who came from west-over-seas.");
            else return new TextObject("{=eXIcG968}Hail, foreigner. Know that I speak for those of us here - the Wilunding - and not for your kind. The good folk here trust me to speak for them.");
        }

        public override TextObject GetClergyGreetingInducted(int rank) => new TextObject("{=lllopLTf}Be welcome, {?PLAYER.GENDER}sister{?}brother{\\?}. We have a community of goods folks here such as yourself, living off the land. As their Hestawick, I guide folks on living according to our ancestors.");

        public override TextObject GetClergyInduction(int rank)
        {
            if (GetInductionAllowed(Hero.MainHero, rank).Item1)
            {
                return new TextObject("{=mWkgY9EA}Why have you not come sooner, {?PLAYER.GENDER}madam{?}sir{\\?}? It is a shame for one us to be outside our faith. Take heed not of the Calradoi lies and other strangers that inhabit this land. For we have carved this piece for ourselves, and 'tis our duty, in this land, to defend one another.");
            }
            else
            {
                return new TextObject("{=YTK8xi4I}Induct you? I think not. The way of the Gods is ours alone, and strangers such as yourself should not be trusted. When we first came, the stone gods of the Calradoi tried to kill us. They who appealed for our swords and lances. Liars, all of them.");
            }
        }

        public override TextObject GetClergyInductionLast(int rank)
        {
            if (GetInductionAllowed(Hero.MainHero, rank).Item1)
            {
                return new TextObject("{=KHqFm8VL}Why have you not come sooner, {?PLAYER.GENDER}madam{?}sir{\\?}? It is a shame for one us to be outside our faith. Take heed not of the Calradoi lies and other strangers that inhabit this land. For we have carved this piece for ourselves, and 'tis our duty to defend one another.");
            }
            else
            {
                return new TextObject("{=bNC6xxFA}Return to those of your kind. We Wilundings stand for each other. If you wish to prove yourself faithful, then take up lance and warhorse. Carry the lion banner, or sew your own, one that shall be sung in the taverns.");
            }
        }

        public override TextObject GetClergyPreachingAnswer(int rank)
        {
            return new TextObject("{=8N086gEI}I preach the canticles of our peoples, the Wilunding. From west-over-sea we came, with the promise to seize ourselves new fertile land. Our ancestors, along our hallowed gods, have so achieved, as Horse, the prophet, has foretold.");
        }

        public override TextObject GetClergyPreachingAnswerLast(int rank)
        {
            return new TextObject("{=axW23T8F}The Calradoi will tell you we are usurpers, for they are arrogant and ignorant. We have merely taken ourselves what was promised us. In their ways of silk and wine, they tried to shame and fool us. We have taken up the horse and lance, and taken it for ourselves.");
        }

        public override TextObject GetClergyProveFaith(int rank) => new TextObject("{=lwBlzvWO}");

        public override TextObject GetClergyProveFaithLast(int rank)
        {
            return new TextObject("{=g6lsBTSz}Mount your horse and ready your lance. That is the way of the Wilunding. We shall plant the land with our spears and carve from it our kingdoms. The wights will be witness to your resolve.");
        }

        public override TextObject GetCultsDescription() => new TextObject("{=dSQLsJkO}Gods");

        public override TextObject GetDescriptionHint()
        {
            return new TextObject("{=snv7NP9i}Osfeyd, or 'faith in the gods' in the Vlandic tongue, is the combined beliefs brought by the various Vlandic peoples, woven together by the prophecy of Horsa, made true by Osric Iron-Arm. In search of new fertile farmland, the Wilunding have killed the Calradic gods in the west.");
        }

        public override TextObject GetFaithDescription() => new TextObject("{=PCchKvO2}Osfeyd, or 'faith in the gods' in the Vlandic tongue, is the combined beliefs brought by the various Vlandic peoples, woven together by the prophecy of Horsa, made true by Osric Iron-Arm. In search of new fertile farmland, the Wilunding have killed the Calradic gods in the west. Their gods make sure they are not but mere farmers - many a man have died, pierced by their lances and bolts, such that their kingdom may thrive. This is the tale of their mighty canticle.");

        public override TextObject GetFaithName() => new TextObject("{=IlqyKSXR}Osfeyd");

        public override string GetId() => "osfeyd";

        public override int GetIdealRank(Settlement settlement) => 1;

        public override (bool, TextObject) GetInductionAllowed(Hero hero, int rank)
        {
            TextObject text = new TextObject("{=aSkNfvzG}Induction is possible.");
            bool possible =  IsCultureNaturalFaith(hero.Culture);
            if (!possible)
            {
                text = new TextObject("{=td1x7oFw}Your culture is not accepted.");
            }

            return new ValueTuple<bool, TextObject>(possible, text);
        }

        public override TextObject GetInductionExplanationText() => new TextObject("{=zpGgTSo2}You need to be of a Wilunding culture (Vlandia, Osrickin, Swedaz, Rhodok)");

        public override int GetMaxClergyRank() => 1;

        public override TextObject GetRankTitle(int rank) => new TextObject("{=bDu6pfX0}Hestawick");

        public override bool IsCultureNaturalFaith(CultureObject culture)
        {
            string id = culture.StringId;
            return id == "vlandia" || id == "swadia" || id == "osrickin" || id == "rhodok";
        }

        public override bool IsHeroNaturalFaith(Hero hero) => IsCultureNaturalFaith(hero.Culture);
    }
}
