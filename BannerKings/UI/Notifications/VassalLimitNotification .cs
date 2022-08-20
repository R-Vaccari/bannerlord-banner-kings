using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.UI.Notifications
{
    public class VassalLimitNotification : InformationData
    {
        public override TextObject TitleText => new TextObject("Over Vassal Limit");

        public override string SoundEventPath => "event:/ui/notification/relation";

        public VassalLimitNotification() : base(new TextObject("{=!}You have more vassals than you can manage."))
        {
        }
    }
}
