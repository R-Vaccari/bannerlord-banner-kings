using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;
using TaleWorlds.Core;

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
    }
}
