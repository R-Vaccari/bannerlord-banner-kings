using SandBox.View.Map;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.SaveSystem;

namespace BannerKings.Components
{
    public class GarrisonPartyComponent : BannerKingsComponent
    {
        public GarrisonPartyComponent(MobileParty target, Settlement origin) : base(origin, "{=YfLY43hV}Garrison from {ORIGIN}")
        {
            TargetParty = target;
            HoursPatrolled = 0;
        }

        protected static string GetPartyId(Settlement origin) => "bkGarrisonParty_" + origin.Name;

        [SaveableProperty(3)] public MobileParty TargetParty { get; private set; }

        [SaveableProperty(4)] public int HoursPatrolled { get; private set; }

        public static MobileParty CreateParty(Settlement origin, MobileParty target)
        {
            string id = GetPartyId(origin);
            if (MobileParty.All.FirstOrDefault(x => x.StringId == id) != null)
            {
                return null;
            }

            var patrol = MobileParty.CreateParty(GetPartyId(origin),
                new GarrisonPartyComponent(target, origin),
                delegate (MobileParty mobileParty)
                {
                    mobileParty.SetPartyUsedByQuest(true);
                    mobileParty.Party.SetVisualAsDirty();
                    mobileParty.Ai.SetInitiative(1f, 0.5f, float.MaxValue);
                    mobileParty.ShouldJoinPlayerBattles = true;
                    mobileParty.Aggressiveness = 1f;
                    mobileParty.Ai.DisableAi();
                    mobileParty.Ai.SetMoveEngageParty(target);
                });
            TroopRoster members = new TroopRoster(patrol.Party);
            var garrisonRoster = origin.Town.GarrisonParty.MemberRoster;
            var minimum = target.MemberRoster.TotalHealthyCount;
            var maximum = (int)(garrisonRoster.TotalHealthyCount * 0.5f);
            if (maximum < minimum || minimum < 25)
            {
                return null;
            }

            for (int i = 0; i < MBRandom.RandomInt(minimum, maximum); i++)
            {
                int index = MBRandom.RandomInt(0, garrisonRoster.GetTroopRoster().Count - 1);
                var element = garrisonRoster.GetElementCopyAtIndex(index);
                members.AddToCounts(element.Character, 1);
                garrisonRoster.AddToCounts(element.Character, -1);
            }

            PartyVisualManager.Current.GetVisualOfParty(patrol.Party).OnStartup();
            patrol.InitializeMobilePartyAtPosition(members, new TroopRoster(patrol.Party), origin.GatePosition);
            return patrol;
        }

        public override void TickHourly()
        {
            /*var garrisonPartyPower = MobileParty.GetTotalStrengthWithFollowers();
            IEnumerable<MobileParty> enemies = MobileParty.FindPartiesAroundPosition(Home.GatePosition,
                40f,
                (MobileParty p) =>
                {
                    return p.MapFaction.IsKingdomFaction && p.MapFaction.IsAtWarWith(MobileParty.MapFaction) &&
                    p.GetTotalStrengthWithFollowers() > garrisonPartyPower;
                });

            if (enemies.Any())
            {
                ReturnHome();
                return;
            }

            if (TargetParty != null && TargetParty.IsActive &&
                garrisonPartyPower >= TargetParty.GetTotalStrengthWithFollowers() &&
                TaleWorlds.CampaignSystem.Campaign.Current.Models.MapDistanceModel.GetDistance(TargetParty, Home) <= 10f)
            {
                MobileParty.SetMoveEngageParty(TargetParty);
            }
            else if (HoursPatrolled < 12)
            {
                MobileParty.SetMovePatrolAroundSettlement(Home);
                HoursPatrolled++;
            }
            else
            {
                ReturnHome();
            }*/
            ReturnHome();
        }

        private void ReturnHome() => MobileParty.Ai.SetMoveGoToSettlement(Home);
    }
}
