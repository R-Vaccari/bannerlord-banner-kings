using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.CampaignSystem.Party;
using BannerKings.Behaviours.Shipping;

namespace BannerKings.Patches
{
    internal class SailingPatches
    {
        /*[HarmonyPatch(typeof(PartyBase), "IsPositionOkForTraveling")]
        internal class IsPositionOkForTravelingPatch
        {
            private static bool Prefix(Vec2 position, ref bool __result)
            {
                BKSailingBehavior behavior = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKSailingBehavior>();
                PathFaceRecord faceIndex = TaleWorlds.CampaignSystem.Campaign.Current.MapSceneWrapper.GetFaceIndex(position);
                if (!faceIndex.IsValid())
                {
                    __result = false;
                    return false;
                }

                if (TaleWorlds.CampaignSystem.Campaign.Current.MapSceneWrapper.GetFaceTerrainType(faceIndex) == TaleWorlds.Core.TerrainType.Water)
                {
                    __result = true;
                    return false;
                }

                if (behavior.IsIndexSea(faceIndex.FaceIndex) || behavior.IsIndexRiver(faceIndex.FaceIndex))
                {
                    __result = true;
                    return false;
                }

                return true;
            }
        }*/
    }
}
