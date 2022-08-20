using BannerKings.Managers.Education.Languages;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Education.Books;

public class BookType : BannerKingsObject
{
    public BookType(string id) : base(id)
    {
    }

    public ItemObject Item { get; private set; }

    public Language Language { get; private set; }

    public SkillObject Skill { get; private set; }

    public BookUse Use { get; private set; }

    public void Initialize(ItemObject bookItem, TextObject description, Language language, BookUse bookUse,
        SkillObject skill = null)
    {
        Initialize(null, description);
        this.Item = bookItem;
        this.description = description;
        this.Language = language;
        this.Use = bookUse;
        this.Skill = skill;
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
            if (hero.HeroDeveloper.CanAddFocusToSkill(Skill))
            {
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