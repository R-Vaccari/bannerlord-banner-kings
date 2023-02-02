using BannerKings.Managers.Skills;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.TwoDimension;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Rites
{
    public abstract class WeaponOffering : ContextualRite
    {
        protected readonly WeaponClass weaponClass;
        protected readonly ItemObject.ItemTiers minimumTier;
        protected EquipmentElement selectedItem;

        public WeaponOffering(WeaponClass weaponClass, ItemObject.ItemTiers minimumTier)
        {
            this.weaponClass = weaponClass;
            this.minimumTier = minimumTier;
        }

        public override void Complete(Hero actionTaker)
        {
            actionTaker.PartyBelongedTo.ItemRoster.AddToCounts(selectedItem, -1);
            
            var piety = GetPietyReward();
            BannerKingsConfig.Instance.ReligionsManager.AddPiety(actionTaker, piety, true);
            actionTaker.AddSkillXp(BKSkills.Instance.Theology, piety * 1.2f);

            if (actionTaker.GetPerkValue(BKPerks.Instance.TheologyRitesOfPassage))
            {
                actionTaker.Clan.AddRenown(5f);
            }
        }

        public override void Execute(Hero executor)
        {
            var roster = executor.PartyBelongedTo.ItemRoster;
            var list = new List<InquiryElement>();
            foreach (var element in roster)
            {
                var item = element.EquipmentElement.Item;
                if (item != null && item.HasWeaponComponent && item.PrimaryWeapon.WeaponClass == weaponClass &&
                    item.Tier >= minimumTier)
                {
                    list.Add(new InquiryElement(element.EquipmentElement,
                        element.EquipmentElement.GetModifiedItemName().ToString(),
                        new ImageIdentifier(item)));
                }
            }

            MBInformationManager.ShowMultiSelectionInquiry(
               new MultiSelectionInquiryData(
                   GetName().ToString(),
                   GetDescription().ToString(),
                   list, 
                   false, 
                   1, 
                   GameTexts.FindText("str_done").ToString(), 
                   string.Empty,
                   delegate (List<InquiryElement> x)
                   {
                       selectedItem = (EquipmentElement)x[0].Identifier;
                       SetDialogue();
                   }, 
                   null, 
                   string.Empty));
        }

        public override bool MeetsCondition(Hero hero, out TextObject reason)
        {
            reason = new TextObject("{=!}This rite is available to be performed.");
            var data = BannerKingsConfig.Instance.ReligionsManager.GetFaithfulData(hero);
            bool baseResult = hero.IsAlive && !hero.IsChild && !hero.IsPrisoner && hero.PartyBelongedTo != null &&
                             data != null && data.HasTimePassedForRite(GetRiteType(), GetTimeInterval(hero));
            bool hasItems = false;
            if (baseResult)
            {
                var roster = hero.PartyBelongedTo.ItemRoster;
                foreach (var element in roster)
                {
                    var item = element.EquipmentElement.Item;
                    if (item != null && item.HasWeaponComponent && item.PrimaryWeapon.WeaponClass == weaponClass && 
                        item.Tier >= minimumTier)
                    {
                        hasItems = true;
                        break;
                    }
                }
            }
            else
            {
                reason = new TextObject("{=!}Not enough time ({YEARS} years) have passed since the last rite of this type was performed.")
                    .SetTextVariable("YEARS", GetTimeInterval(hero).ToString("0.0"));
            }

            if (!hasItems)
            {
                reason = new TextObject("{=!}This rite requires a {TYPE} weapon of minimum tier {TIER}.")
                    .SetTextVariable("TYPE", GameTexts.FindText("str_inventory_weapon", ((int)weaponClass).ToString()).ToString())
                    .SetTextVariable("TIER", (minimumTier + 1).ToString());
            }

            return baseResult && hasItems;
        }

        public override float GetPietyReward()
        {
            return Mathf.Sqrt(selectedItem.ItemValue) * 3f;
        }

        public override RiteType GetRiteType()
        {
            return RiteType.OFFERING;
        }

        public override float GetTimeInterval(Hero hero)
        {
            var result = 2f;
            if (hero.GetPerkValue(BKPerks.Instance.TheologyRitesOfPassage))
            {
                result -= 0.25f;
            }

            return result;
        }

        public override TextObject GetRequirementsText(Hero hero)
        {
            return new TextObject("{=6Yj8erp7}May be performed every {YEARS} years\nRequires a weapon of type ({TYPE})")
                .SetTextVariable("YEARS", GetTimeInterval(hero))
                .SetTextVariable("TYPE", GameTexts.FindText("str_inventory_weapon", ((int)weaponClass).ToString()).ToString());
        }
    }
}
