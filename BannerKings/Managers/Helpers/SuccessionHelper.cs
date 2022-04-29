﻿using BannerKings.Managers.Kingdoms;
using BannerKings.Managers.Titles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Helpers
{
    public static class SuccessionHelper
    {

        public static Hero ApplySuccession(FeudalTitle title, Hero victim, Kingdom kingdom, bool applyEffects = true)
        {
            List<Clan> list = new List<Clan>();
            if (victim.Clan.Kingdom != null)
                list = (from t in victim.Clan.Kingdom.Clans
                        where !t.IsEliminated && !t.IsUnderMercenaryService
                        select t).ToList<Clan>();

            Hero heir = null;
            if (title != null && !list.IsEmpty() && title.sovereign == null)
            {
                SuccessionType succession = title.contract.Succession;
                if (succession == SuccessionType.Hereditary_Monarchy)
                    heir = ApplyHereditarySuccession(list, victim, kingdom);
                else if (succession == SuccessionType.Imperial)
                    heir = ApplyImperialSuccession(list, victim, kingdom);

                if (heir != null && applyEffects)
                {
                    BannerKingsConfig.Instance.TitleManager.InheritTitle(title.deJure, heir, title);
                    Type.GetType("TaleWorlds.CampaignSystem.Actions.ChangeRulingClanAction, TaleWorlds.CampaignSystem")
                        .GetMethod("Apply", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { kingdom, heir.Clan });

                    KingdomDecision decision = kingdom.UnresolvedDecisions.FirstOrDefault(x => x is KingSelectionKingdomDecision);
                    if (decision != null)
                        kingdom.RemoveDecision(decision);

                    if (Clan.PlayerClan.Kingdom != null && Clan.PlayerClan.Kingdom == victim.Clan.Kingdom)
                        InformationManager.AddQuickInformation(new TextObject("{=!}{HEIR} has rightfully inherited the {TITLE}")
                            .SetTextVariable("HEIR", heir.Name)
                            .SetTextVariable("TITLE", title.FullName), 0, heir.CharacterObject);
                    
                }
            }
            return heir;
        }

        public static IEnumerable<SuccessionType> GetValidSuccessions(GovernmentType government)
        {
            if (government == GovernmentType.Feudal)
            {
                yield return SuccessionType.Hereditary_Monarchy;
                yield return SuccessionType.Elective_Monarchy;
                yield break;
            }

            if (government == GovernmentType.Imperial)
            {
                yield return SuccessionType.Imperial;
                yield break;
            }

            if (government == GovernmentType.Republic)
            {
                yield return SuccessionType.Republic;
                yield break;
            }

            yield return SuccessionType.Elective_Monarchy;
            yield return SuccessionType.Hereditary_Monarchy;
        }

        private static void ApplyVanillaSuccession(List<Clan> list, Hero victim, Kingdom kingdom)
        {
            if (list.Count > 1)
            {
                Clan clanToExclude = (victim.Clan.Leader == victim || victim.Clan.Leader == null) ? victim.Clan : null;
                kingdom.AddDecision(new BKKingElectionDecision(victim.Clan, clanToExclude), true);
            }
            else if (list.Count == 1)
                Type.GetType("TaleWorlds.CampaignSystem.Actions.ChangeRulingClanAction, TaleWorlds.CampaignSystem")
                    .GetMethod("Apply", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { kingdom, list.First() });
        }

        private static Hero ApplyHereditarySuccession(List<Clan> list, Hero victim, Kingdom kingdom)
        {
            return victim.Clan.Leader;
        }

        private static Hero ApplyImperialSuccession(List<Clan> list, Hero victim, Kingdom kingdom)
        {
            return GetImperialHeir(list, victim);
        }

        public static Hero GetImperialHeir(List<Clan> list, Hero victim, bool includeLeader = false)
        {
            List<ValueTuple<Hero, float>> candidates = new List<ValueTuple<Hero, float>>();
            foreach (Clan clan in list)
            {

                if (clan != victim.Clan)
                    candidates.Add(new ValueTuple<Hero, float>(clan.Leader, CalculateHeirPreference(victim, clan.Leader)));
                else foreach (Hero familyMember in clan.Heroes)
                        if (!familyMember.IsChild && familyMember.IsAlive && (familyMember.IsNoble || familyMember.IsMinorFactionHero))
                        {
                            if (familyMember == victim && !includeLeader) continue;
                            candidates.Add(new ValueTuple<Hero, float>(clan.Leader, CalculateHeirPreference(victim, clan.Leader)));
                        }   
            }
            return MBRandom.ChooseWeighted(candidates);
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
