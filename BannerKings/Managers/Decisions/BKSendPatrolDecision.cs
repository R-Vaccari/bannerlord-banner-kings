using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Decisions
{
    public class BKSendPatrolDecision : BannerKingsDecision
    {
        public BKSendPatrolDecision(Settlement settlement, bool enabled) : base(settlement, enabled)
        {
        }

        public override string GetHint()
        {
            return new TextObject("{=BtTGO2bG}Send out small patrols to stop criminals. Patrols are cavalry-focused and require a at least 100 strong garrison").ToString();
        }

        public override string GetIdentifier()
        {
            return "decision_patrol_send";
        }

        public override string GetName()
        {
            return new TextObject("{=Pj7fYmTP}Send out patrols").ToString();
        }
    }
}