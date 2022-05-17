using BannerKings.Managers.Court;
using System;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Library;

namespace BannerKings.UI.Items
{
    public class RoyalPositionVM : HeroVM
    {

        private CouncilMember position;
        private Action<string> setId;
        public RoyalPositionVM(CouncilMember position, Action<string> setId) : base(position.Member)
        {
            this.position = position;
            this.setId = setId;
        }

        private void SetId()
        {
            if (setId != null)
                setId(position.Position.ToString());
        }

        [DataSourceProperty]
        public string Title => position.GetName().ToString();
    }
}
