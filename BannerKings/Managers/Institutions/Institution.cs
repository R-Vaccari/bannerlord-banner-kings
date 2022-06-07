
namespace BannerKings.Managers.Institutions
{
    public abstract class Institution
    {
        protected float influence;

        protected Institution()
        {
        }

<<<<<<< HEAD
        public Hero Leader
        {
            get
            {
                if (this.leader == null || !this.leader.IsAlive || this.leader.IsActive)
                    this.leader = GenerateLeader();
                return this.leader;
            }
        }

        public abstract void Destroy();

        public float Influence => this.influence;

        public abstract Hero GenerateLeader();

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

=======
        public float Influence => influence;

        public abstract void Destroy();
>>>>>>> main
    }
}
