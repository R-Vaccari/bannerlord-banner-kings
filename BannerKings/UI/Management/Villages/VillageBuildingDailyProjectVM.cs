using System;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.TownManagement;

namespace BannerKings.UI.Management.Villages
{
    public class VillageBuildingDailyProjectVM : SettlementDailyProjectVM
    {
        public VillageBuildingDailyProjectVM(Action<SettlementProjectVM, bool> onSelection, Action<SettlementProjectVM> onSetAsCurrent, Action onResetCurrent, Building building) : base(onSelection, onSetAsCurrent, onResetCurrent, building)
        {
        }

        public override void RefreshProductionText()
        {
        }
    }
}
