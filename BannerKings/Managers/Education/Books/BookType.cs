using BannerKings.Managers.Education.Languages;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
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

        public void Initialize(ItemObject bookItem, TextObject description, Language language, BookUse bookUse, SkillObject skill = null)
        {
            Initialize(null, description);
            Item = bookItem;
            this.description = description;
            Language = language;
            Use = bookUse;
            Skill = skill;
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
        }
    }

    public enum BookUse
    {
        Skillbook,
        Focusbook,
        Dictionary
    }
}