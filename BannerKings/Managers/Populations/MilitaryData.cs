using System;
using System.Collections.Generic;
using System.Linq;
using BannerKings.Models.Vanilla;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.SaveSystem;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Managers.Populations
{
    public class MilitaryData : BannerKingsData
    {
        public MilitaryData(Settlement settlement, int peasantManpower, int nobleManpower)
        {
            this.settlement = settlement;
            engines = new List<SiegeEngineType>();
            Manpowers = new Dictionary<PopType, float>();
        }

        [SaveableProperty(1)] private Settlement settlement { get; set; }

        [SaveableProperty(4)] private List<SiegeEngineType> engines { get; set; }

        [SaveableProperty(5)] private Dictionary<PopType, float> Manpowers { get; set; }

        public int GetManpower(PopType type)
        {
            int result = 0;
            if (Manpowers.ContainsKey(type))
            {
                result = (int)Manpowers[type];
            }

            return result;
        }

        public float PeasantManpower
        {
            get
            {
                InitManpowers();
                float value = 0f;
                foreach (var pair in Manpowers)
                {
                    if (pair.Key == PopType.Nobles)
                    {
                        continue;
                    }

                    value += pair.Value;
                }
                return value;
            }
        }

        public float NobleManpower
        {
            get
            {
                InitManpowers();
                float value = 0f;
                foreach (var pair in Manpowers)
                {
                    if (pair.Key != PopType.Nobles)
                    {
                        continue;
                    }

                    value += pair.Value;
                }
                return value;
            }
        }


        public float Manpower => PeasantManpower + NobleManpower;

        public ExplainedNumber DraftEfficiency
        {
            get
            {
                var number = new ExplainedNumber(0f);
                if (settlement.Notables is {Count: > 0})
                {
                    number = BannerKingsConfig.Instance.VolunteerModel.GetDraftEfficiency(settlement.Notables[0], 2, settlement);
                }

                return number;
            }
        }

        public ExplainedNumber Militarism => BannerKingsConfig.Instance.VolunteerModel.GetMilitarism(settlement);

        public int Holdout => new BKFoodModel().GetFoodEstimate(settlement, settlement.Town.FoodStocksUpperLimit());

        public IEnumerable<SiegeEngineType> Engines => engines;

        public int Ballistae => new BKSiegeEventModel().GetPrebuiltSiegeEnginesOfSettlement(settlement)
            .Count(x => x == DefaultSiegeEngineTypes.Ballista);

        public int Catapultae => new BKSiegeEventModel().GetPrebuiltSiegeEnginesOfSettlement(settlement)
            .Count(x => x == DefaultSiegeEngineTypes.Catapult);

        public int Trebuchets => new BKSiegeEventModel().GetPrebuiltSiegeEnginesOfSettlement(settlement)
            .Count(x => x == DefaultSiegeEngineTypes.Trebuchet);

        public void DeduceManpower(PopulationData data, int quantity, CharacterObject troop)
        {
            InitManpowers();

            var tier = troop.Tier;
            var noble = Utils.Helpers.IsRetinueTroop(troop);
            if (noble)
            {
                Manpowers[PopType.Nobles] -= quantity;
                data.UpdatePopType(PopType.Nobles, -quantity);
            }
            else
            {
                List<ValueTuple<PopType, float>> options = new List<(PopType, float)>();
                var classes = BannerKingsConfig.Instance.VolunteerModel.GetMilitaryClasses(settlement);
                foreach (var pair in Manpowers)
                {
                    if (pair.Key == PopType.Nobles)
                    {
                        continue;
                    }

                    float militarism = classes.First(x => x.Item1 == pair.Key).Item2;

                    if (troop.Tier >= 3 && pair.Key == PopType.Craftsmen)
                    {
                        militarism *= 1.3f;
                    }

                    options.Add(new (pair.Key, militarism));
                }

                var result = MBRandom.ChooseWeighted(options);
                Manpowers[result] -= quantity;
                data.UpdatePopType(result, -quantity);
            }
        }

        internal override void Update(PopulationData data)
        {
            InitManpowers();

            foreach (ValueTuple<PopType, float> tuple in BannerKingsConfig.Instance.VolunteerModel.GetMilitaryClasses(settlement))
            {
                float militarism = tuple.Item2;
                PopType type = tuple.Item1;
                if (!Manpowers.ContainsKey(type))
                {
                    Manpowers.Add(type, data.GetTypeCount(type) * militarism * 0.5f);
                }

                float maxManpower = data.GetTypeCount(type) * militarism;
                float growth = maxManpower * 0.01f;
                Manpowers[type] += growth;
                Manpowers[type] = MathF.Clamp(Manpowers[type], 0f, maxManpower);
            }
        }

        private void InitManpowers()
        {
            if (Manpowers == null)
            {
                Manpowers = new Dictionary<PopType, float>();
            }

            if (Manpowers.Count == 0)
            {
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                foreach (ValueTuple<PopType, float> tuple in BannerKingsConfig.Instance.VolunteerModel.GetMilitaryClasses(settlement))
                {
                    PopType type = tuple.Item1;
                    if (!Manpowers.ContainsKey(type))
                    {
                        Manpowers.Add(type, data.GetTypeCount(type) * tuple.Item2 * 0.5f);
                    }
                }
            }
        }
    }
}