using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.UI.Notifications
{
    public class UnlandedDemesneLimitNotification : InformationData
    {
        public UnlandedDemesneLimitNotification() : base(new TextObject("{=98EwUZLVh}You have too many unlanded titles."))
        {
        }

        public override TextObject TitleText => new("{=QBAoCrPjq}Over Title Limit");

        public override string SoundEventPath => "event:/ui/notification/relation";
    }
}