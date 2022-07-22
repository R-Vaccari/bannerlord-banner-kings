using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;

namespace BannerKings.UI.Items
{
    public class TripleStringItemVM : StringPairItemVM
    {
        private string value2;
        public TripleStringItemVM(string definition, string value, string value2, BasicTooltipViewModel hint = null) : base (definition, value, hint)
        {
            this.value2 = value2;
        }

        [DataSourceProperty]
        public string SecondValue => value2;
    }
}
