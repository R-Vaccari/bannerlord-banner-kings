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

        private void InitializePerks()
        {
            #region Fian

            FianHighlander = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleFianHighlander"));
            LifestylePerks.Add(FianHighlander);
            FianHighlander.InitializeNew("{=GKwuBosgB}Highlander", null, 80, null,
                "{=UgFkLjtoF}Increases your movement speed by 5%.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=bWGVnJcdH}Increases your swing speed with two handed swords by 6%.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            FianRanger = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleFianRanger"));
            LifestylePerks.Add(FianRanger);
            FianRanger.InitializeNew("{=uUcSFjBZu}Ranger", null, 160, null,
                "{=5NRai8K5}Increase maximum track life by 20%.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=XzbpZji3}Increases your damage with bows by 8%.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            FianFennid = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleFianFennid"));
            LifestylePerks.Add(FianFennid);
            FianFennid.InitializeNew("{=uzPKB1vfv}Fénnid", null, 240, null,
                "{=MOITVVNu}Aiming with your bow is 25% faster.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=lOaVRxdR}Increases your two handed weapon damage by 10%.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            #endregion Fian

            #region Civil

            CivilEngineer = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleCivilEngineer"));
            LifestylePerks.Add(CivilEngineer);
            CivilEngineer.InitializeNew("{=Zj99JCfAi}Civil Engineer", null, 80, null,
                "{=zT2EAi9vS}Settlements have an additional catapult during siege start.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=jOT8dWVAO}Workforce yields 20% extra construction.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            CivilCultivator = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleCivilCultivator"));
            LifestylePerks.Add(CivilCultivator);
            CivilCultivator.InitializeNew("{=QrJBpwEjZ}Cultivator", null, 160, null,
                "{=p240uAnRr}Agricultural yield increases by flat 5%.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=m2zbhJNWq}Village hearth growth increases by 1.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            CivilOverseer = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleCivilOverseer"));
            CivilManufacturer = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleCivilManufacturer"));
            LifestylePerks.Add(CivilOverseer);
            CivilOverseer.InitializeNew("{=7EXnG52jk}Overseer", null, 320, null,
                "{=2RD1zS65c}Stability increases by flat 5%.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=xSQnJ8vLp}Increases infrastructure limit by flat 5.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            LifestylePerks.Add(CivilManufacturer);
            CivilManufacturer.InitializeNew("{=hp3qiyj6u}Manufacturer", null, 240, null,
                "{=4Ts8CyZ5O}Production efficiency increases by flat 15%.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=5rGsTfrHD}Production quality increases by flat 10%.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            #endregion Civil

            #region Siege

            SiegeEngineer = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleSiegeEngineer"));
            LifestylePerks.Add(SiegeEngineer);
            SiegeEngineer.InitializeNew("{=h9TFxcQ9D}Siege Engineer", null, 80, null,
                "{=NWavp3k4s}Get a pre-built ballista as attacker during siege.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=8tLFtjfMH}Damage to walls increased by 10% during siege.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            SiegePlanner = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleSiegePlanner"));
            LifestylePerks.Add(SiegePlanner);
            SiegePlanner.InitializeNew("{=FMWXMyy9a}Siege Planner", null, 160, null,
                "{=Y992PG0RQ}Ranged infantry deals 15% more damage in siege simulations.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=rkKMB9qK}Wall hit points are increased by 25%.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            SiegeOverseer = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleSiegeOverseer"));
            LifestylePerks.Add(SiegeOverseer);
            SiegeOverseer.InitializeNew("{=3UKGeqfOy}Siege Overseer", null, 240, null,
                "{=QoV89LNdA}Army consumes 15% less food during sieges, either attacking or defending.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=ta2z0bEv}Camp preparation is 20% faster.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            #endregion Siege

            #region August

            AugustCommander = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleAugustCommander"));
            LifestylePerks.Add(AugustCommander);
            AugustCommander.InitializeNew("{=wjMm7ypDS}Commander", null, 80, null,
                "{=920FKjJk}Increases your party size by 5.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=O5w7i3CZE}Increases party morale by flat 3%.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            AugustDeFacto = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleAugustDeFacto"));
            LifestylePerks.Add(AugustDeFacto);
            AugustDeFacto.InitializeNew("{=4ALHrZ7RH}De Facto", null, 160, null,
                "{=OGHK9STbX}Settlement autonomy reduced by flat 3%.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=17m9wAshh}Randomly receive positive relations with a councillour.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            AugustDeJure = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleAugustDeJure"));
            LifestylePerks.Add(AugustDeJure);
            AugustDeJure.InitializeNew("{=ts1Qr3r6M}De Jure", null, 240, null,
                "{=F1B683ePb}Demesne limit increased by 1.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=0vCJRv8PF}Title actions cost / yield 5% less / more denarii and influence.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            AugustKingOfKings = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleAugustKingOfKings"));
            LifestylePerks.Add(AugustKingOfKings);
            AugustKingOfKings.InitializeNew("{=dNw5XG2am}King of Kings", null, 320, null,
                "{=4V2tYuZKM}If king level or higher, increase vassal limit by 2.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=aq0oC5FuZ}If king level or higher, increase unlanded demesne limit by 1.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            #endregion August

            #region Cataphract

            CataphractEquites = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleCataphractEquites"));
            LifestylePerks.Add(CataphractEquites);
            CataphractEquites.InitializeNew("{=EkSSetrZM}Equites", null, 80, null,
                "{=YyDwUpOGA}You and troops in your formation deal 10% more charge damage.",
                SkillEffect.PerkRole.Captain, 4f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=tOrehzU70}Mounted troops cost 10% less denarii maintenance.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            CataphractAdaptiveTactics = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleCataphractAdaptiveTactics"));
            LifestylePerks.Add(CataphractAdaptiveTactics);
            CataphractAdaptiveTactics.InitializeNew("{=9sfMkT6st}Adaptive Tactics", null, 160, null,
                "{=e3QWTR03H}Increased damage on horseback with polearms, sidearms and bows by 5%.",
                SkillEffect.PerkRole.Personal, 5f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=D6NGUXhhC}You and troops in your formation have 8% more maneuvering.",
                SkillEffect.PerkRole.Captain, 8f,
                SkillEffect.EffectIncrementType.AddFactor);

            CataphractKlibanophoros = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleCataphractKlibanophori"));
            LifestylePerks.Add(CataphractKlibanophoros);
            CataphractKlibanophoros.InitializeNew("{=4cMzMBajz}Klibanophori", null, 240, null,
                "{=sQMS1A2Sv}You and troops in your formation receive 5% less damange when mounted.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=xBYkBH1X9}You and troops in your formation deal 6% extra thrust damage when mounted.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            #endregion Cataphract

            #region Caravaneer

            CaravaneerStrider = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleCaravaneerStrider"));
            LifestylePerks.Add(CaravaneerStrider);
            CaravaneerStrider.InitializeNew("{=3JRkhS8dw}Strider", null, 80, null,
                "{=z7r2osCcm}Increases your movement speed by 3%.",
                SkillEffect.PerkRole.PartyLeader, 3f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=ulgd9Veb}Increases carry capacity of pack animals by 20%.",
                SkillEffect.PerkRole.PartyLeader, 20f,
                SkillEffect.EffectIncrementType.AddFactor);

            CaravaneerDealer = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleCaravaneerDealer"));
            LifestylePerks.Add(CaravaneerDealer);
            CaravaneerDealer.InitializeNew("{=amsyU9f7R}Dealer", null, 150, null,
                "{=sZQO5rtLc}Caravan wages are reduced by 10%.",
                SkillEffect.PerkRole.PartyOwner, 10f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=pQd9WkJSW}Your caravans move 4% faster during daytime.",
                SkillEffect.PerkRole.PartyOwner, 8f,
                SkillEffect.EffectIncrementType.AddFactor);

            CaravaneerOutsideConnections = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleCaravaneerOutsideConnections"));
            CaravaneerOutsideConnections.InitializeNew("{=LzwqwxURG}Outside Connections", null, 240, null,
                "{=Zic3Hi1Sk}Your caravans have 5% less trade penalty.",
                SkillEffect.PerkRole.PartyOwner, 5f,
                SkillEffect.EffectIncrementType.Add,
                "{=Qm4yQTqrO}Randomly gain relations with merchants where your caravans trade.",
                SkillEffect.PerkRole.PartyOwner, 5f,
                SkillEffect.EffectIncrementType.AddFactor);

            #endregion Caravaneer

            #region Artisan

            ArtisanSmith = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleArtisanSmith"));
            ArtisanSmith.InitializeNew("{=t0bfXkmHF}Smith", null, 80, null,
                "{=7BKvOFWKR}Crafting items costs 10% less energy.",
                SkillEffect.PerkRole.Personal, 10f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=t0bfXkmHF}Smithy hourly cost is 15% cheaper.",
                SkillEffect.PerkRole.Personal, 15f,
                SkillEffect.EffectIncrementType.AddFactor);

            ArtisanCraftsman = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleArtisanCraftsman"));
            ArtisanCraftsman.InitializeNew("{=vw7qzsHeg}Craftsman", null, 160, null,
                "{=9oPj2ddm2}Your workshops have 5% increase in production quality.",
                SkillEffect.PerkRole.ClanLeader, 5f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=5n8vt4SVG}You are 5% more likely to craft an item with a better modifier.",
                SkillEffect.PerkRole.Personal, 5f,
                SkillEffect.EffectIncrementType.AddFactor);

            ArtisanEntrepeneur = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleArtisanEntrepeneur"));
            ArtisanEntrepeneur.InitializeNew("{=9v5AAAN27}Entrepeneur", null, 240, null,
                "{=zxrwcQ2FW}Increased settlement production efficiency by flat 10%.",
                SkillEffect.PerkRole.ClanLeader, 10f,
                SkillEffect.EffectIncrementType.Add,
                "{=hgDpAzeQC}You pay 20% less workshop taxes to other clans. Your settlements tax others' workshops 20% more.",
                SkillEffect.PerkRole.ClanLeader, 20f,
                SkillEffect.EffectIncrementType.AddFactor);

            #endregion Artisan

            #region Outlaw

            OutlawKidnapper = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleOutlawKidnapper"));
            LifestylePerks.Add(OutlawKidnapper);
            OutlawKidnapper.InitializeNew("{=6NQnSYMtv}Kidnapper", null, 80, null,
                "{=hTQfq5XkV}30% better deals reansoming lords.",
                SkillEffect.PerkRole.PartyLeader, 3f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=DGu0nQWa2}Decreases the duration of the disorganized state after breaking sieges and raids by 30%.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            OutlawPlunderer = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleOutlawPlunderer"));
            LifestylePerks.Add(OutlawPlunderer);
            OutlawPlunderer.InitializeNew("{=0rNmjSOXp}Infamous Plunderer", null, 160, null,
                "{=iDh1nWixY}Bandit troops in your party yield influence.",
                SkillEffect.PerkRole.PartyOwner, 10f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=76yNMC5oA}Raiding villages is 15% faster.",
                SkillEffect.PerkRole.Captain, 8f,
                SkillEffect.EffectIncrementType.AddFactor);

            OutlawNightPredator = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleOutlawNightPredator"));
            LifestylePerks.Add(OutlawNightPredator);
            OutlawNightPredator.InitializeNew("{=81OsxJEY4}Night Predator", null, 240, null,
                "{=PiK5TKp2e}Your party is 50% harder to spot in forests.",
                SkillEffect.PerkRole.Personal, 10f,
                SkillEffect.EffectIncrementType.Add,
                "{=8eqR5EXDE}Increased nighttime movement by 6%.",
                SkillEffect.PerkRole.Personal, 20f,
                SkillEffect.EffectIncrementType.AddFactor);

            OutlawUnderworldKing = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleOutlawUnderworldKing"));
            LifestylePerks.Add(OutlawUnderworldKing);
            OutlawUnderworldKing.InitializeNew("{=dhmo9uX7S}Underworld King", null, 320, null,
                "{=5tKXxdcaF}Killing bandit leaders yields renown.",
                SkillEffect.PerkRole.Personal, 10f,
                SkillEffect.EffectIncrementType.Add,
                "{=!}",
                SkillEffect.PerkRole.Personal, 20f,
                SkillEffect.EffectIncrementType.AddFactor);

            #endregion Outlaw

            #region Mercenary

            MercenaryLocalConnections = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleMercenaryLocalConnections"));
            LifestylePerks.Add(MercenaryLocalConnections);
            MercenaryLocalConnections.InitializeNew("{=ftnFbrgua}Local Connections", null, 80, null,
                "{=9LH3yAFwg}While serving as mercenary, gain the ability to recruit from local minor factions in towns.",
                SkillEffect.PerkRole.PartyLeader, 3f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=OYoxMoA8Q}Recruiting mercenary troops is 10% cheaper.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            MercenaryRansacker = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleMercenaryRansacker"));
            MercenaryRansacker.InitializeNew("{=hvxpwfJLQ}Ransacker", null, 160, null,
                "{=k2TbLe718}Gain 10% more share of loot in victories.",
                SkillEffect.PerkRole.PartyOwner, 10f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=76yNMC5oA}Raiding villages is 15% faster.",
                SkillEffect.PerkRole.Captain, 8f,
                SkillEffect.EffectIncrementType.AddFactor);

            MercenaryFamousSellswords = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleMercenarySellswords"));
            MercenaryFamousSellswords.InitializeNew("{=ihzf3GLuE}Famous Sellswords", null, 240, null,
                "{=gQBJxk8T4}Influence award for army participation increased by 30%.",
                SkillEffect.PerkRole.Personal, 10f,
                SkillEffect.EffectIncrementType.Add,
                "{=wsuufdV7a}Renown award for victories increased by 20%.",
                SkillEffect.PerkRole.Personal, 20f,
                SkillEffect.EffectIncrementType.AddFactor);

            #endregion Mercenary
        }

        public override void Initialize()
        {
            InitializePerks();

            #region Theology

            TheologyFaithful = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("TheologyFaithful"));
            TheologyFaithful.InitializeNew("{=fpZGArKcd}Traditionalist", BKSkills.Instance.Lordship, GetTierCost(1),
                LordshipAdaptive,
                "{=2Fr6cd4JG}Piety gain is increased by +0.2 daily.",
                SkillEffect.PerkRole.Personal, 0.2f,
                SkillEffect.EffectIncrementType.Add,
                "{=y8tA4gFtQ}Religious notables' volunteers may be recruited.",
                SkillEffect.PerkRole.Ruler, 1f,
                SkillEffect.EffectIncrementType.Add);

            TheologyBlessed = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("TheologyBlessed"));
            TheologyBlessed.InitializeNew("{=fpZGArKcd}Traditionalist", BKSkills.Instance.Lordship, GetTierCost(2),
                LordshipAdaptive,
                "{=DOuJeyOwp}Blessings last a season longer.",
                SkillEffect.PerkRole.Personal, 0.2f,
                SkillEffect.EffectIncrementType.Add,
                "{=xkTbwwCJh}Blessings cost 10% less piety.",
                SkillEffect.PerkRole.Ruler, 1f,
                SkillEffect.EffectIncrementType.Add);

            TheologyReligiousTeachings = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("TheologyReligiousTeachings"));
            TheologyReligiousTeachings.InitializeNew("{=fpZGArKcd}Traditionalist", BKSkills.Instance.Lordship, GetTierCost(3),
                LordshipAdaptive,
                "{=yixvUSwvg}Children receive 1 extra Wisdom when becoming adults.",
                SkillEffect.PerkRole.Personal, 0.2f,
                SkillEffect.EffectIncrementType.Add,
                "{=vK6y2h6Jp}Daily experience points in Theology for companions and family in party.",
                SkillEffect.PerkRole.Ruler, 1f,
                SkillEffect.EffectIncrementType.Add);

            TheologyRitesOfPassage = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("TheologyRitesOfPassage"));
            TheologyPreacher = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("TheologyPreacher"));

            TheologyPreacher.InitializeNew("{=vmEN7CqvE}Preacher", BKSkills.Instance.Lordship, GetTierCost(4),
                TheologyRitesOfPassage,
                "{=XGznkJyaL}Settlement religious tensions reduced by X%.",
                SkillEffect.PerkRole.Personal, 0.2f,
                SkillEffect.EffectIncrementType.Add,
                "{=owbeNCSNx}Settlement conversion speed increased by X%.",
                SkillEffect.PerkRole.Ruler, 1f,
                SkillEffect.EffectIncrementType.Add);

            TheologyRitesOfPassage.InitializeNew("{=8O4MbEik9}Rites Of Passage", BKSkills.Instance.Lordship, GetTierCost(4),
                TheologyPreacher,
                "{=sNocET9tQ}Rites can be performed again 1 season sooner.",
                SkillEffect.PerkRole.Personal, 0.2f,
                SkillEffect.EffectIncrementType.Add,
                "{=eCmmmtica}Rites yield 5 renown.",
                SkillEffect.PerkRole.Ruler, 1f,
                SkillEffect.EffectIncrementType.Add);

            TheologyLithurgy = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("TheologyLithurgy"));
            TheologyLithurgy.InitializeNew("{=vmEN7CqvE}Preacher", BKSkills.Instance.Lordship, GetTierCost(5),
                LordshipAdaptive,
                "{=JNF5jWykG}Randomly receive relations with religious notables in your settlements.",
                SkillEffect.PerkRole.Personal, 0.2f,
                SkillEffect.EffectIncrementType.Add,
                "{=tv32eSyTg}Some unspecified settlement impact",
                SkillEffect.PerkRole.Ruler, 1f,
                SkillEffect.EffectIncrementType.Add);

            #endregion Theology

            #region Lordship

            LordshipTraditionalist = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LordshipTraditionalist"));
            LordshipAdaptive = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LordshipAdaptive"));
            LordshipTraditionalist.InitializeNew("{=fpZGArKcd}Traditionalist", BKSkills.Instance.Lordship, GetTierCost(1),
                LordshipAdaptive,
                "{=6sSD83UuR}Increased cultural assimilation speed by 10%",
                SkillEffect.PerkRole.Ruler, 0.1f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=umfZG0vY6}Increased militarism in assimilated settlements by flat 1%",
                SkillEffect.PerkRole.Ruler, 1f,
                SkillEffect.EffectIncrementType.Add);

            LordshipAdaptive.InitializeNew("{=VV8JYfVyF}Adaptive", BKSkills.Instance.Lordship, GetTierCost(1),
                LordshipTraditionalist,
                "{=p5SiT32SQ}Reduced loyalty bonus from different cultures by 15%",
                SkillEffect.PerkRole.Ruler, 0.1f,
                SkillEffect.EffectIncrementType.Add,
                "{=ywAFQNWF8}Increased settlement stability target by flat 2%",
                SkillEffect.PerkRole.Ruler, 1f,
                SkillEffect.EffectIncrementType.Add);

            LordshipAccolade = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LordshipAccolade"));
            LordshipManorLord = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LordshipManorLord"));
            LordshipAccolade.InitializeNew("{=s278D7wZo}Accolade", BKSkills.Instance.Lordship, GetTierCost(2), LordshipManorLord,
                "{=nZxWStDGT}Knighting requires 15% less influence",
                SkillEffect.PerkRole.Ruler, -0.15f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=knpty6Xqw}Vassal limit increased by 1",
                SkillEffect.PerkRole.Ruler, 1f,
                SkillEffect.EffectIncrementType.Add);

            LordshipManorLord.InitializeNew("{=3PKUDhKRY}Manor Lord", BKSkills.Instance.Lordship, GetTierCost(2), LordshipAccolade,
                "{=EYcMp8suc}Villages weigh 20% less in demesne limit",
                SkillEffect.PerkRole.Ruler, -0.20f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=FveW1QGGn}Manors provide extra flat 0.2 influence",
                SkillEffect.PerkRole.ClanLeader, 0.2f,
                SkillEffect.EffectIncrementType.Add);

            LordshipMilitaryAdministration = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LordshipMilitaryAdministration"));
            LordshipEconomicAdministration = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LordshipEconomicAdministration"));
            LordshipMilitaryAdministration.InitializeNew("{=gM8Ud0dYq}Military Administration", BKSkills.Instance.Lordship,
                GetTierCost(3), LordshipEconomicAdministration,
                "{=R8SKYrc4a}Increased settlement militarism in settlements by flat 2%",
                SkillEffect.PerkRole.Ruler, 0.02f,
                SkillEffect.EffectIncrementType.Add,
                "{=fXcd0GzJo}Increased settlement drafting speed by 20%",
                SkillEffect.PerkRole.Ruler, 0.2f,
                SkillEffect.EffectIncrementType.AddFactor);

            LordshipEconomicAdministration.InitializeNew("{=fS85GebfM}Economic Administration", BKSkills.Instance.Lordship,
                GetTierCost(3), LordshipMilitaryAdministration,
                "{=oBFvteYSA}Increased settlement production efficiency by 10%",
                SkillEffect.PerkRole.Ruler, 0.2f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=bo9cGoifs}Increased settlement production quality by 5%",
                SkillEffect.PerkRole.Ruler, 0.1f,
                SkillEffect.EffectIncrementType.AddFactor);

            LordshipClaimant = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LordshipClaimant"));
            LordshipPatron = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LordshipPatron"));
            LordshipClaimant.InitializeNew("{=nnTWoxW4o}Claimant", BKSkills.Instance.Lordship, GetTierCost(4), LordshipPatron,
                "{=7gDYHY7hH}Claims are built 30% faster",
                SkillEffect.PerkRole.Ruler, 0.3f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=9zvRdiVsQ}Hostile actions (claim, usurp & revoke) are 5% cheaper",
                SkillEffect.PerkRole.Ruler, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor);

            LordshipPatron.InitializeNew("{=rfKzieduc}Patron", BKSkills.Instance.Lordship, GetTierCost(4), LordshipClaimant,
                "{=8rfmbayQs}Grating titles yields renown",
                SkillEffect.PerkRole.Ruler, 0.2f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=pRUriwZtC}Amicable actions (grant, negotiate) yield more positive relation",
                SkillEffect.PerkRole.Ruler, 0.1f,
                SkillEffect.EffectIncrementType.AddFactor);

            #endregion Lordship

            #region Scholarship

            ScholarshipLiterate = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipLiterate"));
            ScholarshipLiterate.InitializeNew("{=rYAXBM4A5}Literate", BKSkills.Instance.Scholarship, GetTierCost(1), null,
                "{=NNEHfTB0H}Allows reading books", 
                SkillEffect.PerkRole.Personal, 0f,
                SkillEffect.EffectIncrementType.Invalid, 
                string.Empty,
                SkillEffect.PerkRole.None, 0f,
                SkillEffect.EffectIncrementType.Invalid);

            ScholarshipAvidLearner = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipLearner"));
            ScholarshipAvidLearner.InitializeNew("{=ngkSHd3Dq}Avid Learner", BKSkills.Instance.Scholarship, GetTierCost(2), null,
                "{=6mSsRrdZV}Increase language learning rate",
                SkillEffect.PerkRole.Personal, 20f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=U9YtAhttr}Language limit is increased by 1",
                SkillEffect.PerkRole.Personal, 1f,
                SkillEffect.EffectIncrementType.Add);

            ScholarshipTutor = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipTutor"));
            ScholarshipTutor.InitializeNew("{=KFjHxxNRD}Tutor", BKSkills.Instance.Scholarship, GetTierCost(3), null,
                "{=2vVciAsbA}Additional attribute point to clan children coming of age.",
                SkillEffect.PerkRole.ClanLeader, 1f,
                SkillEffect.EffectIncrementType.Add,
                "{=63MySL4Ny}Extra experience gain for companions and family members in party",
                SkillEffect.PerkRole.PartyLeader, 5f,
                SkillEffect.EffectIncrementType.AddFactor);

            ScholarshipWellRead = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipWellRead"));
            ScholarshipWellRead.InitializeNew("{=3Xrmwdf6f}Well Read", BKSkills.Instance.Scholarship, GetTierCost(4), null,
                "{=C4myKfRRY}Increased reading rates for books",
                SkillEffect.PerkRole.Personal, 12f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=O2a5VnD7X}Cultural fascination progresses faster",
                SkillEffect.PerkRole.Personal, 10f,
                SkillEffect.EffectIncrementType.AddFactor);

            ScholarshipAccountant = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipAccountant"));
            ScholarshipMechanic = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipMechanic"));
            ScholarshipMechanic.InitializeNew("{=7vcBynzCA}Mechanic", BKSkills.Instance.Scholarship, GetTierCost(5),
                ScholarshipAccountant,
                "{=8M4E0Gau9}Engineering skill tree yields both perks rather than 1",
                SkillEffect.PerkRole.Personal, 0f,
                SkillEffect.EffectIncrementType.Invalid,
                string.Empty,
                SkillEffect.PerkRole.None, 0f,
                SkillEffect.EffectIncrementType.Invalid);

            ScholarshipAccountant.InitializeNew("{=iLyO7xDFv}Accountant", BKSkills.Instance.Scholarship, GetTierCost(5),
                ScholarshipMechanic,
                "{=BT0avPdYo}Stewardship skill tree yields both perks rather than 1",
                SkillEffect.PerkRole.Personal, 0f,
                SkillEffect.EffectIncrementType.Invalid,
                string.Empty,
                SkillEffect.PerkRole.None, 0f,
                SkillEffect.EffectIncrementType.Invalid);

            ScholarshipTeacher = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipTeacher"));
            ScholarshipTeacher.InitializeNew("{=1F8tDoRUR}Teacher", BKSkills.Instance.Scholarship, GetTierCost(6), null,
                "{=xZJADsQmv}Additional focus points to children coming of age",
                SkillEffect.PerkRole.ClanLeader, 2f,
                SkillEffect.EffectIncrementType.Add,
                "{=!}",
                SkillEffect.PerkRole.None, 10f,
                SkillEffect.EffectIncrementType.AddFactor);

            ScholarshipBookWorm = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipBookWorm"));
            ScholarshipBookWorm.InitializeNew("{=D7XEetUaW}Book Worm", BKSkills.Instance.Scholarship, GetTierCost(7), null,
                "{=C4myKfRRY}Increased reading rates for books",
                SkillEffect.PerkRole.Personal, 20f,
                SkillEffect.EffectIncrementType.Add,
                "{=U9YtAhttr}Language limit is increased by 1",
                SkillEffect.PerkRole.Personal, 1f,
                SkillEffect.EffectIncrementType.Add);
             
            ScholarshipPeerReview = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipPeerReview"));
            ScholarshipPeerReview.InitializeNew("{=JHcJRnGmh}Peer Review", BKSkills.Instance.Scholarship, GetTierCost(8), null,
                "{=ThYBTPUVP}Clan settlements yield more research points",
                SkillEffect.PerkRole.Personal, 20f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=mEvbXToeg}Books yield double skill experience",
                SkillEffect.PerkRole.Personal, 100f,
                SkillEffect.EffectIncrementType.AddFactor);

            ScholarshipBedTimeStory = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipBedTimeStory"));
            ScholarshipBedTimeStory.InitializeNew("{=LUscuSEou}Bed Time Story", BKSkills.Instance.Scholarship, GetTierCost(9), null,
                "{=1B89Whgw3}Daily experience points in random skill for companions and family in party",
                SkillEffect.PerkRole.PartyLeader, 10f,
                SkillEffect.EffectIncrementType.Add,
                string.Empty,
                SkillEffect.PerkRole.Personal, 100f,
                SkillEffect.EffectIncrementType.AddFactor);

            ScholarshipTreasurer = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipTreasurer"));
            ScholarshipNaturalScientist = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipNaturalScientist"));
            ScholarshipTreasurer.InitializeNew("{=GxKoMD9qG}Treasurer", BKSkills.Instance.Scholarship, GetTierCost(10),
                ScholarshipNaturalScientist,
                "{=3fWX5SRyJ}Trade skill tree yields both perks rather than 1",
                SkillEffect.PerkRole.Personal, 0f,
                SkillEffect.EffectIncrementType.Invalid,
                string.Empty,
                SkillEffect.PerkRole.None, 0f,
                SkillEffect.EffectIncrementType.Invalid);

            ScholarshipNaturalScientist.InitializeNew("{=OQH7oheA0}Natural Scientist", BKSkills.Instance.Scholarship,
                GetTierCost(10), ScholarshipTreasurer,
                "{=DSKpGCoz1}Medicine skill tree yields both perks rather than 1",
                SkillEffect.PerkRole.Personal, 0f,
                SkillEffect.EffectIncrementType.Invalid,
                string.Empty,
                SkillEffect.PerkRole.None, 0f,
                SkillEffect.EffectIncrementType.Invalid);

            ScholarshipPolyglot = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipPolyglot"));
            ScholarshipPolyglot.InitializeNew("{=LdovS43ad}Polyglot", BKSkills.Instance.Scholarship, GetTierCost(11), null,
                "{=WgKk9sBsg}Language limit is increased by 2", SkillEffect.PerkRole.Personal, 10f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=CzvhCbDLu}Language learning is significantly increased",
                SkillEffect.PerkRole.None, 0f,
                SkillEffect.EffectIncrementType.Invalid);

            ScholarshipMagnumOpus = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipMagnumOpus"));
            ScholarshipMagnumOpus.InitializeNew("{=3gwf5oMxE}Magnum Opus", BKSkills.Instance.Scholarship, GetTierCost(11), null,
                "{=HbmfXPqi5}+0.2% experience gain for every skill point in Scholarship above 230",
                SkillEffect.PerkRole.Personal, 0.2f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=PW7UEd8MZ}Focus points add 50% more learning limit",
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