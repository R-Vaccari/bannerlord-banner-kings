using BannerKings.Managers.Skills;
using BannerKings.Managers.Titles;
using BannerKings.Managers.Titles.Laws;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Behaviours.Diplomacy.Groups.Demands
{
    public class DemesneLawChangeDemand : Demand
    {
        [SaveableProperty(10)] private DemesneLaw Law { get; set; }
        [SaveableProperty(11)] private FeudalTitle Title { get; set; }

        public DemesneLawChangeDemand() : base("DemesneLawChange")
        {
            SetTexts();
        }

        public override void SetTexts()
        {
            Initialize(new TextObject("{=!}Policy Change"),
                new TextObject());
        }

        public override DemandResponse PositiveAnswer => new DemandResponse(new TextObject("{=!}Concede"),
                    new TextObject("{=!}Accept the demand to pass the {LAW} law. They will be satisfied with this outcome.")
                    .SetTextVariable("LAW", Law.Name),
                    new TextObject("{=!}On {DATE}, the {GROUP} were conceded their {DEMAND} demand.")
                    .SetTextVariable("GROUP", Group.Name)
                    .SetTextVariable("DEMAND", Name),
                    6,
                    250,
                    1000,
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
                        if (fulfiller == Hero.MainHero)
                        {
                            InformationManager.DisplayMessage(new InformationMessage(
                                new TextObject("{=!}The {GROUP} is satisfied! {LAW} is now enacted in the realm.")
                                .SetTextVariable("GROUP", Group.Name)
                                .SetTextVariable("LAW", Law.Name)
                                .ToString(),
                                Color.FromUint(Utils.TextHelper.COLOR_LIGHT_BLUE)));
                        }

                        Title.EnactLaw(Law, fulfiller, false);

                        return true;
                    });
        public override DemandResponse NegativeAnswer => new DemandResponse(new TextObject("{=!}Reject"),
                   new TextObject("{=!}Deny the demand to pass the {LAW} law. They will not like this outcome.")
                   .SetTextVariable("LAW", Law.Name),
                   new TextObject("{=!}On {DATE}, the {GROUP} were rejected their {DEMAND} demand.")
                   .SetTextVariable("GROUP", Group.Name)
                   .SetTextVariable("DEMAND", Name),
                   6,
                   250,
                   1000,
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
                       LoseRelationsWithGroup(fulfiller, -10, 0.5f);
                       if (fulfiller == Hero.MainHero)
                       {
                           InformationManager.DisplayMessage(new InformationMessage(
                               new TextObject("{=!}The {GROUP} is not satisfied...")
                               .SetTextVariable("GROUP", Group.Name)
                               .SetTextVariable("LEADER", Group.Leader.Name)
                               .ToString(),
                               Color.FromUint(Utils.TextHelper.COLOR_LIGHT_RED)));
                       }
                       return false;
                   });

        public override bool Active => Law != null;

        public override float MinimumGroupInfluence => 0.2f;

        public override IEnumerable<DemandResponse> DemandResponses
        {
            get
            {
                yield return PositiveAnswer;
                yield return NegativeAnswer;
                yield return new DemandResponse(new TextObject("{=!}Financial Compromise"),
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

                yield return new DemandResponse(new TextObject("{=!}Leverage Influence"),
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

                yield return new DemandResponse(new TextObject("{=!}Appease (Charm)"),
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

                yield return new DemandResponse(new TextObject("{=!}Dispute (Lordship)"),
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
            }
        }

        public override Demand GetCopy(InterestGroup group)
        {
            DemesneLawChangeDemand demand = new DemesneLawChangeDemand();
            demand.Group = group;
            return demand;
        }

        public override void SetUp()
        {
            Kingdom kingdom = Group.FactionLeader.MapFaction as Kingdom;
            Law = Group.SupportedLaws.GetRandomElementWithPredicate(x => !Title.Contract.DemesneLaws.Contains(x));
            if (Law == null)
            {
                var options = new List<DemesneLaw>();
                foreach (var law in DefaultDemesneLaws.Instance.All)
                {
                    if (!Group.ShunnedLaws.Contains(law) && !Title.Contract.DemesneLaws.Contains(law))
                    {
                        options.Add(law);
                    }
                }
                Law = options.GetRandomElement();
            }

            if (Law != null)
            {
                if (Group.Members.Contains(Hero.MainHero))
                {
                    ShowPlayerDemandOptions();
                }

                if (Group.FactionLeader == Hero.MainHero)
                {
                    ShowPlayerPrompt();
                }
            }
            else
            {
                Finish();
            }
        }

        public override (bool, TextObject) IsDemandCurrentlyAdequate()
        {
            Kingdom kingdom = Group.FactionLeader.MapFaction as Kingdom;
            PolicyObject policy = Group.SupportedPolicies.FirstOrDefault(x => !kingdom.ActivePolicies.Contains(x));
            if (policy == null)
            {
                policy = Group.ShunnedPolicies.FirstOrDefault(x => kingdom.ActivePolicies.Contains(x));
            }

            if (policy == null)
            {
                return new(false, new TextObject("{=!}There are currently no supported policies that aren't already enacted, or shunned policies that are not enacted."));
            }

            if (Active)
            {
                return new(false, new TextObject("{=!}This demand is already under revision by the ruler."));
            }

            return new(true, new TextObject("{=!}This demand is possible."));
        }

        public override void ShowPlayerPrompt()
        {
            TextObject enactText;
            if (Enact)
            {
                enactText = new TextObject("{=!}As a policy supported by the group, they demand it to be enacted.");
            }
            else
            {
                enactText = new TextObject("{=!}As a policy shunned by the group, they demand it to be repealed.");
            }

            InformationManager.ShowInquiry(new InquiryData(Name.ToString(),
                new TextObject("{=!}The {GROUP} group is demanding the chane of state to the {LAW} policy. {ENACT_TEXT} You may choose to resolve it now or postpone the decision. If so, the group will demand a definitive answer 7 days from now.")
                .SetTextVariable("GROUP", Group.Name)
                .SetTextVariable("LAW", Law.Name)
                .SetTextVariable("ENACT_TEXT", enactText)
                .ToString(),
                true,
                true,
                new TextObject("{=!}Resolve").ToString(),
                new TextObject("{=!}Postpone").ToString(),
                () =>
                {
                    ShowPlayerDemandAnswers();
                },
                () =>
                {
                    DueDate = CampaignTime.DaysFromNow(7f);
                },
                Utils.Helpers.GetKingdomDecisionSound()),
                true,
                true);
        }

        public override void ShowPlayerDemandAnswers()
        {
            List<InquiryElement> options = new List<InquiryElement>();
            foreach (var answer in DemandResponses)
            {
                options.Add(new InquiryElement(answer,
                    answer.Name.ToString(),
                    null,
                    answer.IsAdequate(Hero.MainHero),
                    answer.Description.ToString()));
            }

            TextObject enactText;
            if (Enact)
            {
                enactText = new TextObject("{=!}As a policy supported by the group, they demand it to be enacted.");
            }
            else
            {
                enactText = new TextObject("{=!}As a policy shunned by the group, they demand it to be repealed.");
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(Name.ToString(),
                new TextObject("{=!}The {GROUP} is pushing for the state of {LAW} to be changed. {LAW_TEXT} The group is currently lead by {LEADER}{LEADER_ROLE}. The group currently has {INFLUENCE}% influence in the realm and {SUPPORT}% support towards you.")
                .SetTextVariable("SUPPORT", (BannerKingsConfig.Instance.InterestGroupsModel.CalculateGroupSupport(Group).ResultNumber * 100f).ToString("0.00"))
                .SetTextVariable("INFLUENCE", (BannerKingsConfig.Instance.InterestGroupsModel.CalculateGroupInfluence(Group).ResultNumber * 100f).ToString("0.00"))
                .SetTextVariable("LEADER_ROLE", GetHeroRoleText(Group.Leader))
                .SetTextVariable("LEADER", Group.Leader.Name)
                .SetTextVariable("LAW", Law.Name)
                .SetTextVariable("LAW_TEXT", enactText)
                .SetTextVariable("GROUP", Group.Name)
                .ToString(),
                options,
                false,
                1,
                GameTexts.FindText("str_accept").ToString(),
                String.Empty,
                (List<InquiryElement> list) =>
                {
                    DemandResponse response = (DemandResponse)list[0].Identifier;
                    Fulfill(response, Hero.MainHero);
                },
                null,
                Utils.Helpers.GetKingdomDecisionSound()),
                true);
        }

        public override void ShowPlayerDemandOptions()
        {
            Kingdom kingdom = Group.FactionLeader.MapFaction as Kingdom;
            List<InquiryElement> policies = new List<InquiryElement>();
            foreach (var policy in Group.SupportedPolicies)
            {
                if (!kingdom.ActivePolicies.Contains(policy))
                {
                    policies.Add(new InquiryElement(policy,
                        new TextObject("{=!}Enact {LAW}")
                        .SetTextVariable("LAW", policy.Name)
                        .ToString(),
                        null,
                        true,
                        policy.Description.ToString()));
                }
            }

            foreach (var policy in Group.ShunnedPolicies)
            {
                if (kingdom.ActivePolicies.Contains(policy))
                {
                    policies.Add(new InquiryElement(policy,
                        new TextObject("{=!}Repeal {LAW}")
                        .SetTextVariable("LAW", policy.Name)
                        .ToString(),
                        null,
                        true,
                        policy.Description.ToString()));
                }
            }

            bool playerLead = Group.Leader == Hero.MainHero;
            TextObject description;

            description = new TextObject("{=!}Choose the benefactor for the {POSITION} position.");
            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(Name.ToString() + " (1/2)",
                new TextObject("{=!}As a leader of your group you can decide what council position to demand. Only positions from the Privy Council are suitable.").ToString(),
                policies,
                true,
                1,
                GameTexts.FindText("str_accept").ToString(),
                String.Empty,
                (List<InquiryElement> list) =>
                {
                    PolicyObject policy = (PolicyObject)list[0].Identifier;
                    this.Law = policy;
                },
                null), 
                true);
        }

        public override void Finish()
        {
            Law = null;
            DueDate = CampaignTime.Never;
        }

        public override void Tick()
        {
            Kingdom kingdom = Group.FactionLeader.MapFaction as Kingdom;
            if (Enact && kingdom.ActivePolicies.Contains(Law))
            {
                PositiveAnswer.Fulfill(Group.FactionLeader);
            }

            if (!Enact && !kingdom.ActivePolicies.Contains(Law))
            {
                PositiveAnswer.Fulfill(Group.FactionLeader);
            }

            if (IsDueDate)
            {
                ShowPlayerDemandAnswers();
            }
        }
    }
}
