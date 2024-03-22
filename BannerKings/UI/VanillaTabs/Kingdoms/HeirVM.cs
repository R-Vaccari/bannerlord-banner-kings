using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;

namespace BannerKings.UI.VanillaTabs.Kingdoms
{
    public class HeirVM : HeroVM
    {
        private BasicTooltipViewModel hint;
        public HeirVM(Hero hero, ExplainedNumber explanation, bool useCivilian = false) : base(hero, useCivilian)
        {
            Hint = new BasicTooltipViewModel(() => UIHelper.GetHeirTooltip(hero, explanation));
        }

        private void MakeHint()
        {
            Hint.ExecuteBeginHint();
        }

        private void CloseHint()
        {
            Hint.ExecuteEndHint();
        }

        [DataSourceProperty]
        public BasicTooltipViewModel Hint
        {
            get => hint;
            set
            {
                if (value != hint)
                {
                    hint = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }
    }
}
