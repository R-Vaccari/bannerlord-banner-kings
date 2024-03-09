using BannerKings.Behaviours.Diplomacy;
using BannerKings.Behaviours.Diplomacy.Wars;
using BannerKings.Extensions;
using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Institutions.Religions.Faiths;
using BannerKings.Managers.Titles.Governments;
using BannerKings.Managers.Titles;
using BannerKings.Managers.Traits;
using BannerKings.Utils.Models;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using System;

namespace BannerKings.Models.Vanilla
{
    public class BKDiplomacyModel : DefaultDiplomacyModel
    {
        public float TRADE_PACT_INFLUENCE_CAP { get;} = 100f;

        public ExplainedNumber CalculateHeroFiefScore(Settlement settlement, Hero annexing, bool explanations = false)
        {
            ExplainedNumber result = new ExplainedNumber(0f, explanations);
            Clan clan = annexing.Clan;

            if (BannerKingsConfig.Instance.ReligionsManager.HasBlessing(annexing, DefaultDivinities.Instance.AseraMain))
            {
                result.AddFactor(0.2f, DefaultDivinities.Instance.AseraMain.Name);
            }

            if (settlement.Owner == annexing)
            {
                result.Add(150f, new TextObject("{=!}{HERO} is the established owner of {FIEF}")
                    .SetTextVariable("HERO", clan.Leader.Name)
                    .SetTextVariable("FIEF", settlement.Name));
            }

            FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetTitle(settlement);
            if (title != null)
            {
                if (title.deJure == clan.Leader)
                {
                    result.Add(400f, new TextObject("{=!}{HERO} is de jure holder of {TITLE}")
                        .SetTextVariable("HERO", clan.Leader.Name)
                        .SetTextVariable("TITLE", title.FullName));
                }
                else if (title.HeroHasValidClaim(clan.Leader))
                {
                    result.Add(250f, new TextObject("{=!}{HERO} is a legal claimant of {TITLE}")
                       .SetTextVariable("HERO", clan.Leader.Name)
                       .SetTextVariable("TITLE", title.FullName));
                }
            }

            FeudalTitle sovereign = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(annexing.Clan.Kingdom);
            if (sovereign != null)
            {
                if (sovereign.Contract.HasContractAspect(DefaultContractAspects.Instance.ConquestMight))
                {
                    if (settlement.Town != null)
                    {
                        if (clan == settlement.Town.LastCapturedBy)
                        {
                            result.Add(1000f, new TextObject("{=!}Last conquered by {CLAN} ({LAW})")
                                .SetTextVariable("CLAN", clan.Name)
                                .SetTextVariable("LAW", DefaultContractAspects.Instance.ConquestMight.Name));
                        }
                    }
                }
                else if (sovereign.Contract.HasContractAspect(DefaultContractAspects.Instance.ConquestClaim))
                {
                    if (title != null)
                    {
                        if (title.deJure == clan.Leader)
                        {
                            result.Add(1000f, new TextObject("{=!}{HERO} is de jure holder of {TITLE} ({LAW})")
                                .SetTextVariable("HERO", clan.Leader.Name)
                                .SetTextVariable("TITLE", title.FullName)
                                .SetTextVariable("LAW", DefaultContractAspects.Instance.ConquestClaim.Name));
                        }
                        else if (title.HeroHasValidClaim(clan.Leader))
                        {
                            result.Add(500f, new TextObject("{=!}{HERO} is a claimant of {TITLE} ({LAW})")
                                .SetTextVariable("HERO", clan.Leader.Name)
                                .SetTextVariable("TITLE", title.FullName)
                                .SetTextVariable("LAW", DefaultContractAspects.Instance.ConquestClaim.Name));
                        }
                    }
                }
                else if (sovereign.Contract.HasContractAspect(DefaultContractAspects.Instance.ConquestDistributed))
                {
                    foreach (Settlement fief in clan.Settlements)
                    {
                        if (fief.IsTown) result.Add(-150f, new TextObject("{=!}Owns {FIEF} ({LAW})")
                            .SetTextVariable("FIEF", fief.Name)
                            .SetTextVariable("LAW", DefaultContractAspects.Instance.ConquestDistributed.Name));
                        else if (fief.IsCastle) result.Add(-75f, new TextObject("{=!}Owns {FIEF} ({LAW})")
                            .SetTextVariable("FIEF", fief.Name)
                            .SetTextVariable("LAW", DefaultContractAspects.Instance.ConquestDistributed.Name));
                        else if (fief.IsVillage && fief.Village.GetActualOwner() == annexing)
                            result.Add(-30f, new TextObject("{=!}Owns {FIEF} ({LAW})")
                            .SetTextVariable("FIEF", fief.Name)
                            .SetTextVariable("LAW", DefaultContractAspects.Instance.ConquestDistributed.Name));
                    }
                }
            }

            var limit = BannerKingsConfig.Instance.StabilityModel.CalculateDemesneLimit(clan.Leader).ResultNumber;
            var current = BannerKingsConfig.Instance.StabilityModel.CalculateCurrentDemesne(clan).ResultNumber;
            float factor = current / limit;
            result.Add(500f * (1f - factor), new TextObject("{=!}Current Demesne Limit {CURRENT}/{LIMIT}")
                .SetTextVariable("CURRENT", current.ToString("0.0"))
                .SetTextVariable("LIMIT", limit.ToString("0.0")));

            return result;
        }

