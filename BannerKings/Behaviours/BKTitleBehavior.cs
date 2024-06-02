using System;
using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers.Court;
using BannerKings.Managers.Helpers;
using BannerKings.Managers.Skills;
using BannerKings.Managers.Titles;
using BannerKings.UI.Notifications;
using HarmonyLib;
using SandBox.CampaignBehaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace BannerKings.Behaviours
{
    public class BKTitleBehavior : BannerKingsBehavior
    {
        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickHeroEvent.AddNonSerializedListener(this, OnDailyTickHero);
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
            CampaignEvents.BeforeHeroKilledEvent.AddNonSerializedListener(this, OnHeroKilled);
            CampaignEvents.OnClanDestroyedEvent.AddNonSerializedListener(this, OnClanDestroyed);
            CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener(this, OnOwnerChanged);
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void OnDailyTickHero(Hero hero)
        {
            if (hero == null || !hero.IsClanLeader || hero == Hero.MainHero || hero.Clan.Fiefs.Count == 0) return;
            

            if (hero.IsChild || hero.Occupation != Occupation.Lord || hero.Clan == null ||
                hero.Clan.IsMinorFaction || !BannerKingsConfig.Instance.TitleManager.IsHeroTitleHolder(hero))
            {
                return;
            }

            if (hero.Clan != null && hero == hero.Clan.Leader)
            {
                CheckOverDemesneLimit(hero);
                CheckOverUnlandedDemesneLimit(hero);
                //CheckOverVassalLimit(hero);
            }
        }

        private void CheckOverDemesneLimit(Hero hero)
        {
            var limit = BannerKingsConfig.Instance.StabilityModel.CalculateDemesneLimit(hero).ResultNumber;
            var current = BannerKingsConfig.Instance.StabilityModel.CalculateCurrentDemesne(hero.Clan).ResultNumber;

            if (current <= limit)
            {
                int xp = 0;
                foreach (FeudalTitle ownedTitle in BannerKingsConfig.Instance.TitleManager.GetAllDeJure(hero))
                {
                    if (ownedTitle.DeFacto == hero)
                    {
                        xp++;
                    }
                }

                hero.AddSkillXp(BKSkills.Instance.Lordship, xp);
                return;
            }

            if (hero == Hero.MainHero)
            {
                TaleWorlds.CampaignSystem.Campaign.Current.CampaignInformationManager.NewMapNoticeAdded(new DemesneLimitNotification());
                return;
            }

            var diff = current - limit;
            if (diff < 0.1f) return;  

            RunWeekly(() =>
            {
                var settlement = ChooseTitleToGive(hero);
                if (settlement == null) return;

                FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetTitle(settlement);
                Hero receiver = ChooseVassalToGiftLandedTitle(hero, title);
                if (receiver == null) return;

                if (title.deJure == hero)
                {
                    if (title.TitleType == TitleType.Lordship && BannerKingsConfig.Instance.StabilityModel.IsHeroOverVassalLimit(hero))
                        return;

                    var action = BannerKingsConfig.Instance.TitleModel.GetAction(ActionType.Grant, title, hero);
                    BannerKingsConfig.Instance.TitleManager.GrantTitle(action, receiver);
                }
                else if (settlement.Town != null) ChangeOwnerOfSettlementAction.ApplyByGift(settlement, receiver);
            },
            GetType().Name,
            false);
        }

        public float GetSettlementScore(Settlement settlement)
        {
            float result = 0f;

            if (settlement.IsVillage) result = 0.7f;
            else if (settlement.Town != null)
            {
                CouncilData data = BannerKingsConfig.Instance.CourtManager.GetCouncil(settlement.OwnerClan);
                if (data.Location == settlement.Town) return 0f;

                result = settlement.IsTown ? 0.1f : 0.5f;
            }

            if (settlement.LastThreatTime.ElapsedYearsUntilNow < 1f) result *= 1.3f;
            if (settlement.Culture != settlement.OwnerClan.Culture) result *= 1.5f;

            FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetTitle(settlement);
            if (title.deJure == settlement.OwnerClan.Leader) result *= 0.4f;

            return result;
        }

        private Settlement ChooseTitleToGive(Hero giver, bool landed = true)
        {
            var candidates = new List<(Settlement, float)>();
            foreach (Settlement settlement in giver.Clan.Settlements)
            {
                var value = GetSettlementScore(settlement);
                if (value <= 0f) continue;
                candidates.Add(new ValueTuple<Settlement, float>(settlement, value));    
            }

            return MBRandom.ChooseWeighted(candidates);
        }

        public Hero ChooseVassalToGiftLandedTitle(Hero giver, FeudalTitle titleToGive)
        {
            var candidates = new List<(Hero, float)>();

            if (titleToGive.TitleType != TitleType.Lordship)
            {
                var vassals = BannerKingsConfig.Instance.TitleManager.CalculateVassals(giver.Clan);
                foreach (var pair in vassals)
                {
                    var leader = pair.Key.Leader;
                    if (BannerKingsConfig.Instance.StabilityModel.IsHeroOverUnlandedDemesneLimit(leader))
                    {
                        continue;
                    }

                    float score = 100;
                    score += giver.GetRelation(leader);
                    if (pair.Value.Count > 0)
                    {
                        var type = titleToGive.TitleType;
                        foreach (var title in pair.Value)
                        {
                            if (title.TitleType == type)
                            {
                                score -= 60;
                            }
                            else if (title.TitleType < type)
                            {
                                score -= 120;
                            }
                            else if (titleToGive.Vassals.Contains(title))
                            {
                                score += 40;
                            }
                        }
                    }

                    if (BannerKingsConfig.Instance.TitleModel.GetGrantCandidates(giver).Contains(leader))
                    {
                        candidates.Add(new ValueTuple<Hero, float>(leader, score));
                    }
                }
            }
            else
            {
                foreach (var companion in giver.Clan.Companions)
                {
                    float score = 100;
                    score += companion.GetSkillValue(DefaultSkills.Leadership);
                    score += companion.GetSkillValue(DefaultSkills.Tactics);

                    candidates.Add(new ValueTuple<Hero, float>(companion, score));
                }
            }

            return MBRandom.ChooseWeighted(candidates);
        }

        private Hero ChooseVassalToGiftUnlandedTitle(Hero giver, FeudalTitle titleToGive, Dictionary<Clan, List<FeudalTitle>> vassals)
        {
            var candidates = new List<(Hero, float)>();
            foreach (var pair in vassals)
            {
                var leader = pair.Key.Leader;
                if (BannerKingsConfig.Instance.StabilityModel.IsHeroOverUnlandedDemesneLimit(leader))
                {
                    continue;
                }

                float score = 100;
                score += giver.GetRelation(leader);
                if (pair.Value.Count > 0)
                {
                    var type = titleToGive.TitleType;
                    foreach (var title in pair.Value)
                    {
                        if (title.TitleType == type)
                        {
                            score -= 60;
                        }
                        else if (title.TitleType < type)
                        {
                            score -= 120;
                        }
                        else if (titleToGive.Vassals.Contains(title))
                        {
                            score += 40;
                        }
                    }
                }

                if (BannerKingsConfig.Instance.TitleModel.GetGrantCandidates(giver).Contains(leader))
                {
                    candidates.Add(new ValueTuple<Hero, float>(leader, score));
                }
            }

            return MBRandom.ChooseWeighted(candidates);
        }

        private void CheckOverUnlandedDemesneLimit(Hero hero)
        {
            var limit = BannerKingsConfig.Instance.StabilityModel.CalculateUnlandedDemesneLimit(hero).ResultNumber;
            var current = BannerKingsConfig.Instance.StabilityModel.CalculateCurrentUnlandedDemesne(hero.Clan).ResultNumber;
            if (current <= limit)
            {
                return;
            }

            if (hero == Hero.MainHero)
            {
                TaleWorlds.CampaignSystem.Campaign.Current.CampaignInformationManager.NewMapNoticeAdded(new UnlandedDemesneLimitNotification());
            }

            var vassals = BannerKingsConfig.Instance.TitleManager.CalculateVassals(hero.Clan);
            if (vassals.Count == 0)
            {
                return;
            }

            var random = vassals.Keys.ToList().GetRandomElement();
            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(hero, random.Leader, -2);

            /*RunWeekly(() =>
            {
                var diff = current - limit;
                var title = ChooseTitleToGive(hero, diff, false);
                if (title == null || title.TitleType != TitleType.Dukedom) return;

                var receiver = ChooseVassalToGiftUnlandedTitle(hero, title, vassals);
                if (receiver == null) return;

                var action = BannerKingsConfig.Instance.TitleModel.GetAction(ActionType.Grant, title, hero);
                BannerKingsConfig.Instance.TitleManager.GrantTitle(action, receiver);
            },
            GetType().Name,
            false);*/
        }

        private void CheckOverVassalLimit(Hero hero)
        {
            if (!BannerKingsConfig.Instance.StabilityModel.IsHeroOverVassalLimit(hero))
            {
                return;
            }

            if (hero == Hero.MainHero)
            {
                TaleWorlds.CampaignSystem.Campaign.Current.CampaignInformationManager.NewMapNoticeAdded(new VassalLimitNotification());
            }
        }

        private void OnDailyTick()
        {
            if (BannerKingsConfig.Instance.TitleManager == null)
            {
                return;
            }

            foreach (Kingdom kingdom in Kingdom.All)
            {
                FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(kingdom);
                if (title != null)
                {
                    var prohibited = kingdom.ActivePolicies.ToList()
                        .FindAll(x => title.Contract.Government.ProhibitedPolicies.Contains(x));
                    foreach (PolicyObject policy in prohibited)
                    {
                        kingdom.RemovePolicy(policy);
                    }
                }
            }

            foreach (var duchy in BannerKingsConfig.Instance.TitleManager.GetAllTitlesByType(TitleType.Dukedom))
            {
                duchy.TickClaims();
                CheckClaimants(duchy);

                if (duchy.deJure == null) continue;

                var faction = duchy.deJure.Clan.Kingdom;
                if (faction == null || faction != duchy.DeFacto.Clan.Kingdom)
                {
                    continue;
                }

                var currentSovereign = duchy.Sovereign;
                var currentFactionSovereign = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(faction);
                if (currentSovereign == null || currentFactionSovereign == null ||
                    currentSovereign == currentFactionSovereign)
                {
                    continue;
                }

                duchy.TickDrift(currentFactionSovereign);
            }

            foreach (var kingdom in BannerKingsConfig.Instance.TitleManager.GetAllTitlesByType(TitleType.Kingdom))
            {
                kingdom.TickClaims();
                CheckClaimants(kingdom);

                if (kingdom.deJure == null) continue;

                var faction = kingdom.deJure.Clan.Kingdom;
                if (faction == null || faction != kingdom.DeFacto.Clan.Kingdom) continue;  

                var currentFactionSovereign = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(faction);
                if (currentFactionSovereign != null && currentFactionSovereign.TitleType == TitleType.Empire)
                {
                    kingdom.TickDrift(currentFactionSovereign);
                }
            }
        }

        private void CheckClaimants(FeudalTitle feudalTitle) 
        {
            var claimants = BannerKingsConfig.Instance.TitleModel.GetClaimants(feudalTitle);
            var actions = new List<TitleAction>(); 
            foreach (var claimant in claimants)
            {
                if (claimant.Key != Hero.MainHero)
                {
                    TitleAction action = BannerKingsConfig.Instance.TitleModel.GetAction(ActionType.Claim,
                        feudalTitle,
                        claimant.Key);
                    if (action.Possible && action.IsWilling) actions.Add(action);
                }
            }

            foreach (var action in actions) action.TakeAction(null);

            actions.Clear();
            foreach (var claim in feudalTitle.Claims)
            {
                if (claim.Key != Hero.MainHero && feudalTitle.HeroHasValidClaim(claim.Key))
                {
                    TitleAction action = BannerKingsConfig.Instance.TitleModel.GetAction(ActionType.Usurp,
                        feudalTitle,
                        claim.Key);
                    if (action.Possible && action.IsWilling) actions.Add(action);
                }
            }

            foreach (var action in actions) action.TakeAction(null);
        }

        private void OnHeroKilled(Hero victim, Hero killer, KillCharacterAction.KillCharacterActionDetail detail,
            bool showNotification = true)
        {
            if (victim?.Clan == null || BannerKingsConfig.Instance.TitleManager == null || victim == Hero.MainHero)
            {
                return;
            }

            BannerKingsConfig.Instance.TitleManager.RemoveKnights(victim);
            var titles = new List<FeudalTitle>(BannerKingsConfig.Instance.TitleManager.GetAllDeJure(victim));
            var sovereign = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(victim.Clan.Kingdom);
            if (sovereign != null && sovereign.Contract != null)
            {
                if (titles.Contains(sovereign))
                {
                    titles.Remove(sovereign);
                    SuccessionHelper.ApplySovereignSuccession(sovereign, victim, victim.Clan.Kingdom);
                }
            }

            InheritanceHelper.ApplyInheritanceAllTitles(titles, victim);
        }

        private void OnOwnerChanged(Settlement settlement, bool openToClaim, Hero newOwner, Hero oldOwner,
            Hero capturerHero,
            ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail detail)
        {
            var title = BannerKingsConfig.Instance.TitleManager.GetTitle(settlement);
            if (title == null)
            {
                return;
            }

            AddBarter(detail, settlement, oldOwner, newOwner, title);
            AddGift(detail, settlement, oldOwner, newOwner, title);
        }

        private void AddGift(ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail detail, Settlement settlement,
            Hero oldOwner, Hero newOwner, FeudalTitle title)
        {
            if (detail != ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail.ByGift)
            {
                return;
            }

            if (oldOwner == title.deJure)
            {
                BannerKingsConfig.Instance.TitleManager.InheritTitle(oldOwner, newOwner, title);
            }
        }

        private void AddBarter(ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail detail, Settlement settlement,
            Hero oldOwner, Hero newOwner, FeudalTitle title)
        {
            if (detail == ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail.ByBarter)
            {
                if (oldOwner == title.deJure)
                {
                    BannerKingsConfig.Instance.TitleManager.InheritTitle(oldOwner, newOwner, title);
                    if (!settlement.IsVillage)
                    {
                        foreach (var village in settlement.BoundVillages)
                        {
                            var villageTitle = BannerKingsConfig.Instance.TitleManager.GetTitle(village.Settlement);
                            if (villageTitle.deJure == oldOwner)
                            {
                                BannerKingsConfig.Instance.TitleManager.InheritTitle(oldOwner,
                                    newOwner,
                                    villageTitle);
                            }
                        }
                    }
                }

                return;
            }
        }

        private void OnClanDestroyed(Clan clan)
        {
            var titles = BannerKingsConfig.Instance.TitleManager.GetAllDeJure(clan);
            if (titles.Count > 0)
            {
                foreach (var title in titles)
                {
                    if (BannerKingsConfig.Instance.TitleManager.HasSuzerain(title))
                    {
                        var suzerain = BannerKingsConfig.Instance.TitleManager.GetImmediateSuzerain(title);
                        if (suzerain.deJure != null && suzerain.deJure.IsAlive && !clan.Heroes.Contains(suzerain.deJure))
                        {
                            BannerKingsConfig.Instance.TitleManager.InheritTitle(title.deJure, suzerain.deJure, title);
                            continue;
                        }
                    }

                    if (title.Sovereign != null && title.Sovereign.deJure != null)
                    {
                        if (title.Sovereign.deJure != title.deJure && title.Sovereign.deJure.IsAlive)
                        {
                            BannerKingsConfig.Instance.TitleManager.InheritTitle(title.deJure, title.Sovereign.deJure, title);
                            continue;
                        }
                    }

                    var kingdom = clan.Kingdom;
                    if (kingdom != null && !kingdom.IsEliminated)
                    {
                        BannerKingsConfig.Instance.TitleManager.InheritTitle(title.deJure, kingdom.Leader, title);
                    }
                }
            }
        }
    }

    namespace Patches
    {
        [HarmonyPatch(typeof(HeirSelectionCampaignBehavior), "OnHeirSelectionOver")]
        internal class OnHeirSelectionOverPatch
        {
            private static bool Prefix(List<InquiryElement> element)
            {
                Hero newLeader = element.First<InquiryElement>().Identifier as Hero;
                var titles = new List<FeudalTitle>(BannerKingsConfig.Instance.TitleManager.GetAllDeJure(Hero.MainHero));
                if (titles.Count > 0)
                {
                    BannerKingsConfig.Instance.TitleManager.InheritAllTitles(Hero.MainHero, newLeader);
                }

                return true;
            }
        }
    }
}