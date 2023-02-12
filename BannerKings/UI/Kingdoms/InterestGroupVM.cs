using BannerKings.Behaviours.Diplomacy.Groups;
using TaleWorlds.Library;

namespace BannerKings.UI.Kingdoms
{
    public class InterestGroupVM : ViewModel
    {
        private MBBindingList<GroupMemberVM> members;
        private GroupMemberVM leader;
        public InterestGroup Group { get; }

        public InterestGroupVM(InterestGroup interestGroup)
        {
            Group = interestGroup;
            Members = new MBBindingList<GroupMemberVM>();
        }

        [DataSourceProperty] public string GroupName => Group.Name.ToString();

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
            Leader = new GroupMemberVM(Group.Leader, true);
            foreach (var member in Group.Members)
            {
                if (member != Leader.Hero)
                {
                    Members.Add(new GroupMemberVM(member));
                }
            }
        }
    }
}