        public override int GetInfluenceCostOfAnnexation(Clan proposingClan)
        {
            float result = base.GetInfluenceCostOfAnnexation(proposingClan);
            result += BannerKingsConfig.Instance.InfluenceModel.CalculateInfluenceCap(proposingClan).ResultNumber * 0.25f;

            return (int)result;
        }

        public override void GetHeroesForEffectiveRelation(Hero hero1, Hero hero2, out Hero effectiveHero1, out Hero effectiveHero2)
        {
            effectiveHero1 = hero1;
            effectiveHero2 = hero2;

            if (effectiveHero1 == effectiveHero2 || effectiveHero1 == null || effectiveHero2 == null)
            {
                effectiveHero1 = ((hero1.Clan != null) ? hero1.Clan.Leader : hero1);
                effectiveHero2 = ((hero2.Clan != null) ? hero2.Clan.Leader : hero2);
                if (hero1 != null)
                {
                    if (effectiveHero1 == effectiveHero2 || (hero1.IsPlayerCompanion && hero2.IsHumanPlayerCharacter) || (hero1.IsPlayerCompanion && hero2.IsHumanPlayerCharacter))
                    {
                        effectiveHero1 = hero1;
                        effectiveHero2 = hero2;
                    }
                }
            }
        }

        public override int GetInfluenceCostOfProposingWar(Clan proposingClan)
        {
            int result = base.GetInfluenceCostOfProposingWar (proposingClan);
            return result;
        }

        public ExplainedNumber WillJoinWar(IFaction attacker, IFaction defender, IFaction ally,
            DeclareWarAction.DeclareWarDetail detail, bool explanations = false)
        {
            ExplainedNumber result = new ExplainedNumber(0, explanations);
            Clan allyClan = ally.IsClan ? (ally as Clan) : (ally as Kingdom).RulingClan;
            Clan defenderClan = defender.IsClan ? (defender as Clan) : (defender as Kingdom).RulingClan;

            float defenderRelation = allyClan.Leader.GetRelation(defenderClan.Leader);
            result.Add(defenderRelation * 0.2f, new TextObject("{=tB34b2ov}Relation"));

            float honor = allyClan.Leader.GetTraitLevel(DefaultTraits.Honor);
            result.Add(honor * 0.1f, new TextObject("{=0usOMAnM}{HERO}'s honor")
                .SetTextVariable("HERO", allyClan.Name));

            KingdomElection warSupport = new KingdomElection(new BKDeclareWarDecision(null, allyClan, attacker));
            result.Add(warSupport.GetLikelihoodForOutcome(0), new TextObject("{=uXVMjfM9}War support in {ALLY}")
                .SetTextVariable("ALLY", ally.Name));

            /*War war = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKDiplomacyBehavior>().GetWar(attacker, defender);
            if (war != null)
            {
                if (war.CasusBelli == null)
                {
                    result.Add(-0.25f, new TextObject("{=nzbtmjPJ}Unjustified war"));
                }
            }
            else
            {
                result.Add(-0.25f, new TextObject("{=nzbtmjPJ}Unjustified war"));
            }

            if (detail == DeclareWarAction.DeclareWarDetail.CausedByPlayerHostility)
            {
                result.Add(-0.15f, new TextObject("{=Fxqpy1sE}War started by illegal aggression"));
            }*/

            return result;
        }

