using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Institutions.Religions
{
    public abstract class ReligiousLeadership
    {
        [SaveableField(1)]
        protected Religion religion;
        public abstract void Initialize(Religion religion);

        public abstract TextObject GetName();

        public abstract TextObject GetHint();

        public void ChangeClergymanRank(Clergyman clergyman, int newRank)
        {
            TextObject firstName = clergyman.Hero.FirstName;
            TextObject fullName = new TextObject("{=!}{RELIGIOUS_TITLE} {NAME}")
                .SetTextVariable("RELIGIOUS_TITLE", religion.Faith.GetRankTitle(newRank))
                .SetTextVariable("NAME", firstName);
            clergyman.Hero.SetName(fullName, firstName);
        }

        public abstract bool IsLeader(Clergyman clergyman);
    }
}
