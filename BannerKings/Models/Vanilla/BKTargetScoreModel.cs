using BannerKings.Behaviours.Diplomacy;
using BannerKings.Behaviours.Diplomacy.Wars;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings.Models.Vanilla
{
    public class BKTargetScoreModel : DefaultTargetScoreCalculatingModel
    {
        public override float CurrentObjectiveValue(MobileParty mobileParty)
        {
            float result = base.CurrentObjectiveValue(mobileParty);
            if (mobileParty.Army == null || mobileParty.TargetSettlement == null)
            {
                return result;
            }

            IFaction targetFaction = mobileParty.TargetSettlement.MapFaction;
            if (targetFaction != mobileParty.MapFaction && targetFaction.IsAtWarWith(mobileParty.MapFaction))
            {
                CasusBelli justification = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKDiplomacyBehavior>()
                    .GetWar(mobileParty.MapFaction, targetFaction)?.CasusBelli;
                if (justification == null)
                {
                    return result;
                }

                AiBehavior defaultBehavior = mobileParty.DefaultBehavior;
                if (defaultBehavior == AiBehavior.RaidSettlement)
                {
                    result *= justification.RaidWeight;
                }
                else if (defaultBehavior == AiBehavior.BesiegeSettlement)
                {
                    result *= justification.ConquestWeight;
                }
            }

            return result;
        }

        public override float GetTargetScoreForFaction(Settlement targetSettlement, Army.ArmyTypes missionType, MobileParty mobileParty, float ourStrength, int numberOfEnemyFactionSettlements = -1, float totalEnemyMobilePartyStrength = -1)
        {
            float result =  base.GetTargetScoreForFaction(targetSettlement, missionType, mobileParty, ourStrength, numberOfEnemyFactionSettlements, totalEnemyMobilePartyStrength);

            IFaction targetFaction = targetSettlement.MapFaction;
            if (targetFaction != mobileParty.MapFaction && targetFaction.IsAtWarWith(mobileParty.MapFaction))
            {
                War war = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKDiplomacyBehavior>()
                    .GetWar(mobileParty.MapFaction, targetFaction);
                if (war == null)
                {
                    return result;
                }

                CasusBelli justification = war.CasusBelli;
                if (justification.Fief == targetSettlement && missionType == Army.ArmyTypes.Besieger)
                {
                    result *= 4f;
                }

                if (targetSettlement.Town != null && (targetSettlement.Town == war.DefenderFront || 
                    targetSettlement.Town == war.AttackerFront))
                {
                    result *= 2f;
                }
            }

            return result;
        }
    }
}
