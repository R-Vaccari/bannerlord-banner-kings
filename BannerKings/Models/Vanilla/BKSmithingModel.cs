using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;
using TaleWorlds.Core;
using TaleWorlds.Library;
using static TaleWorlds.Core.ArmorComponent;

namespace BannerKings.Models.Vanilla
{
    public class BKSmithingModel : DefaultSmithingModel
    {
        public int GetModifierForCraftedItem(ItemObject item)
        {
            return 0;
        }


        public float CalculateBotchingChance(Hero hero, int difficulty)
        {
            float chance = (float)difficulty / hero.GetSkillValue(DefaultSkills.Crafting) + 1;

            return MBMath.ClampFloat(chance, 0f, 0.9f);
        }

        public int CalculateArmorStamina(ItemObject item)
        {
            float result = 0;
            result += item.Weight * 5f;
            result += item.Tierf * 5f;

            if (item.HasArmorComponent)
            {
                if (item.ItemType == ItemObject.ItemTypeEnum.BodyArmor) result += 50f;
                else if (item.ItemType == ItemObject.ItemTypeEnum.HeadArmor) result += 30f;
                else result += 10f;

                if (item.ArmorComponent.MaterialType == ArmorMaterialTypes.Plate) result += 40f;
                else if (item.ArmorComponent.MaterialType == ArmorMaterialTypes.Chainmail) result += 25f;
                else result += 10f;
            }

            return MBMath.ClampInt((int)result, 10, 300);
        }


        public int CalculateArmorDifficulty(ItemObject item)
        {
            float result = item.Tierf * 20f;
            if (item.HasArmorComponent)
            {
                if (item.ItemType == ItemObject.ItemTypeEnum.BodyArmor || item.ItemType == ItemObject.ItemTypeEnum.HorseHarness) result *= 1.5f;
                else if (item.ItemType == ItemObject.ItemTypeEnum.HeadArmor) result *= 1.2f;

                if (item.ArmorComponent.MaterialType == ArmorMaterialTypes.Plate) result *= 1.4f;
                else if (item.ArmorComponent.MaterialType == ArmorMaterialTypes.Chainmail) result *= 1.25f;
                else if (item.ArmorComponent.MaterialType == ArmorMaterialTypes.Leather) result *= 1.1f;
            }

            return MBMath.ClampInt((int)result, 10, 300);
        }


        public int[] GetCraftingInputForArmor(ItemObject item)
        {
            int[] result = new int[11];

            if (item.HasArmorComponent)
            {
                ArmorMaterialTypes material = item.ArmorComponent.MaterialType;
                if (material == ArmorMaterialTypes.Chainmail || material == ArmorMaterialTypes.Plate)
                {
                    int ingots = (int)((item.Weight * 0.8f) / 0.5f);
                    CraftingMaterials mainMaterial;

                    if (item.Tierf < 4f)
                    {
                        mainMaterial = CraftingMaterials.Iron3;
                        result[10] = 1;
                    }
                    else
                    {
                        result[9] = 1;
                        if (item.Tierf < 5f) mainMaterial = CraftingMaterials.Iron4;
                        else if (item.Tierf < 6f) mainMaterial = CraftingMaterials.Iron5;
                        else mainMaterial = CraftingMaterials.Iron6;
                    }

                    int mainMaterialIndex = (int)mainMaterial;
                    result[mainMaterialIndex] = (int)(ingots * 0.9f);
                    result[mainMaterialIndex - 1] = (int)(ingots * 0.1f);
                }
                else if (material == ArmorMaterialTypes.Leather) result[9] = MBMath.ClampInt((int)(item.Weight / 1f), 1, 100);
                else if (material == ArmorMaterialTypes.Cloth) result[10] = 1;
            } else if (item.HasWeaponComponent)
            {
                
                if (item.WeaponComponent.PrimaryWeapon.IsShield)
                {
                    result[7] = 1;
                    if (item.WeaponComponent.PrimaryWeapon.PhysicsMaterial == "shield_metal")
                        result[4] = (int)((item.Weight * 0.5f) / 0.5f); ;
                    
                } else
                {
                    result[7] = 1;
                    result[3] = 1;
                }
            }           

            return result;
        }

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
