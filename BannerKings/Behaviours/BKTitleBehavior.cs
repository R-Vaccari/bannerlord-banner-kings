using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using static BannerKings.Managers.TitleManager;

namespace BannerKings.Behaviours
{
    class BKTitleBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            throw new NotImplementedException();
        }

        public override void SyncData(IDataStore dataStore)
        {
            CampaignEvents.OnClanDestroyedEvent.AddNonSerializedListener(this, new Action<Clan>(OnClanDestroyed));
            CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener(this, new Action<Settlement, bool, Hero, Hero, Hero, ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail>(OnOwnerChanged));
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

            FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetTitle(settlement);
            if (title != null)
            {
                if (detail == ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail.ByKingDecision)
                    BannerKingsConfig.Instance.TitleManager.InheritTitle(oldOwner, newOwner, title);
                else
                    BannerKingsConfig.Instance.TitleManager.ApplyOwnerChange(settlement, newOwner);
            }
            
        }
    }
}
