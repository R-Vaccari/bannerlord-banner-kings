using BannerKings.Behaviours.Diplomacy.Wars;
using BannerKings.Managers.Goals;
using BannerKings.Utils.Extensions;
using BannerKings.Utils.Models;
using Helpers;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Models.BKModels
{
    public class BKWarModel
    {
        private const float TARGET_FIEF_MULTIPLIER = 5f;

        private float GetTotalManpower(List<Settlement> settlements)
        {
            int limit = 0;
            foreach (Settlement settlement in settlements)
            {
                limit += GetTotalManpower(settlement);
            }

            if (limit == 0)
            {
                limit += 5000;
                limit += settlements.Count * 3000;
            }

            return limit;
        }

        public ExplainedNumber CalculateTotalWarScore(War war, bool explanations = false)
        {
            var result = new ExplainedNumber(0f, explanations);
            CasusBelli justification = war.CasusBelli;
            
            foreach (Settlement settlement in war.Defender.Settlements)
            {
                float score = CalculateFiefScore(settlement).ResultNumber;
                if (settlement == justification.Fief)
                {
                    score *= TARGET_FIEF_MULTIPLIER;
                }

                result.Add(score * justification.ConquestWeight);
            }

            if (war.Defender.IsKingdomFaction)
            {
                result.Add(GetTotalManpower(war.Defender.Settlements));

                foreach (Clan clan in (war.Defender as Kingdom).Clans)
                    foreach (Hero hero in clan.Heroes)
                    {
                        float points = CalculateHeroScore(hero, 0f, false).ResultNumber 
                            * justification.CaptureWeight;
                        result.Add(points, hero.Name);
                    }
            }

            return result;
        }

        public BKExplainedNumber CalculateWarScore(War war, IFaction attacker, IFaction defender, bool isDefenderScore = false, 
            bool explanations = false)
        {
            var result = new BKExplainedNumber(0f, explanations);
            result.LimitMin(-1f);
            result.LimitMax(1f);

            StanceLink attackerLink = attacker.GetStanceWith(defender);
            CasusBelli justification = war.CasusBelli;
            float totalWarScore = CalculateTotalWarScore(war).ResultNumber;

            int defenderCasualties = attackerLink.GetCasualties(defender);
            result.Add(defenderCasualties / totalWarScore,
                new TextObject("{=!}{FACTION} suffered {CASUALTIES} casualties")
                .SetTextVariable("FACTION", defender.Name)
                .SetTextVariable("CASUALTIES", defenderCasualties));

            int attackerCasualties = attackerLink.GetCasualties(attacker);
            result.Add(-attackerCasualties / totalWarScore,
                new TextObject("{=!}{FACTION} suffered {CASUALTIES} casualties")
                .SetTextVariable("FACTION", attacker.Name)
                .SetTextVariable("CASUALTIES", attackerCasualties));

            List<Settlement> attackerConquests = DiplomacyHelper.GetSuccessfullSiegesInWarForFaction(attacker,
               attackerLink, (Settlement x) => x.Town != null);
            foreach (var settlement in attackerConquests)
            {
                if (settlement.MapFaction == attacker)
                {
                    float points = CalculateFiefScore(settlement).ResultNumber * justification.ConquestWeight;
                    if (settlement == war.CasusBelli.Fief)
                    {
                        points *= 0f;
                    }

                    result.Add(points / totalWarScore, settlement.Name);
                }
            }

            List<Settlement> attackerRaids = DiplomacyHelper.GetRaidsInWar(attacker, attackerLink, null);
            foreach (var settlement in attackerRaids)
            {
                float points = CalculateRaidScore(settlement.Village).ResultNumber * justification.RaidWeight;
                result.Add(points / totalWarScore, new TextObject("{=!}Raid of {VILLAGE}")
                    .SetTextVariable("VILLAGE", settlement.Name));
            }

            List<Hero> attackerCaptives = DiplomacyHelper.GetPrisonersOfWarTakenByFaction(attacker, defender);
            Hero defenderHeir = null;
            if (defender.IsKingdomFaction)
            {
                var kingdomTitle = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(defender as Kingdom);
                if (kingdomTitle != null)
                {
                    IEnumerable<KeyValuePair<Hero, ExplainedNumber>> heirs = BannerKingsConfig.Instance.TitleModel
                        .CalculateSuccessionLine(kingdomTitle, defender.Leader.Clan);
                    if (heirs.Count() > 0)
                    {
                        defenderHeir = heirs.First().Key;
                    }
                }
            }

            int attackCaptives = 0;
            float attackCaptivesScore = 0f;
            foreach (var hero in attackerCaptives)
            {
                float points = CalculateHeroScore(hero, totalWarScore, hero == defenderHeir).ResultNumber * justification.CaptureWeight;
                float total = points / totalWarScore;
                if (total >= 1f) result.Add(total, hero.Name);
                else
                {
                    attackCaptives++;
                    attackCaptivesScore += total;
                }
            }

            result.Add(attackCaptivesScore, new TextObject("{=!}Attacker captives (x{TOTAL})")
                .SetTextVariable("TOTAL", attackCaptives));

            if (justification.IsFulfilled(war))
            {
                result.Add(0.5f, new TextObject("{=!}Objective fulfilled ({OBJECTIVE})")
                    .SetTextVariable("OBJECTIVE", justification.ObjectiveText));
            }

            // --- DEFENDER ----

            List<Settlement> defenderConquests = DiplomacyHelper.GetSuccessfullSiegesInWarForFaction(defender,
                attackerLink, (Settlement x) => x.Town != null);
            foreach (var settlement in defenderConquests)
            {
                if (settlement.MapFaction == defender && settlement != justification.Fief)
                {
                    float points = -CalculateFiefScore(settlement).ResultNumber;
                    result.Add(points / totalWarScore, settlement.Name);
                }
            }

            List<Settlement> defenderRaids = DiplomacyHelper.GetRaidsInWar(defender, attackerLink, null);
            foreach (var settlement in defenderRaids)
            {
                float points = -CalculateRaidScore(settlement.Village).ResultNumber * justification.RaidWeight;
                result.Add(points / totalWarScore, new TextObject("{=!}Raid of {VILLAGE}")
                    .SetTextVariable("VILLAGE", settlement.Name));
            }

            List<Hero> defenderCaptives = DiplomacyHelper.GetPrisonersOfWarTakenByFaction(defender, attacker);
            Hero attackerHeir = null;
            if (attacker.IsKingdomFaction)
            {
                var kingdomTitle = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(attacker as Kingdom);
                if (kingdomTitle != null)
                {
                    IEnumerable<KeyValuePair<Hero, ExplainedNumber>> heirs = BannerKingsConfig.Instance.TitleModel
                        .CalculateSuccessionLine(kingdomTitle, attacker.Leader.Clan);
                    if (heirs.Count() > 0)
                    {
                        attackerHeir = heirs.First().Key;
                    }
                }
            }

            int defendCaptives = 0;
            float defendCaptivesScore = 0f;
            foreach (var hero in defenderCaptives)
            {
                float points = -CalculateHeroScore(hero, totalWarScore, hero == attackerHeir).ResultNumber;
                float total = points / totalWarScore;
                if (total <= -1f) result.Add(total, hero.Name);
                else
                {
                    defendCaptives++;
                    defendCaptivesScore += total;
                }
            }

            result.Add(defendCaptivesScore, new TextObject("{=!}Defener captives (x{TOTAL})")
               .SetTextVariable("TOTAL", defendCaptives));

            if (isDefenderScore)
            {
                result.AddFactor(-1f, new TextObject("{=!}Defender's perspective");
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

                float renown = hero.Clan.Renown / 2;
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

        public ExplainedNumber CalculateRaidScore(Village village, bool explanations = false)
        {
            var result = CalculateFiefScore(village.Settlement, false, explanations);
            result.AddFactor(-0.66f, new TextObject("{=!}Raid"));
            return result;
        }

        public ExplainedNumber CalculateFiefScore(Settlement settlement, bool isOriginalFront = false, bool explanations = false)
        {
            var result = new ExplainedNumber(0f, explanations);

            IFaction ownerFaction = settlement.MapFaction;
            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
            if (data != null)
            {
                result.Add(data.TotalPop / 2f, new TextObject("{=!}Population of {FIEF}")
                        .SetTextVariable("FIEF", settlement.Name));

                if (data.ReligionData != null && data.ReligionData.DominantReligion != null)
                {
                    var religion = data.ReligionData.DominantReligion;
                    if (ownerFaction.Leader != null && religion != null)
                    {
                        var leaderReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(ownerFaction.Leader);
                        if (leaderReligion != null && leaderReligion.Faith.GetId() == religion.Faith.GetId())
                        {
                            result.AddFactor(0.25f, new TextObject("{=!}{FIEF} has same faith as it's realm")
                                .SetTextVariable("FIEF", settlement.Name));
                        }
                    }
                }
            }

            if (settlement.Town != null)
            {
                result.Add(settlement.Prosperity, new TextObject("{=!}Prosperity of {FIEF}")
                        .SetTextVariable("FIEF", settlement.Name));
            }
            else if (settlement.Village != null)
            {
                result.Add(settlement.Village.Hearth, new TextObject("{=!}Hearths in {FIEF}")
                        .SetTextVariable("FIEF", settlement.Name));
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

        public BKExplainedNumber CalculateFatigue(War war, IFaction faction, bool explanations = false)
        {
            var result = new BKExplainedNumber(0f, explanations);
            result.LimitMin(0f);
            result.LimitMax(1f);

            IFaction enemy;
            if (war.Defender == faction)
            {
                enemy = war.Attacker;
            }
            else
            {
                enemy = war.Defender;
            }

            StanceLink stance = faction.GetStanceWith(enemy);
            int casualties = stance.GetCasualties(faction);
            result.Add(casualties / GetTotalManpower(faction.Settlements), GameTexts.FindText("str_war_casualties_inflicted"));

            float yearsPassed = stance.WarStartDate.ElapsedYearsUntilNow;
            result.Add(yearsPassed * 0.04f, new TextObject("{=!}War duration"));

            if (war.CasusBelli.Fief != null)
            {
                float daysPassed = MathF.Max(1f, yearsPassed * CampaignTime.DaysInYear);
                result.AddFactor(-war.GetDaysHeldObjective(faction) / daysPassed, new TextObject("{=!}Time controlling objective"));
            }

            return result;
        }

        private int GetTotalManpower(Settlement settlement)
        {
            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
            if (data != null)
            {
                return (int)(BannerKingsConfig.Instance.GrowthModel.CalculateSettlementCap(settlement, data).ResultNumber * 0.08f);
            }

            return 0;
        }
    }
}
