using BannerKings.Managers.Skills;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Behaviours.Diplomacy.Groups.Demands
{
    public abstract class Demand : BannerKingsObject
    {
        public Demand(string stringId) : base(stringId)
        {
        }

        public abstract void SetTexts();

        public abstract Demand GetCopy(InterestGroup group);

        [SaveableProperty(9)] public InterestGroup Group { get; set; }
        public CampaignTime DueDate { get; protected set; }

        public bool IsDueDate => Active && DueDate.GetDayOfYear == CampaignTime.Now.GetDayOfYear && DueDate.GetYear == CampaignTime.Now.GetYear;

        public abstract DemandResponse PositiveAnswer { get; }
        public abstract DemandResponse NegativeAnswer { get; }

        public abstract float MinimumGroupInfluence { get; }

        public abstract bool Active { get; }

        public abstract void SetUp();
        public abstract void ShowPlayerDemandOptions();
        public abstract void ShowPlayerPrompt();
        public abstract void ShowPlayerDemandAnswers();
        public abstract ValueTuple<bool, TextObject> IsDemandCurrentlyAdequate();
        public abstract IEnumerable<DemandResponse> DemandResponses { get; }
        public abstract void Tick();
        public abstract void Finish();

        protected void LoseRelationsWithGroup(Hero fulfiller, int maxLoss, float chance)
        {
            foreach (Hero member in Group.Members)
            {
                if (member != Group.Leader && MBRandom.RandomFloat < chance)
                {
                    int loss = MBRandom.RandomInt(maxLoss, -1);
                    ChangeRelationAction.ApplyRelationChangeBetweenHeroes(fulfiller, member, loss);
                }
            }
        }

        protected string GetHeroRoleText(Hero hero)
        {
            if (hero.Clan != null)
            {
                if (hero.Clan == Clan.PlayerClan)
                {
                    return new TextObject("{=!}, a member of your household").ToString();
                }

                return new TextObject("{=!} of the {CLAN}").SetTextVariable("CLAN", hero.Clan.Name).ToString();
            }

            if (hero.IsNotable)
            {
                return new TextObject("{=!}, a local dignatary").ToString();
            }

            return string.Empty;
        }

        public void Fulfill(DemandResponse response, Hero fulfiller)
        {
            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(fulfiller, Group.Leader, response.Relation);
            bool success = response.Fulfill(fulfiller);

            Group.Leader.AddSkillXp(BKSkills.Instance.Lordship, success ? response.LoserXp : response.SuccessXp);
            fulfiller.AddSkillXp(BKSkills.Instance.Lordship, success ? response.SuccessXp : response.LoserXp);
            Finish();
        }

        public void DoAiChoice()
        {
            List<ValueTuple<DemandResponse, float>> options = new List<(DemandResponse, float)>();
            foreach (DemandResponse response in DemandResponses)
            {
                options.Add(new (response, response.CalculateAiLikelihood(Group.FactionLeader)));
            }

            DemandResponse result = MBRandom.ChooseWeighted(options);
            Fulfill(result, Group.FactionLeader);
        }

        public override bool Equals(object obj)
        {
            if (obj is Demand)
            {
                Demand d = obj as Demand;
                return d.StringId == StringId && d.Group == Group;
            }
            return base.Equals(obj);
        }

        public class DemandResponse
        {
            private Func<Hero, bool> fulfill;
            private Func<Hero, bool> isAdequate;
            private Func<Hero, float> calculateAiLikelihood;

            public DemandResponse(TextObject name, TextObject description, TextObject explanation, int relation, int fulfillerXp,
                int groupLeaderXp, Func<Hero, bool> isAdequate, Func<Hero, float> calculateAiLikelihood, 
                Func<Hero, bool> fulfill)
            {
                this.fulfill = fulfill;
                this.isAdequate = isAdequate;
                this.calculateAiLikelihood = calculateAiLikelihood;
                Name = name;
                LoserXp = groupLeaderXp;
                SuccessXp = fulfillerXp;
                Description = description;
                Relation = relation;
                Explanation = explanation;
            }

            public TextObject Name { get; private set; }
            public TextObject Description { get; private set; }
            public TextObject Explanation { get; private set; }
            public int Relation { get; private set; }
            public int SuccessXp { get; private set; }
            public int LoserXp { get; private set; }
            public float CalculateAiLikelihood(Hero fulfiller) => calculateAiLikelihood(fulfiller);
            public bool IsAdequate(Hero fulfiller) => isAdequate(fulfiller);
            public bool Fulfill(Hero fulfiller) => fulfill(fulfiller);
        }
    }
}
