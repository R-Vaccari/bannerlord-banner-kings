using System.Collections.Generic;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Managers.Institutions
{
    public abstract class Institution
    {

        protected Hero leader;
        protected float influence;
        protected Dictionary<Hero, float> favors;

        public Institution()
        {
            this.favors = new Dictionary<Hero, float>();
        }

        public Hero Leader
        {
            get
            {
                if (this.leader == null || !this.leader.IsAlive || this.leader.IsActive)
                    this.leader = GenerateLeader();
                return this.leader;
            }
        }

        public float Influence => this.influence;

        public abstract Hero GenerateLeader();

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
