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
        public ExplainedNumber MinimumSoldiersThreshold(PartyNeeds needs, bool descriptions);
        public ExplainedNumber CalculateAlcoholNeed(PartyNeeds needs, bool descriptions);
        public ExplainedNumber CalculateArrowsNeed(PartyNeeds needs, bool descriptions);
        public ExplainedNumber CalculateShieldsNeed(PartyNeeds needs, bool descriptions);
        public ExplainedNumber CalculateWeaponsNeed(PartyNeeds needs, bool descriptions);
        public ExplainedNumber CalculateHorsesNeed(PartyNeeds needs, bool descriptions);
        public ExplainedNumber CalculateClothNeed(PartyNeeds needs, bool descriptions);
        public ExplainedNumber CalculateToolsNeed(PartyNeeds needs, bool descriptions);
        public ExplainedNumber CalculateWoodNeed(PartyNeeds needs, bool descriptions);
        public ExplainedNumber CalculateAnimalProductsNeed(PartyNeeds needs, bool descriptions);
    }
}
