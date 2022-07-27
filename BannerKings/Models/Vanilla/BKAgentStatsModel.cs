using BannerKings.Managers.Education;
using BannerKings.Managers.Skills;
using SandBox;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
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
            if (weaponComponentData != null && agent.Character != null && agent.Character.IsHero)
            {
                Hero hero = (agent.Character as CharacterObject).HeroObject;
                EducationData data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(hero);
                MBReadOnlyList<PerkObject> perks = data.Perks;
                if (weaponComponentData.RelevantSkill == DefaultSkills.Bow && perks.Contains(BKPerks.Instance.FianFennid))
                    agentDrivenProperties.ThrustOrRangedReadySpeedMultiplier *= 1.25f;

                if (weaponComponentData.RelevantSkill == DefaultSkills.TwoHanded && !agent.HasMount && weaponComponentData.WeaponClass == WeaponClass.TwoHandedSword)
                    agentDrivenProperties.SwingSpeedMultiplier *= 1.06f;
            }
        }
    }
}
