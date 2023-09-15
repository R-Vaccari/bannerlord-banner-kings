using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Titles.Governments
{
    public class Government : ContractAspect
    {
        private Func<Kingdom, ValueTuple<bool, TextObject>> isAdequate;

        public Government(string stringId) : base(stringId)
        {
        }

        public void Initialize(TextObject name, TextObject description, TextObject effects, float mercantilism,
            float authoritarian, float oligarchic, float egalitarian,
            List<PolicyObject> prohibitedPolicies,
            List<Succession> successions,
            Func<Kingdom, ValueTuple<bool, TextObject>> isAdequate = null)
        {
            Initialize(name, description);
            Authoritarian = authoritarian;
            Oligarchic = oligarchic;
            Egalitarian = egalitarian;
            Effects = effects;
            Mercantilism = mercantilism;
            ProhibitedPolicies = prohibitedPolicies;
            Successions = successions;
            this.isAdequate = isAdequate;
        }

        public override void PostInitialize()
        {
            Government g = DefaultGovernments.Instance.GetById(this);
            Initialize(g.name, g.description, g.Effects, g.Mercantilism,
                g.Authoritarian, g.Oligarchic, g.Egalitarian,
                g.ProhibitedPolicies, 
                g.Successions,
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

        public override bool Equals(object obj)
        {
            if (obj is Government)
            {
                return (obj as Government).StringId == StringId;
            }
            return base.Equals(obj);
        }
    }
}