        public ExplainedNumber GetPactInfluenceCost(Kingdom proposer, Kingdom proposed, bool explanations = false)
        {
            ExplainedNumber result = new ExplainedNumber(0, explanations);
            float peace = GetScoreOfDeclaringPeace(proposed, proposer, proposed, out TextObject reason) / 2f;

            foreach (var clan in proposer.Clans)
            {
                if (clan == proposer.RulingClan || clan.IsUnderMercenaryService)
                {
                    continue;
                }

                float relation = clan.Leader.GetRelation(proposer.RulingClan.Leader) / 150f;
                //result.Add((100000f - peace) * MathF.Sqrt(years), clan.Name);
            }
           
            result.AddFactor(-peace / 100000f, new TextObject("{=!}"));
            return result;
        }

        public bool IsTruceAcceptable(Kingdom proposer, Kingdom proposed, bool explanations = false)
        {
            if (proposed == proposer) return false;
            
            float peace = GetScoreOfDeclaringPeace(proposed, proposer, proposed, out TextObject reason);
            return peace > 0;
        }

        public bool IsTradeAcceptable(Kingdom proposer, Kingdom proposed, bool explanations = false)
        {
            if (proposed == proposer) return false;

            float peace = GetScoreOfDeclaringPeace(proposed, proposer, proposed, out TextObject reason);
            float influence = BannerKingsConfig.Instance.InfluenceModel.CalculateInfluenceCap(proposed.RulingClan)
                .ResultNumber;
            return peace > 0 && influence > TRADE_PACT_INFLUENCE_CAP;
        }

        public ExplainedNumber GetTruceDenarCost(Kingdom proposer, Kingdom proposed, float years = 3f, bool explanations = false)
        {
            ExplainedNumber result = new ExplainedNumber(0, explanations);
            float peace = GetScoreOfDeclaringPeace(proposed, proposer, proposed, out TextObject reason) / 2f;
            result.Add((100000f - peace) * MathF.Sqrt(years), new TextObject("{=PsRfxMEv}Truce duration"));

            float relation = proposed.RulingClan.Leader.GetRelation(proposer.RulingClan.Leader) / 150f;
            result.AddFactor(-relation, new TextObject("{=BlidMNGT}Relation"));

            return result;
        }

