using System.Collections.Generic;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Managers.Institutions
{
    public abstract class Institution
    {
        protected float influence;
        protected Dictionary<Hero, float> favors;

        protected Institution()
        {
            this.favors = new Dictionary<Hero, float>();
        }

        public float Influence => this.influence;

        public abstract void Destroy();

        public float GetFavor(Hero hero)
        {
            if (this.favors.ContainsKey(hero))
                return this.favors[hero];
            return 0f;
        }

        public void AddFavor(Hero hero, float favor)
        {
            if (this.favors.ContainsKey(hero))
                this.favors[hero] += favor;
            else this.favors.Add(hero, favor);
        }

    }
}
