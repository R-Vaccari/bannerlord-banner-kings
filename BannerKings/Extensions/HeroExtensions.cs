using BannerKings.Extensions;
using BannerKings.Managers.Titles;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings.Utils.Extensions
{
    public static class HeroExtensions
    {
        public static bool IsPlayer(this Hero hero)
        {
            return hero.StringId == Hero.MainHero.StringId;
        }

        public static bool IsClanLeader(this Hero hero)
        {
            return hero.Clan?.Leader == hero;
        }

        public static bool IsKingdomLeader(this Hero hero)
        {
            return hero.Clan?.Kingdom?.Leader == hero;
        }

        public static bool IsCommonBorn(this Hero hero) => hero.CharacterObject != null && hero.CharacterObject.OriginalCharacter != null
            && hero.CharacterObject.OriginalCharacter.IsTemplate;

        public static List<Village> GetVillages(this Hero hero)
        {
            var list = new List<Village>();
            var lordships = BannerKingsConfig.Instance.TitleManager
                        .GetAllDeJure(hero)
                        .FindAll(x => x.type == TitleType.Lordship);
            foreach (var lordship in lordships)
            {
                if (lordship.fief.MapFaction == hero.MapFaction)
                {
                    list.Add(lordship.fief.Village);
                }
            }

            if (hero.IsClanLeader())
            {
                foreach (var fief in hero.Clan.Fiefs)
                {
                    foreach (var village in fief.Villages)
                    {
                        if (!list.Contains(village) && village.GetActualOwner() == hero)
                        {
                            list.Add(village);
                        }
                    }
                }
            }
            
            return list;
        }
    }
}