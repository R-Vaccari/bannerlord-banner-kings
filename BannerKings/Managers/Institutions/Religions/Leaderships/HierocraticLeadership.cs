using System;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Leaderships
{
    public class HierocraticLeadership : CentralizedLeadership
    {
        public override Clergyman DecideNewLeader()
        {
            return null;
        }

        public override TextObject GetHint()
        {
            return new TextObject(
                "{=!}A hierocratic organization is centered around a single head of faith. The head of faith is decided upon the faith's clergymen and not by secular lords. They will decide on all matters regarding faith and the spiritual.");
        }

        public override TextObject GetName()
        {
            return new TextObject("{=!}Hierocratic");
        }

        public override void Initialize(Religion religion)
        {
        }

        public override bool IsLeader(Clergyman clergyman)
        {
            throw new NotImplementedException();
        }
    }
}