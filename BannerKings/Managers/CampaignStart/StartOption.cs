using System;
using BannerKings.Managers.Education.Lifestyles;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers.CampaignStart
{
    public class StartOption : BannerKingsObject
    {
        public StartOption(string id) : base(id)
        {
        }

        public TextObject ShortDescription { get; private set; }

        public int Gold { get; private set; }

        public int Food { get; private set; }

        public int Troops { get; private set; }

        public int Morale { get; private set; }

        public float Influence { get; private set; }

        public bool IsCriminal => Criminal > 0f;
        public float Criminal { get; private set; }

        public CultureObject Culture { get; private set; }

        public Lifestyle Lifestyle { get; private set; }

        public Action Action { get; private set; }

        public override bool Equals(object obj)
        {
            if (obj is StartOption option)
            {
                return StringId == option.StringId;
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public void Initialize(TextObject name, TextObject description, TextObject shortDescription, int gold, int food, int troops, int morale, float influence, Action action, float criminal = 0f, CultureObject culture = null, Lifestyle lifestyle = null)
        {
            Initialize(name, description);
            ShortDescription = shortDescription;
            Gold = gold;
            Food = food;
            Troops = troops;
            Morale = morale;
            Influence = influence;
            Action = action;
            Criminal = criminal;
            Culture = culture;
            Lifestyle = lifestyle;
        }

        public void PostInitialize()
        {
            var so = DefaultStartOptions.Instance.GetById(StringId);
            name = so.name;
            description = so.description;
            ShortDescription = so.ShortDescription;
            Gold = so.Gold;
            Food = so.Food;
            Troops = so.Troops;
            Morale = so.Morale;
            Influence = so.Influence;
            Criminal = so.Criminal;
            Culture = so.Culture;
            Lifestyle = so.Lifestyle;
        }
    }
}