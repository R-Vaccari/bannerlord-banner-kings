using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.BarterSystem.Barterables;

namespace BannerKings.Behaviours.Diplomacy.Barterable
{
    public class BKPeaceBarterable : PeaceBarterable
    {
        public BKPeaceBarterable(Hero owner, IFaction peaceOfferingFaction, IFaction offeredFaction, CampaignTime duration) : 
            base(owner, peaceOfferingFaction, offeredFaction, duration)
        {
        }

        public override void Apply()
        {
            base.Apply();

        }

        public override string StringID => "bk_peace_barterable";
    }
}
