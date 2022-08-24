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
            Religions = new Dictionary<Religion, float> {{religion, 1f}};
            Settlement = settlement;
        }

        [field: SaveableField(3)]
        public Dictionary<Religion, float> Religions { get; private set; }

        [field: SaveableField(1)] public Settlement Settlement { get; }

        public Religion DominantReligion
        {
            get
            {
                var eligible = new List<(Religion, float)>();
                if (Religions is null)
                {
                    return null;
                }

                foreach (var rel in Religions)
                {
                    eligible.Add((rel.Key, rel.Value));
                }

                eligible = eligible.OrderByDescending(pair => pair.Item2).ToList();
                return eligible[0].Item1;
            }
        }

        public Clergyman Clergyman
        {
            get
            {
                if (clergyman == null && DominantReligion != null)
                {
                    clergyman = DominantReligion.GenerateClergyman(Settlement);
                }

                return clergyman;
            }
        }

        private void BalanceReligions(Religion dominant)
        {
            if (dominant is null)
            {
                return;
            }

            var candidates = new List<(Religion, float)>();
            var weightDictionary = new Dictionary<Religion, float>();

            var totalWeight = 0f;
            foreach (var pair in Religions)
            {
                var weight = BannerKingsConfig.Instance.ReligionModel.CalculateReligionWeight(pair.Key, Settlement).ResultNumber;
                weightDictionary.Add(pair.Key, weight);
                totalWeight += weight;
            }


            var dominantWeight = weightDictionary[dominant];
            var proportion = dominantWeight / totalWeight;
            var diff = proportion - Religions[dominant];
            if (diff is 0f or float.NaN)
            {
                return;
            }
            
            var conversion = BannerKingsConfig.Instance.ReligionModel.CalculateReligionConversion(dominant, Settlement, diff).ResultNumber;
            foreach (var pair in weightDictionary)
            {
                if (pair.Key == dominant)
                {
                    continue;
                }

                candidates.Add(new (pair.Key, pair.Value));
            }

            var target = MBRandom.ChooseWeighted(candidates);
            if (target is not null)
            {
                Religions[target] -= conversion;
            }

            Religions[dominant] += conversion;
        }

        internal override void Update(PopulationData data)
        {
            var dominant = DominantReligion;
            if (dominant == null)
            {
                InitializeReligions();
            } 
            else
            {
                AddOwnerReligion();
            }

            if (Religions.Count > 1)
            {
                BalanceReligions(dominant);
            }

            clergyman = dominant.GetClergyman(data.Settlement) ?? dominant.GenerateClergyman(Settlement);
        }

        private void InitializeReligions()
        {
            Religions = new Dictionary<Religion, float>();
            AddOwnerReligion();
        }

        private void AddOwnerReligion()
        {
            Hero owner = null;
            if (Settlement.OwnerClan != null)
            {
                owner = Settlement.OwnerClan.Leader;
            }

            if (owner != null)
            {
                var rel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(owner);
                if (rel != null && !Religions.ContainsKey(rel))
                {
                    float value = 0.001f;
                    if (Religions.Count == 0)
                    {
                        value = 1f;
                    }
                    Religions.Add(rel, value);
                }
            }
        }
    }
}