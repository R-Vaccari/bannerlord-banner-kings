using BannerKings.Managers.Education.Languages;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Education.Books
{
    public class BookType : BannerKingsObject
    {
        public BookType(string id) : base(id)
        {
        }

        public ItemObject Item { get; private set; }

        public Language Language { get; private set; }

        public SkillObject Skill { get; private set; }

        public BookUse Use { get; private set; }
        public TraitObject Trait { get; private set; }

        public void Initialize(ItemObject bookItem, TextObject description, Language language, BookUse bookUse, 
            SkillObject skill = null,
            TraitObject trait = null)
        {
            Initialize(null, description);
            Item = bookItem;
            this.description = description;
            Language = language;
            Use = bookUse;
            Skill = skill;
            Trait = trait;
        }

        public void FinishBook(Hero hero)
        {
            if (Skill == null)
            {
                return;
            }

            if (Use == BookUse.Skillbook)
            {
                hero.AddSkillXp(Skill, 1500f);
            }
            else
            {
                hero.AddSkillXp(Skill, 300f);
                if (hero.HeroDeveloper.GetFocus(Skill) < Campaign.Current.Models.CharacterDevelopmentModel.MaxFocusPerSkill)
                {
                    hero.HeroDeveloper.UnspentFocusPoints++;
                    hero.HeroDeveloper.AddFocus(Skill, 1);
                }
                else
                {
                    hero.AddSkillXp(Skill, 1000f);
                }
            }

            if (Trait != null)
            {
                if (hero == Hero.MainHero)
                {
                    InformationManager.ShowInquiry(new InquiryData(
                        new TextObject("{=!}Inspiration from {BOOK}")
                        .SetTextVariable("BOOK", Name).ToString(),
                        new TextObject("{=!}Upon finishing the {BOOK}, you feel inspired by the text to change your sense of {TRAIT}. Will you adopt these teachings?")
                        .SetTextVariable("BOOK", Name)
                        .SetTextVariable("TRAIT", Trait.Name).ToString(),
                        true,
                        true,
                        GameTexts.FindText("str_selection_widget_accept").ToString(),
                        GameTexts.FindText("str_selection_widget_cancel").ToString(),
                        () => hero.SetTraitLevel(Trait, hero.GetTraitLevel(Trait) + 1),
                        null), 
                        true);
                }
            }
        }
    }

    public enum BookUse
    {
        Skillbook,
        Focusbook,
        Dictionary
    }
}