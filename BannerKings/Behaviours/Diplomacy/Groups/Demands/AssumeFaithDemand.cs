using BannerKings.Managers.Institutions.Religions;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Behaviours.Diplomacy.Groups.Demands
{
    public class AssumeFaithDemand : Demand
    {
        public AssumeFaithDemand() : base("AssumeFaith")
        {
            SetTexts();
        }

        [SaveableProperty(1)] private Religion Religion { get; set; }
        private Religion RulerReligion => BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(Group.KingdomDiplomacy.Kingdom.Leader);

        public override DemandResponse PositiveAnswer => new DemandResponse(new TextObject("{=kyB8tkgY}Concede"),
                    new TextObject("{=Z9AB8vR6}Accept the demand to adhere to the {RELIGION} faith. They will be satisfied with this outcome.")
                    .SetTextVariable("RELIGION", Religion.Faith.GetName()),
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
                                new TextObject("{=iG3vZQnP}The {GROUP} is satisfied! You will now adhere to the {RELIGION} faith.")
                                .SetTextVariable("GROUP", Group.Name)
                                .SetTextVariable("RELIGION", Religion.Faith.GetFaithName())
                                .ToString(),
                                Color.FromUint(Utils.TextHelper.COLOR_LIGHT_BLUE)));
                        }

                        BannerKingsConfig.Instance.ReligionsManager.AddToReligion(fulfiller, Religion);

                        return true;
                    });

        public override DemandResponse NegativeAnswer => new DemandResponse(new TextObject("{=PoAmUqGR}Reject"),
                   new TextObject("{=RYmV2PEY}Deny the demand to assumne the {RELIGION} faith. They will not like this outcome.")
                   .SetTextVariable("RELIGION", Religion.Faith.GetName()),
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

        public override float MinimumGroupInfluence => 0.1f;

        public override bool Active => Religion != null;

        public override IEnumerable<DemandResponse> DemandResponses
        {
            get
            {
                yield return PositiveAnswer;
                yield return NegativeAnswer;
                yield return LeverageInfluence;
                yield return AppeaseCharm;
                yield return DisputeLordship;
            }
        }

        protected override TextObject PlayerPromptText => new TextObject("{=yHWrKsKr}The {GROUP} group is demanding you assume the {RELIGION} faith. You may choose to resolve it now or postpone the decision. If so, the group will demand a definitive answer 7 days from now.")
                .SetTextVariable("GROUP", Group.Name)
                .SetTextVariable("RELIGION", Religion.Faith.GetFaithName());

        protected override TextObject PlayerAnswersText => new TextObject("{=1rWYDHQj}The {GROUP} is pushing for you to assume the {RELIGION} faith, the legal faith of the realm. The group is currently lead by {LEADER}{LEADER_ROLE}. The group currently has {INFLUENCE}% influence in the realm and {SUPPORT}% support towards you.")
                .SetTextVariable("SUPPORT", (BannerKingsConfig.Instance.InterestGroupsModel.CalculateGroupSupport(Group as InterestGroup).ResultNumber * 100f).ToString("0.00"))
                .SetTextVariable("INFLUENCE", (BannerKingsConfig.Instance.InterestGroupsModel.CalculateGroupInfluence(Group as InterestGroup).ResultNumber * 100f).ToString("0.00"))
                .SetTextVariable("LEADER_ROLE", GetHeroRoleText(Group.Leader))
                .SetTextVariable("LEADER", Group.Leader.Name)
                .SetTextVariable("RELIGION", Religion.Faith.GetFaithName())
                .SetTextVariable("GROUP", Group.Name);

        public override void Finish()
        {
            Religion = null;
            DueDate = CampaignTime.Never;
        }

        public override Demand GetCopy(DiplomacyGroup group)
        {
            AssumeFaithDemand demand = new AssumeFaithDemand();
            demand.Group = group;
            return demand;
        }

        public override (bool, TextObject) IsDemandCurrentlyAdequate()
        {
            (bool, TextObject) result = new(false, null);

            Religion kingdomReligion = Group.KingdomDiplomacy.Religion;
            if (kingdomReligion != null)
            {
                Religion leaderReligion = RulerReligion;
                if (leaderReligion == null || leaderReligion.Faith != kingdomReligion.Faith)
                {
                    result = new(true, new TextObject("{=WvxUuqmj}This demand is possible."));
                }
                else result = new(false, new TextObject("{=jpYAAwvw}The ruler adheres to the legal faith of the realm."));
            }

            if (Active)
            {
                result = new(false, new TextObject("{=RnN79qMx}This demand is already under revision by the ruler."));
            }

            return result;
        }

        public override void SetTexts()
        {
            if (Religion != null) Religion.PostInitialize();
            Initialize(new TextObject("{=euPvwc76}Assume Faith"),
                new TextObject("{=RYmV2PEY}Demand that the ruler assume the legal faith of the realm."));
        }

        protected override void SetUpInternally()
        {
            Religion kingdomReligion = Group.KingdomDiplomacy.Religion;
            if (kingdomReligion != null)
            {
                Religion = kingdomReligion;
            }
        }

        public override void ShowPlayerDemandOptions()
        {
        }

        protected override bool IsFulfilled() => RulerReligion == Religion;
    }
}
