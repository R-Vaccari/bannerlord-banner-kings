using System.Collections.Generic;
using BannerKings.Managers.Skills;
using BannerKings.Settings;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;

namespace BannerKings.Models.Vanilla
{
    public class BKSiegeEventModel : DefaultSiegeEventModel
    {
        public override float GetConstructionProgressPerHour(SiegeEngineType type, SiegeEvent siegeEvent,
            ISiegeEventSide side)
        {
            var result = base.GetConstructionProgressPerHour(type, siegeEvent, side);
            var effectiveSiegePartyForSide = GetEffectiveSiegePartyForSide(siegeEvent, side.BattleSide);
            if (effectiveSiegePartyForSide is {LeaderHero: { }})
            {
                var data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(effectiveSiegePartyForSide
                    .LeaderHero);
                if (data.HasPerk(BKPerks.Instance.SiegeOverseer))
                {
                    result *= 1.2f;
                }
            }

            if (BannerKingsSettings.Instance.LongerSieges > 0f)
            {
                result *= (1f - BannerKingsSettings.Instance.LongerSieges);
            }

            return result;
        }

        public override IEnumerable<SiegeEngineType> GetPrebuiltSiegeEnginesOfSettlement(Settlement settlement)
        {
            var baseResult = new List<SiegeEngineType>(base.GetPrebuiltSiegeEnginesOfSettlement(settlement));
            if (settlement.OwnerClan != null)
            {
                var data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(settlement.Owner);
                if (data.Perks.Contains(BKPerks.Instance.CivilEngineer))
                {
                    baseResult.Add(DefaultSiegeEngineTypes.Catapult);
                }
            }

            return baseResult;
        }

        public override IEnumerable<SiegeEngineType> GetPrebuiltSiegeEnginesOfSiegeCamp(BesiegerCamp besiegerCamp)
        {
            var baseResult = new List<SiegeEngineType>(base.GetPrebuiltSiegeEnginesOfSiegeCamp(besiegerCamp));
            if (besiegerCamp.BesiegerParty.LeaderHero != null)
            {
                var data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(besiegerCamp.BesiegerParty
                    .LeaderHero);
                if (data.Perks.Contains(BKPerks.Instance.SiegeEngineer))
                {
                    baseResult.Add(DefaultSiegeEngineTypes.Ballista);
                }
            }

            return baseResult;
        }

        public override float GetSiegeEngineDamage(SiegeEvent siegeEvent, BattleSideEnum battleSide,
            SiegeEngineType siegeEngine, SiegeBombardTargets target)
        {
            var baseResult = base.GetSiegeEngineDamage(siegeEvent, battleSide, siegeEngine, target);
            var party = GetEffectiveSiegePartyForSide(siegeEvent, battleSide);

            if (party is {LeaderHero: { }})
            {
                var data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(party.LeaderHero);
                if (battleSide == BattleSideEnum.Attacker && target == SiegeBombardTargets.Wall &&
                    data.Perks.Contains(BKPerks.Instance.SiegeEngineer))
                {
                    baseResult *= 1.1f;
                }
            }

            return baseResult;
        }
    }
}