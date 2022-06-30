using BannerKings.Managers.Skills;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Education.Lifestyles
{
    public class DefaultLifestyles : DefaultTypeInitializer<DefaultLifestyles>
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
            fian = new Lifestyle("training_fian", new TextObject("{=!}Fian"), new TextObject("{=!}"));
            fian.Initialize(DefaultSkills.Bow, DefaultSkills.TwoHanded, new List<PerkObject>() { },
                Game.Current.ObjectManager.GetObjectTypeList<CultureObject>().FirstOrDefault(x => x.StringId == "battania"));

            cataphract = new Lifestyle("training_cataphract", new TextObject("{=!}Cataphract"), new TextObject("{=!}"));
            cataphract.Initialize(DefaultSkills.Polearm, DefaultSkills.Riding, new List<PerkObject>() { },
                Game.Current.ObjectManager.GetObjectTypeList<CultureObject>().FirstOrDefault(x => x.StringId == "empire"));

            diplomat = new Lifestyle("training_diplomat", new TextObject("{=!}Diplomat"), new TextObject("{=!}"));
            diplomat.Initialize(DefaultSkills.Charm, BKSkills.Instance.Lordship, new List<PerkObject>() { });

            august = new Lifestyle("training_august", new TextObject("{=!}August"), new TextObject("{=!}"));
            august.Initialize(DefaultSkills.Leadership, BKSkills.Instance.Lordship, new List<PerkObject>() { });

            siegeEngineer = new Lifestyle("training_siegeEngineer", new TextObject("{=!}Siege Engineer"), new TextObject("{=!}"));
            siegeEngineer.Initialize(DefaultSkills.Engineering, DefaultSkills.Tactics, new List<PerkObject>() { });

            civilAdministrator = new Lifestyle("training_civilAdministrator", new TextObject("{=!}Civil Administrator"), new TextObject("{=!}"));
            civilAdministrator.Initialize(DefaultSkills.Engineering, DefaultSkills.Steward, new List<PerkObject>() { });
        }

        public IEnumerable<Lifestyle> All
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
