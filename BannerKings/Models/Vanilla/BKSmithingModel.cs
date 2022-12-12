using BannerKings.Managers.Education.Lifestyles;
using BannerKings.Managers.Skills;
using BannerKings.Settings;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using static TaleWorlds.Core.ArmorComponent;

namespace BannerKings.Models.Vanilla
{
    public class BKSmithingModel : DefaultSmithingModel
    {
        public ExplainedNumber GetSmithingHourlyPrice(Settlement settlement, Hero hero)
        {
            var result = new ExplainedNumber(BannerKingsSettings.Instance.SmithingGoldCostPerHour, true);

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
            var chance = 0.01f * (difficulty - hero.GetSkillValue(DefaultSkills.Crafting));
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
                result += item.ItemType switch
                {
                    ItemObject.ItemTypeEnum.BodyArmor => 50f,
                    ItemObject.ItemTypeEnum.HeadArmor => 30f,
                    _ => 10f
                };

                result += item.ArmorComponent.MaterialType switch
                {
                    ArmorMaterialTypes.Plate => 40f,
                    ArmorMaterialTypes.Chainmail => 25f,
                    _ => 10f
                };
            }
            else if (item.HasWeaponComponent)
            {
                result += item.ItemType switch
                {
                    ItemObject.ItemTypeEnum.Shield => 20f,
                    _ => 40f
                };
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
                switch (item.ItemType)
                {
                    case ItemObject.ItemTypeEnum.BodyArmor or ItemObject.ItemTypeEnum.HorseHarness:
                        result *= 1.5f;
                        break;
                    case ItemObject.ItemTypeEnum.HeadArmor:
                        result *= 1.2f;
                        break;
                }

                switch (item.ArmorComponent.MaterialType)
                {
                    case ArmorMaterialTypes.Plate:
                        result *= 1.4f;
                        break;
                    case ArmorMaterialTypes.Chainmail:
                        result *= 1.25f;
                        break;
                    case ArmorMaterialTypes.Leather:
                        result *= 1.1f;
                        break;
                }
            }
            else if (item.HasWeaponComponent)
            {
                if (item.ItemType == ItemObject.ItemTypeEnum.Shield)
                {
                    result += item.WeaponComponent.PrimaryWeapon.MaxDataValue / 10f;
                }
                else if (item.ItemType == ItemObject.ItemTypeEnum.Arrows || item.ItemType == ItemObject.ItemTypeEnum.Bolts)
                {
                    result += item.WeaponComponent.PrimaryWeapon.MaxDataValue * item.WeaponComponent.PrimaryWeapon.MissileDamage;
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
                switch (material)
                {
                    case ArmorMaterialTypes.Chainmail or ArmorMaterialTypes.Plate:
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
                            mainMaterial = item.Tierf switch
                            {
                                < 5f => CraftingMaterials.Iron4,
                                < 6f => CraftingMaterials.Iron5,
                                _ => CraftingMaterials.Iron6
                            };
                        }

                        var mainMaterialIndex = (int) mainMaterial;
                        result[mainMaterialIndex] = (int) (ingots * 0.9f);
                        result[mainMaterialIndex - 1] = (int) (ingots * 0.1f);
                        break;
                    }
                    case ArmorMaterialTypes.Leather:
                        result[9] = MBMath.ClampInt((int) (item.Weight / 10f), 1, 100);
                        break;
                    case ArmorMaterialTypes.Cloth:
                        result[10] = 1;
                        break;
                }
            }
            else if (item.HasWeaponComponent)
            {
                if (item.WeaponComponent.PrimaryWeapon.IsShield)
                {
                    result[7] = 2;
                    if (item.WeaponComponent.PrimaryWeapon.PhysicsMaterial == "shield_metal")
                    {
                        result[4] = (int) (item.Weight * 0.5f / 0.5f);
                    }
                }
                else
                {
                    CraftingMaterials mainMaterial;
                    mainMaterial = item.Tierf switch
                    {
                        < 3f => CraftingMaterials.Iron3,
                        < 4f => CraftingMaterials.Iron4,
                        < 5f => CraftingMaterials.Iron5,
                        _ => CraftingMaterials.Iron6
                    };

                    result[7] = 2;
                    result[(int)mainMaterial] = 2;
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
                if (i is >= 2 and <= 6)
                {
                    metalCount += result[i];
                }
            }

            if (item.WeaponComponent is {PrimaryWeapon: { }})
            {
                var metalCap = GetMetalMax(item.WeaponComponent.PrimaryWeapon.WeaponClass);
                if (metalCount > 0 && metalCap > 0)
                {
                    while (metalCount > metalCap)
                    {
                        for (var i = 0; i < result.Length; i++)
                        {
                            if (i is >= 2 and <= 6 && result[i] > 0 && metalCount > metalCap)
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
            return weaponClass switch
            {
                WeaponClass.Dagger or WeaponClass.ThrowingAxe or WeaponClass.ThrowingKnife or WeaponClass.Crossbow
                    or WeaponClass.SmallShield => 1,
                WeaponClass.OneHandedSword or WeaponClass.LowGripPolearm or WeaponClass.TwoHandedPolearm
                    or WeaponClass.OneHandedPolearm or WeaponClass.OneHandedAxe or WeaponClass.Mace
                    or WeaponClass.LargeShield or WeaponClass.Pick => 2,
                WeaponClass.TwoHandedAxe or WeaponClass.TwoHandedMace or WeaponClass.TwoHandedSword => 3,
                _ => -1
            };
        }


        public override int GetEnergyCostForSmithing(ItemObject item, Hero hero)
        {
            var max = Campaign.Current.GetCampaignBehavior<ICraftingCampaignBehavior>().GetMaxHeroCraftingStamina(hero);
            var result = base.GetEnergyCostForSmithing(item, hero);

            if (item.WeaponComponent is {PrimaryWeapon: { }})
            {
                var weaponClass = item.WeaponComponent.PrimaryWeapon.WeaponClass;
                result = weaponClass switch
                {
                    WeaponClass.TwoHandedAxe or WeaponClass.TwoHandedMace or WeaponClass.TwoHandedSword =>
                        (int) (result * 1.5f),
                    WeaponClass.OneHandedSword or WeaponClass.OneHandedAxe or WeaponClass.Mace => (int) (result * 1.2f),
                    _ => result
                };
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
}