using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TaleWorlds.CampaignSystem;
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

        public void PostInitialize()
        {
            Era e = DefaultEras.Instance.GetById(this);
            Initialize(e.Name, e.Description, e.PreviousEra);

            if (Advancements == null) Advancements = new List<BKTroopAdvancement>(50);
        }

        public Era PreviousEra { get; private set; }
        private List<BKTroopAdvancement> Advancements { get; set; }

        public void AddTroopAdvancement(BKTroopAdvancement advancement) => Advancements.Add(advancement);

        public void TriggerEra(CultureObject culture)
        {
            foreach (var advancement in Advancements)
            {
                if (advancement.Culture == culture)
                {
                    advancement.SetEquipment();
                }
            }
        }

        public BKTroopAdvancement GetTroopAdvancement(CharacterObject character) => Advancements.FirstOrDefault(x => x.Character == character);

        public override bool Equals(object obj)
        {
            if (obj is Era)
            {
                return (obj as Era).StringId == StringId;
            }
            return base.Equals(obj);
        }
    }
}
