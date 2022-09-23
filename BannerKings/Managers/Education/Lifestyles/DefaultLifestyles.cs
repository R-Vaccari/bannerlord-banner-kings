using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Education.Lifestyles
{
    public class DefaultLifestyles : DefaultTypeInitializer<DefaultLifestyles, Lifestyle>
    {
        public Lifestyle Kheshig { get; private set; }

        public Lifestyle Varyag { get; private set; }

        public Lifestyle Fian { get; private set; }

        public Lifestyle Diplomat { get; private set; }

        public Lifestyle August { get; private set; }

        public Lifestyle Cataphract { get; private set; }

        public Lifestyle SiegeEngineer { get; private set; }

        public Lifestyle CivilAdministrator { get; private set; }

        public Lifestyle Outlaw { get; private set; }

        public Lifestyle Caravaneer { get; private set; }

        public Lifestyle Mercenary { get; private set; }

        public Lifestyle Artisan { get; private set; }

        public Lifestyle Gladiator { get; private set; }

        public Lifestyle Ritter { get; private set; }

        public Lifestyle Jawwal { get; private set; }

        public Lifestyle Warlord { get; private set; }

        public override IEnumerable<Lifestyle> All
        {
            get
            {
                yield return Cataphract;
                yield return Ritter;
                yield return Fian;
                yield return Varyag;
                yield return Kheshig;
                yield return Jawwal;
                yield return Mercenary;
                yield return Outlaw;
                yield return Gladiator;
                yield return Caravaneer;
                yield return August;
                yield return SiegeEngineer;
                yield return CivilAdministrator;
                yield return Warlord;
            }
        }

        public override void Initialize()
        {
            var cultures = Game.Current.ObjectManager.GetObjectTypeList<CultureObject>();

            Warlord = new Lifestyle("lifestyle_warlod");

            Warlord.Initialize(new TextObject("{=of43diPd}Warlord"),
                new TextObject("{=!}Commander/General Lifestyle"),
                DefaultSkills.Tactics,
                DefaultSkills.Leadership,
                new List<PerkObject>
                {
                    BKPerks.Instance.WarlordCaptain,
                    BKPerks.Instance.WarlordCommander,
                    BKPerks.Instance.WarlordGeneral
                },
                new TextObject("{=tRp08jyH}..."),
                1.5f,
                25f);


            Fian = new Lifestyle("lifestyle_fian");
            Fian.Initialize(new TextObject("{=of43diPd}Fian"), 
                new TextObject("{=!}Fians are the epitome of Battanian combat. Long ago, their ancestors discovered that ambushes in the woods with longbows were specially deadly. But if the bow was not enough, a greatsword would do the rest. Ever since, the mastery of longbow and greatswords has been the staple for Battanian nobility, who stills despises the horse as a sign of status."), 
                DefaultSkills.Bow,
                DefaultSkills.TwoHanded,
                new List<PerkObject>
                {
                    BKPerks.Instance.FianHighlander,
                    BKPerks.Instance.FianRanger, 
                    BKPerks.Instance.FianFennid
                },
                new TextObject("{=tRp08jyH}Battanian settlements have +{EFFECT1} militia\nReduced damage by {EFFECT2}% when mounted"),
                1.5f, 
                25f,
                null,
                cultures.FirstOrDefault(x => x.StringId == "battania"));

            Cataphract = new Lifestyle("lifestyle_cataphract");
            Cataphract.Initialize(new TextObject("{=RWrMBAbv}Cataphract"), 
                new TextObject("{=!}Developed as a tactical necessity, Imperial cataphracts have become the symbol of the Imperial cavalry. Other than smashing through infantry formations, they became a requirement to stand against other heavy cavalry regiments, such as those from the east and south. Though their main weapon is the lance, used for head-on charging, cataphracts also train in skirmishing and mobility drills, adapting to what the battlefield requires, yet not relinquishing their outstanding, heavy equipment."),
                DefaultSkills.Polearm,
                DefaultSkills.Riding,
                new List<PerkObject>
                {
                    BKPerks.Instance.CataphractEquites, 
                    BKPerks.Instance.CataphractAdaptiveTactics,
                    BKPerks.Instance.CataphractKlibanophoros
                },
                new TextObject("{=y2zNEeT5}Increased renown from victories by {EFFECT1}%\nNon-cavalry units need {EFFECT2}% more experience to upgrade"),
                12f, 
                25f,
                null,
                cultures.FirstOrDefault(x => x.StringId == "empire"));

            Diplomat = new Lifestyle("lifestyle_diplomat");
            Diplomat.Initialize(new TextObject("{=uc7xjTQv}Diplomat"), new TextObject("{=!}"),
                DefaultSkills.Charm, 
                BKSkills.Instance.Lordship,
                new List<PerkObject>(),
                new TextObject("{=!}"),
                0f, 
                0f);

            August = new Lifestyle("lifestyle_august");
            August.Initialize(new TextObject("{=wAPqOuBe}August"), 
                new TextObject("{=!}A term coined by the Imperials, August is a lord that outshines his peers. Not only they are shrewd administrators of their realm, they are also distinguished commanders, directly leading their troops against any threats to their people. August leaders are those forever to be remembered in history as examples of what a true leader ought to be."),
                DefaultSkills.Leadership, 
                BKSkills.Instance.Lordship,
                new List<PerkObject>
                {
                    BKPerks.Instance.AugustCommander, BKPerks.Instance.AugustDeFacto,
                    BKPerks.Instance.AugustDeJure, BKPerks.Instance.AugustKingOfKings
                },
                new TextObject("{=H1igmZMn}1 knight less is counted towards vassal limit\nTrade penalty increased by {EFFECT2}%"),
                1f, 
                20f);

            SiegeEngineer = new Lifestyle("lifestyle_siegeEngineer");
            SiegeEngineer.Initialize(new TextObject("{=brd9F4gY}Siege Engineer"), 
                new TextObject("{=!}Highly sought-after specialists, siege engineers specialize in both defending and taking down enemy settlements. A relatively new art, sieges become more prevalent in the continent as populations gather in towns, and all-out wars break between kingdoms rather than small, tribal raids."),
                DefaultSkills.Engineering, 
                DefaultSkills.Tactics,
                new List<PerkObject>
                {
                    BKPerks.Instance.SiegeEngineer,
                    BKPerks.Instance.SiegePlanner,
                    BKPerks.Instance.SiegeOverseer
                },
                new TextObject("{=!}Party expenses are {EFFECT1}% cheaper during sieges\nSettlements of different culture have {EFFECT2}% more autonomy"),
                30f, 
                10f);

            CivilAdministrator = new Lifestyle("lifestyle_civilAdministrator");
            CivilAdministrator.Initialize(new TextObject("{=EJEkuBZ4}Civil Administrator"), 
                new TextObject("{=!}"),
                DefaultSkills.Engineering, DefaultSkills.Steward,
                new List<PerkObject>
                {
                    BKPerks.Instance.CivilEngineer, 
                    BKPerks.Instance.CivilCultivator,
                    BKPerks.Instance.CivilManufacturer, 
                    BKPerks.Instance.CivilOverseer
                },
                new TextObject("{=sCxt8vV7}Reduced demesne weight of towns by {EFFECT1}%\nParty size reduced by {EFFECT2}"),
                20f,
                15f);

            Caravaneer = new Lifestyle("lifestyle_caravaneer");
            Caravaneer.Initialize(new TextObject("{=F5aAvvhD}Caravaneer"), 
                new TextObject("{=!}"),
                DefaultSkills.Trade, 
                DefaultSkills.Scouting,
                new List<PerkObject>
                {
                    BKPerks.Instance.CaravaneerStrider,
                    BKPerks.Instance.CaravaneerDealer,
                    BKPerks.Instance.CaravaneerOutsideConnections
                },
                new TextObject("{=1UEN6xUV}Reduced trade penalty by {EFFECT1}%\nReduced speed during nighttime by {EFFECT2}%"),
                10f,
                8f);

            Artisan = new Lifestyle("lifestyle_artisan");
            Artisan.Initialize(new TextObject("{=RUtaPqBv}Artisan"), new TextObject("{=!}"),
                DefaultSkills.Crafting,
                DefaultSkills.Trade, 
                new List<PerkObject>
                {
                    BKPerks.Instance.ArtisanSmith, 
                    BKPerks.Instance.ArtisanCraftsman, 
                    BKPerks.Instance.ArtisanEntrepeneur
                },
                new TextObject("{=mV7M6SgW}Chance of botching items when smithing reduced by {EFFECT1}%\nRecruiting troops is {EFFECT2}% more expensive"),
                10f, 
                15f);

            Outlaw = new Lifestyle("lifestyle_outlaw");
            Outlaw.Initialize(new TextObject("{=GTYYnH9E}Outlaw"), new TextObject("{=!}"),
                DefaultSkills.Roguery, 
                DefaultSkills.Scouting,
                new List<PerkObject>
                {
                    BKPerks.Instance.OutlawKidnapper, 
                    BKPerks.Instance.OutlawPlunderer,
                    BKPerks.Instance.OutlawNightPredator
                },
                new TextObject("{=gkaq9L2T}Bandit troops are {EFFECT1}% faster on map\nRandomly lose relations with heroes that disapprove criminality when leaving dialogue"),
                10f, 
                8f);

            Mercenary = new Lifestyle("lifestyle_mercenary");
            Mercenary.Initialize(new TextObject("{=kLHXZnLY}Mercenary"), new TextObject("{=!}Mercenaries in the continent have become more and more relevant since the crumbling of the Empire. Ravaged by internal and exterior wars, the Imperial factions rely progressively more on foreign troops, as the populace becomes thinner with every squabbling conflict of Imperial lords and raid from foreigners. Currently, the life of a mercenary may prove quite profitable and prosperous in the continent - if they are to survive."),
                DefaultSkills.Leadership,
                DefaultSkills.Roguery, 
                new List<PerkObject>
                {
                    BKPerks.Instance.MercenaryLocalConnections, 
                    BKPerks.Instance.MercenaryRansacker,
                    BKPerks.Instance.MercenaryFamousSellswords
                },
                new TextObject("{=mAta6M84}Influence is {EFFECT1}% more profitable as mercenary\nSettlement stability reduced by {EFFECT2}%"),
                20f, 
                10f);

            Kheshig = new Lifestyle("lifestyle_kheshig");
            Kheshig.Initialize(new TextObject("{=sccoC5ta}Kheshig"), 
                new TextObject("{=!}Kheshig is the way of life adopted by the eastern nomads, as an adaptation to the harsh steppe life. Kheshig travel in large groups of mounted warriors, often with many spare horses and cattle heads. These feed the horde with dairy and meat. They are known for expert mounted archery skirmishing, and merciless raiding. Yet, Kheshig have a profound sense of respect to those born in the steppes, and find honor in serving a strong leader."),
                DefaultSkills.Riding, 
                DefaultSkills.Bow, 
                new List<PerkObject>
                {
                    BKPerks.Instance.KheshigRaider, 
                    BKPerks.Instance.KheshigOutrider,
                    BKPerks.Instance.KheshigHonorGuard
                },
                new TextObject("{=!}Party size increased by {EFFECT1}%\nNon-Khuzait troops negatively affect morale by {EFFECT2}%"),
                15f, 
                0.5f,
                null,
                cultures.First(x => x.StringId == "khuzait"));

            Varyag = new Lifestyle("lifestyle_varyag");
            Varyag.Initialize(new TextObject("{=4pYayWGi}Varyag"), 
                new TextObject("{=!}Varyags are known in Sturgian culture as men of valor, adventure and prestige. These nearly mythical heroes often prove their worth in fierce shieldwall combat. Being individualistic adventurers, they will often serve as mercenaries, raid and plunder others as the opportunities come, though preferably non-Sturgians. Yet, a high moral code is expected of them, when it comes to courage and honor - a varyag afraid of death or that does not keep his word is no true varyag -, and those that follow the unwritten code are considered Drengrs."),
                DefaultSkills.Athletics,
                DefaultSkills.OneHanded,
                new List<PerkObject>
                {
                    BKPerks.Instance.VaryagShieldBrother,
                    BKPerks.Instance.VaryagRecognizedMercenary,
                    BKPerks.Instance.VaryagDrengr
                },
                new TextObject("{=0HiZjcrv}Infantry troops are {EFFECT1}% faster on the map\nYou and your formation deal {EFFECT2}% less damage when mounted"),
                8f,
                20f,
                null,
                cultures.First(x => x.StringId == "sturgia"));

            Gladiator = new Lifestyle("lifestyle_gladiator");
            Gladiator.Initialize(new TextObject("{=wTyw0yfR}Gladiator"), 
                new TextObject("{=!}As a means of entertainement, Imperials have developed the gladiatorial combat in arenas. It is possible this was an adaptation of earlier forms of duels used by the Palaics to judge valor. Nevertheless, the gladiatorial combat became a staple in the empire. Up to this day, an adventurer of little renown can prove his worth in the arenas and draw attention to himself as someone worthy of the lords' attention."),
                DefaultSkills.Athletics,
                DefaultSkills.Riding,
                new List<PerkObject>
                {
                    BKPerks.Instance.GladiatorPromisingAthlete,
                    BKPerks.Instance.GladiatorTourDeCalradia,
                    BKPerks.Instance.GladiatorCrowdsFavorite
                },
                new TextObject("{=ycDyfToX}Combat experience in tournaments increased by {EFFECT1}%\nTrade penalty increased by {EFFECT2}%"),
                200f,
                20f);

            Ritter = new Lifestyle("lifestyle_ritter");
            Ritter.Initialize(new TextObject("{=axhJNG0M}Ritter"), 
                new TextObject("{=!}Vlandians have developed a concept many of them call Ritter, or Reterius as Imperials call it. These are small lords recognized as distinguished soldiers, preferring mounted combat with lances and swords, who tend to their manor demesnes in times of peace. To non-Vlandians, they are best known for the earth-shaking gallop of their cavalry charges."),
                BKSkills.Instance.Lordship,
                DefaultSkills.Riding,
                new List<PerkObject>
                {
                    BKPerks.Instance.RitterIronHorses,
                    BKPerks.Instance.RitterOathbound,
                    BKPerks.Instance.RitterPettySuzerain
                },
                new TextObject("{=!}You and melee cavalry in your formation deals {EFFECT1}% more melee damage.{EFFECT1}%\nYou and your formation deal {EFFECT2}% less ranged damaged, mounted or otherwise."),
                5f,
                15f,
                null,
                cultures.First(x => x.StringId == "vlandia"));

            Jawwal = new Lifestyle("lifestyle_jawwal");
            Jawwal.Initialize(new TextObject("{=!}Jawwal"),
                new TextObject("{=!}The Jawwal represent a traditional and conservative way of living in the southern Nahasa dunes. Relying on their camels for mobility, carrying goods and even food, they have mastered survival on the sandy dunes, and are known for raiding and harassing tactics, specially with mounted skirmishing."),
                DefaultSkills.Throwing,
                DefaultSkills.Riding,
                new List<PerkObject>
                {
                    BKPerks.Instance.JawwalGhazw,
                    BKPerks.Instance.JawwalCamelMaster,
                    BKPerks.Instance.JawwalDuneRider
                },
                new TextObject("{=!}Party consumes {EFFECT1}% less food while on deserts\nDemesne limit reduced by {EFFECT2}%"),
                30f,
                15f,
                null,
                cultures.First(x => x.StringId == "aserai"));
        }
    }
}