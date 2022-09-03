using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Battania
{
    public class AmraFaith : PolytheisticFaith
    {
        public override TextObject GetFaithName()
        {
            return new TextObject("{=fkHLNjpR}Amra Ollamh");
        }

        public override TextObject GetFaithDescription()
        {
            return new TextObject("{=hKssM7h1}The creed of the Amra Ollamh, “that which is wondrous and great” - is the long-standing folkloric tradition of the people who dwell up the Uchalion plateau. Though outsiders are wont to confuse the Amra Ollamh as little more than tales of goblins, sprites, fair folk and woodland monsters - the creed itself is in fact a deeply involved cultural institution meant to instill the youth of Battania with the lessons of their forebears by way of colloquial metaphor. The Brithem, law-speakers and judges, more than often related to the headmen of their village, maintain traditions whilst the women of the Bandrui preach these tales to children with the hopes of driving them to boldness and hopeful ambitions later on in life. Cattle raiding, braggadocious dueling bouts, and no small amount of criminal activity paired with lessons of mercy make up much of what is learnt by those who follow the Amra Ollamh. Those seeking greater insight into the spiritual matters of the Battanians are more likely to be dissuaded by Brithem lawspeakers, so as not to intrude upon the more esoteric matters handled by the practitioners of the Derwyddon arts.");
        }


        public override TextObject GetClergyForbiddenAnswer(int rank)
        {
            TextObject text;
            text = rank == 2 
                ? new TextObject("{=8s8CQnhD}No respect shall ever come to those who bend the knee willingly, and least of all to the foreign upstarts who gnaw upon our holdings. Those who strike upon our wildling sons, wolfskin or otherwise, put our future in danger as a people.") 
                : new TextObject("{=uUovu9Ds}Were you to lick the boot of Vlandian, Sturgian or make yourself a lackey to the Empire, I’d see you personally whipped raw and thrown into the salted waters of the Bay of Varcheg! If you strike down the wildlings and the wolfskins you run the risk of butchering the blooded leaders of Battania’s next generation - so sharpen your teeth against them as you are wont, but take not their heads.");

            return text;
        }

        public override TextObject GetClergyForbiddenAnswerLast(int rank)
        {
            TextObject text = null;
            if (rank == 2)
            {
                text = new TextObject("{=7MTRx2BL}In truth, anything which would show you to be lacking in cleverness and the means to make mischief will show you to be a failure under the creed of the Amrah Ollamh.");
            }
            else
            {
                text = new TextObject("{=0wPCkUUV}Suffer no humiliation and no slander towards our people, towards your Bandrui and Brithem alike. We are all one Battania, and if you see us sundered you best see us forged anew with haste. Otherwise you’d deserve a fate far fouler than what any draoith could conjure up. I’d personally suggest ye get slathered in honey, your limbs snapped, and thrown into a craggy pit to be feasted on by boars.But you wouldn’t be such a craven to tempt that fate, now would ye {NAME}?")
                    .SetTextVariable("NAME", Hero.MainHero.Name);
            }

            return text;
        }

        public override TextObject GetClergyGreeting(int rank)
        {
            TextObject text = null;
            if (rank == 2)
            {
                text = new TextObject("{=O4OANr92}Oh aye? What brings you before this Brithem judge? Lost your favorite cattle, perhaps scrape your knee on your way up the Uchalion? If you come seeking satisfaction, you’re better off finding it in a Vlandian brothel or in some imperial parlor succoring down milk-of-the-poppy. My judgment is not meant to bring you anything you couldn’t take for yourself, and my law-speaking is for the Battanian people not whatever matter of wild hound you were born to be.");
            }
            else
            {
                text = new TextObject("{=DDa363gh}Am I to have words thee, {NAME}? And if so, are they to be kind ones? Do you come to me in the hopes that I might slather a bride in mud so her wiles are not driven to hysteria by the invisible probings of the fair folk? Mayhaps you heard tales of the Brandui like myself and how we will put a gimlet eye on rivals for a price - oh aye, make their loins shrivel up like a gooseberry and produce only half-wit sons? You best speak your part quick, I’ve no time for the fool courting of foreign folk.You best watch your words too, there’s nary a man in this land who wouldn’t stab you in the gizzard to win my approval.")
                    .SetTextVariable("NAME", Hero.MainHero.Name);
            }

            return text;
        }

        public override (bool, TextObject) GetInductionAllowed(Hero hero, int rank)
        {
            var possible = true;
            var text = new TextObject("{=GAuAoQDG}You will be converted");
            if (hero.GetSkillValue(DefaultSkills.Roguery) < 50)
            {
                possible = false;
                text = new TextObject("{=qyNdYeda}Not enough roquery");
            }

            if (hero.Culture != Utils.Helpers.GetCulture("battania"))
            {
                possible = false;
                text = new TextObject("{=9Sh0wCfR}Unaccepted culture");
            }

            return new ValueTuple<bool, TextObject>(possible, text);
        }

        public override TextObject GetClergyGreetingInducted(int rank)
        {
            TextObject text = null;
            if (rank == 2)
            {
                text = new TextObject("{=MaOtbHO6}Croseo, kinsfolk. How does this season upon the Uchalion greet ye? Have you drunk of the Llyn Tywal lately? Bloodied the noses of Vlandian dogsbodies and Sturgian wastrels? You stand here upon our blessed soil and have fool’s pride enough to come upon your Brithem brethren and act his equal - so I must assume you make your ancestors proud or you’ve stones enough to stand in my eyeline and bluff!");
            }
            else
            {
                text = new TextObject("{=Pr6kjUr7}Croseo, you child of the Uchalion. Have you looked upon the Morcomb in recent days? Have you glutted yourself on the fish of the River Fiur? Bashed the teeth in of any southern fool who’d claim themselves emperor of all they survey? You stand before me on blessed soil and you speak to the Bandrui as though you were a Brithem or mormaer lordling - so I assume you’ve cause to bid for my counsel. Lest ye be a fool, though to be a fool can be a blessed thing.Fool’s have the chance to leave their foolishness behind, after all.");
            }

            return text;
        }

        public override TextObject GetClergyInduction(int rank)
        {
            TextObject text = null;
            var induction = GetInductionAllowed(Hero.MainHero, rank);
            if (rank == 2)
            {
                if (!induction.Item1)
                {
                    text = new TextObject("{=1HVYdfei}Bah! Would you like to know what the difference between you and a rock might be? A rock might cut a silhouette making me think it a Battanian hiding in shadows. You’ve not the guile, the gut, or the making of a merry mischief maker. You may as well come to me as a Temean senator with a formal inquest pleading: “Oh honored Brithem, take me ‘pon the comhdhail and see me cherished by all the true kindred of the Uchalion.”");
                }
                else
                {
                    if (Hero.MainHero.GetSkillValue(DefaultSkills.Roguery) > 100)
                    {
                        text = new TextObject("{=kA3oWrwD}Da Iawn, {NAME}. Rare is it that I look upon one such as yourself and feel pride, and ne’er do I look upon one such as yourself and feel humility! But tis the wisest of men who speaks least and I’ve made myself the fool in your presence; good humor that. Truly your heart beats for Battania, a soul ever the envy of any mormaer or fian gone gallowglass or made to fight upon the highland fields. Blessed this is; a kindness this is.")
                            .SetTextVariable("NAME", Hero.MainHero.Name);
                    }
                    else
                    {
                        text = new TextObject("{=fcyOCUph}Now you have the look of someone I wouldn’t spit upon. There’s a glint in the eye, something at the corner of your lips - you’ve got the capacity to act as a true Battanian should. Shameful that your blood might have been muddled with the conquest of many a foe; but a Battanian heart beats true in defiance of such origins.");
                    }
                }
            }
            else
            {
                if (!induction.Item1)
                {
                    text = new TextObject("{=jQPri20q}You’re a lout, a fool and were it not for the laws of hospitality which I choose to honor I’d bash your teeth in with a rock. You want to be inducted as a follower of the Amra Ollamh, best find yourself a Brithem who can tell you to sod off as I’ve only words to share so disparaging they’d rouse your grandmother from her grave and see her slap sense into that thick head of yours. Bah!");
                }
                else
                {
                    if (Hero.MainHero.GetSkillValue(DefaultSkills.Roguery) > 100)
                    {
                        text = new TextObject("{=mj8psGyr}Da Iawn, {NAME}. Mind me and grant me the courtesy of a moment in stunned silence, for not since the ancient rebel queens of the hill clans which warred with the Laconians and Dryatics before the damnable Calradoi came ashore; have I had the knowledge to know of one whose heart beats so true for Battania. Envious I am, for those who cannot do must surely teach and I know all too well that I shall carry your name ‘pon my lips for many a year to come.You’ve the guile of a rogue and the cadence of one who is worthy of legend.May the mormaer blush at your mention and the fian wish they were half the man you’re being denies by being.")
                            .SetTextVariable("NAME", Hero.MainHero.Name);
                    }
                    else
                    {
                        text = new TextObject("{=6qJaPkki}Wise of ye to come to me rather than a Brithem - they’d feed you pretty words and think to marry you off to some mruigfher with more denars than chest hairs who’d ne’er considered a freeborn maid such as yourself could be more than a bride. Clever girl. I pray that your cunning lasts, for this is a cruel age filled with many a petulant dog who’d deny you the right of gutting him like a trout just by malus of your breasts.");
                    }
                }
            }

            return text;
        }

        public override TextObject GetClergyInductionLast(int rank)
        {
            TextObject text = null;

            var induction = GetInductionAllowed(Hero.MainHero, rank);
            if (rank == 2)
            {
                if (!induction.Item1)
                {
                    text = new TextObject("{=52FGfd5h}Get out of my sight until you’ve got stones enough to be worthy of it. I’d excuse this if you were a child or at least born of the highlands; but you’re just wasting my time with that slackened look about your face!");
                }
                else
                {
                    if (Hero.MainHero.GetSkillValue(DefaultSkills.Roguery) > 100)
                    {
                        text = new TextObject("{=cxzNRyAs}I’ll honor your desire to be formally recognized by a britherm, and when next the comhdhail is gathered I shall speak your praises as one bound to the creed of the Amra Ollamh. You shall be as the creed is known, of the greatest and most wonderous - a teacher who leads by example. Aros yn wir, aros yn ffyddlon;[charactername].Stay true, stay faithful; look homeward and ensure that Battania is ne’er forgotten -be it by scars or song.In living glory or storied tale.")
                            .SetTextVariable("NAME", Hero.MainHero.Name);
                    }
                    else
                    {
                        text = new TextObject("{=KNCxsHWZ}I’ll honor your desire. Swear upon your integrity, upon your mettle and might, that you will shepherd the next generation of Battanians - that you will carry on our traditions, lead by example, take what you desire and give back in kind. There are no oaths I can bind you to, no words that shall suffice - keep the swearing within your heart and may it strangle you bitterly should you break it. A fo ben, bid bont, {NAME}.Be a bridge for our people. Never forget to look homeward.")
                            .SetTextVariable("NAME", Hero.MainHero.Name);
                    }
                }
            }
            else
            {
                if (!induction.Item1)
                {
                    text = new TextObject("{=pHRXLPT8}Get out my sight, you’re no true Battanian and even if you thought yourself one tis the work of a Brithem to judge a man of their mettle - for the Bandrui see true all your flaws and I’m not liable to sully the oaths of our folk for such a tarnished soul as ye. I’ve seen pig iron blades of greater worth than your bleary being. Get ye gone before I rouse the village and see you chased by hounds to the edge of the Uchalion!");
                }
                else
                {
                    if (Hero.MainHero.GetSkillValue(DefaultSkills.Roguery) > 100)
                    {
                        text = new TextObject("{=qqwqxtLH}So aye, I’ll honor you this induction. When next the comhdhail is called I’ll shame every britherm from Bog Beth to Claig Ban with tales of your deed and how easily you came to embody the creed of the Amra Ollamh. Aros yn wir, aros yn ffyddlon; {NAME}.Ye magnificent one. Stay true, stay faithful, and may you become storied among our folk until none dare seek to take the highlands from her trueborn children.")
                            .SetTextVariable("NAME", Hero.MainHero.Name);
                    }
                    else
                    {
                        text = new TextObject("{=obM5RkaH}So aye, I’ll honor your desire. You’ve the quality of one who knows their mettle and who deals in sword arms and battered egos with ease. Such is the rite of any trueborn Battanian. May you lead by example, may you inspire many more to prove their mischief and make mercy theirs to distribute. I’ve no oaths to bind you, and no words nor hollow praise shall suffice - so I bid you swear within your heart that you shall honor the Amra Ollamh and that should you betray it, may it trammel you beneath a crushing stone. A fo ben, bid bont, {NAME}.Look homeward, but do not be bound to its petty whims..You’ve the means for greatness.Seize them.")
                            .SetTextVariable("NAME", Hero.MainHero.Name);
                    }
                }
            }

            return text;
        }

        public override TextObject GetClergyPreachingAnswer(int rank)
        {
            TextObject text = null;
            if (rank == 2)
            {
                text = new TextObject("{=paQBiy0x}My judgment and discernment is a matter of the Amra Ollamh, for I am a teacher who speaks of the great cycles of our people - from the giant’s seat of the lower Rhennod to the riverlands of the Ircara! ‘Tis true, many a land has been lost to fool imperials and bastard-born Nordlanders playing at Sturgians; but a Battanian heart beats ever true. I shepherd our tales, our traditions; ensure the young know how they’re meant to act and that the old don’t forget their place.");
            }
            else
            {
                text = new TextObject("{=grcYNhWO}I carry the soul of our people with my words and I weave our spirit into our actions; for I am a Bandrui and this is my purpose. Be they a child born upon the Ailta Druin or left to a cruel winter’s chill on the Rock of Glanys, it is my voice that guides the next generation of our people to glory! ‘Tis my voice more than any other that tells the tale of our finest hunters, of clever lords in wolfskin mien, or highland rebels cutting the gristle from the gullets of imperial soft bodies. ‘Tis sad, the state of the Uchalion and her people.In elder days my circle would dwell in sacred groves and offer counsel to heroes and high kings - now we languish in walls of stone, beset by foreign hedge knights, Sturgian raiders, and whatever mess the Empire sees fit to view itself as.");
            }

            return text;
        }

        public override TextObject GetClergyPreachingAnswerLast(int rank)
        {
            TextObject text = null;
            if (rank == 2)
            {
                text = new TextObject("{=d0gkCU5b}You might hear tell that our lot are charlatans bickering on about river nymphs and sea dragons - but allegory and metaphor aren’t words belonging only to toga-shriven scribes. And there’s more to this world than you or I have the Sight to simply dismiss after all. But to answer your question, and to say it right, I’ll say just this: I am that which judges a true Battanian of their worth.To see that they make mischief and might alike enough to please their grandfather’s grandfathers in the heavenly hereafter.Ne’er are there more an Uchalion born Battanian than those who follow the creed of the Amra Ollamh.");
            }
            else
            {
                text = new TextObject("{=C4JR5bHD}A cruel humiliation, to be one who touches upon the mysterious and grants counsel now reduced to the stuff of gossiping washerwomen and charlatan knaves in search of river nymphs to bed and lindworms to slay. Allegory and metaphor lost to those who view our lands as savage and backward as their own prejudices. But I’ll speak true and quick then, for you’ve not come for my screed. I am that which guides a Battanian to know their truth.To see their heart beats in rhythm with the cycles of creation, that they know might and mischief enough to please the unseen and to court the joy of their ancestors long displaced.Ne’er are there more an Uchalion born Battanian than those who are guided by the Amra Ollamh.");
            }

            return text;
        }

        public override TextObject GetClergyProveFaith(int rank)
        {
            TextObject text = null;
            if (rank == 2)
            {
                text = new TextObject("{=sJsv8Ra2}‘Tis both no simple thing and a thing that couldn’t be simpler; not that I wish to speak in riddles. Save that nonsense for the draoithe. If you want to prove your mettle, you must simply be a Battanian. Of course, to be Battanian is a tarnished thing in our current state - too many folk wrongly think they know what it means. If you wish to prove yourself wise in the Amra Ollamh, you must make yourself known for your cunning and your guile.");
            }
            else
            {
                text = new TextObject("{=08JGRx5G}Supposing you’re seeking a fate higher than a bóaire ‘pon the highlands, there are ways and means you could show yourself a true Battanian. First matter of that, of course, is to be a Battanian - be ye tarnished or true by the many foes which nip upon our borderlands with the ravening greed of thieving children. Assuming you can manage to be born right of our blood, ‘tis a simple task to follow. Gather yourself a throng of right - minded Battanians and make yours a name to be sung and feared.");
            }


            return text;
        }

        public override TextObject GetClergyProveFaithLast(int rank)
        {
            TextObject text = null;
            if (rank == 2)
            {
                text = new TextObject("{=F8baDzFV}Make a name for yourself which is held in the fear of landed lords and in the whimsical romance of young dreamers. Raid cattle from those without the mettle to keep them safe, and make lords both Battanian and foreign fear your blade but know your charity. You’ll prove your faith by shepherding tomorrow for the Battanian people; with cattle, with iron, and were you of a more legendary portent - by stealing back what others have stolen from us!");
            }
            else
            {
                text = new TextObject("{=fz9opj1F}Don yourself the wolfskins if you’ve the mind for tradition; take that which can be yours by right of might, honor no landed claim - Battanians and beasts alike may graze where they deign to. Bring iron to the forges and the folk of Battania so youngbloods might dream of blades to someday hold in more than their errant wishes! And were you truly to be guided by the Amra Ollamh, you’d cast the Vlandian, Sturgian, and imperial dogs from our land.Preferably off the side of a cliff, but none shall be picky so long as they’re made to flee in fear of your wrath.");
            }

            return text;
        }

        public override string GetId()
        {
            return "amra";
        }

        public override int GetIdealRank(Settlement settlement, bool isCapital)
        {
            if (settlement.IsVillage)
            {
                if (MBRandom.RandomInt(1, 100) < 50)
                {
                    return 2;
                }

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
            return new TextObject("{=iYqD0kQM}Great Spirits");
        }

        public override int GetMaxClergyRank()
        {
            return 2;
        }

        public override TextObject GetRankTitle(int rank)
        {
            TextObject text = null;
            if (rank == 2)
            {
                text = new TextObject("{=apvDLyKQ}Brithem");
            }
            else
            {
                text = new TextObject("{=9CL79r92}Bandrui");
            }

            return text;
        }

        public override MBReadOnlyList<Divinity> GetSecondaryDivinities()
        {
            return pantheon.GetReadOnlyList();
        }

        public override TextObject GetSecondaryDivinitiesDescription()
        {
            return new TextObject("{=CXQJSwOR}Spirits");
        }

        public override TextObject GetBlessingAction()
        {
            return new TextObject("{=GDh2G2Qe}I would like to swear an oath to the spirits.");
        }

        public override TextObject GetBlessingQuestion()
        {
            return new TextObject("{=tPAZEBKs}And what spirits would ye be bound to? Those of wolves and warriors whose savagery are unmatched, or the spirits of Uchalion, who breathe fire into our hearts to fight the enemy?");
        }

        public override TextObject GetBlessingConfirmQuestion()
        {
            return new TextObject("{=ptpmY4F2}Be ye sure of that. Only a fool would break an oath with the spirits.");
        }

        public override TextObject GetBlessingQuickInformation()
        {
            return new TextObject("{=YyAcVG4o}{HERO} has sworn an oath to the {DIVINITY} spirits.");
        }

        public override TextObject GetBlessingActionName()
        {
            return new TextObject("{=bd7HQSJH}pledge an oath to.");
        }
    }
}