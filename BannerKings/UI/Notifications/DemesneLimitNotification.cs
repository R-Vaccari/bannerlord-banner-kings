using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.UI.Notifications
{
    public class UnlandedDemesneLimitNotification : InformationData
    {
        public override TextObject TitleText => new TextObject("{=!}Over Title Limit");

        public override string SoundEventPath => "event:/ui/notification/relation";

        public UnlandedDemesneLimitNotification() : base(new TextObject("{=!}You have too many unlanded titles."))
        {
        }
    }
}
