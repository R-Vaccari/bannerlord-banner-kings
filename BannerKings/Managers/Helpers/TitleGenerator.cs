using BannerKings.Managers.Titles;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;
using BannerKings.Managers.Titles.Governments;
using BannerKings.Managers.Skills;
using BannerKings.UI.Cutscenes;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using System.IO;

namespace BannerKings.Managers.Helpers
{
    public static class TitleGenerator
    {
        internal static FeudalTitle CreateKingdom(Hero deJure, Kingdom faction, TitleType type, List<FeudalTitle> vassals, FeudalContract contract, string stringId = null)
        {
            var title = new FeudalTitle(type, null, vassals, deJure, faction.Leader, faction.Name, contract, stringId);
            BannerKingsConfig.Instance.TitleManager.ExecuteAddTitle(title);
            BannerKingsConfig.Instance.TitleManager.Kingdoms[title] = faction;
            return title;
        }

        internal static FeudalTitle CreateEmpire(Hero deJure, Kingdom faction, List<FeudalTitle> vassals,
          FeudalContract contract, string stringId = null)
        {
            var title = new FeudalTitle(TitleType.Empire, null, vassals, deJure, faction.Leader,
                faction.Name, contract, stringId);
            BannerKingsConfig.Instance.TitleManager.ExecuteAddTitle(title);
            BannerKingsConfig.Instance.TitleManager.Kingdoms[title] = faction;

            return title;
        }

        public static void FoundKingdom(TitleAction action)
        {
            var title = CreateKingdom(action.ActionTaker, action.ActionTaker.Clan.Kingdom, TitleType.Kingdom, new List<FeudalTitle>(action.Vassals), action.Title.Contract);
            action.ActionTaker.Clan.AddRenown(action.Renown);

            action.Title.DriftTitle(title, false);
            foreach (var vassal in action.Vassals)
            {
                vassal.DriftTitle(title);
            }

            action.ActionTaker.ChangeHeroGold(-(int)action.Gold);
            GainKingdomInfluenceAction.ApplyForDefault(action.ActionTaker, -action.Influence);
            MBInformationManager.AddQuickInformation(new TextObject("{=dFTm4AbE}The {TITLE} has been founded by {FOUNDER}.")
                    .SetTextVariable("FOUNDER", action.ActionTaker.EncyclopediaLinkWithName)
                    .SetTextVariable("TITLE", title.FullName),
                0, null, "event:/ui/notification/relation");
            action.ActionTaker.AddSkillXp(BKSkills.Instance.Lordship,
                BannerKingsConfig.Instance.TitleModel.GetSkillReward(TitleType.Kingdom, action.Type));
        }

        public static void FoundEmpire(TitleAction action, TextObject factionName, string stringId = null, string contractType = null)
        {
            var kingdom = action.ActionTaker.Clan.Kingdom;
            kingdom.ChangeKingdomName(factionName, factionName);
            var title = CreateEmpire(action.ActionTaker, kingdom, new List<FeudalTitle>(action.Vassals),
                GenerateContract(contractType), stringId);
            action.ActionTaker.Clan.AddRenown(action.Renown);


            foreach (var vassal in action.Vassals)
            {
                vassal.DriftTitle(title);
            }

            action.ActionTaker.ChangeHeroGold(-(int)action.Gold);
            GainKingdomInfluenceAction.ApplyForDefault(action.ActionTaker, -action.Influence);
            MBInformationManager.ShowSceneNotification(new EmpireFoundedScene(kingdom, title));
            action.ActionTaker.AddSkillXp(BKSkills.Instance.Lordship,
                BannerKingsConfig.Instance.TitleModel.GetSkillReward(TitleType.Empire, action.Type));
        }

        private static Hero GetDeJure(string heroId, Settlement settlement)
        {
            var target = Hero.AllAliveHeroes.FirstOrDefault(x => x.StringId == heroId);
            if (target == null)
            {
                var hero1Dead = Hero.DeadOrDisabledHeroes.FirstOrDefault(x => x.StringId == heroId);
                if (hero1Dead != null)
                {
                    var clan = hero1Dead.Clan;
                    if (!clan.IsEliminated)
                    {
                        target = clan.Leader;
                    }
                    else if (clan.Kingdom != null)
                    {
                        target = clan.Kingdom.Leader;
                    }
                }
            }

            if (target == null && settlement != null)
            {
                target = settlement.Owner;
            }

            return target;
        }

