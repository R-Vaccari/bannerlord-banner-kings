using BannerKings.Behaviours.Diplomacy;
using BannerKings.Behaviours.Diplomacy.Wars;
using BannerKings.Extensions;
using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Institutions.Religions.Faiths;
using BannerKings.Managers.Titles.Governments;
using BannerKings.Managers.Titles;
using BannerKings.Utils.Models;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using System;
using TaleWorlds.Core;
using BannerKings.Behaviours.Mercenary;
using BannerKings.Models.Vanilla.Abstract;
using BannerKings.CampaignContent.Traits;
using BannerKings.Managers.Skills;
using BannerKings.Settings;

namespace BannerKings.Models.Vanilla
{
    public class BKDiplomacyModel : DiplomacyModel
    {
        public override int MaxNeutralRelationLimit => BannerKingsSettings.Instance.FriendlyThreshold;
        public override int MinNeutralRelationLimit => BannerKingsSettings.Instance.HostileThreshold;

        public override int GetCharmExperienceFromRelationGain(Hero hero, float relationChange, ChangeRelationAction.ChangeRelationDetail detail)
        {
            int xp = base.GetCharmExperienceFromRelationGain(hero, relationChange, detail);
            if (xp < BannerKingsSettings.Instance.CharmXpThreshold) 
                return 0;

            if (xp > 0)
                xp = (int)MathF.Max(1f, xp * BannerKingsSettings.Instance.CharmXpMultiplier);

            return xp;
        }

        public override ExplainedNumber WillMercenaryDeclareWar(Kingdom kingdom, Clan firedMercenary, bool explanations = false)
        {
            ExplainedNumber cost = new ExplainedNumber(firedMercenary.Leader.GetTraitLevel(DefaultTraits.Honor) * -50f,
                explanations,
                new TextObject("{=vrm5pNf3}Honor of {HERO}")
                .SetTextVariable("HERO", firedMercenary.Leader.Name));

            float strength = firedMercenary.TotalStrength;
            if (strength < (kingdom.TotalStrength - strength) * 0.1f)
                cost.Add(-1000f, new TextObject("{=!}{CLAN} is too weak in relation to {KINGDOM}")
                    .SetTextVariable("CLAN", firedMercenary.Name)
                    .SetTextVariable("KINGDOM", kingdom.Name));

            if (firedMercenary.IsOutlaw)
                cost.Add(100f, new TextObject("{=!}{CLAN} are outlaws")
                        .SetTextVariable("CLAN", firedMercenary.Name)
                        .SetTextVariable("KINGDOM", kingdom.Name));

            if (firedMercenary.IsMafia)
                cost.Add(50f, new TextObject("{=!}{CLAN} are outlaws")
                        .SetTextVariable("CLAN", firedMercenary.Name)
                        .SetTextVariable("KINGDOM", kingdom.Name));

            float desire = GetScoreOfMercenaryToJoinKingdom(firedMercenary, kingdom) -
                GetScoreOfMercenaryToLeaveKingdom(firedMercenary, kingdom);
            cost.Add(desire, new TextObject("{=!}Desire to leave {KINGDOM}")
                        .SetTextVariable("KINGDOM", kingdom.Name));

            return cost;
        }

        public override ExplainedNumber GetRightInnfluenceCost(ContractRight right, Hero suzerain, Hero vassal)
        {
            ExplainedNumber cost = new ExplainedNumber(right.Influence);

            return cost;
        }

        public override ExplainedNumber WillSuzerainAcceptRight(ContractRight right, Hero suzerain, Hero vassal)
        {
            ExplainedNumber cost = new ExplainedNumber(-10f);
            cost.Add(suzerain.GetRelation(vassal), new TextObject("{=aPEQXOTV}Relationship with {HERO}")
                .SetTextVariable("HERO", vassal.Name));

            return cost;
        }

        public override void AddProposeDiplomacyCostEffects(Hero proposer, ref ExplainedNumber result)
        {
            Utils.Helpers.ApplyTraitEffect(proposer, DefaultTraitEffects.Instance.CalculatingProposals, ref result);
            Utils.Helpers.ApplyPerk(BKPerks.Instance.LordshipSenateOrator, proposer, ref result, false);
        }

        public override int GetInfluenceCostOfAnnexation(Clan proposingClan) =>
            MathF.Round(GetAnnexationCostExplained(proposingClan).ResultNumber);

        public override ExplainedNumber GetAnnexationCostExplained(Clan proposingClan, Town town = null)
        {
            ExplainedNumber cost = new ExplainedNumber(200f);
            cost.Add(BannerKingsConfig.Instance.InfluenceModel.CalculateInfluenceCap(proposingClan).ResultNumber * 0.2f,
                new TextObject("{=wwYABLRd}Clan Influence Limit"));

            if (proposingClan.Kingdom != null)
            {
                if (proposingClan.Kingdom.ActivePolicies.Contains(DefaultPolicies.FeudalInheritance))
                {
                    cost.AddFactor(1f);
                }

                if (proposingClan.Kingdom.ActivePolicies.Contains(DefaultPolicies.PrecarialLandTenure) && proposingClan == proposingClan.Kingdom.RulingClan)
                {
                    cost.AddFactor(-0.5f);
                }
            }

            if (town != null) 
            {
                if (town.IsOwnerUnassigned) cost.AddFactor(-0.5f, new TextObject("{=!}Contested fief"));
            }

            GetPerkEffectsOnKingdomDecisionInfluenceCost(proposingClan, ref cost);
            AddProposeDiplomacyCostEffects(proposingClan.Leader, ref cost);
            return cost;
        }

