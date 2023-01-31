using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions
{
    public class Divinity : BannerKingsObject
    {
        private int blessingCost;

        public Divinity(string id) : base(id)
        {
        }

        public void Initialize(TextObject name, TextObject description, TextObject effects,
            TextObject secondaryTitle = null, int blessingCost = 300, bool isIndefiniteMembership = false)
        {
            Initialize(name, description);
            Effects = effects;
            SecondaryTitle = secondaryTitle ?? new TextObject("{=!}");
            this.blessingCost = blessingCost;
            IsIndefiniteMembership = isIndefiniteMembership;
        }

        public int BaseBlessingCost => blessingCost;

        public int BlessingCost(Hero hero)
        {
            float baseCost = blessingCost;
            if (hero.GetPerkValue(BKPerks.Instance.TheologyBlessed))
            {
                baseCost *= 0.9f;
            }

            return (int)baseCost;
        }

        public TextObject Effects { get; private set; }
        public TextObject SecondaryTitle { get; private set; }
        public bool IsIndefiniteMembership { get; private set; }
    }
}