        internal static FeudalContract GenerateContract(string type)
        {
            var contract = type switch
            {
                "feudal_elective" => new FeudalContract(
                    new Dictionary<FeudalDuties, float> { { FeudalDuties.Ransom, 0.20f }, { FeudalDuties.Auxilium, 0.4f } },
                    new List<FeudalRights> { FeudalRights.Absolute_Land_Rights, FeudalRights.Enfoeffement_Rights },
                    DefaultGovernments.Instance.Feudal, 
                    DefaultSuccessions.Instance.FeudalElective,
                    DefaultInheritances.Instance.Primogeniture,
                    DefaultGenderLaws.Instance.Agnatic),
                "imperial" => new FeudalContract(
                    new Dictionary<FeudalDuties, float> { { FeudalDuties.Ransom, 0.10f }, { FeudalDuties.Taxation, 0.4f } },
                    new List<FeudalRights> { FeudalRights.Assistance_Rights, FeudalRights.Army_Compensation_Rights },
                     DefaultGovernments.Instance.Feudal, DefaultSuccessions.Instance.FeudalElective,
                     DefaultInheritances.Instance.Primogeniture,
                     DefaultGenderLaws.Instance.Agnatic),
                "tribal" => new FeudalContract(
                    new Dictionary<FeudalDuties, float>
                    {
                        {FeudalDuties.Taxation, 0.125f}, {FeudalDuties.Auxilium, 0.66f}
                    }, new List<FeudalRights> { FeudalRights.Conquest_Rights, FeudalRights.Absolute_Land_Rights },
                     DefaultGovernments.Instance.Feudal, DefaultSuccessions.Instance.FeudalElective,
                     DefaultInheritances.Instance.Primogeniture,
                     DefaultGenderLaws.Instance.Agnatic),
                "republic" => new FeudalContract(
                    new Dictionary<FeudalDuties, float> { { FeudalDuties.Ransom, 0.10f }, { FeudalDuties.Taxation, 0.4f } },
                    new List<FeudalRights> { FeudalRights.Assistance_Rights, FeudalRights.Army_Compensation_Rights },
                     DefaultGovernments.Instance.Republic, DefaultSuccessions.Instance.Republic,
                     DefaultInheritances.Instance.Primogeniture,
                     DefaultGenderLaws.Instance.Agnatic),
                _ => new FeudalContract(
                    new Dictionary<FeudalDuties, float> { { FeudalDuties.Ransom, 0.20f }, { FeudalDuties.Auxilium, 0.4f } },
                    new List<FeudalRights> { FeudalRights.Absolute_Land_Rights, FeudalRights.Enfoeffement_Rights },
                     DefaultGovernments.Instance.Feudal, DefaultSuccessions.Instance.Hereditary,
                     DefaultInheritances.Instance.Primogeniture,
                     DefaultGenderLaws.Instance.Agnatic)
            };

            return contract;
        }

        public static FeudalContract GenerateContract(string government, string succession, string inheritance, string genderLaw)
        {
            return new FeudalContract(
                    new Dictionary<FeudalDuties, float> { { FeudalDuties.Ransom, 0.20f }, { FeudalDuties.Auxilium, 0.4f } },
                    new List<FeudalRights> { FeudalRights.Absolute_Land_Rights, FeudalRights.Enfoeffement_Rights },
                    DefaultGovernments.Instance.GetById(government), 
                    DefaultSuccessions.Instance.GetById(succession), 
                    DefaultInheritances.Instance.GetById(inheritance),
                    DefaultGenderLaws.Instance.GetById(genderLaw));
        }

