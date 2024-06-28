using BannerKings.Managers.Goals;
using System.Collections.Generic;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Societies
{
    public class SocietyRank : BannerKingsObject
    {
        public SocietyRank(string id) : base(id) { }
        public void Initialize(TextObject name, TextObject effect, List<Goal> goals, int devotion = 0, int memberLimit = -1)
        {
            Initialize(name, effect);
            Devotion = devotion;
            MemberLimit = memberLimit;
            Goals = goals;
        }

        public int MemberLimit { get; private set; }
        public int Devotion { get; private set; }
        public List<Goal> Goals { get; private set; }
    }
}
