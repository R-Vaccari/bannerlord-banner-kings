using BannerKings.Managers.Education.Lifestyles;
using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using static TaleWorlds.Core.ArmorComponent;

namespace BannerKings.Models.Vanilla;

public class BKSmithingModel : DefaultSmithingModel
{
    public ExplainedNumber GetSmithingHourlyPrice(Settlement settlement, Hero hero)
    {
        var result = new ExplainedNumber(50f, true);

        var prosperity = settlement.Prosperity / 5000f;
        if (prosperity >= 1f)
        {
            result.Add(prosperity, GameTexts.FindText("str_prosperity"));
        }

        if (hero.Clan.Tier > 0)
        {
            result.AddFactor(hero.Clan.Tier * 0.1f, GameTexts.FindText("str_clan"));
        }

        var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(hero);
        if (education.HasPerk(BKPerks.Instance.ArtisanSmith))
        {
            result.AddFactor(-0.15f, BKPerks.Instance.ArtisanSmith.Name);
        }

        return result;
    }

    public int GetModifierForCraftedItem(ItemObject item, Hero hero)
    {
        var result = 0;


        var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(hero);
        if (education.HasPerk(BKPerks.Instance.ArtisanCraftsman) && MBRandom.RandomFloat <= 0.05f)
        {
            result += 1;
        }

        return result;
    }


    public float CalculateBotchingChance(Hero hero, int difficulty)
    {
        var chance = (float) difficulty / hero.GetSkillValue(DefaultSkills.Crafting) + 1;
        var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(hero);
        if (education.Lifestyle == DefaultLifestyles.Instance.Artisan)
        {
            chance -= 0.1f;
        }

        return MBMath.ClampFloat(chance, 0f, 0.9f);
    }

    public int CalculateArmorStamina(ItemObject item, Hero hero)
    {
        float result = 0;
        result += item.Weight * 5f;
        result += item.Tierf * 5f;

        if (item.HasArmorComponent)
        {
            if (item.ItemType == ItemObject.ItemTypeEnum.BodyArmor)
            {
                result += 50f;
            }
            else if (item.ItemType == ItemObject.ItemTypeEnum.HeadArmor)
            {
                result += 30f;
            }
            else
            {
                result += 10f;
            }

            if (item.ArmorComponent.MaterialType == ArmorMaterialTypes.Plate)
            {
                result += 40f;
            }
            else if (item.ArmorComponent.MaterialType == ArmorMaterialTypes.Chainmail)
            {
                result += 25f;
            }
            else
            {
                result += 10f;
            }
        }

        var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(hero);
        if (education.HasPerk(BKPerks.Instance.ArtisanSmith))
        {
            result *= 0.9f;
        }

        return MBMath.ClampInt((int) result, 10, 300);
    }


    public int CalculateArmorDifficulty(ItemObject item)
    {
        var result = item.Tierf * 20f;
        if (item.HasArmorComponent)
        {
            if (item.ItemType == ItemObject.ItemTypeEnum.BodyArmor ||
                item.ItemType == ItemObject.ItemTypeEnum.HorseHarness)
            {
                result *= 1.5f;
            }
            else if (item.ItemType == ItemObject.ItemTypeEnum.HeadArmor)
            {
                result *= 1.2f;
            }

            if (item.ArmorComponent.MaterialType == ArmorMaterialTypes.Plate)
            {
                result *= 1.4f;
            }
            else if (item.ArmorComponent.MaterialType == ArmorMaterialTypes.Chainmail)
            {
                result *= 1.25f;
            }
            else if (item.ArmorComponent.MaterialType == ArmorMaterialTypes.Leather)
            {
                result *= 1.1f;
            }
        }

        return MBMath.ClampInt((int) result, 10, 300);
    }


