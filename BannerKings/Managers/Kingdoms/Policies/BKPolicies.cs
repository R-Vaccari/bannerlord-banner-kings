using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Kingdoms.Policies
{
    public class BKPolicies : DefaultTypeInitializer<BKPolicies, PolicyObject>
    {
        public PolicyObject LimitedArmyPrivilege { get; private set; }

        public override IEnumerable<PolicyObject> All
        {
            get { yield return LimitedArmyPrivilege; }
        }

        public override void Initialize()
        {
            LimitedArmyPrivilege =
                Game.Current.ObjectManager.RegisterPresumedObject(new PolicyObject("policy_limited_army_privilege"));
            LimitedArmyPrivilege.Initialize(new TextObject("{=XkAhxqeOh}Limited Army Privilege"),
                new TextObject(
                    "{=AD8ur13Uo}The privilege of raising armies will be limited to lords of duke level or superior, as well as the crown Marshal."),
                new TextObject("{=AKGyNLJpz}limiting the privilege to raise armies"),
                new TextObject(
                    "{=tur5eZadp}Only crown Marhsal or lords with Duke title or higher may create armies\nArmy influence costs increased by 30%\nArmy members receive 50% more influence"),
                0.7f, 0.15f, -0.7f);
        }
    }
}