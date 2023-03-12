using System;
using BannerKings.Managers.Court;
using BannerKings.Managers.Court.Members.Tasks;
using BannerKings.UI.Items;
using BannerKings.UI.Items.UI;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Court
{
    public class CouncilPositionVM : HeroVM
    {
        private readonly CouncilMember position;
        private readonly Action<string> setId;
        private readonly Action<string> updatePosition;
        private BasicTooltipViewModel hint;
        private BannerKingsSelectorVM<BKItemVM> selector;
        private MBBindingList<InformationElement> positionInfo;

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

            PositionInfo = new MBBindingList<InformationElement>();
            if (position.Member != null)
            {
                PositionInfo.Add(new InformationElement(new TextObject("{=Oy8rn07Z}Competence:").ToString(),
                                (position.Competence.ResultNumber * 100f).ToString("0.00") + '%',
                                new TextObject("{=hdbcAeax}This councillor's competence in their position. The more competent they are, the more likely they are to trigger the tasks' effects and often with better results.").ToString()));

                PositionInfo.Add(new InformationElement(new TextObject("{=hrGvzpLz}Efficiency:").ToString(),
                                (position.CurrentTask.Efficiency * 100f).ToString("0.00") + '%',
                                new TextObject("{=WFrQAqOZ}This task's current efficiency. Efficiency is stacked on top of competence, meaning that a task only functions fully as intended when at 100% efficiency. Some tasks are always at 100%. Others start at 0% and slowly build up to 100%. This means that some tasks require time investment, and switching between them is not productive.").ToString()));
            }
        }

        [DataSourceProperty] public string Title => position.Name.ToString();

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

        private void SetId()
        {
            setId?.Invoke(position.StringId.ToString());
        }

        private void UpdatePosition()
        {
            updatePosition?.Invoke(position.StringId.ToString());
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

        [DataSourceProperty]
        public MBBindingList<InformationElement> PositionInfo
        {
            get => positionInfo;
            set
            {
                if (value != positionInfo)
                {
                    positionInfo = value;
                    OnPropertyChangedWithValue(value);
                }
            }
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