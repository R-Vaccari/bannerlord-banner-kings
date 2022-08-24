using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.UI.Notifications
{
    public class UnlandedDemesneLimitNotification : InformationData
    {
        public UnlandedDemesneLimitNotification() : base(new TextObject("{=M141t3i4X}You have too many unlanded titles."))
        {
        }

        public override TextObject TitleText => new("{=Rg82wxzR9}Over Title Limit");

        public override string SoundEventPath => "event:/ui/notification/relation";
    }
}