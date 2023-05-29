using BannerKings.Behaviours.PartyNeeds;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Models.BKModels
{
    public interface IPartyNeedsModel
    {
        public abstract float ArrowsPerSoldier { get; }
        public abstract float ShieldsPerSoldier { get; }
        public abstract float WeaponsPerSoldier { get; }
        public abstract float HorsesPerSoldier { get; }
        public abstract float ClothPerSoldier { get; }
        public abstract float ToolsPerSoldier { get; }
        public abstract float WoodPerSoldier { get; }
        public abstract float AnimalProductsPerSoldier { get; }
        public abstract float AlcoholPerSoldier { get; }
        public ExplainedNumber MinimumSoldiersThreshold(PartySupplies needs, bool descriptions);
        public ExplainedNumber CalculateAlcoholNeed(PartySupplies needs, bool descriptions);
        public ExplainedNumber CalculateArrowsNeed(PartySupplies needs, bool descriptions);
        public ExplainedNumber CalculateShieldsNeed(PartySupplies needs, bool descriptions);
        public ExplainedNumber CalculateWeaponsNeed(PartySupplies needs, bool descriptions);
        public ExplainedNumber CalculateHorsesNeed(PartySupplies needs, bool descriptions);
        public ExplainedNumber CalculateClothNeed(PartySupplies needs, bool descriptions);
        public ExplainedNumber CalculateToolsNeed(PartySupplies needs, bool descriptions);
        public ExplainedNumber CalculateWoodNeed(PartySupplies needs, bool descriptions);
        public ExplainedNumber CalculateAnimalProductsNeed(PartySupplies needs, bool descriptions);
    }
}
