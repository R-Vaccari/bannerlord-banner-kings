using BannerKings.Managers.Skills;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Education.Lifestyles
{
    public class DefaultLifestyles : DefaultTypeInitializer<DefaultLifestyles, Lifestyle>
    {
        private Lifestyle fian, cataphract, diplomat, august, siegeEngineer, civilAdministrator;

        public Lifestyle Fian => fian;
        public Lifestyle Diplomat => diplomat;
        public Lifestyle August => august;
        public Lifestyle Cataphract => cataphract;
        public Lifestyle SiegeEngineer => siegeEngineer;
        public Lifestyle CivilAdministrator => civilAdministrator;
        public override void Initialize()
        {
            fian = new Lifestyle("lifestyle_fian");
            fian.Initialize(new TextObject("{=!}Fian"), new TextObject("{=!}"), DefaultSkills.Bow, 
                DefaultSkills.TwoHanded, new List<PerkObject>() { BKPerks.Instance.FianHighlander, BKPerks.Instance.FianRanger, BKPerks.Instance.FianFennid },
                Game.Current.ObjectManager.GetObjectTypeList<CultureObject>().FirstOrDefault(x => x.StringId == "battania"));

            cataphract = new Lifestyle("lifestyle_cataphract");
            cataphract.Initialize(new TextObject("{=!}Cataphract"), new TextObject("{=!}"), 
                DefaultSkills.Polearm, DefaultSkills.Riding, new List<PerkObject>() { },
                Game.Current.ObjectManager.GetObjectTypeList<CultureObject>().FirstOrDefault(x => x.StringId == "empire"));

            diplomat = new Lifestyle("lifestyle_diplomat");
            diplomat.Initialize(new TextObject("{=!}Diplomat"), new TextObject("{=!}"), 
                DefaultSkills.Charm, BKSkills.Instance.Lordship, new List<PerkObject>() { });

            august = new Lifestyle("lifestyle_august");
            august.Initialize(new TextObject("{=!}August"), new TextObject("{=!}"), 
                DefaultSkills.Leadership, BKSkills.Instance.Lordship, new List<PerkObject>() { });

            siegeEngineer = new Lifestyle("lifestyle_siegeEngineer");
            siegeEngineer.Initialize(new TextObject("{=!}Siege Engineer"), new TextObject("{=!}"), 
                DefaultSkills.Engineering, DefaultSkills.Tactics, new List<PerkObject>() { });

            civilAdministrator = new Lifestyle("lifestyle_civilAdministrator");
            civilAdministrator.Initialize(new TextObject("{=!}Civil Administrator"), new TextObject("{=!}"), 
                DefaultSkills.Engineering, DefaultSkills.Steward, new List<PerkObject>() { BKPerks.Instance.CivilEngineer, BKPerks.Instance.CivilCultivator,
                BKPerks.Instance.CivilManufacturer, BKPerks.Instance.CivilOverseer });
        }

        public override IEnumerable<Lifestyle> All
        {
            get
            {
                yield return Fian;
                yield return Diplomat;
                yield return August;
                yield return Cataphract;
                yield return SiegeEngineer;
                yield return CivilAdministrator;
            }
        }
    }
}
