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
            siegeEngineer, siegePlanner;

        public PerkObject FianHighlander => fianHighlander;
        public PerkObject FianRanger => fianRanger;
        public PerkObject FianFennid => fianFennid;

        public PerkObject CivilEngineer => civilEngineer;
        public PerkObject CivilCultivator => civilCultivator;
        public PerkObject CivilManufacturer => civilManufacturer;
        public PerkObject CivilOverseer => civilOverseer;


        public PerkObject SiegeEngineer => siegeEngineer;
        public PerkObject SiegePlanner => siegePlanner;

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

        public override void Initialize()
        {
            fianHighlander = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleFianHighlander"));
            LifestylePerks.Add(fianHighlander);
            fianHighlander.InitializeNew("{=!}Highlander", null, 75, null,
                "{=!}Increases your movement speed by 5%",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=!}Increases your swing speed with two handed swords by 6%",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor,
                TroopClassFlag.None, TroopClassFlag.None);

            fianRanger = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleFianRanger"));
            LifestylePerks.Add(fianRanger);
            fianRanger.InitializeNew("{=!}Ranger", null, 150, null,
                "{=5NRai8K5}Increase maximum track life by 20%",
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
                "{=!}Get a pre-built catapult as attacker during siege.",
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
                "{=ta2z0bEv}Camp preparation is 20% faster..",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor,
                TroopClassFlag.None, TroopClassFlag.None);



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
