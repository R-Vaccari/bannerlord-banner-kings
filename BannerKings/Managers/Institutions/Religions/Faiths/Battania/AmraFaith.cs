using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Asera
{
    public class AmraFaith : PolytheisticFaith
    {
        public AmraFaith() : base()
        {

        }
        public override TextObject GetFaithName() => new TextObject("{=!}Amra Ollamh");
        public override TextObject GetFaithDescription() => new TextObject("{=!}The creed of the Amra Ollamh, “that which is wondrous and great” - is the long-standing folkloric tradition of the people who dwell up the Uchalion plateau. Though outsiders are wont to confuse the Amra Ollamh as little more than tales of goblins, sprites, fair folk and woodland monsters - the creed itself is in fact a deeply involved cultural institution meant to instill the youth of Battania with the lessons of their forebears by way of colloquial metaphor. The Brithem, law-speakers and judges, more than often related to the headmen of their village, maintain traditions whilst the women of the Bandrui preach these tales to children with the hopes of driving them to boldness and hopeful ambitions later on in life. Cattle raiding, braggadocious dueling bouts, and no small amount of criminal activity paired with lessons of mercy make up much of what is learnt by those who follow the Amra Ollamh. Those seeking greater insight into the spiritual matters of the Battanians are more likely to be dissuaded by Brithem lawspeakers, so as not to intrude upon the more esoteric matters handled by the practitioners of the Derwyddon arts.");


        public override TextObject GetClergyForbiddenAnswer(int rank)
        {
            TextObject text = null;
            if (rank == 4)
                text = new TextObject("{=!}Do not be as a glutton or a beast; neither hoarding nor bloodlust should color your life. If you take justice into your hands you may find yourself forever grasping for more - allow your betters to guide you as an elder sibling should; do not kill prisoners who have wronged you without their say. But do not reveal yourself to be lax or limp of arm either; for justice will often be your task.");
            else if (rank == 3)
                text = new TextObject("{=!}You must never betray the blood of your blood, to do so is to spit upon the Code. If you are called to serve the Aserai, you must shed blood for their cause - at least once, so as to show that you honor your siblings even should you disagree with their choices. You must ensure any who serve you do not suffer the ravages of conflict without justice on your lips and a safe bastion for them to find reprieve in.");
            else if (rank == 2)
                text = new TextObject("{=!}…A curious question; though I suppose I would know the deeper answers to this. Few are the faqir or even imam who are as studied in the various things so easily lost in transcription or by cause of would-be sultans seeking to justify their cruelty. The Code does not abide warring between Sons, a facet easily lost and ignored but a point of shame that could tarnish many of our leaders.");
            else text = new TextObject("{=!}It is wise to ask this - we dwell in uncertain times. Greed and rage cloud the hearts of many. Do no harm to the other Sons of Asera; the villagers should never fear your blade in these lands. Do not rob them of their wealth, even if the Sultan demands it - you must be charitable, you are their sibling.");

            return text;
        }

        public override TextObject GetClergyForbiddenAnswerLast(int rank)
        {
            TextObject text = null;
            if (rank == 2)
                text = new TextObject("{=!}To not seek vengeance against those who have attacked your holdings, besieged your homes, murdered your people - is a greater shame. No doctrine of peace can outweigh the Code of Asera in this regard. Many such debts need to be paid, and it is horrifying that our rulers so often choose to forget this. The serpent which bites you and slithers away does not leave you in peace; only when the serpent is crushed, maimed, unable to bite you again does it depart in peace.");
            else text = new TextObject("{=!}When you are called by your brothers, you must come to them; you must serve beside them, you must never fail them if it is in your power. Know also that keeping the company of those outside the Code of Asera will surely corrupt you - you are your brother’s keeper, and they in turn yours. For good or for ill.");

            return text;
        }

        public override TextObject GetClergyGreeting(int rank)
        {
            TextObject text = null;
            if (rank == 2)
                text = new TextObject("{=!}Oh aye? What brings you before this Brithem judge? Lost your favorite cattle, perhaps scrape your knee on your way up the Uchalion? If you come seeking satisfaction, you’re better off finding it in a Vlandian brothel or in some imperial parlor succoring down milk-of-the-poppy. My judgment is not meant to bring you anything you couldn’t take for yourself, and my law-speaking is for the Battanian people not whatever matter of wild hound you were born to be.");
            else text = new TextObject("{=!}Am I to have words thee, {NAME}? And if so, are they to be kind ones? Do you come to me in the hopes that I might slather a bride in mud so her wiles are not driven to hysteria by the invisible probings of the fair folk? Mayhaps you heard tales of the Brandui like myself and how we will put a gimlet eye on rivals for a price - oh aye, make their loins shrivel up like a gooseberry and produce only half-wit sons? You best speak your part quick, I’ve no time for the fool courting of foreign folk.You best watch your words too, there’s nary a man in this land who wouldn’t stab you in the gizzard to win my approval.")
                    .SetTextVariable("NAME", Hero.MainHero.Name);

            return text;
        }

        public override TextObject GetClergyGreetingInducted(int rank)
        {
            TextObject text = null;
            if (rank == 2)
                text = new TextObject("{=!}Croseo, kinsfolk. How does this season upon the Uchalion greet ye? Have you drunk of the Llyn Tywal lately? Bloodied the noses of Vlandian dogsbodies and Sturgian wastrels? You stand here upon our blessed soil and have fool’s pride enough to come upon your Brithem brethren and act his equal - so I must assume you make your ancestors proud or you’ve stones enough to stand in my eyeline and bluff!");
            else text = new TextObject("{=!}Croseo, you child of the Uchalion. Have you looked upon the Morcomb in recent days? Have you glutted yourself on the fish of the River Fiur? Bashed the teeth in of any southern fool who’d claim themselves emperor of all they survey? You stand before me on blessed soil and you speak to the Bandrui as though you were a Brithem or mormaer lordling - so I assume you’ve cause to bid for my counsel. Lest ye be a fool, though to be a fool can be a blessed thing.Fool’s have the chance to leave their foolishness behind, after all.");

            return text;
        }

        public override TextObject GetClergyInduction(int rank)
        {
            TextObject text = null;
            
            if (rank == 2)
                text = new TextObject("{=!}Peace be upon you, my kin. Have you come to study the Code of Asera? I shall grant you what wisdom I have gleaned in my long hours of study, but as your brother I must tell you that I find myself more ignorant the more I realize the breadth of what there is still yet to learn.");
            else
            {
                Settlement settlement = Settlement.CurrentSettlement;
                if (settlement == null || !settlement.IsVillage) return null;

                if (Hero.MainHero.Culture != Utils.Helpers.GetCulture("aserai"))
                    text = new TextObject("{=!}Alas, you are no Son of Asera and thus you could never truly follow the Code of Asera. Not in any way that I could fathom. There may be precedent for one beyond our blood to successfully follow the code, but for this you should seek out an Akhund; a scholar of the faith.");

                float relation = 0;
                foreach (Hero notable in settlement.Notables)
                    relation += notable.GetRelation(Hero.MainHero);

                float medium = relation / settlement.Notables.Count;
                if (medium < 0)
                    text = new TextObject("{=!}You think that it would go unnoticed how the folk here cringe at your visage? How your name is whispered with scornful lips? Are they mislead about you? Perhaps, perhaps. We shall see.");
                else if (medium < 20)
                    text = new TextObject("{=!}You are known to me and to this village; not as a savior or as a good soul, but as one of us. You are humble, perhaps because you lack the boldness to pursue being charitable - or perhaps just the means. I do not know, and I do not judge.");
            }

            return text;
        }

        public override TextObject GetClergyInductionLast(int rank)
        {
            TextObject text = null;
            
            if (rank == 2)
                text = new TextObject("{=!}Peace be upon you, my kin. Have you come to study the Code of Asera? I shall grant you what wisdom I have gleaned in my long hours of study, but as your brother I must tell you that I find myself more ignorant the more I realize the breadth of what there is still yet to learn.");
            else
            {
                Settlement settlement = Settlement.CurrentSettlement;
                if (settlement == null || !settlement.IsVillage) return null;

                if (Hero.MainHero.Culture != Utils.Helpers.GetCulture("aserai"))
                    text = new TextObject("{=!}I wish you well in such pursuits, and that you live a life of peace wherever this path may take you.");

                float relation = 0;
                foreach (Hero notable in settlement.Notables)
                    relation += notable.GetRelation(Hero.MainHero);

                float medium = relation / settlement.Notables.Count;
                if (medium < 0)
                    text = new TextObject("{=!}If you wish to be made a follower of the Code of Asera, you must treat these people as you would a sibling - you must cherish them, exalt them, protect them and educate them. Show them your better nature and I shall perform upon you our rites of induction.");
                else if (medium < 20)
                    text = new TextObject("{=!}I welcome you, my kin - blood of my blood. May you go in peace and bring honor to his legacy.");
                else text = new TextObject("{=!}");
            }

            return text;
        }

        public override TextObject GetClergyPreachingAnswer(int rank)
        {
            TextObject text = null;
            if (rank == 2)
                text = new TextObject("{=!}My judgment and discernment is a matter of the Amra Ollamh, for I am a teacher who speaks of the great cycles of our people - from the giant’s seat of the lower Rhennod to the riverlands of the Ircara! ‘Tis true, many a land has been lost to fool imperials and bastard-born Nordlanders playing at Sturgians; but a Battanian heart beats ever true. I shepherd our tales, our traditions; ensure the young know how they’re meant to act and that the old don’t forget their place.");
            else text = new TextObject("{=!}I carry the soul of our people with my words and I weave our spirit into our actions; for I am a Bandrui and this is my purpose. Be they a child born upon the Ailta Druin or left to a cruel winter’s chill on the Rock of Glanys, it is my voice that guides the next generation of our people to glory! ‘Tis my voice more than any other that tells the tale of our finest hunters, of clever lords in wolfskin mien, or highland rebels cutting the gristle from the gullets of imperial soft bodies. ‘Tis sad, the state of the Uchalion and her people.In elder days my circle would dwell in sacred groves and offer counsel to heroes and high kings - now we languish in walls of stone, beset by foreign hedge knights, Sturgian raiders, and whatever mess the Empire sees fit to view itself as.");

            return text;
        }

        public override TextObject GetClergyPreachingAnswerLast(int rank)
        {
            TextObject text = null;
            if (rank == 2)
                text = new TextObject("{=!}You might hear tell that our lot are charlatans bickering on about river nymphs and sea dragons - but allegory and metaphor aren’t words belonging only to toga-shriven scribes. And there’s more to this world than you or I have the Sight to simply dismiss after all. But to answer your question, and to say it right, I’ll say just this: I am that which judges a true Battanian of their worth.To see that they make mischief and might alike enough to please their grandfather’s grandfathers in the heavenly hereafter.Ne’er are there more an Uchalion born Battanian than those who follow the creed of the Amra Ollamh.");
            else text = new TextObject("{=!}A cruel humiliation, to be one who touches upon the mysterious and grants counsel now reduced to the stuff of gossiping washerwomen and charlatan knaves in search of river nymphs to bed and lindworms to slay. Allegory and metaphor lost to those who view our lands as savage and backward as their own prejudices. But I’ll speak true and quick then, for you’ve not come for my screed. I am that which guides a Battanian to know their truth.To see their heart beats in rhythm with the cycles of creation, that they know might and mischief enough to please the unseen and to court the joy of their ancestors long displaced.Ne’er are there more an Uchalion born Battanian than those who are guided by the Amra Ollamh.");

            return text;
        }

        public override TextObject GetClergyProveFaith(int rank)
        {
            TextObject text = null;
            if (rank == 2)
                text = new TextObject("{=!}‘Tis both no simple thing and a thing that couldn’t be simpler; not that I wish to speak in riddles. Save that nonsense for the draoithe. If you want to prove your mettle, you must simply be a Battanian. Of course, to be Battanian is a tarnished thing in our current state - too many folk wrongly think they know what it means. If you wish to prove yourself wise in the Amra Ollamh, you must make yourself known for your cunning and your guile.");
            else text = new TextObject("{=!}Supposing you’re seeking a fate higher than a bóaire ‘pon the highlands, there are ways and means you could show yourself a true Battanian. First matter of that, of course, is to be a Battanian - be ye tarnished or true by the many foes which nip upon our borderlands with the ravening greed of thieving children. Assuming you can manage to be born right of our blood, ‘tis a simple task to follow. Gather yourself a throng of right - minded Battanians and make yours a name to be sung and feared.");


            return text;
        }

        public override TextObject GetClergyProveFaithLast(int rank)
        {
            TextObject text = null;
            if (rank == 2)
                text = new TextObject("{=!}Make a name for yourself which is held in the fear of landed lords and in the whimsical romance of young dreamers. Raid cattle from those without the mettle to keep them safe, and make lords both Battanian and foreign fear your blade but know your charity. You’ll prove your faith by shepherding tomorrow for the Battanian people; with cattle, with iron, and were you of a more legendary portent - by stealing back what others have stolen from us!");
            else text = new TextObject("{=!}Don yourself the wolfskins if you’ve the mind for tradition; take that which can be yours by right of might, honor no landed claim - Battanians and beasts alike may graze where they deign to. Bring iron to the forges and the folk of Battania so youngbloods might dream of blades to someday hold in more than their errant wishes! And were you truly to be guided by the Amra Ollamh, you’d cast the Vlandian, Sturgian, and imperial dogs from our land.Preferably off the side of a cliff, but none shall be picky so long as they’re made to flee in fear of your wrath.");

            return text;
        }

        public override string GetId() => "amra";

        public override int GetIdealRank(Settlement settlement)
        {
            if (settlement.IsVillage)
            {
                if (MBRandom.RandomInt(1, 100) < 50)
                    return 2;
                return 1;
            }

            return 0;
        }

        public override Divinity GetMainDivinity() => mainGod;

        public override TextObject GetMainDivinitiesDescription() => new TextObject("{=!}Great Spirits");

        public override int GetMaxClergyRank() => 2;

        public override TextObject GetRankTitle(int rank)
        {
            TextObject text = null;
            if (rank == 2)
                text = new TextObject("{=!}Brithem");
            else text = new TextObject("{=!}Bandrui");

            return text;
        }

        public override MBReadOnlyList<Divinity> GetSecondaryDivinities() => pantheon.GetReadOnlyList();

        public override TextObject GetSecondaryDivinitiesDescription() => new TextObject("{=!}Spirits");
    }
}
