using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace BannerKings.Components
{
    public class BanditHeroComponent : BanditPartyComponent
    {
        private Hero leader;

        protected internal BanditHeroComponent(Hideout hideout, Hero leader) : base(hideout, false)
        {
            this.leader = leader;
        }

        public override void ChangePartyLeader(Hero newLeader)
        {
            base.ChangePartyLeader(newLeader);
            leader = newLeader;
        }

        public override Hero Leader => leader;

        public override Hero PartyOwner => leader;

        public override TextObject Name => new TextObject("{=!}Bandits of {HERO}")
            .SetTextVariable("HERO", leader.Name);

        public static MobileParty CreateParty(Hideout origin, Hero leader)
        {
            string id = "bkBanditParty_" + origin.Name + leader.Name;
            if (MobileParty.All.FirstOrDefault(x => x.StringId == id) != null)
            {
                return null;
            }

            leader.ChangeHeroGold(10000);
            var party = MobileParty.CreateParty(id,
                new BanditHeroComponent(origin, leader),
                delegate (MobileParty mobileParty)
                {
                    mobileParty.ActualClan = leader.Clan;  
                });

            party.InitializeMobilePartyAtPosition(leader.Clan.DefaultPartyTemplate, origin.Settlement.Position2D);
            return party;
        }
    }
}
