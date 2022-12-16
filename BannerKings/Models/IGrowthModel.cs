﻿using BannerKings.Managers.Populations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings.Models
{
    public interface IGrowthModel : IBannerKingsModel
    {
        public ExplainedNumber CalculateEffect(Settlement settlement, PopulationData data, bool descriptions = false);
    }
}