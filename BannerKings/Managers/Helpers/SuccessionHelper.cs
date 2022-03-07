using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.Core;
using TaleWorlds.Localization;
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
                else if (succession == SuccessionType.Imperial)
                    ApplyImperialSuccession(list, victim, kingdom);
            }
            else
               ApplyVanillaSuccession(list, victim, kingdom);
            
        }

        public static IEnumerable<SuccessionType> GetValidSuccessions(GovernmentType government)
        {
            if (government == GovernmentType.Feudal)
            {
                yield return SuccessionType.Hereditary_Monarchy;
                yield return SuccessionType.Elective_Monarchy;
                yield break;
            }
            else if (government == GovernmentType.Imperial)
            {
                yield return SuccessionType.Imperial;
                yield break;
            }
            else if (government == GovernmentType.Republic)
            {
                yield return SuccessionType.Republic;
                yield break;
            }
            else
            {
                yield return SuccessionType.Elective_Monarchy;
                yield break;
            }
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

        private static void ApplyRepublicSuccession(List<Clan> list, Hero victim, Kingdom kingdom)
        {
            kingdom.AddDecision(new RepublicElectionDecision(victim.Clan), true);
        }

        private static void ApplyImperialSuccession(List<Clan> list, Hero victim, Kingdom kingdom)
        {
            Hero heir = GetImperialHeir(list, victim);
            if (heir != null)
                Type.GetType("TaleWorlds.CampaignSystem.Actions.ChangeRulingClanAction, TaleWorlds.CampaignSystem")
                    .GetMethod("Apply", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { kingdom, heir.Clan });
            else ApplyVanillaSuccession(list, victim, kingdom);
        }

        public static Hero GetImperialHeir(List<Clan> list, Hero victim)
        {
            List<ValueTuple<Hero, float>> candidates = new List<ValueTuple<Hero, float>>();
            foreach (Clan clan in list)
            {

                if (clan != victim.Clan)
                    candidates.Add(new ValueTuple<Hero, float>(clan.Leader, CalculateHeirPreference(victim, clan.Leader)));
                else
                {
                   foreach (Hero familyMember in clan.Heroes)
                        if (!familyMember.IsChild && familyMember != victim && familyMember.IsAlive && (familyMember.IsNoble || familyMember.IsMinorFactionHero))
                            candidates.Add(new ValueTuple<Hero, float>(clan.Leader, CalculateHeirPreference(victim, clan.Leader)));
                }
            }

            Hero heir = MBRandom.ChooseWeighted(candidates);
            if (heir != null)
            {
                if (heir.Clan == victim.Clan)
                    InheritanceHelper.ApplyImperialInheritance(victim, heir);

                if (Clan.PlayerClan.Kingdom != null && Clan.PlayerClan.Kingdom == victim.Clan.Kingdom)
                    InformationManager.AddQuickInformation(new TextObject(heir.Name.ToString() + " was appointed Emperor."));
            }
            return heir;
        }

        private static float CalculateHeirPreference(Hero victim, Hero candidate)
        {
            float relation = victim.GetRelation(candidate);
            float age = candidate.Age / 2f;
            float skills = 0f;
            skills += candidate.GetSkillValue(DefaultSkills.Leadership);
            skills += candidate.GetSkillValue(DefaultSkills.Tactics);
            skills += candidate.GetSkillValue(DefaultSkills.Steward);
            skills += candidate.GetSkillValue(DefaultSkills.Charm);
            skills *= 0.1f;

            return (relation + age + skills) / 3f * (float)Math.Sqrt(candidate.Clan.Tier);
        }
    }
}
