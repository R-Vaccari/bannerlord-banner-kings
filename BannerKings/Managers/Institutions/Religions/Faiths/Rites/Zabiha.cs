using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Rites
{
    public class Zabiha : Offering
    {
        public Zabiha() : base(MBObjectManager.Instance.GetObject<ItemObject>(x => x.StringId == "cow"), 20)
        {
        }

        public override TextObject GetName()
        {
            return new TextObject("{=UO47hvgg6}Zabiha");
        }
    }
}