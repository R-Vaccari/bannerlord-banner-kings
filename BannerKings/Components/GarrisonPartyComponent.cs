using SandBox.View.Map;
using System.Linq;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.SaveSystem;

namespace BannerKings.Components
{
    public class GarrisonPartyComponent : BannerKingsComponent
    {
        public GarrisonPartyComponent(Settlement origin) : base(origin, "{=!}Patrol from {ORIGIN}")
        {
            HoursPatrolled = 0;
        }

        protected static string GetPartyId(Settlement origin) => "bkGarrisonParty_" + origin.Name;

        [SaveableProperty(3)] public MobileParty TargetParty { get; private set; }

        [SaveableProperty(4)] public int HoursPatrolled { get; private set; }

        public static MobileParty CreateParty(Settlement origin)
        {
            string id = GetPartyId(origin);
            if (MobileParty.All.FirstOrDefault(x => x.StringId == id) != null) return null;

            var minimum = 30;
            var garrisonRoster = origin.Town.GarrisonParty.MemberRoster;
            var maximum = (int)(garrisonRoster.TotalHealthyCount * 0.5f);
            if (maximum < 30) return null;

            var patrol = MobileParty.CreateParty(GetPartyId(origin),
                new GarrisonPartyComponent(origin),
                delegate (MobileParty mobileParty)
                {
                    mobileParty.SetPartyUsedByQuest(true);
                    mobileParty.Party.SetVisualAsDirty();
                    mobileParty.Ai.SetInitiative(1f, 0.5f, float.MaxValue);
                    mobileParty.ShouldJoinPlayerBattles = false;
                    mobileParty.Aggressiveness = 1f;
                    mobileParty.ActualClan = origin.OwnerClan;
                });

            TroopRoster members = new TroopRoster(patrol.Party);
            for (int i = 0; i < MBRandom.RandomInt(minimum, maximum); i++)
            {
                int index = MBRandom.RandomInt(0, garrisonRoster.GetTroopRoster().Count - 1);
                var element = garrisonRoster.GetElementCopyAtIndex(index);
                members.AddToCounts(element.Character, 1);
                garrisonRoster.AddToCounts(element.Character, -1);
            }

            PartyVisualManager.Current.GetVisualOfParty(patrol.Party).OnStartup();
            patrol.InitializeMobilePartyAtPosition(members, new TroopRoster(patrol.Party), origin.GatePosition);
            GiveMounts(ref patrol);
            return patrol;
        }

        public override void TickHourly()
        {
            if (MobileParty.MapEvent == null)
            {
                if (HoursPatrolled > 48 && MobileParty.TargetParty == null) ReturnHome();
                else if (MobileParty.Ai.DefaultBehavior != AiBehavior.EngageParty) 
                    MobileParty.Ai.SetMovePatrolAroundSettlement(Home.BoundVillages.GetRandomElement().Settlement);
            }
            HoursPatrolled++;
        }

        private void ReturnHome() => MobileParty.Ai.SetMoveGoToSettlement(Home);
    }
}
