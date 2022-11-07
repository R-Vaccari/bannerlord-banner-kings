using System;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Mercenary
{
    internal class MercenaryPrivilege : BannerKingsObject
    {
        private TextObject availableHint;
        private TextObject unavailableHint;
        private Func<bool> isAvailable;
        public MercenaryPrivilege(string stringId) : base(stringId)
        {
        }

        public void Initialize(TextObject name, TextObject description, int points, 
            TextObject availableHint, TextObject unavailableHint, Func<bool> isAvailable)
        {
            Initialize(name, description);
            this.availableHint = availableHint;
            this.unavailableHint = unavailableHint;
            Points = points;
        }

        public int Points { get; private set; }
        public TextObject Hint => IsAvailable() ? availableHint : unavailableHint;


        public bool IsAvailable() => isAvailable();

    }
}
