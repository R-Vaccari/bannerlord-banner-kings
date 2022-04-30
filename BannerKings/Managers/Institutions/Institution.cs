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
            favors = new Dictionary<Hero, float>();
        }

        public float Influence => influence;

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
