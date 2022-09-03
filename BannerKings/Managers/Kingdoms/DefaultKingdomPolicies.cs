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
            PrivilegedKnighthood.Initialize(new TextObject("{=3UAFVNOE}Privileged Knighthood"),
                new TextObject("{=c9Cwm6Ek}Knights of cultures different to the kingdom's culture will no longer be accepted. Knights with demesne of kingdom culture will recieve a special tithe, helping them sustein their retinues."),
                new TextObject("{=9TThCFw6}restricting knighthood to only their cultural peers"),
                new TextObject("{=FrBNgywz}Cultural restriction for knighthood\nKnight rank lords recieve 10% extra income"), 0.1f,
                0.4f, -0.3f);

            LimitedArmies.Initialize(new TextObject("{=mLktnT01}Limited Army Privilege"),
                new TextObject("{=TPzPPnp9}Armies can no longer be gathered by lords of rank lower than Duke."),
                new TextObject("{=9TThCFw6}restricting knighthood to only their cultural peers"),
                new TextObject("{=rRo0Uz4x}Only lords with Duke title or higher may create armies\nArmy influence costs increased by 30%\nLords in army but not leading it receive 50% more influence"),
                0.1f, 0.4f, -0.3f);
        }
    }
}