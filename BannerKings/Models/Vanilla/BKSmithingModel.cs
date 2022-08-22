using BannerKings.Managers.Education.Lifestyles;
using BannerKings.Managers.Skills;
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
                switch (item.ItemType)
                {
                    case ItemObject.ItemTypeEnum.BodyArmor:
                        result += 50f;
                        break;
                    case ItemObject.ItemTypeEnum.HeadArmor:
                        result += 30f;
                        break;
                    default:
                        result += 10f;
                        break;
                }

                switch (item.ArmorComponent.MaterialType)
                {
                    case ArmorMaterialTypes.Plate:
                        result += 40f;
                        break;
                    case ArmorMaterialTypes.Chainmail:
                        result += 25f;
                        break;
                    default:
                        result += 10f;
                        break;
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
                            switch (item.Tierf)
                            {
                                case < 5f:
                                    mainMaterial = CraftingMaterials.Iron4;
                                    break;
                                case < 6f:
                                    mainMaterial = CraftingMaterials.Iron5;
                                    break;
                                default:
                                    mainMaterial = CraftingMaterials.Iron6;
                                    break;
                            }
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
            switch (weaponClass)
            {
                case WeaponClass.Dagger or WeaponClass.ThrowingAxe or WeaponClass.ThrowingKnife or WeaponClass.Crossbow or WeaponClass.SmallShield:
                    return 1;
                case WeaponClass.OneHandedSword or WeaponClass.LowGripPolearm or WeaponClass.TwoHandedPolearm or WeaponClass.OneHandedPolearm or WeaponClass.OneHandedAxe or WeaponClass.Mace or WeaponClass.LargeShield or WeaponClass.Pick:
                    return 2;
                case WeaponClass.TwoHandedAxe or WeaponClass.TwoHandedMace or WeaponClass.TwoHandedSword:
                    return 3;
                default:
                    return -1;
            }
        }


        public override int GetEnergyCostForSmithing(ItemObject item, Hero hero)
        {
            var max = Campaign.Current.GetCampaignBehavior<ICraftingCampaignBehavior>().GetMaxHeroCraftingStamina(hero);
            var result = base.GetEnergyCostForSmithing(item, hero);

            if (item.WeaponComponent is {PrimaryWeapon: { }})
            {
                var weaponClass = item.WeaponComponent.PrimaryWeapon.WeaponClass;
                switch (weaponClass)
                {
                    case WeaponClass.TwoHandedAxe or WeaponClass.TwoHandedMace or WeaponClass.TwoHandedSword:
                        result = (int) (result * 1.5f);
                        break;
                    case WeaponClass.OneHandedSword or WeaponClass.OneHandedAxe or WeaponClass.Mace:
                        result = (int) (result * 1.2f);
                        break;
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
}