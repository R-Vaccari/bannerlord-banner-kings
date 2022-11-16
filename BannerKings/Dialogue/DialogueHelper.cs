using BannerKings.Behaviours.Marriage;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Dialogue
{
    public static class DialogueHelper
    {
        public static TextObject GetRandomText(Hero conversationHero, List<DialogueOption> options)
        {
            var candidates = new List<(DialogueOption, float)>();
            foreach (var option in options) 
            {
                float score = 0f;
                score += MathF.Clamp(conversationHero.GetRelationWithPlayer() / 100 * option.RelationWeight, -1f, 1f);
                score += MathF.Clamp(conversationHero.GetHeroTraits().Calculating * option.CalculatingWeight, -1f, 1f);
                score += MathF.Clamp(conversationHero.GetHeroTraits().Mercy * option.MercyWeight, -1f, 1f);
                score += MathF.Clamp(conversationHero.GetHeroTraits().Honor * option.HonorWeight, -1f, 1f);
                score += MathF.Clamp(conversationHero.GetHeroTraits().Generosity * option.GenerousWeight, -1f, 1f);

                if (score > 0f && !option.IsDefault)
                {
                    candidates.Add((option, score));
                }
            }

            return MBRandom.ChooseWeighted(candidates).Text;
        }

        internal static List<DialogueOption> GetMarriageInadequateTexts(MarriageContract contract)
        {
            List<DialogueOption> result = new List<DialogueOption>();
            result.Add(new DialogueOption(
                new TextObject("{=!}The union between {PROPOSER} and {PROPOSED} is not adequate. Our {PROPOSED} is worth more than that.")
                .SetTextVariable("PROPOSED", contract.Proposed.Name)
                .SetTextVariable("PROPOSER", contract.Proposed.Name),
                -0.2f,
                0.2f,
                0f,
                0f,
                0f));


            result.Add(new DialogueOption(
                new TextObject("{=!}The union between {PROPOSER} and {PROPOSED} is not adequate. Our {PROPOSED} is worth more than that.")
                .SetTextVariable("PROPOSED", contract.Proposed.Name)
                .SetTextVariable("PROPOSER", contract.Proposed.Name),
                -0.2f,
                0.2f,
                0f,
                0f,
                0f));

            result.Add(new DialogueOption(
                new TextObject("{=!}I would not entertain giving away {PROPOSED} to a pack of mongrels such as the {CLAN}.")
                .SetTextVariable("PROPOSED", contract.Proposed.Name)
                .SetTextVariable("CLAN", contract.Proposer.Clan.Name),
                -1f,
                0f,
                -0.2f,
                0f,
                0f));


            result.Add(new DialogueOption(
                new TextObject("{=!}Unfortunately, I do not find the union between {PROPOSER} and {PROPOSED} to be adequate. You should improve the standing of your family within the realm, and then we may renegotiate.")
                .SetTextVariable("PROPOSED", contract.Proposed.Name)
                .SetTextVariable("PROPOSER", contract.Proposed.Name),
                0.3f,
                0.2f,
                0f,
                0.5f,
                0f,
                true));

            result.Add(new DialogueOption(
                new TextObject("{=!}My friend, the union of {PROPOSER} and {PROPOSED} is not one of fairness. Although it would be a pleasure to strengthen our houses, I do not find this to be the correct path.")
                .SetTextVariable("PROPOSED", contract.Proposed.Name)
                .SetTextVariable("PROPOSER", contract.Proposed.Name),
                1f,
                0f,
                0f,
                0.3f,
                0.2f,
                true));


            return result;
        }
    }
}
