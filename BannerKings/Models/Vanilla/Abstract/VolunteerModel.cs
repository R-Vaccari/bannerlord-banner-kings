using System.Collections.Generic;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using static BannerKings.Managers.PopulationManager;
using BannerKings.Managers.Populations;

namespace BannerKings.Models.Vanilla.Abstract
{
    public abstract class VolunteerModel : DefaultVolunteerModel
    {
        public abstract float GetPopTypeSpawnChance(PopulationData data, PopType popType);
        public abstract List<ValueTuple<PopType, float>> GetMilitaryClasses(Settlement settlement);
        public abstract ExplainedNumber GetDraftEfficiency(Hero hero, Settlement settlement);
        public abstract ExplainedNumber GetMilitarism(Settlement settlement);
        public abstract ExplainedNumber CalculateMaximumRecruitmentIndex(Hero buyerHero, Hero sellerHero, int useValueAsRelation = -101, bool explanations = false);
    }
}
