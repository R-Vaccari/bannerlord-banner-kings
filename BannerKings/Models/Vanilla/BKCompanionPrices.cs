using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;

namespace BannerKings.Models.Vanilla;

public class BKCompanionPrices : DefaultCompanionHiringPriceCalculationModel
{
    public override int GetCompanionHiringPrice(Hero companion)
    {
        var explainedNumber = new ExplainedNumber(0f);
        var currentSettlement = companion.CurrentSettlement;
        var town = currentSettlement != null ? currentSettlement.Town : null;
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


        if (Hero.MainHero.IsPartyLeader && Hero.MainHero.GetPerkValue(DefaultPerks.Steward.PaidInPromise))
        {
            explainedNumber.AddFactor(DefaultPerks.Steward.PaidInPromise.PrimaryBonus * 0.01f);
        }

        if (Hero.MainHero.PartyBelongedTo != null)
        {
            PerkHelper.AddPerkBonusForParty(DefaultPerks.Trade.GreatInvestor, Hero.MainHero.PartyBelongedTo, false,
                ref explainedNumber);
        }

        return (int) explainedNumber.ResultNumber;
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