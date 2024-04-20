using HarmonyLib;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using BannerKings.Utils;
using BannerKings.Settings;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using static BannerKings.Utils.PerksHelpers;

namespace BannerKings.Patches.Perks
{
    internal partial class PerksAndSkillsPatches
    {
        [HarmonyPatch(typeof(DefaultPartyMoraleModel), "GetMoraleEffectsFromPerks")]
        class GetMoraleEffectsFromPerksPatch
        {
            static void Postfix(MobileParty party, ref ExplainedNumber bonus)
            {
                if (party.HasPerk(DefaultPerks.Crossbow.PeasantLeader, false))
                {
                    float num = CalculateTroopTierRatio(party);
                    bonus.AddFactor(DefaultPerks.Crossbow.PeasantLeader.PrimaryBonus * num, DefaultPerks.Crossbow.PeasantLeader.Name);
                }
                Settlement currentSettlement = party.CurrentSettlement;
                if ((currentSettlement != null ? currentSettlement.SiegeEvent : null) != null && party.HasPerk(DefaultPerks.Charm.SelfPromoter, true))
                {
                    bonus.Add(DefaultPerks.Charm.SelfPromoter.SecondaryBonus, DefaultPerks.Charm.SelfPromoter.Name, null);
                }

                int num2 = 0;
                for (int i = 0; i < party.MemberRoster.Count; i++)
                {
                    TroopRosterElement elementCopyAtIndex = party.MemberRoster.GetElementCopyAtIndex(i);
                    if (elementCopyAtIndex.Character.IsMounted)
                    {
                        num2 += elementCopyAtIndex.Number;
                    }
                }
                if (party.Party.NumberOfMounts > party.MemberRoster.TotalManCount - num2)
                {
                    #region DefaultPerks.Steward.Logistician
                    if (BannerKingsSettings.Instance.EnableUsefulPerks && BannerKingsSettings.Instance.EnableUsefulStewardPerks)
                    {
                        DefaultPerks.Steward.Logistician.AddScaledPerkBonus(ref bonus, false, party);
                    }
                    else
                    {
                        if (party.HasPerk(DefaultPerks.Steward.Logistician, false))
                        {
                            bonus.Add(DefaultPerks.Steward.Logistician.PrimaryBonus, DefaultPerks.Steward.Logistician.Name, null);
                        }
                    }
                    #endregion                  
                }
            }

            private static float CalculateTroopTierRatio(MobileParty party)
            {
                int totalManCount = party.MemberRoster.TotalManCount;
                float num = 0f;
                foreach (TroopRosterElement troopRosterElement in party.MemberRoster.GetTroopRoster())
                {
                    if (troopRosterElement.Character.Tier <= 3)
                    {
                        num += troopRosterElement.Number;
                    }
                }
                return num / totalManCount;
            }
        }
    }
}
