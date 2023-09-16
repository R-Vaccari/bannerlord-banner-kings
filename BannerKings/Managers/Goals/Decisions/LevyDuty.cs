using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers.Titles;
using BannerKings.Managers.Titles.Governments;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Goals.Decisions
{
    public class LevyDuty : Goal
    {
        private ContractDuty duty;
        private Hero vassal;
        public LevyDuty(Hero fulfiller = null) : base("LevyDuty", GoalCategory.Kingdom, GoalUpdateType.Hero, fulfiller)
        {
            Initialize(new TextObject("{=!}Levy Duty"),
                new TextObject("{=!}Levy Duty"));
        }

        public override bool IsAvailable()
        {
            return Clan.PlayerClan.Kingdom != null;
        }

        public override bool IsFulfilled(out List<TextObject> failedReasons)
        {
            failedReasons = new List<TextObject>();

            Hero fulfiller = GetFulfiller();

            List<Hero> vassals = BannerKingsConfig.Instance.TitleManager.CalculateAllVassals(fulfiller.Clan);
            if (vassals.Count == 0)
            {
                failedReasons.Add(new TextObject("{=!}You don't have any vassals to levy duties from"));
            }

            FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(fulfiller.Clan.Kingdom);
            if (title == null)
            {
                failedReasons.Add(new TextObject("{=!}Kingdom is not legally stablished with a title"));
            }

            return failedReasons.IsEmpty();
        }
      
        public override void ShowInquiry()
        {
            Hero fulfiller = GetFulfiller();
            FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(fulfiller.Clan.Kingdom);

            List<InquiryElement> duties = new List<InquiryElement>();
            foreach (var duty in title.Contract.ContractAspects)
            {
                if (duty is ContractDuty)
                {
                    duties.Add(new InquiryElement(
                        duty,
                        duty.Name.ToString(),
                        null,
                        true,
                        duty.Description.ToString()));
                }
            }

            List<InquiryElement> vassals = new List<InquiryElement>();
            foreach (var vassal in BannerKingsConfig.Instance.TitleManager.CalculateAllVassals(fulfiller.Clan))
            {
                vassals.Add(new InquiryElement(
                    duty,
                    duty.Name.ToString(),
                    new ImageIdentifier(CampaignUIHelper.GetCharacterCode(vassal.CharacterObject)),
                    true,
                    duty.CalculateDuty(fulfiller, vassal).ToString()));
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TextObject("{=!}Levy Duty (1/2)").ToString(),
                new TextObject("{=!}As a suzerain, you are capable of levying duties from your legal vassals. Such duties depend exclusively on the contract of the realm you serve.").ToString(),
                duties,
                true,
                duties.Count,
                GameTexts.FindText("str_accept").ToString(),
                GameTexts.FindText("str_reject").ToString(),
                delegate (List<InquiryElement> list)
                {
                    duty = list.First().Identifier as ContractDuty;
                    MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                        new TextObject("{=!}Levy Duty (2/2)").ToString(),
                        new TextObject("{=!}As a suzerain, you are capable of levying duties from your legal vassals. Such duties depend exclusively on the contract of the realm you serve.").ToString(),
                        vassals,
                        true,
                        duties.Count,
                        GameTexts.FindText("str_accept").ToString(),
                        GameTexts.FindText("str_reject").ToString(),
                        delegate (List<InquiryElement> list)
                        {
                            vassal = list.First().Identifier as Hero;
                            ApplyGoal();
                        },
                        (List<InquiryElement> list) => ShowInquiry()
                        ));
                },
                null));
        }

        public override void ApplyGoal()
        {
            var hero = GetFulfiller();
            duty.ExecuteDuty(hero, vassal);
        }

        public override void DoAiDecision()
        {
            Hero fulfiller = GetFulfiller();
           

            ApplyGoal();
        }
    }
}