        public ExplainedNumber GetAllianceDesire(Kingdom proposer, Kingdom proposed, bool explanations = false)
        {
            ExplainedNumber result = new ExplainedNumber(0, explanations);
            result.Add(-100f, new TextObject("{=Gq5BnNiN}Reluctance"));

            KingdomElection election = new KingdomElection(new BKDeclareWarDecision(null, proposed.RulingClan, proposer));
            result.Add(election.GetLikelihoodForOutcome(1) * 85f, new TextObject("{=04Smb5KQ}Peace support in {ALLY}")
                .SetTextVariable("ALLY", proposed.Name));

            float relation = proposed.RulingClan.Leader.GetRelation(proposer.RulingClan.Leader);
            result.Add(relation, new TextObject("{=BlidMNGT}Relation"));

            /* War possibleWar = new War(proposer, proposed, null, null);
            if (possibleWar.DefenderFront != null && possibleWar.AttackerFront != null)
            {
                float distance = TaleWorlds.CampaignSystem.Campaign.Current.Models.MapDistanceModel.GetDistance(possibleWar.DefenderFront.Settlement,
                                            possibleWar.AttackerFront.Settlement) * 2f;
                float factor = (TaleWorlds.CampaignSystem.Campaign.AverageDistanceBetweenTwoFortifications / distance) - 0.5f;
                result.AddFactor(factor, new TextObject("{=fiHYU8X3}Distance between realms"));
            }*/
                
            if (proposed.RulingClan.Leader.Culture == proposer.RulingClan.Leader.Culture)
            {
                result.Add(10f, new TextObject("{=qR61PqMa}Shared culture"));
            }

            result.Add(proposer.RulingClan.Leader.GetTraitLevel(DefaultTraits.Honor) * 15f,
                new TextObject("{=vrm5pNf3}Honor of {HERO}").SetTextVariable("HERO", proposer.RulingClan.Leader.Name));

            Religion proposerReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(proposer.RulingClan.Leader);
            if (proposerReligion != null)
            {
                Religion proposedReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(proposed.RulingClan.Leader);
                if (proposedReligion != null)
                {
                    if (proposerReligion == proposedReligion)
                    {
                        result.Add(25f, new TextObject("{=Pcy4iFnT}Shared faith"));
                    }
                    else
                    {
                        FaithStance faithStance = proposedReligion.GetStance(proposerReligion.Faith);
                        if (faithStance != FaithStance.Tolerated)
                        {
                            result.Add(faithStance == FaithStance.Untolerated ? -20f : -50f, new TextObject("Faith differences"));
                        }
                    }
                }
            }

            return result;
        }

        public bool WillAcceptAlliance(Kingdom proposer, Kingdom proposed) => GetAllianceDesire(proposer, proposed).ResultNumber > 0f;

        public ExplainedNumber GetAllianceDenarCost(Kingdom proposer, Kingdom proposed, bool explanations = false)
        {
            ExplainedNumber result = new ExplainedNumber(0, explanations);
            float peace = GetScoreOfDeclaringPeace(proposed, proposer, proposed, out TextObject reason) / 2f;
            result.Add((100000f), new TextObject("{=PsRfxMEv}Truce duration"));

            float income = 0f;
            foreach (Clan clan in proposer.Clans)
            {
                income += BannerKingsConfig.Instance.ClanFinanceModel.CalculateClanIncome(clan).ResultNumber;
            }

            result.Add(income, new TextObject("{=PsRfxMEv}Truce duration"));

            result.Add(proposed.TotalStrength * 20f, new TextObject("{=PsRfxMEv}Truce duration"));

            float relation = proposed.RulingClan.Leader.GetRelation(proposer.RulingClan.Leader) / 150f;
            result.AddFactor(-relation, new TextObject("{=BlidMNGT}Relation"));

            return result;
        }

        public ExplainedNumber GetTradePactInfluenceCost(Kingdom proposer, Kingdom proposed, bool explanations = false)
        {
            ExplainedNumber result = new ExplainedNumber(100, explanations);
            foreach (var fief in proposer.Fiefs)
            {
                if (fief.IsTown && fief.OwnerClan != proposer.RulingClan)
                {
                    result.Add(BannerKingsConfig.Instance.InfluenceModel.CalculateSettlementInfluence(fief.Settlement,
                        BannerKingsConfig.Instance.PopulationManager.GetPopData(fief.Settlement)).ResultNumber,
                        fief.Name);
                }
            }

            float cap = BannerKingsConfig.Instance.InfluenceModel.CalculateInfluenceCap(proposer.RulingClan).ResultNumber;
            result.Add(cap, new TextObject("{=!}Influence limit of {CLAN}")
                .SetTextVariable("CLAN", proposer.RulingClan.Name));

            float peace = GetScoreOfDeclaringPeace(proposed, proposer, proposed, out TextObject reason);
            result.AddFactor(peace / -60000f, new TextObject("{=hAAOEqaJ}Peace interest"));
            return result;
        }

