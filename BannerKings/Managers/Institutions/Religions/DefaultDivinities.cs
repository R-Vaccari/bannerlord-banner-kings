using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions
{
    public class DefaultDivinities : DefaultTypeInitializer<DefaultDivinities, Divinity>
    {
        public Divinity AseraMain { get; private set; }
        public Divinity AseraSecondary1 { get; private set; }
        public Divinity AseraSecondary2 { get; private set; }
        public Divinity AseraSecondary3 { get; private set; }
        public Divinity AmraMain { get; private set; }
        public Divinity AmraSecondary1 { get; private set; }
        public Divinity AmraSecondary2 { get; private set; }
        public Divinity DarusosianMain { get; private set; }
        public Divinity DarusosianSecondary1 { get; private set; }
        public Divinity DarusosianSecondary2 { get; private set; }
        public Divinity VlandiaMain { get; private set; }
        public Divinity VlandiaSecondary1 { get; private set; }
        public Divinity VlandiaSecondary2 { get; private set; }
        public Divinity TreeloreMain { get; private set; } = new Divinity("treelore_main");
        public Divinity Hirvi { get; private set; } = new Divinity("Hirvi");
        public Divinity Mehns { get; private set; } = new Divinity("treelore_moon");
        public Divinity Osric { get; } = new Divinity("Osric");
        public Divinity Wilund { get; } = new Divinity("Wilund");
        public Divinity Oca { get; } = new Divinity("Oca");
        public Divinity Horsa { get; } = new Divinity("Horsa");
        public Divinity Grunwald { get; } = new Divinity("Grunwald");
        public Divinity WindHeaven { get; } = new Divinity("WindHeaven");
        public Divinity WindNorth { get; } = new Divinity("WindNorth");
        public Divinity WindSouth { get; } = new Divinity("WindSouth");
        public Divinity WindEast { get; } = new Divinity("WindEast");
        public Divinity WindWest { get; } = new Divinity("WindWest");
        public Divinity WindHell { get; } = new Divinity("WindHell");
        public Divinity Iltanlar { get; } = new Divinity("Iltanlar");
        public Divinity SheWolf { get; } = new Divinity("SheWolf");
        public Divinity GodsFate { get; } = new Divinity("GodsFate");
        public Divinity Gundar { get; } = new Divinity("Gundar");
        public Divinity Arkina { get; } = new Divinity("Arkina");

        public override IEnumerable<Divinity> All
        {
            get
            {
                yield return AseraMain;
                yield return AseraSecondary1;
                yield return AseraSecondary2;
                yield return AseraSecondary3;
                yield return AmraMain;
                yield return AmraSecondary1;
                yield return AmraSecondary2;
                yield return DarusosianMain;
                yield return DarusosianSecondary1;
                yield return DarusosianSecondary2;
                yield return VlandiaMain;
                yield return VlandiaSecondary1;
                yield return VlandiaSecondary2;
                yield return TreeloreMain;
                yield return Hirvi;
                yield return Mehns;
                yield return Wilund;
                yield return Osric;
                yield return Horsa;
                yield return Oca;
                yield return WindEast;
                yield return WindWest;
                yield return WindHell;        
                yield return WindNorth;
                yield return WindSouth;
                yield return WindHeaven;
                yield return SheWolf;
                yield return Iltanlar;
                yield return GodsFate;
                yield return Gundar;
                yield return Arkina;
                //yield return Grunwald;
                foreach (Divinity item in ModAdditions)
                {
                    yield return item;
                }
            }
        }

        public override void Initialize()
        {
            GodsFate.Initialize(new TextObject("{=NSJgGXr2}Gods of Fate"),
              new TextObject("{=2mLAXyYN}According to the Junme, the world revolves around the concept of Urthr - or in other words, fate. The gods are stewards of the Great Oak that mankind inhabits, keeping its roots and leaves from rotting. Moreover, the gods are the force of justice that fights Ásbani, the God-Devouring Serpent. The gods of fate, they say, give man what is just - rewards to those who are brave and virtuous, and punishment to the wicked, for they do not weave what choices a man makes, but they do the consequences."),
              new TextObject("{=zIEaUmN2}Signicantly increased renown and influence from battles"),
              new TextObject("{=y9QDFWvi}Gods"));

            Gundar.Initialize(new TextObject("{=866PUEks}Gundar"),
               new TextObject("{=0sXzSFmJ}According to the Junme, the world revolves around the concept of Urthr - or in other words, fate. The gods are stewards of the Great Oak that mankind inhabits, keeping its roots and leaves from rotting. Moreover, the gods are the force of justice that fights Ásbani, the God-Devouring Serpent. The gods of fate, they say, give man what is just - rewards to those who are brave and virtuous, and punishment to the wicked, for they do not weave what choices a man makes, but they do the consequences."),
               new TextObject("{=YKydsTv6}"),
               new TextObject("{=b5hM3VcQ}Ancestor Hero"));

            Arkina.Initialize(new TextObject("{=nMw1tIcU}Arkina"),
               new TextObject("{=46o3GYZ9}According to the Junme, the world revolves around the concept of Urthr - or in other words, fate. The gods are stewards of the Great Oak that mankind inhabits, keeping its roots and leaves from rotting. Moreover, the gods are the force of justice that fights Ásbani, the God-Devouring Serpent. The gods of fate, they say, give man what is just - rewards to those who are brave and virtuous, and punishment to the wicked, for they do not weave what choices a man makes, but they do the consequences."),
               new TextObject("{=AEYKYyM2}"),
               new TextObject("{=zvur5q5p}Ancestor Heroine"));

            SheWolf.Initialize(new TextObject("{=kEmNLu7E}Great She-Wolf"),
                new TextObject("{=2HLzoMod}Many among the eastern Devseg, such as the Khuzaits, count themselves among the descendants of the Great She-Wolf. The She-Wolf, it is said, nursed a wounded young boy back to health, who later impregnated her. 12 sons she had, half-wolf, half-man. To be counted as a descendent of the She-wolf is a great honor."),
                new TextObject("{=BYEgOs3m}Increased personal and spouse fertility\nNewborn sons have at least one positive congenital trait"),
                new TextObject("{=XwtY10mI}Deity"),
                300,
                new TextObject("{=2rvylJIy}"),
                new TextObject("{=IqoqdfgQ}"));

            Iltanlar.Initialize(new TextObject("{=tGJxwXlZ}Iltanlar"),
                new TextObject("{=tQ0CUokx}Iltanlar is the god of a mountain-dwelling Devseg group, who migrated from the Sulun Dag mountain range to mount Iltan, and have since been assimilated by the eastern hordes of their cousins. The Iltanlar people value security above most things, thus they have for long dwelled in the mountains, which they perceive to be bountiful in good, protective spirits. Iltanlar, the god, is said to be an ancient hero, who guided his people to their new promised home, where since the Iltanlar people have founded Baltakhand."),
                new TextObject("{=alRsFvuS}Improved Devseg town prosperity\nIncreased troop morale in defensive battles"),
                new TextObject("{=iIxciSmD}Mountain God"),
                300,
                new TextObject("{=YLBqdkgo}"),
                new TextObject("{=zoQnkc6l}"),
                Settlement.All.First(x => x.StringId == "town_K1"));

            WindHeaven.Initialize(new TextObject("{=zgsOhEk0}Yel Uçmag"),
                new TextObject("{=2uIr6SrV}Yel Uçmag, the God of Heaven, also known as the first wind. He is the guardian of Heaven, Uçmag, where the souls of the rightous dwell. For this he is the most hollowed of the Devseg deities, for he hosts the souls of their ancestors, who live in a world of perfect, uncorrupted nature and tradition. He is said to wear armor and weapons of silver, as pure as the heavenly realm he guards."),
                new TextObject("{=CHqvH5GN}Improved settlement stability\nReduced religious tensions"),
                new TextObject("{=wB8uhK2p}Heavenly Wind"),
                200,
                new TextObject("{=6Kk9zqj4}"),
                new TextObject("{=2J38reHp}"));

            WindHell.Initialize(new TextObject("{=Q6sDIhsO}Yel Tamag"),
                new TextObject("{=UFissAlh}The opposite of Yel Uçmag, Yel Tamag is the guarding of the Underworld, a fiery realm where the unvirtuous are punished. Though one may at first associate him with evil, the Devseg see Yel Tamag as a god of Justice. He does not chose who are to be punished, but only how they will be punished, according to their wrongdoings. The Underworld, Tamag, is where the souls may learn and eventually ascend to Uçmag."),
                new TextObject("{=lXd6B6U8}Improved settlement stability"),
                new TextObject("{=qw6Jtkr8}Hellish Wind"),
                300,
                new TextObject("{=3cUOR8yD}"),
                new TextObject("{=9DPtfWO9}"));

            WindNorth.Initialize(new TextObject("{=hfe6A2fr}Yel Kurmag"),
                new TextObject("{=sXVkAm0Z}The Moon god, also know as Cold or Northern Wind. Yel Kurmag is the lord of the the bleak and cold nights, whese diseases and demons thrive. The god stands as a protector for his faithful, and so the Devseg have great respect for him, lest have malicious spirits or blights take them. It is believed he also presides over the fertility of women, as both the time of birth and pregnancy duration seem related to the Moon."),
                new TextObject("{=3IddMjMm}Piety gain for every child born\nProtection against diseases"),
                new TextObject("{=JIxWuMEl}Northern Wind"),
                300,
                new TextObject("{=vSJGIs3Q}"),
                new TextObject("{=kmYeTu9w}"));

            WindSouth.Initialize(new TextObject("{=Mj79BBMO}Yel Kunmag"),
               new TextObject("{=E3TNDN3W}Yel Kunmag is the god of the Solar Wind, or Southern Wind as the non-Devseg would call him. To these, the Land of the Sun lies in the south, where Yel Kunmag flows to. The Sun is an immensely powerful force in the Devseg cosmos, and as such Yel Kunmag is bestowed great influence. Yel Kunmag's investure is seen as a royal prerogative, for the Sun is capable of both raising and destroying Khaganates."),
               new TextObject("{=sxnU0sRJ}Increased influence cap\nImproved ruler legitimacy"),
               new TextObject("{=FEer8lxE}Southern Wind"),
               300,
               new TextObject("{=WGUKdHEP}"),
               new TextObject("{=CVanem5N}"));

            WindWest.Initialize(new TextObject("{=kk5Mk36Q}Yel Batmag"),
                new TextObject("{=ZIqaFPjX}The Western Wind, the harbinger of death. Yel Batmag is in direct opposition to Yel Togmag. He grants death to whom he touches, incuding the Sun, who is again bequeated life by Yel Togmag, in an unending cycle. Death from Yel Batmag, however, is not perceived as sorrowful, or painful, but simply the natural and necessary part of the mundane existance, which the god presides over, as opposed to yield for his ambitions. Devseg shamans, the Kams, were the first to oppose the hordes marching westwards, seeing it as a death sentence."),
                new TextObject("{=52FSBJL8}Protection against curses\nSignificant extra renown from battles"),
                new TextObject("{=gPbo8BHu}Western Wind"),
                300,
                new TextObject("{=wmJQ5quJ}"),
                new TextObject("{=gQlE11FP}"));

            WindEast.Initialize(new TextObject("{=cQgPSwOX}Yel Togmag"),
                new TextObject("{=hio2f86z}Yel Togmag, or the Eastern Wind, is the Devseg Godess that presides over life. Tog, to be born, is her domain, life she bestows the Sun, daugther of Yel Uçmag. Many faithul, during their prayers, turn towards the East in her honor. The Devseg welcome the Eastern Wind as a blessing of fertility and prosperity - a good omen, by all standards."),
                new TextObject("{=p5BmPnab}Increased village animal and agricultural productions\nIncreased village prosperity"),
                new TextObject("{=ZfsXSTva}Eastern Wind"),
                300,
                new TextObject("{=JER19TKx}"),
                new TextObject("{=N04KfW9a}"));

            Wilund.Initialize(new TextObject("{=k0lyiU6c}Wilund"),
                new TextObject("{=h51qIT4c}Wilund is known to non-Vlandians as the first Vlandic warlord to come to the continent. Yet the Vlandic tales tell the story of not a mercenary warlord, but a celestial smith, whose foundry is the world. The stories of how kingdoms rise and fall - that is the craft of Wilund. As such, hestawicks will assure he came to Calradia to make way for the Vlandians to carve their kingdom, or perhaps, kingdoms..."),
                new TextObject("{=1pZ2KLs7}Signicantly increased renown and influence from battles"),
                new TextObject("{=LiH5kRoA}Smith-God"),
                200,
                new TextObject("{=IRxnIJ7K}Wilund is the celestial smith. The world is his foundry. The thunder of the storm and the thunder of hooves are the song of his hammer. The black rain of a thousand arrows of war are the sparks upon his anvil. When he tempers life, a thousand men die and a thousand more are born."),
                new TextObject("{=XNz8ZtFb}We, the Wilunding, owe him a great debt. The Smith came onto this land so that we may forge it into our kingdom. We shall repay him with a thousand years more of conquest."));

            Osric.Initialize(new TextObject("{=m7LSz9jE}Osric Iron-Arm"),
                new TextObject("{=3my7LVkm}Deemed to be the first Vlandic king in Calradia, Osric is responsible for the conquest of Pravend, formerly an imperial capital. While they see it as a trivial game of conquest from a petty tribal warlord, Osric's achievements, in the minds of the Wilunding, prove the prophecy that they are, indeed, promised this fertile land for their taking. Osric is said to have killed the Calradic gods so that the Wilunding instead prosper, and so many among these now call themselves Osrickin."),
                new TextObject("{=NO17q5hW}On capturing a fief, immediatly receive a claim for it"),
                new TextObject("{=dzu0LHfA}Sun-God"),
                300,
                new TextObject("{=UA55pZEo}Osric, as he is known in this land, came to us, the Wilunding, to fulfill the prophecy of Horsa, which we so dearly desired. As vengeance for burning the silk and wine laden homes of the Calradoi, Osric's golden shield, the Sun, was struck and removed from him. With the bones of his lost arm, he promised us a new city. And so it was that Wilund forged him an arm of dark iron and said unto him “If they will not love thee when thou canst not carry gold, then I shall give thee iron to cast a shadow upon the world. And the shadow shall be called conquest.”"),
                new TextObject("{=nFvL5ren}Osric gathered great host of lances and bolts, with which he turned the sky above Paravenos dark. It was the dawn of the winter solstice. The imperials were terrified and opened the gates to flee the city. Then the arrows ceased and the bright sun shone once more, blinding the defenders. Seizing their chance, the good men rode into the city rallying under Osrac’s holy name and cut down the Calradoi in the streets. And so it is said to this day that the empire paid in red blood what they would not pay in black soil."),
                Settlement.All.First(x => x.StringId == "town_V3"));

            Horsa.Initialize(new TextObject("{=bP7IMDYm}Horsa"),
                new TextObject("{=Ua7camfx}The warrior Horse-God, Horsa landed near the lordship of Horsger and planted his spear in the beach. He has since become a prophet-like figure to the Vlandic peoples, foretelling the settlement and rise of Vlandic kingdoms. Though man and warrior, he is also depicted as a horse, a Vlandic symbol of both prosperity through the plough and war though the lance."),
                new TextObject("{=xEM9Xidv}Increased village prosperity\nImproved offensive battle morale"),
                new TextObject("{=3mKjnpfV}Horse-God"),
                300,
                new TextObject("{=j0rhPqSe}When the first of us Wilundings came to Calradia from west-over-sea, we rode inland from Ostican and saw how it was good and green. We buried our weapons to Horsa, the horse god, and asked him to let us leave the life of war behind us, and to turn the saddle to a yoke and set the horse to plough. Yet, the gods of stone that dwell in this land would not let us live in frith. Regarldess, Horsa took our pledge and called more of us from over the sea, and so we came riding."),
                new TextObject("{=CET7oBw9}When we came across the sea, Horsa said unto us: “Ye have buried your spears to me in this soil, and they shall grow again like crops whenever ye need them. And your harvest shall be war, and your bread shall be conquest.” On Horsa's name, we plant our seeds in the fields and our spears into the flesh of our foes. From battle flows the red wellspring that blesses this land. It makes the crops to grow and feeds both gods and men."),
                Settlement.All.First(x => x.StringId == "village_V8_1"));

            Oca.Initialize(new TextObject("{=E4qdoSbS}Oca"),
                new TextObject("{=x7Ct0KBP}Oca, a god favored by the Swedaz, is often depicted as an Ox. He is believed to have the power to provide his faithful with bountiful crop yields, who were traditionally ploughed with oxen teams. Yet, it does not mean he is harmless, or to be trifled with - Oca may slaughter his enemies, including those that do not keep their oaths to him. His cult is strongest in eastern Vlandia, around Ocs Hall, named in his honor, but also a land of many legends."),
                new TextObject("{=FVX4JxEJ}Improved town prosperity\nIncreased farmland acreage output"),
                new TextObject("{=vpyfZpWq}Ox-God"),
                300,
                new TextObject("{=CERlaejw}When we came from west-over-sea, we landed first near Ostican, built the shrine to Horsa, the horse god, and then we followed the shining shield of Osric. As we rode east we saw great mountains of stone shadowing the morning sun. And so it has ever been in Vlandia. The shadows of dawn are long and cold. ut there was one farmer, some say he was a Massa, others that he was the son of a Nord. Most of us count him as a snake god, but that is another tale. His name was Orm. The Battanians told Orm about a towering giant of stone, the one that was sleeping in the way of his sunlight. His name was Mordus. They said that if he got the giant drunk on wine, he’d be easier to knock down. Mordus was one of the Calradians’ Titans of Stone. He was the bastard of a Battanian god. He made his home on the mountainside in the east near the lake of Llyn."),
                new TextObject("{=K09tBz5U}Orm prayed to the god Oca. He said, “Oca, god of the oxen, plough away for me a sleeping giant. And I will give you all the wine you can drink in summer.” Now Orm went to Mordus and offered him all the wine he could drink if only he would spare his oxen. So Mordus watched Orm yoke a fine ox and plough slowly south towards him. And he drank. The sun was setting over the western sea. He drank so much that he didn’t see the ox was ploughing away the mountainside, by the time the ox came close, Mordus was asleep. And so Oca picked up the pace as night fell and charged right at the sleeping Mordus and cut him to pieces. In the morning light, Orm was horrified. There stood Oca, a giant ox, drinking wine from Mordus’ corpse. It had spilled into the furrow made the day before. Orm shouted at Oca: “I asked you to plough away the body of Mordus but here you are drinking up my wine.”, to which the go replied: “No. This is the wine thou hast promised me. But thou gavest it to Mordus. Now I plough no more. Every summer I will come back and drink this wine, and thou shalt have nothing more from the gods. Instead, beest happy with what thou havest. Learnest to pay those who work for thee.”"),
                Settlement.All.First(x => x.StringId == "town_V2"));

            Grunwald.Initialize(new TextObject("{=scfgBd4l}Kronvalt"),
                new TextObject("{=9C6z4h0I}D"),
                new TextObject("{=D1pfyBVq}Improved village prosperity\nIncreased farmland acreage output"),
                new TextObject("{=AB9h5bNE}"),
                300,
                new TextObject("{=AIpMH2cI}The Calradoi call him Koronvaldos, but the god of the Rhodoks is called Kronvalt in Vlandic. The name means “king-chooser”. "),
                new TextObject("{=Xc1h2BII}"));

            AseraMain = new Divinity("asera-main");
            AseraMain.Initialize(new TextObject("{=7BJOY24H}Asera the Patriarch"),
                new TextObject("{=AzD0sa65}First of his line, the legendary patriarch united the various badw tribes of the Nahasa and the coastal Behr al-Yeshm into the lawful confederacy of the Aserai Sultanate. Asera was deified by his deeds and the codes of law which allowed him to establish dominion and settle his people from the Jabal Tamar to the Jabal Ashab. Asera is that which all followers of his Code seek to live up to; though most followers will accept that the words of Asera are transcribed to benefit the bloodlines which followed him. Thus one can only ever seek to live as Asera did, and to know only their success upon arrival in Paradise."),
                new TextObject("{=Z0uU7Tn6}Greater chance of being awarded fiefs during elections"),
                new TextObject("{=aw6BtMa7}Patriarch"),
                200);

            AseraSecondary1 = new Divinity("asera-secondary-1");
            AseraSecondary1.Initialize(new TextObject("{=hVrkPEqy}Damma-Siddiq"),
                new TextObject("{=t5Tahj9q}Descended from those who dedicated themselves to the Code of Asera but who were married into the true bloodlines of Asera’s descendents are known as the Damma-Siddiq; those of truthful blood. It is through their reforms, compromises, and bold rhetoric that marriage may allow even those born outside the Sultanate to come to be viewed as being of the blood of Asera - albeit through legalism and spirit."),
                new TextObject("{=Nhceg9ks}Daily chance of relation gain with a random Aserai Banu\nReduced influence cost for supporting decisions"),
                new TextObject("{=MBYo3Pjx}School"));

            AseraSecondary2 = new Divinity("asera-secondary-2");
            AseraSecondary2.Initialize(new TextObject("{=ROhyHjG4}Ibn-Zakaa"),
                new TextObject("{=SJzGcJt2}Only the direct line of Asera’s sons can claim to be Ibn-Zakaa; to be born a Pure Son. Such claims have led to kinstrife and civil war in the past, with daughters being viewed as a dead end to a pure line, and many a ‘lesser son’ made eunuch as means of societal control. In the modern age, the Ibn-Zakaa are far more enlightened, viewing themselves not as deserving of a divinely appointed respect but rather in the light of those who have much to prove to be worthy in the eyes of their progenitor."),
                new TextObject("{=PA5PCfxE}Daily renown for running a party of Aserai only\nAserai troops cheaper to recruit"),
                new TextObject("{=MBYo3Pjx}School"));

            AseraSecondary3 = new Divinity("asera-secondary-3");
            AseraSecondary3.Initialize(new TextObject("{=VVfdjejb}Rashuqqalih"),
                new TextObject("{=0XorBTBT}The schools of philosophy preached by the Rashuqqalih practitioners of the Code, are concerned foremost with matters of righteousness and societal elegance. To be of the blood of Asera is to be bound to the morals of a mortal man made legendary, and thus matters of failure and mortal flaws must be treated with compassion. The most famous practitioner of the Rashuqqalih school was Queen Eshora, who brought several nomadic badw tribes into the Sultanate by accepting their adherence to tradition as a thing to be celebrated, rather than as a matter to consider them a distasteful other."),
                new TextObject("{=SYDP3fEe}Increased faith presence in foreign settlements\nProsperity for date and camel producing villages"),
                new TextObject("{=MBYo3Pjx}School"));

            AmraMain = new Divinity("amra-main");
            AmraMain.Initialize(new TextObject("{=v3UAh1rJ}Sluagh Aos’An"),
                new TextObject("{=zYHZu2OC}Constituting the major heavenly divine of the Battanians are those known as the Slaugh Aos’An - the Host of Noble Folk who reign between darkened clouds and watch over humanity with starlight torches. Seldom petitioned, as they are viewed as capricious entities; the Slaugh Aos’An are said to visit Battania during the changing of the seasons and to witness the birth of those ordained by fate to bring about weal and doom to the land. To make an oath under the auspices of the Slaugh Aos’An is to be bound to the letter or the spirit of one’s words; never more and never both. To break such an oath is to invite all of fate to conspire towards your end, and to know no peace in Heaven nor Hell."),
                new TextObject("{=iNvtNtoK}Increased prosperity of cattle, hog and forestry villages"),
                new TextObject("{=iYqD0kQM}Great Spirits"),
                200,
                new TextObject("{=gabdBLx4}Test 1"));

            AmraSecondary1 = new Divinity("amra-secondary-1");
            AmraSecondary1.Initialize(new TextObject("{=SgShtGNw}Na Sidhfir"),
                new TextObject("{=oF7u5Z29}Those deemed to have won the favor of the Slaugh Aos’An and the love of the Battanian people for more than a generation may be vaunted into the ranks of the Na Sidhfir - the Immortal Men of the Woods. Occupying a position equally heroic and tragic, the grand figures of the Na Sidhfir are claimed to be tireless and exhausted entities - unable to rest so long as they are remembered, but too self-absorbed to allow their songs to go unsung. Derwyddon practitioners claim the Na Sidhfir possess the bodies of Wolfskins, allowing them to rest and ravage away from the heavenly realms."),
                new TextObject("{=GgVizsVd}Bonus in forest autosimulated battles\nAbility to recruit forest bandit parties under 20 men into your own"),
                new TextObject("{=oWpMWLs8}Ancestor Spirits"),
                300,
                new TextObject("{=0JxYlEBi}Test 2"));

            AmraSecondary2 = new Divinity("amra-secondary-2");
            AmraSecondary2.Initialize(new TextObject("{=KkYfAdjp}Dymhna Sidset"),
                new TextObject("{=qhXb37NZ}Patient devils, the Dymhna Sidset are the stuff of children’s parables and ill told tales around campfires. They are the spittal on a rabid dog’s lips, the rage of a mother bear seeking a misplaced cub, the cold biting steel that strikes only in betrayal. Though the attempted Calradification of the Uchalion Plateau could not purge this pagan belief set entirely, it did compartmentalize and mangle its body of rituals. Giants, ghosts, and many an unseen shade were changed from beings of tale and legend to “patient devils” by the whims of the Empire. In recent years, some have sought to venerate the Dymhna Sidset; viewing them instead as aspects of rebellion and irredentism."),
                new TextObject("{=fJTNgpOj}Faster raiding of non-Battanian villages\nRenown gain for raiding non-Battanians"),
                new TextObject("{=Lz4WcBZd}Natural Spirits"));

            DarusosianMain = new Divinity("darusosian-main");
            DarusosianMain.Initialize(new TextObject("{=Y9jVBX9n}Martyr Darusos"),
                new TextObject("{=7r3RV3jr}Born in a period of relative internal peace and outward expansion, Darusos was a young emperor who allegedly sought reformations within the Calradic Empire before being betrayed by his closest generals and crucified upon a sacred fig tree in the imperial gardens of Lycaron. Those devoted to Darusos view him as having achieved the rite of the divus in his dying hours, achieving immortality and awaiting those who seek to practice his reforms in the heavenly realms."),
                new TextObject("{=4cnju8xP}Improved settlement cultural assimilation\nReduced costs for convincing vassals to assume culture or faith"),
                new TextObject("{=ouvoa3Y4}Emperor Martyr"),
                200);

            DarusosianSecondary1 = new Divinity("darusosian-secondary-1");
            DarusosianSecondary1.Initialize(new TextObject("{=SW29YLBZ}Imperial Cult"),
                new TextObject("{=XyVMjNNp}Though there has long been an imperial cult in the Calradic Empire, it grew in popularity in the generations after Darusos’s murder. Emperors were deified, made to stand alongside their own gods as peers. During the waning generations of the united empire many would proclaim themselves god-emperor, or other divinely appointed titles; the rite of the divus can only transubstantiate an emperor upon their death. Thus the last truly ordained imperial cult is that which preaches the words of Arenicos Divus; though upstart branches have begun for an inevitable worship of Ira Divus."),
                new TextObject("{=LZt31UBY}Bonus influence\nEvery season, gain renown for each title part of the Southern Empire"),
                new TextObject("{=J4D4X2XJ}Cult"));

            DarusosianSecondary2 = new Divinity("darusosian-secondary-2");
            DarusosianSecondary2.Initialize(new TextObject("{=JqPzw7PR}Lycaronian Triad"),
                new TextObject("{=PONMWMm2}The Empire has long held its own pantheon of divine entities which rule over all aspects of mortal life and which are appeased by means of ritual sacrifice, festival activities, and prayers for absolution. Within the Darusosian Martyrdom, the locally vaunted Lycaronian Triad is held above all other eternal divinities and viewed as adjacent to mortal emperors risen to divinity by the rites of the divus. Iovis, the Sky-Father reigns as the henotheistic patriarch who traditionally dwells upon Mount Erithrys. He is accompanied by Astaronia, his bride who represents that which must be protected by the machinations of imperial might; and by his daughter Mesnona who was born from the ego of Iovis and who grants insight to mortal petitioners."),
                new TextObject("{=hWvSm3Zz}Settlement stability increased\nParty morale bonus"),
                new TextObject("{=iE8OCyCv}Gods"),
                300,
                null,
                null,
                Settlement.All.First(x => x.StringId == "town_ES4"));

            VlandiaMain = new Divinity("vlandia-main");
            VlandiaMain.Initialize(new TextObject("{=ePJb0qTR}Lai Vlandia"),
                new TextObject("{=2hEMRmtZ}The Canticles sung in stanza and deed within the Lai Vlandia speak to the grand narrative of the modern age; couplets detailing tales of adventure and romance, of peasant heroes and baronial lords. All modern men are viewed as participants within the Lai Vlandia; regardless of their wishes, so long as they know that Vlandia exists. The songs are often written in such a way that one may make known their beliefs and views on the subject by means of intonation; whilst the tomes depicting the tales are renown for their strange marginalia which often descend into comic flourishes where lesser scriveners depict their favored and reviled characters as strange creatures or grotesque beasts."),
                new TextObject("{=XzD669be}Extra influence as battle reward\nGain extra relations from victories with those that share faith"),
                new TextObject("{=7rdxBfJi}Great Saga"),
                200);

            VlandiaSecondary1 = new Divinity("vlandia-secondary-1");
            VlandiaSecondary1.Initialize(new TextObject("{=iD6W9AS7}Ribaldi Cant"),
                new TextObject("{=TLVS4KGV}Those who buck against the perceived hegemony of the Lai Vlandia, be they peasant rebels or upstart lords, will claim piety by way of the Ribaldi Cant. Though traditionalists will note the Cant is but a deviation of the illuminated manuscripts and oral tales of the pre-Calradic Vlandian culture, modern adherents claim the Cant to be the methodology of rebels, wanderers, rascals, and conquerors. Subtlety is seldom found, and parables and morality plays are all too commonly twisted to back the ideology of the teller. As such the works depicted therein are often viewed as satire or crude witticisms, but few cloisters will deny their worth. They are part of the Lai Vlandia, and thus the Canticles, regardless of their desires. "),
                new TextObject("{=aP0f2WJp}Bonus influence\nIncreased faith presence in settlements"),
                new TextObject("{=neVhyybi}Saga"));

            VlandiaSecondary2 = new Divinity("vlandia-secondary-2");
            VlandiaSecondary2.Initialize(new TextObject("{=45TyGZm6}Meridional Cantigan"),
                new TextObject("{=Wip91rve}The vitriolic and zealous among the modern Vlandians have as of late begun singing the Meridional Cantigan and marching towards the Southlands in a belligerent crusade. Claiming to be a reprisal of a song sung before the days of Osrac Iron-Arm, these goliard warrior-poets speak the harsh rhymes of their ancient conqueror patriarchs and wayward followers. They seek truth in deed, to be worthy of a completed song for an era; to serve in harmony with the Canticles, the Lai Vlandia, and to be remembered in illuminations befitting those enshrined in legend."),
                new TextObject("{=uyhPvxs2}Occasionally receive zealot warriors\nParty morale bonus for vlandian troops"),
                new TextObject("{=neVhyybi}Saga"));

            TreeloreMain.Initialize(new TextObject("{=jWspS9EP}Pérkken, Thunder Wielder"),
                new TextObject("{=5mTA1dhJ}Once, there was naught but the Great Oak, the sea and sky.."),
                new TextObject("{=34gTkB7U}Stability for all settlements of acceptable cultures\nRenown gain for every successful raid on foreign villages"),
                new TextObject("{=z0VYqrO5}Supreme God"));

            Hirvi.Initialize(new TextObject("{=jsa5NhVS}Suurihirvi"),
                new TextObject("{=GiUDpppu}Vakken shamans tell the story of Suurihirvi - king of the forest. Naturally, the Vakken believe the forests to be ripe with spirits, but some are greater than others, and Suurihirvi is the greatest of all. Hunters pray to him for bountiful game, and foresters for protection. In most stories, Suurihirvi is depicted as a great white elk, for it needs not to hide as the forest is its domain."),
                new TextObject("{=4aOCSfYo}Forester and fur villages increased productivity\nWoodland acreage food production greatly increased"),
                new TextObject("{=sK0LTjkU}Forest Spirit"));

            Mehns.Initialize(new TextObject("{=AJ3sq4c9}Méhns Cult"),
                new TextObject("{=tXp4wuPZ}Among the children of the forest, many devouts, specially in the Chertyg region, have devoted themselves to Méhns. The Moon, in their understanding, is the source of prosperity for both land and mankind, and thus the Goddess gained popularity among believers. Although not a warrior as Pérkos, she is also believed to a protector of the children, in a motherly manner, as Méhns is solely responsible for keeping darkness away during nighttime."),
                new TextObject("{=Vn3bTO6r}Prosperity for all villages of acceptable cultures\nIncreased fertility"),
                new TextObject("{=foD5TnsR}Goddess"));
        }
    }
}