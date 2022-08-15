using TaleWorlds.CampaignSystem.ViewModelCollection.Map;

namespace BannerKings.UI.Notifications
{
    public class DemesneLimitNotificationVM : MapNotificationItemBaseVM
    {

        public DemesneLimitNotificationVM(DemesneLimitNotification data) : base(data)
        {
            NotificationIdentifier = "settlementownerchanged";
        }
    }
}
