using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;

namespace BannerKings.Models.Vanilla.Abstract
{
    public abstract class MarriageModel : DefaultMarriageModel
    {
        public abstract ExplainedNumber IsMarriageAdequate(Hero proposer,
            Hero secondHero,
            bool isConsort = false,
            bool explanations = false);

        public abstract ExplainedNumber GetSpouseScore(Hero hero, bool explanations = false);
        public abstract ExplainedNumber GetDowryValue(Hero hero, bool arrangedMarriage = false, bool explanations = false);
        public abstract ExplainedNumber GetInfluenceCost(Hero proposed, bool explanations = false);
        public abstract IEnumerable<Hero> DiscoverAncestors(Hero hero, int n);
    }
}
