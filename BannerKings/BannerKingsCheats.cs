using BannerKings.Behaviors;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Library;

namespace BannerKings
{
    public static class BannerKingsCheats
    {

		[CommandLineFunctionality.CommandLineArgumentFunction("wipe_data", "bannerkings")]
		public static string WipeSaveData(List<string> strings)
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

			BannerKingsConfig.Instance.wipeData = true;
				
			return string.Format("{0} parties destroyed.", count);
		}

		[CommandLineFunctionality.CommandLineArgumentFunction("disable_knighthood", "bannerkings")]
		public static string DisableKnighthood(List<string> strings)
		{
			BannerKingsConfig.Instance.TitleManager.Knighthood = false;

			return "Knighthood requirements for player companions disabled.";
		}

		[CommandLineFunctionality.CommandLineArgumentFunction("enable_knighthood", "bannerkings")]
		public static string EnableKnighthood(List<string> strings)
		{
			BannerKingsConfig.Instance.TitleManager.Knighthood = true;

			return "Knighthood requirements for player companions enabled.";
		}

		[CommandLineFunctionality.CommandLineArgumentFunction("reinit_titles", "bannerkings")]
		public static string ReinitTitles(List<string> strings)
		{
			BannerKingsConfig.Instance.TitleManager.InitializeTitles();

			return "Successfully reinitted titles.";
		}
	}
}
