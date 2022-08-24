using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.UI.Notifications
{
    public class DemesneLimitNotification : InformationData
    {
        public DemesneLimitNotification() : base(new TextObject("{=!}You have more fiefs than you can manage."))
        {
        }

        public override TextObject TitleText => new("Over Demesne Limit");

        public override string SoundEventPath => "event:/ui/notification/relation";
    }
}