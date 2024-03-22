using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;

namespace BannerKings.Models
{
    public abstract class PartyFoodModel : DefaultMobilePartyFoodConsumptionModel
    {
        public abstract float BirdFood { get; }
        public abstract float MuleFood { get; }
        public abstract float CattleFood { get; }
        public abstract float PigFood { get; }
        public abstract float HorseFood { get; }
        public abstract float WarhorseFood { get; } 
        public abstract float SheepFood { get; }    
        public abstract float CalculateAnimalFoodNeed(MobileParty party, bool ignoreCamels);
    }
}
