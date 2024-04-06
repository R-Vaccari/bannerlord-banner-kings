using BannerKings.Behaviours.Relations;
using BannerKings.Managers.Titles;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Models.BKModels.Abstract
{
    public abstract class RelationsModel
    {
        public abstract int HostileFaithModifier { get; }
        public abstract int UntoleratedFaithModifier { get; }
        public abstract int ToleratedFaithModifier { get; }
        public abstract int IlegitimateFiefModifier { get; }
        public abstract int CloseFamilyModifier { get; }
        public abstract int PersonalityTraitModifier { get; }
        public abstract List<RelationsModifier> CalculateModifiers(HeroRelations heroRelations, Hero target);
        public abstract ExplainedNumber CalculateModifiersExplained(HeroRelations heroRelations, Hero target, bool explained = false);
        public abstract ExplainedNumber CalculateModifiersExplained(List<RelationsModifier> modifiers, bool explained = false);
        public abstract int GetClaimRelationImpact(FeudalTitle title);
    }
}
