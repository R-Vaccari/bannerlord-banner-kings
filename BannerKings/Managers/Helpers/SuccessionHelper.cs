using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BannerKings.Managers.Kingdoms.Succession;
using BannerKings.Managers.Titles;
using BannerKings.Managers.Titles.Governments;
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

            Succession succession = title.Contract.Succession;  
            if (succession.ElectedSuccession)
            {
                BKKingElectionDecision decision = new BKKingElectionDecision(victim.Clan, title, victim);
                var vanillaDecision = kingdom.UnresolvedDecisions.FirstOrDefault(x => x is KingSelectionKingdomDecision);
                if (vanillaDecision != null)
                {
                    kingdom.RemoveDecision(vanillaDecision);
                }

                kingdom.UnresolvedDecisions.Add(decision);
                if (!decision.IsAllowed())
                {
                    ResolveSuccession(title, victim, kingdom);
                }
            }
            else
            {
                Hero heir = null;
                var line = BannerKingsConfig.Instance.TitleModel.CalculateSuccessionLine(title, victim.Clan, victim);
                heir = line.First().Key;

                if (heir != null)
                {
                    BannerKingsConfig.Instance.TitleManager.InheritTitle(title.deJure, heir, title);
                    Type.GetType("TaleWorlds.CampaignSystem.Actions.ChangeRulingClanAction, TaleWorlds.CampaignSystem")
                        .GetMethod("Apply", BindingFlags.Public | BindingFlags.Static)
                        .Invoke(null, new object[] { kingdom, heir.Clan });

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