using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths
{
    public class FaithGroup : BannerKingsObject
    {
        public FaithGroup(string id) : base(id)
        {
            
        }

        public void Initialize(TextObject name, TextObject description)
        {
            this.name = name;
            this.description = description;
        }
    }
}