using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Religion
{
    public class ReligionElementVM : ViewModel
    {
        private readonly TextObject name;
        private readonly TextObject description;
        private readonly TextObject hint;

        public ReligionElementVM(TextObject name, TextObject description, TextObject hint)
        {
            this.name = name;
            this.description = description;
            this.hint = hint;
        }

        [DataSourceProperty] public string Name => name.ToString();

        [DataSourceProperty] public string Description => description.ToString();

        [DataSourceProperty] public HintViewModel Hint => new(hint);
    }
}