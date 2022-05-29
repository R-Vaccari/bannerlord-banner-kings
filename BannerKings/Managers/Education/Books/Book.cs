using BannerKings.Managers.Education.Languages;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Education.Books
{
    public class Book
    {
        private BookType type;
        private Language translatedTo;

        public Book(BookType type, Language translatedTo = null)
        {
            this.type = type;
            this.translatedTo = translatedTo;
        }

        public ItemObject Item => type.Item;
        public Language Language
        {
            get
            {
                if (translatedTo != null) return translatedTo;
                return type.Language;
            }
        }
        public TextObject Description => type.Description;
    }
}
