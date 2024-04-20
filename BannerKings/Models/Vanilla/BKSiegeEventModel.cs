using System.Collections.Generic;
using BannerKings.Managers.Skills;
using BannerKings.Settings;
using BannerKings.Utils;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using static BannerKings.Utils.PerksHelpers;

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
            ExplainedNumber explaineResult = new ExplainedNumber(result);
            #region DefaultPerks.Steward.Sweatshops
            if (BannerKingsSettings.Instance.EnableUsefulPerks && BannerKingsSettings.Instance.EnableUsefulStewardPerks)
            {
                if (siegeEvent?.BesiegerCamp.LeaderParty != null && siegeEvent.BesiegerCamp.LeaderParty.HasPerk(DefaultPerks.Steward.Sweatshops, checkSecondaryRole: true))
                {
                    explaineResult.AddFactor(-DefaultPerks.Steward.Sweatshops.SecondaryBonus);
                }
                DefaultPerks.Steward.Sweatshops.AddScaledPerkBonus(ref explaineResult, false, effectiveSiegePartyForSide);
            }
            #endregion
            return explaineResult.ResultNumber;
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
            if (besiegerCamp.LeaderParty.LeaderHero != null)
            {
                var data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(besiegerCamp.LeaderParty
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