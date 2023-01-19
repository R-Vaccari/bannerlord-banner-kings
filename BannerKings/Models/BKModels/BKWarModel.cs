﻿using BannerKings.Behaviours.Diplomacy.Wars;
using BannerKings.Utils.Extensions;
using Helpers;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace BannerKings.Models.BKModels
{
    public class BKWarModel
    {
        public ExplainedNumber CalculateTotalWarScore(War war, bool explanations = false)
        {
            var result = new ExplainedNumber(0f, explanations);

            CasusBelli justification = war.CasusBelli;
            if (justification.Fief != null)
            {
                result.Add(CalculateFiefScore(justification.Fief).ResultNumber * 2f * justification.ConquestWeight);
            }

            return result;
        }

        public ExplainedNumber CalculateWarScore(War war, IFaction attacker, IFaction defender, bool isDefenderScore = false, 
            bool explanations = false)
        {
            var result = new ExplainedNumber(0f, explanations);
            StanceLink attackerLink = attacker.GetStanceWith(defender);
            CasusBelli justification = war.CasusBelli;
            float totalWarScore = CalculateTotalWarScore(war).ResultNumber;

            List<Settlement> attackerConquests = DiplomacyHelper.GetSuccessfullSiegesInWarForFaction(attacker,
               attackerLink, (Settlement x) => x.Town != null);
            foreach (var settlement in attackerConquests)
            {
                if (settlement.MapFaction == attacker)
                {
                    float points = CalculateFiefScore(settlement).ResultNumber * justification.ConquestWeight;
                    result.Add(points / totalWarScore, settlement.Name);
                }
            }

            List<Settlement> attackerRaids = DiplomacyHelper.GetRaidsInWar(attacker, attackerLink, null);
            foreach (var settlement in attackerRaids)
            {
                result.Add(CalculateFiefScore(settlement).ResultNumber / 3 * justification.ConquestWeight,
                    settlement.Name);
            }

            List<Hero> attackerCaptives = DiplomacyHelper.GetPrisonersOfWarTakenByFaction(attacker, defender);
            Hero defenderHeir = null;
            if (defender.IsKingdomFaction)
            {
                var kingdomTitle = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(defender as Kingdom);
                if (kingdomTitle != null)
                {
                    IEnumerable<KeyValuePair<Hero, ExplainedNumber>> heirs = BannerKingsConfig.Instance.TitleModel
                        .CalculateSuccessionLine(kingdomTitle.contract, defender.Leader.Clan);
                    if (heirs.Count() > 0)
                    {
                        defenderHeir = heirs.First().Key;
                    }
                }
            }
           
            foreach (var hero in attackerCaptives)
            {
                result.Add(CalculateHeroScore(hero, totalWarScore, hero == defenderHeir).ResultNumber * justification.CaptureWeight, 
                    hero.Name);
            }

            // --- DEFENDER ----

            List<Settlement> defenderConquests = DiplomacyHelper.GetSuccessfullSiegesInWarForFaction(defender,
                attackerLink, (Settlement x) => x.Town != null);
            foreach (var settlement in defenderConquests)
            {
                if (settlement.MapFaction == defender && settlement != justification.Fief)
                {
                    float points = -CalculateFiefScore(settlement).ResultNumber * justification.ConquestWeight;
                    result.Add(totalWarScore / points,
                        settlement.Name);
                }
            }

            List<Settlement> defenderRaids = DiplomacyHelper.GetRaidsInWar(defender, attackerLink, null);
            foreach (var settlement in defenderRaids)
            {
                result.Add(-CalculateFiefScore(settlement).ResultNumber / 3 * justification.RaidWeight,
                    settlement.Name);
            }

            List<Hero> defenderCaptives = DiplomacyHelper.GetPrisonersOfWarTakenByFaction(defender, attacker);
            Hero attackerHeir = null;
            if (attacker.IsKingdomFaction)
            {
                var kingdomTitle = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(attacker as Kingdom);
                if (kingdomTitle != null)
                {
                    IEnumerable<KeyValuePair<Hero, ExplainedNumber>> heirs = BannerKingsConfig.Instance.TitleModel
                        .CalculateSuccessionLine(kingdomTitle.contract, attacker.Leader.Clan);
                    if (heirs.Count() > 0)
                    {
                        attackerHeir = heirs.First().Key;
                    }
                }
            }

            foreach (var hero in defenderCaptives)
            {
                result.Add(-CalculateHeroScore(hero, totalWarScore, hero == attackerHeir).ResultNumber, hero.Name);
            }

            if (isDefenderScore)
            {
                result.AddFactor(-1f);
            }

            return result;
        }

        public ExplainedNumber CalculateHeroScore(Hero hero, float totalWarScore, bool isKingdomHeir = false, bool explanations = false)
        {
            var result = new ExplainedNumber(0f, explanations);

            if (hero.Clan != null)
            {
                var kingdom = hero.Clan.Kingdom;
                if (kingdom != null)
                {
                    if (isKingdomHeir)
                    {
                        result.Add(totalWarScore * 0.33f);
                    }
                    else if (kingdom.Leader == hero)
                    {
                        result.Add(totalWarScore * 0.5f);
                    }
                }

                float renown = hero.Clan.Renown;
                result.Add(renown);
                if (hero.IsClanLeader())
                {
                    result.Add(renown);
                }
                else if (hero.Clan.Leader.Spouse == hero)
                {
                    result.Add(renown * 0.5f);
                }
            }

            return result;
        }

        public ExplainedNumber CalculateFiefScore(Settlement settlement, bool isOriginalFront = false, bool explanations = false)
        {
            var result = new ExplainedNumber(0f, explanations);

            IFaction ownerFaction = settlement.MapFaction;
            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
            if (data != null)
            {
                result.Add(data.TotalPop / 2f, new TextObject("{=!}Population of {TOWN}")
                        .SetTextVariable("VILLAGE", settlement.Name));

                if (data.ReligionData != null && data.ReligionData.DominantReligion != null)
                {
                    var religion = data.ReligionData.DominantReligion;
                    if (ownerFaction.Leader != null && religion != null)
                    {
                        var leaderReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(ownerFaction.Leader);
                        if (leaderReligion != null && leaderReligion.Faith.GetId() == religion.Faith.GetId())
                        {
                            result.AddFactor(0.25f, new TextObject("{=!}{TOWN} has same faith as it's realm")
                                .SetTextVariable("TOWN", settlement.Name));
                        }
                    }
                }
            }

            result.Add(settlement.Prosperity / 3f, new TextObject("{=!}Prosperity of {TOWN}")
                        .SetTextVariable("VILLAGE", settlement.Name));
            if (settlement.BoundVillages != null)
            {
                foreach (Village village in settlement.BoundVillages)
                {
                    result.Add(village.Hearth, new TextObject("{=!}Hearths in {VILLAGE}")
                        .SetTextVariable("VILLAGE", village.Name));
                }
            }
           
            if (settlement.Culture == ownerFaction.Culture)
            {
                result.AddFactor(0.25f, new TextObject("{=!}{TOWN} has same culture as it's realm")
                    .SetTextVariable("TOWN", settlement.Name));
            }

            if (isOriginalFront)
            {
                result.AddFactor(0.1f, new TextObject("{=!}{TOWN} was a front on war declaration")
                    .SetTextVariable("TOWN", settlement.Name));
            }

            return result;
        }
    }
}
