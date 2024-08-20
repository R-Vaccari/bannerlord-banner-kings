using BannerKings.Behaviours.Diplomacy;
using BannerKings.Behaviours.Diplomacy.Wars;
using BannerKings.CampaignContent.Traits;
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

                    if (mobileParty.TargetSettlement != null && mobileParty.TargetSettlement.Culture != mobileParty.ActualClan.Culture)
                    {
                        result *= 1.3f;
                    }
                }
                else if (defaultBehavior == AiBehavior.BesiegeSettlement || defaultBehavior == AiBehavior.DefendSettlement)
                {
                    result *= justification.ConquestWeight;
                }

                if (defaultBehavior == AiBehavior.BesiegeSettlement)
                {
                    Utils.Helpers.ApplyTraitEffect(mobileParty.LeaderHero, DefaultTraitEffects.Instance.ValorCommander, ref result);
                }

                if (defaultBehavior == AiBehavior.RaidSettlement)
                {
                    Utils.Helpers.ApplyTraitEffect(mobileParty.LeaderHero, DefaultTraitEffects.Instance.MercyRaid, ref result);
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
                if (justification.Fief == targetSettlement)
                {
                    if (missionType == Army.ArmyTypes.Besieger || missionType == Army.ArmyTypes.Defender)
                        result *= 1.5f;
                    else if (targetSettlement.IsVillage &&
                        justification.Fief.Town != null &&
                        justification.Fief.BoundVillages.Contains(targetSettlement.Village) &&
                        missionType == Army.ArmyTypes.Raider)
                    {
                        result *= 1.3f;
                    }
                }

                if (targetSettlement.Town != null && (targetSettlement.Town == war.DefenderFront || 
                    targetSettlement.Town == war.AttackerFront))
                {
                    result *= 1.25f;
                }
            }

            return result;
        }
    }
}
