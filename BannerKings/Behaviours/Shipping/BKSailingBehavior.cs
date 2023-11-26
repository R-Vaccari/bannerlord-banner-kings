using BannerKings.Extensions;
using BannerKings.Managers.Items;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Library;

namespace BannerKings.Behaviours.Shipping
{
    public class BKSailingBehavior : BannerKingsBehavior
    {
        public bool IsIndexSea(int index) => index == 10;
        public bool IsIndexRiver(int index) => index == 11;
        public override void RegisterEvents()
        {
            /*CampaignEvents.TickEvent.AddNonSerializedListener(this,
                (float dt) =>
                {
                    foreach (MobileParty party in MobileParty.All)
                    {
                        PathFaceRecord index = TaleWorlds.CampaignSystem.Campaign.Current.MapSceneWrapper.GetFaceIndex(party.VisualPosition2DWithoutError);
                        if (IsIndexSea(index.FaceIndex) || IsIndexRiver(index.FaceIndex))
                        {
                            party.ChangeVisual("map_icon_siege_camp_tent");
                        }
                    }
                });*/
        }

        public override void SyncData(IDataStore dataStore)
        {
        }
    }
}
