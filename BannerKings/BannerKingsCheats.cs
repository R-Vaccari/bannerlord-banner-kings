using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Library;

namespace BannerKings
{
    public static class BannerKingsCheats
    {

		[CommandLineFunctionality.CommandLineArgumentFunction("destroy_parties", "bannerkings")]
		public static string GetLordsInsideSettlement(List<string> strings)
		{
			IEnumerable<MobileParty> parties = from party in MobileParty.All where party.StringId.Contains("raisedmilitia_") ||
											   party.StringId.Contains("slavecaravan_") || party.StringId.Contains("travellers_")
											   select party;
			List<MobileParty> list = new List<MobileParty>(parties);
			int count = 0;
			foreach (MobileParty party in list)
            {
				BannerKingsConfig.Instance.PopulationManager.RemoveCaravan(party);
				DestroyPartyAction.Apply(null, party);
				count++;
			}	
				
			return string.Format("{0} parties destroyed.", count);
		}
	}
}
