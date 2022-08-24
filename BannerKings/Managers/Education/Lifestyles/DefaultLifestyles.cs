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
        private Lifestyle kheshig, varyag;

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

        public override void Initialize()
        {
            var cultures = Game.Current.ObjectManager.GetObjectTypeList<CultureObject>();

            Fian = new Lifestyle("lifestyle_fian");
            Fian.Initialize(new TextObject("{=Gheoen3CJ}Fian"), new TextObject("{=!}"), DefaultSkills.Bow,
                DefaultSkills.TwoHanded,
                new List<PerkObject>
                    {BKPerks.Instance.FianHighlander, BKPerks.Instance.FianRanger, BKPerks.Instance.FianFennid},
                new TextObject(
                    "{=uJK1YaUP1}Battanian settlements have +{EFFECT1} militia\nReduced damage by {EFFECT2}% when mounted"),
                1f, 30f,
                cultures.FirstOrDefault(x => x.StringId == "battania"));

            Cataphract = new Lifestyle("lifestyle_cataphract");
            Cataphract.Initialize(new TextObject("{=bSe3RKtNN}Cataphract"), new TextObject("{=!}"),
                DefaultSkills.Polearm, DefaultSkills.Riding,
                new List<PerkObject>
                {
                    BKPerks.Instance.CataphractEquites, BKPerks.Instance.CataphractAdaptiveTactics,
                    BKPerks.Instance.CataphractKlibanophoros
                },
                new TextObject("{=fsj5HGfYR}Increased renown from victories by {EFFECT1}%\n"),
                0f, 0f,
                cultures.FirstOrDefault(x => x.StringId == "empire"));

            Diplomat = new Lifestyle("lifestyle_diplomat");
            Diplomat.Initialize(new TextObject("{=er16VXrkD}Diplomat"), new TextObject("{=!}"),
                DefaultSkills.Charm, BKSkills.Instance.Lordship,
                new List<PerkObject>(),
                new TextObject("{=!}"), 0f, 0f);

            August = new Lifestyle("lifestyle_august");
            August.Initialize(new TextObject("{=Q1XVCySVv}August"), new TextObject("{=!}"),
                DefaultSkills.Leadership, BKSkills.Instance.Lordship,
                new List<PerkObject>
                {
                    BKPerks.Instance.AugustCommander, BKPerks.Instance.AugustDeFacto,
                    BKPerks.Instance.AugustDeJure, BKPerks.Instance.AugustKingOfKings
                },
                new TextObject("{=ZE4FdUt6h}1 knight less is counted towards vassal limit\nTrade penalty increased by {EFFECT2}%"),
                1f, 20f);

            SiegeEngineer = new Lifestyle("lifestyle_siegeEngineer");
            SiegeEngineer.Initialize(new TextObject("{=M6pFSw6zR}Siege Engineer"), new TextObject("{=!}"),
                DefaultSkills.Engineering, DefaultSkills.Tactics,
                new List<PerkObject>
                    {BKPerks.Instance.SiegeEngineer, BKPerks.Instance.SiegePlanner, BKPerks.Instance.SiegeOverseer},
                new TextObject("{=!}"), 0f, 0f);

            CivilAdministrator = new Lifestyle("lifestyle_civilAdministrator");
            CivilAdministrator.Initialize(new TextObject("{=rUyTL8ssX}Civil Administrator"), new TextObject("{=!}"),
                DefaultSkills.Engineering, DefaultSkills.Steward,
                new List<PerkObject>
                {
                    BKPerks.Instance.CivilEngineer, BKPerks.Instance.CivilCultivator,
                    BKPerks.Instance.CivilManufacturer, BKPerks.Instance.CivilOverseer
                },
                new TextObject("{=MB4FNAiBp}Reduced demesne weight of towns by {EFFECT1}%\nParty size reduced by {EFFECT2}"),
                20f, 8f);

            Caravaneer = new Lifestyle("lifestyle_caravaneer");
            Caravaneer.Initialize(new TextObject("{=YfYjTaJ13}Caravaneer"), new TextObject("{=!}"),
                DefaultSkills.Trade, DefaultSkills.Scouting,
                new List<PerkObject> {BKPerks.Instance.CaravaneerStrider, BKPerks.Instance.CaravaneerDealer},
                new TextObject("{=1wnJUMFPj}Reduced trade penalty by {EFFECT1}%\nReduced speed during nighttime by {EFFECT2}%"),
                10f, 8f);

            Artisan = new Lifestyle("lifestyle_artisan");
            Artisan.Initialize(new TextObject("{=gYSTW0z9C}Artisan"), new TextObject("{=!}"),
                DefaultSkills.Crafting, DefaultSkills.Trade, new List<PerkObject> {BKPerks.Instance.ArtisanEntrepeneur},
                new TextObject("{=eOOHDbgef}Chance of botching items when smithing reduced by {EFFECT1}%\n{EFFECT2}%"),
                10f, 8f);

            Outlaw = new Lifestyle("lifestyle_outlaw");
            Outlaw.Initialize(new TextObject("{=87TW0PzZ7}Outlaw"), new TextObject("{=!}"),
                DefaultSkills.Roguery, DefaultSkills.Scouting,
                new List<PerkObject>
                {
                    BKPerks.Instance.OutlawKidnapper, BKPerks.Instance.OutlawPlunderer,
                    BKPerks.Instance.OutlawNightPredator, BKPerks.Instance.OutlawUnderworldKing
                },
                new TextObject(
                    "{=qQw2zg3RS}Bandit troops are {EFFECT1}% faster on map\nRandomly lose relations with heroes that disapprove criminality when leaving dialogue"),
                10f, 8f);

            Mercenary = new Lifestyle("lifestyle_mercenary");
            Mercenary.Initialize(new TextObject("{=DiPJLZWf2}Mercenary"), new TextObject("{=!}"),
                DefaultSkills.Leadership, DefaultSkills.Roguery, new List<PerkObject>(),
                new TextObject(
                    "{=077JBdHyr}Reduced demesne weight of towns by {EFFECT1}%\nSettlement stability reduced by {EFFECT2}%"),
                20f, 8f);

            kheshig = new Lifestyle("lifestyle_kheshig");
            kheshig.Initialize(new TextObject("{=tC14CFLiw}Kheshig"), new TextObject("{=!}"),
                DefaultSkills.Leadership, DefaultSkills.Roguery, new List<PerkObject>(),
                new TextObject(
                    "{=077JBdHyr}Reduced demesne weight of towns by {EFFECT1}%\nSettlement stability reduced by {EFFECT2}%"),
                20f, 8f);

            varyag = new Lifestyle("lifestyle_varyag");
            varyag.Initialize(new TextObject("{=MYGQoRm3b}Varyag"), new TextObject("{=!}"),
                DefaultSkills.Leadership, DefaultSkills.Roguery, new List<PerkObject>(),
                new TextObject(
                    "{=077JBdHyr}Reduced demesne weight of towns by {EFFECT1}%\nSettlement stability reduced by {EFFECT2}%"),
                20f, 8f);
        }
    }
}