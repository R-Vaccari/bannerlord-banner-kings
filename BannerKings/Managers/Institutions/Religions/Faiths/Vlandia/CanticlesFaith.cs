using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Vlandia
{
    public class CanticlesFaith : PolytheisticFaith
    {
        public override bool IsHeroNaturalFaith(Hero hero)
        {
            if (hero.Culture.StringId == "vlandia")
            {
                return true;
            }

            return false;
        }

        public override TextObject GetFaithName()
        {
            return new TextObject("{=pTg6co0a}The Canticles");
        }

        public override TextObject GetFaithDescription()
        {
            return new TextObject("{=!}The Canticles is a the history of deeds of the Vlandic peoples. It understands the world as a collection of sagas, of which all people are parts of. It does not discriminate between cultures, nor does it see itself as a 'faith', bur rather, a fact. History is a long, bloody play. There are those who bask themselves in glory, and be worthy of writting about by the scriveners, and those who will be forgotten.");
        }

        public override TextObject GetClergyForbiddenAnswer(int rank)
        {
            var text = rank switch
            {
                3 => new TextObject("{=qa1WqSC2}I fear this is lost upon the younger generation of goliards, and not considered by the scriveners of the priory; but the desecration of text and the perversion of the past. Sack your cities, purge your foes; but do not let them die nameless. Do not burn their libraries. Do not whip them until they swear oaths forsaking their bloodlines."),
                2 => new TextObject("{=cU8zusYy}n truth, nothing. History is based upon fact and truth, the Canticles tells such precedent with the storyteller’s embellishment. Villains are as necessary as heroes when they are a matter of tales. Were you to be a kinslayer, a raider, a plunderer or a butcher of our own folk you would be hated - but you would be remembered.."),
                _ => new TextObject("{=gqJz2YAJ}Cravens are unworthy of legend, and servants undefined by their merit will be forgotten by the passage of time. Little is forbidden upon the path to remembrance, should one not care if they are held as villain or savior when the victors write of the age. But a peacemaker is little remembered when war comes calling, and a courtier must deal in honeyed words rather than glorious deeds")
            };

            return text;
        }

        public override TextObject GetClergyForbiddenAnswerLast(int rank)
        {
            var text = rank switch
            {
                3 => new TextObject("{=ZZSfEks0}The tragic state of Vlandia is that we are many people who have all too often forgotten our many homelands, in favor of venerating our united purpose. Nothing is forbidden if one wishes to be known in history; but it is a blasphemy to destroy history to be known in legend. "),
                2 => new TextObject("{=cPbaoroQ}I would ask though, that you pursue your place in the saga with a kind heart; there are ample enemies enough in the ranks of the gentry and beyond our borders. We’ve little need for more souls of monstrous heart. "),
                _ => new TextObject("{=LjwbtMO6}You will not be shunned or excommunicated by falling into humility and servile unimportance; you will simply be unworthy of remembrance. An epithet upon your grave plank will be all you deserve then, a name that will fade and molder within a generation.")
            };

            return text;
        }

        public override TextObject GetClergyGreeting(int rank)
        {
            var text = rank switch
            {
                3 => new TextObject("{=5sZwfBhS}Is there something that I can say or do to be of use to you, young master? Our time in this world is short indeed, and mine all the shorter by virtue - or perhaps malus - of my age. Did you come to greet an old master and learn the way of the Canticles? If so, I beg you find a goliard - my voice does not carry with gusto as theirs does, and my words may seem withered by comparison. "),
                2 when Hero.MainHero.Clan.Tier <= 3 => new TextObject("{=Zt7QAgx1}Stranger if you were led to me for the services of a town scribe, know that I work only in vellum and that unless you have procured me wax tablets all your own, there is little scrivener can offer you in terms of an education. It is not my job nor responsibility to tend to the illiterate, for I pen the Canticles and lest you somehow be a notable beyond my knowing - I see no cause to enshrine your name in such sacred work."),
                2 => new TextObject("{=nHWeQ7Lz}Hmm? Oh yes, greetings. I feel as though I know you, or perhaps if not you then the deeds of one much like you stranger. You bear an uncanny resemblance to one whose deeds have reached the likes of we humble scriveners. I cannot be certain if I have penned your name or face in silverpoint, but I would be pleased to make your acquaintance should you be that soul. It would be my sacred honor, stranger, to know you."),
                _ => new TextObject("{=C2oSmjZN}Hark, thy nameless and forgotten soul! Come wit and make yourself known such that you might dwell in my mind and be more than but a dim candle among bon fires! Do you not look upon the world and think yourself a lesser than your neighbors? Sons and daughters - children even! The young and the old make themselves worthy of praise, and yet among them you remain unworthy of pen and purpose. Do you not wish to be remembered?")
            };

            return text;
        }

        public override TextObject GetClergyGreetingInducted(int rank)
        {
            var text = rank switch
            {
                3 => new TextObject("{=LVkWL6Vb}Ah yes, another soul upon the path towards eternity; seeking glory and remembrance within the Canticles. The years grow long, and yet I feel all the shorter for them. Time bends the elderly with the weight of their deeds. What do you wish to share with me, and why is it me that you seek over scriveners and goliards?"),
                2 => new TextObject("{=sKB4uXQp}O soul, how goes your tale? Worthy of telling, or on the ascent to such I am sure. Hero or villain, king or killer, I welcome you. Let us speak openly and in full truth, and may we be judged for our words amongst the whole of the Canticles.")
                    .SetTextVariable("NAME", Hero.MainHero.Name),
                _ => new TextObject("{=x4zmztr7}Hail to this one, and may all know their tale! Their name is spoken of, it is whispered and it is sung on wagging tongues - by goliard and troubadour alike! But are they hero or are they villain? A knave, a rake, a blaggard or a savior mayhaps? The Canticles are not yet written in full, the song not yet ended - but does that haunt you or inspire you? How history will see you known?")
            };

            return text;
        }

        public override TextObject GetClergyInduction(int rank)
        {
            TextObject text = null;

            if (rank == 3)
            {
                text = Hero.MainHero.Clan.Tier switch
                {
                    >= 5 => new TextObject("{=hkuoXPq0}Rare have been the days in my life that royals and kingkillers, emperors and sultans would come to speak to me. It has happened before, my words offered little comfort. Powerful rulers seek always to control how they are viewed, how they will be remembered; and in pursuit of eternity they damn themselves by neglecting the present. Your deeds and influence are those which will be penned in our grand works as a culture; but I pray that you do not forget that you live only in this age."),
                    >= 3 => new TextObject("{=TuFp5arq}I have heard of you, even seen the proper pigments used by one who may have potential to be my successor. Their work venerates you, and yet you do not know their name as they know yours. As I know yours."),
                    _ => new TextObject("{=x5xG6QFp}For what purpose do you think this shall bring you comfort? You are young in the eyes of the saga; potential abounds around you. Fate shall give you what it is you require, and you - if you are bold - shall take up that which you desire.")
                };
            }

            if (rank == 2)
            {
                text = Hero.MainHero.Clan.Tier switch
                {
                    >= 5 => new TextObject("{=n9fsvyXc}I am made elevated by your presence, for yours is a name sung upon many a verse within the Canticles. The young novices have ground many a gall nut to properly pen your deeds, and though I have seen your face in silverpoint before I know now how to better see it illuminated."),
                    >= 3 => new TextObject("{=pozjMrDc}O soul, know that I have heard your name already and recognize you by your mien. It is a kindness in the Canticles, to have one’s reputation precede them. Your name is penned in my volumes where appropriate and important."),
                    _ => new TextObject("{=dLm9SktX}I applaud your desire, but you are not unique in this tale. Not yet. You are one of countless many who look upon their place in history and fear being forgotten. You could turn back now, take love and heart in a liminal existence. But should you desire to be known and to be sung of, you must first prove yourself worthy of my ink.")
                };
            }
            else
            {
                text = Hero.MainHero.Clan.Tier switch
                {
                    >= 5 => new TextObject("{=OJz46jAS}Do you think me the fool, o hero? I know well your name, and you are known well to the people. That you think you need be inducted to be part of our tales is both insult and strange courtesy; for you are an important player upon our world’s stage. But if you deign to think you need my approval than so be it. I shall see your name illuminated upon fine vellum, and when word of your deeds reaches the ears of others upon the path, I shall see it penned."),
                    >= 3 => new TextObject("{=fEAYu8Ff}I know your tale, there is value in hearing it but perhaps not yet in speaking it. Take no offense, you are worth more to the people than the escapades of a fishwife of Merroc or the rumors of a downtrodden swamp thug upon the Ebor. I shall pen your name within the tales of this age, a bit player for the moment - but such tertiary cast have a way of finding favoritism with those who seek to place themselves in the shoes of one they can imagine.fight."),
                    _ => new TextObject("{=XnASmGdF}Would you now? Bold of you to ask, bolder still to think you have the stones for it. You are an untested soul, your saga perhaps barely begun or as of yet unworthy of an audience. You are far from a Wilund the Bold or an Oca the Conqueror. I will not anoint you upon this path until you have a name worthy of a scrivener’s pen.")
                };
            }

            return text;
        }

        public override TextObject GetClergyInductionLast(int rank)
        {
            TextObject text = null;

            if (rank == 3)
            {
                text = Hero.MainHero.Clan.Tier switch
                {
                    >= 5 => new TextObject("{=1VowoFXC}That you, someday, shall age and wither as I have. Your grave will come to claim you, as it claims us all. I prithee, live your life so that you inspire the best in those who come after you. Let your legacy be one worth following in heart, not pursued by iron and bloodshed."),
                    >= 3 => new TextObject("{=QJdm8dgE}Have you considered that perhaps when our hands and songs can convey only the small glimpses of the liminal upon vellum, that what is forgotten in the faith of the eternal is more beautiful? That it may yet rest, in Heavens or places fouler? That your name, should it rise ever higher, may be cursed or spoken for as long as our texts yet live? What glory you seek in this life may not follow you to the next. But very well, you are anointed upon our pages. May this bring you peace and allow you time to seek comforts more deeply personal."),
                    _ => new TextObject("{=nhEHrqse}I will remember your name for we have shared these words, but I fade with the hours from my autumn into winter; and what comfort you may hold by being known to an Elder Illuminator is a small thing to cherish when compared to being sung in a bard’s song.")
                };
            }

            if (rank == 2)
            {
                text = Hero.MainHero.Clan.Tier switch
                {
                    >= 5 => new TextObject("{=FGnpjbvG}That you think you need the approval of a humble scrivener to be formally inducted into the Canticles is a courtesy unnecessary but greatly appreciated; for now my own name shall play a role beyond merely a rubricator in the grand tales of this age. "),
                    >= 3 => new TextObject("{=puCm6PoZ}Were you a soul of middling ambitions this could be comfort to you; but were you that, you’d not find yourself so bound within the fate of an era. You are part of the Canticles now, go forward in momentum; change the stars to make your fate one all shall know. Go in boldness. "),
                    _ => new TextObject("{=r7Hmedg3}Compete in tournaments, serve a lord well, participate in a war and make yourself the leader of a true clan - not a mere rabble mob. Once you are known to more than just yourself, once I hear tell of you in tales, I shall see you named among the cast of characters for this age.")
                };
            }
            else
            {
                text = Hero.MainHero.Clan.Tier switch
                {
                    >= 5 => new TextObject("{=G6pDBWYM}May you rise or may you fall, always and unto eternity. Savior or villain, royal or thrall; we await the fate that comes at the song’s end."),
                    >= 3 => new TextObject("{=nJqhdtdy}You shall be the imagined persona of many a child picked last in games of stick fight. Make yourself more known, rise your station and change your stars; in time you may yet find yourself worthy of tale."),
                    _ => new TextObject("{=XX7Ob5HY}Find victory within a tournament, kidnap a haughty noble, put a bastard’s head to the chopping block, or become known to the folk of Vlandia for good or for ill. Only then will I see fit to have your name illuminated in the margins of our tale.")
                };
            }

            return text;
        }

        public override TextObject GetClergyPreachingAnswer(int rank)
        {
            var text = rank switch
            {
                3 => new TextObject("{=4qbtiG9m}I do not preach, I merely illuminate. I have penned the Canticles since I first arrived on the continent as a child, a war orphan in the homeland who found comfort only in the cold embrace of a cloister. I was a goliard, a true firebrand once; and a scrivener when time demanded it of me."),
                2 => new TextObject("{=jtinxrFT}I do not preach, stranger. I pen the Canticles, the compounded histories of our myriad people - of our wars, our epics, our tragedies and our sorrows. While young goliards boast in the cities of our kingdom, I am but a scrivener and have resigned myself to a priory where I might best illuminate the tales of the land by my own hand."),
                _ => new TextObject("{=V08yiF68}Preach? Do not think me some imperial dog quibbling over their penitential! I do not preach, I sing of the Canticles. I tell the tales of our ancestors and our contemporaries, of those worthy of song and those who carry the song ever upon their lips.")
            };

            return text;
        }

        public override TextObject GetClergyPreachingAnswerLast(int rank)
        {
            var text = rank switch
            {
                3 => new TextObject("{=eTPHgScM}I fought, I killed, I was a legend in the making - as all men would claim to be in hubris. But my hands ached upon a sword hilt; far more than when I gripped a quill. And thus I illuminate, even still in my sunset days."),
                2 => new TextObject("{=zQKW5qog}It is a humble post, and those who recite the tales by rote are seldom worthy of the annals themselves - but those who pen them are granted the privilege of signing their names, and thus we shall be remembered as those who carried the song. "),
                _ => new TextObject("{=JD8uK0Bq}Preach, bah! A Vlandian does not preach, they embody the epic and embolden themselves to actions - to pursuits grander than those who came before them!")
            };

            return text;
        }

        public override TextObject GetClergyProveFaith(int rank)
        {
            var text = rank switch
            {
                3 => new TextObject("{=Rp3k6Uzy}In my youth I would tell you that to be sung of requires violence and glorious deeds; but wiser now I say you need only be patient. Our faith - if one can call it a faith beyond that which we put in our peers - is one of syncretism and patience. We speak of the Heavens, the Otherworld, the Sight, all manner of tenets absorbed from lands we’ve conquered and fought upon."),
                2 => new TextObject("{=tcOP12FR}Prove your… Perhaps you do not understand the nature of the Canticles? You are a player upon this stage, a character within the tale; all who live are. Whether you are to be tertiary, secondary; villain or protagonist, is a matter of those who pen the page and the nature of your deeds. If you wish to be exalted by history and sung of in the Canticles, you must spread your influence far and wide."),
                _ => new TextObject("{=RWyggtYq}f you wish to be enshrined within the tales and songs of the Vlandian people, you must embody those who came before you and learn from their deeds. You must then prove your mettle and that you are not a mere follower upon the path, but a being worthy of being sung in the Canticles of the nation. Knights and kings, barons and bandits; all will fade with time if they do not give cause for their memory to be known.")
            };

            return text;
        }

        public override TextObject GetClergyProveFaithLast(int rank)
        {
            var text = rank switch
            {
                3 => new TextObject("{=NcpfHNyp}All things we accept as truth, though not of importance when compared to the legacy we leave upon this world. If you wish to prove your faith, go forth with an open mind and a patient heart; if you are to be a legend worthy of the Canticles, fate shall provide you the chance. "),
                2 => new TextObject("{=Ri01Z2Pe}You must be known, for good or for ill; by deeds worthy of remembrance. Vlandia celebrates warfare and the mercenary life for many reasons, but foremost among them for many is that a sellsword is far more memorable than a money lender or a basket weaver. If you wish to prove your place is more worthy of the song, perform bold deeds. "),
                _ => new TextObject("{=bWocbYpR}If you wish to prove your mettle, you must compete with the deeds of old for the good of our people. Conquer, raid, play at court; make yourself known - live a life both bold and notorious! Better you have ten thousand enemies who remember your name than a single cherished friend whose memory of you will die with them.")
            };

            return text;
        }

        public override string GetId()
        {
            return "canticles";
        }

        public override int GetIdealRank(Settlement settlement, bool isCapital)
        {
            if (isCapital)
            {
                return 3;
            }

            if (settlement.IsTown)
            {
                return 2;
            }

            if (settlement.IsVillage)
            {
                return 1;
            }

            return 0;
        }

        public override Divinity GetMainDivinity()
        {
            return mainGod;
        }

        public override TextObject GetMainDivinitiesDescription()
        {
            return new TextObject("{=7rdxBfJi}Great Saga");
        }

        public override int GetMaxClergyRank()
        {
            return 3;
        }

        public override TextObject GetRankTitle(int rank)
        {
            var text = rank switch
            {
                3 => new TextObject("{=RNXdpUrg}Elder Illuminator"),
                2 => new TextObject("{=oj7TBO2Y}Scrivener"),
                _ => new TextObject("{=HUFLF5Pz}Goliard")
            };

            return text;
        }

        public override MBReadOnlyList<Divinity> GetSecondaryDivinities()
        {
            return pantheon.GetReadOnlyList();
        }

        public override TextObject GetSecondaryDivinitiesDescription()
        {
            return new TextObject("{=neVhyybi}Sagas");
        }

        public override (bool, TextObject) GetInductionAllowed(Hero hero, int rank)
        {
            var text = new TextObject("{=aSkNfvzG}Induction is possible.");
            var possible = hero.Clan is {Tier: >= 3};
            if (!possible)
            {
                text = new TextObject("{=qGy2aWPt}Insufficient clan tier (min. 3)");
            }

            return new ValueTuple<bool, TextObject>(possible, text);
        }

        public override TextObject GetBlessingAction()
        {
            return new TextObject("{=zEqsZ7cV}I would like to medidate on one of the sagas.");
        }

        public override TextObject GetBlessingQuestion()
        {
            return new TextObject("{=8hqRwQwo}And which one of sacred sagas, of heroes past and renowned, would you like to medidate upon?");
        }

        public override TextObject GetBlessingConfirmQuestion()
        {
            return new TextObject("{=2rby15CO}Are you sure? If so, you shall take these teachings deep into your heart. Let your soul sing for the anguished fallen, and hope one day you may join them in the Canticles!");
        }

        public override TextObject GetBlessingQuickInformation()
        {
            return new TextObject("{=e8f8HXZc}{HERO} is now medidating on the {DIVINITY} saga.");
        }

        public override TextObject GetBlessingActionName()
        {
            return new TextObject("{=Yz4aFGU9}medidate upon.");
        }
    }
}