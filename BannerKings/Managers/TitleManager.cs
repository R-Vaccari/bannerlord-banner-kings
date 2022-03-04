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

namespace BannerKings.Managers
{
    public class TitleManager
    {
        public HashSet<FeudalTitle> TITLES { get; set; }
        public Dictionary<Hero, HashSet<FeudalTitle>> TITLE_HOLDERS { get; set; }
        public Dictionary<Kingdom, FeudalTitle> KINGDOMS { get; set; }

        public TitleManager(HashSet<FeudalTitle> titles, Dictionary<Hero, HashSet<FeudalTitle>> titleHolders, Dictionary<Kingdom, FeudalTitle> kingdoms)
        {
            this.TITLES = titles;
            this.TITLE_HOLDERS = titleHolders;
            this.KINGDOMS = kingdoms;
            InitializeTitles();
        }

        private bool IsSettlementPresent(Settlement settlement) => TITLES.FirstOrDefault(x => x.fief == settlement) != null;
        public bool IsHeroTitleHolder(Hero hero) => TITLE_HOLDERS.ContainsKey(hero);
        public FeudalTitle GetTitle(Settlement settlement) => TITLES.FirstOrDefault(x => x.fief == settlement);
        public Hero GetDeJure(Settlement settlement) => TITLES.FirstOrDefault(x => x.fief == settlement).deJure;
        public bool IsHeroKnighted(Hero hero) => hero.IsNoble && TITLE_HOLDERS.ContainsKey(hero);
        public FeudalTitle GetImmediateSuzerain(FeudalTitle title) => TITLES.FirstOrDefault(x => x.vassals != null && x.vassals.Count > 0 && x.vassals.Contains(title));

        private void ExecuteOwnershipChange(Hero oldOwner, Hero newOwner, FeudalTitle title, bool deJure)
        {
            if (deJure)
                title.deJure = newOwner;
            else title.deFacto = newOwner;

            bool stillPresent = title.deFacto == oldOwner || title.deJure == oldOwner;
            AddTitleHolder(newOwner, title);
            if (!stillPresent) RemoveFromTitleHolder(oldOwner, title);
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

        private void RemoveFromTitleHolder(Hero hero, FeudalTitle title)
        {
            if (TITLE_HOLDERS.ContainsKey(hero) && TITLE_HOLDERS[hero].Contains(title))
            {
                TITLE_HOLDERS[hero].Remove(title);
                if (TITLE_HOLDERS[hero].Count == 0)
                    TITLE_HOLDERS.Remove(hero);
            }
        }

        public FeudalTitle CalculateHeroSuzerain(Hero hero)  
        {
            FeudalTitle title = GetHighestTitle(hero);
            Kingdom kingdom1 = GetTitleFaction(title);

            if (kingdom1 == null || hero.Clan.Kingdom == null) return null;

            FeudalTitle suzerain = TITLES.FirstOrDefault(x => x.vassals != null && x.vassals.Count > 0 && x.vassals.Contains(title));
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
                        FeudalTitle suzerainFaction = TITLES.FirstOrDefault(x => x.vassals != null && x.vassals.Count > 0 && x.vassals.Contains(factionTitle));
                        return suzerainFaction;
                    } else
                        return GetHighestTitle(kingdom1.Leader);
                }
            }

