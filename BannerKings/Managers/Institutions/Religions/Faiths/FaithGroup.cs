using System.Collections.Generic;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths;

public class FaithGroup
{
    private readonly List<Faith> members;

    public FaithGroup(TextObject name, TextObject description)
    {
        Name = name;
        Description = description;
        members = new List<Faith>();
    }

    public TextObject Name { get; }

    public TextObject Description { get; }

    public MBReadOnlyList<Faith> Members => members.GetReadOnlyList();

    public void AddMember(Faith faith)
    {
        if (!members.Contains(faith))
        {
            members.Add(faith);
        }
    }

    public void RemoveMember(Faith faith)
    {
        if (members.Contains(faith))
        {
            members.Remove(faith);
        }
    }
}