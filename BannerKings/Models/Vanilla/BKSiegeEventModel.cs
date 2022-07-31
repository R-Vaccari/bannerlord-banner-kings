using BannerKings.Managers.Education;
using BannerKings.Managers.Skills;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;

namespace BannerKings.Models.Vanilla
{
    public class BKSiegeEventModel : DefaultSiegeEventModel
    {

        public override IEnumerable<SiegeEngineType> GetPrebuiltSiegeEnginesOfSettlement(Settlement settlement)
        {
            List<SiegeEngineType> baseResult = new List<SiegeEngineType>(base.GetPrebuiltSiegeEnginesOfSettlement(settlement));
            if (settlement.OwnerClan != null)
            {
                EducationData data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(settlement.Owner);
                if (data.Perks.Contains(BKPerks.Instance.CivilEngineer))
                    baseResult.Add(DefaultSiegeEngineTypes.Catapult);
            }

            return baseResult;
        }

        public override IEnumerable<SiegeEngineType> GetPrebuiltSiegeEnginesOfSiegeCamp(BesiegerCamp besiegerCamp)
        {
            List<SiegeEngineType> baseResult = new List<SiegeEngineType>(base.GetPrebuiltSiegeEnginesOfSiegeCamp(besiegerCamp));
            if (besiegerCamp.BesiegerParty.LeaderHero != null)
            {
                EducationData data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(besiegerCamp.BesiegerParty.LeaderHero);
                if (data.Perks.Contains(BKPerks.Instance.SiegeEngineer))
                    baseResult.Add(DefaultSiegeEngineTypes.Catapult);
            }

            return baseResult;
        }

        public override float GetSiegeEngineDamage(SiegeEvent siegeEvent, BattleSideEnum battleSide, SiegeEngineType siegeEngine, SiegeBombardTargets target)
        {
            float baseResult = base.GetSiegeEngineDamage(siegeEvent, battleSide, siegeEngine, target);
            MobileParty party = this.GetEffectiveSiegePartyForSide(siegeEvent, battleSide);

            if (party != null && party.LeaderHero != null)
            {
                EducationData data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(party.LeaderHero);
                if (battleSide == BattleSideEnum.Attacker && target == SiegeBombardTargets.Wall && data.Perks.Contains(BKPerks.Instance.SiegeEngineer))
                {
                    baseResult *= 1.1f;
                }
            }   

            return baseResult;
        }
    }
}
