using BannerKings.Behaviours;
using BannerKings.Managers.Education.Books;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Goals.Decisions
{
    internal class AcquireBookDecision : Goal
    {
        private BKEducationBehavior behavior;
        private BookType book;

        public AcquireBookDecision() : base("goal_acquire_book", GoalCategory.Personal, GoalUpdateType.Manual)
        {
            var name = new TextObject("{=DNAVAvqp}Acquire Book");
            var description = new TextObject("{=b4tSEcHn}Acquire a book from local book seller. Books can be read for skill improvements and progression in Scholarship.");

            Initialize(name, description);

            behavior = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKEducationBehavior>();
        }

        public override bool IsAvailable() => true;

        public override bool IsFulfilled(out List<TextObject> failedReasons)
        {
            failedReasons = new List<TextObject>();
            var fulfiller = GetFulfiller();

            var settlement = GetFulfiller().CurrentSettlement;
            if (settlement == null || behavior.GetBookSeller(settlement) == null)
            {
                failedReasons.Add(new TextObject("{=R93398Ci}Not in a settlement or there is no book seller available."));
            }

            if (fulfiller.PartyBelongedTo == null || fulfiller.IsPrisoner)
            {
                failedReasons.Add(new TextObject("{=qq99gmhr}Must be in a party and out of captivity."));
            }

            return failedReasons.Count == 0;
        }

        public override void ShowInquiry()
        {
            var elements = new List<InquiryElement>();
            var fulfiller = GetFulfiller();

            var allBooks = DefaultBookTypes.Instance.All;
            foreach (var element in behavior.GetBookRoster(fulfiller.CurrentSettlement))
            {
                var item = element.EquipmentElement.Item;
                var book = allBooks.FirstOrDefault(x => x.Item == element.EquipmentElement.Item);
                var price = book.Item.Value * 1000;

                TextObject hint = new TextObject("{=jAdG0wG9}{DESCRIPTION}\n{GOLD_AMOUNT}{GOLD_ICON}\nLanguage: {LANGUAGE}\n{SKILL}\n{TRAIT}")
                        .SetTextVariable("DESCRIPTION", book.Description)
                        .SetTextVariable("GOLD_AMOUNT", price)
                        .SetTextVariable("LANGUAGE", book.Language.Name)
                        .SetTextVariable("SKILL", book.Skill != null ? new TextObject("{=NrQdJeJU}Skill: {SKILL}").SetTextVariable("SKILL", book.Skill.Name)
                        : TextObject.Empty)
                        .SetTextVariable("TRAIT", book.Trait != null ? new TextObject("{=1P5txvhk}Trait: {TRAIT}").SetTextVariable("TRAIT", book.Trait.Name)
                        : TextObject.Empty);

                elements.Add(new InquiryElement(book, new TextObject("{=e8KTkKtX}{BOOK} ({LANGUAGE})")
                    .SetTextVariable("BOOK", item.Name)
                    .SetTextVariable("LANGUAGE", book.Language.Name).ToString(), 
                    null, fulfiller.Gold >= price,
                    hint.ToString()));
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TextObject("{=DNAVAvqp}Acquire Book").ToString(),
                new TextObject("{=2sftq1sF}Books can be read by those with the Literate perk. Skill books add xp to a specific skill while Focus books add both xp and a focus point, if possible. Dictionaries are used to help reading other books faster.")
                .ToString(),
                elements, 
                true, 
                1,
                1, 
                GameTexts.FindText("str_done").ToString(),
                GameTexts.FindText("str_cancel").ToString(),
                delegate (List<InquiryElement> selectedOptions)
                {
                    book = (BookType)selectedOptions.First().Identifier;
                    ApplyGoal();
                }, 
                null, 
                string.Empty));
        }

        public override void ApplyGoal()
        {
            var fulfiller = GetFulfiller();
            fulfiller.ChangeHeroGold(-book.Item.Value * 1000);
            fulfiller.PartyBelongedTo.ItemRoster.AddToCounts(book.Item, 1);
        }

        public override void DoAiDecision()
        {
            throw new NotImplementedException();
        }
    }
}