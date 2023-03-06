using System;
using BannerKings.Managers.Court;
using BannerKings.Managers.Court.Members.Tasks;
using BannerKings.UI.Items;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;

namespace BannerKings.UI.Court
{
    public class CouncilPositionVM : HeroVM
    {
        private readonly CouncilMember position;
        private readonly Action<string> setId;
        private readonly Action<string> updatePosition;
        private BasicTooltipViewModel hint;
        private BannerKingsSelectorVM<BKItemVM> selector;

        public CouncilPositionVM(CouncilMember position, Action<string> setId, Action<string> updatePosition) : base(
            position.Member)
        {
            this.position = position;
            this.setId = setId;
            this.updatePosition = updatePosition;
            Hint = new BasicTooltipViewModel(() => UIHelper.GetCouncilPositionTooltip(position));

            bool enabled = position.Clan.Leader == Hero.MainHero;
            Selector = new BannerKingsSelectorVM<BKItemVM>(enabled, 0, null);

            int selected = 0;
            int index = 0;
            foreach (CouncilTask option in position.Tasks)
            {
                Selector.AddItem(new BKItemVM(index,
                    true,
                    option.Description,
                    option.Name));

                if (option.StringId == position.CurrentTask.StringId)
                {
                    selected = index;
                }

                index++;
            }

            Selector.SelectedIndex = selected;
            Selector.SetOnChangeAction(OnChange);
        }

        private void OnChange(SelectorVM<BKItemVM> obj)
        {
            if (obj.SelectedItem != null)
            {
                var vm = obj.GetCurrentItem();
                var index = vm.Value;
                CouncilTask task = position.Tasks[index];
                position.SetTask(task);
            }
        }

        [DataSourceProperty] public string Title => position.Name.ToString();

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

        private void SetId()
        {
            setId?.Invoke(position.StringId.ToString());
        }

        private void UpdatePosition()
        {
            updatePosition?.Invoke(position.StringId.ToString());
        }

        [DataSourceProperty]
        public BannerKingsSelectorVM<BKItemVM> Selector
        {
            get => selector;
            set
            {
                if (value != selector)
                {
                    selector = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }
    }
}