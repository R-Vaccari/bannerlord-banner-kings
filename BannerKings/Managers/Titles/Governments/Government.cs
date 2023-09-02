using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Titles.Governments
{
    public class Government : BannerKingsObject
    {
        private Func<Kingdom, ValueTuple<bool, TextObject>> isAdequate;

        public Government(string stringId) : base(stringId)
        {
        }

        public void Initialize(TextObject name, TextObject description, TextObject effects, float mercantilism,
        List<PolicyObject> prohibitedPolicies,
        List<Succession> successions,
        Func<Kingdom, ValueTuple<bool, TextObject>> isAdequate = null)
        {
            Initialize(name, description);
            Effects = effects;
            Mercantilism = mercantilism;
            ProhibitedPolicies = prohibitedPolicies;
            Successions = successions;
            this.isAdequate = isAdequate;
        }

        public void PostInitialize()
        {
            Government g = DefaultGovernments.Instance.GetById(this);
            Initialize(g.name, g.description, g.Effects, g.Mercantilism, g.ProhibitedPolicies, g.Successions,
                g.isAdequate);
        }

        public float Mercantilism { get; private set; }
        public List<PolicyObject> ProhibitedPolicies { get; private set; }
        public List<Succession> Successions { get; private set; }
        public TextObject Effects { get; private set; }
        public ValueTuple<bool, TextObject> IsAdequate(Kingdom kingdom) => IsAdequate(kingdom);

        public bool IsKingdomAdequate(Kingdom kingdom)
        {
            if (isAdequate != null) isAdequate(kingdom);
            return true;
        }
    }
}
