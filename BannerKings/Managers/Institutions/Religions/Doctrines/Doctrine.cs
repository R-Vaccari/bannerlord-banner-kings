using System.Collections.Generic;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Doctrines;

public class Doctrine : BannerKingsObject
{
    public Doctrine(string id, TextObject name, TextObject description,
        TextObject effects, List<string> incompatibleDoctrines) : base(id)
    {
        this.name = name;
        this.description = description;
        this.effects = effects;
        this.incompatibleDoctrines = incompatibleDoctrines;
    }

    private TextObject effects { get; }
    private List<string> incompatibleDoctrines { get; }

    public TextObject Effects => effects;
    public MBReadOnlyList<string> IncompatibleDoctrines => incompatibleDoctrines.GetReadOnlyList();
}