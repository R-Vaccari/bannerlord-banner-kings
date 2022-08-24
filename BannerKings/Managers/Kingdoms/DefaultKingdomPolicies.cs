using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Kingdoms
{
    public class DefaultKingdomPolicies
    {
        public PolicyObject PrivilegedKnighthood { get; }

        public PolicyObject LimitedArmies { get; }

        public void Init()
        {
            PrivilegedKnighthood.Initialize(new TextObject("{=y9nBqXoev}Privileged Knighthood"),
                new TextObject(
                    "{=dX7aP14oU}Knights of cultures different to the kingdom's culture will no longer be accepted. Knights with demesne of kingdom culture will recieve a special tithe, helping them sustein their retinues."),
                new TextObject("{=e9Cc1XWvO}restricting knighthood to only their cultural peers"),
                new TextObject("{=titK8CM87}Cultural restriction for knighthood\nKnight rank lords recieve 10% extra income"), 0.1f,
                0.4f, -0.3f);

            LimitedArmies.Initialize(new TextObject("{=uNQ4ekLyB}Limited Army Privilege"),
                new TextObject("{=OB7fyQJw2}Armies can no longer be gathered by lords of rank lower than Duke."),
                new TextObject("{=e9Cc1XWvO}restricting knighthood to only their cultural peers"),
                new TextObject(
                    "{=rWLQ2Ew4o}Only lords with Duke title or higher may create armies\nArmy influence costs increased by 30%\nLords in army but not leading it receive 50% more influence"),
                0.1f, 0.4f, -0.3f);
        }
    }
}