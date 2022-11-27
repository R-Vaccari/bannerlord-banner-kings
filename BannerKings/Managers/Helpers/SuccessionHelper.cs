using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BannerKings.Managers.Titles;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Helpers
{
    public static class SuccessionHelper
    {
        public static void ApplySovereignSuccession(FeudalTitle title, Hero victim, Kingdom kingdom)
        {
            if (title.sovereign != null)
            {
                return;
            }

            var succession = title.contract.Succession;
            if (succession != SuccessionType.Hereditary_Monarchy && succession != SuccessionType.Imperial)
            {
                return;
            }

            Hero heir = null;
            float heirScore = float.MinValue;
            var line = BannerKingsConfig.Instance.TitleModel.CalculateSuccessionLine(title.contract, victim.Clan, victim);
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
                            .SetTextVariable("TITLE", title.FullName), 0, heir.CharacterObject);
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