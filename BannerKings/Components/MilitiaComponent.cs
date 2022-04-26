using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;
using static BannerKings.Managers.PopulationManager;
using TaleWorlds.SaveSystem;

namespace BannerKings.Components
{
    class MilitiaComponent : PopulationPartyComponent
    {
        [SaveableProperty(1001)]
        public MobileParty Escort { get; set; }

        [SaveableProperty(1002)]
        public AiBehavior Behavior { get; set; }

        public MilitiaComponent(Settlement origin, MobileParty escortTarget) : base(origin, origin, "", false, PopType.None)
        {
            this.Escort = escortTarget;
            this.Behavior = AiBehavior.EscortParty;
        }

        private static MobileParty CreateParty(string id, Settlement origin, MobileParty escortTarget)
        {
            return MobileParty.CreateParty(id + origin, new MilitiaComponent(origin, escortTarget),
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

        public static void CreateMilitiaEscort(Settlement origin, MobileParty escortTarget, MobileParty reference)
        {
            MobileParty caravan = CreateParty(string.Format("bk_raisedmilitia_{0}", origin), origin, escortTarget);
            caravan.InitializeMobilePartyAtPosition(reference.MemberRoster, reference.PrisonRoster, origin.GatePosition);
            caravan.SetMoveEscortParty(escortTarget);
            reference.MemberRoster.RemoveIf(roster => roster.Number > 0);
            reference.PrisonRoster.RemoveIf(roster => roster.Number > 0);
            GiveMounts(ref caravan);
            GiveFood(ref caravan);
            BannerKingsConfig.Instance.PopulationManager.AddParty(caravan);
        }

        public override Hero PartyOwner => HomeSettlement.OwnerClan.Leader;

        public override TextObject Name => new TextObject("Raised Militia from {SETTLEMENT}")
            .SetTextVariable("SETTLEMENT", HomeSettlement.Name);

        public override Settlement HomeSettlement => _target;
    }
}
