using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace BannerKings.Models.Vanilla
{
    public class BKSmithingModel : DefaultSmithingModel
    {

        public override int[] GetSmeltingOutputForItem(ItemObject item)
        {
            int[] result = base.GetSmeltingOutputForItem(item);
            int metalCount = 0;
            for (int i = 0; i < result.Length; i++)
                if (i >= 2 && i <= 6)
                    metalCount += result[i];

            if (item.WeaponComponent != null && item.WeaponComponent.PrimaryWeapon != null)
            {
                int metalCap = GetMetalMax(item.WeaponComponent.PrimaryWeapon.WeaponClass);
                if (metalCount > 0 && metalCap > 0)
                    while (metalCount > metalCap)
                        for (int i = 0; i < result.Length; i++)
                            if (i >= 2 && i <= 6 && result[i] > 0 && metalCount > metalCap)
                            {
                                result[i]--;
                                metalCount--;
                            }
                
                            
                if (item.WeaponComponent.PrimaryWeapon.WeaponClass == WeaponClass.Dagger)
                    result[7] = 0;

                if (result[1] == 0 && metalCap > 0) result[1]++;
            }

            return result;
        }

        public int GetMetalMax(WeaponClass weaponClass)
        {
            if (weaponClass == WeaponClass.Dagger || weaponClass == WeaponClass.ThrowingAxe ||
                weaponClass == WeaponClass.ThrowingKnife || weaponClass == WeaponClass.Crossbow ||
                weaponClass == WeaponClass.SmallShield)
                return 1;
            else if (weaponClass == WeaponClass.OneHandedSword || weaponClass == WeaponClass.LowGripPolearm ||
                weaponClass == WeaponClass.TwoHandedPolearm || weaponClass == WeaponClass.OneHandedPolearm ||
                weaponClass == WeaponClass.OneHandedAxe || weaponClass == WeaponClass.Mace ||
                weaponClass == WeaponClass.LargeShield || weaponClass == WeaponClass.Pick)
                return 2;
            else if (weaponClass == WeaponClass.TwoHandedAxe || weaponClass == WeaponClass.TwoHandedMace ||
                weaponClass == WeaponClass.TwoHandedSword)
                return 3;
            return -1;
        }


        public override int GetEnergyCostForSmithing(ItemObject item, Hero hero)
        {
            int max = Campaign.Current.GetCampaignBehavior<ICraftingCampaignBehavior>().GetMaxHeroCraftingStamina(hero);
            int result = base.GetEnergyCostForSmithing(item, hero);

            if (item.WeaponComponent != null && item.WeaponComponent.PrimaryWeapon != null)
            {
                WeaponClass weaponClass = item.WeaponComponent.PrimaryWeapon.WeaponClass;
                if (weaponClass == WeaponClass.TwoHandedAxe || weaponClass == WeaponClass.TwoHandedMace || weaponClass == WeaponClass.TwoHandedSword)
                    result = (int)(result * 1.5f);
                else if (weaponClass == WeaponClass.OneHandedSword || weaponClass == WeaponClass.OneHandedAxe || weaponClass == WeaponClass.Mace)
                    result = (int)(result * 1.2f);
            }

            return MBMath.ClampInt(result, 15, max);
        }

        public override int GetEnergyCostForSmelting(ItemObject item, Hero hero)
        {
            int result = base.GetEnergyCostForSmelting(item, hero);

            return result;
        }

        public override int GetEnergyCostForRefining(ref Crafting.RefiningFormula refineFormula, Hero hero)
        {
            int result = base.GetEnergyCostForRefining(ref refineFormula, hero);

            return result;
        }
    }
}
