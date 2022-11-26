using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Components
{
    internal class MilitiaComponent : PopulationPartyComponent
    {
        public MilitiaComponent(Settlement origin, MobileParty escortTarget) : base(origin, origin, "", false, PopType.None)
        {
            Escort = escortTarget;
            Behavior = AiBehavior.EscortParty;
        }

        [SaveableProperty(1001)] public MobileParty Escort { get; set; }

        [SaveableProperty(1002)] public AiBehavior Behavior { get; set; }

        public override Hero PartyOwner => HomeSettlement.OwnerClan.Leader;

        public override TextObject Name => new TextObject("{=mNwggeSn}Raised Militia from {SETTLEMENT}")
            .SetTextVariable("SETTLEMENT", HomeSettlement.Name);

        public override Settlement HomeSettlement => Target;

        private static MobileParty CreateParty(string id, Settlement origin, MobileParty escortTarget)
        {
            return MobileParty.CreateParty(id + origin, new MilitiaComponent(origin, escortTarget),
                delegate(MobileParty mobileParty)
                {
                    mobileParty.SetPartyUsedByQuest(true);
                    mobileParty.Party.Visuals.SetMapIconAsDirty();
                    mobileParty.SetInitiative(0.5f, 1f, float.MaxValue);
                    mobileParty.ShouldJoinPlayerBattles = true;
                    mobileParty.Aggressiveness = 0.1f;
                    mobileParty.SetMoveEscortParty(escortTarget);
                    mobileParty.PaymentLimit = Campaign.Current.Models.PartyWageModel.MaxWage;
                });
        }

        public static void CreateMilitiaEscort(Settlement origin, MobileParty escortTarget, MobileParty reference)
        {
            var caravan = CreateParty($"bk_raisedmilitia_{origin}", origin, escortTarget);
            caravan.InitializeMobilePartyAtPosition(reference.MemberRoster, reference.PrisonRoster, origin.GatePosition);
            caravan.SetMoveEscortParty(escortTarget);
            reference.MemberRoster.RemoveIf(roster => roster.Number > 0);
            reference.PrisonRoster.RemoveIf(roster => roster.Number > 0);
            GiveMounts(ref caravan);
            GiveFood(ref caravan);
            BannerKingsConfig.Instance.PopulationManager.AddParty(caravan);
        }
    }
}