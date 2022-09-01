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

        public override IEnumerable<Lifestyle> All
        {
            get
            {
                yield return Cataphract;
                yield return Ritter;
                yield return Fian;
                yield return Varyag;
                yield return Mercenary;
                yield return Outlaw;
                yield return Gladiator;
                yield return Caravaneer;
                yield return August;
                yield return SiegeEngineer;
                yield return CivilAdministrator;
                //yield return Diplomat;
                //yield return Kheshig;
                //
            }
        }

        public override void Initialize()
        {
            var cultures = Game.Current.ObjectManager.GetObjectTypeList<CultureObject>();

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
                1f, 
                30f,
                cultures.FirstOrDefault(x => x.StringId == "battania"));

            Cataphract = new Lifestyle("lifestyle_cataphract");
            Cataphract.Initialize(new TextObject("{=RWrMBAbv}Cataphract"), new TextObject("{=!}"),
                DefaultSkills.Polearm,
                DefaultSkills.Riding,
                new List<PerkObject>
                {
                    BKPerks.Instance.CataphractEquites, 
                    BKPerks.Instance.CataphractAdaptiveTactics,
                    BKPerks.Instance.CataphractKlibanophoros
                },
                new TextObject("{=y2zNEeT5}Increased renown from victories by {EFFECT1}%\n"),
                12f, 
                0f,
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
            August.Initialize(new TextObject("{=wAPqOuBe}August"), new TextObject("{=!}"),
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
            SiegeEngineer.Initialize(new TextObject("{=brd9F4gY}Siege Engineer"), new TextObject("{=!}"),
                DefaultSkills.Engineering, 
                DefaultSkills.Tactics,
                new List<PerkObject>
                {
                    BKPerks.Instance.SiegeEngineer,
                    BKPerks.Instance.SiegePlanner,
                    BKPerks.Instance.SiegeOverseer
                },
                new TextObject("{=!}"),
                0f, 
                0f);

            CivilAdministrator = new Lifestyle("lifestyle_civilAdministrator");
            CivilAdministrator.Initialize(new TextObject("{=EJEkuBZ4}Civil Administrator"), new TextObject("{=!}"),
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
            Caravaneer.Initialize(new TextObject("{=F5aAvvhD}Caravaneer"), new TextObject("{=!}"),
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
            Mercenary.Initialize(new TextObject("{=kLHXZnLY}Mercenary"), new TextObject("{=!}"),
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
            Kheshig.Initialize(new TextObject("{=sccoC5ta}Kheshig"), new TextObject("{=!}"),
                DefaultSkills.Leadership, 
                DefaultSkills.Roguery, 
                new List<PerkObject>
                {
                    BKPerks.Instance.KheshigKhorchin, 
                    BKPerks.Instance.KheshigTorguud,
                    BKPerks.Instance.KheshigKhevtuul
                },
                new TextObject("{=mAta6M84}Reduced demesne weight of towns by {EFFECT1}%\nSettlement stability reduced by {EFFECT2}%"),
                20f, 
                8f);

            Varyag = new Lifestyle("lifestyle_varyag");
            Varyag.Initialize(new TextObject("{=4pYayWGi}Varyag"), new TextObject("{=!}"),
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
                20f);

            Gladiator = new Lifestyle("lifestyle_gladiator");
            Gladiator.Initialize(new TextObject("{=wTyw0yfR}Gladiator"), new TextObject("{=!}"),
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
            Ritter.Initialize(new TextObject("{=axhJNG0M}Ritter"), new TextObject("{=!}"),
                BKSkills.Instance.Lordship,
                DefaultSkills.Riding,
                new List<PerkObject>
                {
                    BKPerks.Instance.RitterIronHorses,
                    BKPerks.Instance.RitterOathbound,
                    BKPerks.Instance.RitterPettySuzerain
                },
                new TextObject("{=3Br24ayr}You and melee cavalry in your formation deals {EFFECT1}% more melee damage.{EFFECT1}%\n You and your formation deal {EFFECT2}% less ranged damaged, mounted or otherwise."),
                5f,
                15f, 
                cultures.First(x => x.StringId == "vlandia"));
        }
    }
}