using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BannerKings.Managers.Kingdoms.Succession;
using BannerKings.Managers.Titles;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Helpers
{
    public static class SuccessionHelper
    {
        public static void ApplySovereignSuccession(FeudalTitle title, Hero victim, Kingdom kingdom)
        {
            if (title.Sovereign != null)
            {
                return;
            }

            var succession = title.Contract.Succession;
            if (succession != SuccessionType.Hereditary_Monarchy && succession != SuccessionType.Imperial)
            {
                if (!kingdom.IsEliminated)
                {
                    if (succession == SuccessionType.FeudalElective)
                    {
                        ApplyFeudalElective(title, victim, kingdom);
                    }
                }
                
                return;
            }

            Hero heir = null;
            float heirScore = float.MinValue;
            var line = BannerKingsConfig.Instance.TitleModel.CalculateSuccessionLine(title, victim.Clan, victim);
            heir = line.First().Key;

            if (heir != null)
            {
                BannerKingsConfig.Instance.TitleManager.InheritTitle(title.deJure, heir, title);
                Type.GetType("TaleWorlds.CampaignSystem.Actions.ChangeRulingClanAction, TaleWorlds.CampaignSystem")
                    .GetMethod("Apply", BindingFlags.Public | BindingFlags.Static)
                    .Invoke(null, new object[] { kingdom, heir.Clan });

                var decision = kingdom.UnresolvedDecisions.FirstOrDefault(x => x is KingSelectionKingdomDecision);
                if (decision != null)
                {
                    kingdom.RemoveDecision(decision);
                }

                if (Clan.PlayerClan.Kingdom != null && Clan.PlayerClan.Kingdom == victim.Clan.Kingdom)
                {
                    MBInformationManager.AddQuickInformation(
                        new TextObject("{=ytkncUx3}{HEIR} has rightfully inherited the {TITLE}")
                            .SetTextVariable("HEIR", heir.Name)
                            .SetTextVariable("TITLE", title.FullName), 
                        0, 
                        heir.CharacterObject,
                        Utils.Helpers.GetKingdomDecisionSound());
                }
            }
        }

        private static void ApplyFeudalElective(FeudalTitle title, Hero victim, Kingdom kingdom)
        {
            var decision = kingdom.UnresolvedDecisions.FirstOrDefault(x => x is KingSelectionKingdomDecision);
            if (decision != null)
            {
                kingdom.UnresolvedDecisions.Remove(decision);
            }

            var electiveDecision = new FeudalElectiveDecision(victim.Clan, title);
            kingdom.UnresolvedDecisions.Add(electiveDecision);
            if (!electiveDecision.IsAllowed())
            {
                ResolveSuccession(title, victim, kingdom);
            }
        }

        private static void ResolveSuccession(FeudalTitle title, Hero victim, Kingdom kingdom)
        {
            Hero heir = victim.Clan.Leader;
            if (!victim.Clan.IsEliminated && heir != victim && heir.IsAlive)
            {
                BannerKingsConfig.Instance.TitleManager.InheritTitle(victim, heir, title);
                if (kingdom == Clan.PlayerClan.MapFaction)
                {
                    InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=okUMw3Df}Due to abscence of other candidates, {HEIR} has inherited the {TITLE} uncontested.")
                        .SetTextVariable("HEIR", heir.Name)
                        .SetTextVariable("TITLE", title.FullName)
                        .ToString(),
                        Utils.Helpers.GetKingdomDecisionSound()));
                }
            }
            else
            {
                kingdom.AddDecision(new KingSelectionKingdomDecision(victim.Clan));
                if (kingdom == Clan.PlayerClan.MapFaction)
                {
                    InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=vWDgQWUh}Due to abscence of {CLAN} candidates, the {TITLE} succession will be voted on by the Peers of the realm.")
                        .SetTextVariable("TITLE", title.FullName)
                        .SetTextVariable("CLAN", victim.Clan.Name)
                        .ToString(),
                        Utils.Helpers.GetKingdomDecisionSound()));
                }
            }
        }

        public static IEnumerable<SuccessionType> GetValidSuccessions(GovernmentType government)
        {
            switch (government)
            {
                case GovernmentType.Feudal:
                    yield return SuccessionType.Hereditary_Monarchy;
                    yield return SuccessionType.Elective_Monarchy;
                    yield return SuccessionType.FeudalElective;
                    yield break;
                case GovernmentType.Imperial:
                    yield return SuccessionType.Imperial;
                    yield break;
                case GovernmentType.Republic:
                    yield return SuccessionType.Republic;
                    yield break;
                default:
                    yield return SuccessionType.Elective_Monarchy;
                    yield return SuccessionType.Hereditary_Monarchy;
                    break;
            }
        }
    }
}