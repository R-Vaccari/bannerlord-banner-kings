using BannerKings.Managers.Education.Lifestyles;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers.CampaignStart
{
    public class StartOption : BannerKingsObject
    {
        private TextObject shortDescription;
        private int gold;
        private int food;
        private int troops;
        private int morale;
        private float influence;
        private float criminal;
        private CultureObject culture;
        private Lifestyle lifestyle;
        private Action action;
        public StartOption(string id) : base(id)
        {
        }

        public void Initialize(TextObject name, TextObject description, TextObject shortDescription, int gold, int food, int troops, int morale, float influence, 
            Action action, float criminal = 0f, CultureObject culture = null, Lifestyle lifestyle = null)
        {
            Initialize(name, description);
            this.shortDescription = shortDescription;
            this.gold = gold;
            this.food = food;
            this.troops = troops;
            this.morale = morale;
            this.influence = influence;
            this.action = action;
            this.criminal = criminal;
            this.culture = culture;
            this.lifestyle = lifestyle;
        }

        public TextObject ShortDescription => shortDescription;
        public int Gold => gold;
        public int Food => food;
        public int Troops => troops;
        public int Morale => morale;
        public float Influence => influence;
        public bool IsCriminal => criminal > 0f;
        public float Criminal => criminal;
        public CultureObject Culture => culture;
        public Lifestyle Lifestyle => lifestyle;
        public Action Action => action;
    }
}