        public override float GetScoreOfDeclaringWar(IFaction factionDeclaresWar, IFaction factionDeclaredWar, IFaction evaluatingClan, out TextObject warReason)
        {
            return GetScoreOfDeclaringWar(factionDeclaresWar, factionDeclaredWar, evaluatingClan, out warReason, null).ResultNumber * 10f;
        }

        public override float GetScoreOfDeclaringPeace(IFaction factionDeclaresPeace, IFaction factionDeclaredPeace, IFaction evaluatingClan, out TextObject peaceReason)
        {
            ExplainedNumber result = new ExplainedNumber(-GetScoreOfDeclaringWar(factionDeclaresPeace, 
                factionDeclaredPeace, evaluatingClan, out peaceReason, null).ResultNumber);

            War war = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKDiplomacyBehavior>().GetWar(factionDeclaresPeace,factionDeclaredPeace);
            if (war != null)
            {
                BKExplainedNumber fatigue = BannerKingsConfig.Instance.WarModel.CalculateFatigue(war, Hero.MainHero.MapFaction, true);
                result.AddFactor(fatigue.ResultNumber);
            }

            return result.ResultNumber * 10f;
        }

        public float CalculateThreatFactor(IFaction attacker, IFaction threat)
        {
            float totalThreat = 0f;
            foreach (Kingdom k in Kingdom.All)
            {
                if (k != attacker && k != threat)
                {
                    totalThreat += k.TotalStrength;
                }
            }

            return threat.TotalStrength / totalThreat;
        }