        public override int GetInfluenceCostOfExpellingClan(Clan proposingClan) =>
            MathF.Round(GetInfluenceCostOfExpellingClanExplained(proposingClan).ResultNumber);

        public ExplainedNumber GetInfluenceCostOfExpellingClanExplained(Clan proposingClan)
        {
            ExplainedNumber cost = new ExplainedNumber(200f);
            cost.Add(BannerKingsConfig.Instance.InfluenceModel.CalculateInfluenceCap(proposingClan).ResultNumber * 0.2f,
                new TextObject("{=wwYABLRd}Clan Influence Limit"));

            GetPerkEffectsOnKingdomDecisionInfluenceCost(proposingClan, ref cost);
            AddProposeDiplomacyCostEffects(proposingClan.Leader, ref cost);
            return cost;
        }

        public override int GetInfluenceCostOfProposingWar(Clan proposingClan) =>
            MathF.Round(GetInfluenceCostOfProposingWarExplained(proposingClan).ResultNumber);

        public ExplainedNumber GetInfluenceCostOfProposingWarExplained(Clan proposingClan)
        {
            ExplainedNumber cost = new ExplainedNumber(200f);
            if (proposingClan.Kingdom.ActivePolicies.Contains(DefaultPolicies.WarTax) && proposingClan == proposingClan.Kingdom.RulingClan)
            {
                cost.AddFactor(1f);
            }

            cost.Add(BannerKingsConfig.Instance.InfluenceModel.CalculateInfluenceCap(proposingClan).ResultNumber * 0.2f,
                new TextObject("{=wwYABLRd}Clan Influence Limit"));

            GetPerkEffectsOnKingdomDecisionInfluenceCost(proposingClan, ref cost);
            AddProposeDiplomacyCostEffects(proposingClan.Leader, ref cost);
            return cost;
        }

        protected void GetPerkEffectsOnKingdomDecisionInfluenceCost(Clan proposingClan, ref ExplainedNumber cost)
        {
            if (proposingClan.Leader.GetPerkValue(DefaultPerks.Charm.Firebrand))
            {
                cost.AddFactor(DefaultPerks.Charm.Firebrand.PrimaryBonus, DefaultPerks.Charm.Firebrand.Name);
            }
        }

        public override int GetInfluenceCostOfProposingPeace(Clan proposingClan) =>
            MathF.Round(GetInfluenceCostOfProposingPeaceExplained(proposingClan).ResultNumber);

        public ExplainedNumber GetInfluenceCostOfProposingPeaceExplained(Clan proposingClan)
        {
            ExplainedNumber cost = new ExplainedNumber(100f);
            cost.Add(BannerKingsConfig.Instance.InfluenceModel.CalculateInfluenceCap(proposingClan).ResultNumber * 0.2f,
                new TextObject("{=wwYABLRd}Clan Influence Limit"));

            GetPerkEffectsOnKingdomDecisionInfluenceCost(proposingClan, ref cost);
            AddProposeDiplomacyCostEffects(proposingClan.Leader, ref cost);
            return cost;
        }

        public override float GetScoreOfKingdomToHireMercenary(Kingdom kingdom, Clan mercenaryClan) =>
            KingdomRecruitMercenary(kingdom, mercenaryClan).ResultNumber;

