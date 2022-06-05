
namespace BannerKings.Managers.Institutions
{
    public abstract class Institution
    {
        protected float influence;

        protected Institution()
        {
        }

        public float Influence => influence;

        public abstract void Destroy();
    }
}
