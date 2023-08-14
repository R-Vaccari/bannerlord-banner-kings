using System.Collections.Generic;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Innovations.Eras
{
    public class Era : BannerKingsObject
    {
        public Era(string stringId) : base(stringId)
        {
            Advancements = new List<BKTroopAdvancement>(50);
        }

        public void Initialize(TextObject name, TextObject description, Era previousEra)
        {
            Initialize(name, description);
            PreviousEra = previousEra;
        }

        public Era PreviousEra { get; private set; }
        private List<BKTroopAdvancement> Advancements { get; set; }

        public void TriggerEra()
        {

        }
    }
}
