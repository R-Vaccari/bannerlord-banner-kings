using HarmonyLib;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem.Party;
using BannerKings.Settings;
using BannerKings.Utils;
using TaleWorlds.CampaignSystem.CampaignBehaviors;

namespace BannerKings.Patches
{
    internal partial class PerksAndSkillsPatches
    {
        [HarmonyPatch(typeof(PerkActivationHandlerCampaignBehavior), "OnPerkOpened")]
        class OnPerkOpenedPatch
        {
            static bool Prefix(Hero hero, PerkObject perk)
            {

                if (hero != null)
                {
                    if (perk == DefaultPerks.OneHanded.Trainer || perk == DefaultPerks.OneHanded.UnwaveringDefense || perk == DefaultPerks.TwoHanded.ThickHides || perk == DefaultPerks.Athletics.WellBuilt)
                    {
                        hero.HitPoints += (int)perk.PrimaryBonus;

                    }
                    if (perk == DefaultPerks.Medicine.PreventiveMedicine)
                    {
                        if (BannerKingsSettings.Instance.EnableUsefulPerks && BannerKingsSettings.Instance.EnableUsefulMedicinePerks)
                        {
                            ExplainedNumber bonus = new ExplainedNumber(0);
                            hero.HitPoints += (int)DefaultPerks.Medicine.PreventiveMedicine.AddScaledPersonalPerkBonus(ref bonus, false, hero);
                        }
                        else
                        {
                            hero.HitPoints += (int)perk.PrimaryBonus;
                        }
                    }
                   
                    else if (perk == DefaultPerks.Crafting.VigorousSmith)
                    {
                        hero.HeroDeveloper.AddAttribute(DefaultCharacterAttributes.Vigor, 1, false);
                    }
                    else if (perk == DefaultPerks.Crafting.StrongSmith)
                    {
                        hero.HeroDeveloper.AddAttribute(DefaultCharacterAttributes.Control, 1, false);
                    }
                    else if (perk == DefaultPerks.Crafting.EnduringSmith)
                    {
                        hero.HeroDeveloper.AddAttribute(DefaultCharacterAttributes.Endurance, 1, false);
                    }
                    else if (perk == DefaultPerks.Crafting.WeaponMasterSmith)
                    {
                        hero.HeroDeveloper.AddFocus(DefaultSkills.OneHanded, 1, false);
                        hero.HeroDeveloper.AddFocus(DefaultSkills.TwoHanded, 1, false);
                    }
                    else if (perk == DefaultPerks.Athletics.Durable)
                    {
                        hero.HeroDeveloper.AddAttribute(DefaultCharacterAttributes.Endurance, 1, false);
                    }
                    else if (perk == DefaultPerks.Athletics.Steady)
                    {
                        hero.HeroDeveloper.AddAttribute(DefaultCharacterAttributes.Control, 1, false);
                    }
                    else if (perk == DefaultPerks.Athletics.Strong)
                    {
                        hero.HeroDeveloper.AddAttribute(DefaultCharacterAttributes.Vigor, 1, false);
                    }
                    if (hero == Hero.MainHero && (perk == DefaultPerks.OneHanded.Prestige || perk == DefaultPerks.TwoHanded.Hope || perk == DefaultPerks.Athletics.ImposingStature || perk == DefaultPerks.Bow.MerryMen || perk == DefaultPerks.Tactics.HordeLeader || perk == DefaultPerks.Scouting.MountedScouts || perk == DefaultPerks.Leadership.Authority || perk == DefaultPerks.Leadership.LeaderOfMasses || perk == DefaultPerks.Leadership.UltimateLeader))
                    {
                        PartyBase.MainParty.MemberRoster.UpdateVersion();
                    }
                }

                return false;
            }

        }
    }
}
