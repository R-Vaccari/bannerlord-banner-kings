using BannerKings.Managers.Education.Lifestyles;
using BannerKings.Managers.Skills;
using SandBox.GameComponents;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace BannerKings.Models.Vanilla
{
    public class BKAgentDamageModel : SandboxAgentApplyDamageModel
    {

        public override bool CanWeaponDismount(Agent attackerAgent, WeaponComponentData attackerWeapon, in Blow blow, in AttackCollisionData collisionData)
        {
            bool result = base.CanWeaponDismount(attackerAgent, attackerWeapon, blow, collisionData);
            if (!result && attackerAgent.Formation != null && attackerAgent.Formation.Captain != null && 
                attackerWeapon.WeaponClass == WeaponClass.Javelin)
            {
                var aggressorCaptain = (attackerAgent.Formation.Captain.Character as CharacterObject).HeroObject;
                var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(aggressorCaptain);

                if (education.HasPerk(BKPerks.Instance.JawwalDuneRider) && MBRandom.RandomFloat < 0.05f)
                {
                    return true;
                }
            }

            return result;
        }

        public override float CalculateDamage(in AttackInformation attackInformation, in AttackCollisionData collisionData, in MissionWeapon weapon, float baseDamage)
        {
            var baseResult = base.CalculateDamage(in attackInformation, in collisionData, in weapon, baseDamage);
            var aggressorCaptain = attackInformation.AttackerCaptainCharacter as CharacterObject;
            var victimCaptain = attackInformation.VictimCaptainCharacter as CharacterObject;

            var agressorUsage = weapon.CurrentUsageItem;

            if (agressorUsage != null && attackInformation.AttackerAgentCharacter is CharacterObject aggressor)
            {
                if (aggressorCaptain is {IsHero: true})
                {
                    var data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(aggressorCaptain.HeroObject);

                    if (collisionData.StrikeType == 1) // thrust
                    {
                        if (aggressor.IsMounted && data.HasPerk(BKPerks.Instance.CataphractKlibanophoros))
                        {
                            baseResult *= 1.06f;
                        }
                    }

                    if (data.Lifestyle != null && data.HasPerk(BKPerks.Instance.KheshigOutrider))
                    {
                        if(aggressor.IsMounted && agressorUsage.RelevantSkill == DefaultSkills.Bow)
                        {
                            baseResult *= 1.05f;
                        }
                    }

                    if (data.Lifestyle != null && data.Lifestyle == DefaultLifestyles.Instance.Ritter)
                    {
                        if (!aggressor.IsMounted)
                        {

                            if (agressorUsage.WeaponClass == WeaponClass.TwoHandedSword && 
                                data.HasPerk(BKPerks.Instance.FianHighlander))
                            {
                                baseResult *= 1.04f;
                            }

                            if (data.HasPerk(BKPerks.Instance.VaryagDrengr))
                            {
                                baseResult *= 1.1f;
                            }
                        } 
                        else
                        {
                            if (agressorUsage.RelevantSkill == DefaultSkills.Throwing && 
                                data.HasPerk(BKPerks.Instance.JawwalCamelMaster))
                            {
                                baseResult *= 1.1f;
                            }
                        }
                    }
                    

                    if (data.Lifestyle != null)
                    {
                        if (data.Lifestyle == DefaultLifestyles.Instance.Ritter)
                        {
                            var notRanged = agressorUsage.RelevantSkill != DefaultSkills.Bow &&
                                            agressorUsage.RelevantSkill != DefaultSkills.Crossbow &&
                                            agressorUsage.RelevantSkill != DefaultSkills.Throwing;

                            if (aggressor.IsMounted)
                            {
                                if (notRanged)
                                {
                                    baseResult *= 1.05f;
                                }
                                else
                                {
                                    baseResult *= 0.85f;
                                }
                            }
                        }
                        else if (data.Lifestyle.Equals(DefaultLifestyles.Instance.Varyag) && aggressor.IsMounted)
                        {
                            baseResult *= 0.8f;
                        }
                    } 
                }

                if (aggressor.HeroObject != null)
                {
                    var data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(aggressor.HeroObject);

                    if (aggressor.IsMounted && data.Lifestyle != null && data.Lifestyle.Equals(DefaultLifestyles.Instance.Fian))
                    {
                        baseResult *= 0.75f;
                    }

                    if (agressorUsage.RelevantSkill == DefaultSkills.Bow && collisionData.CollisionBoneIndex != -1)
                    {
                        if (data.HasPerk(BKPerks.Instance.FianRanger))
                        {
                            baseResult *= 1.08f;
                        }
                    }


                    if (agressorUsage.RelevantSkill == DefaultSkills.TwoHanded && !attackInformation.DoesAttackerHaveMountAgent)
                    {
                        if (data.HasPerk(BKPerks.Instance.FianFennid))
                        {
                            baseResult *= 1.1f;
                        }
                    }

                    if (aggressor.IsMounted && data.Lifestyle == DefaultLifestyles.Instance.Fian)
                    {
                        baseResult *= 1f - DefaultLifestyles.Instance.Fian.SecondEffect * 0.1f;
                    }

                    if (aggressor.IsMounted && data.HasPerk(BKPerks.Instance.CataphractAdaptiveTactics) &&
                        (agressorUsage.RelevantSkill == DefaultSkills.Bow
                         || agressorUsage.RelevantSkill == DefaultSkills.OneHanded ||
                         agressorUsage.RelevantSkill == DefaultSkills.Polearm))
                    {
                        baseResult *= 1.05f;
                    }
                }
            }

            var missionWeapon = attackInformation.VictimMainHandWeapon;
            var victimUsage = missionWeapon.CurrentUsageItem;

            if (attackInformation.VictimAgentCharacter is CharacterObject victim)
            { 

                if (victimCaptain is { IsHero: true })
                {
                    var victimCaptainHero = victimCaptain.HeroObject;
                    var data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(victimCaptainHero);
                    if (victim.IsMounted)
                    {
                        if (data.HasPerk(BKPerks.Instance.CataphractKlibanophoros))
                        {
                            baseResult *= 0.95f;
                        }
                    }

                    if (victimUsage != null)
                    {
                        if (!victim.IsMounted && victimUsage.IsShield && data.HasPerk(BKPerks.Instance.VaryagShieldBrother))
                        {
                            baseResult *= 0.96f;
                        }
                    }
                }
            }

            return baseResult;
        }


        public override bool DecideCrushedThrough(Agent attackerAgent, Agent defenderAgent, float totalAttackEnergy, Agent.UsageDirection attackDirection, StrikeType strikeType, WeaponComponentData defendItem, bool isPassiveUsage)
        {
            if (attackerAgent.IsMainAgent)
                return true;
            return base.DecideCrushedThrough(attackerAgent, defenderAgent, totalAttackEnergy, attackDirection, strikeType, defendItem, isPassiveUsage);
        }

    }
}