        public ExplainedNumber GetScoreOfDeclaringWar(IFaction factionDeclaresWar, IFaction factionDeclaredWar, IFaction evaluatingClan,
           out TextObject warReason, CasusBelli casusBelli = null, bool explanations = false)
        {
            warReason = TextObject.Empty;
            var result = new ExplainedNumber(0f, explanations);
            result.LimitMin(-50000f);
            result.LimitMax(50000f);

            if (factionDeclaresWar.MapFaction == factionDeclaredWar.MapFaction)
            {
                return new ExplainedNumber(-50000f);
            }

            float baseNumber = 0f;

            WarStats attackerStats = CalculateWarStats(factionDeclaresWar, factionDeclaredWar);
            float attackerScore = attackerStats.Strength + attackerStats.ValueOfSettlements - (attackerStats.TotalStrengthOfEnemies * 1.25f);

            if (factionDeclaresWar.IsKingdomFaction)
            {
                var tributes = factionDeclaresWar.Stances.ToList().FindAll(x => x.GetDailyTributePaid(x.Faction2) > 0);
                int tributeCount = tributes.Count;
                result.Add(baseNumber * -0.15f * tributeCount, new TextObject("{=TCVWRr8K}Paying tributes (x{COUNT})")
                    .SetTextVariable("COUNT", tributeCount));
            }

            /*float totalThreat = 0f;
            foreach (Kingdom k in Kingdom.All)
            {
                if (k != factionDeclaredWar && k != factionDeclaresWar)
                {
                    totalThreat += k.TotalStrength;
                }
            }

            float threatFactor = factionDeclaredWar.TotalStrength / totalThreat;
            result.Add(10000f * threatFactor,
                new TextObject("{=s21vTPmS}{THREAT}% threat relative to all possible enemies")
                .SetTextVariable("THREAT", (threatFactor * 100f).ToString("0.0"))); */

            if (factionDeclaredWar.IsKingdomFaction && factionDeclaresWar.IsKingdomFaction)
            {
                var attackerKingdom = (Kingdom)factionDeclaresWar;
                var defenderKingdom = (Kingdom)factionDeclaredWar;

                TextObject reason;
                bool warAllowed = TaleWorlds.CampaignSystem.Campaign.Current.Models.KingdomDecisionPermissionModel
                    .IsWarDecisionAllowedBetweenKingdoms(attackerKingdom, defenderKingdom, out reason);
                if (!warAllowed)
                {
                    return new ExplainedNumber(-50000f, explanations, reason);
                }
   
                KingdomDiplomacy diplomacy = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKDiplomacyBehavior>().GetKingdomDiplomacy(attackerKingdom);
                if (diplomacy != null)
                {
                    if (diplomacy.HasTradePact(defenderKingdom))
                    {
                        result.Add(baseNumber * - 0.25f, new TextObject("{=DPK2KdUk}Trade pact between both realms"));
                    }

                    if (casusBelli == null)
                    {
                        List<CasusBelli> justifications = diplomacy.GetAvailableCasusBelli(defenderKingdom);
                        foreach (CasusBelli justification in justifications)
                        {
                            float num = justification.DeclareWarScore / justifications.Count;
                            result.Add(num, new TextObject("{=onUOp8WF}{CASUS} justification")
                                .SetTextVariable("CASUS", justification.Name));
                            GetPersonalityCasusBelliEffect(ref result, num, evaluatingClan, justification);
                        }
                    }
                    else
                    {
                        float num = casusBelli.DeclareWarScore * 2f;
                        result.Add(num, new TextObject("{=onUOp8WF}{CASUS} justification")
                            .SetTextVariable("CASUS", casusBelli.Name));
                        GetPersonalityCasusBelliEffect(ref result, num, evaluatingClan, casusBelli);
                    }

                    baseNumber = result.BaseNumber;
                    result.Add(baseNumber * -diplomacy.Fatigue, new TextObject("{=Rdmm1Kmh}General war fatigue of {FACTION}")
                        .SetTextVariable("FACTION", diplomacy.Kingdom.Name));
                }

                foreach (Kingdom enemyKingdom in FactionManager.GetEnemyKingdoms(attackerKingdom))
                {
                    if (enemyKingdom != attackerKingdom && enemyKingdom != defenderKingdom)
                    {
                        WarStats enemyStats = CalculateWarStats(factionDeclaresWar, enemyKingdom);
                        float enemyScore = enemyStats.Strength + enemyStats.ValueOfSettlements - (enemyStats.TotalStrengthOfEnemies * 1.25f);
                        float f = 2f - (attackerScore / (enemyScore * 5f));
                        result.Add(-baseNumber * f, new TextObject("{=epNrP2AT}Existing war with {FACTION}")
                        .SetTextVariable("FACTION", enemyKingdom.Name));
                    }
                }
            }

            StanceLink stance = factionDeclaresWar.GetStanceWith(factionDeclaredWar);
            /*int tribute = stance.GetDailyTributePaid(factionDeclaredWar);
            if (tribute > 0)
            {
                result.Add(-10000f, new TextObject("{=ZtL0fh80}{FACTION} is paying us tribute")
                    .SetTextVariable("FACTION", factionDeclaredWar.Name));
            }
            else if (tribute < 0)
            {
                result.Add(baseNumber * 0.3f, new TextObject("{=tvtoXveu}We are paying tribute to {FACTION}")
                    .SetTextVariable("FACTION", factionDeclaredWar.Name));
            }*/

            if (stance.BehaviorPriority == 1)
            {
                result.Add(-baseNumber, new TextObject("{=fvd0nAa3}Defensive stance against {FACTION}")
                    .SetTextVariable("FACTION", factionDeclaredWar.Name));
            }

            if (evaluatingClan != null)
            {
                float relations = evaluatingClan.Leader.GetRelation(factionDeclaredWar.Leader);
                result.Add(baseNumber * (-relations / 100f), new TextObject("{=nnYfQnWv}{HERO1}`s opinion of {HERO2}")
                    .SetTextVariable("HERO1", evaluatingClan.Leader.Name)
                .SetTextVariable("HERO2", factionDeclaredWar.Leader.Name));
            }

            float threatFactor = CalculateThreatFactor(factionDeclaresWar, factionDeclaredWar);
            result.Add(baseNumber * threatFactor * 2f, new TextObject("{=ew3Ga8Lu}{THREAT}% threat relative to possible enemies")
                .SetTextVariable("THREAT", (threatFactor * 100f).ToString("0.0")));

            float attackerStrength = factionDeclaresWar.TotalStrength;
            float defenderStrength = factionDeclaredWar.TotalStrength;
            foreach (IFaction ally in factionDeclaredWar.GetAllies())
            {
                defenderStrength += ally.TotalStrength / 2f;
            }

            float strengthFactor = (attackerStrength / defenderStrength) - 1f;
            result.Add(baseNumber * MathF.Clamp(strengthFactor * 0.6f, -2f, 0.5f), new TextObject("{=KcLdYKrY}Difference in strength"));

            if (factionDeclaredWar.Fiefs.Count < factionDeclaresWar.Fiefs.Count / 2f)
            {
                float fiefFactor = factionDeclaredWar.Fiefs.Count / factionDeclaresWar.Fiefs.Count;
                result.Add(-baseNumber * (2f - fiefFactor), new TextObject("{=SRN3KdjF}Unworthy opponent"));
            }

            if (defenderStrength >= attackerStrength * 1.5f)
            {
                result.Add(baseNumber * MathF.Clamp(strengthFactor * 0.5f, -5f, -0.4f), new TextObject("{=Z7AW5i79}Enemy significantly stronger"));
            }

            float attackerFiefs = factionDeclaresWar.Fiefs.Count;
            float defenderFiefs = factionDeclaredWar.Fiefs.Count;
            float fiefsFactor = (attackerFiefs  / defenderFiefs) - 1f;
            result.Add(baseNumber * MathF.Clamp(fiefsFactor * 0.1f, -2f, 2f), new TextObject("{=MvV0HUdo}Difference in controlled fiefs"));

            War war = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKDiplomacyBehavior>().GetWar(factionDeclaredWar, factionDeclaresWar);
            if (war != null)
            {
                if (war.StartDate.ElapsedYearsUntilNow < 1f) result.Add(50000f, new TextObject("{=UaofTriA}Recently started war"));

                float score = MathF.Clamp(war.CalculateWarScore(war.Attacker, false).ResultNumber /
                    war.TotalWarScore.ResultNumber, -1f, 1f) * 2f;
                result.Add(baseNumber * (war.Attacker == factionDeclaresWar ? -score : score));

                float fatigue = BannerKingsConfig.Instance.WarModel.CalculateFatigue(war, factionDeclaresWar).ResultNumber * 4f;
                result.Add(baseNumber * -fatigue, new TextObject("{=Nxrd7yym}Fatigue over this war"));
            }
            else
            {
                if (stance.IsAtWar)
                {
                    result.Add(-50000f);
                }
                else
                {
                    ValueTuple<Settlement, Settlement> border = GetBorder(factionDeclaresWar, factionDeclaredWar);
                    if (border.Item1 != null && border.Item2 != null)
                    {
                        float distance = TaleWorlds.CampaignSystem.Campaign.Current.Models.MapDistanceModel.GetDistance(border.Item1, border.Item2);
                        float factor = (TaleWorlds.CampaignSystem.Campaign.AverageDistanceBetweenTwoFortifications / distance) - 1f;
                        result.Add(MathF.Clamp(baseNumber * factor * 2f, baseNumber * -2f, 0f), new TextObject("{=fiHYU8X3}Distance between realms"));
                    }
                }

                //WarStats enemyStats = CalculateWarStats(factionDeclaresWar, enemyKingdom);
                //float enemyScore = enemyStats.Strength + enemyStats.ValueOfSettlements - (enemyStats.TotalStrengthOfEnemies * 1.25f);
            }

            if (evaluatingClan != null && evaluatingClan is Clan)
            {
                Clan evaluating = (Clan)evaluatingClan;
                Hero leader = evaluating.Leader;
                float traits = leader.GetTraitLevel(DefaultTraits.Valor) - leader.GetTraitLevel(DefaultTraits.Mercy) +
                    leader.GetTraitLevel(BKTraits.Instance.AptitudeViolence);
                result.Add(baseNumber * (traits / 4f));

                float enemies = 1f;
                if (evaluating.Kingdom != null) enemies += FactionManager.GetEnemyKingdoms(evaluating.Kingdom).Count();

                int gold = (int)(leader.Gold / enemies);
                if (gold < 50000)
                {
                    result.Add(result.BaseNumber * -0.8f);
                }
                else if (gold < 100000)
                {
                    result.Add(result.BaseNumber * -0.4f);
                }
            }

            /*WarStats defenderStats = CalculateWarStats(factionDeclaredWar, factionDeclaresWar);
            float defenderScore = defenderStats.Strength + defenderStats.ValueOfSettlements - (defenderStats.TotalStrengthOfEnemies * 1.25f);
            float scoreProportion = (attackerScore / defenderScore) - 1f;
            result.AddFactor(scoreProportion);*/

            return result;
        }

