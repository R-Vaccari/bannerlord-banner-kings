using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;
using System.Linq;
using TaleWorlds.Core;
using static Populations.PopulationManager;

namespace Populations.Components
{
    class PopulationPartyComponent : PartyComponent
    {
        private Settlement _target;
        private string _name;
        public bool slaveCaravan;
        public PopType popType;
        public PopulationPartyComponent(Settlement target, string name, bool slaveCaravan, PopType popType) : base()
        {
            _target = target;
            _name = name;
            this.slaveCaravan = slaveCaravan;
            this.popType = popType;
        }

        private static MobileParty CreateParty(string id, string origin, bool slaveCaravan, Settlement target, string name, PopType popType)
        {
            return MobileParty.CreateParty(id + origin + target.Name.ToString(), new PopulationPartyComponent(target, String.Format(name, origin), slaveCaravan, popType),
                delegate (MobileParty mobileParty)
            {
                mobileParty.SetPartyUsedByQuest(true);
                mobileParty.Party.Visuals.SetMapIconAsDirty();
                mobileParty.SetInititave(0f, 1f, float.MaxValue);
                mobileParty.ShouldJoinPlayerBattles = false;
                mobileParty.Aggressiveness = 0f;
                mobileParty.SetMoveGoToSettlement(target);
            });
        }

        public static void CreateSlaveCaravan(string id, Settlement origin, Settlement target, string name, int slaves)
        {
            MobileParty caravan = CreateParty(id, origin.Name.ToString(), true, target, name, PopType.None);
            caravan.AddPrisoner(CharacterObject.All.FirstOrDefault(x => x.StringId == "looter"), slaves);
            caravan.InitializeMobileParty(origin.Culture.EliteCaravanPartyTemplate, origin.GatePosition, 0f, 0f, -1);
            GiveMounts(ref caravan);
            GiveFood(ref caravan);
            PopulationConfig.Instance.PopulationManager.AddParty(caravan);
        }

        public static void CreateTravellerParty(string id, Settlement origin, Settlement target, string name, int count, PopType type, CharacterObject civilian)
        {
            MobileParty party = CreateParty(id, origin.Name.ToString(), false, target, name, type);
            PopulationData data = PopulationConfig.Instance.PopulationManager.GetPopData(origin);
            data.UpdatePopType(type, count);
            TroopRoster roster = new TroopRoster(party.Party);
            roster.AddToCounts(civilian, count);
            party.InitializeMobileParty(roster, new TroopRoster(party.Party), origin.GatePosition, 0f);
            GiveFood(ref party);
            PopulationConfig.Instance.PopulationManager.AddParty(party);
        }

        private static void GiveMounts(ref MobileParty party)
        {
            int lacking = party.Party.NumberOfRegularMembers - party.Party.NumberOfMounts;
            ItemObject horse = Items.All.FirstOrDefault(x => x.StringId == "sumpter_horse");
            party.ItemRoster.AddToCounts(horse, lacking);
        }

        private static void GiveFood(ref MobileParty party)
        {
            int lacking = (party.InventoryCapacity - (int)party.ItemRoster.TotalWeight) / 10;
            ItemObject food = Items.AllTradeGoods.FirstOrDefault(x => x.StringId == "fish");
            party.ItemRoster.AddToCounts(food, lacking);
        }

        public override Hero PartyOwner => HomeSettlement.OwnerClan.Leader;

        public override TextObject Name
        {
            get
            {
                return new TextObject(String.Format(_name, HomeSettlement.Name.ToString()));
            }
        }

        public override Settlement HomeSettlement
        {
            get => _target;
        }
    }
}
