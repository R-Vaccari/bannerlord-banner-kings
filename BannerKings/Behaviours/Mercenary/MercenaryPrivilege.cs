using System;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Behaviours.Mercenary
{
    internal class MercenaryPrivilege : BannerKingsObject
    {
        private Func<MercenaryCareer, bool> isAvailable;
        private Action<MercenaryCareer> onAdded;
        public MercenaryPrivilege(string stringId) : base(stringId)
        {
        }

        public void Initialize(TextObject name, TextObject description, TextObject unavailableHint,
            float points, int maxLevel, Func<MercenaryCareer, bool> isAvailable, Action<MercenaryCareer> onAdded)
        {
            Initialize(name, description);
            UnAvailableHint = unavailableHint;
            Points = points;
            MaxLevel = maxLevel;
            this.isAvailable = isAvailable;
            this.onAdded = onAdded;
        }

        [SaveableProperty(1)] public int Level { get; private set; }
        public int MaxLevel { get; private set; }

        public void IncreaseLevel()
        {
            if (Level < MaxLevel)
            {
                Level++;
            }
        }

        public float Points { get; private set; }
        public TextObject UnAvailableHint { get; private set; }

        public bool IsAvailable(MercenaryCareer career) => isAvailable(career);

        public override bool Equals(object obj)
        {
            if (obj is MercenaryPrivilege)
            {
                return StringId == (obj as MercenaryPrivilege).StringId;
            }
            return base.Equals(obj);
        }

        internal void OnPrivilegeAdded(MercenaryCareer career)
        {
            if (onAdded != null)
            {
                onAdded(career);
            }
        }
    }
}