        private ValueTuple<Settlement, Settlement> GetBorder(IFaction faction1, IFaction faction2)
        {
            float distance = float.MaxValue;
            Settlement border1 = null;
            Settlement border2 = null;
            foreach (Town fief1 in faction1.Fiefs)
            {
                foreach (Town fief2 in faction2.Fiefs)
                {
                    float d = TaleWorlds.CampaignSystem.Campaign.Current.Models.MapDistanceModel.GetDistance(fief1.Settlement, fief2.Settlement);
                    if (d < distance)
                    {
                        border1 = fief1.Settlement;
                        border2 = fief2.Settlement;
                        distance = d;
                    }
                }
            }

            return new (border1, border2);
        }

        private void GetPersonalityCasusBelliEffect(ref ExplainedNumber result, float baseResult, IFaction evaluation, CasusBelli casusBelli)
        {
            if (evaluation != null && evaluation.IsClan)
            {
                Hero leader = evaluation.Leader;
                foreach (TraitObject trait in BKTraits.Instance.PersonalityTraits.Concat(BKTraits.Instance.PoliticalTraits))
                {
                    float factor = casusBelli.GetTraitWeight(trait);
                    float level = leader.GetTraitLevel(trait);
                    result.Add(baseResult * factor * level, new TextObject("{=fLSny0R8}{HERO}'s {TRAIT} personality")
                        .SetTextVariable("HERO", leader.Name)
                        .SetTextVariable("TRAIT", trait.Name));
                }
            }
        }

