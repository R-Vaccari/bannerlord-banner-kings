using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Components
{
    internal class RetinueComponent : PopulationPartyComponent
    {
        public RetinueComponent(Settlement origin) : base(origin, origin, "", false, PopType.None)
        {
            behavior = AiBehavior.Hold;
        }

        [SaveableProperty(1001)] public AiBehavior behavior { get; set; }

        public override Hero PartyOwner => HomeSettlement.OwnerClan.Leader;

        public override TextObject Name => new TextObject("{=MNYnLSej}Retinue from {SETTLEMENT}")
            .SetTextVariable("SETTLEMENT", HomeSettlement.Name);

        public override Settlement HomeSettlement => Target;

        private static MobileParty CreateParty(string id, Settlement origin)
        {
            return MobileParty.CreateParty(id, new RetinueComponent(origin),
                delegate(MobileParty mobileParty)
                {
                    mobileParty.SetPartyUsedByQuest(true);
                    mobileParty.Party.Visuals.SetMapIconAsDirty();
                    mobileParty.Ai.DisableAi();
                    mobileParty.Aggressiveness = 0f;
                });
        }

        public static MobileParty CreateRetinue(Settlement origin)
        {
            var retinue = CreateParty($"bk_retinue_{origin.Name}", origin);
            retinue.InitializeMobilePartyAtPosition(origin.Culture.DefaultPartyTemplate, origin.GatePosition, 4);
            EnterSettlementAction.ApplyForParty(retinue, origin);
            BannerKingsConfig.Instance.PopulationManager.AddParty(retinue);
            return retinue;
        }

        public void DailyTick(float level)
        {
            var party = MobileParty;
            var cap = (int) (level * 15f);
            if (party.MemberRoster.TotalManCount < cap)
            {
                var stacks = HomeSettlement.Culture.DefaultPartyTemplate.Stacks;
                var character = stacks[MBRandom.RandomInt(0, stacks.Count - 1)].Character;
                Party.AddMember(character, 1);
            }
            else if (party.MemberRoster.TotalManCount > cap)
            {
                var character = Party.MemberRoster.GetTroopRoster().GetRandomElement().Character;
                Party.MemberRoster.RemoveTroop(character);
            }
        }
    }
}