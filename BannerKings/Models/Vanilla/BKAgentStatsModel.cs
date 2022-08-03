using BannerKings.Managers.Education;
using BannerKings.Managers.Skills;
using SandBox;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace BannerKings.Models.Vanilla
{
    public class BKAgentStatsModel : SandboxAgentStatCalculateModel
    {
        public override void UpdateAgentStats(Agent agent, AgentDrivenProperties agentDrivenProperties)
        {
            base.UpdateAgentStats(agent, agentDrivenProperties);
            EquipmentIndex wieldedItemIndex3 = agent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
            WeaponComponentData weaponComponentData = (wieldedItemIndex3 != EquipmentIndex.None) ? agent.Equipment[wieldedItemIndex3].CurrentUsageItem : null;
            if (weaponComponentData != null && agent.Character != null)
            {
                if (agent.Formation != null && agent.Formation.Captain != null && agent.Formation.Captain.IsHero)
                {
                    Hero captain = (agent.Formation.Captain.Character as CharacterObject).HeroObject;
                    EducationData data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(captain);
                    if (agent.HasMount && data.HasPerk(BKPerks.Instance.CataphractEquites))
                        agentDrivenProperties.MountChargeDamage *= 1.1f;

                    if (agent.HasMount && data.HasPerk(BKPerks.Instance.CataphractAdaptiveTactics))
                        agentDrivenProperties.MountManeuver *= 1.08f;
                }
                

                if (agent.Character.IsHero)
                {
                    Hero hero = (agent.Character as CharacterObject).HeroObject;
                    EducationData data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(hero);
                    if (data.HasPerk(BKPerks.Instance.FianFennid) && weaponComponentData.RelevantSkill == DefaultSkills.Bow)
                        agentDrivenProperties.ThrustOrRangedReadySpeedMultiplier *= 1.25f;

                    if (data.HasPerk(BKPerks.Instance.FianHighlander) && weaponComponentData.RelevantSkill == DefaultSkills.TwoHanded && !agent.HasMount && weaponComponentData.WeaponClass == WeaponClass.TwoHandedSword)
                        agentDrivenProperties.SwingSpeedMultiplier *= 1.06f;
                }
            }
        }
    }
}
