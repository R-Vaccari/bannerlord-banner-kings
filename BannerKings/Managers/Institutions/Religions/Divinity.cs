using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Institutions.Religions
{
    public class Divinity
    {
        [SaveableField(1)] private string id;

        public Divinity(string id, TextObject name, TextObject description, TextObject effects,
            TextObject secondaryTitle = null, int blessingCost = 500)
        {
            this.id = id;
            Name = name;
            Description = description;
            Effects = effects;
            SecondaryTitle = secondaryTitle != null ? secondaryTitle : new TextObject();
            BlessingCost = blessingCost;
        }

        public int BlessingCost { get; }

        public TextObject Name { get; }

        public TextObject Description { get; }

        public TextObject Effects { get; }

        public TextObject SecondaryTitle { get; }
    }
}