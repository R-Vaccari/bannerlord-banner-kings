using BannerKings.Managers.Skills;
using SandBox.GameComponents;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.MountAndBlade;

namespace BannerKings.Models.Vanilla
{
    public class BKAgentStatsModel : SandboxAgentStatCalculateModel
    {

        public override float GetEffectiveMaxHealth(Agent agent)
        {
            var result = base.GetEffectiveMaxHealth(agent);
            if (!agent.IsHuman)
            {
                var riderAgent = agent.RiderAgent;
                if (riderAgent != null)
                {
                    var origin = riderAgent.Origin;
                    if (origin != null)
                    {
                        var partyBase2 = (PartyBase)origin.BattleCombatant;
                        var party = (partyBase2 != null) ? partyBase2.MobileParty : null;

                        if (party != null && party.LeaderHero != null)
                        {
                            var hero = party.LeaderHero;
                            var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(hero);

                            if (riderAgent.Monster != null && riderAgent.Monster.StringId == "camel")
                            {
                                if (education.HasPerk(BKPerks.Instance.JawwalGhazw))
                                {
                                    result *= 1.1f;
                                }

                            } else
                            {
                                if (education.HasPerk(BKPerks.Instance.RitterIronHorses))
                                {
                                    result *= 1.1f;
                                }
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
            if (agent.Character == null)
            {
                return;
            }

            if (agent.Formation is not {Captain: {IsHero: true}})
            {
                return;
            }

            var captain = (agent.Formation.Captain.Character as CharacterObject)?.HeroObject;
            if (captain != null)
            {

                var data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(captain);

                if (agent.HasMount)
                {
                    if (data.HasPerk(BKPerks.Instance.CataphractEquites))
                    {
                        agentDrivenProperties.MountChargeDamage *= 1.1f;
                    }

                    if (data.HasPerk(BKPerks.Instance.CataphractAdaptiveTactics))
                    {
                        agentDrivenProperties.MountManeuver *= 1.08f;
                    }

                    if (agent.MountAgent.Monster.StringId == "camel" &&data.HasPerk(BKPerks.Instance.JawwalCamelMaster))
                    {
                        agentDrivenProperties.MountSpeed *= 1.08f;
                    }
                } 
                else 
                {
                    if (data.HasPerk(BKPerks.Instance.FianFennid))
                    {
                        agentDrivenProperties.ThrustOrRangedReadySpeedMultiplier *= 1.1f;
                    } 
                }
            }
        }
    }
}