        internal static FeudalTitle CreateUnlandedTitle(Hero deJure, TitleType type, List<FeudalTitle> vassals,
            TextObject name, FeudalContract contract, TextObject fullName = null)
        {
            var title = new FeudalTitle(type, null, vassals, deJure, deJure, name, contract, null, fullName);
            BannerKingsConfig.Instance.TitleManager.ExecuteAddTitle(title);
            return title;
        }

        internal static FeudalTitle CreateLandedTitle(Settlement settlement, Hero deJure, TitleType type,
            FeudalContract contract, List<FeudalTitle> vassals = null, TextObject fullName = null)
        {
            var deFacto = settlement.OwnerClan.Leader;
            if (deJure == null)
            {
                deJure = settlement.Owner;
            }

            if (vassals == null)
            {
                vassals = new List<FeudalTitle>();
            }

            if (settlement.BoundVillages != null)
            {
                foreach (var lordship in settlement.BoundVillages)
                {
                    var lordshipTitle = CreateLordship(lordship.Settlement, deJure, contract);
                    vassals.Add(lordshipTitle);
                    BannerKingsConfig.Instance.TitleManager.ExecuteAddTitle(lordshipTitle);
                }
            }

            var title = new FeudalTitle(type, settlement, vassals, deJure, deFacto, settlement.Name, contract, null, fullName);
            BannerKingsConfig.Instance.TitleManager.ExecuteAddTitle(title);
            return title;
        }

        internal static FeudalTitle CreateLordship(Settlement settlement, Hero deJure, FeudalContract contract)
        {
            return new FeudalTitle(TitleType.Lordship, settlement, null,
                deJure, settlement.Village.Bound.Owner, settlement.Name, contract);
        }

        internal static void GenerateDuchy(XmlNode duchy, List<FeudalTitle> vassalsKingdom, FeudalContract contract)
        {
            var vassalsDuchy = new List<FeudalTitle>();
            var dukedomName = new TextObject(duchy.Attributes["name"].Value);
            TextObject dukedomFullName = null;
            XmlAttribute dukedomFullNameAttribute = duchy.Attributes["fullName"];
            if (dukedomFullNameAttribute != null)
            {
                dukedomFullName = new TextObject(dukedomFullNameAttribute.Value);
            }

            var deJureNameDuchy = duchy.Attributes["deJure"].Value;
            var deJureDuchy = GetDeJure(deJureNameDuchy, null);

            if (contract == null)
            {
                string government = duchy.Attributes["government"].Value;
                string succession = duchy.Attributes["succession"].Value;
                string inheritance = duchy.Attributes["inheritance"].Value;
                string genderLaw = duchy.Attributes["genderLaw"].Value;
                contract = GenerateContract(government, succession, inheritance, genderLaw);
            }

            if (duchy.ChildNodes != null)
            {
                foreach (XmlNode county in duchy.ChildNodes)
                {
                    if (county.Name != "county")
                    {
                        return;
                    }

                    var settlementNameCounty = county.Attributes["settlement"].Value;
                    var deJureNameCounty = county.Attributes["deJure"].Value;
                    TextObject countyName = null;
                    XmlAttribute countyFullNameAttribute = county.Attributes["fullName"];
                    if (countyFullNameAttribute != null)
                    {
                        countyName = new TextObject(countyFullNameAttribute.Value);
                    }

                    var settlementCounty = Settlement.All.FirstOrDefault(x => x.Name.ToString() == settlementNameCounty);
                    if (settlementCounty == null)
                    {
                        settlementCounty = Settlement.All.FirstOrDefault(x => x.StringId.ToString() == settlementNameCounty);
                    }

                    var deJureCounty = GetDeJure(deJureNameCounty, settlementCounty);
                    var vassalsCounty = new List<FeudalTitle>();

                    if (county.ChildNodes != null)
                    {
                        foreach (XmlNode barony in county.ChildNodes)
                        {
                            if (barony.Name != "barony")
                            {
                                return;
                            }

                            TextObject baronyName = null;
                            XmlAttribute barnyFullNameAttribute = barony.Attributes["fullName"];
                            if (barnyFullNameAttribute != null)
                            {
                                baronyName = new TextObject(barnyFullNameAttribute.Value);
                            }

                            var settlementNameBarony = barony.Attributes["settlement"].Value;
                            var deJureIdBarony = barony.Attributes["deJure"].Value;
                            var settlementBarony = Settlement.All.FirstOrDefault(x => x.Name.ToString() == settlementNameBarony);
                            if (settlementBarony == null)
                            {
                                settlementBarony = Settlement.All.FirstOrDefault(x => x.StringId.ToString() == settlementNameBarony);
                            }

                            var deJureBarony = GetDeJure(deJureIdBarony, settlementBarony);
                            if (settlementBarony != null)
                            {
                                vassalsCounty.Add(CreateLandedTitle(settlementBarony,
                                    deJureBarony,
                                    TitleType.Barony,
                                    contract,
                                    null,
                                    baronyName));
                            }
                        }
                    }

                    if (settlementCounty != null)
                    {
                        vassalsDuchy.Add(CreateLandedTitle(settlementCounty,
                            deJureCounty,
                            TitleType.County,
                            contract,
                            vassalsCounty,
                            countyName));
                    }
                }
            }

            if (deJureDuchy != null && vassalsDuchy.Count > 0)
            {
                var title = CreateUnlandedTitle(deJureDuchy, TitleType.Dukedom, vassalsDuchy, dukedomName, contract,
                    dukedomFullName);
                if (vassalsKingdom != null) vassalsKingdom.Add(title);
            }
        }

