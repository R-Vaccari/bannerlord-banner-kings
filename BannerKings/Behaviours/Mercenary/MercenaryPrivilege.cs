using System;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Mercenary
{
    internal class MercenaryPrivilege : BannerKingsObject
    {
        private Func<MercenaryCareer, bool> isAvailable;
        public MercenaryPrivilege(string stringId) : base(stringId)
        {
            Points = 0f;
        }

        public void Initialize(TextObject name, TextObject description, 
            TextObject availableHint, TextObject unavailableHint, Func<MercenaryCareer, bool> isAvailable)
        {
            Initialize(name, description);
            AvailableHint = availableHint;
            UnAvailableHint = unavailableHint;
            this.isAvailable = isAvailable;
        }

        public float Points { get; private set; }
        public TextObject AvailableHint { get; private set; }
        public TextObject UnAvailableHint { get; private set; }


        public bool IsAvailable(MercenaryCareer career) => isAvailable(career);

    }
}
