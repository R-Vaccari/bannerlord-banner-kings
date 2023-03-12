using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers.Helpers;
using BannerKings.Managers.Kingdoms;
using BannerKings.Managers.Skills;
using BannerKings.Managers.Titles;
using BannerKings.Settings;
using BannerKings.UI.Notifications;
using HarmonyLib;
using SandBox.CampaignBehaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static TaleWorlds.CampaignSystem.Actions.ChangeKingdomAction;

namespace BannerKings.Behaviours
{
    public class BKTitleBehavior : BannerKingsBehavior
    {
        private Dictionary<Settlement, List<Clan>> conqueredByArmies = new();

        public void RemoveSettlement(Settlement settlement)
        {
            if (conqueredByArmies.ContainsKey(settlement))
            {
                conqueredByArmies.Remove(settlement);
            }
        }

        public void AddClan(Clan clan, Settlement settlement)
        {
            if (!conqueredByArmies.ContainsKey(settlement))
            {
                conqueredByArmies.Add(settlement, new List<Clan>());
            }

            if (!conqueredByArmies[settlement].Contains(clan))
            {
                conqueredByArmies[settlement].Add(clan);
            }
        }

        public void Clear(Settlement settlement)
        {
            if (conqueredByArmies.ContainsKey(settlement))
            {
                conqueredByArmies[settlement].Clear();
            }
        }

