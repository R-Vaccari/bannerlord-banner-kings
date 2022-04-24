using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;
using static BannerKings.Managers.PopulationManager;
using TaleWorlds.SaveSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;

namespace BannerKings.Components
{
    class RetinueComponent : PopulationPartyComponent
    {

        [SaveableProperty(1001)]
        public AiBehavior behavior { get; set; }

        public RetinueComponent(Settlement origin) : base(origin, origin, "", false, PopType.None)
        {
            this.behavior = AiBehavior.Hold;
        }

        private static MobileParty CreateParty(string id, Settlement origin)
        {
            return MobileParty.CreateParty(id + origin, new RetinueComponent(origin),
                delegate (MobileParty mobileParty)
            {
                mobileParty.SetPartyUsedByQuest(true);
                mobileParty.Party.Visuals.SetMapIconAsDirty();
                mobileParty.SetInititave(0.5f, 1f, float.MaxValue);
                mobileParty.ShouldJoinPlayerBattles = true;
                mobileParty.Aggressiveness = 0.1f;
                mobileParty.PaymentLimit = Campaign.Current.Models.PartyWageModel.MaxWage;
            });
        }

        public static MobileParty CreateRetinue(Settlement origin)
        {
            MobileParty retinue = CreateParty(string.Format("bk_retinue_{0}", origin.Name.ToString()), origin);
            retinue.InitializeMobilePartyAtPosition(origin.Culture.DefaultPartyTemplate, origin.GatePosition);
            EnterSettlementAction.ApplyForParty(retinue, origin);
            GiveMounts(ref retinue);
            GiveFood(ref retinue);
            BannerKingsConfig.Instance.PopulationManager.AddParty(retinue);
            return retinue;
        }

        public void DailyTick(float level)
        {
            MobileParty party = this.MobileParty;
            if (party.Food == 0f)
                GiveFood(ref party);

            int cap = (int)(level * 15f);
            if (party.MemberRoster.TotalManCount < cap)
            {
                var stacks = this.HomeSettlement.Culture.DefaultPartyTemplate.Stacks;
                CharacterObject character = stacks[MBRandom.RandomInt(0, stacks.Count - 1)].Character;
                this.Party.AddMember(character, 1);
            } else if (party.MemberRoster.TotalManCount < cap)
            {
                CharacterObject character = this.Party.MemberRoster.GetTroopRoster().GetRandomElement().Character;
                this.Party.MemberRoster.RemoveTroop(character, 1);
            }
        }

        public override Hero PartyOwner => HomeSettlement.OwnerClan.Leader;

        public override TextObject Name => new TextObject("Retinue from {SETTLEMENT}")
            .SetTextVariable("SETTLEMENT", HomeSettlement.Name);

        public override Settlement HomeSettlement => _target;
    }
}
