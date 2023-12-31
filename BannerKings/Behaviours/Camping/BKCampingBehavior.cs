using BannerKings.Extensions;
using Helpers;
using SandBox.View.Map;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Camping
{
    public class BKCampingBehavior : BannerKingsBehavior
    {
        private bool camping = false;
        public void MakeCamp(MobileParty party)
        {
            party.ChangeVisual("map_icon_siege_camp_tent");
            GameMenu.ActivateGameMenu("bk_camping_wait_menu");
            camping = true;
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this,
                (CampaignGameStarter starter) =>
                {
                    starter.AddWaitGameMenu("bk_camping_wait_menu",
                        "{=DjzNUuoT}You are now camping in the vicinity of {FIEF}.",
                        (MenuCallbackArgs args) =>
                        {
                            UpdateCampMenu();
                        },
                        (MenuCallbackArgs args) => true,
                        null,
                        (MenuCallbackArgs args, CampaignTime time) =>
                        {
                            if (time.GetHourOfDay == 12)
                            {
                                UpdateCampMenu();
                            }
                        },
                        GameMenu.MenuAndOptionType.WaitMenuHideProgressAndHoursOption,
                        TaleWorlds.CampaignSystem.Overlay.GameOverlays.MenuOverlayType.None,
                        0f,
                        GameMenu.MenuFlags.None,
                        null);

                    starter.AddGameMenuOption("bk_camping_wait_menu",
                      "bk_camping_leave",
                      new TextObject("{=pZRUL363}Dismantle Camp").ToString(),
                      (MenuCallbackArgs args) =>
                      {
                          args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                          return true;
                      },
                      (MenuCallbackArgs args) => GameMenu.ExitToLast(),
                      true);
                });

            CampaignEvents.TickEvent.AddNonSerializedListener(this, (float dt) =>
            {
                if (camping)
                {
                    MobileParty.MainParty.ChangeVisual("map_icon_siege_camp_tent");
                    if (TaleWorlds.CampaignSystem.Campaign.Current.CurrentMenuContext == null ||
                               TaleWorlds.CampaignSystem.Campaign.Current.CurrentMenuContext.StringId != "bk_camping_wait_menu")
                    {
                        GameMenu.ActivateGameMenu("bk_camping_wait_menu");
                    }
                }
            });
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void UpdateCampMenu()
        {
            MobileParty.MainParty.ChangeVisual("map_icon_siege_camp_tent");
            MBTextManager.SetTextVariable("FIEF", SettlementHelper.FindNearestSettlement(x => !x.IsHideout,
                MobileParty.MainParty));
            bool flag = PartyVisualManager.Current.GetVisualOfParty(MobileParty.MainParty.Party).HumanAgentVisuals != null;
            if (flag)
            {
                PartyVisualManager.Current.GetVisualOfParty(MobileParty.MainParty.Party).HumanAgentVisuals.GetEntity().SetVisibilityExcludeParents(false);
            }
            bool flag2 = PartyVisualManager.Current.GetVisualOfParty(MobileParty.MainParty.Party).MountAgentVisuals != null;
            if (flag2)
            {
                PartyVisualManager.Current.GetVisualOfParty(MobileParty.MainParty.Party).MountAgentVisuals.GetEntity().SetVisibilityExcludeParents(false);
            }
            bool flag3 = PartyVisualManager.Current.GetVisualOfParty(MobileParty.MainParty.Party).CaravanMountAgentVisuals != null;
            if (flag3)
            {
                PartyVisualManager.Current.GetVisualOfParty(MobileParty.MainParty.Party).CaravanMountAgentVisuals.GetEntity().SetVisibilityExcludeParents(false);
            }
        }
    }
}
