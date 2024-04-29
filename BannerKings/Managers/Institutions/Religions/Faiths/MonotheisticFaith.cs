using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths
{
    public abstract class MonotheisticFaith : Faith
    {
        public override TextObject GetFaithTypeName() => new TextObject("{=!}Monotheism");
        public override TextObject GetFaithTypeExplanation() => new TextObject("{=!}");

        public override float BlessingCostFactor => 1f;
        public override float FaithStrengthFactor => 1f;
        public override float JoinSocietyCost => 1f;
        public override float VirtueFactor => 1f;
        public override float ConversionCost => 1f;
    }
}