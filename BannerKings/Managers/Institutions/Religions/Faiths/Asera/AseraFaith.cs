using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Asera
{
    public class AseraFaith : HenotheisticFaith
    {
        public override Settlement FaithSeat => Settlement.All.First(x => x.StringId == "town_A1");

        public override TextObject GetDescriptionHint()
        {
            return new TextObject("{=ezGSFaau}Founded by the immediate sons of the legendary patriarch of the Southlands, the Code of Asera forms the basis of philosophy, art and law among the members of the confederated Aserai tribes.");
        }

        public override Banner GetBanner() => new Banner("1.143.143.1836.1836.764.764.1.0.0.463.116.116.234.234.764.639.1.0.0.321.116.116.316.316.764.919.0.0.0.428.116.116.162.162.964.649.1.0.-91.428.116.116.162.162.564.649.1.0.90");

        public override bool IsCultureNaturalFaith(CultureObject culture)
        {
            if (culture.StringId == "aserai")
            {
                return true;
            }

            return false;
        }
        public override bool IsHeroNaturalFaith(Hero hero) => IsCultureNaturalFaith(hero.Culture);

        public override TextObject GetFaithName()
        {
            return new TextObject("{=4sC3k7fO}Code of Asera");
        }

        public override TextObject GetFaithDescription()
        {
            return new TextObject("{=CoLCEywd}Founded by the immediate sons of the legendary patriarch of the Southlands, the Code of Asera forms the basis of philosophy, art and law among the members of the confederated Aserai tribes. All tribesmen of the Aserai who do not wander as bedouin, are assumed to be followers of the Code of Asera - though only those who seek to study the intricacies of the mythic patrilineal bloodline and its influence upon the world are celebrated and viewed as Sons of Asera; a facet of colloquial nomenclature meant to force camaraderie as all members of the Sultanate make claim to being descendants of Asera. Faqir, akhund, imams, and mushid make up the various levels of clergy who preach the Code and seek to apply its nuanced legalisms and rites to the Southlands. As a whole, those who follow the Code of Asera place an emphasis on charity between communities, vengeance against those who defy precedent or tradition, and mercy given in kindness with justice forced if kindness is scorned.");
        }

        public override TextObject GetClergyForbiddenAnswer(int rank)
        {
            var text = rank switch
            {
                4 => new TextObject("{=uF2hBKKU}Do not be as a glutton or a beast; neither hoarding nor bloodlust should color your life. If you take justice into your hands you may find yourself forever grasping for more - allow your betters to guide you as an elder sibling should; do not kill prisoners who have wronged you without their say. But do not reveal yourself to be lax or limp of arm either; for justice will often be your task."),
                3 => new TextObject("{=EFKv7DGi}You must never betray the blood of your blood, to do so is to spit upon the Code. If you are called to serve the Aserai, you must shed blood for their cause - at least once, so as to show that you honor your siblings even should you disagree with their choices. You must ensure any who serve you do not suffer the ravages of conflict without justice on your lips and a safe bastion for them to find reprieve in."),
                2 => new TextObject("{=bnfbjMGa}…A curious question; though I suppose I would know the deeper answers to this. Few are the faqir or even imam who are as studied in the various things so easily lost in transcription or by cause of would-be sultans seeking to justify their cruelty. The Code does not abide warring between Sons, a facet easily lost and ignored but a point of shame that could tarnish many of our leaders."),
                _ => new TextObject("{=hvuqVf2m}It is wise to ask this - we dwell in uncertain times. Greed and rage cloud the hearts of many. Do no harm to the other Sons of Asera; the villagers should never fear your blade in these lands. Do not rob them of their wealth, even if the Sultan demands it - you must be charitable, you are their sibling.")
            };

            return text;
        }

        public override TextObject GetClergyForbiddenAnswerLast(int rank)
        {
            var text = rank switch
            {
                4 => new TextObject("{=spy2ujFu}…And in weakness, do not allow others to usurp your station. Rebellion, sedition, betrayal - anathema to the Code of Asera. Brother shall not strike against brother, least of all by a knife in the back or by rousing a mob with vicious words or common strife. Suffer not rebellion, for to allow it to foment in the first place speaks to the immensity of your failings. All the pointless bloodshed which may come from such will tarnish you eternal, such that you may find yourself denied Heaven - condemned to an agony of your own making."),
                3 => new TextObject("{=itzfxuQ9}Be mindful also of the company you keep, for though all among the Aserai are as brothers - and even the Jawwal and Bedouin are as our cousins; those beyond our blood lineage may not understand your purpose. They may lead you from the path, away from the Code; away from the good of the Aserai. But perhaps I speak this in fear - the long toll of the Empire upon our culture."),
                2 => new TextObject("{=hmkcSU3i}To not seek vengeance against those who have attacked your holdings, besieged your homes, murdered your people - is a greater shame. No doctrine of peace can outweigh the Code of Asera in this regard. Many such debts need to be paid, and it is horrifying that our rulers so often choose to forget this. The serpent which bites you and slithers away does not leave you in peace; only when the serpent is crushed, maimed, unable to bite you again does it depart in peace."),
                _ => new TextObject("{=8b9Qad8j}When you are called by your brothers, you must come to them; you must serve beside them, you must never fail them if it is in your power. Know also that keeping the company of those outside the Code of Asera will surely corrupt you - you are your brother’s keeper, and they in turn yours. For good or for ill.")
            };

            return text;
        }

        public override TextObject GetClergyGreeting(int rank)
        {
            var text = rank switch
            {
                4 => new TextObject("{=NiDTPPz2}Know that I tolerate your presence on behalf of my blooded siblings in the Banu, and though you may desire to coax me with honeyed words or threats of coercion - know also that I am beyond your grasp. I am the Murshid of [Placename], and my word guides the Sultanate by the reading of the Code of Asera. Speak truthfully and with no deceit, lest I denounce your lineage and see them flayed to the last."),
                3 => new TextObject("{=gw31Xmny}Peace be upon you, outlander. May you walk with the safety of Asera while you dwell in the company of his sons - and while you walk the lands where his Code is law. I am an Imam of the Aserai, and I bid you hospitality expecting it fully in kind. Is there something I may do for our mutual good?"),
                2 => new TextObject("{=fvJt4aTX}Peace be to you, outlander. You do not carry yourself as a Son of Asera, so I shall forgive your ignorance - provided you come here in search of wisdom. I am an Akhund, a scholar of the Code of Asera, the guiding truth of the southlands. If you come here with a clouded heart, there is little I can offer you."),
                _ => new TextObject("{=vP3Sj0UE}Peace be to you, outlander. May you be welcomed in the lands of the Sons of Asera. May you enter here in kindness, but know that we do court the ignorant.")
            };

            return text;
        }

        public override TextObject GetClergyGreetingInducted(int rank)
        {
            var text = rank switch
            {
                4 => new TextObject("{=uQTPq7uS}Blessings of paradise and the Heavens upon you, blooded sibling. You come before the Murshid of [Placename], a guide to the Sultanate and of kin to the Banu. Surely you know that I am a busy man who must contend with matters of state whilst consulting the Code of Asera. What is it that I may do for you while I have this single moment to offer?"),
                3 => new TextObject("{=ws5gbCMX}Mashaera, blood of my blood. It is good to see you are alive and that you are in good health; for the world is rife with conflict beyond our brotherhood and little is to be held as certain. Are you here on pilgrimage or duty, in good tidings or ill news? How may this humble Imam be of service to his sibling?"),
                2 => new TextObject("{=yMvqUQpw}Peace be upon you, my kin. Have you come to study the Code of Asera? I shall grant you what wisdom I have gleaned in my long hours of study, but as your brother I must tell you that I find myself more ignorant the more I realize the breadth of what there is still yet to learn."),
                _ => new TextObject("{=QqhAVhTr}Peace be upon you and may you be favored by Asera, my kin. What may this humble faqir do for you on this glorious day?")
            };

            return text;
        }

        public override TextObject GetClergyInduction(int rank)
        {
            TextObject text = null;
            switch (rank)
            {
                case 4:
                    {
                        if (Hero.MainHero.Culture.StringId != "aserai" || Clan.PlayerClan.Tier <= 2)
                        {
                            text = new TextObject("{=a8p42u63}I have neither the time nor the inclination to perform the rites and observe the oaths of one such as you. You grasp far beyond your station by asking this of me. Consult with a faqir or an imam of the cities, learn to know these lands and learn also your place.");
                            return text;
                        }

                        if (Clan.PlayerClan.Tier <= 4)
                        {
                            text = new TextObject("{=aXDpgcsg}Had I the time to do so, I might honor your request and observe the oath you must take to become the blood of my blood - but you lack decorum. Perhaps if you were a wayward prince or a lord of the Jawwal I would consider this my righteous task.");
                            return text;
                        }

                        if (Clan.PlayerClan.Tier >= 5)
                        {
                            text = new TextObject("{=sJPVusPG}There are few honors I have not known or entertained, but rare is the honor of inducting one such as yourself formally into our brotherhood. You are a good sibling to those whom you share little more than homeland, a steward of the Aserai.");
                            return text;
                        }

                        break;
                    }
                case 3:
                    {
                        if (Hero.MainHero.Culture.StringId == "aserai")
                        {
                            text = new TextObject("{=1AnuWDCo}Forgive my presumption, I thought you were already a follower of the Code of Asera. I celebrate that you have come to find enlightenment and truth, to embrace the warm heart of the patriarch and to accept his guiding lessons. Whether you be a Jawwal come to light, a bedouin ready to rejoin their brothers, or a soul liberated from faiths fouler - know that I welcome you. From this day forth, you shall carry yourself by the Code of Asera; let all the Aserai be as your brothers.");
                            return text;
                        }

                        if (Hero.MainHero.Culture.StringId == "empire" || Hero.MainHero.Culture.StringId == "khuzait")
                        {
                            text = new TextObject("{=DrM1N4wQ}It is good to see our neighbors know the importance of the Code of Asera; though I must apologize - perhaps you are a distant kin. The Empire sought to make us as it makes all things; and thus those who share our blood; who share the blood of Asera; may be from lands beyond our own. I welcome you, wayward sibling, in kindness and in good fortune. May you carry the Code of Asera within your heart, may all that are Aserai be as your brothers.");
                            return text;
                        }

                        if (Hero.MainHero.Spouse != null && Hero.MainHero.Spouse.Culture.StringId == "aserai")
                        {
                            text = new TextObject("{=5HeAMORF}You are of a kind heart to do this for your beloved, for not all from foreign lands would be willing to pursue such enlightenment. Your children will be Sons of Aserai, and by marriage so too are you. Follow the Code of Asera, honor our patriarch and in doing so honor your beloved. You are part of a grand lineage now; go forth unto eternity in peace and in family.");
                            return text;
                        }

                        break;
                    }
                case 2:
                    {
                        if (Hero.MainHero.Culture.StringId == "aserai")
                        {
                            text = new TextObject("{=1AnuWDCo}Forgive my presumption, I thought you were already a follower of the Code of Asera. I celebrate that you have come to find enlightenment and truth, to embrace the warm heart of the patriarch and to accept his guiding lessons. Whether you be a Jawwal come to light, a bedouin ready to rejoin their brothers, or a soul liberated from faiths fouler - know that I welcome you. From this day forth, you shall carry yourself by the Code of Asera; let all the Aserai be as your brothers.");
                            return text;
                        }

                        if (Hero.MainHero.Culture.StringId == "empire" || Hero.MainHero.Culture.StringId == "khuzait")
                        {
                            text = new TextObject("{=DrM1N4wQ}It is good to see our neighbors know the importance of the Code of Asera; though I must apologize - perhaps you are a distant kin. The Empire sought to make us as it makes all things; and thus those who share our blood; who share the blood of Asera; may be from lands beyond our own. I welcome you, wayward sibling, in kindness and in good fortune. May you carry the Code of Asera within your heart, may all that are Aserai be as your brothers.");
                            return text;
                        }

                        break;
                    }
                default:
                {
                    var settlement = Settlement.CurrentSettlement;
                    if (settlement == null || !settlement.IsVillage)
                    {
                        return null;
                    }

                    if (Hero.MainHero.Culture != Utils.Helpers.GetCulture("aserai"))
                    {
                        text = new TextObject("{=byHGTYHh}Alas, you are no Son of Asera and thus you could never truly follow the Code of Asera. Not in any way that I could fathom. There may be precedent for one beyond our blood to successfully follow the code, but for this you should seek out an Akhund; a scholar of the faith.");
                    }

                    float relation = 0;
                    foreach (var notable in settlement.Notables)
                    {
                        relation += notable.GetRelation(Hero.MainHero);
                    }

                    var medium = relation / settlement.Notables.Count;
                    text = medium switch
                    {
                        < 0 => new TextObject("{=A06Nfw6t}You think that it would go unnoticed how the folk here cringe at your visage? How your name is whispered with scornful lips? Are they mislead about you? Perhaps, perhaps. We shall see."),
                        < 20 => new TextObject("{=CfdXEpue}You are known to me and to this village; not as a savior or as a good soul, but as one of us. You are humble, perhaps because you lack the boldness to pursue being charitable - or perhaps just the means. I do not know, and I do not judge."),
                        _ => text
                    };

                    break;
                }
            }

            return text;
        }

        public override TextObject GetClergyInductionLast(int rank)
        {
            TextObject text = null;
            switch (rank)
            {
                case 4:
                    {
                        if (Hero.MainHero.Culture.StringId != "aserai" || Clan.PlayerClan.Tier <= 2)
                        {
                            text = new TextObject("{=M1OKSp0t}Perhaps someday when you are worthy of my audience, should you still be astray from the Code, I may deign it a kindness to serve as witness to your words.");
                            return text;
                        }

                        if (Clan.PlayerClan.Tier <= 4)
                        {
                            text = new TextObject("{=uhD1HNgW}Find an imam or an akhund, they shall better serve you in this task. ");
                            return text;
                        }

                        if (Clan.PlayerClan.Tier >= 5)
                        {
                            text = new TextObject("{=UZmzR1Ty}You are worthier than most to pursue the Code of Asera, and you need only speak this oath to become the blood of my blood: \"I bear witness to the glory of Asera and the grand workings of his sons. I shall follow his code, and all of the Aserai shall be as my brothers.\"I bid you kind passage and glory befitting the power you wield. May you rise all Aserai to new heights, and bring us further into such blessed unity.");
                            return text;
                        }

                        break;
                    }
                case 3:
                    {
                        if (Hero.MainHero.Culture.StringId == "aserai")
                        {
                            text = new TextObject("{=W8ZnNO5o}If you wish to become inducted into our faith, you must simply repeat this oath to me: \"I bear witness to the glory of Asera and the grand workings of his sons. I shall follow his code, and all of the Aserai shall be as my brothers.\"");
                            return text;
                        }

                        if (Hero.MainHero.Culture.StringId == "empire" || Hero.MainHero.Culture.StringId == "khuzait")
                        {
                            text = new TextObject("{=W8ZnNO5o}If you wish to become inducted into our faith, you must simply repeat this oath to me: \"I bear witness to the glory of Asera and the grand workings of his sons. I shall follow his code, and all of the Aserai shall be as my brothers.\"");
                            return text;
                        }

                        if (Hero.MainHero.Spouse != null && Hero.MainHero.Spouse.Culture.StringId == "aserai")
                        {
                            text = new TextObject("{=W8ZnNO5o}If you wish to become inducted into our faith, you must simply repeat this oath to me: \"I bear witness to the glory of Asera and the grand workings of his sons. I shall follow his code, and all of the Aserai shall be as my brothers.\" It is done. I welcome you to our blessed family. Go now to your spouse, be joined in glory and in purpose.");
                            return text;
                        }

                        break;
                    }
                case 2:
                    {
                        if (Hero.MainHero.Culture.StringId == "aserai")
                        {
                            text = new TextObject("{=ugie03WY}Please repeat this oath and I shall serve as your witness: \"I bear witness to the glory of Asera and the grand workings of his sons. I shall follow his code, and all of the Aserai shall be as my brothers.\"I welcome you upon our blessed path, blood of my blood - my kin so far afield. I embrace you as a sibling, and ask that you depart in peace and glory. May every fortune find you.");
                            return text;
                        }

                        if (Hero.MainHero.Culture.StringId == "empire" || Hero.MainHero.Culture.StringId == "khuzait")
                        {
                            text = new TextObject("{=W8ZnNO5o}If you wish to become inducted into our faith, you must simply repeat this oath to me: \"I bear witness to the glory of Asera and the grand workings of his sons. I shall follow his code, and all of the Aserai shall be as my brothers.\"There may be some who question your faith by means of your origin, but know that an adopted son is not unprecedented among the Banu. Know that you are a Son of Asera, a follower of the Code of Asera, and none but you can take this truth away. Go now in peace, blood of my blood.");
                            return text;
                        }

                        break;
                    }
                default:
                {
                    var settlement = Settlement.CurrentSettlement;
                    if (settlement == null || !settlement.IsVillage)
                    {
                        return null;
                    }

                    if (Hero.MainHero.Culture != Utils.Helpers.GetCulture("aserai"))
                    {
                        text = new TextObject("{=mg8AZu6P}I wish you well in such pursuits, and that you live a life of peace wherever this path may take you.");
                        return text;
                    }

                    float relation = 0;
                    foreach (var notable in settlement.Notables)
                    {
                        relation += notable.GetRelation(Hero.MainHero);
                    }

                    var medium = relation / settlement.Notables.Count;
                    text = medium switch
                    {
                        < 0 => new TextObject("{=ZPEn4EDa}If you wish to be made a follower of the Code of Asera, you must treat these people as you would a sibling - you must cherish them, exalt them, protect them and educate them. Show them your better nature and I shall perform upon you our rites of induction."),
                        < 20 => new TextObject("{=DNpeTcRd}I welcome you, my kin - blood of my blood. May you go in peace and bring honor to his legacy."),
                        _ => new TextObject("{=!}")
                    };

                    break;
                }
            }

            return text;
        }

        public override TextObject GetClergyPreachingAnswer(int rank)
        {
            var text = rank switch
            {
                4 => new TextObject("{=uz70E5mx}I offer my sage counsel to the kin of the Banu, to guide them upon the paths walked by Asera of old. I grant them the means of pilgrimage - for only when one knows the deepest wisdom and learned truths of the Code of Asera may they truly strive for self-improvement."),
                3 => new TextObject("{=EofwaHJQ}I preach the glory of Asera, our progenitor of legend. I preach lessons learned from the treasury of wisdom he has granted us as his children. I preach that the Aserai shall reign in happiness and in sorrow, so long as we remain as brothers within the Banu confederacies."),
                2 => new TextObject("{=3sxQsTy7}I preach that which is hidden in poetics of Nahasi dialects and in the riddles of converts stolen away by Darshi slavers in generations past. I seek to unravel the mysteries of all that Asera asked of his sons - to truly understand charity, devotion, and the cost of peace even in these times of eschatological turmoil."),
                _ => new TextObject("{=cDU19QLK}I preach the Code of Asera, the backbone of our society, the soul of our nation. For we are all Sons of Asera in these lands, and it is through the patriarch’s example that we go forth in honor and bring glory to his legacy.")
            };

            return text;
        }

        public override TextObject GetClergyPreachingAnswerLast(int rank)
        {
            var text = rank switch
            {
                4 => new TextObject("{=QfQjwGLe}I ensure they remember that they are as siblings to the lowliest beggar or the most jackal-thirsting Jawwal marauder; that they must always express love towards that which is good and revile the roguery that leads the soul towards wicked deeds."),
                3 => new TextObject("{=nRiRiRA6}I preach that our future shall be as luminous as the sun and as enduring as the Nahasa, so long as we remain united, so long as we are charitable, so long as we welcome even the Jawwal to embrace the Code as their own guiding path in life!"),
                2 => new TextObject("{=EYkmpAva}I preach that we should question the nature of goodness and of wisdom; for a good man would not claim to be wise, and a wise man not claim to be good."),
                _ => new TextObject("{=B9XBmvyy}We are those who adhere to tradition but know also the struggle to move beyond the belligerence of the past - we are no Jawwal, after all.")
            };

            return text;
        }

        public override TextObject GetClergyProveFaith(int rank)
        {
            var text = rank switch
            {
                4 => new TextObject("{=cWg41iew}You come before me, like a wriggling eel slithering to a master archer and asking to learn to wield the bow. I shall speak plainly to you, for my time is short and my station vaunted. When you see that which is evil, you must change it by whatever force you can - if not your hands, then your heart, if not your heart, then your words."),
                3 => new TextObject("{=gToavgYL}A common question among the devout and misguided alike; for how can one truly be assured they walk the righteous path? We know not until the end comes and we are weighed against the deeds of our progenitor; but I shall guide you with what wisdom I can."),
                2 => new TextObject("{=DXwkpjeQ}Consider your actions and why it is you perform them. If you are of influence, do you wield this power for the good of your siblings or do you take it for your own aggrandizement? When your people starve, do you feast? Do you turn to the sword? Is blood spilled for bread gained a worthy trade?"),
                _ => new TextObject("{=u0iwuK22}It does my heart kindly to hear you wish to prove your faith. We live in an age of lip service by most - though I hold no blame to the common man for this; they show their adherence and their faith in their daily labors. If you wish to prove your faith and your adherence to doctrine, you must be a soul who knows peace and who adheres to hierarchy.")
            };

            return text;
        }

        public override TextObject GetClergyProveFaithLast(int rank)
        {
            var text = rank switch
            {
                4 => new TextObject("{=duyNvFEg}Seek to improve the lives of even the least of your people and you shall make kings of them all; and know that should you find yourself poorer and your servants richer that you are a better sibling than many of your blood. You can only hope then that they shall follow your example, follow the Code of Asera, and spread this prosperity to all."),
                3 => new TextObject("{=4zEWWVpe}Foremost in my viewing, as a man of the cities of our lands; is the act of almsgiving. We are as keepers to our brothers, to our sisters and siblings; and only a cruel soul would let the blood of their blood starve.You are only as pure as the lowest among us, and thus you purify yourself of sin when you seek to uplift the whole of the Aserai."),
                2 => new TextObject("{=kM7O8NK3}If you seek to prove your faith, you must internalize the Code of Asera - you must learn and accept that you are but one of his Sons - and you must seek to bring forth a better world for your children to inherit. Prove your faith by making that world. Time will tell if the image in your mind is truly an embodiment of the Code’s doctrines or merely self-indulgence."),
                _ => new TextObject("{=2QKBAsCX}Release your prisoners if you no longer war with their masters; or give them to your master for they shall know better. Choose peace when your enemies desire it; let them lick their wounds and if they strike you again, punish them with biting steel and the shame of their recidivism.")
            };

            return text;
        }

        public override string GetId()
        {
            return "asera";
        }

        public override int GetIdealRank(Settlement settlement)
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

        public override int GetMaxClergyRank()
        {
            return 4;
        }

        public override TextObject GetRankTitle(int rank)
        {
            var text = rank switch
            {
                4 => new TextObject("{=SQq0eH3Z}Murshid"),
                3 => new TextObject("{=LDLhMp3Q}Imam"),
                2 => new TextObject("{=j4Ue5DfM}Akhund"),
                _ => new TextObject("{=ws5kYowZ}Faqir")
            };

            return text;
        }

        public override TextObject GetCultsDescription()
        {
            return new TextObject("{=MBYo3Pjx}Schools");
        }

        public override (bool, TextObject) GetInductionAllowed(Hero hero, int rank)
        {
            bool possible = true;
            TextObject text = new TextObject("{=GAuAoQDG}You will be converted");
            switch (rank)
            {
                case 4:
                    {
                        if (hero.Culture.StringId != "aserai" || hero.Clan.Tier <= 2)
                        {
                            possible = false;
                            text = new TextObject("{=0cGq14Jt}Clan tier is insufficient (less than 3) or not Aserai.");
                        }
                        else if (hero.Clan.Tier <= 4)
                        {
                            possible = false;
                            text = new TextObject("{=PsLzYkAz}Clan tier is insufficient.");
                        }
                        else if (hero.Clan.Tier >= 5)
                        {
                            possible = true;
                        }

                        break;
                    }
                case 3:
                    {
                        if (hero.Culture.StringId == "aserai")
                        {
                            possible = true;
                        }
                        else if (hero.Culture.StringId == "empire" || hero.Culture.StringId == "khuzait")
                        {
                            possible = true;
                        }
                        else if (hero.Spouse != null && hero.Spouse.Culture.StringId == "aserai")
                        {
                            possible = true;
                        } 
                        else
                        {
                            possible = false;
                            text = new TextObject("{=mHrwmbzU}Not part of Aserai, Imperial or Khuzait cultures, or have a spouse part of them");
                        }

                        break;
                    }
                case 2:
                    {
                        if (hero.Culture.StringId == "aserai")
                        {
                            possible = true;
                        }
                        else if (hero.Culture.StringId == "empire" || hero.Culture.StringId == "khuzait")
                        {
                            possible = true;
                        }
                        else
                        {
                            text = new TextObject("{=SLogDwvH}Not part of Aserai, Imperial or Khuzait cultures.");
                        }

                        break;
                    }
                default:
                    {
                        var settlement = Settlement.CurrentSettlement;

                        if (hero.Culture != Utils.Helpers.GetCulture("aserai"))
                        {
                            possible = false;
                            text = new TextObject("{=maHkoBga}Not part of Aserai culture.");
                        }

                        float relation = 0;
                        foreach (var notable in settlement.Notables)
                        {
                            relation += notable.GetRelation(hero);
                        }

                        var medium = relation / settlement.Notables.Count;
                        switch (medium)
                        {
                            case < 0: 
                                {
                                    possible = false;
                                    text = new TextObject("{=DN8yvo7b}Not enough relations with locals (medium of 20).");
                                    break;
                                }
                            case < 20:
                                { 
                                    possible = true;
                                    break;
                                }
                        };

                        break;
                    }
            }

            return new(possible, text);
        }

        public override TextObject GetBlessingAction()
        {
            return new TextObject("{=Wv16hfLw}I would like to study the Code.");
        }

        public override TextObject GetBlessingQuestion()
        {
            return new TextObject("{=MBYo3Pjx}Schools");
        }

        public override TextObject GetBlessingConfirmQuestion()
        {
            return new TextObject("{=2rby15CO}Are you sure?");
        }

        public override TextObject GetBlessingQuickInformation()
        {
            return new TextObject("{=ueDwNTBg}{HERO} is inspired by the teachings of {DIVINITY} code.");
        }

        public override TextObject GetBlessingActionName()
        {
            return new TextObject("{=SwvMtLtQ}study from.");
        }

        public override TextObject GetInductionExplanationText() => new TextObject("{=CrfLUm3F}You must be part of Aserai, Imperial or Khuzait cultures, or have relations with village notables (Faqir), have an Aserai spouse (Imam) or clan tier above 2 (Murshid).");

        public override TextObject GetZealotsGroupName()
        {
            return new TextObject("{=sBGMstSL}Heirs of Asera");
        }
    }
}