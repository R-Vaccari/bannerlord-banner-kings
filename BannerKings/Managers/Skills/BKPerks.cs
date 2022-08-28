using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;

namespace BannerKings.Managers.Skills
{
    public class BKPerks : DefaultTypeInitializer<BKPerks, PerkObject>
    {
        private static readonly int[] Requirements =
        {
            25,
            50,
            75,
            100,
            125,
            150,
            175,
            200,
            225,
            250,
            275,
            300
        };

        public HashSet<PerkObject> LifestylePerks { get; } = new();

        #region Fian

        public PerkObject FianHighlander { get; private set; }

        public PerkObject FianRanger { get; private set; }

        public PerkObject FianFennid { get; private set; }

        #endregion Fian

        #region Civil

        public PerkObject CivilEngineer { get; private set; }

        public PerkObject CivilCultivator { get; private set; }

        public PerkObject CivilManufacturer { get; private set; }

        public PerkObject CivilOverseer { get; private set; }

        #endregion Civil

        #region Siege

        public PerkObject SiegeEngineer { get; private set; }

        public PerkObject SiegePlanner { get; private set; }

        public PerkObject SiegeOverseer { get; private set; }

        #endregion Siege

        #region August

        public PerkObject AugustCommander { get; private set; }

        public PerkObject AugustDeFacto { get; private set; }

        public PerkObject AugustDeJure { get; private set; }

        public PerkObject AugustKingOfKings { get; private set; }

        #endregion August

        #region Cataphract

        public PerkObject CataphractEquites { get; private set; }

        public PerkObject CataphractAdaptiveTactics { get; private set; }

        public PerkObject CataphractKlibanophoros { get; private set; }

        #endregion Cataphract

        #region Caravaneer

        public PerkObject CaravaneerStrider { get; private set; }

        public PerkObject CaravaneerDealer { get; private set; }

        public PerkObject CaravaneerOutsideConnections { get; private set; }

        #endregion Caravaneer

        #region Artisan

        public PerkObject ArtisanSmith { get; private set; }

        public PerkObject ArtisanCraftsman { get; private set; }

        public PerkObject ArtisanEntrepeneur { get; private set; }

        #endregion Artisan

        #region Outlaw

        public PerkObject OutlawKidnapper { get; private set; }

        public PerkObject OutlawPlunderer { get; private set; }

        public PerkObject OutlawNightPredator { get; private set; }

        public PerkObject OutlawUnderworldKing { get; private set; }

        #endregion Outlaw

        #region Kheshig

        public PerkObject KheshigKhorchin { get; }

        public PerkObject KheshigTorguud { get; }

        public PerkObject KheshigKhevtuul { get; }

        #endregion Kheshig

        #region Mercenary

        public PerkObject MercenaryLocalConnections { get; private set; }

        public PerkObject MercenaryRansacker { get; private set; }

        public PerkObject MercenaryFamousSellswords { get; private set; }

        #endregion Mercenary

        #region  Gladiator

        public PerkObject GladiatorPromisingAthlete { get; private set; }

        public PerkObject GladiatorTourDeCalradia { get; private set; }

        public PerkObject GladiatorCrowdsFavorite { get; private set; }

        #endregion  Gladiator


        #region  Ritter

        public PerkObject RitterIronHorses { get; private set; }

        public PerkObject RitterOathbound { get; private set; }

        public PerkObject RitterPettySuzerain { get; private set; }

        #endregion  Ritter


        #region Lordship

        public PerkObject LordshipEconomicAdministration { get; private set; }

        public PerkObject LordshipTraditionalist { get; private set; }

        public PerkObject LordshipAdaptive { get; private set; }

        public PerkObject LordshipAccolade { get; private set; }

        public PerkObject LordshipManorLord { get; private set; }

        public PerkObject LordshipMilitaryAdministration { get; private set; }

        public PerkObject LordshipClaimant { get; private set; }

        public PerkObject LordshipPatron { get; private set; }

        #endregion Lordship

        #region  Scholarship

        public PerkObject ScholarshipLiterate { get; private set; }

        public PerkObject ScholarshipAvidLearner { get; private set; }

        public PerkObject ScholarshipTutor { get; private set; }

        public PerkObject ScholarshipWellRead { get; private set; }

        public PerkObject ScholarshipTeacher { get; private set; }

        public PerkObject ScholarshipBookWorm { get; private set; }

        public PerkObject ScholarshipPeerReview { get; private set; }

        public PerkObject ScholarshipBedTimeStory { get; private set; }

        public PerkObject ScholarshipPolyglot { get; private set; }

        public PerkObject ScholarshipMechanic { get; private set; }

        public PerkObject ScholarshipAccountant { get; private set; }

        public PerkObject ScholarshipNaturalScientist { get; private set; }

        public PerkObject ScholarshipTreasurer { get; private set; }

        public PerkObject ScholarshipMagnumOpus { get; private set; }

        #endregion  Scholarship

        #region  Theology

