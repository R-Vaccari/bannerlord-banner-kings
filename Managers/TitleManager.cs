using System.Collections.Generic;
using System.Linq;
using System.Xml;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Populations.Managers
{
    public class TitleManager
    {
        public HashSet<FeudalTitle> TITLES { get; set; }
        public Dictionary<Hero, HashSet<FeudalTitle>> TITLE_HOLDERS { get; set; }

        public TitleManager(HashSet<FeudalTitle> titles, Dictionary<Hero, HashSet<FeudalTitle>> titleHolders)
        {
            this.TITLES = titles;
            this.TITLE_HOLDERS = titleHolders;
            InitializeTitles();
        }

        public bool IsHeroTitleHolder(Hero hero) => TITLE_HOLDERS.ContainsKey(hero);

        public HashSet<FeudalTitle> GetTitles(Hero hero)
        {
            if (IsHeroTitleHolder(hero)) return TITLE_HOLDERS[hero];
            else return null;
        }

        public void AddTitleHolder(Hero hero, FeudalTitle title)
        {
            if (!TITLE_HOLDERS.ContainsKey(hero))
                TITLE_HOLDERS.Add(hero, new HashSet<FeudalTitle>() { title });
            else
                TITLE_HOLDERS[hero].Add(title);
        }

        private void AddTitleAndHolder(Hero hero, FeudalTitle title)
        {
            AddTitleHolder(hero, title);
            TITLES.Add(title);
        }

        public void InitializeTitles()
        {
            XmlDocument doc = Helpers.Helpers.CreateDocumentFromXmlFile(BasePath.Name + "Modules/Populations/ModuleData/titles.xml");
            foreach (XmlNode kingdom in doc.ChildNodes[1].ChildNodes[0])
            {
                if (kingdom.Name != "kingdom") return;

                HashSet<FeudalTitle> vassalsKingdom = new HashSet<FeudalTitle>();
                string name = kingdom.Attributes["name"].Value;
                string deJureNameKingdom = kingdom.Attributes["deJure"].Value;
                Hero deJureKingdom = Hero.AllAliveHeroes.FirstOrDefault(x => x.StringId == deJureNameKingdom);

                if (kingdom.ChildNodes != null)
                    foreach (XmlNode duchy in kingdom.ChildNodes)
                    {
                        if (duchy.Name != "duchy") return;

                        HashSet<FeudalTitle> vassalsDuchy = new HashSet<FeudalTitle>();
                        string dukedomName = duchy.Attributes["name"].Value;
                        string deJureNameDuchy = duchy.Attributes["deJure"].Value;
                        Hero deJureDuchy = Hero.AllAliveHeroes.FirstOrDefault(x => x.StringId == deJureNameDuchy);

                        if (duchy.ChildNodes != null)
                            foreach (XmlNode county in duchy.ChildNodes)
                            {
                                if (county.Name != "county") return;

                                string settlementNameCounty = county.Attributes["settlement"].Value;
                                string deJureNameCounty = county.Attributes["deJure"].Value;
                                Settlement settlementCounty = Settlement.All.FirstOrDefault(x => x.Name.ToString() == settlementNameCounty);
                                Hero deJureCounty = Hero.AllAliveHeroes.FirstOrDefault(x => x.StringId == deJureNameCounty);
                                HashSet<FeudalTitle> vassalsCounty = new HashSet<FeudalTitle>();

                                if (county.ChildNodes != null)
                                    foreach (XmlNode barony in county.ChildNodes)
                                    {
                                        if (barony.Name != "barony") return;

                                        string settlementNameBarony = barony.Attributes["settlement"].Value;
                                        string deJureIdBarony = barony.Attributes["deJure"].Value;
                                        Settlement settlementBarony = Settlement.All.FirstOrDefault(x => x.Name.ToString() == settlementNameBarony);
                                        Hero deJureBarony = Hero.AllAliveHeroes.FirstOrDefault(x => x.StringId == deJureIdBarony);
                                        if (settlementBarony != null)
                                            vassalsCounty.Add(CreateLandedTitle(settlementBarony, deJureBarony, TitleType.Barony));
                                    }

                                if (settlementCounty != null)
                                    vassalsDuchy.Add(CreateLandedTitle(settlementCounty, deJureCounty, TitleType.County, vassalsCounty));
                            }

                        if (deJureDuchy != null && vassalsDuchy.Count > 0)
                            vassalsKingdom.Add(CreateUnlandedTitle(deJureDuchy, TitleType.Dukedom, vassalsDuchy, "Dukedom of " + dukedomName));
                    }

                if (deJureKingdom != null && vassalsKingdom.Count > 0)
                    CreateUnlandedTitle(deJureKingdom, TitleType.Kingdom, vassalsKingdom, name);
            }
        }

        private FeudalTitle CreateUnlandedTitle(Hero deJure, TitleType type, HashSet<FeudalTitle> vassals, string name)
        {
            FeudalTitle title = new FeudalTitle(type, null, vassals, deJure, deJure, name);
            AddTitleAndHolder(deJure, title);
            return title;
        }
            

        private FeudalTitle CreateLandedTitle(Settlement settlement, Hero deJure, TitleType type, HashSet<FeudalTitle> vassals = null)
        {
            Hero deFacto = settlement.OwnerClan.Leader;
            if (deJure == null) deJure = settlement.Owner;
            if (vassals == null) vassals = new HashSet<FeudalTitle>();
            if (settlement.BoundVillages != null)
                foreach (Village lordship in settlement.BoundVillages)
                {
                    FeudalTitle lordshipTitle = CreateLordship(lordship.Settlement, deJure);
                    vassals.Add(lordshipTitle);
                    AddTitleAndHolder(deFacto, lordshipTitle);
                }
            FeudalTitle title = new FeudalTitle(type, settlement, vassals, deJure, deFacto, settlement.Name.ToString());
            AddTitleAndHolder(deFacto, title);
            return title;
        }

        private FeudalTitle CreateLordship(Settlement settlement, Hero deJure) => new FeudalTitle(TitleType.Lordship, settlement, null,
            deJure, settlement.Village.Bound.Owner, settlement.Name.ToString());

        public class FeudalTitle
        {
            public TitleType type { get; set; }
            public Settlement fief { get; set; }
            public HashSet<FeudalTitle> vassals { get; set; }
            public Hero deJure { get; set; }
            public Hero deFacto { get; set; }
            public TextObject name { get; set; }
            public TextObject shortName { get; set; }

            public FeudalTitle(TitleType type, Settlement fief, HashSet<FeudalTitle> vassals, Hero deJure, Hero deFacto, string name)
            {
                this.type = type;
                this.fief = fief;
                this.vassals = vassals;
                this.deJure = deJure;
                this.deFacto = deFacto;
                this.name = new TextObject(Helpers.Helpers.GetTitlePrefix(type) + " of " + name);
                this.shortName = new TextObject(name);
            }
        }

        public enum TitleType
        {
            Empire,
            Kingdom,
            Dukedom,
            County,
            Barony,
            Lordship
        }
    }
}
