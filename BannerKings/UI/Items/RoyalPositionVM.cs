using BannerKings.Managers.Court;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Library;

namespace BannerKings.UI.Items
{
    public class RoyalPositionVM : HeroVM
    {

        private CouncilMember position;

        public RoyalPositionVM(CouncilMember position) : base(position.Member)
        {
            this.position = position;
        }

        [DataSourceProperty]
        public string Title => position.GetName().ToString();

        [DataSourceProperty]
        public string String => position.Position.ToString();
    }
}
