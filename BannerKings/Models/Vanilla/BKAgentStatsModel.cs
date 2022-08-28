using BannerKings.Managers.Education;
using BannerKings.Managers.Skills;
using SandBox.GameComponents;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace BannerKings.Models.Vanilla
{
    public class BKAgentStatsModel : SandboxAgentStatCalculateModel
    {

        public override float GetEffectiveMaxHealth(Agent agent)
        {
            float result = base.GetEffectiveMaxHealth(agent);
            if (!agent.IsHuman)
            {
                Agent riderAgent = agent.RiderAgent;
                if (riderAgent != null)
                {
                    IAgentOriginBase origin = riderAgent.Origin;
                    if (origin != null)
                    {
                        PartyBase partyBase2 = (PartyBase)origin.BattleCombatant;
                        MobileParty party = (partyBase2 != null) ? partyBase2.MobileParty : null;

                        if (party != null && party.LeaderHero != null)
                        {
                            Hero hero = party.LeaderHero;
                            EducationData education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(hero);
                            if (education.HasPerk(BKPerks.Instance.RitterIronHorses))
                            {
                                result *= 1.1f;
                            }
                        }
                    }
                }
            }

            return result;
        }

        public override void UpdateAgentStats(Agent agent, AgentDrivenProperties agentDrivenProperties)
        {
            base.UpdateAgentStats(agent, agentDrivenProperties);
            //MissionWeapon missionWeapon = agent.WieldedWeapon;
            //WeaponComponentData weaponComponentData = (!missionWeapon.Equals(MissionWeapon.Invalid)) ? agent.Equipment[missionWeapon.CurrentUsageIndex].CurrentUsageItem  : null;
            if (agent.Character == null)
            {
                return;
            }

            if (agent.Formation is not {Captain: {IsHero: true}})
            {
                return;
            }

            var captain = (agent.Formation.Captain.Character as CharacterObject)?.HeroObject;
            var data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(captain);
            if (agent.HasMount && data.HasPerk(BKPerks.Instance.CataphractEquites))
            {
                agentDrivenProperties.MountChargeDamage *= 1.1f;
            }

            if (agent.HasMount && data.HasPerk(BKPerks.Instance.CataphractAdaptiveTactics))
            {
                agentDrivenProperties.MountManeuver *= 1.08f;
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