using BannerKings.Managers.Populations.Estates;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
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
        public MercenaryCareer(Clan clan, Kingdom kingdom)
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

        [SaveableProperty(1)] public Clan Clan { get; private set; }
        [SaveableProperty(2)] public Kingdom Kingdom { get; private set; }
        [SaveableProperty(3)] public float Reputation { get; private set; }

        [SaveableProperty(4)] private Dictionary<Kingdom, List<MercenaryPrivilege>> KingdomPrivileges { get; set; }
        [SaveableProperty(5)] private Dictionary<Kingdom, float> KingdomProgress { get; set; }
        [SaveableProperty(6)] private Dictionary<CultureObject, CustomTroop> LevyTroops { get; set; }
        [SaveableProperty(7)] private Dictionary<CultureObject, CustomTroop> ProfessionalTroops { get; set; }
        [SaveableProperty(8)] private Dictionary<Kingdom, CampaignTime> PrivilegeTimes { get; set; }

        [SaveableProperty(9)] public int ServiceDays { get; private set; }
        [SaveableProperty(10)] public CampaignTime ContractDueDate { get; private set; }

        public void AddPoints()
        {
            KingdomProgress[Kingdom] += 1000f;
            PrivilegeTimes[Kingdom] = CampaignTime.YearsFromNow(-1f);
        }

        public void PostInitialize()
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

            if (Clan.IsUnderMercenaryService && Kingdom != null) ContractDueDate = CampaignTime.YearsFromNow(1f);
            else ContractDueDate = CampaignTime.Never;
        }

        public void Tick(float progress)
        {
            if (!Clan.IsUnderMercenaryService || Kingdom == null) return;

            KingdomProgress[Kingdom] += progress;
            ServiceDays++;
            if (((float)ServiceDays / (float)CampaignTime.DaysInYear) % 1f == 0f)
            {
                AddReputation(0.05f, new TextObject("{=YoEwwXZN}A year of service has passed."));
            }

            foreach (MercenaryPrivilege privilege in DefaultMercenaryPrivileges.Instance.All)
            {
                var list = KingdomPrivileges[Kingdom].FindAll(x => x.Equals(privilege));
                if (list.Count > 1)
                {
                    list.RemoveAt(0);
                }
            }

            if (ContractDueDate.IsNow || ContractDueDate.IsPast)
            {
                if (Clan == Clan.PlayerClan)
                {
                    if (BannerKingsConfig.Instance.DiplomacyModel.GetScoreOfKingdomToHireMercenary(Kingdom, Clan) >
                        BannerKingsConfig.Instance.DiplomacyModel.GetScoreOfKingdomToSackMercenary(Kingdom, Clan))
                    {
                        InformationManager.ShowInquiry(new InquiryData(new TextObject("{=!}Mercenary Contract").ToString(),
                            new TextObject("{=!}The due date for your service to the {KINGDOM} has come. They are willing to extend your service as their hireling for another year, until {DATE}. Accepting this proposal will make you contract-bound to serve another year, and make it unlikely for them to preemptively release you from your service. By rejecting this proposal you may leave or be released at any time without any consequences.")
                            .SetTextVariable("KINGDOM", Kingdom.Name)
                            .SetTextVariable("DATE", CampaignTime.YearsFromNow(1f).ToString())
                            .ToString(),
                            true,
                            true,
                            GameTexts.FindText("str_accept").ToString(),
                            GameTexts.FindText("str_reject").ToString(),
                            () => ExtendTime(),
                            null,
                            Utils.Helpers.GetKingdomDecisionSound()),
                            true,
                            true);
                    }
                }
                else if (Kingdom.RulingClan == Clan.PlayerClan)
                {
                    InformationManager.ShowInquiry(new InquiryData(new TextObject("{=!}Mercenary Contract").ToString(),
                            new TextObject("{=!}The due date for your service to the {CLAN} has come. It is your choice to extend the binding of the contract for another year, until {DATE}. Doing so will make it unlikely for them to preemptively leave their service. They will also expect to not be release from this duty - releasing them before the due date will negatively impact their predisposition towards you.")
                            .SetTextVariable("CLAN", Clan.Name)
                            .SetTextVariable("DATE", CampaignTime.YearsFromNow(1f).ToString())
                            .ToString(),
                            true,
                            true,
                            GameTexts.FindText("str_accept").ToString(),
                            GameTexts.FindText("str_reject").ToString(),
                            () => ExtendTime(),
                            null,
                            Utils.Helpers.GetKingdomDecisionSound()),
                            true,
                            true);
                }
            }
        }

        public bool HasPrivilegeCurrentKingdom(MercenaryPrivilege privilege) => KingdomPrivileges[Kingdom].Any(x => x.Equals(privilege));

        public void AddReputation(float reputation, TextObject reason)
        {
            Reputation += reputation;
            Reputation = MathF.Clamp(Reputation, 0f, 1f);

            if (Clan == Clan.PlayerClan)
            {
                if (reputation > 0f)
                    MBInformationManager.AddQuickInformation(new TextObject("{=H7GxnhBB}You have gained {REPUTATION}% mercenary reputation! {REASON}")
                        .SetTextVariable("REPUTATION", reputation * 100f)
                        .SetTextVariable("REASON", reason));
                else
                    MBInformationManager.AddQuickInformation(new TextObject("{=!}You have lost {REPUTATION}% mercenary reputation! {REASON}")
                        .SetTextVariable("REPUTATION", MathF.Abs(reputation * 100f))
                        .SetTextVariable("REASON", reason));
            }
        }

        public bool HasTimePassedForPrivilege(Kingdom kingdom) => PrivilegeTimes[kingdom].ElapsedSeasonsUntilNow >= 2f;

        public CampaignTime GetPrivilegeTime(Kingdom kingdom) => PrivilegeTimes[kingdom];

        public int GetPrivilegeLevelCurrentKingdom(MercenaryPrivilege privilege)
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
            return KingdomProgress[Kingdom] > privilege.Points && HasTimePassedForPrivilege(Kingdom);
        }

        public void AddKingdom(Kingdom kingdom)
        {
            Kingdom = kingdom;
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

        public void RemoveKingdom(Kingdom kingdom, bool fired = false)
        {
            if (Kingdom != kingdom) return;

            float daysLeft = ContractDueDate.RemainingDaysFromNow;
            if (daysLeft > 0)
            {
                Hero ruler = kingdom.RulingClan.Leader;
                float relation = -MBMath.Map(daysLeft, 1, CampaignTime.DaysInYear, 15, 50);
                if (!fired)
                {
                    ChangeRelationAction.ApplyRelationChangeBetweenHeroes(Clan.Leader, ruler, (int)relation);
                    AddReputation(relation * 0.005f, new TextObject("{=!}Left service with {DAYS} days remaining.")
                        .SetTextVariable("DAYS", (int)daysLeft));

                    foreach (Hero member in Clan.Lords)
                    {
                        if (member == Clan.Leader) continue;
                        ChangeRelationAction.ApplyRelationChangeBetweenHeroes(member, ruler, (int)(relation / 2f));
                    }
                }
                else
                {
                    if (Clan != Clan.PlayerClan)
                    {
                        ChangeRelationAction.ApplyRelationChangeBetweenHeroes(ruler, Clan.Leader, (int)relation);
                        foreach (Hero member in Clan.Lords)
                        {
                            if (member == Clan.Leader) continue;
                            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(ruler, member, (int)(relation / 2f));
                        }
                    }
                    else
                    {
                        InformationManager.DisplayMessage(new InformationMessage(
                            new TextObject("{=!}The lords of {KINGDOM} have decided to release you from your service with {DAYS} left in the contract.")
                            .SetTextVariable("KINGDOM", kingdom.Name)
                            .SetTextVariable("DAYS", (int)daysLeft)
                            .ToString(),
                            Color.FromUint(Utils.TextHelper.COLOR_LIGHT_RED)));
                    }
                }
            }

            Kingdom = null;
        }

        public void ExtendTime() => ContractDueDate = CampaignTime.YearsFromNow(1f);

        public float GetPoints(Kingdom kingdom)
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
            MercenaryPrivilege newPrivilege;
            if (KingdomPrivileges[Kingdom].Contains(privilege))
            {
                newPrivilege = KingdomPrivileges[Kingdom].First(x => x.Equals(privilege));
            }
            else
            {
                newPrivilege = new MercenaryPrivilege(privilege.StringId);
                newPrivilege.Initialize(privilege.Name, privilege.Description,
                    privilege.UnAvailableHint, privilege.Points,
                    privilege.MaxLevel,
                    privilege.IsAvailable,
                    privilege.OnPrivilegeAdded);
            }

            bool granted = privilege.OnPrivilegeAdded(this);
            if (granted)
            {
                KingdomProgress[Kingdom] -= privilege.Points;
                if (KingdomPrivileges[Kingdom].Contains(privilege))
                {
                    KingdomPrivileges[Kingdom].First(x => x.Equals(privilege)).IncreaseLevel();
                }
                else
                {
                    KingdomPrivileges[Kingdom].Add(newPrivilege);
                    newPrivilege.IncreaseLevel();
                }

                PrivilegeTimes[Kingdom] = CampaignTime.Now;
                if (Clan == Clan.PlayerClan)
                {
                    MBInformationManager.AddQuickInformation(new TextObject("{=pqydS2kr}{CLAN} has acquired the {PRIVILEGE} privilege!")
                        .SetTextVariable("CLAN", Clan.PlayerClan.Name)
                        .SetTextVariable("PRIVILEGE", privilege.Name),
                        0,
                        null,
                        Utils.Helpers.GetKingdomDecisionSound());
                }
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

        internal CustomTroop GetTroop(CultureObject culture, bool isLevy = true)
        {
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
                        float workshopCost = BannerKingsConfig.Instance.WorkshopModel.GetCostForPlayer(wk);
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
                var capital = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKCapitalBehavior>().GetCapital(clan.Kingdom);
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
