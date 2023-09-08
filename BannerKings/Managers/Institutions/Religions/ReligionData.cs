using BannerKings.Managers.Populations;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Institutions.Religions
{
    public class ReligionData : BannerKingsData
    {
        public ReligionData(Religion religion, Settlement settlement)
        {
            Religions = new Dictionary<Religion, float> {{religion, 1f}};
            Settlement = settlement;
        }

        [field: SaveableField(3)]
        public Dictionary<Religion, float> Religions { get; private set; }

        [field: SaveableField(1)] public Settlement Settlement { get; }

        public Religion GetRandomReligion()
        {
            foreach (var pair in Religions)
            {
                if (MBRandom.RandomFloat <= pair.Value)
                {
                    return pair.Key;
                }
            }

            return DominantReligion;
        }

        public float GetHeathenPercentage(Religion target)
        {
            var result = 0f;
            if (Religions.Count > 0)
            {
                foreach (var religion in Religions)
                {
                    if (religion.Key != target)
                    {
                        result += religion.Value;
                    }
                }
            }

            return result;
        }

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

                if (eligible.Count == 0)
                {
                    // in case owner has no faith
                    return null;
                }

                eligible = eligible.OrderByDescending(pair => pair.Item2).ToList();
                return eligible[0].Item1;
            }
        }

        public ExplainedNumber Tension => BannerKingsConfig.Instance.ReligionModel.CalculateTensionTarget(this);

        private void BalanceReligions(Religion dominant)
        {
            if (dominant is null)
            {
                return;
            }

            var weightDictionary = new Dictionary<Religion, float>();
            var totalWeight = 0f;
            foreach (var pair in Religions)
            {
                var weight = BannerKingsConfig.Instance.ReligionModel.CalculateReligionWeight(pair.Key, Settlement).ResultNumber;
                weightDictionary.Add(pair.Key, weight);
                totalWeight += weight;
            }

            foreach (var pair in weightDictionary)
            {
                float targetProportion = pair.Value / totalWeight;
                float diff = targetProportion - Religions[pair.Key];
                Religions[pair.Key] += diff * 0.01f;
            }
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
                AddHeroesReligion();
            }

            if (dominant == null)
            {
                return;
            }

            if (Religions.Count > 1)
            {
                BalanceReligions(dominant);
            }

            foreach (Religion rel in Religions.Keys)
            {
                if (rel == DominantReligion)
                {
                    var clergyman = rel.GetClergyman(data.Settlement);
                    if (clergyman != null && data.Settlement.Notables.Contains(clergyman.Hero))
                    {
                        EnterSettlementAction.ApplyForCharacterOnly(clergyman.Hero, data.Settlement);
                    }
                }
               
            }
        }

        private void InitializeReligions()
        {
            Religions = new Dictionary<Religion, float>();
            AddHeroesReligion();
        }

        private void AddHeroesReligion()
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
                    var value = 0.001f;
                    if (Religions.Count == 0)
                    {
                        value = 1f;
                    }
                    Religions.Add(rel, value);
                }
            }

            if (Settlement.Notables != null)
            {
                foreach (var notable in Settlement.Notables)
                {
                    var rel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(notable);
                    if (rel != null && !Religions.ContainsKey(rel))
                    {
                        Religions.Add(rel, 0.001f);
                    }
                }
            }
        }
    }
}