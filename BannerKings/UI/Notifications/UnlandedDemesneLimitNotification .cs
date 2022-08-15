using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.UI.Notifications
{
    public class DemesneLimitNotification : InformationData
    {
        public override TextObject TitleText => new TextObject("Over Demesne Limit");

        public override string SoundEventPath => "event:/ui/notification/relation";

        public DemesneLimitNotification() : base(new TextObject("{=!}You have more fiefs than you can manage."))
        {
        }
    }
}
