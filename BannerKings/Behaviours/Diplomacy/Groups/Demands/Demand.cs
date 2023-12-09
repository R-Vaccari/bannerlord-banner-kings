using BannerKings.Managers.Skills;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
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

        public DemandResponse DisputeLordship => new DemandResponse(new TextObject("{=!}Dispute (Lordship)"),
                   new TextObject("{=!}Dispute on legal terms the efficacy of this demand. {LEADER} may or not be compelled to conceded, based on their Lordship ({LEADER_LORDSHIP}) against yours ({PLAYER_LORDSHIP}). Whether the group is satisfied or not depends on the leader being convinced.\nMinimum Lordship: {LORDSHIP}")
                   .SetTextVariable("LEADER", Group.Leader.Name)
                   .SetTextVariable("LEADER_LORDSHIP", Group.Leader.GetSkillValue(BKSkills.Instance.Lordship))
                   .SetTextVariable("PLAYER_LORDSHIP", Hero.MainHero.GetSkillValue(BKSkills.Instance.Lordship))
                   .SetTextVariable("LORDSHIP", 100),
                   new TextObject("{=!}On {DATE}, the {GROUP} were defeated on legal grounds over the {DEMAND} demand.")
                   .SetTextVariable("GROUP", Group.Name)
                   .SetTextVariable("DEMAND", Name),
                   -5,
                   1000,
                   100,
                   (Hero fulfiller) =>
                   {
                       return fulfiller.GetSkillValue(BKSkills.Instance.Lordship) > 100;
                   },
                   (Hero fulfiller) =>
                   {
                       return 1f;
                   },
                   (Hero fulfiller) =>
                   {
                       float factor = fulfiller.GetSkillValue(BKSkills.Instance.Lordship) /
                       Group.Leader.GetSkillValue(BKSkills.Instance.Lordship);

                       if (MBRandom.RandomFloat <= factor * 0.5f)
                       {
                           if (fulfiller == Hero.MainHero)
                           {
                               InformationManager.DisplayMessage(new InformationMessage(
                                   new TextObject("{=!}{LEADER} was appeased! The {GROUP} back down on their demand.")
                                   .SetTextVariable("GROUP", Group.Name)
                                   .SetTextVariable("LEADER", Group.Leader.Name)
                                   .ToString(),
                                   Color.FromUint(Utils.TextHelper.COLOR_LIGHT_BLUE)));
                           }

                           return true;
                       }
                       else
                       {
                           if (fulfiller == Hero.MainHero)
                           {
                               InformationManager.DisplayMessage(new InformationMessage(
                                   new TextObject("{=!}{LEADER} was appeased! The {GROUP} back down on their demand.")
                                   .SetTextVariable("GROUP", Group.Name)
                                   .SetTextVariable("LEADER", Group.Leader.Name)
                                   .ToString(),
                                   Color.FromUint(Utils.TextHelper.COLOR_LIGHT_BLUE)));
                           }

                           return false;
                       }
                   });

        public DemandResponse AppeaseCharm => new DemandResponse(new TextObject("{=!}Appease (Charm)"),
                   new TextObject("{=!}Use your diplomatic and charm skills to convince {LEADER} out of this idea. This option may or may not work. The chance of working depends on your Charm and how much {LEADER} likes you - its unlikely you will charm an enemy. Whether the group is satisfied or not depends on the leader being convinced.\nMinimum Charm: {CHARM}")
                   .SetTextVariable("LEADER", Group.Leader.Name)
                   .SetTextVariable("CHARM", 150),
                   new TextObject("{=!}On {DATE}, the {GROUP} were appeased to forfeit their {DEMAND} demand.")
                   .SetTextVariable("GROUP", Group.Name)
                   .SetTextVariable("DEMAND", Name),
                   0,
                   500,
                   100,
                   (Hero fulfiller) =>
                   {
                       return fulfiller.GetSkillValue(DefaultSkills.Charm) > 150;
                   },
                   (Hero fulfiller) =>
                   {
                       return 1f;
                   },
                   (Hero fulfiller) =>
                   {
                       int relation = Group.Leader.GetRelation(fulfiller);
                       int skill = (int)(fulfiller.GetSkillValue(DefaultSkills.Charm) / 3f);
                       float chance = (relation + skill) / 200f;
                       if (MBRandom.RandomFloat <= chance)
                       {
                           if (fulfiller == Hero.MainHero)
                           {
                               InformationManager.DisplayMessage(new InformationMessage(
                                   new TextObject("{=!}{LEADER} was appeased! The {GROUP} back down on their demand.")
                                   .SetTextVariable("GROUP", Group.Name)
                                   .SetTextVariable("LEADER", Group.Leader.Name)
                                   .ToString(),
                                   Color.FromUint(Utils.TextHelper.COLOR_LIGHT_BLUE)));
                           }

                           ChangeRelationAction.ApplyRelationChangeBetweenHeroes(fulfiller, Group.Leader, 6);
                           return true;
                       }
                       else
                       {
                           if (fulfiller == Hero.MainHero)
                           {
                               InformationManager.DisplayMessage(new InformationMessage(
                                   new TextObject("{=!}{LEADER} was not convinced... The {GROUP} is not satisfied with this outcome.")
                                   .SetTextVariable("GROUP", Group.Name)
                                   .SetTextVariable("LEADER", Group.Leader.Name)
                                   .ToString(),
                                   Color.FromUint(Utils.TextHelper.COLOR_LIGHT_RED)));
                           }

                           ChangeRelationAction.ApplyRelationChangeBetweenHeroes(fulfiller, Group.Leader, -9);
                           return false;
                       }
                   });

        public DemandResponse LeverageInfluence => new DemandResponse(new TextObject("{=!}Leverage Influence"),
                   new TextObject("{=!}Use your political influence to deny this demand. {LEADER} will not be pleased with this option, but the group will accept it. They will be satisfied with this outcome.")
                   .SetTextVariable("LEADER", Group.Leader.Name)
                   .SetTextVariable("INFLUENCE", MBRandom.RoundRandomized(BannerKingsConfig.Instance.InterestGroupsModel
                   .CalculateLeverageInfluenceCost(Hero.MainHero, 100, 1f).ResultNumber)),
                   new TextObject("{=!}On {DATE}, the {GROUP} were politically influenced to abandon their {DEMAND} demand.")
                   .SetTextVariable("GROUP", Group.Name)
                   .SetTextVariable("DEMAND", Name),
                   -8,
                   500,
                   200,
                   (Hero fulfiller) =>
                   {
                       return fulfiller.Clan.Influence >=
                       BannerKingsConfig.Instance.InterestGroupsModel.CalculateLeverageInfluenceCost(fulfiller, 100, 1f).ResultNumber;
                   },
                   (Hero fulfiller) =>
                   {
                       return 1f;
                   },
                   (Hero fulfiller) =>
                   {
                       if (fulfiller == Hero.MainHero)
                       {
                           InformationManager.DisplayMessage(new InformationMessage(
                               new TextObject("{=!}The {GROUP} back down on their demand! {LEADER} was politically compelled to give up.")
                               .SetTextVariable("GROUP", Group.Name)
                               .SetTextVariable("LEADER", Group.Leader.Name)
                               .ToString(),
                               Color.FromUint(Utils.TextHelper.COLOR_LIGHT_BLUE)));
                       }

                       ChangeClanInfluenceAction.Apply(fulfiller.Clan, -BannerKingsConfig.Instance.InterestGroupsModel
                           .CalculateLeverageInfluenceCost(fulfiller, 100, 1f).ResultNumber);
                       return true;
                   });

        public DemandResponse FinancialCompromise => new DemandResponse(new TextObject("{=!}Financial Compromise"),
                   new TextObject("{=!}Negotiate with {LEADER} a financial compromise to appease the group's demand. A sum of denars based on your income will be paied out to the group, mostly to the leader. They will be satisfied with this outcome.")
                   .SetTextVariable("LEADER", Group.Leader.Name)
                   .SetTextVariable("DENARS", MBRandom.RoundRandomized(BannerKingsConfig.Instance.InterestGroupsModel
                   .CalculateFinancialCompromiseCost(Hero.MainHero, 10000, 1f).ResultNumber)),
                   new TextObject("{=!}On {DATE}, a financial compromise was made with {GROUP} due to their {DEMAND} demand.")
                   .SetTextVariable("GROUP", Group.Name)
                   .SetTextVariable("DEMAND", Name),
                   5,
                   300,
                   300,
                   (Hero fulfiller) =>
                   {
                       return true;
                   },
                   (Hero fulfiller) =>
                   {
                       return 1f;
                   },
                   (Hero fulfiller) =>
                   {
                       int denars = MBRandom.RoundRandomized(BannerKingsConfig.Instance.InterestGroupsModel
                       .CalculateFinancialCompromiseCost(fulfiller, 10000, 1f).ResultNumber);
                       fulfiller.ChangeHeroGold(-denars);

                       LoseRelationsWithGroup(fulfiller, -6, 0.2f);
                       if (fulfiller == Hero.MainHero)
                       {
                           InformationManager.DisplayMessage(new InformationMessage(
                               new TextObject("{=!}The {GROUP} accepts the compromise... However, some criticize it as bribery.")
                               .SetTextVariable("GROUP", Group.Name)
                               .ToString(),
                               Color.FromUint(Utils.TextHelper.COLOR_LIGHT_YELLOW)));
                       }

                       return true;
                   });

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
