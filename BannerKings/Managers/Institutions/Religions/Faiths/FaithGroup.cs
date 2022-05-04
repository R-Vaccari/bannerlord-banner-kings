using System.Collections.Generic;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths
{
    public class FaithGroup
    {
        private List<Faith> members;
        private TextObject name;
        private TextObject description;

        public FaithGroup(TextObject name, TextObject description)
        {
            this.name = name;
            this.description = description;
            members = new List<Faith>();
        }

        public MBReadOnlyList<Faith> Members => members.GetReadOnlyList();

        public void AddMember(Faith faith)
        {
            if (!members.Contains(faith))
                members.Add(faith);
        }

        public void RemoveMember(Faith faith)
        {
            if (members.Contains(faith))
                members.Remove(faith);
        }
    }
}
