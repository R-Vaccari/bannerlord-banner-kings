using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths
{
    public abstract class DualisticFaith : Faith
    {
        public override TextObject GetFaithTypeName() => new TextObject("{=!}Dualism");
        public override TextObject GetFaithTypeExplanation() => new TextObject("{=!}Dualism is based on the dichotomy of two fundamental concepts, which can be opposing concepts - such as good and evil - or not. Dualists can believe both in a single, supreme god, or multiple gods.");

        public override float BlessingCostFactor => 1.3f;
        public override float FaithStrengthFactor => 1.1f;
        public override float JoinSocietyCost => 1.5f;
        public override float VirtueFactor => 2f;
        public override float ConversionCost => 0.8f;
    }
}
