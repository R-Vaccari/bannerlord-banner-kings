using BannerKings.Managers.Kingdoms.Peerage;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.CampaignSystem.Settlements;
using BannerKings.Managers.Titles;
using BannerKings.Managers.Titles.Governments;

namespace BannerKings.Managers.Goals.Decisions
{
    public class PetitionRightGoal : Goal
    {
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
                failedReasons.Add(new TextObject("{=!}Mercenaries cannot claim fiefs"));
            }

            FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(GetFulfiller());
            if (title == null)
            {
                failedReasons.Add(new TextObject("{=!}You must have at least one title in order to petition rights"));
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

                TextObject hint = new TextObject("{=!}{SUZERAIN} will fulfil this request.")
                        .SetTextVariable("SUZERAIN", suzerain.Name);
                if (!canFulfill)
                {
                    hint = new TextObject("{=!}{SUZERAIN} cannot fulfil this request at this time.")
                        .SetTextVariable("SUZERAIN", suzerain.Name);
                }

                if (accept.ResultNumber < 1f)
                {
                    hint = new TextObject("{=!}{SUZERAIN} is not willing to fulfil your request.{newline}{newline}Explanations:{newline}{EXPLANATION}")
                        .SetTextVariable("EXPLANATION", accept.GetExplanations())
                        .SetTextVariable("SUZERAIN", suzerain.Name);
                }

                options.Add(new InquiryElement(right,
                    new TextObject("{=oFfExhaM}{DESCRIPTION}\n{EFFECTS}")
                    .SetTextVariable("DESCRIPTION", right.Description)
                    .SetTextVariable("EFFECTS", right.EffectText)
                    .ToString(),
                    null,
                    canFulfill && accept.ResultNumber >= 1f,
                    hint.ToString()));
            }

            InformationManager.ShowInquiry(new InquiryData(new TextObject("{=!}Petition Right").ToString(),
                new TextObject("{=!}Petition the fulfilment of one of your rights by your suzerain, {SUZERAIN}. Your suzerain may or may not be willing or able to fulfil your request. Your rights are defined in the contract of your realm's title. Your suzerain is defined by your highest title.")
                .SetTextVariable("SUZERAIN", suzerain.Name)
                .ToString(),
                true,
                true,
                GameTexts.FindText("str_selection_widget_accept").ToString(),
                GameTexts.FindText("str_selection_widget_cancel").ToString(),
                () => ApplyGoal(),
                null));
        }

        public override void ApplyGoal()
        {
            Clan clan = GetFulfiller().Clan;
            var decision = new PeerageKingdomDecision(clan.Kingdom.RulingClan, clan);
            if (clan != Clan.PlayerClan)
            {
                var election = new KingdomElection(decision);
                if (election.GetLikelihoodForOutcome(0) < 0.4f) return;
            }

            clan.Kingdom.AddDecision(decision, false);
            GainKingdomInfluenceAction.ApplyForDefault(GetFulfiller(), -decision.GetProposalInfluenceCost());

            if (clan == Clan.PlayerClan)
            {
                MBInformationManager.AddQuickInformation(new TextObject("{=5YsS2g7T}The Peers of {KINGDOM} will now vote on your request.")
                .SetTextVariable("KINGDOM", Clan.PlayerClan.Kingdom.Name),
                0,
                null,
                Utils.Helpers.GetKingdomDecisionSound());
            }
        }

        public override void DoAiDecision()
        {
        }
    }
}
