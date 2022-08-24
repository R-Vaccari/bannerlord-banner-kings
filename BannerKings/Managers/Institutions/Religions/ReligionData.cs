using BannerKings.Managers.Populations;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Institutions.Religions
{
    public class ReligionData : BannerKingsData
    {
        [SaveableField(2)] private Clergyman clergyman;

        public ReligionData(Religion religion, Settlement settlement)
        {
            Religions = new Dictionary<Religion, float>();
            Religions.Add(religion, 1f);
            Settlement = settlement;
        }

        [field: SaveableField(3)] public Dictionary<Religion, float> Religions { get; }

        [field: SaveableField(1)] public Settlement Settlement { get; }

        public Religion DominantReligion
        {
            get
            {
                var eligible = new List<(Religion, float)>();
                foreach (var rel in Religions)
                {
                    eligible.Add((rel.Key, rel.Value));
                }

                eligible.OrderByDescending(pair => pair.Item2);
                return eligible[0].Item1;
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

        private void BalanceReligions(Religion dominant)
        {
            List<(Religion, float)> candidates = new List<(Religion, float)>();
            Dictionary<Religion, float> weightDictionary = new Dictionary<Religion, float>();

            float totalWeight = 0f;
            foreach (var pair in Religions)
            {
                var weight = BannerKingsConfig.Instance.ReligionModel.CalculateReligionWeight(pair.Key, Settlement).ResultNumber;
                weightDictionary.Add(pair.Key, weight);
                totalWeight += weight;
            }


            var dominantWeight = weightDictionary[dominant];
            float proportion = dominantWeight / totalWeight;
            float diff = proportion - Religions[dominant];
            if (diff != 0f)
            {
                float conversion = BannerKingsConfig.Instance.ReligionModel.CalculateReligionConversion(dominant, Settlement, diff).ResultNumber;
                foreach (var pair in weightDictionary)
                {
                    if (pair.Key == dominant)
                    {
                        continue;
                    }

                    candidates.Add(new (pair.Key, pair.Value));
                }

                Religion target = MBRandom.ChooseWeighted(candidates);
                Religions[dominant] += conversion;
                Religions[target] -= conversion;
            }
        }

        internal override void Update(PopulationData data)
        {
            var dominant = DominantReligion;
            Hero owner = null;
            if (Settlement.OwnerClan != null)
            {
                owner = Settlement.OwnerClan.Leader;
            }

            if (owner != null)
            {
                Religion rel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(owner);
                if (rel != null && !Religions.ContainsKey(rel))
                {
                    Religions.Add(rel, 0.001f);
                }
            }

            BalanceReligions(dominant);

            clergyman = dominant.GetClergyman(data.Settlement);
            if (clergyman == null)
            {
                clergyman = dominant.GenerateClergyman(Settlement);
            }
        }

    }
}