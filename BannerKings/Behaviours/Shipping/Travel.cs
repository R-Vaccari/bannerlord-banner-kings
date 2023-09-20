using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.SaveSystem;

namespace BannerKings.Behaviours.Shipping
{
    public class Travel
    {
        public Travel(MobileParty party, CampaignTime arrival, Settlement destination)
        {
            Party = party;
            Arrival = arrival;
            Destination = destination;
        }

        [SaveableProperty(1)] public MobileParty Party { get; private set; }
        [SaveableProperty(2)] public CampaignTime Arrival { get; private set; }
        [SaveableProperty(3)] public Settlement Destination { get; private set; }
    }
}
