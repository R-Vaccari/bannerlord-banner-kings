using System;
using System.Linq;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static BannerKings.Managers.TitleManager;
using static TaleWorlds.CampaignSystem.Actions.ChangeKingdomAction;
using TaleWorlds.CampaignSystem.Election;
using BannerKings.Managers.Kingdoms;

namespace BannerKings.Behaviours
{
    public class BKTitleBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickClanEvent.AddNonSerializedListener(this, new Action<Clan>(OnDailyTickClan));
            CampaignEvents.ClanChangedKingdom.AddNonSerializedListener(this, new Action<Clan, Kingdom, Kingdom, ChangeKingdomActionDetail, bool>(OnClanChangedKingdom));
            CampaignEvents.OnClanDestroyedEvent.AddNonSerializedListener(this, new Action<Clan>(OnClanDestroyed));
            CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener(this, new Action<Settlement, bool, Hero, Hero, Hero, ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail>(OnOwnerChanged));
        }

        public override void SyncData(IDataStore dataStore)
        {
            
        }

        public void OnDailyTickClan(Clan clan)
        {
            if (BannerKingsConfig.Instance.TitleManager == null || clan.Kingdom == null || clan.IsUnderMercenaryService ||
                !clan.IsEliminated || clan.IsRebelClan || clan.IsBanditFaction) return;
            BannerKingsConfig.Instance.TitleManager.GiveLordshipOnKingdomJoin(clan.Kingdom, clan);
        }

        public void OnClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom, ChangeKingdomAction.ChangeKingdomActionDetail detail, bool showNotification)
        {
            if (detail != ChangeKingdomActionDetail.JoinKingdom || BannerKingsConfig.Instance.TitleManager == null) return;
            BannerKingsConfig.Instance.TitleManager.GiveLordshipOnKingdomJoin(newKingdom, clan);
        }

        private void OnClanDestroyed(Clan clan) 
        {
            
            if (BannerKingsConfig.Instance.TitleManager == null) return;
            List<FeudalTitle> titles = BannerKingsConfig.Instance.TitleManager.GetAllDeJure(clan);
            if (titles.Count > 0)
            {
                foreach (FeudalTitle title in titles)
                {
                    if (BannerKingsConfig.Instance.TitleManager.HasSuzerain(title))
                    {
                        FeudalTitle suzerain = BannerKingsConfig.Instance.TitleManager.GetImmediateSuzerain(title);
                        if (suzerain.deJure.IsAlive && !clan.Heroes.Contains(suzerain.deJure))
                        {
                            BannerKingsConfig.Instance.TitleManager.InheritAllTitles(title.deJure, suzerain.deJure);
                            continue;
                        }
                            
                    } 
                    
                    if (title.sovereign != null)
                        if (title.sovereign.deJure != title.deJure && title.sovereign.deJure.IsAlive)
                        {
                            BannerKingsConfig.Instance.TitleManager.InheritAllTitles(title.deJure, title.sovereign.deJure); 
                            continue;
                        }    
                    
                    if (title.deJure != title.deFacto)
                    {
                        BannerKingsConfig.Instance.TitleManager.DeactivateDeJure(title);
                        continue;
                    }

                    BannerKingsConfig.Instance.TitleManager.DeactivateTitle(title);
                }  
            }
        }

        private void OnOwnerChanged(Settlement settlement, bool openToClaim, Hero newOwner, Hero oldOwner, Hero capturerHero, 
            ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail detail)
        {
            if (BannerKingsConfig.Instance.TitleManager == null) return;

            if (settlement.Town != null && settlement.Town.IsOwnerUnassigned &&
                detail != ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail.ByLeaveFaction) return;

            BannerKingsConfig.Instance.TitleManager.ApplyOwnerChange(settlement, newOwner);
            Kingdom kingdom = newOwner.Clan.Kingdom;
            if (kingdom == null) return;

            FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(kingdom);
            if (title == null || title.contract == null) return;


            if (detail == ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail.BySiege)
            {
                if (title.contract.rights.Contains(FeudalRights.Absolute_Land_Rights))
                {
                    foreach (Clan clan in kingdom.Clans)
                        if (clan.Leader == title.deJure)
                        {
                            ChangeOwnerOfSettlementAction.ApplyByDefault(clan.Leader, settlement);
                            if (clan.Leader == Hero.MainHero)
                            {
                                GameTexts.SetVariable("SETTLEMENT", settlement.Name);
                                InformationManager.ShowInquiry(new InquiryData(new TextObject("Absolute Land Right").ToString(),
                                    new TextObject("By contract law, you have been awarded the ownership of {SETTLEMENT} due to your legal right to this fief.").ToString(),
                                    true, false, GameTexts.FindText("str_done").ToString(), null, null, null), true);
                            }
                        }
                      
                }  
                    
                if (title.contract.rights.Contains(FeudalRights.Conquest_Rights))
                {
                    List<KingdomDecision> decisions = kingdom.UnresolvedDecisions.ToList();
                    KingdomDecision decision = decisions.FirstOrDefault(x => x is SettlementClaimantDecision && (x as SettlementClaimantDecision).Settlement == settlement);
                    if (decision != null)
                        kingdom.RemoveDecision(decision);

                    MobileParty party = settlement.LastAttackerParty;
                    Army army = party.Army;
                    if (army != null)
                    {
                        List<Clan> clans = (List<Clan>)(from p in army.Parties select p.ActualClan);
                        kingdom.AddDecision(new BKSettlementClaimantDecision(kingdom.RulingClan, settlement, capturerHero, null, clans, true), true);
                        if (clans.Contains(Clan.PlayerClan))
                        {
                            GameTexts.SetVariable("ARMY", army.Name);
                            GameTexts.SetVariable("SETTLEMENT", settlement.Name);
                            InformationManager.ShowInquiry(new InquiryData(new TextObject("Conquest Right - Election").ToString(),
                                new TextObject("By contract law, you and the participants of {ARMY} will compete in election for the ownership of {SETTLEMENT}.").ToString(),
                                true, false, GameTexts.FindText("str_done").ToString(), null, null, null), true);
                        }
                    }
                    else
                    {
                        ChangeOwnerOfSettlementAction.ApplyByDefault(capturerHero, settlement);
                        if (capturerHero == Hero.MainHero)
                        {
                            GameTexts.SetVariable("SETTLEMENT", settlement.Name);
                            InformationManager.ShowInquiry(new InquiryData(new TextObject("Conquest Right").ToString(),
                                new TextObject("By contract law, you have been awarded the ownership of {SETTLEMENT} due to you conquering it.").ToString(),
                                true, false, GameTexts.FindText("str_done").ToString(), null, null, null), true);
                        }    
                    }
                }
            }
        }
    }
}