    public int[] GetCraftingInputForArmor(ItemObject item)
    {
        var result = new int[11];

        if (item.HasArmorComponent)
        {
            var material = item.ArmorComponent.MaterialType;
            if (material == ArmorMaterialTypes.Chainmail || material == ArmorMaterialTypes.Plate)
            {
                var ingots = (int) (item.Weight * 0.8f / 0.5f);
                CraftingMaterials mainMaterial;

                if (item.Tierf < 4f)
                {
                    mainMaterial = CraftingMaterials.Iron3;
                    result[10] = 1;
                }
                else
                {
                    result[9] = 1;
                    if (item.Tierf < 5f)
                    {
                        mainMaterial = CraftingMaterials.Iron4;
                    }
                    else if (item.Tierf < 6f)
                    {
                        mainMaterial = CraftingMaterials.Iron5;
                    }
                    else
                    {
                        mainMaterial = CraftingMaterials.Iron6;
                    }
                }

                var mainMaterialIndex = (int) mainMaterial;
                result[mainMaterialIndex] = (int) (ingots * 0.9f);
                result[mainMaterialIndex - 1] = (int) (ingots * 0.1f);
            }
            else if (material == ArmorMaterialTypes.Leather)
            {
                result[9] = MBMath.ClampInt((int) (item.Weight / 10f), 1, 100);
            }
            else if (material == ArmorMaterialTypes.Cloth)
            {
                result[10] = 1;
            }
        }
        else if (item.HasWeaponComponent)
        {
            if (item.WeaponComponent.PrimaryWeapon.IsShield)
            {
                result[7] = 1;
                if (item.WeaponComponent.PrimaryWeapon.PhysicsMaterial == "shield_metal")
                {
                    result[4] = (int) (item.Weight * 0.5f / 0.5f);
                }

                ;
            }
            else
            {
                result[7] = 1;
                result[3] = 1;
            }
        }

        return result;
    }

    public override int[] GetSmeltingOutputForItem(ItemObject item)
    {
        var result = base.GetSmeltingOutputForItem(item);
        var metalCount = 0;
        for (var i = 0; i < result.Length; i++)
        {
            if (i >= 2 && i <= 6)
            {
                metalCount += result[i];
            }
        }

        if (item.WeaponComponent != null && item.WeaponComponent.PrimaryWeapon != null)
        {
            var metalCap = GetMetalMax(item.WeaponComponent.PrimaryWeapon.WeaponClass);
            if (metalCount > 0 && metalCap > 0)
            {
                while (metalCount > metalCap)
                {
                    for (var i = 0; i < result.Length; i++)
                    {
                        if (i >= 2 && i <= 6 && result[i] > 0 && metalCount > metalCap)
                        {
                            result[i]--;
                            metalCount--;
                        }
                    }
                }
            }


            if (item.WeaponComponent.PrimaryWeapon.WeaponClass == WeaponClass.Dagger)
            {
                result[7] = 0;
            }

            if (result[1] == 0 && metalCap > 0)
            {
                result[1]++;
            }
        }

        return result;
    }

    public int GetMetalMax(WeaponClass weaponClass)
    {
        if (weaponClass == WeaponClass.Dagger || weaponClass == WeaponClass.ThrowingAxe ||
            weaponClass == WeaponClass.ThrowingKnife || weaponClass == WeaponClass.Crossbow ||
            weaponClass == WeaponClass.SmallShield)
        {
            return 1;
        }

        if (weaponClass == WeaponClass.OneHandedSword || weaponClass == WeaponClass.LowGripPolearm ||
            weaponClass == WeaponClass.TwoHandedPolearm || weaponClass == WeaponClass.OneHandedPolearm ||
            weaponClass == WeaponClass.OneHandedAxe || weaponClass == WeaponClass.Mace ||
            weaponClass == WeaponClass.LargeShield || weaponClass == WeaponClass.Pick)
        {
            return 2;
        }

        if (weaponClass == WeaponClass.TwoHandedAxe || weaponClass == WeaponClass.TwoHandedMace ||
            weaponClass == WeaponClass.TwoHandedSword)
        {
            return 3;
        }

        return -1;
    }


    public override int GetEnergyCostForSmithing(ItemObject item, Hero hero)
    {
        var max = Campaign.Current.GetCampaignBehavior<ICraftingCampaignBehavior>().GetMaxHeroCraftingStamina(hero);
        var result = base.GetEnergyCostForSmithing(item, hero);

        if (item.WeaponComponent != null && item.WeaponComponent.PrimaryWeapon != null)
        {
            var weaponClass = item.WeaponComponent.PrimaryWeapon.WeaponClass;
            if (weaponClass == WeaponClass.TwoHandedAxe || weaponClass == WeaponClass.TwoHandedMace ||
                weaponClass == WeaponClass.TwoHandedSword)
            {
                result = (int) (result * 1.5f);
            }
            else if (weaponClass == WeaponClass.OneHandedSword || weaponClass == WeaponClass.OneHandedAxe ||
                     weaponClass == WeaponClass.Mace)
            {
                result = (int) (result * 1.2f);
            }
        }

        return MBMath.ClampInt(result, 15, max);
    }

    public override int GetEnergyCostForSmelting(ItemObject item, Hero hero)
    {
        var result = base.GetEnergyCostForSmelting(item, hero);

        return result;
    }

    public override int GetEnergyCostForRefining(ref Crafting.RefiningFormula refineFormula, Hero hero)
    {
        var result = base.GetEnergyCostForRefining(ref refineFormula, hero);

        return result;
    }
}