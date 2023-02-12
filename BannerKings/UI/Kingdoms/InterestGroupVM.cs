using BannerKings.Behaviours.Diplomacy.Groups;
using System.Linq;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Kingdoms
{
    public class InterestGroupVM : ViewModel
    {
        private MBBindingList<GroupMemberVM> members;
        private GroupMemberVM leader;
        private bool isEmpty;
        public InterestGroup Group { get; }

        public InterestGroupVM(InterestGroup interestGroup)
        {
            Group = interestGroup;
            Members = new MBBindingList<GroupMemberVM>();
        }

        [DataSourceProperty] public string LeaderText => new TextObject("{=SrfYbg3x}Leader").ToString();
        [DataSourceProperty] public string GroupName => Group.Name.ToString();
        [DataSourceProperty] public HintViewModel Hint => new HintViewModel(Group.Description);

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

        public override void RefreshValues()
        {
            base.RefreshValues();
            Members.Clear();
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
        }
    }
}
