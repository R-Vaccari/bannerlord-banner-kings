using BannerKings.Managers.Decisions;
using BannerKings.Managers.Education.Lifestyles;
using BannerKings.Managers.Policies;
using BannerKings.Managers.Skills;
using BannerKings.Populations;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using static BannerKings.Managers.Policies.BKTaxPolicy;
using static BannerKings.Managers.Policies.BKWorkforcePolicy;

namespace BannerKings.Managers.AI
{
    public class AIBehavior
    {

        public Lifestyle ChooseLifestyle(Hero hero)
        {
            List<(Lifestyle, float)> candidates = new List<(Lifestyle, float)>();

            int rogueWeight = hero.GetTraitLevel(DefaultTraits.RogueSkills) - hero.GetTraitLevel(DefaultTraits.Mercy) - 
                hero.GetTraitLevel(DefaultTraits.Honor) + hero.GetTraitLevel(DefaultTraits.Thug) + hero.GetTraitLevel(DefaultTraits.Thief);

            int politicianWeight = hero.GetTraitLevel(DefaultTraits.Politician) + hero.GetTraitLevel(DefaultTraits.Commander);

            int merchantWeight = hero.GetTraitLevel(DefaultTraits.Blacksmith) + hero.GetTraitLevel(DefaultTraits.Manager);

            int siegeWeight = hero.GetTraitLevel(DefaultTraits.Siegecraft);

            int healerWeight = hero.GetTraitLevel(DefaultTraits.Surgery);

            int warriorWeight = hero.GetTraitLevel(DefaultTraits.ArcherFIghtingSkills) + hero.GetTraitLevel(DefaultTraits.BossFightingSkills) + 
                hero.GetTraitLevel(DefaultTraits.CavalryFightingSkills) + hero.GetTraitLevel(DefaultTraits.HuscarlFightingSkills) +
                hero.GetTraitLevel(DefaultTraits.HopliteFightingSkills) + hero.GetTraitLevel(DefaultTraits.HorseArcherFightingSkills) +
                hero.GetTraitLevel(DefaultTraits.KnightFightingSkills) + hero.GetTraitLevel(DefaultTraits.PeltastFightingSkills) +
                hero.GetTraitLevel(DefaultTraits.Fighter);

            int mercenaryWeight = hero.GetTraitLevel(DefaultTraits.RogueSkills) - hero.GetTraitLevel(DefaultTraits.Honor);

            Occupation occupation = hero.Occupation;
            if (occupation == Occupation.Lord)
            {
                politicianWeight += 2;
                warriorWeight += 3;

                if (!hero.Clan.IsMinorFaction) mercenaryWeight = 0;
                healerWeight -= 1;
            } else if (occupation == Occupation.Wanderer)
            {
                warriorWeight += 4;
                mercenaryWeight += 1;
            } else if (hero.IsNotable)
            {
                if (occupation == Occupation.GangLeader) rogueWeight += 2;
                mercenaryWeight += 4;

                politicianWeight = 0;
                warriorWeight = 0;
                mercenaryWeight = 0;
            }


            foreach (Lifestyle lf in DefaultLifestyles.Instance.All)
            {
                if (!lf.CanLearn(hero)) continue;

                SkillObject first = lf.FirstSkill;
                SkillObject second = lf.SecondSkill;
                (Lifestyle, float) tuple = new(lf, 0f);

                if (first == DefaultSkills.Medicine || second == DefaultSkills.Medicine) tuple.Item2 += healerWeight;
                else if (first == DefaultSkills.Engineering || second == DefaultSkills.Engineering) tuple.Item2 += siegeWeight;
                else if (first == DefaultSkills.Trade || second == DefaultSkills.Trade) tuple.Item2 += merchantWeight;
                else if (first == DefaultSkills.Leadership || second == DefaultSkills.Leadership || 
                    first == BKSkills.Instance.Lordship || second == BKSkills.Instance.Lordship) tuple.Item2 += politicianWeight;
                else if (first == DefaultSkills.Roguery || second == DefaultSkills.Roguery)
                {
                    if (hero.Clan != null && hero.Clan.IsMinorFaction) tuple.Item2 += mercenaryWeight;
                    tuple.Item2 += rogueWeight;
                }
                else tuple.Item2 += warriorWeight;

                if (lf.Culture == hero.Culture && tuple.Item2 != 0f) tuple.Item2 += 1f;

                candidates.Add(tuple);
            }

            return MBRandom.ChooseWeighted(candidates);
        }

        public bool AcceptNotableAid(Clan clan, PopulationData data)
        {
            Kingdom kingdom = clan.Kingdom;
            return data.Stability >= 0.5f && data.NotableSupport.ResultNumber >= 0.5f && kingdom != null &&
                    FactionManager.GetEnemyFactions(kingdom).Count() > 0 && clan.Influence > 50f * clan.Tier;
        }