        public PerkObject TheologyFaithful { get; private set; }
        public PerkObject TheologyBlessed { get; private set; }
        public PerkObject TheologyReligiousTeachings { get; private set; }
        public PerkObject TheologyRitesOfPassage { get; private set; }
        public PerkObject TheologyPreacher { get; private set; }
        public PerkObject TheologyLithurgy { get; private set; }

        #endregion  Theology

        public override IEnumerable<PerkObject> All
        {
            get
            {
                foreach (var perkObject in Game.Current.ObjectManager.GetObjectTypeList<PerkObject>())
                {
                    yield return perkObject;
                }

                foreach (var lifestylePerk in LifestylePerks)
                {
                    yield return lifestylePerk;
                }
            }
        }

        private void InitializeLifestylePerks()
        {
            #region Fian

            FianHighlander = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleFianHighlander"));
            LifestylePerks.Add(FianHighlander);
            FianHighlander.InitializeNew("{=U7W2kGgA}Highlander", null, 
                80, 
                null,
                "{=WgmJfTeR}Increases your movement speed by 5%.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=uXKiKZXR}Increases your swing speed with two handed swords by 6%.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            FianRanger = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleFianRanger"));
            LifestylePerks.Add(FianRanger);
            FianRanger.InitializeNew("{=w7GFfrAy}Ranger", null, 
                160, 
                null,
                "{=ZK5MjmMK}Increase maximum track life by 20%.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=RKMxkhwX}Increases your damage with bows by 8%.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            FianFennid = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleFianFennid"));
            LifestylePerks.Add(FianFennid);
            FianFennid.InitializeNew("{=qvQEEEM4}Fénnid", null, 
                240, 
                null,
                "{=kFGo9afp}Aiming with your bow is 25% faster.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=fxuVYTrJ}Increases your two handed weapon damage by 10%.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            #endregion Fian

            #region Civil

            CivilEngineer = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleCivilEngineer"));
            LifestylePerks.Add(CivilEngineer);
            CivilEngineer.InitializeNew("{=M9R9NkrP}Civil Engineer", null, 
                80,
                null,
                "{=J6oPqQmt}Settlements have an additional catapult during siege start.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=8AmeeiL0}Workforce yields 20% extra construction.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            CivilCultivator = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleCivilCultivator"));
            LifestylePerks.Add(CivilCultivator);
            CivilCultivator.InitializeNew("{=phRxxa8X}Cultivator", null, 
                160, 
                null,
                "{=EH3ExMr9}Agricultural yield increases by flat 5%.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=Z2cPBwOj}Village hearth growth increases by 1.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            CivilOverseer = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleCivilOverseer"));
            CivilManufacturer = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleCivilManufacturer"));
            LifestylePerks.Add(CivilOverseer);
            CivilOverseer.InitializeNew("{=DZXXrNon}Overseer", null, 
                320, 
                null,
                "{=zaVqT3bv}Stability increases by flat 5%.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=wBqTCqgx}Increases infrastructure limit by flat 5.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            LifestylePerks.Add(CivilManufacturer);
            CivilManufacturer.InitializeNew("{=UmFnG5z2}Manufacturer", null, 
                240, 
                null,
                "{=UruYDkr2}Production efficiency increases by flat 15%.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=eQQW0Brf}Production quality increases by flat 10%.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            #endregion Civil

            #region Siege

            SiegeEngineer = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleSiegeEngineer"));
            LifestylePerks.Add(SiegeEngineer);
            SiegeEngineer.InitializeNew("{=brd9F4gY}Siege Engineer", null, 
                80, 
                null,
                "{=2jDEHBg3}Get a pre-built ballista as attacker during siege.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=mcVnKCsL}Damage to walls increased by 10% during siege.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            SiegePlanner = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleSiegePlanner"));
            LifestylePerks.Add(SiegePlanner);
            SiegePlanner.InitializeNew("{=VyzxZL7T}Siege Planner", null, 
                160,
                null,
                "{=5jMZb0xZ}Ranged infantry deals 15% more damage in siege simulations.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=KWfdgmuc}Wall hit points are increased by 25%.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            SiegeOverseer = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleSiegeOverseer"));
            LifestylePerks.Add(SiegeOverseer);
            SiegeOverseer.InitializeNew("{=tWvXqDWY}Siege Overseer", null, 
                240, 
                null,
                "{=9SoSFu8s}Army consumes 15% less food during sieges, either attacking or defending.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=nvJhzGbv}Camp preparation is 20% faster.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            #endregion Siege

            #region August

            AugustCommander = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleAugustCommander"));
            LifestylePerks.Add(AugustCommander);
            AugustCommander.InitializeNew("{=q6cxgOou}Commander", null,
                80,
                null,
                "{=DWEDZjn1}Increases your party size by 5.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=OxaKMeUa}Increases party morale by flat 3%.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            AugustDeFacto = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleAugustDeFacto"));
            LifestylePerks.Add(AugustDeFacto);
            AugustDeFacto.InitializeNew("{=Yy1wcNon}De Facto", null,
                160, 
                null,
                "{=J6oPqQmt}Settlement autonomy reduced by flat 3%.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=o4Ptq4SC}Randomly receive positive relations with a councillour.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            AugustDeJure = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleAugustDeJure"));
            LifestylePerks.Add(AugustDeJure);
            AugustDeJure.InitializeNew("{=HRUBrSjM}De Jure", null, 
                240, 
                null,
                "{=nBZtX2R0}Demesne limit increased by 1.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=tbJa33Qp}Title actions cost / yield 5% less / more denarii and influence.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            AugustKingOfKings = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleAugustKingOfKings"));
            LifestylePerks.Add(AugustKingOfKings);
            AugustKingOfKings.InitializeNew("{=6pfSPkvd}King of Kings", null,
                320, 
                null,
                "{=fyoL3m5n}If king level or higher, increase vassal limit by 2.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=aeGjJJZw}If king level or higher, increase unlanded demesne limit by 1.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            #endregion August

            #region Cataphract

            CataphractEquites = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleCataphractEquites"));
            LifestylePerks.Add(CataphractEquites);
            CataphractEquites.InitializeNew("{=oYAOv2KP}Equites", null,
                80, 
                null,
                "{=BpFCxR6C}You and troops in your formation deal 10% more charge damage.",
                SkillEffect.PerkRole.Captain, 4f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=R5NiPF7H}Mounted troops cost 10% less denarii maintenance.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            CataphractAdaptiveTactics = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleCataphractAdaptiveTactics"));
            LifestylePerks.Add(CataphractAdaptiveTactics);
            CataphractAdaptiveTactics.InitializeNew("{=gg9Yxqfy}Adaptive Tactics", null,
                160, 
                null,
                "{=Pup1khtn}Increased damage on horseback with polearms, sidearms and bows by 5%.",
                SkillEffect.PerkRole.Personal, 5f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=oYKOf3zK}You and troops in your formation have 8% more maneuvering.",
                SkillEffect.PerkRole.Captain, 8f,
                SkillEffect.EffectIncrementType.AddFactor);

            CataphractKlibanophoros = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleCataphractKlibanophori"));
            LifestylePerks.Add(CataphractKlibanophoros);
            CataphractKlibanophoros.InitializeNew("{=iETO50gi}Klibanophori", null, 
                240, 
                null,
                "{=a2sO3wbW}You and troops in your formation receive 5% less damange when mounted.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=JSxDnFbu}You and troops in your formation deal 6% extra thrust damage when mounted.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            #endregion Cataphract

            #region Caravaneer

            CaravaneerStrider = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleCaravaneerStrider"));
            LifestylePerks.Add(CaravaneerStrider);
            CaravaneerStrider.InitializeNew("{=Nk505umn}Strider", null, 
                80,
                null,
                "{=s0zsXS2Z}Increases your movement speed by 3%.",
                SkillEffect.PerkRole.PartyLeader, 3f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=NGas0eu2}Increases carry capacity of pack animals by 20%.",
                SkillEffect.PerkRole.PartyLeader, 20f,
                SkillEffect.EffectIncrementType.AddFactor);

            CaravaneerDealer = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleCaravaneerDealer"));
            LifestylePerks.Add(CaravaneerDealer);
            CaravaneerDealer.InitializeNew("{=6yEOGwgd}Dealer", null, 
                150, 
                null,
                "{=njAV5qnr}Caravan wages are reduced by 10%.",
                SkillEffect.PerkRole.PartyOwner, 10f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=REgC6u81}Your caravans move 4% faster during daytime.",
                SkillEffect.PerkRole.PartyOwner, 8f,
                SkillEffect.EffectIncrementType.AddFactor);

            CaravaneerOutsideConnections = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleCaravaneerOutsideConnections"));
            CaravaneerOutsideConnections.InitializeNew("{=ZX0fpu3t}Outside Connections", null, 
                240, 
                null,
                "{=0C3HpYf5}Your caravans have 5% less trade penalty.",
                SkillEffect.PerkRole.PartyOwner, 5f,
                SkillEffect.EffectIncrementType.Add,
                "{=TWFxSheh}Randomly gain relations with merchants where your caravans trade.",
                SkillEffect.PerkRole.PartyOwner, 5f,
                SkillEffect.EffectIncrementType.AddFactor);

            #endregion Caravaneer

            #region Artisan

            ArtisanSmith = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleArtisanSmith"));
            ArtisanSmith.InitializeNew("{=etbv7s6N}Smith", null, 
                80, 
                null,
                "{=zOzu5By2}Crafting items costs 10% less energy.",
                SkillEffect.PerkRole.Personal, 10f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=etbv7s6N}Smithy hourly cost is 15% cheaper.",
                SkillEffect.PerkRole.Personal, 15f,
                SkillEffect.EffectIncrementType.AddFactor);

            ArtisanCraftsman = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleArtisanCraftsman"));
            ArtisanCraftsman.InitializeNew("{=iktjoMi1}Craftsman", null, 
                160, 
                null,
                "{=3TB6TJvJ}Your workshops have 5% increase in production quality.",
                SkillEffect.PerkRole.ClanLeader, 5f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=GqTajw9S}You are 5% more likely to craft an item with a better modifier.",
                SkillEffect.PerkRole.Personal, 5f,
                SkillEffect.EffectIncrementType.AddFactor);

            ArtisanEntrepeneur = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleArtisanEntrepeneur"));
            ArtisanEntrepeneur.InitializeNew("{=hNHACmv9}Entrepeneur", null, 
                240,
                null,
                "{=qiMW8Wio}Increased settlement production efficiency by flat 10%.",
                SkillEffect.PerkRole.ClanLeader, 10f,
                SkillEffect.EffectIncrementType.Add,
                "{=p70n92jh}You pay 20% less workshop taxes to other clans. Your settlements tax others' workshops 20% more.",
                SkillEffect.PerkRole.ClanLeader, 20f,
                SkillEffect.EffectIncrementType.AddFactor);

            #endregion Artisan

            #region Outlaw

            OutlawKidnapper = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleOutlawKidnapper"));
            LifestylePerks.Add(OutlawKidnapper);
            OutlawKidnapper.InitializeNew("{=fWwFLnTw}Kidnapper", null, 
                80, 
                null,
                "{=kbBbDiyR}30% better deals reansoming lords.",
                SkillEffect.PerkRole.PartyLeader, 3f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=hBQu6YKu}Decreases the duration of the disorganized state after breaking sieges and raids by 30%.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            OutlawPlunderer = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleOutlawPlunderer"));
            LifestylePerks.Add(OutlawPlunderer);
            OutlawPlunderer.InitializeNew("{=Gqvm5XTq}Infamous Plunderer", null,
                160,
                null,
                "{=njer0pyD}Bandit troops in your party yield influence.",
                SkillEffect.PerkRole.PartyOwner, 10f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=PawhUHjG}Raiding villages is 15% faster.",
                SkillEffect.PerkRole.Captain, 8f,
                SkillEffect.EffectIncrementType.AddFactor);

            OutlawNightPredator = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleOutlawNightPredator"));
            LifestylePerks.Add(OutlawNightPredator);
            OutlawNightPredator.InitializeNew("{=JjE7nzmH}Night Predator", null, 
                240,
                null,
                "{=MB9f1s0O}Your party is 50% harder to spot in forests.",
                SkillEffect.PerkRole.Personal, 10f,
                SkillEffect.EffectIncrementType.Add,
                "{=HWydDHb3}Increased nighttime movement by 6%.",
                SkillEffect.PerkRole.Personal, 20f,
                SkillEffect.EffectIncrementType.AddFactor);

            OutlawUnderworldKing = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleOutlawUnderworldKing"));
            LifestylePerks.Add(OutlawUnderworldKing);
            OutlawUnderworldKing.InitializeNew("{=OMefnnZ9}Underworld King", null, 
                320, 
                null,
                "{=GpcWSVCy}Killing bandit leaders yields renown.",
                SkillEffect.PerkRole.Personal, 10f,
                SkillEffect.EffectIncrementType.Add,
                "{=!}",
                SkillEffect.PerkRole.Personal, 20f,
                SkillEffect.EffectIncrementType.AddFactor);

            #endregion Outlaw

            #region Mercenary

            MercenaryLocalConnections = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleMercenaryLocalConnections"));
            MercenaryLocalConnections.InitializeNew("{=8XeyqTNh}Local Connections", null, 
                80, 
                null,
                "{=jhZ8TFCB}While serving as mercenary, gain the ability to recruit from local minor factions in towns.",
                SkillEffect.PerkRole.PartyLeader, 3f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=JMubUFej}Recruiting mercenary troops is 10% cheaper.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            MercenaryRansacker = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleMercenaryRansacker"));
            MercenaryRansacker.InitializeNew("{=n9ZMPe6w}Ransacker", null, 
                160, 
                null,
                "{=TAfrnnO4}Killing enemies provides 10% more share battle contribution.",
                SkillEffect.PerkRole.PartyOwner, 10f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=PawhUHjG}Raiding villages is 15% faster.",
                SkillEffect.PerkRole.Captain, 8f,
                SkillEffect.EffectIncrementType.AddFactor);

            MercenaryFamousSellswords = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleMercenarySellswords"));
            MercenaryFamousSellswords.InitializeNew("{=976FNbqA}Famous Sellswords", null, 
                240, 
                null,
                "{=EkFaisgP}Influence award for army participation increased by 30%.",
                SkillEffect.PerkRole.Personal, 10f,
                SkillEffect.EffectIncrementType.Add,
                "{=35Mq4ASE}Renown award for victories increased by 20%.",
                SkillEffect.PerkRole.Personal, 20f,
                SkillEffect.EffectIncrementType.AddFactor);

            #endregion Mercenary

            #region Ritter

            RitterIronHorses = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleRitterIronHorses"));
            RitterIronHorses.InitializeNew("{=!}Iron Horses", null,
                80,
                null,
                "{=jCBbUvHx}Mounts of your commanded troops have 10% more hit points.",
                SkillEffect.PerkRole.PartyLeader, 3f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=3GsZXXOi}10% production bonus to villages that are bound to castles.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            RitterOathbound = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleRitterOathbound"));
            RitterOathbound.InitializeNew("{=!}Oathbound", null,
                160,
                null,
                "{=!}Every season, get a chance of improving relations with your suzerain.",
                SkillEffect.PerkRole.PartyOwner, 10f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=!}Recruting mounted troops from your settlements is 15% cheaper.",
                SkillEffect.PerkRole.Captain, 8f,
                SkillEffect.EffectIncrementType.AddFactor);

            RitterPettySuzerain = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleRitterPettySuzerain"));
            RitterPettySuzerain.InitializeNew("{=!}Petty Suzerain", null,
                240,
                null,
                "{=!}Village notables are 20% more likely to produce noble troops instead of peasants.",
                SkillEffect.PerkRole.Personal, 10f,
                SkillEffect.EffectIncrementType.Add,
                "{=!}Village hearths increase by +0.1 daily.",
                SkillEffect.PerkRole.Personal, 20f,
                SkillEffect.EffectIncrementType.AddFactor);

            #endregion Ritter


            #region Gladiator

            GladiatorPromisingAthlete = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleGladiatorPromisingAthlete"));
            GladiatorPromisingAthlete.InitializeNew("{=!}Promising Athlete", null,
                80,
                null,
                "{=!}Gain 30% more profit from bets.",
                SkillEffect.PerkRole.Personal, 3f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=!}Gain 2 relations with a random notable on tournament victory.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            GladiatorTourDeCalradia = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleGladiatorTourDeCalradia"));
            GladiatorTourDeCalradia.InitializeNew("{=!}Tour de Calradia", null,
                160,
                null,
                "{=!}After a tournament is finished, receive a notification of the neartest ongoing tournament.",
                SkillEffect.PerkRole.Personal, 0f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=ilKDq9f5}Double the amount of betting you can use in tournaments.",
                SkillEffect.PerkRole.Personal, 0f,
                SkillEffect.EffectIncrementType.AddFactor);

            GladiatorCrowdsFavorite = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleGladiatorCrowdsFavorite"));
            GladiatorCrowdsFavorite.InitializeNew("{=!}Crowds' Favorite", null,
                240,
                null,
                "{=!}Gain double renown rewards for tournament victories.",
                SkillEffect.PerkRole.Personal, 10f,
                SkillEffect.EffectIncrementType.Add,
                "{=!}Gain 10 influence from tournament victories.",
                SkillEffect.PerkRole.Personal, 20f,
                SkillEffect.EffectIncrementType.AddFactor);

            #endregion Gladiator
        }

        public override void Initialize()
        {
            InitializeLifestylePerks();

            #region Theology

            TheologyFaithful = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("TheologyFaithful"));
            TheologyFaithful.InitializeNew("{=mnpTkVYf}Faithful", BKSkills.Instance.Lordship, 
                GetTierCost(1),
                LordshipAdaptive,
                "{=8zbXJZWL}Piety gain is increased by +0.2 daily.",
                SkillEffect.PerkRole.Personal, 0.2f,
                SkillEffect.EffectIncrementType.Add,
                "{=TYjyR0Ls}Religious notables' volunteers may be recruited.",
                SkillEffect.PerkRole.Ruler, 1f,
                SkillEffect.EffectIncrementType.Add);

            TheologyBlessed = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("TheologyBlessed"));
            TheologyBlessed.InitializeNew("{=hmysbhA8}Blessed", BKSkills.Instance.Lordship, 
                GetTierCost(2),
                LordshipAdaptive,
                "{=p2ekwXZR}Blessings last a season longer.",
                SkillEffect.PerkRole.Personal, 0.2f,
                SkillEffect.EffectIncrementType.Add,
                "{=CsHxKFue}Blessings cost 10% less piety.",
                SkillEffect.PerkRole.Ruler, 1f,
                SkillEffect.EffectIncrementType.Add);

            TheologyReligiousTeachings = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("TheologyReligiousTeachings"));
            TheologyReligiousTeachings.InitializeNew("{=jAXfadxv}Religious Teachings", BKSkills.Instance.Lordship, 
                GetTierCost(3),
                LordshipAdaptive,
                "{=c7v8hrEa}Children receive 1 extra Wisdom when becoming adults.",
                SkillEffect.PerkRole.Personal, 0.2f,
                SkillEffect.EffectIncrementType.Add,
                "{=yctR4vY6}Daily experience points in Theology for companions and family in party.",
                SkillEffect.PerkRole.Ruler, 1f,
                SkillEffect.EffectIncrementType.Add);

            TheologyRitesOfPassage = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("TheologyRitesOfPassage"));
            TheologyPreacher = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("TheologyPreacher"));

            TheologyPreacher.InitializeNew("{=9TwjtYhb}Preacher", BKSkills.Instance.Lordship, 
                GetTierCost(4),
                TheologyRitesOfPassage,
                "{=J6oPqQmt}Settlement religious tensions reduced by X%.",
                SkillEffect.PerkRole.Personal, 0.2f,
                SkillEffect.EffectIncrementType.Add,
                "{=J6oPqQmt}Settlement conversion speed increased by 5%.",
                SkillEffect.PerkRole.Ruler, 1f,
                SkillEffect.EffectIncrementType.Add);

            TheologyRitesOfPassage.InitializeNew("{=or8rXdjy}Rites Of Passage", BKSkills.Instance.Lordship, 
                GetTierCost(4),
                TheologyPreacher,
                "{=mbfGsOCE}Rites can be performed again 1 season sooner.",
                SkillEffect.PerkRole.Personal, 0.2f,
                SkillEffect.EffectIncrementType.Add,
                "{=aTGOhnQS}Rites yield 5 renown.",
                SkillEffect.PerkRole.Ruler, 1f,
                SkillEffect.EffectIncrementType.Add);

            TheologyLithurgy = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("TheologyLithurgy"));
            TheologyLithurgy.InitializeNew("{=n3FhFzTo}Lithurgy", BKSkills.Instance.Lordship, 
                GetTierCost(5),
                null,
                "{=4hNMnjUh}Randomly receive relations with religious notables in your settlements.",
                SkillEffect.PerkRole.Personal, 0.2f,
                SkillEffect.EffectIncrementType.Add,
                "{=kb6BZ0EY}Some unspecified settlement impact",
                SkillEffect.PerkRole.Ruler, 1f,
                SkillEffect.EffectIncrementType.Add);

            #endregion Theology

            #region Lordship

            LordshipTraditionalist = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LordshipTraditionalist"));
            LordshipAdaptive = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LordshipAdaptive"));
            LordshipTraditionalist.InitializeNew("{=uVzu9bd1}Traditionalist", BKSkills.Instance.Lordship, 
                GetTierCost(1),
                LordshipAdaptive,
                "{=rEZSUexA}Increased cultural assimilation speed by 10%",
                SkillEffect.PerkRole.Ruler, 0.1f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=bqxzRYLB}Increased militarism in assimilated settlements by flat 1%",
                SkillEffect.PerkRole.Ruler, 1f,
                SkillEffect.EffectIncrementType.Add);

            LordshipAdaptive.InitializeNew("{=G8gRRBpj}Adaptive", BKSkills.Instance.Lordship,
                GetTierCost(1),
                LordshipTraditionalist,
                "{=PseNxgTc}Reduced loyalty bonus from different cultures by 15%",
                SkillEffect.PerkRole.Ruler, 0.1f,
                SkillEffect.EffectIncrementType.Add,
                "{=EVeiLBOF}Increased settlement stability target by flat 2%",
                SkillEffect.PerkRole.Ruler, 1f,
                SkillEffect.EffectIncrementType.Add);

            LordshipAccolade = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LordshipAccolade"));
            LordshipManorLord = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LordshipManorLord"));
            LordshipAccolade.InitializeNew("{=o6kuCQHW}Accolade", BKSkills.Instance.Lordship, 
                GetTierCost(2), 
                LordshipManorLord,
                "{=KynB5Njq}Knighting requires 15% less influence",
                SkillEffect.PerkRole.Ruler, -0.15f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=ZzDmAkN4}Vassal limit increased by 1",
                SkillEffect.PerkRole.Ruler, 1f,
                SkillEffect.EffectIncrementType.Add);

            LordshipManorLord.InitializeNew("{=XUu53n1F}Manor Lord", BKSkills.Instance.Lordship, 
                GetTierCost(2), 
                LordshipAccolade,
                "{=uanVb5h8}Villages weigh 20% less in demesne limit",
                SkillEffect.PerkRole.Ruler, -0.20f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=kJD93Rh2}Manors provide extra flat 0.2 influence",
                SkillEffect.PerkRole.ClanLeader, 0.2f,
                SkillEffect.EffectIncrementType.Add);

            LordshipMilitaryAdministration = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LordshipMilitaryAdministration"));
            LordshipEconomicAdministration = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LordshipEconomicAdministration"));
            LordshipMilitaryAdministration.InitializeNew("{=wzJW8mFC}Military Administration", BKSkills.Instance.Lordship, 
                GetTierCost(3), 
                LordshipEconomicAdministration,
                "{=tqWtfNch}Increased settlement militarism in settlements by 2%",
                SkillEffect.PerkRole.Ruler, 0.02f,
                SkillEffect.EffectIncrementType.Add,
                "{=6hRejPPe}Increased settlement drafting speed by 20%",
                SkillEffect.PerkRole.Ruler, 0.2f,
                SkillEffect.EffectIncrementType.AddFactor);

            LordshipEconomicAdministration.InitializeNew("{=SEB2hNAG}Economic Administration", BKSkills.Instance.Lordship, 
                GetTierCost(3),
                LordshipMilitaryAdministration,
                "{=w2KEdfGJ}Increased settlement production efficiency by 10%",
                SkillEffect.PerkRole.Ruler, 0.2f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=UjmvizdY}Increased settlement production quality by 5%",
                SkillEffect.PerkRole.Ruler, 0.1f,
                SkillEffect.EffectIncrementType.AddFactor);

            LordshipClaimant = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LordshipClaimant"));
            LordshipPatron = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LordshipPatron"));
            LordshipClaimant.InitializeNew("{=6hY9WysN}Claimant", BKSkills.Instance.Lordship, 
                GetTierCost(4), 
                LordshipPatron,
                "{=6hY9WysN}Claims are built 30% faster",
                SkillEffect.PerkRole.Ruler, 0.3f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=pQ6oCYEb}Hostile actions (claim, usurp & revoke) are 5% cheaper",
                SkillEffect.PerkRole.Ruler, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor);

            LordshipPatron.InitializeNew("{=aHL9od5c}Patron", BKSkills.Instance.Lordship, 
                GetTierCost(4),
                LordshipClaimant,
                "{=moMBKpGt}Grating titles yields renown",
                SkillEffect.PerkRole.Ruler, 0.2f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=jndzbOjF}Amicable actions (grant, negotiate) yield more positive relation",
                SkillEffect.PerkRole.Ruler, 0.1f,
                SkillEffect.EffectIncrementType.AddFactor);

            #endregion Lordship

            #region Scholarship

            ScholarshipLiterate = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipLiterate"));
            ScholarshipLiterate.InitializeNew("{=EFGT3zVR}Literate", BKSkills.Instance.Scholarship, 
                GetTierCost(1), 
                null,
                "{=bm513T3G}Allows reading books", 
                SkillEffect.PerkRole.Personal, 0f,
                SkillEffect.EffectIncrementType.Invalid, 
                string.Empty,
                SkillEffect.PerkRole.None, 0f,
                SkillEffect.EffectIncrementType.Invalid);

            ScholarshipAvidLearner = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipLearner"));
            ScholarshipAvidLearner.InitializeNew("{=tmS5CdWA}Avid Learner", BKSkills.Instance.Scholarship, 
                GetTierCost(2), 
                null,
                "{=JNDa4Q9N}Increase language learning rate",
                SkillEffect.PerkRole.Personal, 20f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=iE5hXmjw}Language limit is increased by 1",
                SkillEffect.PerkRole.Personal, 1f,
                SkillEffect.EffectIncrementType.Add);

            ScholarshipTutor = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipTutor"));
            ScholarshipTutor.InitializeNew("{=T5khtP0R}Tutor", BKSkills.Instance.Scholarship, 
                GetTierCost(3), 
                null,
                "{=uXF06oDk}Additional attribute point to clan children coming of age.",
                SkillEffect.PerkRole.ClanLeader, 1f,
                SkillEffect.EffectIncrementType.Add,
                "{=uFKqv5XM}Extra experience gain for companions and family members in party",
                SkillEffect.PerkRole.PartyLeader, 5f,
                SkillEffect.EffectIncrementType.AddFactor);

            ScholarshipWellRead = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipWellRead"));
            ScholarshipWellRead.InitializeNew("{=ntTyYVuH}Well Read", BKSkills.Instance.Scholarship, 
                GetTierCost(4), 
                null,
                "{=BfnH3yR4}Increased reading rates for books",
                SkillEffect.PerkRole.Personal, 12f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=XxqhKKR5}Cultural fascination progresses faster",
                SkillEffect.PerkRole.Personal, 10f,
                SkillEffect.EffectIncrementType.AddFactor);

            ScholarshipAccountant = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipAccountant"));
            ScholarshipMechanic = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipMechanic"));
            ScholarshipMechanic.InitializeNew("{=BUyRc4AY}Mechanic", BKSkills.Instance.Scholarship, 
                GetTierCost(5),
                ScholarshipAccountant,
                "{=iY5A6B2Y}Engineering skill tree yields both perks rather than 1",
                SkillEffect.PerkRole.Personal, 0f,
                SkillEffect.EffectIncrementType.Invalid,
                string.Empty,
                SkillEffect.PerkRole.None, 0f,
                SkillEffect.EffectIncrementType.Invalid);

            ScholarshipAccountant.InitializeNew("{=o8yaA6r6}Accountant", BKSkills.Instance.Scholarship, 
                GetTierCost(5),
                ScholarshipMechanic,
                "{=zQT8PzBc}Stewardship skill tree yields both perks rather than 1",
                SkillEffect.PerkRole.Personal, 0f,
                SkillEffect.EffectIncrementType.Invalid,
                string.Empty,
                SkillEffect.PerkRole.None, 0f,
                SkillEffect.EffectIncrementType.Invalid);

            ScholarshipTeacher = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipTeacher"));
            ScholarshipTeacher.InitializeNew("{=cYxDhcpG}Teacher", BKSkills.Instance.Scholarship,
                GetTierCost(6),
                null,
                "{=fPje2R7V}Additional focus points to children coming of age",
                SkillEffect.PerkRole.ClanLeader, 2f,
                SkillEffect.EffectIncrementType.Add,
                "{=!}",
                SkillEffect.PerkRole.None, 10f,
                SkillEffect.EffectIncrementType.AddFactor);

            ScholarshipBookWorm = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipBookWorm"));
            ScholarshipBookWorm.InitializeNew("{=4S4MV14E}Book Worm", BKSkills.Instance.Scholarship,
                GetTierCost(7), 
                null,
                "{=BfnH3yR4}Increased reading rates for books",
                SkillEffect.PerkRole.Personal, 20f,
                SkillEffect.EffectIncrementType.Add,
                "{=iE5hXmjw}Language limit is increased by 1",
                SkillEffect.PerkRole.Personal, 1f,
                SkillEffect.EffectIncrementType.Add);
             
            ScholarshipPeerReview = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipPeerReview"));
            ScholarshipPeerReview.InitializeNew("{=o2cMkCJt}Peer Review", BKSkills.Instance.Scholarship, 
                GetTierCost(8), 
                null,
                "{=XdiiPz1L}Clan settlements yield more research points",
                SkillEffect.PerkRole.Personal, 20f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=Tr4vXMDi}Books yield double skill experience",
                SkillEffect.PerkRole.Personal, 100f,
                SkillEffect.EffectIncrementType.AddFactor);

            ScholarshipBedTimeStory = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipBedTimeStory"));
            ScholarshipBedTimeStory.InitializeNew("{=S8D75zGm}Bed Time Story", BKSkills.Instance.Scholarship,
                GetTierCost(9), 
                null,
                "{=dsaqAcgd}Daily experience points in random skill for companions and family in party",
                SkillEffect.PerkRole.PartyLeader, 10f,
                SkillEffect.EffectIncrementType.Add,
                string.Empty,
                SkillEffect.PerkRole.Personal, 100f,
                SkillEffect.EffectIncrementType.AddFactor);

            ScholarshipTreasurer = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipTreasurer"));
            ScholarshipNaturalScientist = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipNaturalScientist"));
            ScholarshipTreasurer.InitializeNew("{=G0HZtZGF}Treasurer", BKSkills.Instance.Scholarship, 
                GetTierCost(10),
                ScholarshipNaturalScientist,
                "{=at3o6Jsb}Trade skill tree yields both perks rather than 1",
                SkillEffect.PerkRole.Personal, 0f,
                SkillEffect.EffectIncrementType.Invalid,
                string.Empty,
                SkillEffect.PerkRole.None, 0f,
                SkillEffect.EffectIncrementType.Invalid);

            ScholarshipNaturalScientist.InitializeNew("{=y34n8Mxh}Natural Scientist", BKSkills.Instance.Scholarship,
                GetTierCost(10), ScholarshipTreasurer,
                "{=W6FgZML0}Medicine skill tree yields both perks rather than 1",
                SkillEffect.PerkRole.Personal, 0f,
                SkillEffect.EffectIncrementType.Invalid,
                string.Empty,
                SkillEffect.PerkRole.None, 0f,
                SkillEffect.EffectIncrementType.Invalid);

            ScholarshipPolyglot = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipPolyglot"));
            ScholarshipPolyglot.InitializeNew("{=LbpgEp03}Polyglot", BKSkills.Instance.Scholarship, 
                GetTierCost(11),
                null,
                "{=28gM5dpU}Language limit is increased by 2", SkillEffect.PerkRole.Personal, 10f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=A81Gi3e4}Language learning is significantly increased",
                SkillEffect.PerkRole.None, 0f,
                SkillEffect.EffectIncrementType.Invalid);

            ScholarshipMagnumOpus = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipMagnumOpus"));
            ScholarshipMagnumOpus.InitializeNew("{=CjDwUkqP}Magnum Opus", BKSkills.Instance.Scholarship,
                GetTierCost(11),
                null,
                "{=iDdeeLXK}+0.2% experience gain for every skill point in Scholarship above 230",
                SkillEffect.PerkRole.Personal, 0.2f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=DAvAqkn3}Focus points add 50% more learning limit",
                SkillEffect.PerkRole.Personal, 50f,
                SkillEffect.EffectIncrementType.AddFactor);
            #endregion Scholarship
        }

        private static int GetTierCost(int tierIndex)
        {
            return Requirements[tierIndex - 1];
        }
    }
}