        internal static void GenerateKingdom(XmlNode kingdom)
        {
            var vassalsKingdom = new List<FeudalTitle>();
            var factionName = kingdom.Attributes["faction"].Value;
            var deJureNameKingdom = kingdom.Attributes["deJure"].Value;
            var deJureKingdom = GetDeJure(deJureNameKingdom, null);
            var faction = Kingdom.All.FirstOrDefault(x => x.Name.ToString() == factionName);
            if (faction == null)
            {
                faction = Kingdom.All.FirstOrDefault(x => x.StringId.ToString() == factionName);
            }

            string government = kingdom.Attributes["government"].Value;
            string succession = kingdom.Attributes["succession"].Value;
            string inheritance = kingdom.Attributes["inheritance"].Value;
            string genderLaw = kingdom.Attributes["genderLaw"].Value;
            var contract = GenerateContract(government, succession, inheritance, genderLaw);

            if (contract == null)
            {
                return;
            }

            if (kingdom.ChildNodes != null)
            {
                foreach (XmlNode duchy in kingdom.ChildNodes)
                {
                    if (duchy.Name != "duchy") return;
                    GenerateDuchy(duchy, vassalsKingdom, contract);
                }
            }

            if (deJureKingdom != null && vassalsKingdom.Count > 0 && faction != null)
            {
                var sovereign = CreateKingdom(deJureKingdom, faction, TitleType.Kingdom, vassalsKingdom, contract);
                foreach (var duchy in vassalsKingdom)
                {
                    duchy.SetSovereign(sovereign);
                }
            }
        }

        internal static void InitializeTitles()
        {
            XmlDocument doc = Utils.Helpers.CreateDocumentFromXmlFile(BannerKingsConfig.Instance.TitlesGeneratorPath);
            var titlesNode = doc.ChildNodes[1].ChildNodes[0];
            var autoGenerate = bool.Parse(titlesNode.Attributes["autoGenerate"].Value);

            foreach (XmlNode node in titlesNode)
            {
                if (node.Name == "kingdom") GenerateKingdom(node);
                if (node.Name == "duchy") GenerateDuchy(node, null, null);
            }

            if (autoGenerate)
            {
                foreach (var settlement in Settlement.All)
                {
                    if (settlement.IsVillage)
                    {
                        continue;
                    }

                    if (settlement.OwnerClan is { Leader: { } } &&
                        (settlement.IsTown || settlement.IsCastle))
                    {
                        CreateLandedTitle(settlement,
                            settlement.Owner, settlement.IsTown ? TitleType.County : TitleType.Barony,
                            GenerateContract("feudal"));
                    }
                }
            }
        }
    }
}
