using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Titles;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

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

			List<CharacterObject> civillians = MBObjectManager.Instance.GetObjectTypeList<CharacterObject>().ToList()
				.FindAll(x => x.StringId.Contains("craftsman_") || x.StringId == "noble_empire" || x.StringId == "noble_vlandia" ||
				x.StringId == "noble_sturgia" || x.StringId == "noble_aserai" || x.StringId == "noble_khuzait" || x.StringId == "noble_battania");

			foreach (MobileParty party in MobileParty.All)
			{
				foreach (CharacterObject civillian in civillians)
				{
					if (party.MemberRoster != null && party.MemberRoster.Contains(civillian))
					{
						int memberCount = party.MemberRoster.GetTroopCount(civillian);
						party.MemberRoster.RemoveTroop(civillian, memberCount);
					}

					if (party.PrisonRoster != null && party.PrisonRoster.Contains(civillian))
					{
						int memberCount = party.PrisonRoster.GetTroopCount(civillian);
						party.PrisonRoster.RemoveTroop(civillian, memberCount);
					}
				}
			}

			BannerKingsConfig.Instance.wipeData = true;	
			return string.Format("{0} parties destroyed.", count);
		}

		[CommandLineFunctionality.CommandLineArgumentFunction("give_title", "bannerkings")]
		public static string GiveTitle(List<string> strings)
		{
			if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
				return CampaignCheats.ErrorType;

			if (CampaignCheats.CheckParameters(strings, 0) || CampaignCheats.CheckParameters(strings, 1))
				return "Format is \"bannerkings.give_title [TitleName] | [PersonName]";

			string[] array = CampaignCheats.ConcatenateString(strings).Split('|');

			if (array.Length != 2)
				return "Format is \"bannerkings.give_title [TitleName] | [PersonName]";
			

			FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetTitleByName(array[0].Trim());
			if (title == null) return string.Format("No title found with name {0}", array[0]);

			Hero hero = Hero.AllAliveHeroes.FirstOrDefault(x => x.Name != null && x.Name.ToString() == array[1].Trim());
			if (hero == null) return string.Format("No hero found with name {0}", array[1]);

			BannerKingsConfig.Instance.TitleManager.InheritTitle(title.deJure, hero, title);
			return "Title successfully inherited.";
		}


		[CommandLineFunctionality.CommandLineArgumentFunction("add_piety", "bannerkings")]
		public static string AddPiety(List<string> strings)
		{
			float piety;
			if (float.TryParse(strings[0], out piety)) BannerKingsConfig.Instance.ReligionsManager.AddPiety(Hero.MainHero, piety);
			else return string.Format("{0} is not a number.", strings[0]);

			return string.Format("{0} piety added to Main player.", piety);
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
