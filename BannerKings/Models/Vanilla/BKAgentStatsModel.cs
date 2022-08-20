using BannerKings.Managers.Skills;
using SandBox.GameComponents;
using TaleWorlds.CampaignSystem;
using TaleWorlds.MountAndBlade;

namespace BannerKings.Models.Vanilla
{
    public class BKAgentStatsModel : SandboxAgentStatCalculateModel
    {
        public override void UpdateAgentStats(Agent agent, AgentDrivenProperties agentDrivenProperties)
        {
            base.UpdateAgentStats(agent, agentDrivenProperties);
            //MissionWeapon missionWeapon = agent.WieldedWeapon;
            //WeaponComponentData weaponComponentData = (!missionWeapon.Equals(MissionWeapon.Invalid)) ? agent.Equipment[missionWeapon.CurrentUsageIndex].CurrentUsageItem  : null;
            if (agent.Character != null)
            {
                if (agent.Formation != null && agent.Formation.Captain != null && agent.Formation.Captain.IsHero)
                {
                    var captain = (agent.Formation.Captain.Character as CharacterObject).HeroObject;
                    var data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(captain);
                    if (agent.HasMount && data.HasPerk(BKPerks.Instance.CataphractEquites))
                    {
                        agentDrivenProperties.MountChargeDamage *= 1.1f;
                    }

                    if (agent.HasMount && data.HasPerk(BKPerks.Instance.CataphractAdaptiveTactics))
                    {
                        agentDrivenProperties.MountManeuver *= 1.08f;
                    }
                }


                /*if (agent.Character.IsHero)
                {
                    Hero hero = (agent.Character as CharacterObject).HeroObject;
                    EducationData data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(hero);
                    if (data.HasPerk(BKPerks.Instance.FianFennid) && weaponComponentData.RelevantSkill == DefaultSkills.Bow)
                        agentDrivenProperties.ThrustOrRangedReadySpeedMultiplier *= 1.25f;
    
                    if (data.HasPerk(BKPerks.Instance.FianHighlander) && weaponComponentData.RelevantSkill == DefaultSkills.TwoHanded && !agent.HasMount && weaponComponentData.WeaponClass == WeaponClass.TwoHandedSword)
                        agentDrivenProperties.SwingSpeedMultiplier *= 1.06f;
                }*/
            }
        }
    }
}