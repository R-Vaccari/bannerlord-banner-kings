using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Asera
{
    public class AseraFaith : MonotheisticFaith
    {
        public override TextObject GetFaithName()
        {
            return new("{=!}Code of Asera");
        }

        public override TextObject GetFaithDescription()
        {
            return new(
                "{=!}Founded by the immediate sons of the legendary patriarch of the Southlands, the Code of Asera forms the basis of philosophy, art and law among the members of the confederated Aserai tribes. All tribesmen of the Aserai who do not wander as bedouin, are assumed to be followers of the Code of Asera - though only those who seek to study the intricacies of the mythic patrilineal bloodline and its influence upon the world are celebrated and viewed as Sons of Asera; a facet of colloquial nomenclature meant to force camaraderie as all members of the Sultanate make claim to being descendants of Asera. Faqir, akhund, imams, and mushid make up the various levels of clergy who preach the Code and seek to apply its nuanced legalisms and rites to the Southlands. As a whole, those who follow the Code of Asera place an emphasis on charity between communities, vengeance against those who defy precedent or tradition, and mercy given in kindness with justice forced if kindness is scorned.");
        }


        public override TextObject GetClergyForbiddenAnswer(int rank)
        {
            TextObject text = null;
            if (rank == 4)
            {
                text = new TextObject(
                    "{=!}Do not be as a glutton or a beast; neither hoarding nor bloodlust should color your life. If you take justice into your hands you may find yourself forever grasping for more - allow your betters to guide you as an elder sibling should; do not kill prisoners who have wronged you without their say. But do not reveal yourself to be lax or limp of arm either; for justice will often be your task.");
            }
            else if (rank == 3)
            {
                text = new TextObject(
                    "{=!}You must never betray the blood of your blood, to do so is to spit upon the Code. If you are called to serve the Aserai, you must shed blood for their cause - at least once, so as to show that you honor your siblings even should you disagree with their choices. You must ensure any who serve you do not suffer the ravages of conflict without justice on your lips and a safe bastion for them to find reprieve in.");
            }
            else if (rank == 2)
            {
                text = new TextObject(
                    "{=!}…A curious question; though I suppose I would know the deeper answers to this. Few are the faqir or even imam who are as studied in the various things so easily lost in transcription or by cause of would-be sultans seeking to justify their cruelty. The Code does not abide warring between Sons, a facet easily lost and ignored but a point of shame that could tarnish many of our leaders.");
            }
            else
            {
                text = new TextObject(
                    "{=!}It is wise to ask this - we dwell in uncertain times. Greed and rage cloud the hearts of many. Do no harm to the other Sons of Asera; the villagers should never fear your blade in these lands. Do not rob them of their wealth, even if the Sultan demands it - you must be charitable, you are their sibling.");
            }

            return text;
        }

        public override TextObject GetClergyForbiddenAnswerLast(int rank)
        {
            TextObject text = null;
            if (rank == 4)
            {
                text = new TextObject(
                    "{=!}…And in weakness, do not allow others to usurp your station. Rebellion, sedition, betrayal - anathema to the Code of Asera. Brother shall not strike against brother, least of all by a knife in the back or by rousing a mob with vicious words or common strife. Suffer not rebellion, for to allow it to foment in the first place speaks to the immensity of your failings. All the pointless bloodshed which may come from such will tarnish you eternal, such that you may find yourself denied Heaven - condemned to an agony of your own making.");
            }
            else if (rank == 3)
            {
                text = new TextObject(
                    "{=!}Be mindful also of the company you keep, for though all among the Aserai are as brothers - and even the Jawwal and Bedouin are as our cousins; those beyond our blood lineage may not understand your purpose. They may lead you from the path, away from the Code; away from the good of the Aserai. But perhaps I speak this in fear - the long toll of the Empire upon our culture.");
            }
            else if (rank == 2)
            {
                text = new TextObject(
                    "{=!}To not seek vengeance against those who have attacked your holdings, besieged your homes, murdered your people - is a greater shame. No doctrine of peace can outweigh the Code of Asera in this regard. Many such debts need to be paid, and it is horrifying that our rulers so often choose to forget this. The serpent which bites you and slithers away does not leave you in peace; only when the serpent is crushed, maimed, unable to bite you again does it depart in peace.");
            }
            else
            {
                text = new TextObject(
                    "{=!}When you are called by your brothers, you must come to them; you must serve beside them, you must never fail them if it is in your power. Know also that keeping the company of those outside the Code of Asera will surely corrupt you - you are your brother’s keeper, and they in turn yours. For good or for ill.");
            }

            return text;
        }

        public override TextObject GetClergyGreeting(int rank)
        {
            TextObject text = null;
            if (rank == 4)
            {
                text = new TextObject(
                    "{=!}Know that I tolerate your presence on behalf of my blooded siblings in the Banu, and though you may desire to coax me with honeyed words or threats of coercion - know also that I am beyond your grasp. I am the Murshid of [Placename], and my word guides the Sultanate by the reading of the Code of Asera. Speak truthfully and with no deceit, lest I denounce your lineage and see them flayed to the last.");
            }
            else if (rank == 3)
            {
                text = new TextObject(
                    "{=!}Peace be upon you, outlander. May you walk with the safety of Asera while you dwell in the company of his sons - and while you walk the lands where his Code is law. I am an Imam of the Aserai, and I bid you hospitality expecting it fully in kind. Is there something I may do for our mutual good?");
            }
            else if (rank == 2)
            {
                text = new TextObject(
                    "{=!}Peace be to you, outlander. You do not carry yourself as a Son of Asera, so I shall forgive your ignorance - provided you come here in search of wisdom. I am an Akhund, a scholar of the Code of Asera, the guiding truth of the southlands. If you come here with a clouded heart, there is little I can offer you.");
            }
            else
            {
                text = new TextObject(
                    "{=!}Peace be to you, outlander. May you be welcomed in the lands of the Sons of Asera. May you enter here in kindness, but know that we do court the ignorant.");
            }

            return text;
        }

        public override TextObject GetClergyGreetingInducted(int rank)
        {
            TextObject text = null;
            if (rank == 4)
            {
                text = new TextObject(
                    "{=!}Blessings of paradise and the Heavens upon you, blooded sibling. You come before the Murshid of [Placename], a guide to the Sultanate and of kin to the Banu. Surely you know that I am a busy man who must contend with matters of state whilst consulting the Code of Asera. What is it that I may do for you while I have this single moment to offer?");
            }
            else if (rank == 3)
            {
                text = new TextObject(
                    "{=!}Mashaera, blood of my blood. It is good to see you are alive and that you are in good health; for the world is rife with conflict beyond our brotherhood and little is to be held as certain. Are you here on pilgrimage or duty, in good tidings or ill news? How may this humble Imam be of service to his sibling?");
            }
            else if (rank == 2)
            {
                text = new TextObject(
                    "{=!}Peace be upon you, my kin. Have you come to study the Code of Asera? I shall grant you what wisdom I have gleaned in my long hours of study, but as your brother I must tell you that I find myself more ignorant the more I realize the breadth of what there is still yet to learn.");
            }
            else
            {
                text = new TextObject(
                    "{=!}Peace be upon you and may you be favored by Asera, my kin. What may this humble faqir do for you on this glorious day?");
            }

            return text;
        }

        public override TextObject GetClergyInduction(int rank)
        {
            TextObject text = null;
            if (rank == 4)
            {
            }
            else if (rank == 3)
            {
                text = new TextObject(
                    "{=!}Mashaera, blood of my blood. It is good to see you are alive and that you are in good health; for the world is rife with conflict beyond our brotherhood and little is to be held as certain. Are you here on pilgrimage or duty, in good tidings or ill news? How may this humble Imam be of service to his sibling?");
            }
            else if (rank == 2)
            {
                text = new TextObject(
                    "{=!}Peace be upon you, my kin. Have you come to study the Code of Asera? I shall grant you what wisdom I have gleaned in my long hours of study, but as your brother I must tell you that I find myself more ignorant the more I realize the breadth of what there is still yet to learn.");
            }
            else
            {
                var settlement = Settlement.CurrentSettlement;
                if (settlement == null || !settlement.IsVillage)
                {
                    return null;
                }

                if (Hero.MainHero.Culture != Utils.Helpers.GetCulture("aserai"))
                {
                    text = new TextObject(
                        "{=!}Alas, you are no Son of Asera and thus you could never truly follow the Code of Asera. Not in any way that I could fathom. There may be precedent for one beyond our blood to successfully follow the code, but for this you should seek out an Akhund; a scholar of the faith.");
                }

                float relation = 0;
                foreach (var notable in settlement.Notables)
                {
                    relation += notable.GetRelation(Hero.MainHero);
                }

                var medium = relation / settlement.Notables.Count;
                if (medium < 0)
                {
                    text = new TextObject(
                        "{=!}You think that it would go unnoticed how the folk here cringe at your visage? How your name is whispered with scornful lips? Are they mislead about you? Perhaps, perhaps. We shall see.");
                }
                else if (medium < 20)
                {
                    text = new TextObject(
                        "{=!}You are known to me and to this village; not as a savior or as a good soul, but as one of us. You are humble, perhaps because you lack the boldness to pursue being charitable - or perhaps just the means. I do not know, and I do not judge.");
                }
            }

            return text;
        }

        public override TextObject GetClergyInductionLast(int rank)
        {
            TextObject text = null;
            if (rank == 4)
            {
            }
            else if (rank == 3)
            {
                text = new TextObject(
                    "{=!}Mashaera, blood of my blood. It is good to see you are alive and that you are in good health; for the world is rife with conflict beyond our brotherhood and little is to be held as certain. Are you here on pilgrimage or duty, in good tidings or ill news? How may this humble Imam be of service to his sibling?");
            }
            else if (rank == 2)
            {
                text = new TextObject(
                    "{=!}Peace be upon you, my kin. Have you come to study the Code of Asera? I shall grant you what wisdom I have gleaned in my long hours of study, but as your brother I must tell you that I find myself more ignorant the more I realize the breadth of what there is still yet to learn.");
            }
            else
            {
                var settlement = Settlement.CurrentSettlement;
                if (settlement == null || !settlement.IsVillage)
                {
                    return null;
                }

                if (Hero.MainHero.Culture != Utils.Helpers.GetCulture("aserai"))
                {
                    text = new TextObject(
                        "{=!}I wish you well in such pursuits, and that you live a life of peace wherever this path may take you.");
                }

                float relation = 0;
                foreach (var notable in settlement.Notables)
                {
                    relation += notable.GetRelation(Hero.MainHero);
                }

                var medium = relation / settlement.Notables.Count;
                if (medium < 0)
                {
                    text = new TextObject(
                        "{=!}If you wish to be made a follower of the Code of Asera, you must treat these people as you would a sibling - you must cherish them, exalt them, protect them and educate them. Show them your better nature and I shall perform upon you our rites of induction.");
                }
                else if (medium < 20)
                {
                    text = new TextObject(
                        "{=!}I welcome you, my kin - blood of my blood. May you go in peace and bring honor to his legacy.");
                }
                else
                {
                    text = new TextObject("{=!}");
                }
            }

            return text;
        }

        public override TextObject GetClergyPreachingAnswer(int rank)
        {
            TextObject text = null;
            if (rank == 4)
            {
                text = new TextObject(
                    "{=!}I offer my sage counsel to the kin of the Banu, to guide them upon the paths walked by Asera of old. I grant them the means of pilgrimage - for only when one knows the deepest wisdom and learned truths of the Code of Asera may they truly strive for self-improvement.");
            }
            else if (rank == 3)
            {
                text = new TextObject(
                    "{=!}I preach the glory of Asera, our progenitor of legend. I preach lessons learned from the treasury of wisdom he has granted us as his children. I preach that the Aserai shall reign in happiness and in sorrow, so long as we remain as brothers within the Banu confederacies.");
            }
            else if (rank == 2)
            {
                text = new TextObject(
                    "{=!}I preach that which is hidden in poetics of Nahasi dialects and in the riddles of converts stolen away by Darshi slavers in generations past. I seek to unravel the mysteries of all that Asera asked of his sons - to truly understand charity, devotion, and the cost of peace even in these times of eschatological turmoil.");
            }
            else
            {
                text = new TextObject(
                    "{=!}I preach the Code of Asera, the backbone of our society, the soul of our nation. For we are all Sons of Asera in these lands, and it is through the patriarch’s example that we go forth in honor and bring glory to his legacy.");
            }


            return text;
        }

        public override TextObject GetClergyPreachingAnswerLast(int rank)
        {
            TextObject text = null;

            if (rank == 4)
            {
                text = new TextObject(
                    "{=!}I ensure they remember that they are as siblings to the lowliest beggar or the most jackal-thirsting Jawwal marauder; that they must always express love towards that which is good and revile the roguery that leads the soul towards wicked deeds.");
            }
            else if (rank == 3)
            {
                text = new TextObject(
                    "{=!}I preach that our future shall be as luminous as the sun and as enduring as the Nahasa, so long as we remain united, so long as we are charitable, so long as we welcome even the Jawwal to embrace the Code as their own guiding path in life!");
            }
            else if (rank == 2)
            {
                text = new TextObject(
                    "{=!}I preach that we should question the nature of goodness and of wisdom; for a good man would not claim to be wise, and a wise man not claim to be good.");
            }
            else
            {
                text = new TextObject(
                    "{=!}We are those who adhere to tradition but know also the struggle to move beyond the belligerence of the past - we are no Jawwal, after all.");
            }


            return text;
        }

        public override TextObject GetClergyProveFaith(int rank)
        {
            TextObject text = null;
            if (rank == 4)
            {
                text = new TextObject(
                    "{=!}You come before me, like a wriggling eel slithering to a master archer and asking to learn to wield the bow. I shall speak plainly to you, for my time is short and my station vaunted. When you see that which is evil, you must change it by whatever force you can - if not your hands, then your heart, if not your heart, then your words.");
            }
            else if (rank == 3)
            {
                text = new TextObject(
                    "{=!}A common question among the devout and misguided alike; for how can one truly be assured they walk the righteous path? We know not until the end comes and we are weighed against the deeds of our progenitor; but I shall guide you with what wisdom I can.");
            }
            else if (rank == 2)
            {
                text = new TextObject(
                    "{=!}Consider your actions and why it is you perform them. If you are of influence, do you wield this power for the good of your siblings or do you take it for your own aggrandizement? When your people starve, do you feast? Do you turn to the sword? Is blood spilled for bread gained a worthy trade?");
            }
            else
            {
                text = new TextObject(
                    "{=!}It does my heart kindly to hear you wish to prove your faith. We live in an age of lip service by most - though I hold no blame to the common man for this; they show their adherence and their faith in their daily labors. If you wish to prove your faith and your adherence to doctrine, you must be a soul who knows peace and who adheres to hierarchy.");
            }


            return text;
        }

        public override TextObject GetClergyProveFaithLast(int rank)
        {
            TextObject text = null;
            if (rank == 4)
            {
                text = new TextObject(
                    "{=!}Seek to improve the lives of even the least of your people and you shall make kings of them all; and know that should you find yourself poorer and your servants richer that you are a better sibling than many of your blood. You can only hope then that they shall follow your example, follow the Code of Asera, and spread this prosperity to all.");
            }
            else if (rank == 3)
            {
                text = new TextObject(
                    "{=!}Foremost in my viewing, as a man of the cities of our lands; is the act of almsgiving. We are as keepers to our brothers, to our sisters and siblings; and only a cruel soul would let the blood of their blood starve.You are only as pure as the lowest among us, and thus you purify yourself of sin when you seek to uplift the whole of the Aserai.");
            }
            else if (rank == 2)
            {
                text = new TextObject(
                    "{=!}If you seek to prove your faith, you must internalize the Code of Asera - you must learn and accept that you are but one of his Sons - and you must seek to bring forth a better world for your children to inherit. Prove your faith by making that world. Time will tell if the image in your mind is truly an embodiment of the Code’s doctrines or merely self-indulgence.");
            }
            else
            {
                text = new TextObject(
                    "{=!}Release your prisoners if you no longer war with their masters; or give them to your master for they shall know better. Choose peace when your enemies desire it; let them lick their wounds and if they strike you again, punish them with biting steel and the shame of their recidivism.");
            }

            return text;
        }

        public override string GetId()
        {
            return "asera";
        }

        public override int GetIdealRank(Settlement settlement, bool isCapital)
        {
            if (settlement.IsTown)
            {
                return 3;
            }

            if (settlement.IsCastle)
            {
                return 2;
            }

            return 1;
        }

        public override Divinity GetMainDivinity()
        {
            throw new NotImplementedException();
        }

        public override TextObject GetMainDivinitiesDescription()
        {
            throw new NotImplementedException();
        }

        public override int GetMaxClergyRank()
        {
            return 4;
        }

        public override TextObject GetRankTitle(int rank)
        {
            TextObject text = null;
            if (rank == 4)
            {
                text = new TextObject("{=!}Murshid");
            }
            else if (rank == 3)
            {
                text = new TextObject("{=!}Imam");
            }
            else if (rank == 2)
            {
                text = new TextObject("{=!}Akhund");
            }
            else
            {
                text = new TextObject("{=!}Faqir");
            }

            return text;
        }

        public override MBReadOnlyList<Divinity> GetSecondaryDivinities()
        {
            return pantheon.GetReadOnlyList();
        }

        public override TextObject GetSecondaryDivinitiesDescription()
        {
            return new("{=!}Schools");
        }

        public override (bool, TextObject) GetInductionAllowed(Hero hero, int rank)
        {
            throw new NotImplementedException();
        }

        public override TextObject GetBlessingAction()
        {
            throw new NotImplementedException();
        }

        public override TextObject GetBlessingQuestion()
        {
            throw new NotImplementedException();
        }

        public override TextObject GetBlessingConfirmQuestion()
        {
            throw new NotImplementedException();
        }

        public override TextObject GetBlessingQuickInformation()
        {
            throw new NotImplementedException();
        }

        public override TextObject GetBlessingActionName()
        {
            throw new NotImplementedException();
        }
    }
}