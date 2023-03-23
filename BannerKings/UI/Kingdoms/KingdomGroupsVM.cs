using BannerKings.Behaviours.Diplomacy;
using System.Linq;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Kingdoms
{
    public class KingdomGroupsVM : BannerKingsViewModel
    {
        private MBBindingList<InterestGroupVM> interestGroups;
        private InterestGroupVM currentGroup;
        private string interestGroupsCount;
        public KingdomGroupsVM(KingdomDiplomacy diplomacy) : base(null, false)
        {
            KingdomDiplomacy = diplomacy;
            InterestGroups = new MBBindingList<InterestGroupVM>();

            if (KingdomDiplomacy != null)
            {
                foreach (var group in KingdomDiplomacy.Groups)
                {
                    InterestGroups.Add(new InterestGroupVM(group, this));
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
        public string GroupsText => new TextObject("{=!}Groups").ToString();

        [DataSourceProperty]
        public string InterestGroupsText => new TextObject("{=!}Interest Groups").ToString();

        [DataSourceProperty]
        public string RadicalGroupsText => new TextObject("{=!}Radical Groups").ToString();

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

        public void SetGroup(InterestGroupVM group)
        {
            if (CurrentGroup != null)
            {
                CurrentGroup.IsSelected = false;
            }

            CurrentGroup = group;
            CurrentGroup.IsSelected = true;
        }


        [DataSourceProperty]
        public InterestGroupVM CurrentGroup
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
    }
}
