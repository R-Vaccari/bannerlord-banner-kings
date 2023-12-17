using SandBox.View.Map;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace BannerKings.Extensions
{
    public static class MobilePartyExtensions
    {
        public static bool IsAvailableForArmies(this MobileParty mobileParty)
        {
            Hero leaderHero = mobileParty.LeaderHero;
            if (mobileParty.IsLordParty &&
                mobileParty.Army == null &&
                leaderHero != null &&
                !mobileParty.IsMainParty &&
                leaderHero != leaderHero.MapFaction.Leader &&
                !mobileParty.Ai.DoNotMakeNewDecisions)
            {
                Settlement currentSettlement = mobileParty.CurrentSettlement;
                if (((currentSettlement != null) ? currentSettlement.SiegeEvent : null) == null &&
                    !mobileParty.IsDisbanding && mobileParty.Food > -(mobileParty.FoodChange * 5f) &&
                    mobileParty.PartySizeRatio > 0.6f && leaderHero.CanLeadParty() &&
                    mobileParty.MapEvent == null &&
                    mobileParty.BesiegedSettlement == null)
                {
                    return true;
                }
            }

            return false;
        }

        public static void ChangeVisual(this MobileParty mobileParty, string prefab)
        {
            List<GameEntity> children = PartyVisualManager.Current.GetVisualOfParty(mobileParty.Party).StrategicEntity.GetChildren().ToList();
            if (children.Count > 0)
                PartyVisualManager.Current.GetVisualOfParty(mobileParty.Party).StrategicEntity.RemoveAllChildren();
            
            Scene scene = PartyVisualManager.Current.GetVisualOfParty(mobileParty.Party).StrategicEntity.Scene;
            GameEntity gameEntity = GameEntity.Instantiate(scene, prefab, true);
            MatrixFrame frame = MatrixFrame.Identity;
            frame.rotation.ApplyScaleLocal(1f);
            frame.Rotate(1.5707964f, Vec3.Up);
            gameEntity.SetFrame(ref frame);
            PartyVisualManager.Current.GetVisualOfParty(mobileParty.Party).StrategicEntity.AddChild(gameEntity, false);
        }
    }
}
