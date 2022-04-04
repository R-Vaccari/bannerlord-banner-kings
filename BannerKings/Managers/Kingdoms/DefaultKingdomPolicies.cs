using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Kingdoms
{
    public class DefaultKingdomPolicies
    {

        private PolicyObject privilegedKnighthood, limitedArmies;

        public PolicyObject PrivilegedKnighthood => this.privilegedKnighthood;
        public PolicyObject LimitedArmies => this.limitedArmies;

        public void Init()
        {
            this.privilegedKnighthood.Initialize(new TextObject("{=!}Privileged Knighthood", null), 
                new TextObject("{=!}Knights of cultures different to the kingdom's culture will no longer be accepted. Knights with demesne of kingdom culture will recieve a special tithe, helping them sustein their retinues.", null), 
                new TextObject("{=!}restricting knighthood to only their cultural peers", null), 
                new TextObject("{=!}Cultural restriction for knighthood\nKnight rank lords recieve 10% extra income", 
                null), 0.1f, 0.4f, -0.3f);

            this.limitedArmies.Initialize(new TextObject("{=!}Limited Army Privilege", null),
                new TextObject("{=!}Armies can no longer be gathered by lords of rank lower than Duke.", null),
                new TextObject("{=!}restricting knighthood to only their cultural peers", null),
                new TextObject("{=!}Only lords with Duke title or higher may create armies\nArmy influence costs increased by 30%\nLords in army but not leading it receive 50% more influence",
                null), 0.1f, 0.4f, -0.3f);
        }
    }
}