        public override void RegisterEvents()
        {
            //CampaignEvents.RulingCLanChanged.AddNonSerializedListener(this, new Action<Kingdom, Clan>(this.OnRulingClanChanged));
            CampaignEvents.DailyTickHeroEvent.AddNonSerializedListener(this, OnDailyTickHero);
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
            CampaignEvents.HeroKilledEvent.AddNonSerializedListener(this, OnHeroKilled);
            CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, OnDailyTickSettlement);
            CampaignEvents.ClanChangedKingdom.AddNonSerializedListener(this, OnClanChangedKingdom);
            CampaignEvents.OnClanDestroyedEvent.AddNonSerializedListener(this, OnClanDestroyed);
            CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener(this, OnOwnerChanged);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("bannerkings-armies", ref conqueredByArmies);
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
                Campaign.Current.CampaignInformationManager.NewMapNoticeAdded(new DemesneLimitNotification());
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
                Campaign.Current.CampaignInformationManager.NewMapNoticeAdded(new UnlandedDemesneLimitNotification());
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
                if (title == null || title.type != TitleType.Dukedom)
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
                Campaign.Current.CampaignInformationManager.NewMapNoticeAdded(new VassalLimitNotification());
            }
        }

        private void OnDailyTick()
        {
            if (BannerKingsConfig.Instance.TitleManager == null)
            {
                return;
            }

            foreach (var duchy in BannerKingsConfig.Instance.TitleManager.GetAllTitlesByType(TitleType.Dukedom))
            {
                duchy.TickClaims();
                var faction = duchy.deJure.Clan.Kingdom;
                if (faction == null || faction != duchy.DeFacto.Clan.Kingdom)
                {
                    continue;
                }

                var currentSovereign = duchy.sovereign;
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
            }
        }

        private void OnHeroKilled(Hero victim, Hero killer, KillCharacterAction.KillCharacterActionDetail detail,
            bool showNotification = true)
        {
            if (victim?.Clan == null || BannerKingsConfig.Instance.TitleManager == null)
            {
                return;
            }

            BannerKingsConfig.Instance.TitleManager.RemoveKnights(victim);
            var sovereign = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(victim.Clan.Kingdom);
            if (sovereign?.contract == null)
            {
                return;
            }

            var titles = new List<FeudalTitle>(BannerKingsConfig.Instance.TitleManager.GetAllDeJure(victim));
            if (titles.Count == 0)
            {
                return;
            }

            if (victim == Hero.MainHero)
            {
                return;
            }

            bool applySuccession = false;
            if (sovereign != null)
            {
                if (titles.Contains(sovereign))
                {
                    applySuccession = true;
                    titles.Remove(sovereign);
                }
            }

            InheritanceHelper.ApplyInheritanceAllTitles(titles, victim);
            if (sovereign != null && applySuccession)
            {
                SuccessionHelper.ApplySovereignSuccession(sovereign, victim, victim.Clan.Kingdom);
            }
        }

        public void OnDailyTickSettlement(Settlement settlement)
        {
            if (settlement.Town == null || !settlement.Town.IsOwnerUnassigned || settlement.OwnerClan == null
                || BannerKingsConfig.Instance.TitleManager == null)
            {
                return;
            }

            var kingdom = settlement.OwnerClan.Kingdom;
            if (kingdom == null)
            {
                return;
            }

            var sovereign = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(kingdom);
            if (sovereign?.contract == null)
            {
                return;
            }

            if (sovereign.contract.Rights.Contains(FeudalRights.Conquest_Rights))
            {
                var decisions = kingdom.UnresolvedDecisions.ToList();
                var bkDecision = decisions.FirstOrDefault(x =>
                    x is BKSettlementClaimantDecision decision && decision.Settlement == settlement);
                if (bkDecision != null)
                {
                    return;
                }

                var vanillaDecision = decisions.FirstOrDefault(x =>
                    x is SettlementClaimantDecision decision && !(decision is BKSettlementClaimantDecision) &&
                    decision.Settlement == settlement);
                if (vanillaDecision != null)
                {
                    kingdom.RemoveDecision(vanillaDecision);
                }

                if (!conqueredByArmies.ContainsKey(settlement))
                {
                    return;
                }

                var clans = conqueredByArmies[settlement].FindAll(x => x.Kingdom == kingdom && !x.IsUnderMercenaryService);
                switch (clans.Count)
                {
                    case 1:
                        ChangeOwnerOfSettlementAction.ApplyByKingDecision(clans[0].Leader, settlement);
                        break;
                    case 0:
                        kingdom.AddDecision(new KingSelectionKingdomDecision(kingdom.RulingClan), true);
                        break;
                    default:
                    {
                        kingdom.AddDecision(
                            new BKSettlementClaimantDecision(kingdom.RulingClan, settlement, null, null,
                                conqueredByArmies[settlement], true), true);
                        if (clans.Contains(Clan.PlayerClan) && !Clan.PlayerClan.IsUnderMercenaryService)
                        {
                            var party = clans[0].Leader.PartyBelongedTo;
                            Army army = null;
                            if (party != null)
                            {
                                army = party.Army;
                            }

                            if (army != null)
                            {
                                GameTexts.SetVariable("ARMY", army.Name);
                            }
                            else
                            {
                                GameTexts.SetVariable("ARMY", new TextObject("{=FaUbxgHO}the conquering army"));
                            }

                            GameTexts.SetVariable("SETTLEMENT", settlement.Name);
                            InformationManager.ShowInquiry(new InquiryData(
                                new TextObject("{=fG4nhQtn}Conquest Right - Election").ToString(),
                                new TextObject("{=i1XurmS0}By contract law, you and the participants of {ARMY} will compete in election for the ownership of {SETTLEMENT}.").ToString(),
                                true, false, GameTexts.FindText("str_done").ToString(), null, null, null), true);
                        }

                        break;
                    }
                }
            }
        }

        private void OnOwnerChanged(Settlement settlement, bool openToClaim, Hero newOwner, Hero oldOwner,
            Hero capturerHero,
            ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail detail)
        {
            if (BannerKingsConfig.Instance.TitleManager == null)
            {
                return;
            }

            if (conqueredByArmies.ContainsKey(settlement))
            {
                conqueredByArmies.Remove(settlement);
            }

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

        public void OnDailyTickClan(Clan clan)
        {
            if (BannerKingsConfig.Instance.TitleManager == null || clan.Kingdom == null || clan.IsUnderMercenaryService ||
                !clan.IsEliminated || clan.IsRebelClan || clan.IsBanditFaction)
            {
                return;
            }

            BannerKingsConfig.Instance.TitleManager.GiveLordshipOnKingdomJoin(clan.Kingdom, clan);
        }

        public void OnClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom,
            ChangeKingdomActionDetail detail, bool showNotification)
        {
            if (detail != ChangeKingdomActionDetail.JoinKingdom || BannerKingsConfig.Instance.TitleManager == null)
            {
                return;
            }

            BannerKingsConfig.Instance.TitleManager.GiveLordshipOnKingdomJoin(newKingdom, clan);
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

                    if (title.sovereign != null)
                    {
                        if (title.sovereign.deJure != title.deJure && title.sovereign.deJure.IsAlive)
                        {
                            BannerKingsConfig.Instance.TitleManager.InheritTitle(title.deJure, title.sovereign.deJure, title);
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
        [HarmonyPatch(typeof(SettlementClaimantCampaignBehavior))]
        internal class SettlementClaimantCampaignBehaviorPatches
        {
            [HarmonyPostfix]
            [HarmonyPatch("OnSettlementOwnerChanged")]
            private static void OnSettlementOwnerChangedPostfix(Settlement settlement, bool openToClaim, Hero newOwner, Hero oldOwner, Hero capturerHero, ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail detail)
            {
                if (settlement.Town is { IsOwnerUnassigned: true } &&
                detail != ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail.ByLeaveFaction)
                {
                    return;
                }

                var title = BannerKingsConfig.Instance.TitleManager.GetTitle(settlement);
                if (title == null)
                {
                    return;
                }

                BannerKingsConfig.Instance.TitleManager.ApplyOwnerChange(settlement, newOwner);
                var kingdom = newOwner.Clan.Kingdom;
                if (kingdom == null)
                {
                    return;
                }

                var sovereign = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(kingdom);
                if (sovereign?.contract == null)
                {
                    return;
                }

                BKTitleBehavior behavior = Campaign.Current.GetCampaignBehavior<BKTitleBehavior>();

                if (detail == ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail.BySiege)
                {
                    var absoluteRightGranted = false;
                    if (sovereign.contract.Rights.Contains(FeudalRights.Absolute_Land_Rights))
                    {
                        foreach (var clan in kingdom.Clans)
                        {
                            if (clan.Leader == title.deJure)
                            {
                                ChangeOwnerOfSettlementAction.ApplyByKingDecision(clan.Leader, settlement);
                                absoluteRightGranted = true;
                                if (clan.Leader == Hero.MainHero)
                                {
                                    GameTexts.SetVariable("SETTLEMENT", settlement.Name);
                                    InformationManager.ShowInquiry(new InquiryData(
                                        new TextObject("{=Rc5gU5bS}Absolute Land Right").ToString(),
                                        new TextObject("{=AuEhA2EB}By contract law, you have been awarded the ownership of {SETTLEMENT} due to your legal right to this fief.")
                                            .ToString(),
                                        true, false, GameTexts.FindText("str_done").ToString(), null, null, null), true);
                                }
                            }
                        }
                        settlement.Town.IsOwnerUnassigned = false;
                    }

                    if (!absoluteRightGranted && sovereign.contract.Rights.Contains(FeudalRights.Conquest_Rights))
                    {
                        var decisions = kingdom.UnresolvedDecisions.ToList();
                        var decision = decisions.FirstOrDefault(x =>
                            x is SettlementClaimantDecision claimantDecision && claimantDecision.Settlement == settlement);
                        if (decision != null)
                        {
                            kingdom.RemoveDecision(decision);
                        }

                        var party = settlement.LastAttackerParty;
                        var army = party.Army;
                        if (army != null)
                        {
                            behavior.Clear(settlement);
                            foreach (var clanParty in army.Parties)
                            {
                                if (!clanParty.ActualClan.IsUnderMercenaryService)
                                {
                                    behavior.AddClan(clanParty.ActualClan, settlement);
                                }
                            }

                            return;
                        }

                        ChangeOwnerOfSettlementAction.ApplyByKingDecision(capturerHero, settlement);
                        if (capturerHero == Hero.MainHero)
                        {
                            GameTexts.SetVariable("SETTLEMENT", settlement.Name);
                            InformationManager.ShowInquiry(new InquiryData(new TextObject("{=pht4SLHe}Conquest Right").ToString(),
                                new TextObject("{=FKMakM2V}By contract law, you have been awarded the ownership of {SETTLEMENT} due to you conquering it.")
                                    .ToString(),
                                true, false, GameTexts.FindText("str_done").ToString(), null, null, null), true);
                        }

                        settlement.Town.IsOwnerUnassigned = false;
                    }
                }
            }
        }

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