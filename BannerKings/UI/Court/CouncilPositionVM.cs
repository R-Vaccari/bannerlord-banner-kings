using System;
using BannerKings.Managers.Court;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;

namespace BannerKings.UI.Court
{
    public class CouncilPositionVM : HeroVM
    {
        private readonly CouncilPosition position;
        private readonly Action<string> setId;
        private readonly Action<string> updatePosition;
        private BasicTooltipViewModel hint;

        public CouncilPositionVM(CouncilPosition position, Action<string> setId, Action<string> updatePosition) : base(
            position.Member)
        {
            this.position = position;
            this.setId = setId;
            this.updatePosition = updatePosition;
            Hint = new BasicTooltipViewModel(() => UIHelper.GetCouncilPositionTooltip(position));
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
    }
}