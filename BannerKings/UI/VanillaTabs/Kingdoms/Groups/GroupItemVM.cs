using BannerKings.Behaviours.Diplomacy.Groups;
using BannerKings.Behaviours.Diplomacy;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Core.ViewModelCollection.Generic;

namespace BannerKings.UI.VanillaTabs.Kingdoms.Groups
{
    public class GroupItemVM : BannerKingsViewModel
    {
        private MBBindingList<GroupMemberVM> members;
        private MBBindingList<StringPairItemVM> headers;
        protected KingdomGroupsVM groupsVM;
        protected GroupMemberVM leader;
        protected ImageIdentifierVM clanBanner;
        protected string emptyGroupText, actionName;
        protected HintViewModel actionHint;
        protected bool isEmpty, isActionEnabled;
        private bool isInterest, isRadical;

        public GroupItemVM(DiplomacyGroup group, KingdomGroupsVM groupsVM) : base(null, false)
        {
            Group = group;
            this.groupsVM = groupsVM;
            KingdomDiplomacy = groupsVM.KingdomDiplomacy;
            isInterest = group.IsInterestGroup;
            isRadical = !isInterest;
        }

        public KingdomDiplomacy KingdomDiplomacy { get; }
        public DiplomacyGroup Group { get; }

        public void SetGroup()
        {
            groupsVM.SetGroup(this);
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            if (Group.Leader != null)
            {
                Leader = new GroupMemberVM(Group.Leader, true);
                if (Group.Leader.Clan != null)
                {
                    ClanBanner = new ImageIdentifierVM(BannerCode.CreateFrom(Group.Leader.Clan.Banner), true);
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
        public string EmptyGroupText
        {
            get => emptyGroupText;
            set
            {
                if (value != emptyGroupText)
                {
                    emptyGroupText = value;
                    OnPropertyChangedWithValue(value, "EmptyGroupText");
                }
            }
        }

        [DataSourceProperty]
        public bool IsInterest
        {
            get => isInterest;
            set
            {
                if (value != isInterest)
                {
                    isInterest = value;
                    OnPropertyChangedWithValue(value, "IsInterest");
                }
            }
        }

        [DataSourceProperty]
        public bool IsRadical
        {
            get => isRadical;
            set
            {
                if (value != isRadical)
                {
                    isRadical = value;
                    OnPropertyChangedWithValue(value, "IsRadical");
                }
            }
        }

        [DataSourceProperty]
        public ImageIdentifierVM ClanBanner
        {
            get => clanBanner;
            set
            {
                if (value != clanBanner)
                {
                    clanBanner = value;
                    OnPropertyChangedWithValue(value, "ClanBanner");
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
        public bool IsActionEnabled
        {
            get => isActionEnabled;
            set
            {
                if (value != isActionEnabled)
                {
                    isActionEnabled = value;
                    OnPropertyChangedWithValue(value, "IsActionEnabled");
                }
            }
        }

        [DataSourceProperty]
        public string ActionName
        {
            get => actionName;
            set
            {
                if (value != actionName)
                {
                    actionName = value;
                    OnPropertyChangedWithValue(value, "ActionName");
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel ActionHint
        {
            get => actionHint;
            set
            {
                if (value != actionHint)
                {
                    actionHint = value;
                    OnPropertyChangedWithValue(value, "ActionHint");
                }
            }
        }
    }
}
