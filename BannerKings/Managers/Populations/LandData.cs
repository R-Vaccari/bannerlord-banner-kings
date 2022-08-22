using System;
using System.Collections.Generic;
using BannerKings.Managers.Innovations;
using BannerKings.Managers.Policies;
using BannerKings.Managers.Populations.Villages;
using BannerKings.Managers.Skills;
using BannerKings.Models.Vanilla;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Populations
{
    public class LandData : BannerKingsData
    {
        public LandData(PopulationData data)
        {
            this.data = data;
            composition = new float[3];
            Init(data.TotalPop);
        }

        [SaveableProperty(1)] private PopulationData data { get; set; }

        [SaveableProperty(2)] private float farmland { get; set; }

        [SaveableProperty(3)] private float pasture { get; set; }

        [SaveableProperty(4)] private float woodland { get; set; }

        [SaveableProperty(5)] private float fertility { get; set; }

        [SaveableProperty(6)] private float terrainDifficulty { get; set; }

        [SaveableProperty(7)] private float[] composition { get; set; }

        public TerrainType Terrain => Campaign.Current.MapSceneWrapper.GetTerrainTypeAtPosition(data.Settlement.Position2D);

        public int AvailableWorkForce
        {
            get
            {
                var serfs = data.GetTypeCount(PopulationManager.PopType.Serfs) * (data.Settlement.IsVillage ? 0.85f : 0.5f);
                float slaves = data.GetTypeCount(PopulationManager.PopType.Slaves);

                var town = data.Settlement.Town;
                if (town != null && town.BuildingsInProgress.Count > 0)
                {
                    slaves -= slaves * data.EconomicData.StateSlaves * 0.5f;
                }

                if (!data.Settlement.IsVillage)
                {
                    if (BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(data.Settlement, "workforce",
                            (int) BKWorkforcePolicy.WorkforcePolicy.Martial_Law))
                    {
                        var militia = data.Settlement.Town.Militia / 2;
                        serfs -= militia / 2f;
                    }
                    else if (BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(data.Settlement, "workforce",
                                 (int) BKWorkforcePolicy.WorkforcePolicy.Land_Expansion))
                    {
                        serfs *= 0.8f;
                        slaves *= 0.8f;
                    }
                    else if (BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(data.Settlement, "workforce",
                                 (int) BKWorkforcePolicy.WorkforcePolicy.Construction))
                    {
                        serfs *= 0.85f;
                        slaves -= slaves * data.EconomicData.StateSlaves * 0.5f;
                    }
                }

                return Math.Max((int) (serfs + slaves), 0);
            }
        }

        public float WorkforceSaturation
        {
            get
            {
                float available = AvailableWorkForce;
                var farms = farmland / GetRequiredLabor("farmland");
                var pasture = this.pasture / GetRequiredLabor("pasture");
                return available / (farms + pasture);
            }
        }

        public float Acreage => farmland + pasture + woodland;
        public float Farmland => farmland;
        public float Pastureland => pasture;
        public float Woodland => woodland;
        public float Fertility => fertility;
        public float Difficulty => terrainDifficulty;

        private void Init(int totalPops)
        {
            float farmRatio;
            float pastureRatio;
            float woodRatio;
            switch (Terrain)
            {
                case TerrainType.Desert:
                    fertility = 0.5f;
                    terrainDifficulty = 1.4f;
                    farmRatio = 0.9f;
                    pastureRatio = 0.08f;
                    woodRatio = 0.02f;
                    break;
                case TerrainType.Steppe:
                    fertility = 0.75f;
                    terrainDifficulty = 1f;
                    farmRatio = 0.45f;
                    pastureRatio = 0.5f;
                    woodRatio = 0.05f;
                    break;
                case TerrainType.Mountain:
                    fertility = 0.7f;
                    terrainDifficulty = 2f;
                    farmRatio = 0.5f;
                    pastureRatio = 0.35f;
                    woodRatio = 0.15f;
                    break;
                case TerrainType.Canyon:
                    fertility = 0.5f;
                    terrainDifficulty = 2f;
                    farmRatio = 0.9f;
                    pastureRatio = 0.08f;
                    woodRatio = 0.02f;
                    break;
                case TerrainType.Forest:
                    fertility = 0.5f;
                    terrainDifficulty = 2f;
                    farmRatio = 0.45f;
                    pastureRatio = 0.15f;
                    woodRatio = 0.40f;
                    break;
                default:
                    fertility = 1f;
                    terrainDifficulty = 1f;
                    farmRatio = 0.7f;
                    pastureRatio = 0.22f;
                    woodRatio = 0.08f;
                    break;
            }

            composition[0] = farmRatio;
            composition[1] = pastureRatio;
            composition[2] = woodRatio;
            var acres = data.Settlement.IsVillage
                ? totalPops * MBRandom.RandomFloatRanged(3f, 3.5f)
                : totalPops * MBRandom.RandomFloatRanged(2.5f, 3.0f);
            farmland = acres * farmRatio;
            pasture = acres * pastureRatio;
            woodland = acres * woodRatio;
        }

        public float GetRequiredLabor(string type)
        {
            return type switch
            {
                "farmland" => 4f,
                "pasture" => 8f,
                _ => 10f
            };
        }

        public float GetAcreOutput(string type)
        {
            var result = type switch
            {
                "farmland" => 0.018f,
                "pasture" => 0.006f,
                _ => 0.0012f
            };

            Hero owner = null;
            if (data.Settlement.OwnerClan != null)
            {
                owner = data.Settlement.Owner;
            }

            if (owner != null)
            {
                var data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(owner);
                if (data.HasPerk(BKPerks.Instance.CivilCultivator))
                {
                    result *= 0.05f;
                }
            }

            var innovations = BannerKingsConfig.Instance.InnovationsManager.GetInnovationData(data.Settlement.Culture);
            if (innovations != null)
            {
                if (type == "farmland")
                {
                    if (innovations.HasFinishedInnovation(DefaultInnovations.Instance.HeavyPlough))
                    {
                        result *= 0.08f;
                    }

                    if (innovations.HasFinishedInnovation(DefaultInnovations.Instance.ThreeFieldsSystem))
                    {
                        result *= 0.25f;
                    }
                }
            }


            return result;
        }

        internal override void Update(PopulationData data)
        {
            if (this.data.Settlement.IsVillage)
            {
                var villageData = data.VillageData;
                var construction = this.data.VillageData.Construction;
                var progress = 15f / construction;
                var type = villageData.CurrentDefault.BuildingType;
                if (type != DefaultVillageBuildings.Instance.DailyProduction)
                {
                    if (type == DefaultVillageBuildings.Instance.DailyFarm)
                    {
                        this.farmland += progress;
                    }
                    else if (type == DefaultVillageBuildings.Instance.DailyPasture)
                    {
                        pasture += progress;
                    }
                    else
                    {
                        this.woodland += progress;
                    }
                }
            }
            else if (BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(this.data.Settlement, "workforce",
                         (int) BKWorkforcePolicy.WorkforcePolicy.Land_Expansion))
            {
                var laborers = AvailableWorkForce * 0.2f;
                var construction = laborers * 0.010f;
                var progress = 15f / construction;

                if (progress > 0f)
                {
                    var list = new List<(int, float)>
                    {
                        new ValueTuple<int, float>(0, composition[0]),
                        new ValueTuple<int, float>(1, composition[1]),
                        new ValueTuple<int, float>(2, composition[2])
                    };
                    var choosen = MBRandom.ChooseWeighted(list);

                    switch (choosen)
                    {
                        case 0:
                            this.farmland += progress;
                            break;
                        case 1:
                            pasture += progress;
                            break;
                        default:
                            this.woodland += progress;
                            break;
                    }
                }
            }


            if (WorkforceSaturation > 1f)
            {
                var list = new List<(int, float)>
                {
                    new ValueTuple<int, float>(0, composition[0]),
                    new ValueTuple<int, float>(1, composition[1]),
                    new ValueTuple<int, float>(2, composition[2])
                };
                var choosen = MBRandom.ChooseWeighted(list);

                var construction = this.data.Settlement.IsVillage
                    ? this.data.VillageData.Construction
                    : new BKConstructionModel().CalculateDailyConstructionPower(this.data.Settlement.Town).ResultNumber;
                construction *= 0.8f;
                var progress = 15f / construction;

                switch (choosen)
                {
                    case 0:
                        this.farmland += progress;
                        break;
                    case 1:
                        pasture += progress;
                        break;
                    default:
                        this.woodland += progress;
                        break;
                }
            }

            var farmland = this.farmland;
            var pastureland = pasture;
            var woodland = this.woodland;

            this.farmland = MBMath.ClampFloat(farmland, 0f, 100000f);
            pasture = MBMath.ClampFloat(pastureland, 0f, 50000f);
            this.woodland = MBMath.ClampFloat(woodland, 0f, 50000f);
        }
    }
}