        private WarStats CalculateWarStats(IFaction faction, IFaction targetFaction)
        {
            Clan rulingClan = faction.IsClan ? (faction as Clan) : (faction as Kingdom).RulingClan;
            float valueOfSettlements = faction.Fiefs.Sum((Town f) => (float)(f.IsTown ? 2000 : 1000) + f.Prosperity * 0.33f) * 50f;
            float enemyStrength = 0f;
            foreach (StanceLink stanceLink in faction.Stances)
            {
                if (stanceLink.IsAtWar && stanceLink.Faction1 != targetFaction && stanceLink.Faction2 != targetFaction && (!stanceLink.Faction2.IsMinorFaction || stanceLink.Faction2.Leader == Hero.MainHero))
                {
                    IFaction faction2 = (stanceLink.Faction1 == faction) ? stanceLink.Faction2 : stanceLink.Faction1;
                    enemyStrength += faction2.TotalStrength;
                }
            }

            return new WarStats
            {
                RulingClan = rulingClan,
                Strength = faction.TotalStrength,
                ValueOfSettlements = valueOfSettlements,
                TotalStrengthOfEnemies = enemyStrength
            };
        }

        public struct WarStats
        {
            public Clan RulingClan;
            public float Strength;
            public float ValueOfSettlements;
            public float TotalStrengthOfEnemies;
        }
    }
}


