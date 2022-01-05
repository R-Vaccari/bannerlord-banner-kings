using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
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

        private bool IsSettlementPresent(Settlement settlement) => TITLES.FirstOrDefault(x => x.fief == settlement) != null;
        public bool IsHeroTitleHolder(Hero hero) => TITLE_HOLDERS.ContainsKey(hero);
        public FeudalTitle GetTitle(Settlement settlement) => TITLES.FirstOrDefault(x => x.fief == settlement);
        public Hero GetDeJure(Settlement settlement) => TITLES.FirstOrDefault(x => x.fief == settlement).deJure;
        public bool IsHeroKnighted(Hero hero) => hero.IsNoble && TITLE_HOLDERS.ContainsKey(hero);
        public bool HasSuzerain(Settlement settlement)
        {
            FeudalTitle vassal = TITLES.FirstOrDefault(x => x.fief == settlement);
            if (vassal != null)
            {
                FeudalTitle suzerain = TITLES.FirstOrDefault(x => x.vassals.Contains(vassal));
                return suzerain != null;
            }
            return false;
        }

        public HashSet<FeudalTitle> GetTitles(Hero hero)
        {
            if (IsHeroTitleHolder(hero)) return TITLE_HOLDERS[hero];
            else return null;
        }

        public FeudalTitle GetHighestTitle(Hero hero)
        {
            if (IsHeroTitleHolder(hero))
            {
                FeudalTitle highestTitle = null;
                foreach (FeudalTitle title in TITLE_HOLDERS[hero])
                    if (highestTitle == null || title.type < highestTitle.type)
                        highestTitle = title;
                return highestTitle;
            }
            else return null;
        }

        public HashSet<FeudalTitle> GetVassals(TitleType threshold, Hero lord)
        {
            HashSet<FeudalTitle> allTitles = TITLE_HOLDERS[lord];
            HashSet<FeudalTitle> vassals = new HashSet<FeudalTitle>();
            foreach (FeudalTitle title in allTitles)
                if (title.deFacto.MapFaction == lord.MapFaction && (title.deFacto == title.deJure || title.deJure.MapFaction == lord.MapFaction)
                    && (int)title.type <= (int)threshold)
                    vassals.Add(title);
            return vassals;
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
            XmlNode titlesNode = doc.ChildNodes[1].ChildNodes[0];
            bool autoGenerate = bool.Parse(titlesNode.Attributes["autoGenerate"].Value);
 
            foreach (XmlNode kingdom in titlesNode)
            {
                if (kingdom.Name != "kingdom") return;

                HashSet<FeudalTitle> vassalsKingdom = new HashSet<FeudalTitle>();
                string factionName = kingdom.Attributes["faction"].Value;
                string deJureNameKingdom = kingdom.Attributes["deJure"].Value;
                Hero deJureKingdom = Hero.AllAliveHeroes.FirstOrDefault(x => x.StringId == deJureNameKingdom);
                Kingdom faction = Kingdom.All.FirstOrDefault(x => x.Name.ToString() == factionName);
                string contractType = kingdom.Attributes["contract"].Value;
                FeudalContract contract = GenerateContract(contractType);

                if (contract == null) return;

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
                                            vassalsCounty.Add(CreateLandedTitle(settlementBarony, deJureBarony, TitleType.Barony, contract));
                                    }

                                if (settlementCounty != null)
                                    vassalsDuchy.Add(CreateLandedTitle(settlementCounty, deJureCounty, TitleType.County, contract, vassalsCounty));
                            }

                        if (deJureDuchy != null && vassalsDuchy.Count > 0)
                            vassalsKingdom.Add(CreateUnlandedTitle(deJureDuchy, TitleType.Dukedom, vassalsDuchy, "Dukedom of " + dukedomName, contract));
                    }

                if (deJureKingdom != null && vassalsKingdom.Count > 0 && faction != null)
                    CreateKingdom(deJureKingdom, faction, TitleType.Kingdom, vassalsKingdom, contract);
            }

            if (autoGenerate) 
                foreach (Settlement settlement in Settlement.All)
                {
                    if (settlement.IsVillage) return;
                    else if ((settlement.IsTown || settlement.IsCastle) && !IsSettlementPresent(settlement))
                        CreateLandedTitle(settlement, settlement.Owner, settlement.IsTown ? TitleType.County : TitleType.Barony, GenerateContract("feudal"), null);
                }         
        }

        public void GrantLordship(FeudalTitle title, Hero giver, Hero receiver)
        {
            title.deFacto = receiver;
            title.deJure = receiver;
            TITLE_HOLDERS[giver].Remove(title);
            AddTitleAndHolder(receiver, title);
            receiver.IsNoble = true;
        }

        public void ShowContract(Hero lord, string buttonString)
        {
            FeudalTitle title = PopulationConfig.Instance.TitleManager.GetHighestTitle(lord);
            string description = PopulationConfig.Instance.TitleManager.GetContractText(title);
            InformationManager.ShowInquiry(new InquiryData(string.Format("{=feudalism_contract_header}Enfoeffement Contract for {0}", title.name),
                description, true, false, buttonString, "", null, null), false);
        }

        public string GetContractText(FeudalTitle title)
        {
            FeudalContract contract = title.contract;
            StringBuilder sb = new StringBuilder(string.Format("{=str_feudalism_contract_paragraph}You, {0}, formally accept to be henceforth bound to the {1}, fulfill your duties as well as uphold your rights," +
                " what can not be undone by means other than abdication of all rights and lands associated with the contract, treachery, or death.", Hero.MainHero.Name, title.name));
            sb.Append(Environment.NewLine);
            sb.Append("   ");
            sb.Append(Environment.NewLine);
            sb.Append("{=feudalism_contract_duties}Duties");
            sb.Append(Environment.NewLine);
            foreach (KeyValuePair<FeudalDuties, float> duty in contract.duties)
            {
                sb.Append(string.Format(this.GetDutyString(duty.Key), duty.Value));
                sb.Append(Environment.NewLine);
            }
            sb.Append(Environment.NewLine);
            sb.Append("   ");
            sb.Append(Environment.NewLine);
            sb.Append("{=feudalism_contract_rights}Rights");
            sb.Append(Environment.NewLine);
            foreach (FeudalRights right in contract.rights)
            {
                sb.Append(GetRightString(right));
                sb.Append(Environment.NewLine);
            }
           
            return sb.ToString();
        }

        private string GetDutyString(FeudalDuties duty)
        {
            if (duty == FeudalDuties.Taxation)
                return "You are due {0} of your fief's income to your suzerain.";
            else if (duty == FeudalDuties.Auxilium)
                return "You are obliged to militarily participate in armies.";
            else return "You are obliged to contribute to {0} of your suzerain's ransom.";
        }

        private string GetRightString(FeudalRights right)
        {
            if (right == FeudalRights.Absolute_Land_Rights)
                return "You are entitled to ownership of any conquered lands whose title you own.";
            else if (right == FeudalRights.Enfoeffement_Rights)
                return "You are entitled to be granted land in case you have none, whenever possible.";
            else return "";
        }

        private FeudalContract GenerateContract(string type)
        {
            if (type == "imperial")
                return new FeudalContract(new Dictionary<FeudalDuties, float>() {
                    { FeudalDuties.Ransom, 0.10f },
                    { FeudalDuties.Taxation, 0.4f }
                }, new HashSet<FeudalRights>() {
                    FeudalRights.Assistance_Rights,
                    FeudalRights.Army_Compensation_Rights
                });
            else if (type == "tribal")
                return new FeudalContract(new Dictionary<FeudalDuties, float>() {
                    { FeudalDuties.Taxation, 0.125f },
                    { FeudalDuties.Auxilium, 0.66f }
                }, new HashSet<FeudalRights>() {
                    FeudalRights.Conquest_Rights,
                    FeudalRights.Absolute_Land_Rights
                });
            else return new FeudalContract(new Dictionary<FeudalDuties, float>() {
                    { FeudalDuties.Ransom, 0.20f },
                    { FeudalDuties.Auxilium, 0.4f }
                }, new HashSet<FeudalRights>() {
                    FeudalRights.Absolute_Land_Rights,
                    FeudalRights.Enfoeffement_Rights
                });
        }

        private FeudalTitle CreateKingdom(Hero deJure, Kingdom faction, TitleType type, HashSet<FeudalTitle> vassals, FeudalContract contract)
        {
            FeudalTitle title = new FeudalTitle(type, null, vassals, deJure, faction.Leader, faction.Name.ToString(), contract);
            AddTitleAndHolder(deJure, title);
            return title;
        }

        private FeudalTitle CreateUnlandedTitle(Hero deJure, TitleType type, HashSet<FeudalTitle> vassals, string name, FeudalContract contract)
        {
            FeudalTitle title = new FeudalTitle(type, null, vassals, deJure, deJure, name, contract);
            AddTitleAndHolder(deJure, title);
            return title;
        }
            
        private FeudalTitle CreateLandedTitle(Settlement settlement, Hero deJure, TitleType type, FeudalContract contract, HashSet<FeudalTitle> vassals = null)
        {
            Hero deFacto = settlement.OwnerClan.Leader;
            if (deJure == null) deJure = settlement.Owner;
            if (vassals == null) vassals = new HashSet<FeudalTitle>();
            if (settlement.BoundVillages != null)
                foreach (Village lordship in settlement.BoundVillages)
                {
                    FeudalTitle lordshipTitle = CreateLordship(lordship.Settlement, deJure, contract);
                    vassals.Add(lordshipTitle);
                    AddTitleAndHolder(deFacto, lordshipTitle);
                }
            FeudalTitle title = new FeudalTitle(type, settlement, vassals, deJure, deFacto, settlement.Name.ToString(), contract);
            AddTitleAndHolder(deFacto, title);
            return title;
        }

        private FeudalTitle CreateLordship(Settlement settlement, Hero deJure, FeudalContract contract) => new FeudalTitle(TitleType.Lordship, settlement, null,
            deJure, settlement.Village.Bound.Owner, settlement.Name.ToString(), contract);

        public class FeudalTitle
        {
            public TitleType type { get; set; }
            public Settlement fief { get; set; }
            public HashSet<FeudalTitle> vassals { get; set; }
            public Hero deJure { get; set; }
            public Hero deFacto { get; set; }
            public TextObject name { get; set; }
            public TextObject shortName { get; set; }
            public float dueTax { get; set; }

            public FeudalContract contract { get; }

            public FeudalTitle(TitleType type, Settlement fief, HashSet<FeudalTitle> vassals, Hero deJure, Hero deFacto, string name, FeudalContract contract)
            {
                this.type = type;
                this.fief = fief;
                this.vassals = vassals;
                this.deJure = deJure;
                this.deFacto = deFacto;
                this.name = new TextObject(Helpers.Helpers.GetTitlePrefix(type) + " of " + name);
                this.shortName = new TextObject(name);
                this.contract = contract;
                dueTax = 0;
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

        public class FeudalContract
        {
            public Dictionary<FeudalDuties, float> duties { get; set; }
            public HashSet<FeudalRights> rights { get; set; }

            public FeudalContract(Dictionary<FeudalDuties, float> duties, HashSet<FeudalRights> rights)
            {
                this.duties = duties;
                this.rights = rights;
            }
        }

        public enum FeudalDuties
        {
            Ransom,
            Taxation,
            Auxilium

        }

        public enum FeudalRights
        {
            Absolute_Land_Rights,
            Conquest_Rights,
            Enfoeffement_Rights,
            Assistance_Rights,
            Army_Compensation_Rights
        }
    }
}
