using System.Collections.Generic;
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
            }
        }

        public override void Initialize()
        {
            AseraMain = new Divinity("asera-main");
            AseraMain.Initialize(new TextObject("{=7BJOY24H}Asera the Patriarch"),
                new TextObject("{=AzD0sa65}First of his line, the legendary patriarch united the various badw tribes of the Nahasa and the coastal Behr al-Yeshm into the lawful confederacy of the Aserai Sultanate. Asera was deified by his deeds and the codes of law which allowed him to establish dominion and settle his people from the Jabal Tamar to the Jabal Ashab. Asera is that which all followers of his Code seek to live up to; though most followers will accept that the words of Asera are transcribed to benefit the bloodlines which followed him. Thus one can only ever seek to live as Asera did, and to know only their success upon arrival in Paradise."),
                new TextObject("{=!}"));

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
                new TextObject("{=!}"));

            AmraSecondary1 = new Divinity("amra-secondary-1");
            AmraSecondary1.Initialize(new TextObject("{=SgShtGNw}Na Sidhfir"),
                new TextObject("{=oF7u5Z29}Those deemed to have won the favor of the Slaugh Aos’An and the love of the Battanian people for more than a generation may be vaunted into the ranks of the Na Sidhfir - the Immortal Men of the Woods. Occupying a position equally heroic and tragic, the grand figures of the Na Sidhfir are claimed to be tireless and exhausted entities - unable to rest so long as they are remembered, but too self-absorbed to allow their songs to go unsung. Derwyddon practitioners claim the Na Sidhfir possess the bodies of Wolfskins, allowing them to rest and ravage away from the heavenly realms."),
                new TextObject("{=GgVizsVd}Bonus in forest autosimulated battles\nAbility to recruit forest bandit parties under 20 men into your own"),
                new TextObject("{=oWpMWLs8}Ancestor Spirits"));

            AmraSecondary2 = new Divinity("amra-secondary-2");
            AmraSecondary2.Initialize(new TextObject("{=KkYfAdjp}Dymhna Sidset"),
                new TextObject("{=qhXb37NZ}Patient devils, the Dymhna Sidset are the stuff of children’s parables and ill told tales around campfires. They are the spittal on a rabid dog’s lips, the rage of a mother bear seeking a misplaced cub, the cold biting steel that strikes only in betrayal. Though the attempted Calradification of the Uchalion Plateau could not purge this pagan belief set entirely, it did compartmentalize and mangle its body of rituals. Giants, ghosts, and many an unseen shade were changed from beings of tale and legend to “patient devils” by the whims of the Empire. In recent years, some have sought to venerate the Dymhna Sidset; viewing them instead as aspects of rebellion and irredentism."),
                new TextObject("{=fJTNgpOj}Faster raiding of non-Battanian villages\nRenown gain for raiding non-Battanians"),
                new TextObject("{=Lz4WcBZd}Natural Spirits"));


            DarusosianMain = new Divinity("darusosian-main");
            DarusosianMain.Initialize(new TextObject("{=Y9jVBX9n}Martyr Darusos"),
                new TextObject("{=7r3RV3jr}Born in a period of relative internal peace and outward expansion, Darusos was a young emperor who allegedly sought reformations within the Calradic Empire before being betrayed by his closest generals and crucified upon a sacred fig tree in the imperial gardens of Lycaron. Those devoted to Darusos view him as having achieved the rite of the divus in his dying hours, achieving immortality and awaiting those who seek to practice his reforms in the heavenly realms."),
                new TextObject("{=!}"));

            DarusosianSecondary1 = new Divinity("darusosian-secondary-1");
            DarusosianSecondary1.Initialize(new TextObject("{=SW29YLBZ}Imperial Cult"),
                new TextObject("{=XyVMjNNp}Though there has long been an imperial cult in the Calradic Empire, it grew in popularity in the generations after Darusos’s murder. Emperors were deified, made to stand alongside their own gods as peers. During the waning generations of the united empire many would proclaim themselves god-emperor, or other divinely appointed titles; the rite of the divus can only transubstantiate an emperor upon their death. Thus the last truly ordained imperial cult is that which preaches the words of Arenicos Divus; though upstart branches have begun for an inevitable worship of Ira Divus."),
                new TextObject("{=LZt31UBY}Bonus influence\nEvery season, gain renown for each title part of the Southern Empire"),
                new TextObject("{=J4D4X2XJ}Cult"));

            DarusosianSecondary2 = new Divinity("darusosian-secondary-2");
            DarusosianSecondary2.Initialize(new TextObject("{=JqPzw7PR}Lycaronian Triad"),
                new TextObject("{=PONMWMm2}The Empire has long held its own pantheon of divine entities which rule over all aspects of mortal life and which are appeased by means of ritual sacrifice, festival activities, and prayers for absolution. Within the Darusosian Martyrdom, the locally vaunted Lycaronian Triad is held above all other eternal divinities and viewed as adjacent to mortal emperors risen to divinity by the rites of the divus. Iovis, the Sky-Father reigns as the henotheistic patriarch who traditionally dwells upon Mount Erithrys. He is accompanied by Astaronia, his bride who represents that which must be protected by the machinations of imperial might; and by his daughter Mesnona who was born from the ego of Iovis and who grants insight to mortal petitioners."),
                new TextObject("{=hWvSm3Zz}Settlement stability increased\nParty morale bonus"),
                new TextObject("{=iE8OCyCv}Gods"));

            VlandiaMain = new Divinity("vlandia-main");
            VlandiaMain.Initialize(new TextObject("{=ePJb0qTR}Lai Vlandia"),
                new TextObject("{=2hEMRmtZ}The Canticles sung in stanza and deed within the Lai Vlandia speak to the grand narrative of the modern age; couplets detailing tales of adventure and romance, of peasant heroes and baronial lords. All modern men are viewed as participants within the Lai Vlandia; regardless of their wishes, so long as they know that Vlandia exists. The songs are often written in such a way that one may make known their beliefs and views on the subject by means of intonation; whilst the tomes depicting the tales are renown for their strange marginalia which often descend into comic flourishes where lesser scriveners depict their favored and reviled characters as strange creatures or grotesque beasts."),
                new TextObject("{=!}"));

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
        }
    }
}