        public void SettlementManagement(Settlement target)
        {
            if (BannerKingsConfig.Instance.PopulationManager == null || !BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(target))
                return;

            if (target.OwnerClan == Clan.PlayerClan) return;
            
            Kingdom kingdom = target.OwnerClan.Kingdom;
            List<BannerKingsDecision> currentDecisions = BannerKingsConfig.Instance.PolicyManager.GetDefaultDecisions(target);
            List<BannerKingsDecision> changedDecisions = new List<BannerKingsDecision>();

            Town town = target.Town;
            if (town != null && town.Governor != null)
            {
                if (town.FoodStocks < (float)town.FoodStocksUpperLimit() * 0.2f && town.FoodChange < 0f)
                {
                    BKRationDecision rationDecision = (BKRationDecision)currentDecisions.FirstOrDefault(x => x.GetIdentifier() == "decision_ration");
                    rationDecision.Enabled = true;
                    changedDecisions.Add(rationDecision);
                }
                else
                {
                    BKRationDecision rationDecision = (BKRationDecision)currentDecisions.FirstOrDefault(x => x.GetIdentifier() == "decision_ration");
                    rationDecision.Enabled = false;
                    changedDecisions.Add(rationDecision);
                }

                MobileParty garrison = town.GarrisonParty;
                if (garrison != null)
                {
                    float wage = garrison.TotalWage;
                    float income = Campaign.Current.Models.SettlementTaxModel.CalculateTownTax(town).ResultNumber;
                    if (wage >= income * 0.5f)
                        BannerKingsConfig.Instance.PolicyManager.UpdateSettlementPolicy(target, new BKGarrisonPolicy(BKGarrisonPolicy.GarrisonPolicy.Dischargement, target));
                    else if (wage <= income * 0.2f)
                        BannerKingsConfig.Instance.PolicyManager.UpdateSettlementPolicy(target, new BKGarrisonPolicy(BKGarrisonPolicy.GarrisonPolicy.Enlistment, target));
                    else BannerKingsConfig.Instance.PolicyManager.UpdateSettlementPolicy(target, new BKGarrisonPolicy(BKGarrisonPolicy.GarrisonPolicy.Standard, target));
                }

                if (town.LoyaltyChange < 0) UpdateTaxPolicy(1, target);
                else UpdateTaxPolicy(-1, target);

                if (kingdom != null)
                {
                    IEnumerable<Kingdom> enemies = FactionManager.GetEnemyKingdoms(kingdom);
                    bool atWar = enemies.Count() > 0;

                    if (target.Owner.GetTraitLevel(DefaultTraits.Calculating) > 0)
                    {
                        BKSubsidizeMilitiaDecision subsidizeMilitiaDecision = (BKSubsidizeMilitiaDecision)currentDecisions.FirstOrDefault(x => x.GetIdentifier() == "decision_militia_subsidize");
                        subsidizeMilitiaDecision.Enabled = atWar ? true : false;
                        changedDecisions.Add(subsidizeMilitiaDecision);
                    }
                }

                BKCriminalPolicy criminal = (BKCriminalPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(target, "criminal");
                int mercy = target.Owner.GetTraitLevel(DefaultTraits.Mercy);
                BKCriminalPolicy targetCriminal = null;

                if (mercy > 0) targetCriminal = new BKCriminalPolicy(BKCriminalPolicy.CriminalPolicy.Forgiveness, target);
                else if (mercy < 0) targetCriminal = new BKCriminalPolicy(BKCriminalPolicy.CriminalPolicy.Execution, target);
                else targetCriminal = new BKCriminalPolicy(BKCriminalPolicy.CriminalPolicy.Enslavement, target);

                if (targetCriminal.Policy != criminal.Policy)
                    BannerKingsConfig.Instance.PolicyManager.UpdateSettlementPolicy(target, targetCriminal);

                BKTaxSlavesDecision taxSlavesDecision = (BKTaxSlavesDecision)currentDecisions.FirstOrDefault(x => x.GetIdentifier() == "decision_slaves_tax");
                if (target.Owner.GetTraitLevel(DefaultTraits.Authoritarian) > 0)
                    taxSlavesDecision.Enabled = true;
                else if (target.Owner.GetTraitLevel(DefaultTraits.Egalitarian) > 0)
                    taxSlavesDecision.Enabled = false;
                changedDecisions.Add(taxSlavesDecision);

                BKWorkforcePolicy workforce = (BKWorkforcePolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(target, "workforce");
                List<ValueTuple<WorkforcePolicy, float>> workforcePolicies = new List<ValueTuple<WorkforcePolicy, float>>();
                workforcePolicies.Add((WorkforcePolicy.None, 1f));
                float saturation = BannerKingsConfig.Instance.PopulationManager.GetPopData(target).LandData.WorkforceSaturation;
                if (saturation > 1f)
                    workforcePolicies.Add((WorkforcePolicy.Land_Expansion, 2f));
                if (town.Security < 20f)
                    workforcePolicies.Add((WorkforcePolicy.Martial_Law, 2f));
                BannerKingsConfig.Instance.PolicyManager.UpdateSettlementPolicy(target, new BKWorkforcePolicy(MBRandom.ChooseWeighted(workforcePolicies), target));

                foreach (BannerKingsDecision dec in changedDecisions)
                    BannerKingsConfig.Instance.PolicyManager.UpdateSettlementDecision(target, dec);
            }
            else if (target.IsVillage && target.Village.MarketTown.Governor != null)
            {
                VillageData villageData = BannerKingsConfig.Instance.PopulationManager.GetPopData(target).VillageData;
                villageData.StartRandomProject();
                float hearths = target.Village.Hearth;
                if (hearths < 300f) UpdateTaxPolicy(-1, target);
                else if (hearths > 1000f) UpdateTaxPolicy(1, target);
            }
        }

        private static void UpdateTaxPolicy(int value, Settlement settlement)
        {
            BKTaxPolicy tax = ((BKTaxPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(settlement, "tax"));
            TaxType taxType = tax.Policy;
            if ((value == 1 && taxType != TaxType.High) || value == -1 && taxType != TaxType.Low)
                BannerKingsConfig.Instance.PolicyManager.UpdateSettlementPolicy(settlement, new BKTaxPolicy(taxType + value, settlement));

        }
    }
}
