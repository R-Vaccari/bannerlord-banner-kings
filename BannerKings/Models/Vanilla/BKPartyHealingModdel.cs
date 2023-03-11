using BannerKings.Managers.Court.Members.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;

namespace BannerKings.Models.Vanilla
{
    public class BKPartyHealingModdel : DefaultPartyHealingModel
    {
        public override ExplainedNumber GetDailyHealingHpForHeroes(MobileParty party, bool includeDescriptions = false)
        {
            ExplainedNumber result = base.GetDailyHealingHpForHeroes(party, includeDescriptions);
            Hero leader = party.LeaderHero;
            if (leader != null && party.CurrentSettlement != null)
            {
                if (BannerKingsConfig.Instance.CourtManager.HasCurrentTask(leader.Clan, DefaultCouncilTasks.Instance.FamilyCare,
                    out float healCompetence))
                {
                    result.AddFactor(0.2f * healCompetence, DefaultCouncilTasks.Instance.FamilyCare.Name);
                }
            }

            return result;
        }
    }
}
