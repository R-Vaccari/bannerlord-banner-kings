using BannerKings.Managers.Court;
using System;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Library;

namespace BannerKings.UI.Items
{
    public class CouncilPositionVM : HeroVM
    {

        private CouncilMember position;
        private Action<string> setId, updatePosition;
        public CouncilPositionVM(CouncilMember position, Action<string> setId, Action<string> updatePosition) : base(position.Member)
        {
            this.position = position;
            this.setId = setId;
            this.updatePosition = updatePosition;
        }

        private void SetId()
        {
            if (setId != null)
                setId(position.Position.ToString());
        }

        private void UpdatePosition()
        {
            if (updatePosition != null)
                updatePosition(position.Position.ToString());
        }

        [DataSourceProperty]
        public string Title => position.GetName().ToString();
    }
}
