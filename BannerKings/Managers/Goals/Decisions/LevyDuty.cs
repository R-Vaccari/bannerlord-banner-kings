using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers.Titles;
using BannerKings.Managers.Titles.Governments;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Goals.Decisions
{
    public class LevyDuty : Goal
    {
        private ContractDuty duty;
        private Hero vassal;
        public LevyDuty(Hero fulfiller = null) : base("LevyDuty", GoalCategory.Kingdom, GoalUpdateType.Hero, fulfiller)
        {
            Initialize(new TextObject("{=Q8hIKnka}Levy Duty"),
                new TextObject("{=jbiRrtAW}Levy Duty"));
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
                failedReasons.Add(new TextObject("{=HRL3zkzA}You don't have any vassals to levy duties from"));
            }

            FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(fulfiller.Clan.Kingdom);
            if (title == null)
            {
                failedReasons.Add(new TextObject("{=NGClDDOB}Kingdom is not legally stablished with a title"));
            }

            return failedReasons.IsEmpty();
        }
      
        public override void ShowInquiry()
        {
            Hero fulfiller = GetFulfiller();
            FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(fulfiller.Clan.Kingdom);

            List<InquiryElement> vassals = new List<InquiryElement>();
            foreach (var vassal in BannerKingsConfig.Instance.TitleManager.CalculateAllVassals(fulfiller.Clan))
            {
                vassals.Add(new InquiryElement(
                    vassal,
                    vassal.Name.ToString(),
                    new ImageIdentifier(CampaignUIHelper.GetCharacterCode(vassal.CharacterObject)),
                    true,
                    string.Empty));
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TextObject("{=hKJE5vJN}Levy Duty (1/2)").ToString(),
                new TextObject("{=xuYLfEqa}As a suzerain, you are capable of levying duties from your legal vassals. Such duties depend exclusively on the contract of the realm you serve.").ToString(),
                vassals,
                true,
                1,
                GameTexts.FindText("str_accept").ToString(),
                GameTexts.FindText("str_reject").ToString(),
                delegate (List<InquiryElement> list)
                {
                    vassal = list.First().Identifier as Hero;
                    List<InquiryElement> duties = new List<InquiryElement>();
                    foreach (var duty in title.Contract.ContractAspects)
                    {
                        if (duty is ContractDuty)
                        {
                            bool available = true;
                            ContractDuty contractDuty = (ContractDuty)duty;
                            TextObject description = new TextObject("{=pG8N2TW5}{DESCRIPTION}{newline}{newline}Results:{newline}{RESULTS}")
                               .SetTextVariable("DESCRIPTION", duty.Description)
                               .SetTextVariable("RESULTS", contractDuty.GetResults(fulfiller, vassal));
                            
                            FeudalTitle vassalHighest = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(vassal);     
                            if (vassalHighest != null && vassalHighest.GetDutyTime(contractDuty)
                                .ElapsedSeasonsUntilNow < contractDuty.SeasonsDelay)
                            {
                                available = false;
                                description = new TextObject("{=CgEtkPdz}It has been {SEASONS} seasons since this duty was last levied from the {TITLE}. This duty may be levied every {DELAY} seasons.")
                                .SetTextVariable("SEASONS", vassalHighest.GetDutyTime(contractDuty).ElapsedSeasonsUntilNow.ToString("0.0"))
                                .SetTextVariable("TITLE", vassalHighest.FullName)
                                .SetTextVariable("DELAY", contractDuty.SeasonsDelay);
                            }

                            if (!contractDuty.CanFulfill(fulfiller, vassal))
                            {
                                available = false;
                                description = new TextObject("{=jpTP3DwZ}{HERO} is not able to fulfill this duty at this time.")
                                .SetTextVariable("HERO", vassal.Name);
                            }

                            duties.Add(new InquiryElement(
                                duty,
                                duty.Name.ToString(),
                                null,
                                available,
                                description.ToString()));
                        }
                    }
                    
                    MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                        new TextObject("{=nFvY6Yyw}Levy Duty (2/2)").ToString(),
                        new TextObject("{=xuYLfEqa}As a suzerain, you are capable of levying duties from your legal vassals. Such duties depend exclusively on the contract of the realm you serve.").ToString(),
                        duties,
                        true,
                        1,
                        GameTexts.FindText("str_accept").ToString(),
                        GameTexts.FindText("str_reject").ToString(),
                        delegate (List<InquiryElement> list)
                        {
                            duty = list.First().Identifier as ContractDuty;
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