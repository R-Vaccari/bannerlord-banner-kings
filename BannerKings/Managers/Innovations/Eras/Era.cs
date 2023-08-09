using System.Collections.Generic;


namespace BannerKings.Managers.Innovations.Eras
{
    public class Era : BannerKingsObject
    {
        public Era(string stringId) : base(stringId)
        {
            Advancements = new List<BKTroopAdvancement>(50);
        }

        public Era PreviousEra { get; private set; }
        private List<BKTroopAdvancement> Advancements { get; set; }

        public void TriggerEra()
        {

        }
    }
}
