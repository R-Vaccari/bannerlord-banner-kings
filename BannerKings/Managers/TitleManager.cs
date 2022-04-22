using BannerKings.Managers.Titles;
using BannerKings.Models;
using BannerKings.Models.BKModels;
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
        private Dictionary<Kingdom, FeudalTitle> Kingdoms { get; set; }

        [SaveableProperty(3)]
        public bool Knighthood { get; set; } = true;

        private Dictionary<Hero, List<FeudalTitle>> DeJures { get; set; }


        public TitleManager(Dictionary<FeudalTitle, Hero> titles, Dictionary<Hero, List<FeudalTitle>> titleHolders, Dictionary<Kingdom, FeudalTitle> kingdoms)
        {
            this.Titles = titles;
            this.Kingdoms = kingdoms;
            this.Knighthood = true;
            InitializeTitles();
            RefreshDeJure();
        }

        public void FixTitles()
        {
            foreach (FeudalTitle title in Titles.Keys)
            {
                if (title.type == TitleType.Dukedom)
                {
                    if (title.shortName.ToString() == "1" && title.deJure.StringId == "lord_3_1")
                        title.SetName(new TextObject("Perassia"));
                    else if (title.shortName.ToString() == "2" && title.deJure.StringId == "lord_3_16")
                        title.SetName(new TextObject("Jarjarys"));
                    else if (title.shortName.ToString() == "2" && title.deJure.StringId == "lord_3_5")
                        title.SetName(new TextObject("Retana"));
                    else if (title.shortName.ToString() == "2" && title.deJure.StringId == "lord_3_3")
                        title.SetName(new TextObject("Caldea"));
                    else if (title.shortName.ToString() == "1" && title.deJure.StringId == "lord_6_17")
                        title.SetName(new TextObject("Kohi Rohini"));
                    else if (title.shortName.ToString() == "1" && title.deJure.StringId == "lord_6_1")
                        title.SetName(new TextObject("Tanaz Baikal"));
                    else if (title.shortName.ToString() == "1" && title.deJure.StringId == "lord_6_4")
                        title.SetName(new TextObject("Bars Dagan"));
                    else if (title.shortName.ToString() == "1" && title.deJure.StringId == "lord_6_5")
                        title.SetName(new TextObject("Devseg"));
                }
            }
        }

        public void RefreshDeJure()
        {
            if (this.DeJures == null) this.DeJures = new Dictionary<Hero, List<FeudalTitle>>();
            else this.DeJures.Clear();
            foreach (FeudalTitle title in this.Titles.Keys.ToList())
            {
                Hero hero = title.deJure;
                if (!this.DeJures.ContainsKey(hero))
                    this.DeJures.Add(hero, new List<FeudalTitle>() { title });
                else this.DeJures[hero].Add(title);
            }
        }

        public bool IsHeroTitleHolder(Hero hero)
        {
            foreach (FeudalTitle title in Titles.Keys.ToList())
                if (title.deFacto == hero || title.deJure == hero)
                    return true;

            return false;
        }

        public FeudalTitle GetTitle(Settlement settlement)
        {
            try
            {
                if (BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
                {
                    TitleData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement).TitleData;
                    if (data != null) return data.Title;
                }
                return Titles.Keys.ToList().Find(x => x.fief == settlement);
            }
            catch (Exception ex)
            {
                string cause = "Exception in Banner Kings GetTitle method. ";
                string objInfo = null;
                if (settlement != null)
                    objInfo = string.Format("Name [{0}], Id [{1}], Culture [{2}].", settlement.Name, settlement.StringId, settlement.Culture);
                else objInfo = "Null settlement.";

                throw new BannerKingsException(cause + objInfo, ex);
            }
        }

        public List<FeudalTitle> GetAllTitlesByType(TitleType type) => this.Titles.Keys.ToList().FindAll(x => x.type == type);

        public FeudalTitle GetTitleByName(String name)
        {
            return this.Titles.FirstOrDefault(x => x.Key.FullName.ToString() == name).Key;
        }

        public GovernmentType GetSettlementGovernment(Settlement settlement)
        {
            GovernmentType type = GovernmentType.Feudal;
            FeudalTitle title = this.GetTitle(settlement);
            if (title != null)
                if (title.contract != null)
                    type = title.contract.Government;

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
                if (deJure)
                {
                    title.deJure = newOwner;
                    Titles[title] = newOwner;
                    DeJures[oldOwner].Remove(title);
                    if (DeJures.ContainsKey(newOwner))
                        DeJures[newOwner].Add(title);
                    else DeJures.Add(newOwner, new List<FeudalTitle>() { title });
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

        public Dictionary<Clan, List<FeudalTitle>> CalculateVassalClanTitles(Clan suzerainClan)
        {
            Dictionary<Clan, List<FeudalTitle>> clans = new Dictionary<Clan, List<FeudalTitle>> ();
            Kingdom kingdom = suzerainClan.Kingdom;
            if (kingdom == null || suzerainClan == null) return clans;

            List<FeudalTitle> suzerainTitles = this.GetAllDeJure(suzerainClan);
            if (suzerainTitles.Count == 0) return clans;

            foreach (FeudalTitle title in suzerainTitles)
                if (title.vassals != null && title.vassals.Count > 0)
                    foreach (FeudalTitle vassal in title.vassals)
                    {
                        if (vassal.deJure.Clan == suzerainClan) continue;

                        FeudalTitle vassalSuzerain = this.CalculateHeroSuzerain(vassal.deJure);
                        if (vassalSuzerain == null) continue;
                        else
                        {
                            Clan suzerainDeJureClan = vassalSuzerain.deJure.Clan;
                            if (suzerainDeJureClan == suzerainClan) 
                            {
                                Clan vassalDeJureClan = vassal.deJure.Clan;
                                if (!clans.ContainsKey(vassalDeJureClan))
                                    clans.Add(vassalDeJureClan, new List<FeudalTitle>() { vassal });
                                else clans[vassalDeJureClan].Add(title);
                            }
                        }
                    }
            

            return clans;
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

        public void AddOngoingClaim(TitleAction action)
        {
            Hero claimant = action.ActionTaker;
            action.Title.AddOngoingClaim(action.ActionTaker);
            GainKingdomInfluenceAction.ApplyForDefault(claimant, -action.Influence);
            claimant.ChangeHeroGold((int)-action.Gold);
            claimant.Clan.Renown -= action.Renown;
            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(action.ActionTaker, action.Title.deJure, (int)Math.Min(-5f, (float)new BKTitleModel().GetRelationImpact(action.Title) * -0.1f), true);

            if (action.Title.deJure == Hero.MainHero)
                InformationManager.AddQuickInformation(new TextObject("{=!}{CLAIMANT} is building a claim on your title, {TITLE}.")
                    .SetTextVariable("CLAIMANT", claimant.Name)
                    .SetTextVariable("TITLE", action.Title.FullName));
        }
        
        public void GrantTitle(Hero receiver, Hero grantor, FeudalTitle title, float influence)
        {
            ExecuteOwnershipChange(grantor, receiver, title, true);
            Kingdom kingdom = grantor.Clan.Kingdom;
            if (receiver.Clan.Kingdom != null && receiver.Clan.Kingdom == kingdom)
                ExecuteOwnershipChange(grantor, receiver, title, false);

            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(grantor, receiver, (int)((float)new BKTitleModel().GetRelationImpact(title) * -1f), true);
            GainKingdomInfluenceAction.ApplyForDefault(grantor, influence);
        }

        public void UsurpTitle(Hero oldOwner, TitleAction action)
        {
            Hero usurper = action.ActionTaker;
            FeudalTitle title = action.Title;
            InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=!}{USURPER} has usurped the {TITLE}.")
                .SetTextVariable("USURPER", usurper.Name)
                .SetTextVariable("TITLE", action.Title.FullName)
                .ToString()));
            if (title.deJure == Hero.MainHero)
                InformationManager.AddQuickInformation(new TextObject("{=!}{USURPER} has usurped your title, {TITLE}.")
                    .SetTextVariable("USURPER", usurper.Name)
                    .SetTextVariable("TITLE", action.Title.FullName));
           
            int impact = new BKTitleModel().GetRelationImpact(title);
            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(usurper, oldOwner, impact, true);
            Kingdom kingdom = oldOwner.Clan.Kingdom;
            if (kingdom != null) 
                foreach(Clan clan in kingdom.Clans) 
                    if (clan != oldOwner.Clan && clan != usurper.Clan && !clan.IsUnderMercenaryService)
                    {
                        int random = MBRandom.RandomInt(1, 100);
                        if (random <= 10)
                            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(usurper, oldOwner, (int)((float)impact * 0.3f), true);
                    }

            if (action.Gold > 0)
                usurper.ChangeHeroGold((int)-action.Gold);
            if (action.Influence > 0)
                usurper.Clan.Influence -= action.Influence;
            if (action.Renown > 0)
                usurper.Clan.Renown -= action.Renown;

            title.RemoveClaim(usurper);
            title.AddClaim(oldOwner, ClaimType.Previous_Owner, true);
            ExecuteOwnershipChange(oldOwner, usurper, title, true);

            //OwnershipNotification notification = new OwnershipNotification(title, new TextObject(string.Format("You are now the rightful owner to {0}", title.name)));
            //Campaign.Current.CampaignInformationManager.NewMapNoticeAdded(notification);
        }

        public void GiveLordshipOnKingdomJoin(Kingdom newKingdom, Clan clan)
        {
            List<FeudalTitle> clanTitles = BannerKingsConfig.Instance.TitleManager.GetAllDeJure(clan);
            if (clanTitles.Count > 0) return;

            FeudalTitle sovereign = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(newKingdom);
            if (sovereign == null || sovereign.contract == null) return;

            if (!sovereign.contract.Rights.Contains(FeudalRights.Enfoeffement_Rights)) return;

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
                    GameTexts.SetVariable("FIEF", lordship.FullName);
                    GameTexts.SetVariable("SOVEREIGN", sovereign.FullName);
                    InformationManager.ShowInquiry(new InquiryData("Enfoeffement Right", new TextObject("You have been generously granted the {FIEF} as part of your vassal rights to the {SOVEREIGN}.").ToString(),
                    true, false, GameTexts.FindText("str_done").ToString(), null, null, null), false);
                }
            }
        }

        public List<FeudalTitle> GetAllDeJure(Hero hero)
        {
            if (this.DeJures != null)
            {
                List<FeudalTitle> titleList = null;
                this.DeJures.TryGetValue(hero, out titleList);
                if (titleList == null) titleList = new List<FeudalTitle>();
                return titleList;
            }
                
            
            List<FeudalTitle> list = new List<FeudalTitle>();
            foreach (FeudalTitle title in Titles.Keys.ToList())
                if (title.deJure == hero)
                    list.Add(title);
            return list;
        }

        public List<FeudalTitle> GetAllDeJure(Clan clan)
        {
            List<FeudalTitle> list = new List<FeudalTitle>();
            foreach (Hero hero in clan.Heroes)
                    list.AddRange(GetAllDeJure(hero));

            return list;
        }
        public FeudalTitle GetHighestTitle(Hero hero)
        {
            if (hero != null)
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
            try
            {
                if (faction != null && Kingdoms.ContainsKey(faction))
                    return Kingdoms[faction];
                else return null;
            }
            catch (Exception ex)
            {
                string cause = "Exception in Banner Kings GetSovereignTitle method. ";
                string objInfo = null;
                if (faction != null)
                    objInfo = string.Format("Name [{0}], Id [{1}], Culture [{2}].", faction.Name, faction.StringId, faction.Culture);
                else objInfo = "Null faction.";

                throw new BannerKingsException(cause + objInfo, ex);
            }
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
            InformationManager.ShowInquiry(new InquiryData(string.Format("Enfoeffement Contract for {0}", sovereign.FullName),
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
                " what can not be undone by means other than abdication of all rights and lands associated with the contract, treachery, or death.", Hero.MainHero.Name.ToString(), title.FullName.ToString()));
            sb.Append(Environment.NewLine);
            sb.Append("   ");
            sb.Append(Environment.NewLine);
            sb.Append("Duties");
            sb.Append(Environment.NewLine);
            foreach (KeyValuePair<FeudalDuties, float> duty in contract.Duties)
            {
                sb.Append(this.GetDutyString(duty.Key, duty.Value));
                sb.Append(Environment.NewLine);
            }
            sb.Append(Environment.NewLine);
            sb.Append("   ");
            sb.Append(Environment.NewLine);
            sb.Append("Rights");
            sb.Append(Environment.NewLine);
            foreach (FeudalRights right in contract.Rights)
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

        public IEnumerable<GovernmentType> GetSuccessionTypes()
        {
            yield return GovernmentType.Feudal;
            yield return GovernmentType.Tribal;
            yield return GovernmentType.Imperial;
            yield return GovernmentType.Republic;
            yield break;
        }

        public IEnumerable<InheritanceType> GetInheritanceTypes()
        {
            yield return InheritanceType.Primogeniture;
            yield return InheritanceType.Ultimogeniture;
            yield return InheritanceType.Seniority;
            yield break;
        }

        public IEnumerable<GenderLaw> GetGenderLawTypes()
        {
            yield return GenderLaw.Agnatic;
            yield return GenderLaw.Cognatic;
            yield break;
        }

        public IEnumerable<GovernmentType> GetGovernmentTypes()
        {
            yield return GovernmentType.Feudal;
            yield return GovernmentType.Tribal;
            yield return GovernmentType.Imperial;
            yield return GovernmentType.Republic;
            yield break;
        }
    }
}
