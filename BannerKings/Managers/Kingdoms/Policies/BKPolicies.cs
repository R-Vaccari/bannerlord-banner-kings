using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Kingdoms.Policies
{
    public class BKPolicies : DefaultTypeInitializer<BKPolicies>
    {
        private PolicyObject armyPrivilege;

        public PolicyObject LimitedArmyPrivilege => armyPrivilege;
        public override void Initialize()
        {
            armyPrivilege = Game.Current.ObjectManager.RegisterPresumedObject(new PolicyObject("policy_limited_army_privilege"));
            armyPrivilege.Initialize(new TextObject("{=!}Limited Army Privilege", null), 
                new TextObject("{=!}The privilege of raising armies will be limited to lords of duke level or superior, as well as the crown Marshal.", null), 
                new TextObject("{=!}limiting the privilege to raise armies", null), 
                new TextObject("{=!}Only crown Marhsal or lords with Duke title or higher may create armies\nArmy influence costs increased by 30%\nArmy members receive 50% more influence", null), 
                0.7f, 0.15f, -0.7f);
        }
    }
}