        public ExplainedNumber KingdomRecruitMercenary(Kingdom kingdom, Clan mercenaryClan, bool explanations = false)
        {
            int commanders = 0;
            foreach (Clan clan in kingdom.Clans)
                commanders += clan.CommanderLimit;

            float baseResult = (kingdom.Fiefs.Count * 4f) - (kingdom.Fiefs.Count * commanders) * 12f;
            ExplainedNumber result = new ExplainedNumber(MathF.Max(baseResult, 0f), 
                explanations,
                new TextObject("{=!}{KINGDOM} needs more fighting forces").SetTextVariable("KINGDOM", kingdom.Name));
            float strength = kingdom.TotalStrength;
            if (mercenaryClan.Kingdom == kingdom)
                strength -= mercenaryClan.TotalStrength;

            var enemies = FactionManager.GetEnemyKingdoms(kingdom);
            foreach (Kingdom enemy in enemies)
                result.Add(((enemy.TotalStrength * 1.1f) - strength) * 0.1f, new TextObject("{=!}War against {KINGDOM}")
                    .SetTextVariable("KINGDOM", enemy.Name));

            if (enemies.IsEmpty()) result.Add(-20f, new TextObject("{=!}No wars being fought"));
            float baseNumber = MathF.Abs(result.BaseNumber);

            if (kingdom.KingdomBudgetWallet > 100000) result.Add(baseNumber * 0.1f, new TextObject("{=!}{KINGDOM} has significant budget for sellswords")
                    .SetTextVariable("KINGDOM", kingdom.Name));
            else if (kingdom.KingdomBudgetWallet > 50000) result.Add(baseNumber * 0.05f, new TextObject("{=!}{KINGDOM} has extra budget for sellswords")
                    .SetTextVariable("KINGDOM", kingdom.Name));

            Hero ruler = kingdom.RulingClan.Leader;
            if (mercenaryClan.IsOutlaw)
            {
                result.Add(baseNumber * (ruler.GetTraitLevel(DefaultTraits.Honor) * -0.2f), 
                    new TextObject("{=!}{HERO}'s opinion of outlaws")
                    .SetTextVariable("HERO", ruler.Name));
            }

            float zealotry = 1f;
            if (mercenaryClan.IsSect)
            {
                zealotry += ruler.GetTraitLevel(BKTraits.Instance.Zealous) * 0.2f;
            }

            if (mercenaryClan.IsMafia)
            {
                result.Add(baseNumber * (ruler.GetTraitLevel(DefaultTraits.Generosity) * 0.2f),
                    new TextObject("{=!}{HERO}'s opinion of mafias")
                    .SetTextVariable("HERO", ruler.Name));
            }

            result.Add(baseNumber * (ruler.GetTraitLevel(BKTraits.Instance.Humble) * 0.1f),
                    new TextObject("{=!}Personality trait ({TRAIT})")
                    .SetTextVariable("TRAIT", BKTraits.Instance.Humble.Name));

            Religion rulerReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(kingdom.RulingClan.Leader);
            Religion mercenaryReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(mercenaryClan.Leader);

            if (rulerReligion != null)
            {
                if (mercenaryReligion != null)
                {
                    FaithStance stance = rulerReligion.GetStance(mercenaryReligion.Faith);
                    if (stance == FaithStance.Tolerated)
                    {
                        if (mercenaryClan.IsSect && rulerReligion.Faith.Equals(mercenaryReligion.Faith))
                            result.Add(baseNumber * (1f * zealotry) + 15f, new TextObject("{=!}Shared faith (Sect)"));
                        else result.Add(baseNumber * (0.25f * zealotry) + 15f, new TextObject("{=Pcy4iFnT}Shared faith"));
                    }
                    else if (stance == FaithStance.Untolerated) result.Add(baseNumber * (-0.3f * zealotry) - 25f, new TextObject("{=!}Faith differences"));
                    else result.Add(baseNumber * (-0.5f * zealotry) - 60f, new TextObject("{=!}Faith differences"));
                }
                else result.Add(baseNumber * (-0.3f * zealotry) - 25f, new TextObject("{=!}Faith differences"));
            }

            if (mercenaryClan.Culture == kingdom.Culture) result.Add(baseNumber * 0.2f, GameTexts.FindText("str_culture"));

            result.Add(baseNumber * (mercenaryClan.Leader.GetRelation(kingdom.RulingClan.Leader) * 0.01f),
                new TextObject("{=nnYfQnWv}{HERO1}`s opinion of {HERO2}")
                .SetTextVariable("HERO1", ruler.Name)
                .SetTextVariable("HERO2", mercenaryClan.Leader.Name));

            MercenaryCareer career = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKMercenaryCareerBehavior>()
                .GetCareer(mercenaryClan);

            if (career != null)
            {
                float factor = mercenaryClan.Tier switch
                {
                    < 2 => 0.0f,
                    < 3 => 0.15f,
                    < 4 => 0.3f,
                    _ => 0.5f
                };

                result.Add(baseNumber * (career.Reputation - factor), new TextObject("{=!}Reputation"));

                if (career.ContractDueDate.IsFuture)
                    result.Add(baseNumber * (career.ContractDueDate.RemainingDaysFromNow / CampaignTime.DaysInYear), 
                        new TextObject("{=!}Contract due date"));

                foreach (Town town in kingdom.Fiefs)
                    if (town.LastCapturedBy == mercenaryClan)
                        result.Add(baseNumber  * 0.15f, town.Name);
            }

            return result;
        }

        public override float GetScoreOfKingdomToSackMercenary(Kingdom kingdom, Clan mercenaryClan) =>
            KingdomSackMercenary(kingdom, mercenaryClan).ResultNumber;