            return null;
        }

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

        public void InheritTitles(Hero oldOwner, Hero heir)
        {
            if (IsHeroTitleHolder(oldOwner))
            {
                HashSet<FeudalTitle> set = GetTitles(oldOwner);
                HashSet<FeudalTitle> titles = new HashSet<FeudalTitle>(set);
                foreach (FeudalTitle title in titles)
                {
                    if (title.deJure == oldOwner) ExecuteOwnershipChange(oldOwner, heir, title, true);
                    if (title.deFacto == oldOwner) ExecuteOwnershipChange(oldOwner, heir, title, false);
                }
            }
        }

        public void UsurpTitle(Hero oldOwner, Hero usurper, FeudalTitle title, UsurpCosts costs)
        {
            ExecuteOwnershipChange(oldOwner, usurper, title, true);
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

        public FeudalTitle GetHighestTitleWithinFaction(Hero hero, Kingdom faction)
        {
            if (IsHeroTitleHolder(hero))
            {
                FeudalTitle highestTitle = null;
                foreach (FeudalTitle title in TITLE_HOLDERS[hero])
                    if ((highestTitle == null || title.type < highestTitle.type) && GetTitleFaction(title) == faction)
                        highestTitle = title;
                return highestTitle;
            }
            else return null;
        }

        public FeudalTitle GetSovereignTitle(Kingdom faction)
        {
            if (KINGDOMS.ContainsKey(faction))
                return KINGDOMS[faction];
            else return null;
        }

        public Kingdom GetFactionFromSettlement(Settlement settlement)
        {
            FeudalTitle title = TITLES.FirstOrDefault(x => x.fief != null && x.fief == settlement);
            if (title != null)
            {
                Kingdom faction = KINGDOMS.FirstOrDefault(x => x.Value == title.sovereign).Key;
                return faction;
            }
            return null;
        }

        public FeudalTitle GetSovereignFromSettlement(Settlement settlement)
        {
            FeudalTitle title = TITLES.FirstOrDefault(x => x.fief != null && x.fief == settlement);
            if (title != null)
                return title.sovereign;
            
            return null;
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

        public HashSet<FeudalTitle> GetVassals(Hero lord)
        {
            TitleType threshold = this.GetHighestTitle(lord).type + 1;
            HashSet<FeudalTitle> allTitles = TITLE_HOLDERS[lord];
            HashSet<FeudalTitle> vassals = new HashSet<FeudalTitle>();
            foreach (FeudalTitle title in allTitles)
                if (title.deFacto.MapFaction == lord.MapFaction && (title.deFacto == title.deJure || title.deJure.MapFaction == lord.MapFaction)
                    && (int)title.type <= (int)threshold)
                    vassals.Add(title);
            return vassals;
        }


        public Kingdom GetTitleFaction(FeudalTitle title)
        {
            Kingdom faction = null;
            FeudalTitle sovereign = title.sovereign;
            if (sovereign != null)
                faction = KINGDOMS.FirstOrDefault(x => x.Value == sovereign).Key;
            else if (KINGDOMS.ContainsValue(title))
                faction = KINGDOMS.FirstOrDefault(x => x.Value == title).Key;

            return faction;
        }

        public IEnumerable<SuccessionType> GetValidSuccessions(GovernmentType government)
        {
            if (government == GovernmentType.Feudal)
            {
                yield return SuccessionType.Hereditary_Monarchy;
                yield return SuccessionType.Elective_Monarchy;
                yield break;
            } else if (government == GovernmentType.Imperial)
            {
                yield return SuccessionType.Imperial;
                yield break;
            } else if (government == GovernmentType.Republic)
            {
                yield return SuccessionType.Republic;
                yield break;
            } else
            {
                yield return SuccessionType.Elective_Monarchy;
                yield break;
            }   
        }


        public void InitializeTitles()
        {
            XmlDocument doc = BannerKings.Helpers.Helpers.CreateDocumentFromXmlFile(BasePath.Name + "Modules/BannerKings/ModuleData/titles.xml");
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
                        bool present = IsSettlementPresent(settlement);
                        if (!present && settlement.OwnerClan != null && settlement.OwnerClan.Leader != null) 
                            CreateLandedTitle(settlement, settlement.Owner, settlement.IsTown ? TitleType.County : TitleType.Barony, GenerateContract("feudal"), null);
                    }
                }         
        }

        public void GrantLordship(FeudalTitle title, Hero giver, Hero receiver)
        {
            ExecuteOwnershipChange(giver, receiver, title, true);
            ExecuteOwnershipChange(giver, receiver, title, false);
            receiver.IsNoble = true;
        }

        public void ApplyOwnerChange(Settlement settlement, Hero newOwner)
        {
            FeudalTitle title = TITLES.FirstOrDefault(x => x.fief == settlement);
            if (title == null) return;

            ExecuteOwnershipChange(settlement.Owner, newOwner, title, false);
            if (!settlement.IsVillage && settlement.BoundVillages != null && settlement.BoundVillages.Count > 0 && title.vassals != null &&
                title.vassals.Count > 0)
                foreach (FeudalTitle lordship in title.vassals.Where(y => y.type == TitleType.Lordship))
                    ExecuteOwnershipChange(settlement.Owner, newOwner, title, false);
        }

        public void ShowContract(Hero lord, string buttonString)
        {
            FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(lord);
            string description = BannerKingsConfig.Instance.TitleManager.GetContractText(title);
            InformationManager.ShowInquiry(new InquiryData(string.Format("Enfoeffement Contract for {0}", title.name),
                description, true, false, buttonString, "", null, null), false);
        }

        public FeudalTitle GetDuchy(FeudalTitle title)
        {

            IEnumerable<FeudalTitle> duchies = TITLES.Where(x => x.type == TitleType.Dukedom && x.sovereign != null && x.sovereign == title.sovereign);
            FeudalTitle result = null;
            foreach (FeudalTitle duchy in duchies)
                if (duchy.vassals.Contains(title))
                    result = duchy;

            if (result == null)
                foreach (FeudalTitle duchy in duchies)
                    foreach (FeudalTitle county in duchy.vassals)
                        if (county.vassals.Contains(title))
                            result = duchy;

            if (result == null)
                foreach (FeudalTitle duchy in duchies)
                    foreach (FeudalTitle county in duchy.vassals)
                        foreach (FeudalTitle barony in county.vassals)
                            if (barony.vassals.Contains(title))
                                result = duchy;

            return result;
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
                if (duty.Key != FeudalDuties.Auxilium) sb.Append(string.Format(this.GetDutyString(duty.Key), duty.Value));
                else sb.Append(this.GetDutyString(duty.Key));
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
            else if (right == FeudalRights.Conquest_Rights)
                return "You are entitle to the ownership of any lands you conquered by yourself.";
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
                }, GovernmentType.Imperial, SuccessionType.Imperial,
                InheritanceType.Primogeniture, GenderLaw.Agnatic);
            else if (type == "tribal")
                return new FeudalContract(new Dictionary<FeudalDuties, float>() {
                    { FeudalDuties.Taxation, 0.125f },
                    { FeudalDuties.Auxilium, 0.66f }
                }, new HashSet<FeudalRights>() {
                    FeudalRights.Conquest_Rights,
                    FeudalRights.Absolute_Land_Rights
                }, GovernmentType.Tribal, SuccessionType.Elective_Monarchy,
                InheritanceType.Seniority, GenderLaw.Agnatic);
            else if (type == "republic")
                return new FeudalContract(new Dictionary<FeudalDuties, float>() {
                    { FeudalDuties.Ransom, 0.10f },
                    { FeudalDuties.Taxation, 0.4f }
                }, new HashSet<FeudalRights>() {
                    FeudalRights.Assistance_Rights,
                    FeudalRights.Army_Compensation_Rights
                }, GovernmentType.Republic, SuccessionType.Republic,
                InheritanceType.Primogeniture, GenderLaw.Cognatic);
            else return new FeudalContract(new Dictionary<FeudalDuties, float>() {
                    { FeudalDuties.Ransom, 0.20f },
                    { FeudalDuties.Auxilium, 0.4f }
                }, new HashSet<FeudalRights>() {
                    FeudalRights.Absolute_Land_Rights,
                    FeudalRights.Enfoeffement_Rights
                }, GovernmentType.Feudal, SuccessionType.Hereditary_Monarchy,
                InheritanceType.Primogeniture, GenderLaw.Agnatic);
        }

        private FeudalTitle CreateKingdom(Hero deJure, Kingdom faction, TitleType type, HashSet<FeudalTitle> vassals, FeudalContract contract)
        {
            FeudalTitle title = new FeudalTitle(type, null, vassals, deJure, faction.Leader, faction.Name.ToString(), contract);
            AddTitleAndHolder(deJure, title);
            KINGDOMS.Add(faction, title);
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
            public TitleType type { get; private set; }
            public Settlement fief { get; private set; }
            public HashSet<FeudalTitle> vassals { get; private set; }
            public Hero deJure { get; internal set; }
            public Hero deFacto { get; internal set; }
            public TextObject name { get; private set; }
            public TextObject shortName { get; private set; }
            public float dueTax { get; set; }
            public FeudalTitle sovereign { get; private set; }
            public FeudalContract contract { get; private set;  }

            public FeudalTitle(TitleType type, Settlement fief, HashSet<FeudalTitle> vassals, Hero deJure, Hero deFacto, string name, FeudalContract contract)
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
            public Dictionary<FeudalDuties, float> duties { get; private set; }
            public HashSet<FeudalRights> rights { get; private set; }
            public GovernmentType government { get; private set; }
            public SuccessionType succession { get; private set; }
            public InheritanceType inheritance { get; private set; }
            public GenderLaw genderLaw { get; private set; }

            public FeudalContract(Dictionary<FeudalDuties, float> duties, HashSet<FeudalRights> rights, GovernmentType government,
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
