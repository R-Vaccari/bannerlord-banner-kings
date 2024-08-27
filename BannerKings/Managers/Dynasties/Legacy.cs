using TaleWorlds.Localization;

namespace BannerKings.Managers.Dynasties
{
    public class Legacy : BannerKingsObject
    {
        public Legacy(string stringId) : base(stringId)
        {
        }

        public void Initialize(TextObject name,
            TextObject description,
            TextObject effects,
            LegacyType type,
            int points,
            Legacy requirement = null)
        {
            Initialize(name, description);
            Effects = effects;
            Type = type;
            Requirement = requirement;
            Points = points;
        }

        public int Points { get; private  set; }
        public Legacy Requirement { get; private set; }
        public LegacyType Type { get; private set; }
        public TextObject Effects { get; private set; }

        public bool IsAvailable(Dynasty dynasty) 
        {
            bool available = dynasty.Renown >= Points;
            if (available && Requirement != null) available = dynasty.HasLegacy(Requirement);

            return available;
        }
    }
}
