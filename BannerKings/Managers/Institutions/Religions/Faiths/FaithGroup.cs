using System.Collections.Generic;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Institutions.Religions.Faiths
{
    public class FaithGroup
    {
        [SaveableField(1)]
        private List<Faith> members;

        [SaveableField(2)]
        private TextObject name;

        [SaveableField(3)]
        private TextObject description;

        public FaithGroup(TextObject name, TextObject description)
        {
            this.name = name;
            this.description = description;
            members = new List<Faith>();
        }

        public TextObject Name => name;
        public TextObject Description => description;
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
