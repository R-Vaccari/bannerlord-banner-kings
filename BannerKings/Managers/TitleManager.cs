using BannerKings.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers
{
    public class TitleManager
    {
        [SaveableProperty(1)]
        private Dictionary<FeudalTitle, Hero> Titles { get; set; }

        [SaveableProperty(2)]
        public Dictionary<Kingdom, FeudalTitle> Kingdoms { get; set; }

        [SaveableProperty(3)]
        public bool Knighthood { get; set; } = true;


        public TitleManager(Dictionary<FeudalTitle, Hero> titles, Dictionary<Hero, List<FeudalTitle>> titleHolders, Dictionary<Kingdom, FeudalTitle> kingdoms)
        {
            this.Titles = titles;
            this.Kingdoms = kingdoms;
            this.Knighthood = true;
            InitializeTitles();
        }

        public bool IsHeroTitleHolder(Hero hero)
        {
            FeudalTitle result = null;
            foreach (KeyValuePair<FeudalTitle, Hero> pair in Titles)
                if (pair.Key.deFacto == hero || pair.Key.deJure == hero)
                {
                    result = pair.Key;
                    break;
                }

            return result != null;
        }
        public FeudalTitle GetTitle(Settlement settlement)
        {
            FeudalTitle result = null;
            foreach (KeyValuePair<FeudalTitle, Hero> pair in Titles)
                if (pair.Key.fief == settlement)
                {
                    result = pair.Key;
                    break;
                }
                
            return result;
        }

        public GovernmentType GetSettlementGovernment(Settlement settlement)
        {
            GovernmentType type = GovernmentType.Feudal;
            FeudalTitle title = this.GetTitle(settlement);
            if (title != null)
                if (title.contract != null)
                    type = title.contract.government;

            return type;
        }

        public bool IsHeroKnighted(Hero hero) => hero.IsNoble && IsHeroTitleHolder(hero);
        public FeudalTitle GetImmediateSuzerain(FeudalTitle target)
        {
            FeudalTitle result = null;
            foreach (KeyValuePair<FeudalTitle, Hero> pair in Titles)
                    if (pair.Key.vassals != null && pair.Key.vassals.Contains(target))
                    {
                        result = pair.Key;
                        break;
                    }

            return result;
        }

        private void ExecuteOwnershipChange(Hero oldOwner, Hero newOwner, FeudalTitle title, bool deJure)
        {
            if (Titles.ContainsKey(title))
            {
                Hero hero = Titles[title];
                if (deJure)
                {
                    title.deJure = newOwner;
                    Titles[title] = newOwner;
                }
                else title.deFacto = newOwner;
            }
        }

        private void ExecuteAddTitle(FeudalTitle title)
        {
            List<FeudalTitle> keys = Titles.Keys.ToList();
            if (!keys.Contains(title))
                Titles.Add(title, title.deJure);
        }

        public FeudalTitle CalculateHeroSuzerain(Hero hero)  
        {
            FeudalTitle title = GetHighestTitle(hero);
            if (title == null) return null;
            Kingdom kingdom1 = GetTitleFaction(title);

            if (kingdom1 == null || hero.Clan.Kingdom == null) return null;

            FeudalTitle suzerain = GetImmediateSuzerain(title);
            if (suzerain != null)
            {
                Kingdom kingdom2 = GetTitleFaction(suzerain);
                if (kingdom2 == kingdom1)
                    return suzerain;
                else
                {
                    FeudalTitle factionTitle = GetHighestTitleWithinFaction(hero, kingdom1);
                    if (factionTitle != null)
                    {
                        FeudalTitle suzerainFaction = GetImmediateSuzerain(factionTitle);
                        return suzerainFaction;
                    } else
                        return GetHighestTitle(kingdom1.Leader);
                }
            }

            return null;
        }

        public bool HasSuzerain(Settlement settlement)
        {
            FeudalTitle vassal = GetTitle(settlement);
            if (vassal != null)
            {
                FeudalTitle suzerain = GetImmediateSuzerain(vassal);
                return suzerain != null;
            }
            return false;
        }

        public bool HasSuzerain(FeudalTitle vassal)
        {
            FeudalTitle suzerain = GetImmediateSuzerain(vassal);
            return suzerain != null;
        }

        public void InheritAllTitles(Hero oldOwner, Hero heir)
        {
            if (IsHeroTitleHolder(oldOwner))
            {
                List<FeudalTitle> set = GetAllDeJure(oldOwner);
                List<FeudalTitle> titles = new List<FeudalTitle>(set);
                foreach (FeudalTitle title in titles)
                {
                    if (title.deJure == oldOwner) ExecuteOwnershipChange(oldOwner, heir, title, true);
                    if (title.deFacto == oldOwner) ExecuteOwnershipChange(oldOwner, heir, title, false);
                }
            }
        }

        public void InheritTitle(Hero oldOwner, Hero heir, FeudalTitle title)
        {
            if (IsHeroTitleHolder(oldOwner))
            {
                if (title.deJure == oldOwner) ExecuteOwnershipChange(oldOwner, heir, title, true);
                if (title.deFacto == oldOwner) ExecuteOwnershipChange(oldOwner, heir, title, false);  
            }
        }

        public void UsurpTitle(Hero oldOwner, Hero usurper, FeudalTitle title, UsurpCosts costs)
        {
            ExecuteOwnershipChange(oldOwner, usurper, title, true);
            if (title.type == TitleType.Barony || title.type == TitleType.County)
                if (title.vassals != null)
                    foreach (FeudalTitle vassal in title.vassals)
                        if (vassal.type == TitleType.Lordship)
                            ExecuteOwnershipChange(oldOwner, usurper, vassal, true);

            int impact = new BKUsurpationModel().GetUsurpRelationImpact(title);
            ChangeRelationAction.ApplyPlayerRelation(oldOwner, impact, true, true);
            Kingdom kingdom = oldOwner.Clan.Kingdom;
            if (kingdom != null) 
                foreach(Clan clan in kingdom.Clans) 
                    if (clan != oldOwner.Clan && clan.IsMapFaction && clan.IsKingdomFaction)
                    {
                        int random = MBRandom.RandomInt(1, 100);
                        if (random <= 10)
                            ChangeRelationAction.ApplyPlayerRelation(oldOwner, (int)((float)impact * 0.3f), true, true);
                    }

            if (costs.gold > 0)
                usurper.ChangeHeroGold((int)-costs.gold);
            if (costs.influence > 0)
                usurper.Clan.Influence -= costs.influence;
            if (costs.renown > 0)
                usurper.Clan.Renown -= costs.renown;
            //OwnershipNotification notification = new OwnershipNotification(title, new TextObject(string.Format("You are now the rightful owner to {0}", title.name)));
            //Campaign.Current.CampaignInformationManager.NewMapNoticeAdded(notification);
        }

        public void GiveLordshipOnKingdomJoin(Kingdom newKingdom, Clan clan)
        {
            List<FeudalTitle> clanTitles = BannerKingsConfig.Instance.TitleManager.GetAllDeJure(clan);
            if (clanTitles.Count > 0) return;

            FeudalTitle sovereign = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(newKingdom);
            if (sovereign == null || sovereign.contract == null) return;

            if (!sovereign.contract.rights.Contains(FeudalRights.Enfoeffement_Rights)) return;

            List<FeudalTitle> titles = BannerKingsConfig.Instance.TitleManager.GetAllDeJure(newKingdom.Leader);
            if (titles.Count == 0) return;

            List<FeudalTitle> lordships = titles.FindAll(x => x.type == TitleType.Lordship);
            if (lordships.Count == 0) return;

            FeudalTitle lordship = (from l in lordships where l.fief != null select l into x orderby x.fief.Village.Hearth select x).FirstOrDefault<FeudalTitle>();
            if (lordship != null)
            {
                BannerKingsConfig.Instance.TitleManager.GrantLordship(lordship, newKingdom.Leader, clan.Leader);
                if (clan == Clan.PlayerClan)
                {
                    GameTexts.SetVariable("FIEF", lordship.name);
                    GameTexts.SetVariable("SOVEREIGN", sovereign.name);
                    InformationManager.ShowInquiry(new InquiryData("Enfoeffement Right", new TextObject("You have been generously granted the {FIEF} as part of your vassal rights to the {SOVEREIGN}.").ToString(),
                    true, false, GameTexts.FindText("str_done").ToString(), null, null, null), false);
                }
            }
        }

        public List<FeudalTitle> GetAllDeJure(Hero hero)
        {
            List<FeudalTitle> list = new List<FeudalTitle>();
            foreach (KeyValuePair<FeudalTitle, Hero> pair in Titles)
                if (pair.Value == hero)
                    list.Add(pair.Key);
            return list;
        }

        public List<FeudalTitle> GetAllDeJure(Clan clan)
        {
            List<FeudalTitle> list = new List<FeudalTitle>();
            foreach (Hero hero in clan.Heroes)
                foreach (FeudalTitle title in GetAllDeJure(hero))
                    list.Add(title);

            return list;
        }
        public FeudalTitle GetHighestTitle(Hero hero)
        {
            if (hero != null && IsHeroTitleHolder(hero))
            {
                FeudalTitle highestTitle = null;
                foreach (FeudalTitle title in GetAllDeJure(hero))
                    if (highestTitle == null || title.type < highestTitle.type)
                        highestTitle = title;
                return highestTitle;
            }
            else return null;
        }

        public FeudalTitle GetHighestTitleWithinFaction(Hero hero, Kingdom faction)
        {
            if (hero != null && faction != null  && IsHeroTitleHolder(hero))
            {
                FeudalTitle highestTitle = null;
                foreach (FeudalTitle title in GetAllDeJure(hero))
                    if ((highestTitle == null || title.type < highestTitle.type) && GetTitleFaction(title) == faction)
                        highestTitle = title;
                return highestTitle;
            }
            else return null;
        }

        public FeudalTitle GetSovereignTitle(Kingdom faction)
        {
            if (faction != null && Kingdoms.ContainsKey(faction))
                return Kingdoms[faction];
            else return null;
        }

        public FeudalTitle GetSovereignFromSettlement(Settlement settlement)
        {
            FeudalTitle title = GetTitle(settlement);
            if (title != null)
                return title.sovereign;
            
            return null;
        }

        public List<FeudalTitle> GetVassals(TitleType threshold, Hero lord)
        {
            List<FeudalTitle> allTitles = GetAllDeJure(lord);
            List<FeudalTitle> vassals = new List<FeudalTitle>();
            foreach (FeudalTitle title in allTitles)
                if (title.deFacto.MapFaction == lord.MapFaction && (title.deFacto == title.deJure || title.deJure.MapFaction == lord.MapFaction)
                    && (int)title.type <= (int)threshold)
                    vassals.Add(title);
            return vassals;
        }

        public List<FeudalTitle> GetVassals(Hero lord)
        {
            List<FeudalTitle> vassals = new List<FeudalTitle>();
            FeudalTitle highest = this.GetHighestTitle(lord);
            if (highest != null) 
            {
                TitleType threshold = this.GetHighestTitle(lord).type + 1;
                List<FeudalTitle> allTitles = GetAllDeJure(lord);

                foreach (FeudalTitle title in allTitles)
                    if (title.deFacto.MapFaction == lord.MapFaction && (title.deFacto == title.deJure || title.deJure.MapFaction == lord.MapFaction)
                        && (int)title.type >= (int)threshold)
                        vassals.Add(title);
            }
            
            return vassals;
        }


        public Kingdom GetTitleFaction(FeudalTitle title)
        {
            Kingdom faction = null;
            FeudalTitle sovereign = title.sovereign;
            if (sovereign != null)
                faction = Kingdoms.FirstOrDefault(x => x.Value == sovereign).Key;
            else if (Kingdoms.ContainsValue(title))
                faction = Kingdoms.FirstOrDefault(x => x.Value == title).Key;

            return faction;
        }

        public void InitializeTitles()
        {
            XmlDocument doc = BannerKings.Helpers.Helpers.CreateDocumentFromXmlFile(BasePath.Name + "Modules/BannerKings/ModuleData/titles.xml");
            XmlNode titlesNode = doc.ChildNodes[1].ChildNodes[0];
            bool autoGenerate = bool.Parse(titlesNode.Attributes["autoGenerate"].Value);
 
            foreach (XmlNode kingdom in titlesNode)
            {
                if (kingdom.Name != "kingdom") return;

                List<FeudalTitle> vassalsKingdom = new List<FeudalTitle>();
                string factionName = kingdom.Attributes["faction"].Value;
                string deJureNameKingdom = kingdom.Attributes["deJure"].Value;
                Hero deJureKingdom = GetDeJure(deJureNameKingdom, null);
                Kingdom faction = Kingdom.All.FirstOrDefault(x => x.Name.ToString() == factionName);
                string contractType = kingdom.Attributes["contract"].Value;
                FeudalContract contract = GenerateContract(contractType);

                if (contract == null) return;

                if (kingdom.ChildNodes != null)
                    foreach (XmlNode duchy in kingdom.ChildNodes)
                    {
                        if (duchy.Name != "duchy") return;

                        List<FeudalTitle> vassalsDuchy = new List<FeudalTitle>();
                        string dukedomName = duchy.Attributes["name"].Value;
                        string deJureNameDuchy = duchy.Attributes["deJure"].Value;
                        Hero deJureDuchy = GetDeJure(deJureNameDuchy, null);

                        if (duchy.ChildNodes != null)
                            foreach (XmlNode county in duchy.ChildNodes)
                            {
                                if (county.Name != "county") return;

                                string settlementNameCounty = county.Attributes["settlement"].Value;
                                string deJureNameCounty = county.Attributes["deJure"].Value;
                                Settlement settlementCounty = Settlement.All.FirstOrDefault(x => x.Name.ToString() == settlementNameCounty);
                                Hero deJureCounty = GetDeJure(deJureNameCounty, settlementCounty);
                                List<FeudalTitle> vassalsCounty = new List<FeudalTitle>();

                                if (county.ChildNodes != null)
                                    foreach (XmlNode barony in county.ChildNodes)
                                    {
                                        if (barony.Name != "barony") return;

                                        string settlementNameBarony = barony.Attributes["settlement"].Value;
                                        string deJureIdBarony = barony.Attributes["deJure"].Value;
                                        Settlement settlementBarony = Settlement.All.FirstOrDefault(x => x.Name.ToString() == settlementNameBarony);
                                        Hero deJureBarony = GetDeJure(deJureIdBarony, settlementBarony);
                                        if (settlementBarony != null)
                                            vassalsCounty.Add(CreateLandedTitle(settlementBarony, deJureBarony, TitleType.Barony, contract));
                                    }

                                if (settlementCounty != null)
                                    vassalsDuchy.Add(CreateLandedTitle(settlementCounty, deJureCounty, TitleType.County, contract, vassalsCounty));
                            }

                        if (deJureDuchy != null && vassalsDuchy.Count > 0)
                            vassalsKingdom.Add(CreateUnlandedTitle(deJureDuchy, TitleType.Dukedom, vassalsDuchy, dukedomName, contract));
                    }

                if (deJureKingdom != null && vassalsKingdom.Count > 0 && faction != null)
                {
                    FeudalTitle sovereign = CreateKingdom(deJureKingdom, faction, TitleType.Kingdom, vassalsKingdom, contract);
                    foreach (FeudalTitle duchy in vassalsKingdom)
                        duchy.SetSovereign(sovereign);
                }
                    
            }

            if (autoGenerate) 
                foreach (Settlement settlement in Settlement.All)
                {
                    if (settlement.IsVillage) continue;
                    else {
                        if (settlement.OwnerClan != null && settlement.OwnerClan.Leader != null) 
                            CreateLandedTitle(settlement, settlement.Owner, settlement.IsTown ? TitleType.County : TitleType.Barony, GenerateContract("feudal"), null);
                    }
                }         
        }

        private Hero GetDeJure(string heroId, Settlement settlement)
        {
            Hero target = Hero.AllAliveHeroes.FirstOrDefault(x => x.StringId == heroId);
            if (target == null)
            {
                Hero hero1Dead = Hero.DeadOrDisabledHeroes.FirstOrDefault(x => x.StringId == heroId);
                if (hero1Dead != null)
                {
                    Clan clan = hero1Dead.Clan;
                    if (!clan.IsEliminated)
                        target = clan.Leader;
                    else if (clan.Kingdom != null)
                        target = clan.Kingdom.Leader;
                }
            }

            if (target == null && settlement != null)
                target = settlement.Owner;

            return target;
        }

        public void GrantLordship(FeudalTitle title, Hero giver, Hero receiver)
        {
            ExecuteOwnershipChange(giver, receiver, title, true);
            ExecuteOwnershipChange(giver, receiver, title, false);
            receiver.IsNoble = true;
        }

        public void ApplyOwnerChange(Settlement settlement, Hero newOwner)
        {
            FeudalTitle title = GetTitle(settlement);
            if (title == null) return;

            ExecuteOwnershipChange(settlement.Owner, newOwner, title, false);
            if (!settlement.IsVillage && settlement.BoundVillages != null && settlement.BoundVillages.Count > 0 && title.vassals != null &&
                title.vassals.Count > 0)
                foreach (FeudalTitle lordship in title.vassals.Where(y => y.type == TitleType.Lordship))
                    ExecuteOwnershipChange(settlement.Owner, newOwner, title, false);
        }


        public void DeactivateTitle(FeudalTitle title)
        {
            ExecuteOwnershipChange(title.deJure, null, title, true);
            ExecuteOwnershipChange(title.deFacto, null, title, false);
        }

        public void DeactivateDeJure(FeudalTitle title) => ExecuteOwnershipChange(title.deJure, null, title, true);

        public void ShowContract(Hero lord, string buttonString)
        {
            Kingdom kingdom = lord.Clan.Kingdom;
            if (kingdom == null) return;
            
            FeudalTitle sovereign = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(kingdom);
            if (sovereign == null || sovereign.contract == null) return;
            
            string description = BannerKingsConfig.Instance.TitleManager.GetContractText(sovereign);
            InformationManager.ShowInquiry(new InquiryData(string.Format("Enfoeffement Contract for {0}", sovereign.name),
                description, true, false, buttonString, "", null, null), false);
        }

        public FeudalTitle GetDuchy(FeudalTitle title)
        {
            IEnumerable<FeudalTitle> duchies = Titles.Keys.Where(x => x.type == TitleType.Dukedom && x.sovereign != null && x.sovereign == title.sovereign);

            FeudalTitle suzerain1 = this.GetImmediateSuzerain(title);
            if (suzerain1 != null)
            {
                if (suzerain1.type == TitleType.Dukedom)
                    return suzerain1;
                else
                {
                    FeudalTitle suzerain2 = this.GetImmediateSuzerain(suzerain1);
                    if (suzerain2 != null)
                    {
                        if (suzerain2.type == TitleType.Dukedom)
                            return suzerain2;
                        else
                        {
                            FeudalTitle suzerain3 = this.GetImmediateSuzerain(suzerain2);
                            if (suzerain3 != null)
                            {
                                if (suzerain3.type == TitleType.Dukedom)
                                    return suzerain3;
                            }
                        }
                    }
                }
            }

            return null;
        }

        public string GetContractText(FeudalTitle title)
        {
            FeudalContract contract = title.contract;
            StringBuilder sb = new StringBuilder(string.Format("You, {0}, formally accept to be henceforth bound to the {1}, fulfill your duties as well as uphold your rights," +
                " what can not be undone by means other than abdication of all rights and lands associated with the contract, treachery, or death.", Hero.MainHero.Name.ToString(), title.name.ToString()));
            sb.Append(Environment.NewLine);
            sb.Append("   ");
            sb.Append(Environment.NewLine);
            sb.Append("Duties");
            sb.Append(Environment.NewLine);
            foreach (KeyValuePair<FeudalDuties, float> duty in contract.duties)
            {
                sb.Append(this.GetDutyString(duty.Key, duty.Value));
                sb.Append(Environment.NewLine);
            }
            sb.Append(Environment.NewLine);
            sb.Append("   ");
            sb.Append(Environment.NewLine);
            sb.Append("Rights");
            sb.Append(Environment.NewLine);
            foreach (FeudalRights right in contract.rights)
            {
                sb.Append(GetRightString(right));
                sb.Append(Environment.NewLine);
            }
           
            return sb.ToString();
        }

        private string GetDutyString(FeudalDuties duty, float factor)
        {
            GameTexts.SetVariable("DUTY_FACTOR", (factor * 100f).ToString("0") + '%');
            string text = null;
            if (duty == FeudalDuties.Taxation)
                text = "You are due {DUTY_FACTOR} of your fiefs' income to your suzerain.";
            else if (duty == FeudalDuties.Auxilium)
                text = "You are obliged to militarily participate in armies, for {DUTY_FACTOR} of their durations.";
            else text = "You are obliged to contribute to {DUTY_FACTOR} of your suzerain's ransom.";

            return new TextObject(text).ToString();
        }

        private string GetRightString(FeudalRights right)
        {
            if (right == FeudalRights.Absolute_Land_Rights)
                return "You are entitled to ownership of any conquered lands whose title you own.";
            else if (right == FeudalRights.Enfoeffement_Rights)
                return "You are entitled to be granted land in case you have none, whenever possible.";
            else if (right == FeudalRights.Conquest_Rights)
                return "You are entitled to the ownership of any lands you conquered by yourself.";
            else return "";
        }

        private FeudalContract GenerateContract(string type)
        {
            if (type == "imperial")
                return new FeudalContract(new Dictionary<FeudalDuties, float>() {
                    { FeudalDuties.Ransom, 0.10f },
                    { FeudalDuties.Taxation, 0.4f }
                }, new List<FeudalRights>() {
                    FeudalRights.Assistance_Rights,
                    FeudalRights.Army_Compensation_Rights
                }, GovernmentType.Imperial, SuccessionType.Imperial,
                InheritanceType.Primogeniture, GenderLaw.Agnatic);
            else if (type == "tribal")
                return new FeudalContract(new Dictionary<FeudalDuties, float>() {
                    { FeudalDuties.Taxation, 0.125f },
                    { FeudalDuties.Auxilium, 0.66f }
                }, new List<FeudalRights>() {
                    FeudalRights.Conquest_Rights,
                    FeudalRights.Absolute_Land_Rights
                }, GovernmentType.Tribal, SuccessionType.Elective_Monarchy,
                InheritanceType.Seniority, GenderLaw.Agnatic);
            else if (type == "republic")
                return new FeudalContract(new Dictionary<FeudalDuties, float>() {
                    { FeudalDuties.Ransom, 0.10f },
                    { FeudalDuties.Taxation, 0.4f }
                }, new List<FeudalRights>() {
                    FeudalRights.Assistance_Rights,
                    FeudalRights.Army_Compensation_Rights
                }, GovernmentType.Republic, SuccessionType.Republic,
                InheritanceType.Primogeniture, GenderLaw.Cognatic);
            else return new FeudalContract(new Dictionary<FeudalDuties, float>() {
                    { FeudalDuties.Ransom, 0.20f },
                    { FeudalDuties.Auxilium, 0.4f }
                }, new List<FeudalRights>() {
                    FeudalRights.Absolute_Land_Rights,
                    FeudalRights.Enfoeffement_Rights
                }, GovernmentType.Feudal, SuccessionType.Hereditary_Monarchy,
                InheritanceType.Primogeniture, GenderLaw.Agnatic);
        }

        private FeudalTitle CreateKingdom(Hero deJure, Kingdom faction, TitleType type, List<FeudalTitle> vassals, FeudalContract contract)
        {
            FeudalTitle title = new FeudalTitle(type, null, vassals, deJure, faction.Leader, faction.Name.ToString(), contract);
            ExecuteAddTitle(title);
            Kingdoms.Add(faction, title);
            return title;
        }

        private FeudalTitle CreateUnlandedTitle(Hero deJure, TitleType type, List<FeudalTitle> vassals, string name, FeudalContract contract)
        {
            FeudalTitle title = new FeudalTitle(type, null, vassals, deJure, deJure, name, contract);
            ExecuteAddTitle(title);
            return title;
        }
            
        private FeudalTitle CreateLandedTitle(Settlement settlement, Hero deJure, TitleType type, FeudalContract contract, List<FeudalTitle> vassals = null)
        {
            Hero deFacto = settlement.OwnerClan.Leader;
            if (deJure == null) deJure = settlement.Owner;
            if (vassals == null) vassals = new List<FeudalTitle>();
            if (settlement.BoundVillages != null)
                foreach (Village lordship in settlement.BoundVillages)
                {
                    FeudalTitle lordshipTitle = CreateLordship(lordship.Settlement, deJure, contract);
                    vassals.Add(lordshipTitle);
                    ExecuteAddTitle(lordshipTitle);
                }
            FeudalTitle title = new FeudalTitle(type, settlement, vassals, deJure, deFacto, settlement.Name.ToString(), contract);
            ExecuteAddTitle(title);
            return title;
        }

        private FeudalTitle CreateLordship(Settlement settlement, Hero deJure, FeudalContract contract) => new FeudalTitle(TitleType.Lordship, settlement, null,
            deJure, settlement.Village.Bound.Owner, settlement.Name.ToString(), contract);

        public class FeudalTitle
        {
            [SaveableProperty(1)]
            public TitleType type { get; private set; }

            [SaveableProperty(2)]
            public Settlement fief { get; private set; }

            [SaveableProperty(3)]
            public List<FeudalTitle> vassals { get; private set; }

            [SaveableProperty(4)]
            public Hero deJure { get; internal set; }

            [SaveableProperty(5)]
            public Hero deFacto { get; internal set; }

            [SaveableProperty(6)]
            public TextObject name { get; private set; }

            [SaveableProperty(7)]
            public TextObject shortName { get; private set; }

            [SaveableProperty(8)]
            public float dueTax { get; set; }

            [SaveableProperty(9)]
            public FeudalTitle sovereign { get; private set; }

            [SaveableProperty(10)]
            public FeudalContract contract { get; private set;  }

            public override bool Equals(object obj)
            {
                if (obj is FeudalTitle)
                {
                    FeudalTitle target = (FeudalTitle)obj;
                    return this.fief != null ? this.fief == target.fief : this.name == target.name;
                }
                return base.Equals(obj);
            }

            public FeudalTitle(TitleType type, Settlement fief, List<FeudalTitle> vassals, Hero deJure, Hero deFacto, string name, FeudalContract contract)
            {
                this.type = type;
                this.fief = fief;
                this.vassals = vassals;
                this.deJure = deJure;
                this.deFacto = deFacto;
                this.name = new TextObject(BannerKings.Helpers.Helpers.GetTitlePrefix(type) + " of " + name);
                this.shortName = new TextObject(name);
                this.contract = contract;
                dueTax = 0;
            }

            public Hero DeFacto
            {
                get
                {
                    return this.deFacto;
                }
            }

            public bool Active => this.deJure != null || this.deFacto != null;

            public void SetSovereign(FeudalTitle sovereign)
            {
                this.sovereign = sovereign;
                if (this.vassals != null && this.vassals.Count > 0)
                    foreach (FeudalTitle vassal in this.vassals)
                        vassal.SetSovereign(sovereign);
            }
        }

        public class FeudalContract
        {
            [SaveableProperty(1)]
            public Dictionary<FeudalDuties, float> duties { get; private set; }

            [SaveableProperty(2)]
            public List<FeudalRights> rights { get; private set; }

            [SaveableProperty(3)]
            public GovernmentType government { get; private set; }

            [SaveableProperty(4)]
            public SuccessionType succession { get; private set; }

            [SaveableProperty(5)]
            public InheritanceType inheritance { get; private set; }

            [SaveableProperty(6)]
            public GenderLaw genderLaw { get; private set; }

            public FeudalContract(Dictionary<FeudalDuties, float> duties, List<FeudalRights> rights, GovernmentType government,
                SuccessionType succession, InheritanceType inheritance, GenderLaw genderLaw)
            {
                this.duties = duties;
                this.rights = rights;
                this.government = government;
                this.succession = succession;
                this.inheritance = inheritance;
                this.genderLaw = genderLaw;
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

        public class UsurpCosts
        {
            public float gold { get; private set; }
            public float influence { get; private set; }
            public float renown { get; private set; }

            public UsurpCosts(float gold, float influence, float renown)
            {
                this.gold = gold;
                this.influence = influence;
                this.renown = renown;
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

        public enum CasusBelli
        {
            None,
            Conquest,
            Provocation,
            Lawful_Claim,
            Imperial_Reconquest
        }

        public enum LegitimacyType
        {
            Lawful,
            Lawful_Foreigner,
            Unlawful,
            Unlawful_Foreigner
        }

        public enum SuccessionType
        {
            Hereditary_Monarchy,
            Elective_Monarchy,
            Imperial,
            Republic
        }

        public enum InheritanceType
        {
            Primogeniture,
            Ultimogeniture,
            Seniority
        }

        public enum GenderLaw
        {
            Agnatic,
            Cognatic
        }

        public enum GovernmentType
        {
            Feudal,
            Tribal,
            Imperial,
            Republic
        }
    }
}
