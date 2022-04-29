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
            favors = new Dictionary<Hero, float>();
        }

        public Hero Leader
        {
            get
            {
                if (leader == null || !leader.IsAlive || leader.IsActive)
                    leader = GenerateLeader();
                return leader;
            }
        }

        public float Influence => influence;

        public abstract Hero GenerateLeader();

        public abstract void Destroy();

        public float GetFavor(Hero hero)
        {
            if (favors.ContainsKey(hero))
                return favors[hero];
            return 0f;
        }

        public void AddFavor(Hero hero, float favor)
        {
            if (favors.ContainsKey(hero))
                favors[hero] += favor;
            else favors.Add(hero, favor);
        }

    }
}
