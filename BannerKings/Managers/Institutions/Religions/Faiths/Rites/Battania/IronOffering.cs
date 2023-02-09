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
        public override TextObject GetDescription() => new TextObject("{=!}Although the spirits accept many kinds of offerings, iron has been the staple option in the Uchalion plateau. Due to it's vast usefulness, it is prized as a gft worthy of the Gods, despite its raw nature.");
    }
}
