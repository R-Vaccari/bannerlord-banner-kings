using BannerKings.Managers.Skills;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Education.Lifestyles
{
    public class DefaultLifestyles : DefaultTypeInitializer<DefaultLifestyles, Lifestyle>
    {
        private Lifestyle fian, cataphract, diplomat, august, siegeEngineer, civilAdministrator, caravaneer, outlaw, mercenary, 
            kheshig, varyag;

        public Lifestyle Fian => fian;
        public Lifestyle Diplomat => diplomat;
        public Lifestyle August => august;
        public Lifestyle Cataphract => cataphract;
        public Lifestyle SiegeEngineer => siegeEngineer;
        public Lifestyle CivilAdministrator => civilAdministrator;
        public Lifestyle Outlaw => outlaw;
        public Lifestyle Caravaneer => caravaneer;
        public Lifestyle Mercenary => mercenary;
        public override void Initialize()
        {
            MBReadOnlyList<CultureObject> cultures = Game.Current.ObjectManager.GetObjectTypeList<CultureObject>();

            fian = new Lifestyle("lifestyle_fian");
            fian.Initialize(new TextObject("{=!}Fian"), new TextObject("{=!}"), DefaultSkills.Bow, 
                DefaultSkills.TwoHanded, new List<PerkObject>() { BKPerks.Instance.FianHighlander, BKPerks.Instance.FianRanger, BKPerks.Instance.FianFennid },
                 new TextObject("{=!}Battanian settlements have +{EFFECT1} militia\nReduced damage by {EFFECT2}% when mounted"), 
                 1f, 30f,
                cultures.FirstOrDefault(x => x.StringId == "battania"));

            cataphract = new Lifestyle("lifestyle_cataphract");
            cataphract.Initialize(new TextObject("{=!}Cataphract"), new TextObject("{=!}"), 
                DefaultSkills.Polearm, DefaultSkills.Riding, 
                new List<PerkObject>() { BKPerks.Instance.CataphractEquites, BKPerks.Instance.CataphractAdaptiveTactics, BKPerks.Instance.CataphractKlibanophoros },
                new TextObject("{=!}Increased renown from victories by {EFFECT1}%\n"), 
                0f, 0f,
                cultures.FirstOrDefault(x => x.StringId == "empire"));

            diplomat = new Lifestyle("lifestyle_diplomat");
            diplomat.Initialize(new TextObject("{=!}Diplomat"), new TextObject("{=!}"), 
                DefaultSkills.Charm, BKSkills.Instance.Lordship, 
                new List<PerkObject>() { }, 
                new TextObject("{=!}"), 0f, 0f);

            august = new Lifestyle("lifestyle_august");
            august.Initialize(new TextObject("{=!}August"), new TextObject("{=!}"), 
                DefaultSkills.Leadership, BKSkills.Instance.Lordship, 
                new List<PerkObject>() { BKPerks.Instance.AugustCommander, BKPerks.Instance.AugustDeFacto,
                BKPerks.Instance.AugustDeJure, BKPerks.Instance.AugustKingOfKings }, 
                new TextObject("{=!}1 knight less is counted towards vassal limit\nTrade penalty increased by {EFFECT2}%"), 
                1f, 20f);

            siegeEngineer = new Lifestyle("lifestyle_siegeEngineer");
            siegeEngineer.Initialize(new TextObject("{=!}Siege Engineer"), new TextObject("{=!}"), 
                DefaultSkills.Engineering, DefaultSkills.Tactics, 
                new List<PerkObject>() { BKPerks.Instance.SiegeEngineer, BKPerks.Instance.SiegePlanner,  BKPerks.Instance.SiegeOverseer }, 
                new TextObject("{=!}"), 0f, 0f);

            civilAdministrator = new Lifestyle("lifestyle_civilAdministrator");
            civilAdministrator.Initialize(new TextObject("{=!}Civil Administrator"), new TextObject("{=!}"), 
                DefaultSkills.Engineering, DefaultSkills.Steward, 
                new List<PerkObject>() { BKPerks.Instance.CivilEngineer, BKPerks.Instance.CivilCultivator,
                BKPerks.Instance.CivilManufacturer, BKPerks.Instance.CivilOverseer }, 
                new TextObject("{=!}Reduced demesne weight of towns by {EFFECT1}%\nParty size reduced by {EFFECT2}"), 
                20f, 8f);

            caravaneer = new Lifestyle("lifestyle_caravaneer");
            caravaneer.Initialize(new TextObject("{=!}Caravaneer"), new TextObject("{=!}"),
                DefaultSkills.Trade, DefaultSkills.Scouting, 
                new List<PerkObject>() { BKPerks.Instance.CaravaneerStrider, BKPerks.Instance.CaravaneerDealer, BKPerks.Instance.CaravaneerEntrepeneur },
                new TextObject("{=!}Reduced trade penalty by {EFFECT1}%\nReduced speed during nighttime by {EFFECT2}%"),
                20f, 8f);

            outlaw = new Lifestyle("lifestyle_outlaw");
            outlaw.Initialize(new TextObject("{=!}Outlaw"), new TextObject("{=!}"),
                DefaultSkills.Roguery, DefaultSkills.Scouting,
                new List<PerkObject>() { BKPerks.Instance.OutlawKidnapper, BKPerks.Instance.OutlawPlunderer,
                BKPerks.Instance.OutlawNightPredator, BKPerks.Instance.OutlawUnderworldKing },
                new TextObject("{=!}Bandit troops are {EFFECT1}% faster on map\nRandomly lose relations with heroes that disapprove criminality when entering dialogue"),
                10f, 8f);

            mercenary = new Lifestyle("lifestyle_mercenary");
            mercenary.Initialize(new TextObject("{=!}Mercenary"), new TextObject("{=!}"),
                DefaultSkills.Leadership, DefaultSkills.Roguery, new List<PerkObject>() {  },
                new TextObject("{=!}Reduced demesne weight of towns by {EFFECT1}%\nSettlement stability reduced by {EFFECT2}%"),
                20f, 8f);

            kheshig = new Lifestyle("lifestyle_kheshig");
            kheshig.Initialize(new TextObject("{=!}Kheshig"), new TextObject("{=!}"),
                DefaultSkills.Leadership, DefaultSkills.Roguery, new List<PerkObject>() { },
                new TextObject("{=!}Reduced demesne weight of towns by {EFFECT1}%\nSettlement stability reduced by {EFFECT2}%"),
                20f, 8f);

            varyag = new Lifestyle("lifestyle_varyag");
            varyag.Initialize(new TextObject("{=!}Varyag"), new TextObject("{=!}"),
                DefaultSkills.Leadership, DefaultSkills.Roguery, new List<PerkObject>() { },
                new TextObject("{=!}Reduced demesne weight of towns by {EFFECT1}%\nSettlement stability reduced by {EFFECT2}%"),
                20f, 8f);
        }

        public override IEnumerable<Lifestyle> All
        {
            get
            {
                yield return Fian;
                //yield return Diplomat;
                yield return Cataphract;
                yield return Mercenary;
                yield return Outlaw;
                yield return Caravaneer;
                yield return August;
                yield return SiegeEngineer;
                yield return CivilAdministrator;
            }
        }
    }
}
