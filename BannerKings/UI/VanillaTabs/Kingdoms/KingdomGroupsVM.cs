using BannerKings.Behaviours.Diplomacy;
using BannerKings.UI.VanillaTabs.Kingdoms.Groups;
using System.Linq;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.VanillaTabs.Kingdoms
{
    public class KingdomGroupsVM : BannerKingsViewModel
    {
        private MBBindingList<InterestGroupVM> interestGroups;
        private MBBindingList<RadicalGroupVM> radicalGroups;
        private GroupItemVM currentGroup;
        private string interestGroupsCount, radicalGroupsCount;
        public KingdomGroupsVM(KingdomDiplomacy diplomacy) : base(null, false)
        {
            KingdomDiplomacy = diplomacy;
            InterestGroups = new MBBindingList<InterestGroupVM>();
            RadicalGroups = new MBBindingList<RadicalGroupVM>();

            if (KingdomDiplomacy != null)
            {
                foreach (var group in KingdomDiplomacy.Groups)
                {
                    InterestGroups.Add(new InterestGroupVM(group, this));
                }

                foreach (var group in KingdomDiplomacy.RadicalGroups)
                {
                    RadicalGroups.Add(new RadicalGroupVM(group, this));
                }

                InterestGroupsCountText = $"({InterestGroups.Count.ToString()})";
                if (CurrentGroup == null && InterestGroups.Count > 0)
                {
                    SetGroup(InterestGroups.First());
                }
            }
        }

        public KingdomDiplomacy KingdomDiplomacy { get; }

        [DataSourceProperty]
        public string GroupsText => new TextObject("{=F4Vv8Lc8}Groups").ToString();

        [DataSourceProperty]
        public string InterestGroupsText => new TextObject("{=AnFLBw7F}Interest Groups").ToString();

        [DataSourceProperty]
        public string RadicalGroupsText => new TextObject("{=k7QVZnaK}Radical Groups").ToString();

        public override void RefreshValues()
        {
            base.RefreshValues();
            if (InterestGroups != null)
            {
                foreach (var item in InterestGroups)
                {
                    item.RefreshValues();
                }
            }
        }

        public void SetGroup(GroupItemVM group)
        {
            if (CurrentGroup != null)
            {
                CurrentGroup.IsSelected = false;
            }

            CurrentGroup = group;
            CurrentGroup.IsSelected = true;
        }


        [DataSourceProperty]
        public GroupItemVM CurrentGroup
        {
            get => currentGroup;
            set
            {
                if (value != currentGroup)
                {
                    currentGroup = value;
                    OnPropertyChangedWithValue(value, "CurrentGroup");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<RadicalGroupVM> RadicalGroups
        {
            get => radicalGroups;
            set
            {
                if (value != radicalGroups)
                {
                    radicalGroups = value;
                    OnPropertyChangedWithValue(value, "RadicalGroups");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<InterestGroupVM> InterestGroups
        {
            get => interestGroups;
            set
            {
                if (value != interestGroups)
                {
                    interestGroups = value;
                    OnPropertyChangedWithValue(value, "InterestGroups");
                }
            }
        }

        [DataSourceProperty]
        public string InterestGroupsCountText
        {
            get => interestGroupsCount;
            set
            {
                if (value != interestGroupsCount)
                {
                    interestGroupsCount = value;
                    OnPropertyChangedWithValue(value, "InterestGroupsCountText");
                }
            }
        }

        [DataSourceProperty]
        public string RadicalGroupsCountText
        {
            get => radicalGroupsCount;
            set
            {
                if (value != radicalGroupsCount)
                {
                    radicalGroupsCount = value;
                    OnPropertyChangedWithValue(value, "RadicalGroupsCountText");
                }
            }
        }
    }
}
