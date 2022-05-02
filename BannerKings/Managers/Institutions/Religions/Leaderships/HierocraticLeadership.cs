using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Leaderships
{
    public class HierocraticLeadership : CentralizedLeadership
    {
        public override Clergyman DecideNewLeader()
        {
            throw new NotImplementedException();
        }

        public override TextObject GetHint() => new TextObject("{=!}A hierocratic organization is centered around a single head of faith. The head of faith is decided upon the faith's clergymen and not by secular lords. They will decide on all matters regarding faith and the spiritual.");

        public override TextObject GetName() => new TextObject("{=!}Hierocratic");

        public override void Initialize(Religion religion)
        {
            throw new NotImplementedException();
        }
    }
}
