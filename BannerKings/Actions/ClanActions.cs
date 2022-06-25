using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Actions
{
    public static class ClanActions
    {

        public static Clan CreateNewClan(Hero hero, Settlement settlement, string id, TextObject name = null, float renown = 150f)
        {
            if (name == null) name = GetRandomName(hero.Culture, settlement);
            if (name == null || Clan.All.Any((Clan t) => t.Name.Equals(name) || t.Name.HasSameValue(name) ||
                t.Name.ToString().Trim() == name.ToString().Trim()) || Clan.PlayerClan.Name.Equals(name)) return null;

            Clan originalClan = hero.Clan;
            Clan clan = Clan.CreateClan(id);
            clan.InitializeClan(name, name, hero.Culture, Banner.CreateOneColoredBannerWithOneIcon(settlement.MapFaction.Banner.GetFirstIconColor(), settlement.MapFaction.Banner.GetPrimaryColor(),
                hero.Culture.PossibleClanBannerIconsIDs.GetRandomElement()), settlement.GatePosition, false);
            clan.AddRenown(renown);
            clan.SetLeader(hero);
            hero.AddPower(-hero.Power);

            JoinClan(hero, clan);
            if (hero.Spouse != null && !Utils.Helpers.IsClanLeader(hero.Spouse)) JoinClan(hero.Spouse, clan);

            if (hero.Children.Count > 0)
                foreach (Hero child in hero.Children)
                    if (child.IsChild) JoinClan(child, clan);

            if (originalClan != null)
            {
                if (originalClan.Kingdom != null) clan.Kingdom = originalClan.Kingdom;
            }

            return clan;
        }

        public static void JoinClan(Hero hero, Clan clan)
        {
            hero.Clan = null;
            hero.CompanionOf = null;
            hero.Clan = clan;
        }

        public static TextObject GetRandomName(CultureObject culture, Settlement settlement)
        {
            TextObject random = null;
            if (culture.ClanNameList.Count > 1)
                random = culture.ClanNameList.GetRandomElementInefficiently();
            else
            {
                random = culture.ClanNameList[0];
                random.SetTextVariable("ORIGIN_SETTLEMENT", settlement.Name);
            }
            return random;
        }
    }
}
