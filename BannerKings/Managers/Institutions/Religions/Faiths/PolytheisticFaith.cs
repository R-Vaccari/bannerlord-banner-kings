using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths
{
    public abstract class PolytheisticFaith : Faith
    {
        public override TextObject GetFaithTypeName() => new TextObject("{=!}Polytheism");
        public override TextObject GetFaithTypeExplanation() => new TextObject("{=!}Polytheists believe in the existence of multiple gods or goddesses, all of which may be worthy of worship. As such, they are open to the belief that the gods of other faiths do, in fact, exist.");

        public override float BlessingCostFactor => 1.3f;
        public override float FaithStrengthFactor => 0.7f;
        public override float JoinSocietyCost => 0.5f;
        public override float VirtueFactor => 0.5f;
        public override float ConversionCost => 1.3f;
    }
}