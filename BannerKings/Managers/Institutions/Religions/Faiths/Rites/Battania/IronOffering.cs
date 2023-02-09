using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Rites.Battania
{
    public class IronOffering : Offering
    {
        public IronOffering() : base(DefaultItems.IronOre, 50)
        {
        }

        public override TextObject GetName() => new TextObject("{=DhQXSbip}Iarnaig-Tairgseadh");
        public override TextObject GetDescription() => new TextObject("{=!}Believed to me");
    }
}
