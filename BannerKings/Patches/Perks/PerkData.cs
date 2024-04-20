using TaleWorlds.CampaignSystem.CharacterDevelopment;
using BannerKings.Utils;
using BannerKings.Settings;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Patches.Perks
{
    class PerkData
    {
        public PerkSubData PrimaryPerk { get; set; }
        public PerkSubData SecondaryPerk { get; set; }
       
        private static void ChangePerkRole(PerkObject perk, SkillEffect.PerkRole newRole, bool isSecondary = false)
        {
            if (perk != null)
            {
                if (isSecondary)
                {
                    perk.SetPrivatePropertyValue("SecondaryRole", newRole);
                }
                else
                {
                    perk.SetPrivatePropertyValue("PrimaryRole", newRole);
                }
            }
        }
        public void ChangePerk(PerkObject perk)
        {
            if (perk != null)
            {
                if (SecondaryPerk != null)
                {
                    perk.SetPrivatePropertyValue("SecondaryBonus", SecondaryPerk.BonusEverySkill);
                    if (BannerKingsSettings.Instance.EnableUsefulPerksFromAllPartyMembers)
                    {
                        perk.SetPrivatePropertyValue("SecondaryDescription", SecondaryPerk.GetFormattedDescription1(perk.SecondaryIncrementType));
                    }
                    else
                    {
                        perk.SetPrivatePropertyValue("SecondaryDescription", SecondaryPerk.GetFormattedDescription2(perk.SecondaryIncrementType));
                    }                    
                    if (SecondaryPerk.Role.HasValue)
                    {
                        ChangePerkRole(perk, SecondaryPerk.Role.Value, true);
                    }
                }
                if (PrimaryPerk != null)
                {
                    perk.SetPrivatePropertyValue("PrimaryBonus", PrimaryPerk.BonusEverySkill);
                    if (BannerKingsSettings.Instance.EnableUsefulPerksFromAllPartyMembers)
                    {
                        perk.SetPrivatePropertyValue("PrimaryDescription", PrimaryPerk.GetFormattedDescription1(perk.PrimaryIncrementType));
                    }
                    else
                    {
                        perk.SetPrivatePropertyValue("PrimaryDescription", PrimaryPerk.GetFormattedDescription2(perk.PrimaryIncrementType));
                    }
                    if (PrimaryPerk.Role.HasValue)
                    {
                        ChangePerkRole(perk, PrimaryPerk.Role.Value, false);
                    }
                }

            }
        }
    }
}
