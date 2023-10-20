using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Empire
{
    public class DarusosianFaith : MonotheisticFaith
    {
        public override Settlement FaithSeat => Settlement.All.First(x => x.StringId == "town_ES4");
        public override TextObject GetDescriptionHint()
        {
            return new TextObject("{=f1Jd0nwL}Though the Calradic Empire had long possessed an imperial cult to venerate Emperors who were deemed to be “deliverers” and “saviors” of the civilized world, during the latter years prior to the schism and inerrenegrum the practice of viewing all emperors as god-emperors had become vulgorously commonplace. It was during this period that doctrines and reforms attributed to the teenage Emperor Darusos, a figure infamously denied the rite of the divus by the generals who usurped him, were uncovered.");
        }

        public override Banner GetBanner() => new Banner("11.35.19.1836.1836.768.788.1.0.-30.521.142.116.393.393.764.764.1.0.0.521.121.116.312.312.764.764.1.0.17.503.22.116.179.179.764.764.1.0.0.443.121.116.175.175.994.764.1.0.0.443.121.116.175.175.534.764.1.1.0.161.121.116.119.119.764.764.1.1.0");

        public override bool IsCultureNaturalFaith(CultureObject culture)
        {
            if (culture.StringId == "empire")
            {
                return true;
            }

            return false;
        }
        public override bool IsHeroNaturalFaith(Hero hero)
        {
            if (IsCultureNaturalFaith(hero.Culture) && hero.MapFaction != null && hero.MapFaction.IsKingdomFaction && 
                hero.MapFaction.StringId == "empire_s")
            {
                if (hero.IsLord) return true;
                else if (MBRandom.RandomFloat < 0.9f) return true;
            }

            return false;
        }

        public override TextObject GetFaithName()
        {
            return new TextObject("{=wrOvqkyB}Darusosian Martyrdom");
        }

        public override TextObject GetFaithDescription()
        {
            return new TextObject("{=NhdiHm3M}Though the Calradic Empire had long possessed an imperial cult to venerate Emperors who were deemed to be “deliverers” and “saviors” of the civilized world, during the latter years prior to the schism and inerrenegrum the practice of viewing all emperors as god-emperors had become vulgorously commonplace. It was during this period that doctrines and reforms attributed to the teenage Emperor Darusos, a figure infamously denied the rite of the divus by the generals who usurped him, were uncovered. Viewed as a divine Martyr, rebels and lesser cults throughout the Empire began to preach his teachings and claim that the divinity of the imperial line was inherent; that the rite of the divus was a formality at best or a means of vaunting false emperors to positions of divine power. Now worshiped throughout the Southern Empire and in hidden cells of far flung holdings, the Darusosian Martyrdom preach the divine mandate that enshrines the rulership of the line of Arenicos. Lay flamines, their purer superiors of the Flamines Castus, and the figurehead of the Rex Sacrarum of Lycaron, collect alms and absolve the sins of petty mortal ambition to those who seek to walk the path of the Martyr.");
        }

        public override TextObject GetClergyForbiddenAnswer(int rank)
        {
            var text = rank switch
            {
                3 => new TextObject("{=7qeT1yJm}I’ve no qualms over many a mortal failing, but if you are to bind thine self to the throne of the Martyr and seek to untwist your soul of sin - you must serve none save for another Darusosian Martyr. Be it Rhagaea Pethros, the Divine Heir Ira Pethros; or any other who may yet emerge immaculate - you must serve only the ends of our Empire. Seek not to burden thyself with a hoard of filthy lucre, for denars shall twist thine soul to marred, mangled ends."),
                2 => new TextObject("{=VW8kECbX}Be not one who acts unbenignantly, for we servants within the Darusosian Martyrdom need be graceful in our deeds. Do not sully your house with cohorts or spouses who were born of upstart states or pagan cultures who deny the will of the Martyr."),
                _ => new TextObject("{=kh0srVMy}Only a blind man would look upon the state of the Empire and think it saved. If you wish to remain in the good graces of the Darusosian Martyrdom, you should not seek to usurp the whims of the imperial throne. Should you be blessed to serve the house of Pethros, allow them to grant you territories - never make demands of them. If you fail to provide donations and services to our clergy, this too shall be noticed and your reputation shall fall.")
            };

            return text;
        }

        public override TextObject GetClergyForbiddenAnswerLast(int rank)
        {
            var text = rank switch
            {
                3 => new TextObject("{=99QxoVg0}Give generously to the Martyrdom, for to do otherwise is to see patrician and plebeian alike suffer unjustly. Thou knowest also that many a foul upstart seeks to further denigrate our Empire. They must be rent asunder, their holdings broken, their deviant crowns shattered beneath our bootheels.Knoweth your purpose in our faith shall ever be as a cudgel and a bulwark.Lest you stumble across a mystery that transfixes thine mind, thou shall know peace only at our duty’s end."),
                2 => new TextObject("{=kvKjx0Q8}Do not allow our enemies to remain comfortable in their belligerent stagnancy. Be as a scourge to the Embers of the Flame, but take not the heads of their heresiarch masters - they must learn penance upon the path to re-entering the Martyr’s good graces."),
                _ => new TextObject("{=hmZTmO6L}If the Empire goes to war against usurpers or savages, you must join and do your part. If you fail to act in service to the Calradic ideals of the Empire, you shall be acting in heresy against the will of Darusos, Arenicos, and all others who have been deified by their deeds on the throne.")
            };

            return text;
        }

        public override TextObject GetClergyGreeting(int rank)
        {
            var text = rank switch
            {
                3 => new TextObject("{=GF8kvMfn}Oh, thou’rt of a kind to think they may posture before the Rex Sacrarum? Set thine eyes upon my personage with a coy and feigned humility? For what foul purpose doth thou think this shall profit them? To come before me so discourteously, as if I were some mere member of the lay flamines meant to play scenes of betrayal like some fool goliard of the upstart borderland principalities? Glory be to Darusos, for he was not spared or given mercy from unclean company which sought beyond their station and it seems now that I am to be tested in the same unkind way."),
                2 => new TextObject("{=eqT2hW8A}May you find the mercy not granted to the Martyr, {NAME}, for I have none to spare. You have the bearings and reputation of those who profit from the suffering of the Empire; a carrion beast treating our glorious nation as though it were a bloated carcass. The Martyr may yet save the soul of turncoats, rogues, blackguards and insurgents - but do not think me one so willing to stain my hands at your expense.")
                    .SetTextVariable("NAME", Hero.MainHero.Name),
                _ => new TextObject("{=0JAT22ju}You have the look of one who has… defied the imperial acts of Calraditas. You do not carry yourself in a Calradic manner, nor way - I do not suppose you hold the Martyr, Darusos, in your heart? I apologize for my pejorative tone, I would offer you succor and hospitality for sake of commensality but I fear my words would offend you. Or rather, they would drive you to shameful sorrow or to mock a Martyr - dead by savage hands.I would not do you such an unkindness.Pray, forgive me. I shan’t go on unless you desire it.You may help yourself to wine and bread if you would like; never let it be said the lay flamen of[settlementname] has forgotten the virtues.")
            };

            return text;
        }

        public override TextObject GetClergyGreetingInducted(int rank)
        {
            var text = rank switch
            {
                3 => new TextObject("{=R1LE52Mw}Too often am I beset by the impudent rabble, souls defiled and piteous in their plight. To know your company, it seems a wise kindness. Thou’rt in search of clarity, of purpose; - one provided not by honeyed tongue nor second face - but one which would make thee dear to the Martyr and all we have sought to save. Speak thy part, {NAME}. Indulge thyself in terms most honorable.I shall receive thee, should our purposes coalesce.")
                    .SetTextVariable("NAME", Hero.MainHero.Name),
                2 => new TextObject("{=eqT2hW8A}May you find the mercy not granted to the Martyr, {NAME}, for I have none to spare. You have the bearings and reputation of those who profit from the suffering of the Empire; a carrion beast treating our glorious nation as though it were a bloated carcass. The Martyr may yet save the soul of turncoats, rogues, blackguards and insurgents - but do not think me one so willing to stain my hands at your expense.")
                    .SetTextVariable("NAME", Hero.MainHero.Name),
                _ => new TextObject("{=CFrhW6EZ}Ave, citizen - and welcome. May I offer you wine and bread in commensality? I wish you hospitality and grace whilst under the auspices of the Empire. I am but the humble lay flamen of [settlementname], devoted to the service of those who anoint the Martyr Darusos and who accept the auctoritas divus of the imperial line. If you are friend, I bid you allow me offer you succor and comfort; if you are yet to accept the Martyr into your heart, pray allow me to elucidate.You shall know no regret save for time spent in ignorance of a beatific truth.")
            };

            return text;
        }

        public override TextObject GetClergyInduction(int rank)
        {
            TextObject text = rank switch
            {
                3 or 2 => new TextObject("{=0rBEhKBT}{PLAYER_NAME} was it? Yes, yes… You serve our Empire and thus already walk upon a righteous path; but full glad am I to know you seek to open your heart to the sacraments of the Martyr. It is a kindness that you do not seek to rest idle upon the laurels of your current service - some pay only lip service when devotion is required. It warms my soul to see you are not of such company.")
                .SetTextVariable("PLAYER_NAME", Hero.MainHero.Name),
                _ => new TextObject("{=rO6aFDKn}It is the heart of the epithetical pietas that one who serves our Empire seeks to embrace it at its fullest. You do a great and virtuous kindness, let none question the sacred acts of your honor from this day forward. Know that you move forward in pursuit of the Martyr’s will, in a position where others will yearn for your praise, where your enemies will fail to stand fast against you and your peers.")
            };

            return text;
        }

        public override TextObject GetClergyInductionLast(int rank)
        {
            TextObject text = rank switch
            {
                3 or 2 => new TextObject("{=6kDbxhEj}Go forth in the Martyr’s glory, and with his blessing. Embody the ideals of Empire, scourge the wicked and the unclean; lead those with darkened hearts into the light. "),
                _ => new TextObject("{=mQsZcBj5}As you pursue the path of the Martyr and the divinity which has graced us, show kindness where it is deserved and trust only those who eagerly seek to repay it.")
            };

            return text;
        }

        public override TextObject GetClergyPreachingAnswer(int rank)
        {
            var text = rank switch
            {
                3 => new TextObject("{=QhEHgXaL}Mine is the voice that speaks for the Martyr’s will, alas only in mournful penitentials. I wear this mantle by my own volition, for overlong have there been aspiring tyrants who sought to further usurp his mandates by way of vile machinations. Seldom am I to preach, for mine is the role to contemplate the mysteries of creation as glanced by Darusos as he was made eternal through the rites of divus.Fret not, for though I hold all the myriad gods of our Empire - both those liberated from savage cultures and those brought with us from our ancestral birthplace - I am that which holds the Martyr above all others."),
                2 => new TextObject("{=oZddNoSP}I preach the doctrines of Darusos, our Emperor betrayed in his youth by the vile treachery of his generals. A child, wise beyond his years and graced with the beatific sight of Heaven; he sought to see the Empire reign as a place of order where patrician and plebeian alike need not suffer fear. He would see us fed, watered, and granted the joys that we are due - and if not for the craven workings of sellswords and tyrant upstarts, he would have reigned one thousand years."),
                _ => new TextObject("{=fPT4Barn}Lo, for I preach the tragedy of the Martyr Darusos, an Emperor betrayed in his youth by the cruel ambitions of his generals. He was targeted, for even the most oathbound man can be made a craven sellsword when they gaze upon the throne of an Empire. Darusos was a goodly sort, he wept for the Palaics and the Vakken, he wrote an elegy for the Perassic League in their fading days. He saw the Empire as united in soul and in spirit, undone from within only by the actions of wicked men. It was for this that Darusos was crucified upon a sacred fig tree and set ablaze - all his aspirations but kindling… Take heart, for this is the sermon of a Martyr - not merely one of a man.")
            };

            return text;
        }

        public override TextObject GetClergyPreachingAnswerLast(int rank)
        {
            var text = rank switch
            {
                3 => new TextObject("{=dh0cpao4}‘Tis my desire to spread the denied transpositions of the Martyr to the licentious masses of our age. To absolve our Empire the foul stains of hubristic usurpers, and to see seized with thine own hands a fate where the line betwixt Darusos to Arenicos, and beyond to Rhagaea Pethros and Ira Pethros may undergo the rite of divus before tragedy may befall them. I preach that the god - emperors of old who claimed deified rank, clutched at such power only through the long gaze of history.By candlelight mine eyes have gleaned that such heights are not unattainable, that within the mysteries and doctrine of Darusos, sits the illustrious path to Heaven that man may tread without departing the mortal coil."),
                2 => new TextObject("{=EkCUxcN4}But no. Betrayed by his generals, spat upon by the Senate, and mocked by his honor guard - Darusos was crucified to a sacred fig tree and burnt to cinders. His great works and scribed doctrines set ablaze with him. As one of the Flamines Castus, I seek to purify those who have walked a path of sin and shame.For the Martyr knows that the failings of those who betrayed them were born from human frailty -they feared change, and in their fear enacted change as fearsome as any other.None of us are immaculate, but there are some who may become pure and unsullied such as I.Pray then, that you may become like me."),
                _ => new TextObject("{=b9bfZtiZ}The imperial line was enshrined upon the throne by a hand most divine during its origins, and thus though Darusos was murdered by militants - his words could not be silenced. Though upstarts like Lucon Osticos and Garios Comnos would seek to usurp the mandate of heaven, consider that the late Emperor Arenicos Pethros was a man-made-divine who was Martyred in a similar fashion. It is only through Empress Rhagaea Pethros and the heir Ira Pethros - that this holy line continues. History should not repeat itself further.No more heroes of our Empire need die for the vanity of lesser men or savage nationals.Darusos awaits us in heaven, and paradise is awarded to those who seek to better the lives of his citizens, and enact the reforms he was denied in life.")
            };

            return text;
        }

        public override TextObject GetClergyProveFaith(int rank)
        {
            var text = rank switch
            {
                3 => new TextObject("{=xcCHnLYz}Ahh, ‘tis wisdom and arrogance that thou shalt seek the Rex Sacrarum for elucidation. What is thine intent, to seek one wise beyond the ken of an age and bid they preach the rote doctrine of the lay flamines? Thoust have no need to be coy, nor ignorant, nor false. I shall speak and do so with courtesy - for this is what the Martyr would have of me. Perilous is the hour and heretics run rife through our Empire.Scourge those who cling to the Augeo Sophica or the Parens Patriae, shame their leaders; see them seized by brutish hands befitting brutish miens such as theirs.Let not their disarray see our fiefs fall to the avaricious hearts of lesser, wayward men."),
                2 => new TextObject("{=1co9h8gN}Purity comes at a cost. Though I would not ask you to make a pauper of yourself, know that denars have led far too many upon an unrighteous path. Donate your wealth to the Martyrdom, free yourself from the burdens of silver and gold. Give food to the needy, see our soldiers fed so we may endure the long siege we suffer upon our ideals. Make yourself known and friend to others within the ranks of the Flamines Castus, gain an audience and supplicate yourself before the Rex Sacrarum. "),
                _ => new TextObject("{=wiZnk7bD}Alas we are beset by foes who desire what they think is best for the soul of our Empire. Converting the upstarts of the Augeo Sophica cult or the Parens Patriae movement. Members of the Northern and Western breakaway territories need to be put to the sword, for they have sought to sunder the imperial line and represent daggers waiting in shadows so long as they yet live…")
            };

            return text;
        }

        public override TextObject GetClergyProveFaithLast(int rank)
        {
            var text = rank switch
            {
                3 => new TextObject("{=BZcB9LOv}I would suffice to bid you take arms against the Embers of the Flame, they preach a false doctrine and grant hospitality to those who would see us destroyed. Their workings dishonor the Martyr - and should you shackle their heresiarch, I would see them brought before me. So they might know their lowly place… Mine are matters of far higher importance, thou shalt not impress me.Court the patricians and the flamines; sully not mine hours with vainglorious pursuits."),
                2 => new TextObject("{=8aqNYgR4}Bring high ranking officers and soldiers from the upstart states and once tamed cultures to heel; see them gifted to our cause so they might learn their place once more. And most importantly, I beg you, ensure that Lycaron does not fall into enemy hands - for should it be destroyed we may see all records of Darusos lost."),
                _ => new TextObject("{=NNdUyOWc}But pardon my words, for you might think me a brute calling you to hear the clarion call of a crusade. Nay, this is not my intent. In truth you could do much by providing food to the garrisons of our imperial holdings, disposing of looters who prey upon the downtrodden, and diminishing the number of disreputable blackguards from the Embers of the Flame - who dare speak the Martyr’s name in vanity.")
            };

            return text;
        }

        public override string GetId()
        {
            return "darusosian";
        }

        public override int GetIdealRank(Settlement settlement)
        {
            if (settlement.IsTown)
            {
                return 2;
            }

            if (settlement.IsCastle || settlement.IsVillage)
            {
                return 1;
            }

            return 0;
        }

        public override int GetMaxClergyRank()
        {
            return 3;
        }

        public override TextObject GetRankTitle(int rank)
        {
            var text = rank switch
            {
                3 => new TextObject("{=cOu0U6zz}Rex Sacrarum"),
                2 => new TextObject("{=A8FOsW0q}Flamines Castus"),
                _ => new TextObject("{=P3BGTUzQ}Flamines")
            };

            return text;
        }

        public override TextObject GetCultsDescription()
        {
            return new TextObject("{=3mvrd7pr}Cults");
        }

        public override (bool, TextObject) GetInductionAllowed(Hero hero, int rank)
        {
            var text = new TextObject("{=aSkNfvzG}Induction is possible.");
            var kingdom = hero.Clan.Kingdom;
            if (kingdom == null || kingdom.StringId != "empire_s")
            {
                text = new TextObject("{=cVchNb9c}Not a member of the Southern Empire.");
                return new(false, text);
            }

            return new(true, text);
        }

        public override TextObject GetBlessingAction()
        {
            return new TextObject("{=CAjwbXaC}I would like to medidate upon the Martyr.");
        }

        public override TextObject GetBlessingQuestion()
        {
            return new TextObject("{=m0MZaCM0}How so? Would you like to medidate upon the Cult and our line of hallowed emperors? Or would you pray for guidance from the Triad?");
        }

        public override TextObject GetBlessingConfirmQuestion()
        {
            return new TextObject("{=XxE5MQKJ}Assert your will for guidance. The study of the Martyr and the gods is not to be taken lightly.");
        }

        public override TextObject GetBlessingQuickInformation()
        {
            return new TextObject("{=hDY5SEOL}{HERO} is medidating upon {DIVINITY}.");
        }

        public override TextObject GetBlessingActionName()
        {
            return new TextObject("{=Yz4aFGU9}medidate upon.");
        }

        public override TextObject GetInductionExplanationText() => new TextObject("{=NNMB0hrF}Only members of the Southern Empire are accepted.");
    }
}