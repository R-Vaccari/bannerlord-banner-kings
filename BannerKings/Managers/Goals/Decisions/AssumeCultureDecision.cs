using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Goals.Decisions
{
    internal class AssumeCultureDecision : Goal
    {
        private CultureObject culture;

        public AssumeCultureDecision() : base("goal_assume_cukture_decision", GoalCategory.Personal, GoalUpdateType.Manual)
        {
            var name = new TextObject("{=LcqUwqJz}Assume Culture");
            var description = new TextObject("{=XCancyYB}Assume a culture different than your current. Cultures can be assumed from settlements, your spouse or your faction leader. Direct family members will assume the culture as well. Assuming a culture yields a significant negative impact on clan renown.\n\n");

            Initialize(name, description);
        }

        private HashSet<CultureObject> GetCultureOptions()
        {
            var hero = GetFulfiller();
            HashSet<CultureObject> options = new HashSet<CultureObject>();
            foreach (var settlement in hero.Clan.Settlements)
            {
                if (settlement.Culture != hero.Culture)
                {
                    options.Add(settlement.Culture);
                }
            }

            if (hero.Spouse != null && hero.Spouse.Culture != hero.Culture)
            {
                options.Add(hero.Spouse.Culture);
            }

            var kingdom = hero.Clan.Kingdom;
            if (kingdom != null && kingdom.Leader != hero && kingdom.Leader.Culture != hero.Culture)
            {
                options.Add(kingdom.Leader.Culture);
            }

            return options;
        }

        internal override bool IsAvailable()
        {
            return true;
        }

        internal override bool IsFulfilled(out List<TextObject> failedReasons)
        {
            failedReasons = new List<TextObject>();

            if (GetCultureOptions().Count == 0)
            {
                failedReasons.Add(new TextObject("{=KvZByu7f}You do not have a settlement, spouse or faction leader with a different culture."));
            }

            if (GetFulfiller().Clan.Renown < 100f)
            {
                failedReasons.Add(new TextObject("{=7mHzFzBA}You need at least 100 clan renown."));
            }

            return failedReasons.IsEmpty();
        }

        internal override void ShowInquiry()
        {
            var options = new List<InquiryElement>();

            foreach (var culture in GetCultureOptions())
            {
                options.Add(new InquiryElement(culture,
                    culture.Name.ToString(), 
                    null));
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TextObject("{=LcqUwqJz}Assume Culture").ToString(),
                new TextObject("{=u4JwjfVz}Assume a culture different than your current. Cultures can be assumed from settlements, your spouse or your faction leader. Direct family members will assume the culture as well. Assuming a culture yields a significant negative impact on clan renown.").ToString(),
                options, 
                true, 
                1, 
                GameTexts.FindText("str_done").ToString(),
                GameTexts.FindText("str_cancel").ToString(),
                delegate (List<InquiryElement> selectedOptions)
                {
                    culture = (CultureObject)selectedOptions.First().Identifier;
                    ApplyGoal();
                }, 
                null, 
                string.Empty));
        }

        internal override void ApplyGoal()
        {
            var fulfiller = GetFulfiller();
            foreach (var hero in fulfiller.Clan.Heroes)
            {
                var leader = hero.Clan.Leader;
                if (hero == leader || leader.Children.Contains(hero) || hero == leader.Spouse ||
                    leader.Siblings.Contains(hero) || leader.Father == hero || leader.Mother == hero)
                {
                    hero.Culture = culture;
                }
            }

            MBInformationManager.AddQuickInformation(new TextObject("{=zV5itG5E}The {CLAN} has assumed the {CULTURE} culture.")
                .SetTextVariable("CLAN", fulfiller.Clan.Name)
                .SetTextVariable("CULTURE", fulfiller.Culture.Name),
                0, null, "event:/ui/notification/relation");

            fulfiller.Clan.Renown -= 100f;
        }

        public override void DoAiDecision()
        {
            throw new NotImplementedException();
        }
    }
}