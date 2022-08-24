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
            PrivilegedKnighthood.Initialize(new TextObject("{=nST3M8jt}Privileged Knighthood"),
                new TextObject(
                    "{=CpkG9oj1}Knights of cultures different to the kingdom's culture will no longer be accepted. Knights with demesne of kingdom culture will recieve a special tithe, helping them sustein their retinues."),
                new TextObject("{=Aac1vr4c}restricting knighthood to only their cultural peers"),
                new TextObject("{=yGGBSmS7}Cultural restriction for knighthood\nKnight rank lords recieve 10% extra income"), 0.1f,
                0.4f, -0.3f);

            LimitedArmies.Initialize(new TextObject("{=HMznfMYo}Limited Army Privilege"),
                new TextObject("{=Bm0y34GY}Armies can no longer be gathered by lords of rank lower than Duke."),
                new TextObject("{=Aac1vr4c}restricting knighthood to only their cultural peers"),
                new TextObject(
                    "{=v7VpnU96}Only lords with Duke title or higher may create armies\nArmy influence costs increased by 30%\nLords in army but not leading it receive 50% more influence"),
                0.1f, 0.4f, -0.3f);
        }
    }
}