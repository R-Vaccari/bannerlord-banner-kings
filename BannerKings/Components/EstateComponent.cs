using BannerKings.Managers.Populations.Estates;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Components
{
    internal class EstateComponent : BannerKingsComponent
    {
        public EstateComponent(Settlement origin, Estate estate) : base(origin, 
            "{=NzSOneTv}Estate Retinue from {ORIGIN}")
        {
            Behavior = AiBehavior.Hold;
            Estate = estate;
        }

        [SaveableProperty(1001)] public MobileParty Escort { get; set; }
        [SaveableProperty(1002)] public AiBehavior Behavior { get; set; }
        [SaveableProperty(1003)] public Estate Estate { get; set; }

        public override TextObject Name => new TextObject("{=NzSOneTv}Estate Retinue from {ORIGIN}")
            .SetTextVariable("ORIGIN", HomeSettlement.Name);

        private static MobileParty CreateParty(string id, Estate estate, Settlement origin)
        {
            return MobileParty.CreateParty(id, new EstateComponent(origin, estate),
                delegate(MobileParty mobileParty)
                {
                    mobileParty.SetPartyUsedByQuest(true);
                    mobileParty.Party.SetVisualAsDirty();
                    mobileParty.Ai.SetInitiative(0.5f, 1f, float.MaxValue);
                    mobileParty.ShouldJoinPlayerBattles = true;
                    mobileParty.Aggressiveness = 0.1f;
                    mobileParty.SetWagePaymentLimit(TaleWorlds.CampaignSystem.Campaign.Current.Models.PartyWageModel.MaxWage);
                });
        }

        public static void CreateRetinue(Estate estate)
        {
            Settlement origin = estate.EstatesData.Settlement;
            if (origin.MilitiaPartyComponent != null)
            {  
                MobileParty retinue = CreateParty($"bk_retinue_{origin}_{estate}_{MBRandom.RandomInt()}", estate, origin);
                retinue.InitializeMobilePartyAtPosition(origin.Culture.MilitiaPartyTemplate,
                origin.GatePosition,
                (int)(estate.MaxManpower.ResultNumber * 0.5f));
                GiveMounts(ref retinue);
                GiveFood(ref retinue);
                EnterSettlementAction.ApplyForParty(retinue, origin);
                estate.SetParty(retinue);
            }  
        }

        public override void TickHourly()
        {
            var behavior = Behavior;
            if (behavior == AiBehavior.EscortParty)
            {
                MobileParty.Ai.SetMoveEscortParty(Escort);
                if (MobileParty.CurrentSettlement != null) LeaveSettlementAction.ApplyForParty(MobileParty);
            }
            else if (behavior == AiBehavior.GoToSettlement)
            {
                MobileParty.Ai.SetMoveGoToSettlement(HomeSettlement);
                if (TaleWorlds.CampaignSystem.Campaign.Current.Models.MapDistanceModel.GetDistance(Party.MobileParty, HomeSettlement) <= 1f)
                    EnterSettlementAction.ApplyForParty(Party.MobileParty, HomeSettlement);
            }
        }
    }
}