using BannerKings.Managers.Skills;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Diplomacy.Groups.Demands
{
    public abstract class Demand : BannerKingsObject
    {
        private Func<InterestGroup, bool> isAdequate;
        public Demand(string stringId) : base(stringId)
        {
        }

        public void Initialize(TextObject name, TextObject description, bool impactAllRelations,
            Func<InterestGroup, bool> isAdequate, List<DemandResponse> responses)
        {
            Initialize(name, description);
            this.isAdequate = isAdequate;
        }

        public abstract Demand GetCopy(InterestGroup group);

        public InterestGroup Group { get; protected set; }

        public abstract void ShowPlayerDemandOptions();
        public abstract void ShowPlayerDemandAnswers();
        public abstract ValueTuple<bool, TextObject> IsDemandCurrentlyAdequate();

        public abstract IEnumerable<DemandResponse> DemandResponses { get; }

        public void Fulfill(DemandResponse response, Hero fulfiller)
        {
            response.Fulfill(this, fulfiller);

            Group.Leader.AddSkillXp(BKSkills.Instance.Lordship, response.GroupLeaderXp);
            fulfiller.AddSkillXp(BKSkills.Instance.Lordship, response.FulfillerXp);
        }

        public class DemandResponse
        {
            private Action<Hero> fulfill;
            private Func<Hero, bool> isAdequate;
            private Func<Hero, float> calculateAiLikelihood;

            public DemandResponse(TextObject name, TextObject description, int relation, int fulfillerXp,
                int groupLeaderXp, Func<Hero, bool> isAdequate, Func<Hero, float> calculateAiLikelihood, Action<Hero> fulfill)
            {
                this.fulfill = fulfill;
                this.isAdequate = isAdequate;
                this.calculateAiLikelihood = calculateAiLikelihood;
                Name = name;
                GroupLeaderXp = groupLeaderXp;
                FulfillerXp = fulfillerXp;
                Description = description;
                Relation = relation;
            }

            public TextObject Name { get; private set; }
            public TextObject Description { get; private set; }
            public int Relation { get; private set; }
            public int FulfillerXp { get; private set; }
            public int GroupLeaderXp { get; private set; }
            public float CalculateAiLikelihood(Hero fulfiller) => calculateAiLikelihood(fulfiller);
            public bool IsAdequate(Hero fulfiller) => isAdequate(fulfiller);
            public void Fulfill(Demand demand, Hero fulfiller) => fulfill(demand, fulfiller);
        }
    }
}
