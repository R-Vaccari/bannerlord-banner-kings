using BannerKings.Behaviours.Diplomacy;
using TaleWorlds.Library;

namespace BannerKings.UI.Kingdoms
{
    public class KingdomGroupsVM : BannerKingsViewModel
    {
        private MBBindingList<InterestGroupVM> interestGroups;

        public KingdomGroupsVM(KingdomDiplomacy diplomacy) : base(null, false)
        {
            KingdomDiplomacy = diplomacy;
            InterestGroups = new MBBindingList<InterestGroupVM>();
        }

        public KingdomDiplomacy KingdomDiplomacy { get; }

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

        public override void RefreshValues()
        {
            base.RefreshValues();
            InterestGroups.Clear();
            if (KingdomDiplomacy != null)
            {
                foreach (var group in KingdomDiplomacy.Groups)
                {
                    InterestGroups.Add(new InterestGroupVM(group, KingdomDiplomacy));
                }

                foreach (var item in InterestGroups)
                {
                    item.RefreshValues();
                }
            }
        }
    }
}
