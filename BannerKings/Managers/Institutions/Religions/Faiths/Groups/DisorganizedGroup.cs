using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Groups
{
    public class DisorganizedGroup : FaithGroup
    {
        public DisorganizedGroup(string id) : base(id)
        {
        }

        public override bool ShouldHaveLeader => IsReformed;
        public override bool IsPreacher => ReformedPreacher;
        public override bool IsTemporal => ReformedTemporal;
        public override bool IsPolitical => ReformedPolitical;
        [SaveableProperty(10)] public bool IsReformed { get; private set; }
        [SaveableProperty(11)] public bool ReformedPreacher { get; private set; }
        [SaveableProperty(12)] public bool ReformedTemporal { get; private set; }
        [SaveableProperty(13)] public bool ReformedPolitical { get; private set; }

        public override TextObject Explanation => IsReformed ? new TextObject("{=!}The {GROUPS} may not have a representative.")
            .SetTextVariable("GROUPS", Name)
            :
            new TextObject("{=!}The {GROUPS} is a unreformed group that can be reformed. While unreformed, the group does not accept a faith leader.")
            .SetTextVariable("GROUPS", Name);
    }
}
