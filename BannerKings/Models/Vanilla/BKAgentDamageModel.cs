using BannerKings.Managers.Education;
using BannerKings.Managers.Skills;
using SandBox;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace BannerKings.Models.Vanilla
{
    public class BKAgentDamageModel : SandboxAgentApplyDamageModel
    {
        
        public override float CalculateDamage(ref AttackInformation attackInformation, ref AttackCollisionData collisionData, in MissionWeapon weapon, float baseDamage)
        {
            float baseResult = base.CalculateDamage(ref attackInformation, ref collisionData, in weapon, baseDamage);
            CharacterObject characterObject = attackInformation.AttackerAgentCharacter as CharacterObject;
            WeaponComponentData currentUsageItem2 = weapon.CurrentUsageItem;

            if (currentUsageItem2 != null && characterObject != null && characterObject.HeroObject != null)
            {
                EducationData data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(characterObject.HeroObject);
                MBReadOnlyList<PerkObject> perks = data.Perks;
                if (currentUsageItem2.RelevantSkill == DefaultSkills.Bow && collisionData.CollisionBoneIndex != -1)
                    if (perks.Contains(BKPerks.Instance.FianRanger)) baseResult *= 1.08f;
                

                if (currentUsageItem2.RelevantSkill == DefaultSkills.TwoHanded && !attackInformation.DoesAttackerHaveMountAgent)
                    if (perks.Contains(BKPerks.Instance.FianFennid)) baseResult *= 1.1f;
            }
                
 
            return baseResult;
        }
    }
}
