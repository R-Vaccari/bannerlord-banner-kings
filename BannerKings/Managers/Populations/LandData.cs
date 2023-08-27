using System;
using System.Collections.Generic;
using BannerKings.Managers.Innovations;
using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Institutions.Religions.Doctrines;
using BannerKings.Managers.Policies;
using BannerKings.Managers.Populations.Villages;
using BannerKings.Managers.Skills;
using BannerKings.Managers.Titles.Laws;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.SaveSystem;
using static BannerKings.Managers.PopulationManager;

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

        public float[] Composition => composition;

        public TerrainType Terrain => Campaign.Current.MapSceneWrapper != null ? 
            Campaign.Current.MapSceneWrapper.GetTerrainTypeAtPosition(data.Settlement.Position2D) : TerrainType.Plain;

        public bool IsExcessLaborExpandingAcreage
        {
            get
            {
                bool result;
                if (data.Settlement.IsVillage)
                {
                    result = WorkforceSaturation > 1.3f;
                }
                else
                {
                    result = WorkforceSaturation > 1f && data.Settlement.Town.FoodChange < 0f;
                }

                return result;
            }
        }

        public int AvailableSerfsWorkForce
        {
            get
            {
                var serfs = data.GetTypeCount(PopulationManager.PopType.Serfs) * (data.Settlement.IsVillage ? 0.85f : 0.5f);
                int toSubtract = 0;

                if (!data.Settlement.IsVillage)
                {
                    if (BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(data.Settlement, "workforce",
                            (int)BKWorkforcePolicy.WorkforcePolicy.Martial_Law))
                    {
                        var militia = data.Settlement.Town.Militia / 2;
                        serfs -= militia / 2f;
                    }
                    else if (BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(data.Settlement, "workforce",
                                 (int)BKWorkforcePolicy.WorkforcePolicy.Land_Expansion))
                    {
                        toSubtract += LandExpansionWorkforce / 2;
                    }
                    else if (BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(data.Settlement, "workforce",
                                 (int)BKWorkforcePolicy.WorkforcePolicy.Construction))
                    {
                        serfs -= SerfsConstructionForce;
                    }
                }

                return Math.Max((int)(serfs - toSubtract), 0);
            }
        }

        public int AvailableTenantsWorkForce
        {
            get
            {
                var tenants = data.GetTypeCount(PopulationManager.PopType.Tenants) * (data.Settlement.IsVillage ? 0.85f : 0.5f);
                int toSubtract = 0;

                if (!data.Settlement.IsVillage)
                {
                    if (BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(data.Settlement, "workforce",
                            (int)BKWorkforcePolicy.WorkforcePolicy.Martial_Law))
                    {
                        var militia = data.Settlement.Town.Militia / 2;
                        tenants -= militia / 2f;
                    }
                    else if (BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(data.Settlement, "workforce",
                                 (int)BKWorkforcePolicy.WorkforcePolicy.Land_Expansion))
                    {
                        toSubtract += LandExpansionWorkforce / 2;
                    }
                    else if (BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(data.Settlement, "workforce",
                                 (int)BKWorkforcePolicy.WorkforcePolicy.Construction))
                    {
                        tenants -= TenantsConstructionForce;
                    }
                }

                return Math.Max((int)(tenants - toSubtract), 0);
            }
        }

        public int AvailableSlavesWorkForce
        {
            get
            {
                float slaves = data.GetTypeCount(PopulationManager.PopType.Slaves);
                int toSubtract = 0;

                var town = data.Settlement.Town;
                if (town != null && town.BuildingsInProgress.Count > 0)
                {
                    slaves -= slaves * data.EconomicData.StateSlaves * 0.5f;
                }

                if (!data.Settlement.IsVillage)
                {
                    if (BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(data.Settlement, "workforce",
                                (int)BKWorkforcePolicy.WorkforcePolicy.Land_Expansion))
                    {
                        toSubtract += LandExpansionWorkforce / 2;
                    }
                    else if (BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(data.Settlement, "workforce",
                                 (int)BKWorkforcePolicy.WorkforcePolicy.Construction))
                    {
                        slaves -= SlavesConstructionForce;
                    }
                }

                return Math.Max((int)(slaves - toSubtract), 0);
            }
        }

        public int SerfsConstructionForce
        {
            get
            {
                var serfs = data.GetTypeCount(PopulationManager.PopType.Serfs) * (data.Settlement.IsVillage ? 0.85f : 0.5f);
                return Math.Max((int)(serfs * 0.15), 0);
            }
        }

        public int TenantsConstructionForce
        {
            get
            {
                var tenants = data.GetTypeCount(PopulationManager.PopType.Tenants) * (data.Settlement.IsVillage ? 0.85f : 0.5f);
                return Math.Max((int)(tenants * 0.15), 0);
            }
        }

        public int SlavesConstructionForce => Math.Max((int)(data.EconomicData.StateSlaves * 0.5), 0);

        public int AvailableWorkForce => AvailableTenantsWorkForce + AvailableSlavesWorkForce + AvailableSerfsWorkForce;

        public int LandExpansionWorkforce
        {
            get
            {
                float serfs = data.GetTypeCount(PopulationManager.PopType.Serfs) * (data.Settlement.IsVillage ? 0.85f : 0.5f);
                float slaves = data.GetTypeCount(PopulationManager.PopType.Slaves);
                float tenants = data.GetTypeCount(PopulationManager.PopType.Tenants);

                return Math.Max((int)((serfs + slaves + tenants) * 0.2f), 0);
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

        public int WorkforceExcess
        {
            get
            {
                int available = AvailableWorkForce;
                int farms = (int)(farmland / GetRequiredLabor("farmland"));
                int pasture = (int)(this.pasture / GetRequiredLabor("pasture"));
                return available - (farms + pasture);
            }
        }

        public float Acreage => farmland + pasture + woodland;
        public float Farmland => farmland;
        public float Pastureland => pasture;
        public float Woodland => woodland;
        public float Fertility => fertility;
        public float Difficulty => terrainDifficulty;

        public float DifficultyFinal => 15f * terrainDifficulty;

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
                "farmland" => 0.017f,
                "pasture" => 0.0065f,
                _ => 0.0012f
            };

            Hero owner = null;
            if (data.Settlement.OwnerClan != null)
            {
                owner = data.Settlement.Owner;
            }

            if (data.ReligionData != null)
            {
                var religion = data.ReligionData.DominantReligion;
                if (religion != null && religion.HasDoctrine(DefaultDoctrines.Instance.Pastoralism))
                {
                    if (type == "farmland" || type == "pasture")
                    {
                        result *= 1.08f;
                    }
                }

                if (owner != null)
                {
                    var ownerReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(owner);
                    if (ownerReligion == religion)
                    {
                        if (BannerKingsConfig.Instance.ReligionsManager.HasBlessing(owner, DefaultDivinities.Instance.Oca, religion))
                        {
                            result *= 1.1f;
                        }
                    }
                }
            }

            if (owner != null)
            {
                var data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(owner);
                if (data.HasPerk(BKPerks.Instance.CivilCultivator))
                {
                    result *= 1.05f;
                }
            }

            var innovations = BannerKingsConfig.Instance.InnovationsManager.GetInnovationData(data.Settlement.Culture);
            if (innovations != null)
            {
                if (type == "farmland")
                {
                    if (innovations.HasFinishedInnovation(DefaultInnovations.Instance.HeavyPlough))
                    {
                        result *= 1.12f;
                    }

                    if (innovations.HasFinishedInnovation(DefaultInnovations.Instance.HorseCollar))
                    {
                        result *= 1.25f;
                    }

                    if (innovations.HasFinishedInnovation(DefaultInnovations.Instance.HeavyPlough))
                    {
                        result *= 1.12f;
                    }

                    if (innovations.HasFinishedInnovation(DefaultInnovations.Instance.ThreeFieldsSystem))
                    {
                        result *= 1.4f;
                    }
                }
            }

            return result;
        }

        public float GetAcreClassOutput(string type, PopType populationClass)
        {
            float result = GetAcreOutput(type);

            if (data.TitleData != null && data.TitleData.Title != null)
            {
                var title = data.TitleData.Title;
                if (populationClass == PopType.Serfs)
                {
                    if (title.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.SerfsAgricultureDuties))
                    {
                        result *= 1.1f;
                    }
                    else if (title.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.SerfsLaxDuties))
                    {
                        result *= 0.95f;
                    }
                }
                else if (populationClass == PopType.Slaves)
                {
                    if (title.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.SlavesAgricultureDuties))
                    {
                        result *= 1.1f;
                    }
                }
            }

            return result;
        }

        internal override void Update(PopulationData data)
        {
            if (this.data.Settlement.IsVillage)
            {
                var type = data.VillageData.CurrentDefault.BuildingType;
                if (type.StringId != DefaultVillageBuildings.Instance.DailyProduction.StringId)
                {
                    var progress = BannerKingsConfig.Instance.ConstructionModel.CalculateLandExpansion(data, LandExpansionWorkforce).ResultNumber;
                    if (type.StringId == DefaultVillageBuildings.Instance.DailyFarm.StringId)
                    {
                        this.farmland += progress;
                    }
                    else if (type.StringId == DefaultVillageBuildings.Instance.DailyPasture.StringId)
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
                var progress = BannerKingsConfig.Instance.ConstructionModel.CalculateLandExpansion(data, LandExpansionWorkforce).ResultNumber;

                if (progress > 0f)
                {
                    var list = new List<(int, float)>
                    {
                        new(0, composition[0]),
                        new(1, composition[1]),
                        new(2, composition[2])
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

            if (IsExcessLaborExpandingAcreage)
            {
                var list = new List<(int, float)>
                {
                    new(0, composition[0]),
                    new(1, composition[1]),
                    new(2, composition[2])
                };
                var choosen = MBRandom.ChooseWeighted(list);
                var progress = BannerKingsConfig.Instance.ConstructionModel.CalculateLandExpansion(data, WorkforceExcess).ResultNumber;

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