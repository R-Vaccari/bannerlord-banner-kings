using System;
using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers.Helpers;
using BannerKings.Managers.Skills;
using BannerKings.Managers.Titles;
using BannerKings.Settings;
using BannerKings.UI.Notifications;
using HarmonyLib;
using SandBox.CampaignBehaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using static System.Collections.Specialized.BitVector32;

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
            if (hero == null)
            {
                return;
            }

            if (hero.IsChild || hero.Occupation != Occupation.Lord || hero.Clan == null ||
                hero.Clan.IsMinorFaction || !BannerKingsConfig.Instance.TitleManager.IsHeroTitleHolder(hero))
            {
                return;
            }

            if (hero.Clan != null && hero == hero.Clan.Leader)
            {
                if (BannerKingsSettings.Instance.AIManagement)
                {
                    CheckOverDemesneLimit(hero);
                    CheckOverUnlandedDemesneLimit(hero);
                    //CheckOverVassalLimit(hero);
                }
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
            if (diff < 0.5f)
            {
                return;
            }

            RunWeekly(() =>
            {
                var title = BannerKingsConfig.Instance.AI.ChooseTitleToGive(hero, diff);
                if (title == null)
                {
                    return;
                }

                var receiver = BannerKingsConfig.Instance.AI.ChooseVassalToGiftLandedTitle(hero, title);
                if (receiver == null)
                {
                    return;
                }

                var action = BannerKingsConfig.Instance.TitleModel.GetAction(ActionType.Grant, title, hero);
                BannerKingsConfig.Instance.TitleManager.GrantTitle(action, receiver);
            },
            GetType().Name,
            false);
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

            RunWeekly(() =>
            {
                var diff = current - limit;
                var title = BannerKingsConfig.Instance.AI.ChooseTitleToGive(hero, diff, false);
                if (title == null || title.TitleType != TitleType.Dukedom)
                {
                    return;
                }

                var receiver = BannerKingsConfig.Instance.AI.ChooseVassalToGiftUnlandedTitle(hero, title, vassals);
                if (receiver == null)
                {
                    return;
                }

                var action = BannerKingsConfig.Instance.TitleModel.GetAction(ActionType.Grant, title, hero);
                BannerKingsConfig.Instance.TitleManager.GrantTitle(action, receiver);
            },
            GetType().Name,
            false);
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

                var faction = kingdom.deJure.Clan.Kingdom;
                if (faction == null || faction != kingdom.DeFacto.Clan.Kingdom)
                {
                    continue;
                }

                var currentFactionSovereign = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(faction);
                if (currentFactionSovereign.TitleType == TitleType.Empire)
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
                        if (suzerain.deJure.IsAlive && !clan.Heroes.Contains(suzerain.deJure))
                        {
                            BannerKingsConfig.Instance.TitleManager.InheritTitle(title.deJure, suzerain.deJure, title);
                            continue;
                        }
                    }

                    if (title.Sovereign != null)
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