        public ExplainedNumber KingdomSackMercenary(Kingdom kingdom, Clan mercenaryClan, bool explanations = false)
        {
            if (FactionManager.GetEnemyKingdoms(kingdom).Any() && kingdom.MercenaryWallet > 0)
                return new ExplainedNumber(0f);

            ExplainedNumber result = new ExplainedNumber(base.GetScoreOfKingdomToSackMercenary(kingdom, mercenaryClan), explanations);

            if (result.ResultNumber > 0)
            {
                MercenaryCareer career = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKMercenaryCareerBehavior>()
                    .GetCareer(mercenaryClan);

                if (career != null)
                {
                    if (career.ContractDueDate.IsFuture)
                        result.AddFactor(-0.8f, new TextObject("{=!}Contract due date"));
                }

                foreach (Town town in kingdom.Fiefs)
                    if (town.LastCapturedBy == mercenaryClan)
                        result.Add(-0.15f, town.Name);
            }

            return result;
        }

        public override ExplainedNumber GetMercenaryDownPayment(Clan mercenaryClan, Kingdom kingdom, bool explanations = false)
        {
            ExplainedNumber result = new ExplainedNumber(kingdom.RulingClan.Gold * 0.05f, 
                explanations, 
                new TextObject("{=!}Ruler's wealth"));
            result.LimitMin(0f);

            result.Add(mercenaryClan.TotalStrength * mercenaryClan.MercenaryAwardMultiplier / 2f, new TextObject("{=!}Military force of {CLAN}")
                .SetTextVariable("CLAN", mercenaryClan.Name));

            MercenaryCareer career = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKMercenaryCareerBehavior>().GetCareer(mercenaryClan);
            if (career != null)
            {
                result.AddFactor(career.Reputation - 1f, new TextObject("{=!}Mercenary reputation ({REPUTATION}%)")
                .SetTextVariable("REPUTATION", (career.Reputation * 100f).ToString())); 
            } 
            else result.AddFactor(-1f, new TextObject("{=!}Mercenary reputation ({REPUTATION}%)")
                .SetTextVariable("REPUTATION", 0f));

            if (mercenaryClan.IsSect) result.AddFactor(-0.8f, new TextObject("{=!}Sect"));
            Utils.Helpers.ApplyPerk(BKPerks.Instance.LordshipSellswordCareer, mercenaryClan.Leader, ref result);
            return result;
        }

        public ExplainedNumber GetMercenaryPietyCost(Clan mercenaryClan, Kingdom kingdom, bool explanations = false)
        {
            ExplainedNumber result = new ExplainedNumber(0, explanations);

            if (mercenaryClan.IsSect)
            {
                Hero ruler = kingdom.RulingClan.Leader;
                float pietyGain = BannerKingsConfig.Instance.ReligionModel.CalculatePietyChange(ruler).ResultNumber;
                result.Add(mercenaryClan.TotalStrength * mercenaryClan.MercenaryAwardMultiplier / 2f, new TextObject("{=!}Military force of {CLAN}")
                .SetTextVariable("CLAN", mercenaryClan.Name));

                result.Add(pietyGain * 500f, new TextObject("{=!}Piety"));
            }

            return result;
        }

        public override float GetScoreOfMercenaryToJoinKingdom(Clan mercenaryClan, Kingdom kingdom) =>
            MercenaryJoinScore(mercenaryClan, kingdom).ResultNumber;     

        public ExplainedNumber MercenaryJoinScore(Clan mercenaryClan, Kingdom kingdom, bool explanations = false)
        {
            if (mercenaryClan == Clan.PlayerClan) return new ExplainedNumber(0f, false);

            ExplainedNumber result = new ExplainedNumber(base.GetScoreOfMercenaryToJoinKingdom(mercenaryClan, kingdom), explanations);
            Religion mercenaryReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(mercenaryClan.Leader);
            Religion rulerReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(kingdom.RulingClan.Leader);

            if (mercenaryReligion != null && (rulerReligion == null || mercenaryReligion.GetStance(rulerReligion.Faith) == FaithStance.Tolerated))
            {
                if (mercenaryClan.IsSect) result.Add(1000f, new TextObject("{=!}Shared faith"));
                else if (!mercenaryClan.IsOutlaw) result.Add(400f, new TextObject("{=!}Shared faith"));

                Utils.Helpers.ApplyPerk(BKPerks.Instance.TheologySect, kingdom.RulingClan.Leader, ref result);
            }

            if (!mercenaryClan.IsOutlaw)
            {
                if (mercenaryClan.Culture == kingdom.Culture)
                {
                    result.Add(250f, GameTexts.FindText("str_culture"));
                }
            }

            float relationsFactor = 5f;
            if (mercenaryClan.IsOutlaw) relationsFactor = 7f;
            else if (mercenaryClan.IsSect) relationsFactor = 2.5f;

            result.Add(MathF.Max(mercenaryClan.Leader.GetRelation(kingdom.RulingClan.Leader), 0) * relationsFactor,
                new TextObject("{=nnYfQnWv}{HERO1}`s opinion of {HERO2}")
                .SetTextVariable("HERO1", mercenaryClan.Leader.Name)
                .SetTextVariable("HERO2", kingdom.RulingClan.Leader.Name));
            return result;
        }

        public override float GetScoreOfMercenaryToLeaveKingdom(Clan mercenaryClan, Kingdom kingdom)
        {
            float leave = MercenaryLeaveScore(mercenaryClan, kingdom).ResultNumber;
            return leave;
        }

