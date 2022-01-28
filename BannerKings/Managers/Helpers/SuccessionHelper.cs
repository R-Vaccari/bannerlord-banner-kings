
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Election;
using static BannerKings.Managers.TitleManager;

namespace BannerKings.Managers.Helpers
{
    public static class SuccessionHelper
    {

        public static void ApplySuccession(FeudalTitle title, List<Clan> list, Hero victim, Kingdom kingdom)
        {
            if (title != null)
            {
                SuccessionType succession = title.contract.succession;
                if (succession == SuccessionType.Elective_Monarchy || succession == SuccessionType.Republic)
                    ApplyVanillaSuccession(list, victim, kingdom);
                else if (succession == SuccessionType.Hereditary_Monarchy)
                    ApplyHereditarySuccession(list, victim, kingdom);
            }
            else
               ApplyVanillaSuccession(list, victim, kingdom);
            
        }

        private static void ApplyVanillaSuccession(List<Clan> list, Hero victim, Kingdom kingdom)
        {
            if (list.Count > 1)
            {
                Clan clanToExclude = (victim.Clan.Leader == victim || victim.Clan.Leader == null) ? victim.Clan : null;
                kingdom.AddDecision(new KingSelectionKingdomDecision(victim.Clan, clanToExclude), true);
            }
            else if (list.Count == 1)
                Type.GetType("TaleWorlds.CampaignSystem.Actions.ChangeRulingClanAction, TaleWorlds.CampaignSystem")
                    .GetMethod("Apply", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { kingdom, list.First<Clan>() });
        }

        private static void ApplyHereditarySuccession(List<Clan> list, Hero victim, Kingdom kingdom)
        {
            Clan clan = victim.Clan;
            if (clan.Heroes.Any((Hero x) => !x.IsChild && x != victim && x.IsAlive && (x.IsNoble || x.IsMinorFactionHero)))
                Type.GetType("TaleWorlds.CampaignSystem.Actions.ChangeRulingClanAction, TaleWorlds.CampaignSystem")
                    .GetMethod("Apply", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { kingdom, clan });
            else ApplyVanillaSuccession(list, victim, kingdom);
        }

        public static void GetImperialHeir(List<Clan> list, Hero victim)
        {
            List<ValueTuple<Hero, int>> candidates = new List<ValueTuple<Hero, int>>();
            foreach (Clan clan in list)
            {
                Hero leader = clan.Leader;
                if (clan != victim.Clan && leader != null)
                    candidates.Add(new ValueTuple<Hero, int>(leader, victim.GetRelation(leader)));
                else
                {
                    (from h in clan.Heroes
                     where h.IsActive && h.IsAlive && !h.IsChild && h.IsNoble
                     select h).ToList<Hero>();
                }
            }
        }
    }
}
