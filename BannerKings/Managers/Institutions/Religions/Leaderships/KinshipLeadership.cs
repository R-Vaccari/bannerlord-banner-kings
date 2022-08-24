using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Leaderships
{
    public class KinshipLeadership : DescentralizedLeadership
    {
        public override TextObject GetHint()
        {
            return new TextObject(
                "{=XD9xTfh7}Kinship religions have their religions leaders dictated by the landed clans that adhere to the faith. Each clan is responsable for appointing a proeminent preacher from among their fiefs.");
        }

        public override TextObject GetName()
        {
            return new TextObject("{=ZdHi8mGH}Kinship");
        }
    }
}