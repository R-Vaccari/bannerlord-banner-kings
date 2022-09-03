using SandBox.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using BannerKings.Managers.Skills;

namespace BannerKings.Models.Vanilla
{
    public class BKBattleMoraleModel : SandboxBattleMoraleModel
    {

        public override float CalculateMoraleChangeToCharacter(Agent agent, float maxMoraleChange)
        {
            var result = base.CalculateMoraleChangeToCharacter(agent, maxMoraleChange);
            if (agent.IsHuman && Mission.Current != null)
            {
                var characterObject = agent.Character as CharacterObject;
                var origin = agent.Origin;
                var partyBase = (PartyBase)((origin != null) ? origin.BattleCombatant : null);
                var hero = (partyBase != null) ? partyBase.LeaderHero : null;
                if (characterObject != null && hero != null && !characterObject.IsMounted)
                {
                    var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(hero);
                    if (education.HasPerk(BKPerks.Instance.VaryagDrengr))
                    {
                        if (result < 0f)
                        {
                            result *= 0.8f;
                        }
                    }
                }
            }

            return result;
        }
    }
}
