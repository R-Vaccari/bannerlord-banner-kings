using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths
{
    public abstract class HenotheisticFaith : Faith
    {
        public override TextObject GetFaithTypeName() => new TextObject("{=!}Henotheism");
        public override TextObject GetFaithTypeExplanation() => new TextObject("{=!}Henotheism believe in the existence of multiple gods or godly divinities, yet prescribe higher importance to one, supreme god. All of these divinities are considered worthy or worship.");

        public override float BlessingCostFactor => 1.15f;
        public override float FaithStrengthFactor => 0.85f;
        public override float JoinSocietyCost => 0.75f;
        public override float VirtueFactor => 0.75f;
        public override float ConversionCost => 1.15f;
    }
}
