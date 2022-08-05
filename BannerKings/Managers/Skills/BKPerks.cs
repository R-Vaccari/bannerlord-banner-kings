using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace BannerKings.Managers.Skills
{
    public class BKPerks : DefaultTypeInitializer<BKPerks, PerkObject>
    {

        public HashSet<PerkObject> LifestylePerks { get; private set; } = new HashSet<PerkObject>();

        private PerkObject scholarshipLiterate, scholarshipPolyglot, scholarshipLearner,
            scholarshipTutor, scholarshipWellRead, scholarshipMechanic, scholarshipAccountant, scholarshipTeacher, scholarshipBookWorm,
            scholarshipPeerReview, scholarshipBedtimeStory, scholarshipScientist, scholarshipTreasurer, scholarshipMagnumOpus;

        private PerkObject lordshipTraditionalist, lordshipAdaptive, lordshipAccolade, lordshipManorLord,
            lordshipMilitaryAdministration, lordshipEconomicAdministration, lordshipClaimant, lordshipPatron;

        private PerkObject fianHighlander, fianRanger, fianFennid, civilEngineer, civilCultivator, civilOverseer, civilManufacturer,
            siegeEngineer, siegePlanner, siegeOverseer, augustCommander, augustDeFacto, augustDeJure, augustKingOfKings,
            cataphractEquites, cataphractAdaptiveTactics, cataphractKlibanophori, caravaneerStrider, caravaneerDealer,
            caravaneerEntrepeneur, outlawKidnapper, outlawPlunderer, outlawNightPredator, outlawUnderworldKing,
            kheshigKhorchin, kheshigTorguud, kheshigKhevtuul;

        public PerkObject FianHighlander => fianHighlander;
        public PerkObject FianRanger => fianRanger;
        public PerkObject FianFennid => fianFennid;

        public PerkObject CivilEngineer => civilEngineer;
        public PerkObject CivilCultivator => civilCultivator;
        public PerkObject CivilManufacturer => civilManufacturer;
        public PerkObject CivilOverseer => civilOverseer;


        public PerkObject SiegeEngineer => siegeEngineer;
        public PerkObject SiegePlanner => siegePlanner;
        public PerkObject SiegeOverseer => siegeOverseer;

        public PerkObject AugustCommander => augustCommander;
        public PerkObject AugustDeFacto => augustDeFacto;
        public PerkObject AugustDeJure => augustDeJure;
        public PerkObject AugustKingOfKings => augustKingOfKings;

        public PerkObject CataphractEquites => cataphractEquites;
        public PerkObject CataphractAdaptiveTactics => cataphractAdaptiveTactics;
        public PerkObject CataphractKlibanophoros => cataphractKlibanophori;

        public PerkObject CaravaneerStrider => caravaneerStrider;
        public PerkObject CaravaneerDealer => caravaneerDealer;
        public PerkObject CaravaneerEntrepeneur => caravaneerEntrepeneur;

        public PerkObject OutlawKidnapper => outlawKidnapper;
        public PerkObject OutlawPlunderer => outlawPlunderer;
        public PerkObject OutlawNightPredator => outlawNightPredator;


        public PerkObject LordshipEconomicAdministration => lordshipEconomicAdministration;
        public PerkObject ScholarshipLiterate => scholarshipLiterate;
        public PerkObject ScholarshipAvidLearner => scholarshipLearner;
        public PerkObject ScholarshipTutor => scholarshipTutor;
        public PerkObject ScholarshipWellRead => scholarshipWellRead;
        public PerkObject ScholarshipTeacher => scholarshipTeacher;
        public PerkObject ScholarshipBookWorm => scholarshipBookWorm;
        public PerkObject ScholarshipPeerReview => scholarshipPeerReview;
        public PerkObject ScholarshipBedTimeStory => scholarshipBedtimeStory;
        public PerkObject ScholarshipPolyglot=> scholarshipPolyglot;
        public PerkObject ScholarshipMechanic => scholarshipMechanic;
        public PerkObject ScholarshipAccountant => scholarshipAccountant;
        public PerkObject ScholarshipNaturalScientist => scholarshipScientist;
        public PerkObject ScholarshipTreasurer => scholarshipTreasurer;
        public PerkObject ScholarshipMagnumOpus => scholarshipMagnumOpus;

        private void InitializePerks()
        {
            fianHighlander = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleFianHighlander"));
            LifestylePerks.Add(fianHighlander);
            fianHighlander.InitializeNew("{=!}Highlander", null, 75, null,
                "{=!}Increases your movement speed by 5%.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=!}Increases your swing speed with two handed swords by 6%.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor,
                TroopClassFlag.None, TroopClassFlag.None);

            fianRanger = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleFianRanger"));
            LifestylePerks.Add(fianRanger);
            fianRanger.InitializeNew("{=!}Ranger", null, 150, null,
                "{=5NRai8K5}Increase maximum track life by 20%.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=XzbpZji3}Increases your damage with bows by 8%.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor,
                TroopClassFlag.None, TroopClassFlag.None);

            fianFennid = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleFianFennid"));
            LifestylePerks.Add(fianFennid);
            fianFennid.InitializeNew("{=!}Fénnid", null, 225, null,
                "{=MOITVVNu}Aiming with your bow is 25% faster.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=lOaVRxdR}Increases your two handed weapon damage by 10%.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor,
                TroopClassFlag.None, TroopClassFlag.None);

            civilEngineer = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleCivilEngineer"));
            LifestylePerks.Add(civilEngineer);
            civilEngineer.InitializeNew("{=!}Civil Engineer", null, 75, null,
                "{=!}Settlements have an additional catapult during siege start.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=!}Workforce yields 20% extra construction.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor,
                TroopClassFlag.None, TroopClassFlag.None);

            civilCultivator = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleCivilCultivator"));
            LifestylePerks.Add(civilCultivator);
            civilCultivator.InitializeNew("{=!}Cultivator", null, 150, null,
                "{=!}Agricultural yield increases by flat 5%.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=!}Village hearth growth increases by 1.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor,
                TroopClassFlag.None, TroopClassFlag.None);

            civilOverseer = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleCivilOverseer"));
            civilManufacturer = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleCivilManufacturer"));

            LifestylePerks.Add(civilOverseer);
            civilOverseer.InitializeNew("{=!}Overseer", null, 300, null,
                "{=!}Stability increases by flat 5%.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=!}Increases infrastructure limit by flat 5.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor,
                TroopClassFlag.None, TroopClassFlag.None);

            LifestylePerks.Add(civilManufacturer);
            civilManufacturer.InitializeNew("{=!}Manufacturer", null, 225, null,
                "{=!}Production efficiency increases by flat 15%.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=!}Production quality increases by flat 10%.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor,
                TroopClassFlag.None, TroopClassFlag.None);


            siegeEngineer = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleSiegeEngineer"));
            LifestylePerks.Add(siegeEngineer);
            siegeEngineer.InitializeNew("{=!}Siege Engineer", null, 75, null,
                "{=!}Get a pre-built ballista as attacker during siege.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=!}Damage to walls increased by 10% during siege.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor,
                TroopClassFlag.None, TroopClassFlag.None);

            siegePlanner = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleSiegePlanner"));
            LifestylePerks.Add(siegePlanner);
            siegePlanner.InitializeNew("{=!}Siege Planner", null, 150, null,
                "{=!}Ranged infantry deals 15% more damage in siege simulations.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=rkKMB9qK}Wall hit points are increased by 25%.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor,
                TroopClassFlag.None, TroopClassFlag.None);

            siegeOverseer = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleSiegeOverseer"));
            LifestylePerks.Add(siegeOverseer);
            siegeOverseer.InitializeNew("{=!}Siege Overseer", null, 225, null,
                "{=!}Army consumes 15% less food during sieges, either attacking or defending.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=ta2z0bEv}Camp preparation is 20% faster.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor,
                TroopClassFlag.None, TroopClassFlag.None);



            augustCommander = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleAugustCommander"));
            LifestylePerks.Add(augustCommander);
            augustCommander.InitializeNew("{=!}Commander", null, 75, null,
                "{=920FKjJk}Increases your party size by 5.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=!}Increases party morale by flat 3%.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor,
                TroopClassFlag.None, TroopClassFlag.None);


            augustDeFacto = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleAugustDeFacto"));
            LifestylePerks.Add(augustDeFacto);
            augustDeFacto.InitializeNew("{=!}De Facto", null, 150, null,
                "{=!}Settlement autonomy reduced by flat 3%.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=!}Randomly receive positive relations with a councillour.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor,
                TroopClassFlag.None, TroopClassFlag.None);

            augustDeJure = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleAugustDeJure"));
            LifestylePerks.Add(augustDeJure);
            augustDeJure.InitializeNew("{=!}De Jure", null, 225, null,
                "{=!}Demesne limit increased by 1.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=!}Title actions cost / yield 5% less / more denarii and influence.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor,
                TroopClassFlag.None, TroopClassFlag.None);

            augustKingOfKings = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleAugustKingOfKings"));
            LifestylePerks.Add(augustKingOfKings);
            augustKingOfKings.InitializeNew("{=!}King of Kings", null, 300, null,
                "{=!}If king level or higher, increase vassal limit by 2.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=!}If king level or higher, increase unlanded demesne limit by 1.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor,
                TroopClassFlag.None, TroopClassFlag.None);



            cataphractEquites = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleCataphractEquites"));
            LifestylePerks.Add(cataphractEquites);
            cataphractEquites.InitializeNew("{=!}Equites", null, 75, null,
                "{=!}You and troops in your formation deal 10% more charge damage.",
                SkillEffect.PerkRole.Captain, 4f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=!}Mounted troops cost 10% less denarii maintenance.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor,
                TroopClassFlag.None, TroopClassFlag.None);


            cataphractAdaptiveTactics = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleCataphractAdaptiveTactics"));
            LifestylePerks.Add(cataphractAdaptiveTactics);
            cataphractAdaptiveTactics.InitializeNew("{=!}Adaptive Tactics", null, 150, null,
                "{=!}Increased damage on horseback with polearms, sidearms and bows by 5%.",
                SkillEffect.PerkRole.Personal, 5f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=!}You and troops in your formation have 8% more maneuvering.",
                SkillEffect.PerkRole.Captain, 8f,
                SkillEffect.EffectIncrementType.AddFactor,
                TroopClassFlag.None, TroopClassFlag.None);

            cataphractKlibanophori = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleCataphractKlibanophori"));
            LifestylePerks.Add(cataphractKlibanophori);
            cataphractKlibanophori.InitializeNew("{=!}Klibanophori", null, 225, null,
                "{=!}You and troops in your formation receive 5% less damange when mounted.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=!}You and troops in your formation deal 6% extra thrust damage when mounted.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor,
                TroopClassFlag.None, TroopClassFlag.None);



            caravaneerStrider = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleCaravaneerStrider"));
            LifestylePerks.Add(caravaneerStrider);
            caravaneerStrider.InitializeNew("{=!}Strider", null, 75, null,
                "{=!}Increases your movement speed by 3%.",
                SkillEffect.PerkRole.PartyLeader, 3f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=ulgd9Veb}Increases carry capacity of pack animals by 20%.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor,
                TroopClassFlag.None, TroopClassFlag.None);


            caravaneerDealer = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleCaravaneerDealer"));
            LifestylePerks.Add(caravaneerDealer);
            caravaneerDealer.InitializeNew("{=!}Dealer", null, 150, null,
                "{=!}Caravan wages are reduced by 10%.",
                SkillEffect.PerkRole.PartyOwner, 10f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=!}Your caravans move 4% faster during daytime.",
                SkillEffect.PerkRole.Captain, 8f,
                SkillEffect.EffectIncrementType.AddFactor,
                TroopClassFlag.None, TroopClassFlag.None);

            caravaneerEntrepeneur = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleCaravaneerEntrepeneur"));
            LifestylePerks.Add(caravaneerEntrepeneur);
            caravaneerEntrepeneur.InitializeNew("{=!}Entrepeneur", null, 225, null,
                "{=!}Increased settlement production efficiency by flat 10%.",
                SkillEffect.PerkRole.Personal, 10f,
                SkillEffect.EffectIncrementType.Add,
                "{=!}You pay 20% less workshop taxes to other clans. Your settlements tax others' workshops 20% more.",
                SkillEffect.PerkRole.Personal, 20f,
                SkillEffect.EffectIncrementType.AddFactor,
                TroopClassFlag.None, TroopClassFlag.None);



            outlawKidnapper = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleOutlawKidnapper"));
            LifestylePerks.Add(outlawKidnapper);
            outlawKidnapper.InitializeNew("{=!}Kidnapper", null, 75, null,
                "{=!}30% better deals reansoming lords.",
                SkillEffect.PerkRole.PartyLeader, 3f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=!}Decreases the duration of the disorganized state after breaking sieges and raids by 30%.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor,
                TroopClassFlag.None, TroopClassFlag.None);


            outlawPlunderer = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleOutlawPlunderer"));
            LifestylePerks.Add(outlawPlunderer);
            outlawPlunderer.InitializeNew("{=!}Infamous Plunderer", null, 150, null,
                "{=!}Bandit troops in your party yield influence.",
                SkillEffect.PerkRole.PartyOwner, 10f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=!}Raiding villages is 15% faster.",
                SkillEffect.PerkRole.Captain, 8f,
                SkillEffect.EffectIncrementType.AddFactor,
                TroopClassFlag.None, TroopClassFlag.None);

            outlawNightPredator = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleOutlawNightPredator"));
            LifestylePerks.Add(outlawNightPredator);
            outlawNightPredator.InitializeNew("{=!}Night Predator", null, 225, null,
                "{=!}Your party is 50% harder to spot in forests.",
                SkillEffect.PerkRole.Personal, 10f,
                SkillEffect.EffectIncrementType.Add,
                "{=!}Increased nighttime movement by 6%.",
                SkillEffect.PerkRole.Personal, 20f,
                SkillEffect.EffectIncrementType.AddFactor,
                TroopClassFlag.None, TroopClassFlag.None);

            outlawNightPredator = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleOutlawUnderworldKing"));
            LifestylePerks.Add(outlawNightPredator);
            outlawNightPredator.InitializeNew("{=!}Underworld King", null, 300, null,
                "{=!}Killing bandit leaders yields renown.",
                SkillEffect.PerkRole.Personal, 10f,
                SkillEffect.EffectIncrementType.Add,
                "{=!}",
                SkillEffect.PerkRole.Personal, 20f,
                SkillEffect.EffectIncrementType.AddFactor,
                TroopClassFlag.None, TroopClassFlag.None);
        }




        public override void Initialize()
        {
            InitializePerks();

            lordshipTraditionalist = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LordshipTraditionalist"));
            lordshipAdaptive = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LordshipAdaptive"));

            lordshipTraditionalist.InitializeNew("{=!}Traditionalist", BKSkills.Instance.Lordship, GetTierCost(1), lordshipAdaptive,
                "{=!}Increased cultural assimilation speed by 10%",
                SkillEffect.PerkRole.Ruler, 0.1f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=!}Increased militarism in assimilated settlements by flat 1%",
                SkillEffect.PerkRole.Ruler, 1f,
                SkillEffect.EffectIncrementType.Add,
                TroopClassFlag.None, TroopClassFlag.None);

            lordshipAdaptive.InitializeNew("{=!}Adaptive", BKSkills.Instance.Lordship, GetTierCost(1), lordshipTraditionalist,
                "{=!}Reduced loyalty onus from different cultures by 15%",
                SkillEffect.PerkRole.Ruler, 0.1f,
                SkillEffect.EffectIncrementType.Add,
                "{=!}Increased settlement stability target by flat 2%",
                SkillEffect.PerkRole.Ruler, 1f,
                SkillEffect.EffectIncrementType.Add,
                TroopClassFlag.None, TroopClassFlag.None);

            lordshipAccolade = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LordshipAccolade"));
            lordshipManorLord = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LordshipManorLord"));

            lordshipAccolade.InitializeNew("{=!}Accolade", BKSkills.Instance.Lordship, GetTierCost(2), lordshipManorLord,
                "{=!}Knighting requires 15% less influence", 
                SkillEffect.PerkRole.Ruler, -0.15f, 
                SkillEffect.EffectIncrementType.AddFactor, 
                "{=!}Vassal limit increased by 1",
                SkillEffect.PerkRole.Ruler, 1f,
                SkillEffect.EffectIncrementType.Add,
                TroopClassFlag.None, TroopClassFlag.None);

            lordshipManorLord.InitializeNew("{=!}Manor Lord", BKSkills.Instance.Lordship, GetTierCost(2), lordshipAccolade,
                "{=!}Villages weigh 20% less in demesne limit",
                SkillEffect.PerkRole.Ruler, -0.20f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=!}Manors provide extra flat 0.2 influence",
                SkillEffect.PerkRole.ClanLeader, 0.2f,
                SkillEffect.EffectIncrementType.Add,
                TroopClassFlag.None, TroopClassFlag.None);

            lordshipMilitaryAdministration = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LordshipMilitaryAdministration"));
            lordshipEconomicAdministration = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LordshipEconomicAdministration"));

            lordshipMilitaryAdministration.InitializeNew("{=!}Military Administration", BKSkills.Instance.Lordship, GetTierCost(3), lordshipEconomicAdministration,
                "{=!}Increased settlement militarism in settlements by flat 2%",
                SkillEffect.PerkRole.Ruler, 0.02f,
                SkillEffect.EffectIncrementType.Add,
                "{=!}Increased settlement drafting speed by 20%",
                SkillEffect.PerkRole.Ruler, 0.2f,
                SkillEffect.EffectIncrementType.AddFactor,
                TroopClassFlag.None, TroopClassFlag.None);

            lordshipEconomicAdministration.InitializeNew("{=!}Economic Administration", BKSkills.Instance.Lordship, GetTierCost(3), lordshipMilitaryAdministration,
                "{=!}Increased settlement production efficiency by 10%",
                SkillEffect.PerkRole.Ruler, 0.2f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=!}Increased settlement production quality by 5%",
                SkillEffect.PerkRole.Ruler, 0.1f,
                SkillEffect.EffectIncrementType.AddFactor,
                TroopClassFlag.None, TroopClassFlag.None);

            lordshipClaimant = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LordshipClaimant"));
            lordshipPatron = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LordshipPatron"));

            lordshipClaimant.InitializeNew("{=!}Claimant", BKSkills.Instance.Lordship, GetTierCost(4), lordshipPatron,
                "{=!}Claims are built 30% faster",
                SkillEffect.PerkRole.Ruler, 0.3f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=!}Hostile actions (claim, usurp & revoke) are 5% cheaper",
                SkillEffect.PerkRole.Ruler, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                TroopClassFlag.None, TroopClassFlag.None);

            lordshipPatron.InitializeNew("{=!}Patron", BKSkills.Instance.Lordship, GetTierCost(4), lordshipClaimant,
                "{=!}Grating titles yields renown",
                SkillEffect.PerkRole.Ruler, 0.2f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=!}Amicable actions (grant, negotiate) yield more positive relation",
                SkillEffect.PerkRole.Ruler, 0.1f,
                SkillEffect.EffectIncrementType.AddFactor,
                TroopClassFlag.None, TroopClassFlag.None);



            scholarshipLiterate = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipLiterate"));
            scholarshipLiterate.InitializeNew("{=!}Literate", BKSkills.Instance.Scholarship, GetTierCost(1), null,
                "{=!}Allows reading books", SkillEffect.PerkRole.Personal, 0f,
                SkillEffect.EffectIncrementType.Invalid, string.Empty,
                SkillEffect.PerkRole.None, 0f,
                SkillEffect.EffectIncrementType.Invalid,
                TroopClassFlag.None, TroopClassFlag.None);

            scholarshipLearner = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipLearner"));
            scholarshipLearner.InitializeNew("{=!}Avid Learner", BKSkills.Instance.Scholarship, GetTierCost(2), null,
                "{=!}Increase language learning rate", 
                SkillEffect.PerkRole.Personal, 20f,
                SkillEffect.EffectIncrementType.AddFactor, 
                "{=!}Language limit is increased by 1",
                SkillEffect.PerkRole.Personal, 1f,
                SkillEffect.EffectIncrementType.Add,
                TroopClassFlag.None, TroopClassFlag.None);

            scholarshipTutor = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipTutor"));
            scholarshipTutor.InitializeNew("{=!}Tutor", BKSkills.Instance.Scholarship, GetTierCost(3), null,
                "{=!}Additional attribute point to clan children coming of age.", 
                SkillEffect.PerkRole.ClanLeader, 1f,
                SkillEffect.EffectIncrementType.Add, 
                "{=!}Extra experience gain for companions and family members in party",
                SkillEffect.PerkRole.PartyLeader, 5f,
                SkillEffect.EffectIncrementType.AddFactor,
                TroopClassFlag.None, TroopClassFlag.None);

            scholarshipWellRead = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipWellRead"));
            scholarshipWellRead.InitializeNew("{=!}Well Read", BKSkills.Instance.Scholarship, GetTierCost(4), null,
                "{=!}Increased reading rates for books",
                SkillEffect.PerkRole.Personal, 12f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=!}Cultural fascination progresses faster",
                SkillEffect.PerkRole.Personal, 10f,
                SkillEffect.EffectIncrementType.AddFactor,
                TroopClassFlag.None, TroopClassFlag.None);

            
            scholarshipMechanic = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipMechanic"));
            scholarshipAccountant = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipAccountant"));

            scholarshipMechanic.InitializeNew("{=!}Mechanic", BKSkills.Instance.Scholarship, GetTierCost(5), scholarshipAccountant,
                "{=!}Engineering skill tree yields both perks rather than 1",
                SkillEffect.PerkRole.Personal, 0f,
                SkillEffect.EffectIncrementType.Invalid,
                string.Empty,
                SkillEffect.PerkRole.None, 0f,
                SkillEffect.EffectIncrementType.Invalid,
                TroopClassFlag.None, TroopClassFlag.None);

            
            scholarshipAccountant.InitializeNew("{=!}Accountant", BKSkills.Instance.Scholarship, GetTierCost(5), scholarshipMechanic,
                "{=!}Stewardship skill tree yields both perks rather than 1",
                SkillEffect.PerkRole.Personal, 0f,
                SkillEffect.EffectIncrementType.Invalid,
                string.Empty,
                SkillEffect.PerkRole.None, 0f,
                SkillEffect.EffectIncrementType.Invalid,
                TroopClassFlag.None, TroopClassFlag.None);

            scholarshipTeacher = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipTeacher"));
            scholarshipTeacher.InitializeNew("{=!}Teacher", BKSkills.Instance.Scholarship, GetTierCost(6), null,
                "{=!}Additional focus points to children coming of age",
                SkillEffect.PerkRole.ClanLeader, 2f,
                SkillEffect.EffectIncrementType.Add,
                "{=!}",
                SkillEffect.PerkRole.None, 10f,
                SkillEffect.EffectIncrementType.AddFactor,
                TroopClassFlag.None, TroopClassFlag.None);

            scholarshipBookWorm = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipBookWorm"));
            scholarshipBookWorm.InitializeNew("{=!}Book Worm", BKSkills.Instance.Scholarship, GetTierCost(7), null,
                "{=!}Increased reading rates for books",
                SkillEffect.PerkRole.Personal, 20f,
                SkillEffect.EffectIncrementType.Add,
                "{=!}Language limit is increased by 1",
                SkillEffect.PerkRole.Personal, 1f,
                SkillEffect.EffectIncrementType.Add,
                TroopClassFlag.None, TroopClassFlag.None);

            scholarshipPeerReview = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipPeerReview"));
            scholarshipPeerReview.InitializeNew("{=!}Peer Review", BKSkills.Instance.Scholarship, GetTierCost(8), null,
                "{=!}Clan settlements yield more research points",
                SkillEffect.PerkRole.Personal, 20f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=!}Books yield double skill experience",
                SkillEffect.PerkRole.Personal, 100f,
                SkillEffect.EffectIncrementType.AddFactor,
                TroopClassFlag.None, TroopClassFlag.None);

            scholarshipBedtimeStory = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipBedTimeStory"));
            scholarshipBedtimeStory.InitializeNew("{=!}Bed Time Story", BKSkills.Instance.Scholarship, GetTierCost(9), null,
                "{=!}Daily experience points in random skill for companions and family in party",
                SkillEffect.PerkRole.PartyLeader, 10f,
                SkillEffect.EffectIncrementType.Add,
                "",
                SkillEffect.PerkRole.Personal, 100f,
                SkillEffect.EffectIncrementType.AddFactor,
                TroopClassFlag.None, TroopClassFlag.None);

            scholarshipTreasurer = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipTreasurer"));
            scholarshipScientist = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipNaturalScientist"));

            scholarshipTreasurer.InitializeNew("{=!}Treasurer", BKSkills.Instance.Scholarship, GetTierCost(10), scholarshipScientist,
                "{=!}Trade skill tree yields both perks rather than 1",
                SkillEffect.PerkRole.Personal, 0f,
                SkillEffect.EffectIncrementType.Invalid,
                string.Empty,
                SkillEffect.PerkRole.None, 0f,
                SkillEffect.EffectIncrementType.Invalid,
                TroopClassFlag.None, TroopClassFlag.None);
            
            scholarshipScientist.InitializeNew("{=!}Natural Scientist", BKSkills.Instance.Scholarship, GetTierCost(10), scholarshipTreasurer,
                "{=!}Medicine skill tree yields both perks rather than 1",
                SkillEffect.PerkRole.Personal, 0f,
                SkillEffect.EffectIncrementType.Invalid,
                string.Empty,
                SkillEffect.PerkRole.None, 0f,
                SkillEffect.EffectIncrementType.Invalid,
                TroopClassFlag.None, TroopClassFlag.None);

            scholarshipPolyglot = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipPolyglot"));
            scholarshipPolyglot.InitializeNew("{=!}Polyglot", BKSkills.Instance.Scholarship, GetTierCost(11), null,
                "{=!}Language limit is increased by 2", SkillEffect.PerkRole.Personal, 10f, 
                SkillEffect.EffectIncrementType.AddFactor,
                "{=!}Language learning is significantly increased", 
                SkillEffect.PerkRole.None, 0f, 
                SkillEffect.EffectIncrementType.Invalid, 
                TroopClassFlag.None, TroopClassFlag.None);

            scholarshipMagnumOpus = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipMagnumOpus"));
            scholarshipMagnumOpus.InitializeNew("{=!}Magnum Opus", BKSkills.Instance.Scholarship, GetTierCost(11), null,
                "{=!}+0.2% experience gain for every skill point in Scholarship above 230", 
                SkillEffect.PerkRole.Personal, 0.2f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=!}Focus points add 50% more learning limit",
                SkillEffect.PerkRole.Personal, 50f,
                SkillEffect.EffectIncrementType.AddFactor,
                TroopClassFlag.None, TroopClassFlag.None);
        }


        private int GetTierCost(int tierIndex) => Requirements[tierIndex - 1];

        private static readonly int[] Requirements = new int[]
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

        public override IEnumerable<PerkObject> All => throw new System.NotImplementedException();
    }
}
