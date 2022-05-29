using BannerKings.Managers.Education.Languages;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Education.Books
{
    public class BookType
    {
        private ItemObject bookItem;
        private TextObject description;
        private Language language;
        private Action<Hero> onBookFinished;

        public BookType(ItemObject bookItem, TextObject description, Language language, Action<Hero> onBookFinished)
        {
            this.bookItem = bookItem;
            this.description = description;
            this.language = language;
        }

        public void FinishBook(Hero hero)
        {
            onBookFinished(hero);
        }

        public ItemObject Item => bookItem;
        public Language Language => language;
        public TextObject Description => description;
    }
}
