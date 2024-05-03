using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;

namespace BannerKings.Models.Vanilla.Abstract
{
    public abstract class ArmyModel : DefaultArmyManagementCalculationModel
    {
        public abstract bool CanCreateArmy(Hero armyLeader);
    }
}
