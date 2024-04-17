using BannerKings.Settings;
using BannerKings.Utils;
using Helpers;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace BannerKings.Models.Vanilla
{
    internal class BKPartyTrainningModel : DefaultPartyTrainingModel
    {
        private int GetPerkExperiencesForTroops(PerkObject perk)
        {
            if (perk == DefaultPerks.Leadership.CombatTips || perk == DefaultPerks.Leadership.RaiseTheMeek || perk == DefaultPerks.OneHanded.MilitaryTradition || perk == DefaultPerks.Crossbow.RenownMarksmen || perk == DefaultPerks.Steward.SevenVeterans || perk == DefaultPerks.Steward.DrillSergant)
            {
                return MathF.Round(perk.PrimaryBonus);
            }
            if (perk == DefaultPerks.Polearm.Drills || perk == DefaultPerks.Athletics.WalkItOff || perk == DefaultPerks.Athletics.AGoodDaysRest || perk == DefaultPerks.Bow.Trainer || perk == DefaultPerks.Bow.BullsEye || perk == DefaultPerks.Throwing.Saddlebags)
            {
                return MathF.Round(perk.SecondaryBonus);
            }
            return 0;
        }


        public override ExplainedNumber GetEffectiveDailyExperience(MobileParty mobileParty, TroopRosterElement troop)
        {
            ExplainedNumber result = default;
            ExceptionUtils.TryCatch(() =>
            {
                if (troop.Character.Culture == null) return;

                if (mobileParty.IsLordParty && !troop.Character.IsHero && (mobileParty.Army == null || mobileParty.Army.LeaderParty != MobileParty.MainParty) && mobileParty.MapEvent == null && (mobileParty.Party.Owner == null || mobileParty.Party.Owner.Clan != Clan.PlayerClan))
                {
                    if (mobileParty.LeaderHero != null && mobileParty.LeaderHero == mobileParty.ActualClan.Leader)
                    {
                        result.Add(15f + (float)troop.Character.Tier * 3f, null, null);
                    }
                    else
                    {
                        result.Add(10f + (float)troop.Character.Tier * 2f, null, null);
                    }
                }
                if (mobileParty.IsActive && mobileParty.HasPerk(DefaultPerks.Leadership.CombatTips, false))
                {
                    result.Add((float)this.GetPerkExperiencesForTroops(DefaultPerks.Leadership.CombatTips), null, null);
                }
                if (mobileParty.IsActive && mobileParty.HasPerk(DefaultPerks.Leadership.RaiseTheMeek, false) && troop.Character.Tier < 3)
                {
                    result.Add((float)this.GetPerkExperiencesForTroops(DefaultPerks.Leadership.RaiseTheMeek), null, null);
                }
                if (mobileParty.IsGarrison)
                {
                    Settlement currentSettlement = mobileParty.CurrentSettlement;
                    if (((currentSettlement != null) ? currentSettlement.Town.Governor : null) != null && mobileParty.CurrentSettlement.Town.Governor.GetPerkValue(DefaultPerks.Bow.BullsEye))
                    {
                        result.Add((float)this.GetPerkExperiencesForTroops(DefaultPerks.Bow.BullsEye), null, null);
                    }
                }
                if (mobileParty.IsActive && mobileParty.HasPerk(DefaultPerks.Polearm.Drills, true))
                {
                    result.Add((float)this.GetPerkExperiencesForTroops(DefaultPerks.Polearm.Drills), null, null);
                }
                if (mobileParty.IsActive && mobileParty.HasPerk(DefaultPerks.OneHanded.MilitaryTradition, false) && troop.Character.IsInfantry)
                {
                    result.Add((float)this.GetPerkExperiencesForTroops(DefaultPerks.OneHanded.MilitaryTradition), null, null);
                }
                if (mobileParty.IsActive && mobileParty.HasPerk(DefaultPerks.Athletics.WalkItOff, true) && !troop.Character.IsMounted && mobileParty.IsMoving)
                {
                    result.Add((float)this.GetPerkExperiencesForTroops(DefaultPerks.Athletics.WalkItOff), null, null);
                }
                if (mobileParty.IsActive && mobileParty.HasPerk(DefaultPerks.Throwing.Saddlebags, true) && troop.Character.IsInfantry)
                {
                    result.Add((float)this.GetPerkExperiencesForTroops(DefaultPerks.Throwing.Saddlebags), null, null);
                }
                if (mobileParty.IsActive && mobileParty.HasPerk(DefaultPerks.Athletics.AGoodDaysRest, true) && !troop.Character.IsMounted && !mobileParty.IsMoving && mobileParty.CurrentSettlement != null)
                {
                    result.Add((float)this.GetPerkExperiencesForTroops(DefaultPerks.Athletics.AGoodDaysRest), null, null);
                }
                if (mobileParty.IsActive && mobileParty.HasPerk(DefaultPerks.Bow.Trainer, true) && troop.Character.IsRanged)
                {
                    result.Add((float)this.GetPerkExperiencesForTroops(DefaultPerks.Bow.Trainer), null, null);
                }
                if (mobileParty.IsActive && mobileParty.HasPerk(DefaultPerks.Crossbow.RenownMarksmen, false) && troop.Character.IsRanged)
                {
                    result.Add((float)this.GetPerkExperiencesForTroops(DefaultPerks.Crossbow.RenownMarksmen), null, null);
                }
                if (mobileParty.IsActive && mobileParty.IsMoving)
                {
                    if (mobileParty.Morale > 75f)
                    {
                        PerkHelper.AddPerkBonusForParty(DefaultPerks.Scouting.ForcedMarch, mobileParty, false, ref result);
                    }
                    if (mobileParty.ItemRoster.TotalWeight > (float)mobileParty.InventoryCapacity)
                    {
                        PerkHelper.AddPerkBonusForParty(DefaultPerks.Scouting.Unburdened, mobileParty, false, ref result);
                    }
                }
                #region DefaultPerks.Steward.SevenVeterans
                if (BannerKingsSettings.Instance.EnableUsefulPerks && BannerKingsSettings.Instance.EnableUsefulStewardPerks)
                {
                    if (mobileParty.IsActive && troop.Character.Tier >= 4)
                    {
                        DefaultPerks.Steward.SevenVeterans.AddScaledPerkBonus(ref result, false, mobileParty, DefaultSkills.Steward, 25, 25, 100, Utils.Helpers.SkillScale.Both, minValue: 0, maxValue: 60f);
                    }
                }
                else
                {
                    if (mobileParty.IsActive && mobileParty.HasPerk(DefaultPerks.Steward.SevenVeterans, false) && troop.Character.Tier >= 4)
                    {
                        result.Add((float)this.GetPerkExperiencesForTroops(DefaultPerks.Steward.SevenVeterans), null, null);
                    }
                }
                #endregion
                #region DefaultPerks.Steward.DrillSergant
                if (BannerKingsSettings.Instance.EnableUsefulPerks && BannerKingsSettings.Instance.EnableUsefulStewardPerks)
                {
                    if (mobileParty.IsActive)
                    {
                        DefaultPerks.Steward.DrillSergant.AddScaledPerkBonus(ref result, false, mobileParty, DefaultSkills.Steward, 25, 25, 100, Utils.Helpers.SkillScale.Both, minValue: 0, maxValue: 30f);
                    }
                }
                else
                {
                    if (mobileParty.IsActive && mobileParty.HasPerk(DefaultPerks.Steward.DrillSergant, false))
                    {
                        result.Add((float)this.GetPerkExperiencesForTroops(DefaultPerks.Steward.DrillSergant), null, null);
                    }
                }
                #endregion

                if (troop.Character.Culture.IsBandit)
                {
                    PerkHelper.AddPerkBonusForParty(DefaultPerks.Roguery.NoRestForTheWicked, mobileParty, true, ref result);
                }
            },
            GetType().Name,
            false);

            return result;
        }
    }
}
