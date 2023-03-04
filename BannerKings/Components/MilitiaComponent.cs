using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Components
{
    internal class MilitiaComponent : BannerKingsComponent
    {
        public MilitiaComponent(Settlement origin, MobileParty escortTarget) : base(origin, "{=scETr7Ej}Raised Militia from {ORIGIN}")
        {
            Escort = escortTarget;
            Behavior = AiBehavior.EscortParty;
        }

        [SaveableProperty(1001)] public MobileParty Escort { get; set; }

        [SaveableProperty(1002)] public AiBehavior Behavior { get; set; }

        public override TextObject Name => new TextObject("{=scETr7Ej}Raised Militia from {ORIGIN}")
            .SetTextVariable("ORIGIN", HomeSettlement.Name);

        private static MobileParty CreateParty(string id, Settlement origin, MobileParty escortTarget)
        {
            return MobileParty.CreateParty(id + origin, new MilitiaComponent(origin, escortTarget),
                delegate(MobileParty mobileParty)
                {
                    mobileParty.SetPartyUsedByQuest(true);
                    mobileParty.Party.Visuals.SetMapIconAsDirty();
                    mobileParty.Ai.SetInitiative(0.5f, 1f, float.MaxValue);
                    mobileParty.ShouldJoinPlayerBattles = true;
                    mobileParty.Aggressiveness = 0.1f;
                    mobileParty.Ai.SetMoveEscortParty(escortTarget);
                    mobileParty.SetWagePaymentLimit(Campaign.Current.Models.PartyWageModel.MaxWage);
                });
        }

        public static void CreateMilitiaEscort(Settlement origin, MobileParty escortTarget, MobileParty reference)
        {
            var caravan = CreateParty($"bk_raisedmilitia_{origin}", origin, escortTarget);
            caravan.InitializeMobilePartyAtPosition(reference.MemberRoster, reference.PrisonRoster, origin.GatePosition);
            caravan.Ai.SetMoveEscortParty(escortTarget);
            reference.MemberRoster.RemoveIf(roster => roster.Number > 0);
            reference.PrisonRoster.RemoveIf(roster => roster.Number > 0);
            GiveMounts(ref caravan);
            GiveFood(ref caravan);
            BannerKingsConfig.Instance.PopulationManager.AddParty(caravan);
        }

        public override void TickHourly()
        {
            var behavior = Behavior;
            if (behavior == AiBehavior.EscortParty)
            {
                MobileParty.Ai.SetMoveEscortParty(Escort);
            }
            else
            {
                MobileParty.Ai.SetMoveGoToSettlement(HomeSettlement);
            }
        }
    }
}