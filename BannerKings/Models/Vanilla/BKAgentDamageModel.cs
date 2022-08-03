using BannerKings.Managers.Education;
using BannerKings.Managers.Education.Lifestyles;
using BannerKings.Managers.Skills;
using SandBox;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace BannerKings.Models.Vanilla
{
    public class BKAgentDamageModel : SandboxAgentApplyDamageModel
    {
        
        public override float CalculateDamage(ref AttackInformation attackInformation, ref AttackCollisionData collisionData, in MissionWeapon weapon, float baseDamage)
        {
            float baseResult = base.CalculateDamage(ref attackInformation, ref collisionData, in weapon, baseDamage);
            CharacterObject aggressor = attackInformation.AttackerAgentCharacter as CharacterObject;
            CharacterObject aggressorCaptain = attackInformation.AttackerCaptainCharacter as CharacterObject;
            CharacterObject victim = attackInformation.VictimAgentCharacter as CharacterObject;
            CharacterObject victimCaptain = attackInformation.VictimCaptainCharacter as CharacterObject;

            WeaponComponentData agressorUsage = weapon.CurrentUsageItem;

            if (agressorUsage != null && aggressor != null)
            {

                if (aggressorCaptain != null && aggressorCaptain.IsHero)
                {
                    EducationData data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(aggressorCaptain.HeroObject);
                    if (collisionData.StrikeType == 1) // thrust
                    {
                        if (aggressor.IsMounted && data.HasPerk(BKPerks.Instance.CataphractKlibanophoros))
                            baseResult *= 1.06f;
                    }
                }

                if (aggressor.HeroObject != null)
                {
                    EducationData data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(aggressor.HeroObject);
                    if (agressorUsage.RelevantSkill == DefaultSkills.Bow && collisionData.CollisionBoneIndex != -1)
                        if (data.HasPerk(BKPerks.Instance.FianRanger)) baseResult *= 1.08f;


                    if (agressorUsage.RelevantSkill == DefaultSkills.TwoHanded && !attackInformation.DoesAttackerHaveMountAgent)
                        if (data.HasPerk(BKPerks.Instance.FianFennid)) baseResult *= 1.1f;

                    if (aggressor.IsMounted && data.Lifestyle == DefaultLifestyles.Instance.Fian)
                        baseResult *= 1f - (DefaultLifestyles.Instance.Fian.SecondEffect * 0.1f);

                    if (aggressor.IsMounted && data.HasPerk(BKPerks.Instance.CataphractAdaptiveTactics) && (agressorUsage.RelevantSkill == DefaultSkills.Bow
                        || agressorUsage.RelevantSkill == DefaultSkills.OneHanded || agressorUsage.RelevantSkill == DefaultSkills.Polearm))
                        baseResult *= 1.05f;
                }
            }

            if (victim != null)
            {
                
                if (victim.IsMounted && victimCaptain != null && victimCaptain.IsHero)
                {
                    Hero victimCaptainHero = victimCaptain.HeroObject;
                    EducationData data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(victimCaptainHero);
                    if (data.HasPerk(BKPerks.Instance.CataphractKlibanophoros))
                        baseResult *= 0.95f;
                }
            }
                
 
            return baseResult;
        }

    }
}
