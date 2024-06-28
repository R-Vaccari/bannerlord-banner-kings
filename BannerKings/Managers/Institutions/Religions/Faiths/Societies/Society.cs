using System.Collections.Generic;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Societies
{
    public class Society : BannerKingsObject
    {
        public Society(string stringId) : base(stringId)
        {
        }

        public void Initialize(TextObject name,
            TextObject description,
            Dictionary<TraitObject, float> traits,
            SocietyRank firstRank,
            SocietyRank secondRank,
            SocietyRank thirdRank)
        {
            Initialize(name, description);
            Traits = traits;
            FirstRank = firstRank;
            SecondRank = secondRank;
            ThirdRank = thirdRank;
        }

        public SocietyRank FirstRank { get; private set; }
        public SocietyRank SecondRank { get; private set; }
        public SocietyRank ThirdRank { get; private set; }
        public Dictionary<TraitObject, float> Traits { get; private set; }

        public TextObject GetEffectsText() => new TextObject("{=!}Effects{newline}Rank I: {FIRST}{newline}Rank II: {SECOND}{newline}Rank III: {THIRD}")
            .SetTextVariable("FIRST", FirstRank.Description)
            .SetTextVariable("SECOND", SecondRank.Description)
            .SetTextVariable("THIRD", ThirdRank.Description);
    }
}
