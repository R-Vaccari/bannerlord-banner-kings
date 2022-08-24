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
            FianHighlander.InitializeNew("{=WShtRHwcn}Highlander", null, 80, null,
                "{=WODH0Hy5O}Increases your movement speed by 5%.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=cSLHONetV}Increases your swing speed with two handed swords by 6%.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            FianRanger = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleFianRanger"));
            LifestylePerks.Add(FianRanger);
            FianRanger.InitializeNew("{=Hsqn3Xh4r}Ranger", null, 160, null,
                "{=5NRai8K5}Increase maximum track life by 20%.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=XzbpZji3}Increases your damage with bows by 8%.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            FianFennid = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleFianFennid"));
            LifestylePerks.Add(FianFennid);
            FianFennid.InitializeNew("{=jm9Lo85Xx}Fénnid", null, 240, null,
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
            CivilEngineer.InitializeNew("{=df9bA2gXJ}Civil Engineer", null, 80, null,
                "{=kb6GC0QGO}Settlements have an additional catapult during siege start.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=0JSHiv31U}Workforce yields 20% extra construction.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            CivilCultivator = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleCivilCultivator"));
            LifestylePerks.Add(CivilCultivator);
            CivilCultivator.InitializeNew("{=8YHUYYuAf}Cultivator", null, 160, null,
                "{=YG4aXVDV0}Agricultural yield increases by flat 5%.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=ZkQZZjLF1}Village hearth growth increases by 1.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            CivilOverseer = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleCivilOverseer"));
            CivilManufacturer = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleCivilManufacturer"));
            LifestylePerks.Add(CivilOverseer);
            CivilOverseer.InitializeNew("{=2TgNzF8sd}Overseer", null, 320, null,
                "{=tP0uO7XS9}Stability increases by flat 5%.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=err2U9dE2}Increases infrastructure limit by flat 5.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            LifestylePerks.Add(CivilManufacturer);
            CivilManufacturer.InitializeNew("{=2z1XrKE4V}Manufacturer", null, 240, null,
                "{=oCBBkESrs}Production efficiency increases by flat 15%.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=EnVusLCom}Production quality increases by flat 10%.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            #endregion Civil

            #region Siege

            SiegeEngineer = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleSiegeEngineer"));
            LifestylePerks.Add(SiegeEngineer);
            SiegeEngineer.InitializeNew("{=coZ3K6OrR}Siege Engineer", null, 80, null,
                "{=vM5YJ7BwL}Get a pre-built ballista as attacker during siege.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=Fn14FwFHK}Damage to walls increased by 10% during siege.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            SiegePlanner = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleSiegePlanner"));
            LifestylePerks.Add(SiegePlanner);
            SiegePlanner.InitializeNew("{=nNLADxbWS}Siege Planner", null, 160, null,
                "{=hsveqR7Fb}Ranged infantry deals 15% more damage in siege simulations.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=rkKMB9qK}Wall hit points are increased by 25%.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            SiegeOverseer = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleSiegeOverseer"));
            LifestylePerks.Add(SiegeOverseer);
            SiegeOverseer.InitializeNew("{=mizBLp5L0}Siege Overseer", null, 240, null,
                "{=BnvQRwQBx}Army consumes 15% less food during sieges, either attacking or defending.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=ta2z0bEv}Camp preparation is 20% faster.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            #endregion Siege

            #region August

            AugustCommander = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleAugustCommander"));
            LifestylePerks.Add(AugustCommander);
            AugustCommander.InitializeNew("{=PBd8NXta1}Commander", null, 80, null,
                "{=920FKjJk}Increases your party size by 5.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=UZC0dKHrO}Increases party morale by flat 3%.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            AugustDeFacto = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleAugustDeFacto"));
            LifestylePerks.Add(AugustDeFacto);
            AugustDeFacto.InitializeNew("{=HYJ9pLSsR}De Facto", null, 160, null,
                "{=YEMns4nFV}Settlement autonomy reduced by flat 3%.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=YXMcpMWyj}Randomly receive positive relations with a councillour.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            AugustDeJure = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleAugustDeJure"));
            LifestylePerks.Add(AugustDeJure);
            AugustDeJure.InitializeNew("{=BZNyO2zX1}De Jure", null, 240, null,
                "{=A1DWeYy57}Demesne limit increased by 1.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=nnkjvkZ2W}Title actions cost / yield 5% less / more denarii and influence.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            AugustKingOfKings = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleAugustKingOfKings"));
            LifestylePerks.Add(AugustKingOfKings);
            AugustKingOfKings.InitializeNew("{=Gz3WmLUEx}King of Kings", null, 320, null,
                "{=s3MEfqoy9}If king level or higher, increase vassal limit by 2.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=DM9nTMzLM}If king level or higher, increase unlanded demesne limit by 1.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            #endregion August

            #region Cataphract

            CataphractEquites = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleCataphractEquites"));
            LifestylePerks.Add(CataphractEquites);
            CataphractEquites.InitializeNew("{=DOaN48d15}Equites", null, 80, null,
                "{=RJv5eG6e6}You and troops in your formation deal 10% more charge damage.",
                SkillEffect.PerkRole.Captain, 4f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=jaq4Q6PM7}Mounted troops cost 10% less denarii maintenance.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            CataphractAdaptiveTactics = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleCataphractAdaptiveTactics"));
            LifestylePerks.Add(CataphractAdaptiveTactics);
            CataphractAdaptiveTactics.InitializeNew("{=cx1AxtKm0}Adaptive Tactics", null, 160, null,
                "{=CpQoNTG06}Increased damage on horseback with polearms, sidearms and bows by 5%.",
                SkillEffect.PerkRole.Personal, 5f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=zLd9AYQKY}You and troops in your formation have 8% more maneuvering.",
                SkillEffect.PerkRole.Captain, 8f,
                SkillEffect.EffectIncrementType.AddFactor);

            CataphractKlibanophoros = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleCataphractKlibanophori"));
            LifestylePerks.Add(CataphractKlibanophoros);
            CataphractKlibanophoros.InitializeNew("{=7v4GWgnT3}Klibanophori", null, 240, null,
                "{=9gfZHw68B}You and troops in your formation receive 5% less damange when mounted.",
                SkillEffect.PerkRole.Personal, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=XagrEb98S}You and troops in your formation deal 6% extra thrust damage when mounted.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            #endregion Cataphract

            #region Caravaneer

            CaravaneerStrider = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleCaravaneerStrider"));
            LifestylePerks.Add(CaravaneerStrider);
            CaravaneerStrider.InitializeNew("{=1JCa85iYn}Strider", null, 80, null,
                "{=O3u4bRK4y}Increases your movement speed by 3%.",
                SkillEffect.PerkRole.PartyLeader, 3f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=ulgd9Veb}Increases carry capacity of pack animals by 20%.",
                SkillEffect.PerkRole.PartyLeader, 20f,
                SkillEffect.EffectIncrementType.AddFactor);

            CaravaneerDealer = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleCaravaneerDealer"));
            LifestylePerks.Add(CaravaneerDealer);
            CaravaneerDealer.InitializeNew("{=WCZNLj8wb}Dealer", null, 150, null,
                "{=RnRdR8JAX}Caravan wages are reduced by 10%.",
                SkillEffect.PerkRole.PartyOwner, 10f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=U2bTznY4Y}Your caravans move 4% faster during daytime.",
                SkillEffect.PerkRole.PartyOwner, 8f,
                SkillEffect.EffectIncrementType.AddFactor);

            CaravaneerOutsideConnections = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleCaravaneerOutsideConnections"));
            CaravaneerOutsideConnections.InitializeNew("{=jEievG2FP}Outside Connections", null, 240, null,
                "{=EKFRMBGsS}Your caravans have 5% less trade penalty.",
                SkillEffect.PerkRole.PartyOwner, 5f,
                SkillEffect.EffectIncrementType.Add,
                "{=9198sxNW3}Randomly gain relations with merchants where your caravans trade.",
                SkillEffect.PerkRole.PartyOwner, 5f,
                SkillEffect.EffectIncrementType.AddFactor);

            #endregion Caravaneer

            #region Artisan

            ArtisanSmith = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleArtisanSmith"));
            ArtisanSmith.InitializeNew("{=HHFZet71v}Smith", null, 80, null,
                "{=AqyAtMRmF}Crafting items costs 10% less energy.",
                SkillEffect.PerkRole.Personal, 10f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=HHFZet71v}Smithy hourly cost is 15% cheaper.",
                SkillEffect.PerkRole.Personal, 15f,
                SkillEffect.EffectIncrementType.AddFactor);

            ArtisanCraftsman = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleArtisanCraftsman"));
            ArtisanCraftsman.InitializeNew("{=LNYzK4yLx}Craftsman", null, 160, null,
                "{=qUBGhhZYB}Your workshops have 5% increase in production quality.",
                SkillEffect.PerkRole.ClanLeader, 5f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=GaTvTfc5P}You are 5% more likely to craft an item with a better modifier.",
                SkillEffect.PerkRole.Personal, 5f,
                SkillEffect.EffectIncrementType.AddFactor);

            ArtisanEntrepeneur = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleArtisanEntrepeneur"));
            ArtisanEntrepeneur.InitializeNew("{=kpo24fq7y}Entrepeneur", null, 240, null,
                "{=3czO4e7ak}Increased settlement production efficiency by flat 10%.",
                SkillEffect.PerkRole.ClanLeader, 10f,
                SkillEffect.EffectIncrementType.Add,
                "{=mxeaKeaio}You pay 20% less workshop taxes to other clans. Your settlements tax others' workshops 20% more.",
                SkillEffect.PerkRole.ClanLeader, 20f,
                SkillEffect.EffectIncrementType.AddFactor);

            #endregion Artisan

            #region Outlaw

            OutlawKidnapper = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleOutlawKidnapper"));
            LifestylePerks.Add(OutlawKidnapper);
            OutlawKidnapper.InitializeNew("{=a3gQrjsua}Kidnapper", null, 80, null,
                "{=KbVbbDZRy}30% better deals reansoming lords.",
                SkillEffect.PerkRole.PartyLeader, 3f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=OSKPwE6sY}Decreases the duration of the disorganized state after breaking sieges and raids by 30%.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            OutlawPlunderer = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleOutlawPlunderer"));
            LifestylePerks.Add(OutlawPlunderer);
            OutlawPlunderer.InitializeNew("{=8bd2tUGwJ}Infamous Plunderer", null, 160, null,
                "{=p1vPEegZt}Bandit troops in your party yield influence.",
                SkillEffect.PerkRole.PartyOwner, 10f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=YczYSjwcF}Raiding villages is 15% faster.",
                SkillEffect.PerkRole.Captain, 8f,
                SkillEffect.EffectIncrementType.AddFactor);

            OutlawNightPredator = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleOutlawNightPredator"));
            LifestylePerks.Add(OutlawNightPredator);
            OutlawNightPredator.InitializeNew("{=csQap6U17}Night Predator", null, 240, null,
                "{=sYePS4rXd}Your party is 50% harder to spot in forests.",
                SkillEffect.PerkRole.Personal, 10f,
                SkillEffect.EffectIncrementType.Add,
                "{=WpeHEJEgM}Increased nighttime movement by 6%.",
                SkillEffect.PerkRole.Personal, 20f,
                SkillEffect.EffectIncrementType.AddFactor);

            OutlawUnderworldKing = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleOutlawUnderworldKing"));
            LifestylePerks.Add(OutlawUnderworldKing);
            OutlawUnderworldKing.InitializeNew("{=iypmP8YCD}Underworld King", null, 320, null,
                "{=6MuZoxmOF}Killing bandit leaders yields renown.",
                SkillEffect.PerkRole.Personal, 10f,
                SkillEffect.EffectIncrementType.Add,
                "{=kGwNAXMVR}",
                SkillEffect.PerkRole.Personal, 20f,
                SkillEffect.EffectIncrementType.AddFactor);

            #endregion Outlaw

            #region Mercenary

            MercenaryLocalConnections = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleMercenaryLocalConnections"));
            LifestylePerks.Add(MercenaryLocalConnections);
            MercenaryLocalConnections.InitializeNew("{=kGwNAXMVR}Local Connections", null, 80, null,
                "{=kGwNAXMVR}While serving as mercenary, gain the ability to recruit from local minor factions in towns.",
                SkillEffect.PerkRole.PartyLeader, 3f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=kGwNAXMVR}Recruiting mercenary troops is 10% cheaper.",
                SkillEffect.PerkRole.Personal, 0.03f,
                SkillEffect.EffectIncrementType.AddFactor);

            MercenaryRansacker = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleMercenaryRansacker"));
            MercenaryRansacker.InitializeNew("{=kGwNAXMVR}Ransacker", null, 160, null,
                "{=kGwNAXMVR}Gain 10% more share of loot in victories.",
                SkillEffect.PerkRole.PartyOwner, 10f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=YczYSjwcF}Raiding villages is 15% faster.",
                SkillEffect.PerkRole.Captain, 8f,
                SkillEffect.EffectIncrementType.AddFactor);

            MercenaryFamousSellswords = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LifestyleMercenarySellswords"));
            MercenaryFamousSellswords.InitializeNew("{=kGwNAXMVR}Famous Sellswords", null, 240, null,
                "{=kGwNAXMVR}Influence award for army participation increased by 30%.",
                SkillEffect.PerkRole.Personal, 10f,
                SkillEffect.EffectIncrementType.Add,
                "{=kGwNAXMVR}Renown award for victories increased by 20%.",
                SkillEffect.PerkRole.Personal, 20f,
                SkillEffect.EffectIncrementType.AddFactor);

            #endregion Mercenary
        }

        public override void Initialize()
        {
            InitializePerks();

            #region Theology

            TheologyFaithful = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("TheologyFaithful"));
            TheologyFaithful.InitializeNew("{=!}Faithful", BKSkills.Instance.Lordship, GetTierCost(1),
                LordshipAdaptive,
                "{=kGwNAXMVR}Piety gain is increased by +0.2 daily.",
                SkillEffect.PerkRole.Personal, 0.2f,
                SkillEffect.EffectIncrementType.Add,
                "{=kGwNAXMVR}Religious notables' volunteers may be recruited.",
                SkillEffect.PerkRole.Ruler, 1f,
                SkillEffect.EffectIncrementType.Add);

            TheologyBlessed = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("TheologyBlessed"));
            TheologyBlessed.InitializeNew("{=!}Blessed", BKSkills.Instance.Lordship, GetTierCost(2),
                LordshipAdaptive,
                "{=kGwNAXMVR}Blessings last a season longer.",
                SkillEffect.PerkRole.Personal, 0.2f,
                SkillEffect.EffectIncrementType.Add,
                "{=kGwNAXMVR}Blessings cost 10% less piety.",
                SkillEffect.PerkRole.Ruler, 1f,
                SkillEffect.EffectIncrementType.Add);

            TheologyReligiousTeachings = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("TheologyReligiousTeachings"));
            TheologyReligiousTeachings.InitializeNew("{=!}Religious Teachings", BKSkills.Instance.Lordship, GetTierCost(3),
                LordshipAdaptive,
                "{=kGwNAXMVR}Children receive 1 extra Wisdom when becoming adults.",
                SkillEffect.PerkRole.Personal, 0.2f,
                SkillEffect.EffectIncrementType.Add,
                "{=kGwNAXMVR}Daily experience points in Theology for companions and family in party.",
                SkillEffect.PerkRole.Ruler, 1f,
                SkillEffect.EffectIncrementType.Add);

            TheologyRitesOfPassage = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("TheologyRitesOfPassage"));
            TheologyPreacher = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("TheologyPreacher"));

            TheologyPreacher.InitializeNew("{=kGwNAXMVR}Preacher", BKSkills.Instance.Lordship, GetTierCost(4),
                TheologyRitesOfPassage,
                "{=kGwNAXMVR}Settlement religious tensions reduced by X%.",
                SkillEffect.PerkRole.Personal, 0.2f,
                SkillEffect.EffectIncrementType.Add,
                "{=kGwNAXMVR}Settlement conversion speed increased by 5%.",
                SkillEffect.PerkRole.Ruler, 1f,
                SkillEffect.EffectIncrementType.Add);

            TheologyRitesOfPassage.InitializeNew("{=kGwNAXMVR}Rites Of Passage", BKSkills.Instance.Lordship, GetTierCost(4),
                TheologyPreacher,
                "{=kGwNAXMVR}Rites can be performed again 1 season sooner.",
                SkillEffect.PerkRole.Personal, 0.2f,
                SkillEffect.EffectIncrementType.Add,
                "{=kGwNAXMVR}Rites yield 5 renown.",
                SkillEffect.PerkRole.Ruler, 1f,
                SkillEffect.EffectIncrementType.Add);

            TheologyLithurgy = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("TheologyLithurgy"));
            TheologyLithurgy.InitializeNew("{=kGwNAXMVR}Preacher", BKSkills.Instance.Lordship, GetTierCost(5),
                null,
                "{=kGwNAXMVR}Randomly receive relations with religious notables in your settlements.",
                SkillEffect.PerkRole.Personal, 0.2f,
                SkillEffect.EffectIncrementType.Add,
                "{=kGwNAXMVR}Some unspecified settlement impact",
                SkillEffect.PerkRole.Ruler, 1f,
                SkillEffect.EffectIncrementType.Add);

            #endregion Theology

            #region Lordship

            LordshipTraditionalist = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LordshipTraditionalist"));
            LordshipAdaptive = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LordshipAdaptive"));
            LordshipTraditionalist.InitializeNew("{=kGwNAXMVR}Traditionalist", BKSkills.Instance.Lordship, GetTierCost(1),
                LordshipAdaptive,
                "{=kGwNAXMVR}Increased cultural assimilation speed by 10%",
                SkillEffect.PerkRole.Ruler, 0.1f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=kGwNAXMVR}Increased militarism in assimilated settlements by flat 1%",
                SkillEffect.PerkRole.Ruler, 1f,
                SkillEffect.EffectIncrementType.Add);

            LordshipAdaptive.InitializeNew("{=kGwNAXMVR}Adaptive", BKSkills.Instance.Lordship, GetTierCost(1),
                LordshipTraditionalist,
                "{=kGwNAXMVR}Reduced loyalty bonus from different cultures by 15%",
                SkillEffect.PerkRole.Ruler, 0.1f,
                SkillEffect.EffectIncrementType.Add,
                "{=kGwNAXMVR}Increased settlement stability target by flat 2%",
                SkillEffect.PerkRole.Ruler, 1f,
                SkillEffect.EffectIncrementType.Add);

            LordshipAccolade = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LordshipAccolade"));
            LordshipManorLord = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LordshipManorLord"));
            LordshipAccolade.InitializeNew("{=kGwNAXMVR}Accolade", BKSkills.Instance.Lordship, GetTierCost(2), LordshipManorLord,
                "{=kGwNAXMVR}Knighting requires 15% less influence",
                SkillEffect.PerkRole.Ruler, -0.15f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=kGwNAXMVR}Vassal limit increased by 1",
                SkillEffect.PerkRole.Ruler, 1f,
                SkillEffect.EffectIncrementType.Add);

            LordshipManorLord.InitializeNew("{=kGwNAXMVR}Manor Lord", BKSkills.Instance.Lordship, GetTierCost(2), LordshipAccolade,
                "{=kGwNAXMVR}Villages weigh 20% less in demesne limit",
                SkillEffect.PerkRole.Ruler, -0.20f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=kGwNAXMVR}Manors provide extra flat 0.2 influence",
                SkillEffect.PerkRole.ClanLeader, 0.2f,
                SkillEffect.EffectIncrementType.Add);

            LordshipMilitaryAdministration = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LordshipMilitaryAdministration"));
            LordshipEconomicAdministration = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LordshipEconomicAdministration"));
            LordshipMilitaryAdministration.InitializeNew("{=kGwNAXMVR}Military Administration", BKSkills.Instance.Lordship,
                GetTierCost(3), LordshipEconomicAdministration,
                "{=kGwNAXMVR}Increased settlement militarism in settlements by flat 2%",
                SkillEffect.PerkRole.Ruler, 0.02f,
                SkillEffect.EffectIncrementType.Add,
                "{=kGwNAXMVR}Increased settlement drafting speed by 20%",
                SkillEffect.PerkRole.Ruler, 0.2f,
                SkillEffect.EffectIncrementType.AddFactor);

            LordshipEconomicAdministration.InitializeNew("{=kGwNAXMVR}Economic Administration", BKSkills.Instance.Lordship,
                GetTierCost(3), LordshipMilitaryAdministration,
                "{=kGwNAXMVR}Increased settlement production efficiency by 10%",
                SkillEffect.PerkRole.Ruler, 0.2f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=kGwNAXMVR}Increased settlement production quality by 5%",
                SkillEffect.PerkRole.Ruler, 0.1f,
                SkillEffect.EffectIncrementType.AddFactor);

            LordshipClaimant = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LordshipClaimant"));
            LordshipPatron = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("LordshipPatron"));
            LordshipClaimant.InitializeNew("{=kGwNAXMVR}Claimant", BKSkills.Instance.Lordship, GetTierCost(4), LordshipPatron,
                "{=kGwNAXMVR}Claims are built 30% faster",
                SkillEffect.PerkRole.Ruler, 0.3f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=kGwNAXMVR}Hostile actions (claim, usurp & revoke) are 5% cheaper",
                SkillEffect.PerkRole.Ruler, 0.05f,
                SkillEffect.EffectIncrementType.AddFactor);

            LordshipPatron.InitializeNew("{=kGwNAXMVR}Patron", BKSkills.Instance.Lordship, GetTierCost(4), LordshipClaimant,
                "{=kGwNAXMVR}Grating titles yields renown",
                SkillEffect.PerkRole.Ruler, 0.2f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=kGwNAXMVR}Amicable actions (grant, negotiate) yield more positive relation",
                SkillEffect.PerkRole.Ruler, 0.1f,
                SkillEffect.EffectIncrementType.AddFactor);

            #endregion Lordship

            #region Scholarship

            ScholarshipLiterate = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipLiterate"));
            ScholarshipLiterate.InitializeNew("{=kGwNAXMVR}Literate", BKSkills.Instance.Scholarship, GetTierCost(1), null,
                "{=kGwNAXMVR}Allows reading books", 
                SkillEffect.PerkRole.Personal, 0f,
                SkillEffect.EffectIncrementType.Invalid, 
                string.Empty,
                SkillEffect.PerkRole.None, 0f,
                SkillEffect.EffectIncrementType.Invalid);

            ScholarshipAvidLearner = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipLearner"));
            ScholarshipAvidLearner.InitializeNew("{=kGwNAXMVR}Avid Learner", BKSkills.Instance.Scholarship, GetTierCost(2), null,
                "{=kGwNAXMVR}Increase language learning rate",
                SkillEffect.PerkRole.Personal, 20f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=kGwNAXMVR}Language limit is increased by 1",
                SkillEffect.PerkRole.Personal, 1f,
                SkillEffect.EffectIncrementType.Add);

            ScholarshipTutor = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipTutor"));
            ScholarshipTutor.InitializeNew("{=kGwNAXMVR}Tutor", BKSkills.Instance.Scholarship, GetTierCost(3), null,
                "{=kGwNAXMVR}Additional attribute point to clan children coming of age.",
                SkillEffect.PerkRole.ClanLeader, 1f,
                SkillEffect.EffectIncrementType.Add,
                "{=kGwNAXMVR}Extra experience gain for companions and family members in party",
                SkillEffect.PerkRole.PartyLeader, 5f,
                SkillEffect.EffectIncrementType.AddFactor);

            ScholarshipWellRead = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipWellRead"));
            ScholarshipWellRead.InitializeNew("{=kGwNAXMVR}Well Read", BKSkills.Instance.Scholarship, GetTierCost(4), null,
                "{=kGwNAXMVR}Increased reading rates for books",
                SkillEffect.PerkRole.Personal, 12f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=kGwNAXMVR}Cultural fascination progresses faster",
                SkillEffect.PerkRole.Personal, 10f,
                SkillEffect.EffectIncrementType.AddFactor);

            ScholarshipAccountant = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipAccountant"));
            ScholarshipMechanic = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipMechanic"));
            ScholarshipMechanic.InitializeNew("{=kGwNAXMVR}Mechanic", BKSkills.Instance.Scholarship, GetTierCost(5),
                ScholarshipAccountant,
                "{=kGwNAXMVR}Engineering skill tree yields both perks rather than 1",
                SkillEffect.PerkRole.Personal, 0f,
                SkillEffect.EffectIncrementType.Invalid,
                string.Empty,
                SkillEffect.PerkRole.None, 0f,
                SkillEffect.EffectIncrementType.Invalid);

            ScholarshipAccountant.InitializeNew("{=kGwNAXMVR}Accountant", BKSkills.Instance.Scholarship, GetTierCost(5),
                ScholarshipMechanic,
                "{=kGwNAXMVR}Stewardship skill tree yields both perks rather than 1",
                SkillEffect.PerkRole.Personal, 0f,
                SkillEffect.EffectIncrementType.Invalid,
                string.Empty,
                SkillEffect.PerkRole.None, 0f,
                SkillEffect.EffectIncrementType.Invalid);

            ScholarshipTeacher = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipTeacher"));
            ScholarshipTeacher.InitializeNew("{=kGwNAXMVR}Teacher", BKSkills.Instance.Scholarship, GetTierCost(6), null,
                "{=kGwNAXMVR}Additional focus points to children coming of age",
                SkillEffect.PerkRole.ClanLeader, 2f,
                SkillEffect.EffectIncrementType.Add,
                "{=kGwNAXMVR}",
                SkillEffect.PerkRole.None, 10f,
                SkillEffect.EffectIncrementType.AddFactor);

            ScholarshipBookWorm = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipBookWorm"));
            ScholarshipBookWorm.InitializeNew("{=kGwNAXMVR}Book Worm", BKSkills.Instance.Scholarship, GetTierCost(7), null,
                "{=kGwNAXMVR}Increased reading rates for books",
                SkillEffect.PerkRole.Personal, 20f,
                SkillEffect.EffectIncrementType.Add,
                "{=kGwNAXMVR}Language limit is increased by 1",
                SkillEffect.PerkRole.Personal, 1f,
                SkillEffect.EffectIncrementType.Add);
             
            ScholarshipPeerReview = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipPeerReview"));
            ScholarshipPeerReview.InitializeNew("{=kGwNAXMVR}Peer Review", BKSkills.Instance.Scholarship, GetTierCost(8), null,
                "{=kGwNAXMVR}Clan settlements yield more research points",
                SkillEffect.PerkRole.Personal, 20f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=kGwNAXMVR}Books yield double skill experience",
                SkillEffect.PerkRole.Personal, 100f,
                SkillEffect.EffectIncrementType.AddFactor);

            ScholarshipBedTimeStory = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipBedTimeStory"));
            ScholarshipBedTimeStory.InitializeNew("{=kGwNAXMVR}Bed Time Story", BKSkills.Instance.Scholarship, GetTierCost(9), null,
                "{=kGwNAXMVR}Daily experience points in random skill for companions and family in party",
                SkillEffect.PerkRole.PartyLeader, 10f,
                SkillEffect.EffectIncrementType.Add,
                string.Empty,
                SkillEffect.PerkRole.Personal, 100f,
                SkillEffect.EffectIncrementType.AddFactor);

            ScholarshipTreasurer = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipTreasurer"));
            ScholarshipNaturalScientist = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipNaturalScientist"));
            ScholarshipTreasurer.InitializeNew("{=kGwNAXMVR}Treasurer", BKSkills.Instance.Scholarship, GetTierCost(10),
                ScholarshipNaturalScientist,
                "{=kGwNAXMVR}Trade skill tree yields both perks rather than 1",
                SkillEffect.PerkRole.Personal, 0f,
                SkillEffect.EffectIncrementType.Invalid,
                string.Empty,
                SkillEffect.PerkRole.None, 0f,
                SkillEffect.EffectIncrementType.Invalid);

            ScholarshipNaturalScientist.InitializeNew("{=kGwNAXMVR}Natural Scientist", BKSkills.Instance.Scholarship,
                GetTierCost(10), ScholarshipTreasurer,
                "{=kGwNAXMVR}Medicine skill tree yields both perks rather than 1",
                SkillEffect.PerkRole.Personal, 0f,
                SkillEffect.EffectIncrementType.Invalid,
                string.Empty,
                SkillEffect.PerkRole.None, 0f,
                SkillEffect.EffectIncrementType.Invalid);

            ScholarshipPolyglot = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipPolyglot"));
            ScholarshipPolyglot.InitializeNew("{=kGwNAXMVR}Polyglot", BKSkills.Instance.Scholarship, GetTierCost(11), null,
                "{=kGwNAXMVR}Language limit is increased by 2", SkillEffect.PerkRole.Personal, 10f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=kGwNAXMVR}Language learning is significantly increased",
                SkillEffect.PerkRole.None, 0f,
                SkillEffect.EffectIncrementType.Invalid);

            ScholarshipMagnumOpus = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipMagnumOpus"));
            ScholarshipMagnumOpus.InitializeNew("{=kGwNAXMVR}Magnum Opus", BKSkills.Instance.Scholarship, GetTierCost(11), null,
                "{=kGwNAXMVR}+0.2% experience gain for every skill point in Scholarship above 230",
                SkillEffect.PerkRole.Personal, 0.2f,
                SkillEffect.EffectIncrementType.AddFactor,
                "{=kGwNAXMVR}Focus points add 50% more learning limit",
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