        public override ExplainedNumber MercenaryLeaveScore(Clan mercenaryClan, Kingdom kingdom, bool explanations = false)
        {
            float num = 0.005f * MathF.Min(200f, mercenaryClan.LastFactionChangeTime.ElapsedDaysUntilNow);
            ExplainedNumber result = new ExplainedNumber(10000f * num - 5000f, explanations);
            MercenaryCareer career = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKMercenaryCareerBehavior>()
                .GetCareer(mercenaryClan);

            if (career != null)
            {
                if (career.ContractDueDate.IsFuture)
                    result.AddFactor(-0.8f, new TextObject("{=!}Contract due date"));

                Religion mercenaryReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(mercenaryClan.Leader);
                Religion rulerReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(kingdom.RulingClan.Leader);

                if (mercenaryReligion != null && (rulerReligion == null || mercenaryReligion.GetStance(rulerReligion.Faith) != FaithStance.Tolerated))
                {
                    if (mercenaryClan.IsSect) result.Add(1000f, new TextObject("{=!}Faith differences"));
                    else if (!mercenaryClan.IsOutlaw)
                    {
                        if (rulerReligion == null || mercenaryReligion.GetStance(rulerReligion.Faith) == FaithStance.Untolerated)
                            result.Add(150f, new TextObject("{=!}Faith differences"));
                        else result.Add(350f, new TextObject("{=!}Faith differences"));
                    }
                }

                if (!mercenaryClan.IsOutlaw)
                {
                    if (mercenaryClan.Culture != kingdom.Culture)
                    {
                        result.Add(100f, GameTexts.FindText("str_culture"));
                    }
                }

                float relationsFactor = 5f;
                if (mercenaryClan.IsOutlaw) relationsFactor = 7f;
                else if (mercenaryClan.IsSect) relationsFactor = 2.5f;

                result.Add(MathF.Min(mercenaryClan.Leader.GetRelation(kingdom.RulingClan.Leader), 0) * relationsFactor,
                    new TextObject("{=nnYfQnWv}{HERO1}`s opinion of {HERO2}")
                    .SetTextVariable("HERO1", mercenaryClan.Leader.Name)
                    .SetTextVariable("HERO2", kingdom.RulingClan.Leader.Name));
            }
            
            return result;
        }

