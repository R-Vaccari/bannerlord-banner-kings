using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using BannerKings.Settings;
using BannerKings.Utils;
using BannerKings.Utils.Models;

namespace BannerKings.Models.Vanilla
{
    internal class BKBuildingEffectModel : DefaultBuildingEffectModel
    {
        public override float GetBuildingEffectAmount(Building building, BuildingEffectEnum effect)
        {
            var bulidingEffectAmount = building.BuildingType.GetBaseBuildingEffectAmount(effect, building.CurrentLevel);
            ExplainedNumber explainedNumber = new ExplainedNumber(bulidingEffectAmount, false, null);
            if (bulidingEffectAmount > 0)
            {
                if (effect == BuildingEffectEnum.Foodstock && building.Town.Governor != null && building.Town.Governor.GetPerkValue(DefaultPerks.Engineering.Battlements) && (building.BuildingType == DefaultBuildingTypes.CastleGranary || building.BuildingType == DefaultBuildingTypes.SettlementGranary))
                {
                    explainedNumber.Add(DefaultPerks.Engineering.Battlements.SecondaryBonus, DefaultPerks.Engineering.Battlements.Name, null);
                }
                #region DefaultPerks.Steward.Contractors
                if (BannerKingsSettings.Instance.EnableUsefulPerks && BannerKingsSettings.Instance.EnableUsefulStewardPerks)
                {
                    if (building.Town.IsTown)
                    {
                        DefaultPerks.Steward.Contractors.AddScaledGovernerPerkBonusForTownWithTownHeros(ref explainedNumber, true, building.Town);
                    }
                }
                else
                {
                    if (building.Town.IsTown)
                    {
                        PerkHelper.AddPerkBonusForTown(DefaultPerks.Steward.Contractors, building.Town, ref explainedNumber);
                    }
                }
                #endregion
                #region DefaultPerks.Steward.MasterOfPlanning
                if (BannerKingsSettings.Instance.EnableUsefulPerks && BannerKingsSettings.Instance.EnableUsefulStewardPerks)
                {
                    DefaultPerks.Steward.MasterOfPlanning.AddScaledGovernerPerkBonusForTownWithTownHeros(ref explainedNumber, true, building.Town);
                }
                else
                {
                    if (building.Town.Governor != null && building.Town.Governor.GetPerkValue(DefaultPerks.Steward.MasterOfPlanning))
                    {
                        explainedNumber.AddFactor(DefaultPerks.Steward.MasterOfPlanning.SecondaryBonus, DefaultPerks.Steward.MasterOfPlanning.Name);
                    }
                }
                #endregion
                Hero governor = building.Town.Governor;
                if (governor != null && governor.GetPerkValue(DefaultPerks.Charm.PublicSpeaker) && (building.BuildingType == DefaultBuildingTypes.SettlementMarketplace || building.BuildingType == DefaultBuildingTypes.FestivalsAndGamesDaily || building.BuildingType == DefaultBuildingTypes.SettlementForum))
                {
                    explainedNumber.AddFactor(DefaultPerks.Charm.PublicSpeaker.SecondaryBonus, DefaultPerks.Charm.PublicSpeaker.Name);
                }

            }
            return explainedNumber.ResultNumber;
        }
    }
}
