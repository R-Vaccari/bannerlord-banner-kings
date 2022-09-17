using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Utils
{
    internal static class NotificationsHelper
    {

        internal static void AddQuickNotificationWithSound(TextObject content, BasicCharacterObject announcer = null)
        {
            MBInformationManager.AddQuickInformation(content,
                0, 
                announcer, 
                "event:/ui/notification/relation");
        }
    }
}
