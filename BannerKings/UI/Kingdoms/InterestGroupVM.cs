using BannerKings.Behaviours.Diplomacy;
using BannerKings.Behaviours.Diplomacy.Groups;
using BannerKings.Utils.Models;
using System;
using System.Linq;
using TaleWorlds.Core.ViewModelCollection.Generic;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Kingdoms
{
    public class InterestGroupVM : BannerKingsViewModel
    {
        private MBBindingList<StringPairItemVM> headers;
        private MBBindingList<StringPairItemVM> secondaryHeaders;
        private MBBindingList<StringPairItemVM> tertiaryHeaders;
        private MBBindingList<GroupMemberVM> members;
        private GroupMemberVM leader;
        private bool isEmpty;

        public KingdomDiplomacy KingdomDiplomacy { get; }
        public InterestGroup Group { get; }

        public InterestGroupVM(InterestGroup interestGroup, KingdomDiplomacy diplomacy) : base(null, false)
        {
            Group = interestGroup;
            KingdomDiplomacy = diplomacy;
            Members = new MBBindingList<GroupMemberVM>();
            Headers = new MBBindingList<StringPairItemVM>();
            SecondaryHeaders = new MBBindingList<StringPairItemVM>();
            TertiaryHeaders = new MBBindingList<StringPairItemVM>();
        }

        [DataSourceProperty] public string LeaderText => new TextObject("{=SrfYbg3x}Leader").ToString();
        [DataSourceProperty] public string GroupName => Group.Name.ToString();
        [DataSourceProperty] public HintViewModel Hint => new HintViewModel(Group.Description);

        public override void RefreshValues()
        {
            base.RefreshValues();
            Members.Clear();
            Headers.Clear();
            SecondaryHeaders.Clear();
            TertiaryHeaders.Clear();
            IsEmpty = Group.Members.Count == 0;
            if (Group.Leader != null)
            {
                Leader = new GroupMemberVM(Group.Leader, true);
            }

            foreach (var member in Group.GetSortedMembers().Take(5))
            {
                if (member != Leader.Hero)
                {
                    Members.Add(new GroupMemberVM(member));
                }
            }

            BKExplainedNumber influence = BannerKingsConfig.Instance.InterestGroupsModel
                .CalculateGroupInfluence(Group, KingdomDiplomacy, true);

            Headers.Add(new StringPairItemVM(new TextObject("{=EkFaisgP}Influence").ToString(),
                FormatValue(influence.ResultNumber),
                new BasicTooltipViewModel(() => influence.GetFormattedPercentage())));

            BKExplainedNumber support = BannerKingsConfig.Instance.InterestGroupsModel
                .CalculateGroupSupport(Group, KingdomDiplomacy, true);

            Headers.Add(new StringPairItemVM(new TextObject("{=!}Support").ToString(),
                FormatValue(support.ResultNumber), 
                new BasicTooltipViewModel(() => support.GetFormattedPercentage())));

            Headers.Add(new StringPairItemVM(new TextObject("{=!}Members").ToString(),
                Group.Members.Count.ToString(),
                new BasicTooltipViewModel(() => new TextObject("{=!}The amount of members in this group.").ToString())));

            TextObject endorsedExplanation = new TextObject("{=!}Laws\n{LAWS}\n\n\nPolicies\n{POLICIES}\n\n\nCasus Belli\n{CASUS}")
                .SetTextVariable("LAWS", Group.SupportedLaws.Aggregate("", (current, law) => 
                current + Environment.NewLine + law.Name))
                .SetTextVariable("POLICIES", Group.SupportedPolicies.Aggregate("", (current, policy) =>
                current + Environment.NewLine + policy.Name))
                .SetTextVariable("CASUS", Group.SupportedCasusBelli.Aggregate("", (current, casus) =>
                current + Environment.NewLine + casus.Name));

            TertiaryHeaders.Add(new StringPairItemVM(new TextObject("{=!}Endorsed Acts").ToString(), 
                string.Empty,
                new BasicTooltipViewModel(() => endorsedExplanation.ToString())));

            TextObject demandsExplanation = new TextObject("{=!}Possible demands\n{DEMANDS}")
                .SetTextVariable("DEMANDS", Group.PossibleDemands.Aggregate("", (current, law) =>
                current + Environment.NewLine + law.Name));

            TertiaryHeaders.Add(new StringPairItemVM(new TextObject("{=!}Demands").ToString(),
                string.Empty,
                new BasicTooltipViewModel(() => demandsExplanation.ToString())));

            TextObject shunnedExplanation = new TextObject("{=!}Laws\n{LAWS}\n\n\nPolicies\n{POLICIES}")
                .SetTextVariable("LAWS", Group.SupportedLaws.Aggregate("", (current, law) =>
                current + Environment.NewLine + law.Name))
                .SetTextVariable("POLICIES", Group.SupportedPolicies.Aggregate("", (current, policy) =>
                current + Environment.NewLine + policy.Name));

            TertiaryHeaders.Add(new StringPairItemVM(new TextObject("{=!}Shunned Acts").ToString(),
                string.Empty,
                new BasicTooltipViewModel(() => shunnedExplanation.ToString())));
        }

        [DataSourceProperty]
        public bool IsEmpty
        {
            get => isEmpty;
            set
            {
                if (value != isEmpty)
                {
                    isEmpty = value;
                    OnPropertyChangedWithValue(value, "IsEmpty");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<StringPairItemVM> Headers
        {
            get => headers;
            set
            {
                if (value != headers)
                {
                    headers = value;
                    OnPropertyChangedWithValue(value, "Headers");
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

        [DataSourceProperty]
        public GroupMemberVM Leader
        {
            get => leader;
            set
            {
                if (value != leader)
                {
                    leader = value;
                    OnPropertyChangedWithValue(value, "Leader");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<GroupMemberVM> Members
        {
            get => members;
            set
            {
                if (value != members)
                {
                    members = value;
                    OnPropertyChangedWithValue(value, "Members");
                }
            }
        }
    }
}
