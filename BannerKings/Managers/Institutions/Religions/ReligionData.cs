using BannerKings.Managers.Populations;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Institutions.Religions
{
    public class ReligionData : BannerKingsData
    {
        [SaveableField(2)] private Clergyman clergyman;

        public ReligionData(Religion religion, Settlement settlement)
        {
            Religions = new Dictionary<Religion, float> {{religion, 1f}};
            Settlement = settlement;
        }

        [field: SaveableField(3)] public Dictionary<Religion, float> Religions { get; }

        [field: SaveableField(1)] public Settlement Settlement { get; }

        public Religion DominantReligion
        {
            get
            {
                var eligible = Religions.Select(rel => (rel.Key, rel.Value));
                eligible = eligible.OrderByDescending(pair => pair.Value);

                return eligible.FirstOrDefault().Key;
            }
        }

        public Clergyman Clergyman
        {
            get
            {
                if (clergyman == null)
                {
                    clergyman = DominantReligion.GenerateClergyman(Settlement);
                }

                return clergyman;
            }
        }

        internal override void Update(PopulationData data)
        {
            var dominant = DominantReligion;

            clergyman = dominant.GetClergyman(data.Settlement) ?? dominant.GenerateClergyman(Settlement);
        }

        private (Religion, float) AddOwnerReligion(Religion dominant)
        {
            //TODO: Basileus
            return default;
        }
    }
}