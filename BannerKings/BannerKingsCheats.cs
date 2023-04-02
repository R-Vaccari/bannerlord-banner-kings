using BannerKings.Behaviours;
using BannerKings.Managers.Court;
using BannerKings.Behaviours.Mercenary;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace BannerKings
{
    public static class BannerKingsCheats
    {
        [CommandLineFunctionality.CommandLineArgumentFunction("wipe_data", "bannerkings")]
        public static string WipeSaveData(List<string> strings)
        {
            var parties = from party in MobileParty.All
                where party.StringId.Contains("raisedmilitia_") ||
                      party.StringId.Contains("slavecaravan_") || party.StringId.Contains("travellers_")
                select party;
            var list = new List<MobileParty>(parties);
            var count = 0;
            foreach (var party in list)
            {
                BannerKingsConfig.Instance.PopulationManager.RemoveCaravan(party);
                DestroyPartyAction.Apply(null, party);
                count++;
            }

            var civillians = MBObjectManager.Instance.GetObjectTypeList<CharacterObject>().ToList()
                .FindAll(x => x.StringId.Contains("craftsman_") || x.StringId is "noble_empire" or "noble_vlandia" or "noble_sturgia" or "noble_aserai" or "noble_khuzait" or "noble_battania");

            foreach (var party in MobileParty.All)
            {
                foreach (var civillian in civillians)
                {
                    if (party.MemberRoster != null && party.MemberRoster.Contains(civillian))
                    {
                        var memberCount = party.MemberRoster.GetTroopCount(civillian);
                        party.MemberRoster.RemoveTroop(civillian, memberCount);
                    }

                    if (party.PrisonRoster != null && party.PrisonRoster.Contains(civillian))
                    {
                        var memberCount = party.PrisonRoster.GetTroopCount(civillian);
                        party.PrisonRoster.RemoveTroop(civillian, memberCount);
                    }
                }
            }

            BannerKingsConfig.Instance.wipeData = true;
            return $"{count} parties destroyed.";
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("give_title", "bannerkings")]
        public static string GiveTitle(List<string> strings)
        {
            if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
            {
                return CampaignCheats.ErrorType;
            }

            if (CampaignCheats.CheckParameters(strings, 0) || CampaignCheats.CheckParameters(strings, 1))
            {
                return "Format is \"bannerkings.give_title [TitleName] | [PersonName]";
            }

            var array = CampaignCheats.ConcatenateString(strings).Split('|');

            if (array.Length != 2)
            {
                return "Format is \"bannerkings.give_title [TitleName] | [PersonName]";
            }


            var title = BannerKingsConfig.Instance.TitleManager.GetTitleByName(array[0].Trim());
            if (title == null)
            {
                return $"No title found with name {array[0]}";
            }

            var hero = Hero.AllAliveHeroes.FirstOrDefault(x => x.Name != null && x.Name.ToString() == array[1].Trim());
            if (hero == null)
            {
                return $"No hero found with name {array[1]}";
            }

            BannerKingsConfig.Instance.TitleManager.InheritTitle(title.deJure, hero, title);
            return "Title successfully inherited.";
        }


        [CommandLineFunctionality.CommandLineArgumentFunction("add_piety", "bannerkings")]
        public static string AddPiety(List<string> strings)
        {
            if (strings == null || strings.Count == 0)
            {
                return "Format is \"bannerkings.add_piety [Quantity]";
            }

            if (float.TryParse(strings[0], out var piety))
            {
                BannerKingsConfig.Instance.ReligionsManager.AddPiety(Hero.MainHero, piety);
            }
            else
            {
                return $"{strings[0]} is not a number.";
            }

            return $"{piety} piety added to Main player.";
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("add_career_points", "bannerkings")]
        public static string AddCareer(List<string> strings)
        {

            MercenaryCareer career = Campaign.Current.GetCampaignBehavior<BKMercenaryCareerBehavior>().GetCareer(Clan.PlayerClan);
            if (career != null)
            {
                career.AddPoints();
                return "Career points added!";
            }

            return "No mercenary career found.";
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("give_player_full_peerage", "bannerkings")]
        public static string GrantPeerage(List<string> strings)
        {
            var council = BannerKingsConfig.Instance.CourtManager.GetCouncil(Clan.PlayerClan);
            council.SetPeerage(new Peerage(new TextObject("{=9OhMK2Wk}Full Peerage"), true,
                                true, true, true, true, false));

            return "Full Peerage set.";
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("spawn_bandit_hero", "bannerkings")]
        public static string SpawnBanditHero(List<string> strings)
        {
            BKBanditBehavior behavior = Campaign.Current.GetCampaignBehavior<BKBanditBehavior>();
            Clan clan = Clan.BanditFactions.GetRandomElementInefficiently();
            behavior.CreateBanditHero(clan);

            return $"Attempting to spawn hero for bandit faction {clan.Name}";
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