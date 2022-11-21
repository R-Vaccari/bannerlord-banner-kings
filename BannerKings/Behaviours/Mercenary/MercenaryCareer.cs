using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
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
            LevyTroops = new Dictionary<CultureObject, CharacterObject>();
            ProfessionalTroops = new Dictionary<CultureObject, CharacterObject>();
            PrivilegeTimes = new Dictionary<Kingdom, CampaignTime>();
            AddKingdom(kingdom);
        }


        [SaveableProperty(1)] public Clan Clan { get; private set;  }
        [SaveableProperty(2)] public Kingdom Kingdom { get; private set;  }
        [SaveableProperty(3)] public float Reputation { get; private set; }


        [SaveableProperty(4)] private Dictionary<Kingdom, List<MercenaryPrivilege>> KingdomPrivileges { get; set; }
        [SaveableProperty(5)] private Dictionary<Kingdom, float> KingdomProgress { get; set; }
        [SaveableProperty(6)] private Dictionary<CultureObject, CharacterObject> LevyTroops { get; set; }
        [SaveableProperty(7)] private Dictionary<CultureObject, CharacterObject> ProfessionalTroops { get; set; }
        [SaveableProperty(8)] private Dictionary<Kingdom, CampaignTime> PrivilegeTimes { get; set; }

        internal void Tick()
        {
            KingdomProgress[Kingdom] += 1000f;
        }

        internal bool HasTimePassedForPrivilege(Kingdom kingdom) => PrivilegeTimes[kingdom].ElapsedSeasonsUntilNow >= 2f;

        internal CampaignTime GetPrivilegeTime(Kingdom kingdom) => PrivilegeTimes[kingdom];

        internal bool IsTroopCustom(CharacterObject character)
        {
            var culture = character.Culture;
            bool matches = false;
            if (LevyTroops.ContainsKey(culture))
            {
                matches = LevyTroops[culture] == character;
            }

            if (!matches && ProfessionalTroops.ContainsKey(culture))
            {
                matches = ProfessionalTroops[culture] == character;
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
                PrivilegeTimes.Add(kingdom, CampaignTime.Zero);
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
                    privilege.IsAvailable);
                copy.IncreaseLevel();
                KingdomPrivileges[Kingdom].Add(copy);
            }

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

        internal CharacterObject GetTroop(Kingdom kingdom, bool isLevy = true)
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
                    LevyTroops[culture] = troop;
                }
                else
                {
                    LevyTroops.Add(culture, troop);
                }
            }
            else
            {
                if (ProfessionalTroops.ContainsKey(culture))
                {
                    ProfessionalTroops[culture] = troop;
                }
                else
                {
                    ProfessionalTroops.Add(culture, troop);
                }
            }
        }
    }
}
