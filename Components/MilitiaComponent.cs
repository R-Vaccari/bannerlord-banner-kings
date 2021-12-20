using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;
using static Populations.PopulationManager;
using TaleWorlds.SaveSystem;

namespace Populations.Components
{
    class MilitiaComponent : PopulationPartyComponent
    {
        [SaveableProperty(1)]
        public MobileParty _escortTarget { get; set; }

        [SaveableProperty(2)]
        public AiBehavior behavior { get; set; }



        public MilitiaComponent(Settlement target, Settlement origin, string name, bool slaveCaravan, PopType popType,
            MobileParty escortTarget) : base(target, origin, name, slaveCaravan, popType)
        {
            this._escortTarget = escortTarget;
        }

        private static MobileParty CreateParty(string id, Settlement origin, bool slaveCaravan, Settlement target, string name, PopType popType, MobileParty escortTarget)
        {
            return MobileParty.CreateParty(id + origin, new MilitiaComponent(target, origin, String.Format(name, origin.Name.ToString()), slaveCaravan, popType, escortTarget),
                delegate (MobileParty mobileParty)
            {
                mobileParty.SetPartyUsedByQuest(true);
                mobileParty.Party.Visuals.SetMapIconAsDirty();
                mobileParty.SetInititave(0.5f, 1f, float.MaxValue);
                mobileParty.ShouldJoinPlayerBattles = true;
                mobileParty.Aggressiveness = 0.1f;
                mobileParty.SetMoveEscortParty(escortTarget);
                mobileParty.PaymentLimit = Campaign.Current.Models.PartyWageModel.MaxWage;
            });
        }

        public static void CreateMilitiaEscort(string id, Settlement origin, Settlement target, string name, MobileParty escortTarget, MobileParty reference)
        {
            MobileParty caravan = CreateParty(id, origin, true, target, name, PopType.None, escortTarget);
            caravan.InitializeMobilePartyAtPosition(reference.MemberRoster, reference.PrisonRoster, origin.GatePosition);
            reference.MemberRoster.RemoveIf(roster => roster.Number > 0);
            reference.PrisonRoster.RemoveIf(roster => roster.Number > 0);
            GiveMounts(ref caravan);
            GiveFood(ref caravan);
            PopulationConfig.Instance.PopulationManager.AddParty(caravan);
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
