using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Populations
{
    public class PopulationClass
    {
        public PopulationClass(PopulationManager.PopType type, int count)
        {
            this.type = type;
            this.count = count;
        }

        [SaveableProperty(1)] public PopulationManager.PopType type { get; set; }

        [SaveableProperty(2)] public int count { get; set; }
    }
}