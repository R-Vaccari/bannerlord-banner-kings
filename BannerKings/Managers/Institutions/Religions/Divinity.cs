using BannerKings.Managers.Skills;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions
{
    public class Divinity : BannerKingsObject
    {
        private int blessingCost;
        private Func<Hero, bool> canBeInducted;

        public Divinity(string id) : base(id)
        {
        }

        public void Initialize(TextObject name, TextObject description, TextObject effects,
            TextObject secondaryTitle = null, int blessingCost = 300, 
            TextObject dialogue = null,
            TextObject lastDialogue = null,
            Settlement shrine = null, 
            Func<Hero, bool> canBeInducted = null)
        {
            Initialize(name, description);
            Effects = effects;
            SecondaryTitle = secondaryTitle ?? new TextObject("{=!}");
            Dialogue = dialogue;
            LastDialogue = lastDialogue;
            Shrine = shrine;
            this.blessingCost = blessingCost;
            this.canBeInducted = canBeInducted;
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

        public bool CanBeInducted(Hero hero)
        {
            if (canBeInducted != null)
            {
                return canBeInducted(hero);
            }

            return true;
        }

        public TextObject Effects { get; private set; }
        public TextObject Dialogue { get; private set; }
        public TextObject LastDialogue { get; private set; }
        public TextObject SecondaryTitle { get; private set; }
        public Settlement Shrine { get; private set; }
    }
}