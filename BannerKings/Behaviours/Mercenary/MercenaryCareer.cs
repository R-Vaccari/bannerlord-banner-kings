using BannerKings.Managers.Populations.Estates;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Behaviours.Mercenary
{
    public class MercenaryCareer
    {
        internal MercenaryCareer(Clan clan, Kingdom kingdom)
        {
            Clan = clan;
            Kingdom = kingdom;
            Reputation = 0f;
            KingdomPrivileges = new Dictionary<Kingdom, List<MercenaryPrivilege>>();
            KingdomProgress = new Dictionary<Kingdom, float>();
            LevyTroops = new Dictionary<CultureObject, CustomTroop>();
            ProfessionalTroops = new Dictionary<CultureObject, CustomTroop>();
            PrivilegeTimes = new Dictionary<Kingdom, CampaignTime>();
            AddKingdom(kingdom);
        }

        [SaveableProperty(1)] public Clan Clan { get; private set;  }
        [SaveableProperty(2)] public Kingdom Kingdom { get; private set;  }
        [SaveableProperty(3)] public float Reputation { get; private set; }

        [SaveableProperty(4)] private Dictionary<Kingdom, List<MercenaryPrivilege>> KingdomPrivileges { get; set; }
        [SaveableProperty(5)] private Dictionary<Kingdom, float> KingdomProgress { get; set; }
        [SaveableProperty(6)] private Dictionary<CultureObject, CustomTroop> LevyTroops { get; set; }
        [SaveableProperty(7)] private Dictionary<CultureObject, CustomTroop> ProfessionalTroops { get; set; }
        [SaveableProperty(8)] private Dictionary<Kingdom, CampaignTime> PrivilegeTimes { get; set; }

        [SaveableProperty(9)] public int ServiceDays { get; private set; }

        internal void PostInitialize()
        {
            foreach (var pair in LevyTroops)
            {
                pair.Value.PostInitialize(pair.Key);
            }

            foreach (var pair in ProfessionalTroops)
            {
                pair.Value.PostInitialize(pair.Key);
            }

            foreach (var list in KingdomPrivileges.Values)
            {
                foreach (var privilege in list)
                {
                    var copy = DefaultMercenaryPrivileges.Instance.GetById(privilege.StringId);
                    privilege.Initialize(copy.Name, copy.Description, copy.UnAvailableHint, copy.Points,
                        copy.MaxLevel, copy.IsAvailable, copy.OnPrivilegeAdded);
                }
            }
        }

        internal void Tick(float progress)
        {
            KingdomProgress[Kingdom] += progress;
            ServiceDays++;
            if (((float)ServiceDays / (float)CampaignTime.DaysInYear) % 1f == 0f)
            {
                AddReputation(0.05f, new TextObject("{=!}A year of service has passed."));
            }
        }

        internal bool HasPrivilegeCurrentKingdom(MercenaryPrivilege privilege) => KingdomPrivileges[Kingdom].Any(x => x.Equals(privilege));

        internal void AddReputation(float reputation, TextObject reason)
        {
            Reputation += reputation;
            Reputation = MathF.Clamp(Reputation, 0f, 1f);

            if (Clan == Clan.PlayerClan)
            {
                MBInformationManager.AddQuickInformation(new TextObject("{=!}You have gained {REPUTATION}% mercenary reputation! {REASON}")
                    .SetTextVariable("REPUTATION", reputation * 100f)
                    .SetTextVariable("REASON", reason));
            }
        }

        internal bool HasTimePassedForPrivilege(Kingdom kingdom) => PrivilegeTimes[kingdom].ElapsedSeasonsUntilNow >= 2f;

        internal CampaignTime GetPrivilegeTime(Kingdom kingdom) => PrivilegeTimes[kingdom];

        internal int GetPrivilegeLevelCurrentKingdom(MercenaryPrivilege privilege) 
        {
            int result = 0;
            var current = KingdomPrivileges[Kingdom].FirstOrDefault(x => x.Equals(privilege));
            if (current != null)
            {
                result = current.Level;
            }

            return result;
        }

        internal bool IsTroopCustom(CharacterObject character)
        {
            var culture = character.Culture;
            bool matches = false;
            if (LevyTroops.ContainsKey(culture))
            {
                matches = LevyTroops[culture].Character == character;
            }

            if (!matches && ProfessionalTroops.ContainsKey(culture))
            {
                matches = ProfessionalTroops[culture].Character == character;
            }

            return matches;
        }

        internal bool CanLevelUpPrivilege(MercenaryPrivilege privilege)
        {
            if (KingdomPrivileges[Kingdom].Contains(privilege))
            {
                var currentPrivilege = KingdomPrivileges[Kingdom].First(x => x.Equals(privilege));
                return currentPrivilege.Level < currentPrivilege.MaxLevel &&
                    KingdomProgress[Kingdom] > currentPrivilege.Points &&
                    currentPrivilege.IsAvailable(this) && HasTimePassedForPrivilege(Kingdom);
            }
            return KingdomProgress[Kingdom] > privilege.Points;
        }

        internal void AddKingdom(Kingdom kingdom)
        {
            if (!KingdomPrivileges.ContainsKey(kingdom))
            {
                KingdomPrivileges.Add(kingdom, new List<MercenaryPrivilege>());
            }

            if (!KingdomProgress.ContainsKey(kingdom))
            {
                KingdomProgress.Add(kingdom, 0f);
            }

            if (!PrivilegeTimes.ContainsKey(kingdom))
            {
                PrivilegeTimes.Add(kingdom, CampaignTime.Now);
            }
        }

        internal float GetPoints(Kingdom kingdom)
        {
            if (KingdomProgress.ContainsKey(kingdom))
            {
                return KingdomProgress[kingdom];
            }

            return 0f;
        }

        internal List<MercenaryPrivilege> GetPrivileges(Kingdom kingdom)
        {
            var list = new List<MercenaryPrivilege>();
            foreach (var kingdomPrivilege in KingdomPrivileges[kingdom])
            {
                list.Add(kingdomPrivilege);
            }

            return list;
        }

        internal void AddPrivilege(MercenaryPrivilege privilege)
        {
            if (KingdomPrivileges[Kingdom].Contains(privilege))
            {
                KingdomPrivileges[Kingdom].First(x => x.Equals(privilege)).IncreaseLevel();
            }
            else
            {
                var copy = new MercenaryPrivilege(privilege.StringId);
                copy.Initialize(privilege.Name, privilege.Description,
                    privilege.UnAvailableHint, privilege.Points,
                    privilege.MaxLevel,
                    privilege.IsAvailable,
                    privilege.OnPrivilegeAdded);
                copy.IncreaseLevel();
                KingdomPrivileges[Kingdom].Add(copy);
            }

            privilege.OnPrivilegeAdded(this);
            PrivilegeTimes[Kingdom] = CampaignTime.Now;

            if (Clan == Clan.PlayerClan)
            {
                MBInformationManager.AddQuickInformation(new TextObject("{=!}{CLAN} has acquired the {PRIVILEGE} privilege!")
                    .SetTextVariable("CLAN", Clan.PlayerClan.Name)
                    .SetTextVariable("PRIVILEGE", privilege.Name),
                    0,
                    null,
                    Utils.Helpers.GetKingdomDecisionSound());
            }
        }
        internal CustomTroop GetTroop(Kingdom kingdom, bool isLevy = true)
        {
            var culture = kingdom.Culture;
            if (isLevy)
            {
                if (LevyTroops.ContainsKey(culture))
                {
                    return LevyTroops[culture];
                }
            }
            else
            {
                if (ProfessionalTroops.ContainsKey(culture))
                {
                    return ProfessionalTroops[culture];
                }
            }

            return null;
        }

        internal void AddTroop(Kingdom kingdom, CharacterObject troop, bool isLevy = true)
        {
            var culture = kingdom.Culture;
            if (isLevy)
            {
                if (LevyTroops.ContainsKey(culture))
                {
                    LevyTroops[culture].Character = troop;
                }
                else
                {
                    LevyTroops.Add(culture, new CustomTroop(troop));
                }
            }
            else
            {
                if (ProfessionalTroops.ContainsKey(culture))
                {
                    ProfessionalTroops[culture].Character = troop;
                }
                else
                {
                    ProfessionalTroops.Add(culture, new CustomTroop(troop));
                }
            }
        }

        internal static Workshop GetWorkshopPrivilege(MercenaryCareer career)
        {
            Workshop workshop = null;
            var clan = career.Kingdom.RulingClan;
            foreach (var town in clan.Fiefs)
            {
                foreach (var wk in town.Workshops)
                {
                    if (wk.Owner.IsNotable || wk.Owner.Clan == clan)
                    {
                        float workshopCost = BannerKingsConfig.Instance.WorkshopModel.GetBuyingCostForPlayer(wk);
                        if (clan.Gold >= workshopCost && workshopCost < 300000f)
                        {
                            workshop = wk;
                            break;
                        }
                    }
                }
            }

            return workshop;
        }

        internal static Estate GetEstatePrivilege(MercenaryCareer career)
        {
            Estate estate = null;
            var clan = career.Kingdom.RulingClan;
            foreach (var village in clan.Villages)
            {
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(village.Settlement);
                if (data != null && data.EstateData != null)
                {
                    foreach (var et in data.EstateData.Estates)
                    {
                        var action = BannerKingsConfig.Instance.EstatesModel.GetGrant(et, clan.Leader, career.Clan.Leader);
                        if (action.Possible)
                        {
                            estate = et;
                        }
                    }
                }
            }

            return estate;
        }

        internal static Settlement GetBaronyPrivilege(MercenaryCareer career)
        {
            Settlement castle = null;
            var clan = career.Kingdom.RulingClan;
            if (clan.Fiefs.Count > 1)
            {
                var capital = Campaign.Current.GetCampaignBehavior<BKCapitalBehavior>().GetCapital(clan.Kingdom);
                foreach (var fief in clan.Fiefs)
                {
                    var title = BannerKingsConfig.Instance.TitleManager.GetTitle(fief.Settlement);
                    if (fief.IsCastle && title != null && title.deJure == clan.Leader && fief != capital)
                    {
                        var action = BannerKingsConfig.Instance.TitleModel.GetAction(Managers.Titles.ActionType.Grant, title,
                            title.deJure, career.Clan.Leader);
                        if (action.Possible)
                        {
                            castle = fief.Settlement;
                            break;
                        }
                    } 
                }
            }

            return castle;
        }
    }
}
