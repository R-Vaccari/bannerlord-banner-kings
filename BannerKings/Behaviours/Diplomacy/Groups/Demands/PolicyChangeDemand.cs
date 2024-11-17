using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Behaviours.Diplomacy.Groups.Demands
{
    public class PolicyChangeDemand : Demand
    {
        [SaveableProperty(10)] private PolicyObject Policy { get; set; }
        [SaveableProperty(11)] private bool Enact { get; set; }

        public override bool Equals(object obj)
        {
            bool same = false;
            if (obj is PolicyChangeDemand) same = (obj as PolicyChangeDemand).Policy == Policy &&
                    (obj as PolicyChangeDemand).Enact == Enact;

            return base.Equals(obj) && same;
        }

        public PolicyChangeDemand() : base("PolicyChange")
        {
            SetTexts();
        }

        public override void SetTexts()
        {
            Initialize(new TextObject("{=swnsBayb}Policy Change"),
                new TextObject());
        }

        public override DemandResponse PositiveAnswer => new DemandResponse(new TextObject("{=kyB8tkgY}Concede"),
                    new TextObject("{=7eV3kaNh}Accept the demand to change the current state of the {POLICY} policy. They will be satisfied with this outcome.")
                    .SetTextVariable("POLICY", Policy.Name),
                    new TextObject("{=Pr6r49e8}On {DATE}, the {GROUP} were conceded their {DEMAND} demand.")
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
                                new TextObject("{=rMFDh1CE}The {GROUP} is satisfied! {POLICY} is now {ENACTED} in the realm.")
                                .SetTextVariable("GROUP", Group.Name)
                                .SetTextVariable("POLICY", Policy.Name)
                                .SetTextVariable("ENACTED", Enact ? new TextObject("{=Rsomo9ZV}enacted") : new TextObject("{=hfqmT9UH}repealed"))
                                .ToString(),
                                Color.FromUint(Utils.TextHelper.COLOR_LIGHT_BLUE)));
                        }

                        Kingdom kingdom = Group.FactionLeader.MapFaction as Kingdom;
                        if (Enact)
                        {
                            kingdom.AddPolicy(Policy);
                        }
                        else
                        {
                            kingdom.RemovePolicy(Policy);
                        }

                        return true;
                    });
        public override DemandResponse NegativeAnswer => new DemandResponse(new TextObject("{=PoAmUqGR}Reject"),
                   new TextObject("{=RYmV2PEY}Deny the demand to change the state of the {POLICY} policy. They will not like this outcome.")
                   .SetTextVariable("POLICY", Policy.Name),
                   new TextObject("{=icR6DbJR}On {DATE}, the {GROUP} were rejected their {DEMAND} demand.")
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
                               new TextObject("{=Wi3oUWpJ}The {GROUP} is not satisfied...")
                               .SetTextVariable("GROUP", Group.Name)
                               .SetTextVariable("LEADER", Group.Leader.Name)
                               .ToString(),
                               Color.FromUint(Utils.TextHelper.COLOR_LIGHT_RED)));
                       }
                       return false;
                   });

        public override bool Active => Policy != null;

        public override float MinimumGroupInfluence => 0.2f;

        protected override TextObject PlayerPromptText
        {
            get
            {
                TextObject enactText;
                if (Enact) enactText = new TextObject("{=tA5qf6B1}As a policy supported by the group, they demand it to be enacted.");
                else enactText = new TextObject("{=thaUwMx7}As a policy shunned by the group, they demand it to be repealed.");

                return new TextObject("{=EAWEvkPr}The {GROUP} group is demanding the chane of state to the {POLICY} policy. {ENACT_TEXT} You may choose to resolve it now or postpone the decision. If so, the group will demand a definitive answer 7 days from now.")
                .SetTextVariable("GROUP", Group.Name)
                .SetTextVariable("POLICY", Policy.Name)
                .SetTextVariable("ENACT_TEXT", enactText);
            }
        }

        protected override TextObject PlayerAnswersText
        {
            get
            {
                TextObject enactText;
                if (Enact) enactText = new TextObject("{=tA5qf6B1}As a policy supported by the group, they demand it to be enacted.");
                else enactText = new TextObject("{=thaUwMx7}As a policy shunned by the group, they demand it to be repealed.");

                return new TextObject("{=17xuAnQx}The {GROUP} is pushing for the state of {POLICY} to be changed. {POLICY_TEXT} The group is currently lead by {LEADER}{LEADER_ROLE}. The group currently has {INFLUENCE}% influence in the realm and {SUPPORT}% support towards you.")
                .SetTextVariable("SUPPORT", (BannerKingsConfig.Instance.InterestGroupsModel.CalculateGroupSupport((Group as InterestGroup)).ResultNumber * 100f).ToString("0.00"))
                .SetTextVariable("INFLUENCE", (BannerKingsConfig.Instance.InterestGroupsModel.CalculateGroupInfluence((Group as InterestGroup)).ResultNumber * 100f).ToString("0.00"))
                .SetTextVariable("LEADER_ROLE", GetHeroRoleText(Group.Leader))
                .SetTextVariable("LEADER", Group.Leader.Name)
                .SetTextVariable("POLICY", Policy.Name)
                .SetTextVariable("POLICY_TEXT", enactText)
                .SetTextVariable("GROUP", Group.Name);
            }
        }

        public override IEnumerable<DemandResponse> DemandResponses
        {
            get
            {
                yield return PositiveAnswer;
                yield return NegativeAnswer;
                yield return FinancialCompromise;
                yield return LeverageInfluence;
                yield return AppeaseCharm;
                yield return DisputeLordship;
            }
        }

        public override Demand GetCopy(DiplomacyGroup group)
        {
            PolicyChangeDemand demand = new PolicyChangeDemand();
            demand.Group = group;
            return demand;
        }

        protected override void SetUpInternally()
        {
            Kingdom kingdom = Group.FactionLeader.MapFaction as Kingdom;
            Policy = (Group as InterestGroup).SupportedPolicies.GetRandomElementWithPredicate(x => !kingdom.ActivePolicies.Contains(x));
            if (Policy == null)
            {
                Policy = (Group as InterestGroup).ShunnedPolicies.GetRandomElementWithPredicate(x => kingdom.ActivePolicies.Contains(x));
            }

            if (Policy != null)
            {
                Enact = (Group as InterestGroup).SupportedPolicies.Contains(Policy);
            }
        }

        public override (bool, TextObject) IsDemandCurrentlyAdequate()
        {
            Kingdom kingdom = Group.FactionLeader.MapFaction as Kingdom;
            PolicyObject policy = (Group as InterestGroup).SupportedPolicies.FirstOrDefault(x => !kingdom.ActivePolicies.Contains(x));
            if (policy == null)
            {
                policy = (Group as InterestGroup).ShunnedPolicies.FirstOrDefault(x => kingdom.ActivePolicies.Contains(x));
            }

            if (policy == null)
            {
                return new(false, new TextObject("{=dYhoJxo6}There are currently no supported policies that aren't already enacted, or shunned policies that are not enacted."));
            }

            if (Active)
            {
                return new(false, new TextObject("{=RnN79qMx}This demand is already under revision by the ruler."));
            }

            return new(true, new TextObject("{=WvxUuqmj}This demand is possible."));
        }
      
        public override void ShowPlayerDemandOptions()
        {
            SetTexts();
            Kingdom kingdom = Group.FactionLeader.MapFaction as Kingdom;
            List<InquiryElement> policies = new List<InquiryElement>();
            foreach (var policy in (Group as InterestGroup).SupportedPolicies)
            {
                if (!kingdom.ActivePolicies.Contains(policy))
                {
                    policies.Add(new InquiryElement(policy,
                        new TextObject("{=um3nEuzk}Enact {POLICY}")
                        .SetTextVariable("POLICY", policy.Name)
                        .ToString(),
                        null,
                        true,
                        policy.Description.ToString()));
                }
            }

            foreach (var policy in (Group as InterestGroup).ShunnedPolicies)
            {
                if (kingdom.ActivePolicies.Contains(policy))
                {
                    policies.Add(new InquiryElement(policy,
                        new TextObject("{=ey2viAVF}Repeal {POLICY}")
                        .SetTextVariable("POLICY", policy.Name)
                        .ToString(),
                        null,
                        true,
                        policy.Description.ToString()));
                }
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(Name.ToString() + " (1/2)",
                new TextObject("{=A3UyJ3SH}As a leader of your group you can decide what policy ought to be changed. These can be policies supported by the group that are currently inactive, or active policies that are shunned by the group.").ToString(),
                policies,
                true,
                1,
                1,
                GameTexts.FindText("str_accept").ToString(),
                String.Empty,
                (List<InquiryElement> list) =>
                {
                    PolicyObject policy = (PolicyObject)list[0].Identifier;
                    this.Policy = policy;
                },
                null), 
                true);
        }
       
        public override void Finish()
        {
            Policy = null;
            DueDate = CampaignTime.Never;
        }

        protected override bool IsFulfilled() => (Enact && Group.KingdomDiplomacy.Kingdom.ActivePolicies.Contains(Policy)) ||
            (!Enact && !Group.KingdomDiplomacy.Kingdom.ActivePolicies.Contains(Policy));
    }
}
