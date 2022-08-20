using TaleWorlds.CampaignSystem.ViewModelCollection.Map.MapNotificationTypes;

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