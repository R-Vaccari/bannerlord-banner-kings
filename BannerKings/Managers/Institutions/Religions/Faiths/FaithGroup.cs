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
            this.members = new List<Faith>();
        }

        public TextObject Name => name;
        public TextObject Description => description;
        public MBReadOnlyList<Faith> Members => this.members.GetReadOnlyList();

        public void AddMember(Faith faith)
        {
            if (!this.members.Contains(faith))
                this.members.Add(faith);
        }

        public void RemoveMember(Faith faith)
        {
            if (this.members.Contains(faith))
                this.members.Remove(faith);
        }
    }
}
