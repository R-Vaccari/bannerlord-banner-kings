using BannerKings.Behaviours;
using BannerKings.Managers.Helpers;
using BannerKings.Managers.Innovations;
using BannerKings.Managers.Court;
using BannerKings.Behaviours.Mercenary;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.CampaignBehaviors;

namespace BannerKings
{
    public static class BannerKingsCheats
    {
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


        [CommandLineFunctionality.CommandLineArgumentFunction("start_rebellion", "bannerkings")]
        public static string StartRebellionEvent(List<string> strings)
        {
            if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
            {
                return CampaignCheats.ErrorType;
            }

            if (CampaignCheats.CheckParameters(strings, 0))
            {
                return "Format is \"bannerkings.start_rebellion [Settlement]";
            }

            string id = strings.First();
            Settlement settlement = Settlement.All.FirstOrDefault(x => x.StringId == id || x.Name.ToString() == id);
            if (settlement == null)
            {
                return "No settlement found with this id or name.";
            }
            else
            {
                if (settlement.Town == null)
                {
                    return "Not a castle or fief.";
                }
                else TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<RebellionsCampaignBehavior>().StartRebellionEvent(settlement);
            }

            return "Title successfully inherited.";
        }


        [CommandLineFunctionality.CommandLineArgumentFunction("add_piety", "bannerkings")]
        public static string AddPiety(List<string> strings)
        {
            if (strings == null || strings.Count == 0)
            {
                return "Format is \"bannerkings.add_piety [Quantity\"]";
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

            MercenaryCareer career = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKMercenaryCareerBehavior>().GetCareer(Clan.PlayerClan);
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
            BKBanditBehavior behavior = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKBanditBehavior>();
            Clan clan = Clan.BanditFactions.GetRandomElementInefficiently();
            behavior.CreateBanditHero(clan);

            return $"Attempting to spawn hero for bandit faction {clan.Name}";
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("advance_era", "bannerkings")]
        public static string AdvanceEra(List<string> strings)
        {
            if (strings == null || strings.Count == 0)
            {
                return "Format is \"bannerkings.advance_era [Culture_id\"]";
            }

            CultureObject culture = MBObjectManager.Instance.GetObject<CultureObject>(strings[0]);
            if (culture == null)
            {
                return "Invalid culture id";
            }

            InnovationData data = BannerKingsConfig.Instance.InnovationsManager.GetInnovationData(culture);
            if (data == null)
            {
                return "Innovations dont exist for this culture";
            }

            data.SetEra(data.FindNextEra());

            return "Era advanced if available.";
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("make_alliance", "bannerkings")]
        public static string MakeAlliance(List<string> strings)
        {
            if (strings == null || strings.Count == 0)
            {
                return "Format is \"bannerkings.make_alliance [Kingdom_id\"]";
            }

            Kingdom kingdom = Kingdom.All.FirstOrDefault(x => x.StringId == strings[0]);
            if (kingdom == null)
            {
                return "Invalid kingdom id";
            }

            if (!Hero.MainHero.MapFaction.IsKingdomFaction)
            {
                return "Player not in a kingdom";
            }

            FactionManager.DeclareAlliance(Hero.MainHero.MapFaction, kingdom);
            return "Alliance set.";
        }
    }
}