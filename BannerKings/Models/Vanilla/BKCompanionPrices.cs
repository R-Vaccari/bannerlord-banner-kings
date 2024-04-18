using BannerKings.Settings;
using BannerKings.Utils;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace BannerKings.Models.Vanilla
{
    public class BKCompanionPrices : DefaultCompanionHiringPriceCalculationModel
    {
        public override int GetCompanionHiringPrice(Hero companion) => GetHiringPrice(companion, true);

        public int GetHiringPrice(Hero companion, bool addGearCosts)
        {
            var explainedNumber = new ExplainedNumber(0f);
            var currentSettlement = companion.CurrentSettlement;

            if (addGearCosts)
            {
                var town = currentSettlement?.Town;
                if (town == null)
                {
                    town = SettlementHelper.FindNearestTown().Town;
                }

                var num = 0f;
                for (var equipmentIndex = EquipmentIndex.WeaponItemBeginSlot;
                     equipmentIndex < EquipmentIndex.NumEquipmentSetSlots;
                     equipmentIndex++)
                {
                    var itemRosterElement = companion.CharacterObject.Equipment[equipmentIndex];
                    if (itemRosterElement.Item != null)
                    {
                        num += town.GetItemPrice(itemRosterElement);
                    }
                }

                for (var equipmentIndex2 = EquipmentIndex.WeaponItemBeginSlot;
                     equipmentIndex2 < EquipmentIndex.NumEquipmentSetSlots;
                     equipmentIndex2++)
                {
                    var itemRosterElement2 = companion.CharacterObject.FirstCivilianEquipment[equipmentIndex2];
                    if (itemRosterElement2.Item != null)
                    {
                        num += town.GetItemPrice(itemRosterElement2);
                    }
                }

                explainedNumber.Add(num / 2f);
            }

            explainedNumber.Add(companion.CharacterObject.Level * 10);
            var skills = MBObjectManager.Instance.GetObjectTypeList<SkillObject>();
            foreach (var skill in skills)
            {
                var skillValue = companion.GetSkillValue(skill);
                if (skillValue > 30)
                {
                    explainedNumber.Add(skillValue * GetCostFactor(skill));
                }
            }

            #region Steward.PaidInPromise 
            if (Hero.MainHero.IsPartyLeader && Hero.MainHero.GetPerkValue(DefaultPerks.Steward.PaidInPromise))
            {
                if (BannerKingsSettings.Instance.EnableUsefulPerks && BannerKingsSettings.Instance.EnableUsefulStewardPerks)
                {
                    DefaultPerks.Steward.PaidInPromise.AddScaledPersonlOrClanLeaderPerkBonusWithClanAndFamilyMembers(ref explainedNumber, false, Hero.MainHero, DefaultSkills.Steward, 20, 100, 0, minValue: -0.4f, maxValue: 0);
                }
                else
                {
                    explainedNumber.AddFactor(DefaultPerks.Steward.PaidInPromise.PrimaryBonus * 0.01f, null);
                }
            }
            #endregion

            if (Hero.MainHero.PartyBelongedTo != null)
            {
                PerkHelper.AddPerkBonusForParty(DefaultPerks.Trade.GreatInvestor, Hero.MainHero.PartyBelongedTo, false,
                    ref explainedNumber);
            }

            return (int)explainedNumber.ResultNumber;
        }

        public int GetHeroWage(Hero hero)
        {
            var totalCost = 0f;
            foreach (var skill in MBObjectManager.Instance.GetObjectTypeList<SkillObject>())
            {
                float skillValue = hero.GetSkillValue(skill);
                if (skillValue > 30)
                {
                    totalCost += skillValue * GetCostFactor(skill);
                }
            }

            return MathF.Round(totalCost * 0.005f);
            //return MBRandom.RoundRandomized(totalCost * 0.005f);//party wage keep changing every hour because of random value
        }

        public int GetCostFactor(SkillObject skill)
        {
            if (skill == DefaultSkills.Bow || skill == DefaultSkills.OneHanded || skill == DefaultSkills.TwoHanded ||
                skill == DefaultSkills.Crossbow || skill == DefaultSkills.Throwing || skill == DefaultSkills.Polearm ||
                skill == DefaultSkills.Riding || skill == DefaultSkills.Athletics)
            {
                return 3;
            }

            if (skill == DefaultSkills.Charm
                || skill == DefaultSkills.Roguery)
            {
                return 6;
            }

            if (skill == DefaultSkills.Scouting || skill == DefaultSkills.Crafting || skill == DefaultSkills.Medicine
                || skill == DefaultSkills.Engineering || skill == DefaultSkills.Trade)
            {
                return 10;
            }

            return 15;
        }
    }
}