        public override ExplainedNumber CalculateHeroFiefScore(Settlement settlement, Hero annexing, bool explanations = false)
        {
            ExplainedNumber result = new ExplainedNumber(0f, explanations);
            Clan clan = annexing.Clan;

            /*if (BannerKingsConfig.Instance.ReligionsManager.HasBlessing(annexing, DefaultDivinities.Instance.AseraMain))
            {
                result.AddFactor(0.2f, DefaultDivinities.Instance.AseraMain.Name);
            }*/

            if (settlement.Owner == annexing)
            {
                result.Add(150f, new TextObject("{=CfiavKU4}{HERO} is the established owner of {FIEF}")
                    .SetTextVariable("HERO", clan.Leader.Name)
                    .SetTextVariable("FIEF", settlement.Name));
            }

            FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetTitle(settlement);
            if (title != null)
            {
                if (title.deJure == clan.Leader)
                {
                    result.Add(400f, new TextObject("{=OQmLP0qp}{HERO} is de jure holder of {TITLE}")
                        .SetTextVariable("HERO", clan.Leader.Name)
                        .SetTextVariable("TITLE", title.FullName));
                }
                else if (title.HeroHasValidClaim(clan.Leader))
                {
                    result.Add(250f, new TextObject("{=FUyGdaRm}{HERO} is a legal claimant of {TITLE}")
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
                            result.Add(1000f, new TextObject("{=sAb8WSFG}Last conquered by {CLAN} ({LAW})")
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
                            result.Add(1000f, new TextObject("{=OQmLP0qp}{HERO} is de jure holder of {TITLE} ({LAW})")
                                .SetTextVariable("HERO", clan.Leader.Name)
                                .SetTextVariable("TITLE", title.FullName)
                                .SetTextVariable("LAW", DefaultContractAspects.Instance.ConquestClaim.Name));
                        }
                        else if (title.HeroHasValidClaim(clan.Leader))
                        {
                            result.Add(500f, new TextObject("{=Tk8HfVjp}{HERO} is a claimant of {TITLE} ({LAW})")
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
                        if (fief.IsTown) result.Add(-150f, new TextObject("{=ow8Bo8qm}Owns {FIEF} ({LAW})")
                            .SetTextVariable("FIEF", fief.Name)
                            .SetTextVariable("LAW", DefaultContractAspects.Instance.ConquestDistributed.Name));
                        else if (fief.IsCastle) result.Add(-75f, new TextObject("{=ow8Bo8qm}Owns {FIEF} ({LAW})")
                            .SetTextVariable("FIEF", fief.Name)
                            .SetTextVariable("LAW", DefaultContractAspects.Instance.ConquestDistributed.Name));
                        else if (fief.IsVillage && fief.Village.GetActualOwner() == annexing)
                            result.Add(-30f, new TextObject("{=ow8Bo8qm}Owns {FIEF} ({LAW})")
                            .SetTextVariable("FIEF", fief.Name)
                            .SetTextVariable("LAW", DefaultContractAspects.Instance.ConquestDistributed.Name));
                    }
                }
            }

            var limit = BannerKingsConfig.Instance.StabilityModel.CalculateDemesneLimit(clan.Leader).ResultNumber;
            var current = BannerKingsConfig.Instance.StabilityModel.CalculateCurrentDemesne(clan).ResultNumber;
            float factor = current / limit;
            result.Add(500f * (1f - factor), new TextObject("{=P7tvtWh6}Current Demesne Limit {CURRENT}/{LIMIT}")
                .SetTextVariable("CURRENT", current.ToString("0.0"))
                .SetTextVariable("LIMIT", limit.ToString("0.0")));

            return result;
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

        public override ExplainedNumber WillJoinWar(IFaction attacker, IFaction defender, IFaction ally,
            DeclareWarAction.DeclareWarDetail detail, bool explanations = false)
        {
            ExplainedNumber result = new ExplainedNumber(0, explanations);
            Clan allyClan = ally.IsClan ? (ally as Clan) : (ally as Kingdom).RulingClan;
            Clan defenderClan = defender.IsClan ? (defender as Clan) : (defender as Kingdom).RulingClan;

            if (ally.IsAtWarWith(attacker))
                result.Add(-1000f, new TextObject("{=LebeA6tg}Already at war with {FACTION}")
                    .SetTextVariable("FACTION", attacker.Name));

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

        public override ExplainedNumber GetPactInfluenceCost(Kingdom proposer, Kingdom proposed, bool explanations = false)
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
           
            result.AddFactor(-peace / 100000f, new TextObject("{=hAAOEqaJ}Peace interest"));
            AddProposeDiplomacyCostEffects(proposer.Leader, ref result);
            return result;
        }

        public override bool IsTruceAcceptable(Kingdom proposer, Kingdom proposed, bool explanations = false)
        {
            if (proposed == proposer) return false;
            
            float peace = GetScoreOfDeclaringPeace(proposed, proposer, proposed, out TextObject reason);
            return peace > 0;
        }

        public override bool WillAcceptTrade(Kingdom proposer, Kingdom proposed, bool explanations = false) => 
            GetTradeDesire(proposer, proposed, explanations).ResultNumber > 0f;

        public override ExplainedNumber GetTruceDenarCost(Kingdom proposer, Kingdom proposed, float years = 3f, bool explanations = false)
        {
            ExplainedNumber result = new ExplainedNumber(0, explanations);
            float peace = GetScoreOfDeclaringPeace(proposed, proposer, proposed, out TextObject reason) / 2f;
            result.Add((100000f - peace) * MathF.Sqrt(years), new TextObject("{=PsRfxMEv}Truce duration"));

            float relation = proposed.RulingClan.Leader.GetRelation(proposer.RulingClan.Leader) / 150f;
            result.AddFactor(-relation, new TextObject("{=BlidMNGT}Relation"));

            return result;
        }

        public override void AddAmicablePactDesireEffects(Kingdom proposer, Kingdom proposed, ref ExplainedNumber result, bool explanations = false)
        {
            result.Add(proposer.RulingClan.Leader.GetTraitLevel(DefaultTraits.Honor) * 15f,
                new TextObject("{=vrm5pNf3}Honor of {HERO}")
                .SetTextVariable("HERO", proposer.RulingClan.Leader.Name));

            Utils.Helpers.ApplyTraitEffect(proposer.RulingClan.Leader, DefaultTraitEffects.Instance.HonorDiplomacy, ref result);
            Utils.Helpers.ApplyPerk(BKPerks.Instance.LordshipDiplomaticTies, proposer.Leader, ref result);
        }

        public override ExplainedNumber GetTradeDesire(Kingdom proposer, Kingdom proposed, bool explanations = false)
        {
            ExplainedNumber result = new ExplainedNumber(0, explanations);
            result.Add(-50f, new TextObject("{=Gq5BnNiN}Reluctance"));

            result.Add(proposed.Leader.GetTraitLevel(DefaultTraits.Generosity) * 10f, 
                new TextObject("{=wMius2i9}{TITLE} of {NAME}")
                .SetTextVariable("TITLE", DefaultTraits.Generosity.Name)
                .SetTextVariable("NAME", proposed.Leader.Name));

            float relation = proposed.RulingClan.Leader.GetRelation(proposer.RulingClan.Leader);
            result.Add(relation / 3f, new TextObject("{=BlidMNGT}Relation"));

            AddAmicablePactDesireEffects(proposer, proposed, ref result, explanations);
            return result;
        }

        public override ExplainedNumber GetAllianceDesire(Kingdom proposer, Kingdom proposed, bool explanations = false)
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

            AddAmicablePactDesireEffects(proposer, proposed, ref result, explanations);
            return result;
        }

        public override bool WillAcceptAlliance(Kingdom proposer, Kingdom proposed) => GetAllianceDesire(proposer, proposed).ResultNumber > 0f;

        public override ExplainedNumber GetAllianceDenarCost(Kingdom proposer, Kingdom proposed, bool explanations = false)
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

        public override ExplainedNumber GetTradePactInfluenceCost(Kingdom proposer, Kingdom proposed, bool explanations = false)
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
            result.Add(cap, new TextObject("{=1RD1OWYP}Influence limit of {CLAN}")
                .SetTextVariable("CLAN", proposer.RulingClan.Name));

            float peace = GetScoreOfDeclaringPeace(proposed, proposer, proposed, out TextObject reason);
            result.AddFactor(peace / -60000f, new TextObject("{=hAAOEqaJ}Peace interest"));
            AddProposeDiplomacyCostEffects(proposer.Leader, ref result);
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

        public override ExplainedNumber GetScoreOfDeclaringWar(IFaction factionDeclaresWar, IFaction factionDeclaredWar, IFaction evaluatingClan,
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
                        result.Add(MathF.Abs(baseNumber) * - 0.25f, new TextObject("{=DPK2KdUk}Trade pact between both realms"));
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

                        if (justifications.Count == 0)
                        {
                            result.Add(-2000f, new TextObject("{=!}No war justifications"));
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
                    result.Add(MathF.Abs(baseNumber) * -diplomacy.Fatigue, new TextObject("{=Rdmm1Kmh}General war fatigue of {FACTION}")
                        .SetTextVariable("FACTION", diplomacy.Kingdom.Name));
                }

                foreach (Kingdom enemyKingdom in FactionManager.GetEnemyKingdoms(attackerKingdom))
                {
                    if (enemyKingdom != attackerKingdom && enemyKingdom != defenderKingdom)
                    {
                        WarStats enemyStats = CalculateWarStats(factionDeclaresWar, enemyKingdom);
                        float enemyScore = enemyStats.Strength + enemyStats.ValueOfSettlements - (enemyStats.TotalStrengthOfEnemies * 1.25f);
                        float f = 2f - (attackerScore / (enemyScore * 5f));
                        result.Add(-MathF.Abs(baseNumber) * f, new TextObject("{=epNrP2AT}Existing war with {FACTION}")
                        .SetTextVariable("FACTION", enemyKingdom.Name));
                    }
                }
            }

            if (factionDeclaresWar.IsKingdomFaction)
            {
                var tributes = factionDeclaresWar.Stances.ToList().FindAll(x => x.GetDailyTributePaid(x.Faction2) > 0);
                int tributeCount = tributes.Count;
                result.Add(MathF.Abs(baseNumber) * -0.1f * tributeCount, new TextObject("{=TCVWRr8K}Paying tributes (x{COUNT})")
                    .SetTextVariable("COUNT", tributeCount));
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


            if (factionDeclaresWar.Fiefs.Count == 1 || factionDeclaredWar.TotalStrength >= factionDeclaresWar.TotalStrength * 1.4f)
            {
                result.Add(-MathF.Abs(baseNumber) * 1.2f, new TextObject("{=fvd0nAa3}Defensive stance against {FACTION}")
                    .SetTextVariable("FACTION", factionDeclaredWar.Name));
            }

            if (evaluatingClan != null)
            {
                float relations = evaluatingClan.Leader.GetRelation(factionDeclaredWar.Leader);
                result.Add(MathF.Abs(baseNumber) * (-relations / 100f), new TextObject("{=nnYfQnWv}{HERO1}`s opinion of {HERO2}")
                    .SetTextVariable("HERO1", evaluatingClan.Leader.Name)
                .SetTextVariable("HERO2", factionDeclaredWar.Leader.Name));
            }

            float threatFactor = CalculateThreatFactor(factionDeclaresWar, factionDeclaredWar);
            result.Add(MathF.Abs(baseNumber) * threatFactor * 2f, new TextObject("{=ew3Ga8Lu}{THREAT}% threat relative to possible enemies")
                .SetTextVariable("THREAT", (threatFactor * 100f).ToString("0.0")));

            float attackerStrength = factionDeclaresWar.TotalStrength;
            float defenderStrength = factionDeclaredWar.TotalStrength;
            foreach (IFaction ally in factionDeclaredWar.GetAllies())
            {
                defenderStrength += ally.TotalStrength / 2f;
            }

            float strengthFactor = (attackerStrength / defenderStrength) - 1f;
            result.Add(MathF.Abs(baseNumber) * MathF.Clamp(strengthFactor * 0.6f, -2f, 0.5f), new TextObject("{=KcLdYKrY}Difference in strength"));

            /*if (factionDeclaredWar.Fiefs.Count < factionDeclaresWar.Fiefs.Count / 2f)
            {
                float fiefFactor = factionDeclaredWar.Fiefs.Count / factionDeclaresWar.Fiefs.Count;
                result.Add(-MathF.Abs(baseNumber) * (2f - fiefFactor), new TextObject("{=SRN3KdjF}Unworthy opponent"));
            }*/

            if (defenderStrength >= attackerStrength * 1.5f)
            {
                result.Add(MathF.Abs(baseNumber) * MathF.Clamp(strengthFactor * 0.5f, -5f, -0.4f), new TextObject("{=Z7AW5i79}Enemy significantly stronger"));
            }

            float attackerFiefs = factionDeclaresWar.Fiefs.Count;
            float defenderFiefs = factionDeclaredWar.Fiefs.Count;

            if (attackerFiefs == 1 || attackerFiefs == 2)
            {
                if (defenderFiefs > attackerFiefs)
                {
                    result.Add(-baseNumber, new TextObject("{=7ix3cKGX}{FACTION} should defend its few fiefs rather than attacking")
                        .SetTextVariable("FACTION", factionDeclaresWar.Name));
                }
            }

            float fiefsFactor = (attackerFiefs  / defenderFiefs) - 1f;
            result.Add(MathF.Abs(baseNumber) * MathF.Clamp(fiefsFactor * 0.1f, -2f, 2f), new TextObject("{=MvV0HUdo}Difference in controlled fiefs"));

            if (defenderFiefs >= attackerFiefs * 2f)
                result.Add(-MathF.Abs(baseNumber), new TextObject("{=bwVkTDdv}{FACTION1} controls more than twice the fiefs than {FACTION2}")
                    .SetTextVariable("FACTION1", factionDeclaredWar.Name)
                    .SetTextVariable("FACTION2", factionDeclaresWar.Name));

            War war = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKDiplomacyBehavior>().GetWar(factionDeclaredWar, factionDeclaresWar);
            if (war != null)
            {
                if (war.StartDate.ElapsedYearsUntilNow < 1f) result.Add(50000f, new TextObject("{=UaofTriA}Recently started war"));

                float score = MathF.Clamp(war.CalculateWarScore(war.Attacker, false).ResultNumber /
                    war.TotalWarScore.ResultNumber, -1f, 1f) * 2f;
                result.Add(MathF.Abs(baseNumber) * (war.Attacker == factionDeclaresWar ? -score : score));

                float fatigue = BannerKingsConfig.Instance.WarModel.CalculateFatigue(war, factionDeclaresWar).ResultNumber * 4f;
                result.Add(MathF.Abs(baseNumber) * -fatigue, new TextObject("{=Nxrd7yym}Fatigue over this war"));
            }
            else
            {
                if (stance.IsAtWar)
                {
                    bool isInAllyWar = false;
                    foreach (IFaction ally in factionDeclaresWar.GetAllies())
                    {
                        War allyWar = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKDiplomacyBehavior>().GetAllyWar(ally, factionDeclaredWar, factionDeclaresWar);
                        if (allyWar != null)
                        {
                            isInAllyWar = true;
                            if (war.StartDate.ElapsedYearsUntilNow < 1f) result.Add(50000f, new TextObject("{=UaofTriA}Recently started war"));

                            float score = MathF.Clamp(war.CalculateWarScore(war.Attacker, false).ResultNumber /
                                war.TotalWarScore.ResultNumber, -1f, 1f) * 2f;
                            result.Add(MathF.Abs(baseNumber) * (war.Attacker == factionDeclaresWar ? -score : score));

                            float fatigue = BannerKingsConfig.Instance.WarModel.CalculateFatigue(war, factionDeclaresWar).ResultNumber * 4f;
                            result.Add(MathF.Abs(baseNumber) * -fatigue, new TextObject("{=Nxrd7yym}Fatigue over this war"));
                        }
                    }
                }
                else
                {
                    ValueTuple<Settlement, Settlement> border = GetBorder(factionDeclaresWar, factionDeclaredWar);
                    if (border.Item1 != null && border.Item2 != null)
                    {
                        float distance = TaleWorlds.CampaignSystem.Campaign.Current.Models.MapDistanceModel.GetDistance(border.Item1, border.Item2);
                        float factor = (TaleWorlds.CampaignSystem.Campaign.AverageDistanceBetweenTwoFortifications / distance) - 1f;
                        float baseAbs = MathF.Abs(baseNumber);
                        result.Add(MathF.Clamp(baseAbs * factor * 2f, baseAbs * -2f, 0f), new TextObject("{=fiHYU8X3}Distance between realms"));
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
                result.Add(MathF.Abs(baseNumber) * (traits / 4f));

                float enemies = 1f;
                if (evaluating.Kingdom != null) enemies += FactionManager.GetEnemyKingdoms(evaluating.Kingdom).Count();

                int gold = (int)(leader.Gold / enemies);
                if (gold < 50000)
                {
                    result.Add(MathF.Abs(result.BaseNumber) * -0.8f);
                }
                else if (gold < 100000)
                {
                    result.Add(MathF.Abs(result.BaseNumber) * -0.4f);
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


