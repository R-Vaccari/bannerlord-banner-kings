using BannerKings.Behaviours.Diplomacy;
using BannerKings.Behaviours.Diplomacy.Groups;
using BannerKings.Behaviours.Diplomacy.Groups.Demands;
using BannerKings.Utils.Models;
using Bannerlord.UIExtenderEx.Attributes;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Generic;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.VanillaTabs.Kingdoms.Groups
{
    public class InterestGroupVM : GroupItemVM
    {
        private MBBindingList<StringPairItemVM> secondaryHeaders, tertiaryHeaders;
        private bool isDemandEnabled;
        private string demandName;
        private HintViewModel demandHint;

        public InterestGroup InterestGroup => (InterestGroup)Group;

        public InterestGroupVM(InterestGroup interestGroup, KingdomGroupsVM groupsVM) : base(interestGroup, groupsVM)
        {
            Members = new MBBindingList<GroupMemberVM>();
            Headers = new MBBindingList<StringPairItemVM>();
            SecondaryHeaders = new MBBindingList<StringPairItemVM>();
            TertiaryHeaders = new MBBindingList<StringPairItemVM>();
        }

        [DataSourceProperty] public string LeaderText => new TextObject("{=SrfYbg3x}Leader").ToString();
        [DataSourceProperty] public string GroupName => InterestGroup.Name.ToString();
        [DataSourceProperty] public string GroupText => InterestGroup.Description.ToString();
        [DataSourceProperty] public HintViewModel Hint => new HintViewModel(InterestGroup.Description);

        public override void RefreshValues()
        {
            base.RefreshValues();
            Members.Clear();
            Headers.Clear();
            SecondaryHeaders.Clear();
            TertiaryHeaders.Clear();
            IsEmpty = InterestGroup.Members.Count == 0;


            foreach (var member in InterestGroup.GetSortedMembers(KingdomDiplomacy).Take(5))
            {
                if (member != Leader.Hero)
                {
                    Members.Add(new GroupMemberVM(member, true));
                }
            }

            BKExplainedNumber influence = InterestGroup.InfluenceExplained;

            Headers.Add(new StringPairItemVM(new TextObject("{=EkFaisgP}Influence").ToString(),
                FormatValue(influence.ResultNumber),
                new BasicTooltipViewModel(() => influence.GetFormattedPercentage())));

            BKExplainedNumber support = InterestGroup.SupportExplained;

            Headers.Add(new StringPairItemVM(new TextObject("{=b0smO4NW}Support").ToString(),
                FormatValue(support.ResultNumber),
                new BasicTooltipViewModel(() => support.GetFormattedPercentage())));

            Headers.Add(new StringPairItemVM(new TextObject("{=eZEhpmxY}Members").ToString(),
                InterestGroup.Members.Count.ToString(),
                new BasicTooltipViewModel(() => new TextObject("{=Bm7Nc90U}The amount of members in this group.").ToString())));

            SecondaryHeaders.Add(new StringPairItemVM(new TextObject("{=OA58FJuM}Endorsed Trait").ToString(),
                InterestGroup.MainTrait.Name.ToString(),
                new BasicTooltipViewModel(() => new TextObject("{=znxbBpaL}This group favors those with this personality trait. Hero with this trait are more likely to join the group, and the group supports more a sovereign with this trait.").ToString())));

            SecondaryHeaders.Add(new StringPairItemVM(new TextObject("{=r8QiF5TC}Allows Nobility").ToString(),
               GameTexts.FindText(InterestGroup.AllowsNobles ? "str_yes" : "str_no").ToString(),
               new BasicTooltipViewModel(() => new TextObject("{=1Fb3U8yb}Whether or not lords are allowed to participate in this group.").ToString())));

            SecondaryHeaders.Add(new StringPairItemVM(new TextObject("{=9XYN8ORW}Allows Commoners").ToString(),
               GameTexts.FindText(InterestGroup.AllowsCommoners ? "str_yes" : "str_no").ToString(),
               new BasicTooltipViewModel(() => new TextObject("{=p3tBzVXX}Whether or not relevant commoners (notables) are allowed to participate in this group.").ToString())));

            int lords = InterestGroup.Members.FindAll(x => x.IsLord).Count;
            int notables = InterestGroup.Members.FindAll(x => x.IsNotable).Count;

            TertiaryHeaders.Add(new StringPairItemVM(new TextObject("{=LwfduROT}Endorsed Acts").ToString(),
                string.Empty,
                new BasicTooltipViewModel(() => UIHelper.GetGroupEndorsed(InterestGroup))));

            TertiaryHeaders.Add(new StringPairItemVM(new TextObject("{=F5nvf0YA}Demands").ToString(),
                string.Empty,
                new BasicTooltipViewModel(() => UIHelper.GetGroupDemands(InterestGroup))));

            TertiaryHeaders.Add(new StringPairItemVM(new TextObject("{=r3H9r011}Shunned Acts").ToString(),
                string.Empty,
                new BasicTooltipViewModel(() => UIHelper.GetGroupShunned(InterestGroup))));

            DemandName = new TextObject("{=zVboRONd}Push Demand").ToString();
            IsDemandEnabled = InterestGroup.Leader == Hero.MainHero;
            DemandHint = new HintViewModel(new TextObject("{=r43NMSOf}Group leaders are able to push demands to their suzerain. You are not part of this group, and therefore have no say in its matters."));

            if (InterestGroup.Members.Contains(Hero.MainHero))
            {
                ActionName = new TextObject("{=3sRdGQou}Leave").ToString();
                IsActionEnabled = true;
                ActionHint = new HintViewModel(new TextObject("{=tEticUst}Leave this group. This will break any ties to their interests and demands. Leaving a group will hurt your relations with it's members, mainly the group leader. If you are the leader yourself, this impact will be increased."));

                if (!IsDemandEnabled)
                {
                    DemandHint = new HintViewModel(new TextObject("{=KEgFGRpy}As a member of this group you are not able to push demands. You may vote on the demand specifications once the group leader pushes them. To become leader, be the most influential member of the group.")
                        .SetTextVariable("SUZERAIN", InterestGroup.FactionLeader.Name));
                }
                else
                {
                    DemandHint = new HintViewModel(new TextObject("{=C2Ne3tF7}Deliver a demand to {SUZERAIN}. As the leader of this group, you are able to dictate what to demand from your ruler. Once a demand is made you can not disclaim the consequences, be them positive or otherwise.")
                        .SetTextVariable("SUZERAIN", InterestGroup.FactionLeader.Name));
                }

                if (!InterestGroup.CanHeroLeave(Hero.MainHero, KingdomDiplomacy))
                {
                    IsActionEnabled = false;
                    ActionHint = new HintViewModel(new TextObject("{=jBxzXBGZ}You cannot leave this group until a year has passed since you joined ({DATE}).")
                        .SetTextVariable("DATE", InterestGroup.JoinTime[Hero.MainHero].ToString()));
                }
            }
            else
            {
                ActionName = new TextObject("{=es0Y3Bxc}Join").ToString();
                IsActionEnabled = InterestGroup.CanHeroJoin(Hero.MainHero, KingdomDiplomacy);
                ActionHint = new HintViewModel(new TextObject("{=pmRmHSYe}Join this group. Being a group member means you will be aligned with their interests and demands. The group leader will be responsible for the group's interaction with the realm's sovereign, and their actions will impact the entire group. For example, a malcontent group leader may make pressure for a member of the group to be awarded a title or property and thus increase the group's influence."));

                if (InterestGroup.FactionLeader == Hero.MainHero)
                {
                    var currentDemand = InterestGroup.CurrentDemand;
                    DemandName = new TextObject("{=nteMrGXZ}Resolve Demand").ToString();
                    IsDemandEnabled = currentDemand != null;
                    TextObject demandText = new TextObject("{=PFLUEun9}The {GROUP} is currently not pushing for any demands.")
                        .SetTextVariable("GROUP", InterestGroup.Name);
                    if (IsDemandEnabled)
                    {
                        demandText = new TextObject("{=9w9bh2WH}The {GROUP} is currently pushing for the {DEMAND} demand.")
                        .SetTextVariable("GROUP", InterestGroup.Name)
                        .SetTextVariable("DEMAND", currentDemand.Name);
                    }
                    DemandHint = new HintViewModel(new TextObject("{=7GbeADru}As the sovereign of your realm, you are responsible for resolving demands of groups. {DEMAND_TEXT}")
                        .SetTextVariable("SUZERAIN", InterestGroup.FactionLeader.Name)
                        .SetTextVariable("DEMAND_TEXT", demandText));
                }
            }
        }

        [DataSourceMethod]
        private void ExecuteAction()
        {
            if (InterestGroup.Members.Contains(Hero.MainHero))
            {
                InformationManager.ShowInquiry(new InquiryData(
                    new TextObject("{=ds1KP4Qc}Leave Group").ToString(),
                    new TextObject("{=pXkS1xV5}Leaving the group will harm the members' opinion of you, specially if you lead the group.").ToString(),
                    true,
                    true,
                    GameTexts.FindText("str_accept").ToString(),
                    GameTexts.FindText("str_cancel").ToString(),
                    () =>
                    {
                        InterestGroup.RemoveMember(Hero.MainHero);
                        RefreshValues();
                    },
                    null));
            }
            else
            {
                InformationManager.ShowInquiry(new InquiryData(
                   new TextObject("{=9SnWS77u}Join Group").ToString(),
                   new TextObject("{=XWOjp2ZM}You may join the {GROUP} group, represented by {LEADER}. Once joined, other members will expect your participation.")
                   .SetTextVariable("GROUP", GroupName)
                   .SetTextVariable("LEADER", InterestGroup.Leader.Name)
                   .ToString(),
                   true,
                   true,
                   GameTexts.FindText("str_accept").ToString(),
                   GameTexts.FindText("str_cancel").ToString(),
                   () =>
                   {
                       InterestGroup.AddMember(Hero.MainHero);
                       RefreshValues();
                   },
                   null));
            }
            RefreshValues();
        }

        [DataSourceMethod]
        private void ExecuteDemand()
        {
            var list = new List<InquiryElement>();
            if (InterestGroup.FactionLeader == Hero.MainHero)
            {
                InterestGroup.CurrentDemand.ShowPlayerPrompt();
            }
            else
            {
                BKExplainedNumber influence = BannerKingsConfig.Instance.InterestGroupsModel
                                .CalculateGroupInfluence(InterestGroup, true);
                foreach (Demand demand in InterestGroup.PossibleDemands)
                {
                    var possible = InterestGroup.CanPushDemand(demand, influence.ResultNumber);
                    list.Add(new InquiryElement(demand,
                        demand.Name.ToString(),
                        null,
                        possible.Item1,
                        new TextObject("{=ez3NzFgO}{TEXT}\n{EXPLANATIONS}")
                        .SetTextVariable("TEXT", demand.Description)
                        .SetTextVariable("EXPLANATIONS", possible.Item2)
                        .ToString()));
                }

                MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(new TextObject("{=zVboRONd}Push Demand").ToString(),
                    new TextObject("{=Ya1W2cuE}As the representative of the {GROUP}, you are able to push demands to {SUZERAIN} in the group's name. Pushing for a demand will often harm your relationship with your suzerain. How the group will respond will feel about it will depend entirely on the suzerain's response. Once a demand is pushed, the group is both unable to press the same kind of request and its influence is lowered, temporarily.")
                    .SetTextVariable("GROUP", GroupName)
                    .SetTextVariable("SUZERAIN", InterestGroup.FactionLeader.Name)
                    .ToString(),
                    list,
                    true,
                    1,
                    1,
                    GameTexts.FindText("str_accept").ToString(),
                    GameTexts.FindText("str_cancel").ToString(),
                    (list) =>
                    {
                        Demand demand = (Demand)list.First().Identifier;
                        demand.SetUp();
                    },
                    null));
            }

            RefreshValues();
        }

        [DataSourceProperty]
        public bool IsDemandEnabled
        {
            get => isDemandEnabled;
            set
            {
                if (value != isDemandEnabled)
                {
                    isDemandEnabled = value;
                    OnPropertyChangedWithValue(value, "IsDemandEnabled");
                }
            }
        }

        [DataSourceProperty]
        public string DemandName
        {
            get => demandName;
            set
            {
                if (value != demandName)
                {
                    demandName = value;
                    OnPropertyChangedWithValue(value, "DemandName");
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel DemandHint
        {
            get => demandHint;
            set
            {
                if (value != demandHint)
                {
                    demandHint = value;
                    OnPropertyChangedWithValue(value, "DemandHint");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<StringPairItemVM> SecondaryHeaders
        {
            get => secondaryHeaders;
            set
            {
                if (value != secondaryHeaders)
                {
                    secondaryHeaders = value;
                    OnPropertyChangedWithValue(value, "SecondaryHeaders");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<StringPairItemVM> TertiaryHeaders
        {
            get => tertiaryHeaders;
            set
            {
                if (value != tertiaryHeaders)
                {
                    tertiaryHeaders = value;
                    OnPropertyChangedWithValue(value, "TertiaryHeaders");
                }
            }
        }
    }
}
