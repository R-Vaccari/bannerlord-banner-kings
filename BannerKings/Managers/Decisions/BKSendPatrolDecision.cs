using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Decisions
{
    public class BKSendPatrolDecision : BannerKingsDecision
    {
        public BKSendPatrolDecision(Settlement settlement, bool enabled) : base(settlement, enabled)
        {

        }

        public override string GetHint() => new TextObject("{=!}Send out small patrols to stop criminals. Patrols are cavalry-focused and require a at least 100 strong garrison").ToString();

        public override string GetIdentifier() => "decision_patrol_send";

        public override string GetName() => new TextObject("{=!}Send out patrols").ToString();
    }
}
