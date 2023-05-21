using System;
using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers.Populations.Estates;
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

        public int GetNotableManpower(PopType type, Hero notable, EstateData data)
        {
            int result = 0;

            if (data != null)
            {
                var estate = data.GetHeroEstate(notable);
                if (estate != null)
                {
                    return estate.GetManpower(type);
                }
            }

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
                return Manpowers[PopType.Nobles];
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

        public PopType GetCharacterManpowerType(CharacterObject character)
        {
            if (Utils.Helpers.IsRetinueTroop(character))
            {
                return PopType.Nobles;
            }

            List<ValueTuple<PopType, float>> options = new List<(PopType, float)>();
            var classes = BannerKingsConfig.Instance.VolunteerModel.GetMilitaryClasses(settlement);
            foreach (var pair in Manpowers)
            {
                PopType poptype = pair.Key;
                if (poptype == PopType.Nobles)
                {
                    continue;
                }

                float militarism = 0f;
                foreach (var tuple in classes)
                {
                    if (tuple.Item1 == poptype)
                    {
                        militarism = tuple.Item2;
                    }
                }

                if (character.Tier >= 3 && poptype == PopType.Craftsmen)
                {
                    militarism *= 1.3f;
                }

                if (militarism != 0f)
                {
                    options.Add(new(pair.Key, militarism));
                }
            }

            return MBRandom.ChooseWeighted(options);
        }

        public void AddManpowerFromSoldiers(PopulationData data, int quantity, CharacterObject troop)
        {
            InitManpowers();
            PopType type = GetCharacterManpowerType(troop);

            Manpowers[type] += quantity;
            data.UpdatePopType(type, quantity);
        }

        public void DeduceManpower(PopulationData data, int quantity, CharacterObject troop, Hero notable)
        {
            InitManpowers();
            PopType type = GetCharacterManpowerType(troop);
            if (data.EstateData != null)
            {
                var estate = data.EstateData.GetHeroEstate(notable);
                if (estate != null && (type == PopType.Serfs || type == PopType.Slaves))
                {
                    estate.AddManpower(type, -quantity);
                    estate.AddPopulation(type, -quantity);
                    return;
                }
            }

            Manpowers[type] -= quantity;
            data.UpdatePopType(type, -quantity);
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

                if (data.EstateData != null)
                {
                    foreach (var estate in data.EstateData.Estates)
                    {
                        if (estate.IsDisabled)
                        {
                            continue;
                        }

                        float estateMaxManpower = estate.GetTypeCount(type) * militarism;
                        float estateGrowth = estateMaxManpower * 0.01f;
                        estate.AddManpower(type, estateGrowth);
                    }
                }
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