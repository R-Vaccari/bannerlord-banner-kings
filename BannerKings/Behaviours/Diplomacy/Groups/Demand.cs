using BannerKings.Managers.Skills;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Diplomacy.Groups
{
    public class Demand : BannerKingsObject
    {
        private Func<InterestGroup, bool> isAdequate;
        public Demand(string stringId) : base(stringId)
        {
        }

        public void Initialize(TextObject name, TextObject description, bool impactAllRelations,
            Func<InterestGroup, bool> isAdequate, List<DemandResponse> responses)
        {
            Initialize(name, description);
            ImpactAllRelations = impactAllRelations;
            this.isAdequate = isAdequate;
            DemandResponses = responses;
        }

        public bool ImpactAllRelations { get; private set; }
        public bool IsAdequate(InterestGroup group) => isAdequate(group);
        public List<DemandResponse> DemandResponses { get; private set; }

        public void Fulfill(DemandResponse response, InterestGroup group, Hero fulfiller)
        {
            response.Fulfill(group, fulfiller);
            if (ImpactAllRelations)
            {

            }

            fulfiller.AddSkillXp(BKSkills.Instance.Lordship, 1000);
        }

        public class DemandResponse
        {
            private Action<InterestGroup, Hero> fulfill;
            private Func<Hero, bool> isAdequate;

            public DemandResponse(TextObject name, TextObject description, int relation, Func<Hero, bool> isAdequate, Action<InterestGroup, Hero> fulfill)
            {
                this.fulfill = fulfill;
                this.isAdequate = isAdequate;
                Name = name;
                Description = description;
                Relation = relation;
            }   

            public TextObject Name { get; private set; }
            public TextObject Description { get; private set; }
            public int Relation { get; private set; }
            public bool IsAdequate(Hero fulfiller) => isAdequate(fulfiller);
            public void Fulfill(InterestGroup group, Hero fulfiller) => fulfill(group, fulfiller);
        }
    }
}
