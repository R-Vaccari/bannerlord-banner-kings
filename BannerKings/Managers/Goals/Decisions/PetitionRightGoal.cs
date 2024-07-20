using System.Collections.Generic;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using BannerKings.Managers.Titles;
using BannerKings.Managers.Titles.Governments;
using System.Linq;

namespace BannerKings.Managers.Goals.Decisions
{
    public class PetitionRightGoal : Goal
    {
        private ContractRight chosenRight;
        public PetitionRightGoal(Hero fulfiller = null) : base("goal_claim_fief", fulfiller)
        {
        }

        public override bool TickClanLeaders => true;

        public override bool TickClanMembers => false;

        public override bool TickNotables => false;

        public override GoalCategory Category => GoalCategory.Kingdom;

        public override Goal GetCopy(Hero fulfiller)
        {
            PetitionRightGoal copy = new PetitionRightGoal(fulfiller);
            copy.Initialize(Name, Description);
            return copy;
        }

        public override bool IsAvailable()
        {
            Clan clan = GetFulfiller().Clan;
            return clan.Kingdom != null && clan.Kingdom.Leader != GetFulfiller();
        }

        public override bool IsFulfilled(out List<TextObject> failedReasons)
        {
            failedReasons = new List<TextObject>();

            Clan clan = GetFulfiller().Clan;

            if (clan.IsUnderMercenaryService)
            {
                failedReasons.Add(new TextObject("{=!}Mercenaries cannot petition rights."));
            }

            FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(GetFulfiller());
            if (title == null)
            {
                failedReasons.Add(new TextObject("{=!}You must have at least one title in order to petition rights."));
            }

            return failedReasons.Count == 0;
        }

        public override void ShowInquiry()
        {
            var options = new List<InquiryElement>();
            Clan clan = GetFulfiller().Clan;
            Kingdom kingdom = clan.Kingdom;
            FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(GetFulfiller());
            Hero suzerain = BannerKingsConfig.Instance.TitleManager.CalculateHeroSuzerain(GetFulfiller()).deJure;
            foreach (ContractAspect aspect in title.Contract.ContractAspects)
            {
                if (aspect is not ContractRight) continue;

                ContractRight right = (ContractRight)aspect;
                bool canFulfill = right.CanFulfill(suzerain, GetFulfiller());
                ExplainedNumber accept = BannerKingsConfig.Instance.DiplomacyModel.WillSuzerainAcceptRight(right, suzerain, GetFulfiller());
                ExplainedNumber influence = BannerKingsConfig.Instance.DiplomacyModel.GetRightInnfluenceCost(right, suzerain, GetFulfiller());
                int cost = MBRandom.RoundRandomized(influence.ResultNumber);

                TextObject hint = new TextObject("{=!}{DESCRIPTION}{newline}{newline}{EFFECTS}{newline}{newline}{EXPLANATION}")
                    .SetTextVariable("DESCRIPTION", right.Description)
                    .SetTextVariable("EFFECTS", right.EffectText);
                    
                if (!canFulfill)
                {
                    hint = hint.SetTextVariable("EXPLANATION", new TextObject("{=!}{SUZERAIN} cannot fulfil this request at this time.")
                        .SetTextVariable("SUZERAIN", suzerain.Name));
                }
                else if (accept.ResultNumber < 1f)
                {
                    hint = hint.SetTextVariable("EXPLANATION", 
                        new TextObject("{=!}{SUZERAIN} is not willing to fulfil your request.{newline}{newline}Explanations:{newline}{EXPLANATION}")
                        .SetTextVariable("EXPLANATION", accept.GetExplanations())
                        .SetTextVariable("SUZERAIN", suzerain.Name));
                }
                else
                {
                    hint = hint.SetTextVariable("EXPLANATION",
                        new TextObject("{=!}{SUZERAIN} will fulfil this request.")
                       .SetTextVariable("SUZERAIN", suzerain.Name));
                }

                options.Add(new InquiryElement(right,
                    new TextObject("{=Hyfgj4Mw}{TYPE} - {INFLUENCE}{INFLUENCE_ICON}")
                    .SetTextVariable("TYPE", right.Name)
                    .SetTextVariable("INFLUENCE", cost)
                    .SetTextVariable("INFLUENCE_ICON", Utils.TextHelper.INFLUENCE_ICON)
                    .ToString(),
                    null,
                    canFulfill && accept.ResultNumber >= 1f ,
                    hint.ToString()));;
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                Name.ToString(),
                new TextObject("{=!}Petition the fulfilment of one of your rights by your suzerain, {SUZERAIN}. Your suzerain may or may not be willing (according to their opinion) or able to fulfil your request. Your rights are defined in the contract of your realm's title. Your suzerain is defined by your highest title.")
                .SetTextVariable("SUZERAIN", suzerain.Name)
                .ToString(),
                options,
                true,
                1,
                1,
                GameTexts.FindText("str_done").ToString(),
                GameTexts.FindText("str_cancel").ToString(),
                delegate (List<InquiryElement> selectedOptions)
                {
                    chosenRight = (ContractRight)selectedOptions.First().Identifier;
                    ApplyGoal();
                },
                null,
                string.Empty));
        }

        public override void ApplyGoal()
        {
            Hero suzerain = BannerKingsConfig.Instance.TitleManager.CalculateHeroSuzerain(GetFulfiller()).deJure;
            ExplainedNumber influence = BannerKingsConfig.Instance.DiplomacyModel.GetRightInnfluenceCost(chosenRight, suzerain, GetFulfiller());
            GainKingdomInfluenceAction.ApplyForDefault(GetFulfiller(), influence.ResultNumber);
            chosenRight.Execute(suzerain, GetFulfiller());
        }

        public override void DoAiDecision()
        {
        }
    }
}
