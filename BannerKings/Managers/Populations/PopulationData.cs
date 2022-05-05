using System;
using System.Linq;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.SaveSystem;
using static BannerKings.Managers.PopulationManager;
using BannerKings.Models;
using BannerKings.Models.Populations;
using TaleWorlds.ObjectSystem;
using BannerKings.Managers.Institutions;
using BannerKings.Managers.Populations.Villages;
using BannerKings.Managers.Populations;
using static BannerKings.Managers.Policies.BKWorkforcePolicy;
using BannerKings.Managers.Titles;
using BannerKings.Managers;

namespace BannerKings.Populations
{
    public class PopulationData : BannerKingsData
    {
        [SaveableProperty(1)]
        private List<PopulationClass> classes { get; set; }

        [SaveableProperty(2)]
        private float stability { get; set; }

        [SaveableProperty(3)]
        private Settlement settlement { get; set; }

        [SaveableProperty(4)]
        private CultureData cultureData { get; set; }

        [SaveableProperty(5)]
        private VillageData villageData { get; set; }

        [SaveableProperty(6)]
        private LandData landData { get; set; }

        [SaveableProperty(7)]
        private MilitaryData militaryData { get; set; }

        [SaveableProperty(8)]
        private EconomicData economicData { get; set; }

        [SaveableProperty(9)]
        private TournamentData tournamentData { get; set; }

        [SaveableProperty(10)]
        private TitleData titleData { get; set; }

        public PopulationData(List<PopulationClass> classes, Settlement settlement, float assimilation, List<CultureDataClass> cultures = null, Guild guild = null)
        {
            this.classes = classes;
            this.stability = 0.5f;
            this.settlement = settlement;
            this.economicData = new EconomicData(settlement, guild);

            if (cultures == null)
            {
                CultureDataClass cultureData = new CultureDataClass(settlement.Culture, 1f, 1f);
                cultures = new List<CultureDataClass>();
                cultures.Add(cultureData);
            }
            this.cultureData = new CultureData(settlement.Owner, cultures);
            float total = TotalPop;
            float nobles = classes.First(x => x.type == PopType.Nobles).count;
            this.militaryData = new MilitaryData(settlement, (int)(total * 0.04f), (int)(nobles * 0.08f));
            this.landData = new LandData(this);

            if (settlement.Village != null)
                this.villageData = new VillageData(settlement.Village);
        }

        public CultureData CultureData => this.cultureData;
        public MilitaryData MilitaryData => this.militaryData;
        public LandData LandData => this.landData;
        public EconomicData EconomicData => this.economicData;
        public TournamentData TournamentData
        {
            get => this.tournamentData;
            set => this.tournamentData = value;
        }

        public TitleData TitleData
        {
            get
            {
                return titleData;
            }
        }

        public VillageData VillageData => this.villageData;

        public ExplainedNumber Foreigner => new BKForeignerModel().CalculateEffect(this.settlement);

        public int TotalPop {
            get
            {
                int total = 0;
                classes.ForEach(popClass => total += popClass.count);
                return total;
            }
        }

        public Settlement Settlement => this.settlement;
        public ExplainedNumber Growth
        {
            get
            {
                BKGrowthModel model = (BKGrowthModel)BannerKingsConfig.Instance.Models.First(x => x.GetType() == typeof(BKGrowthModel));
                return model.CalculateEffect(this.settlement, this);
            }
        }

        public float Stability
        {
            get => this.stability;
            set
            {
                if (value != stability)
                    stability = MBMath.ClampFloat(value, 0f, 1f);
            }
        }

        public List<PopulationClass> Classes
        {
            get => classes;
            set
            {
                if (value != classes)
                    classes = value;
            }
        }

        public void UpdatePopulation(Settlement settlement, int pops, PopType target)
        {
            if (target == PopType.None)
            {
                if (settlement.Owner == Hero.MainHero)
                    InformationManager.DisplayMessage(new InformationMessage());
                bool divisibleNegative = ((float)pops * -1f) > 20;
                if (pops > 20 || divisibleNegative)
                {
                    int fractions = (int)((float)pops / (divisibleNegative ? -20f : 20f));
                    int reminder = pops % 20;
                    for (int i = 0; i < fractions; i++)
                        this.SelectAndUpdatePop(settlement, divisibleNegative ? -20 : 20);
                    
                    this.SelectAndUpdatePop(settlement, divisibleNegative ? -reminder : reminder);
                }
                else this.SelectAndUpdatePop(settlement, pops);
            }
            else this.UpdatePopType(target, pops);

            this.BalanceClasses(settlement);
        }

        private void BalanceClasses(Settlement settlement)
        {
            Dictionary<PopType, float[]> dic = PopulationManager.GetDesiredPopTypes(settlement);
            Dictionary<PopType, float> currentDic = new Dictionary<PopType, float>()
            {
                { PopType.Nobles, this.GetCurrentTypeFraction(PopType.Nobles) },
                { PopType.Craftsmen, this.GetCurrentTypeFraction(PopType.Craftsmen) },
                { PopType.Serfs, this.GetCurrentTypeFraction(PopType.Serfs) },
                { PopType.Slaves, this.GetCurrentTypeFraction(PopType.Slaves) }
            };
            
            if (currentDic[PopType.Slaves] > dic[PopType.Slaves][1])
            {
                int random = MBMath.ClampInt(MBRandom.RandomInt(0, 25), 0, this.GetTypeCount(PopType.Slaves));
                this.UpdatePopType(PopType.Slaves, -random);
                this.UpdatePopType(PopType.Serfs, random);
                if (settlement.Town != null)
                    settlement.Town.Security -= (float)random * 0.01f;
            }

            if (currentDic[PopType.Serfs] > dic[PopType.Serfs][1])
            {
                int random = MBMath.ClampInt(MBRandom.RandomInt(0, 25), 0, this.GetTypeCount(PopType.Serfs));
                this.UpdatePopType(PopType.Serfs, -random);
                this.UpdatePopType(PopType.Craftsmen, random);
            }

            if (currentDic[PopType.Craftsmen] > dic[PopType.Craftsmen][1])
            {
                int random = MBMath.ClampInt(MBRandom.RandomInt(0, 25), 0, this.GetTypeCount(PopType.Craftsmen));
                this.UpdatePopType(PopType.Craftsmen, -random);
                this.UpdatePopType(PopType.Nobles, random);
            }
        }

        private void SelectAndUpdatePop(Settlement settlement, int pops)
        {
            if (pops != 0)
            {
                Dictionary<PopType, float[]> desiredTypes = GetDesiredPopTypes(settlement);
                List<ValueTuple<PopType, float>> typesList = new List<ValueTuple<PopType, float>>();

                
                if (pops < 0)
                {
                    PopulationClass slaveClass = classes.FirstOrDefault(x => x.type == PopType.Slaves);
                    if (slaveClass != null && slaveClass.count > 0)
                    {
                        UpdatePopType(PopType.Slaves, pops);
                        return;
                    }     
                }

                classes.ForEach(popClass =>
                {
                    PopType type = popClass.type;
                    if (pops < 0 && popClass.count >= pops)
                    {
                        bool hasExcess = GetCurrentTypeFraction(type) > desiredTypes[type][1];
                        typesList.Add(new ValueTuple<PopType, float>(popClass.type, (float)popClass.type * 5f + desiredTypes[type][0] * (hasExcess ? 2f : 1f)));
                    }
                    else if (pops > 0)
                    {
                        bool isLacking = GetCurrentTypeFraction(type) < desiredTypes[type][0];
                        typesList.Add(new ValueTuple<PopType, float>(popClass.type, desiredTypes[type][0] * (isLacking ? 2f : 1f)));
                    }
                });

                PopType targetType = MBRandom.ChooseWeighted(typesList);
                UpdatePopType(targetType, pops);
            }
        }

        public void UpdatePopType(PopType type, int count, bool stateSlaves = false)
        {
            if (type != PopType.None)
            {
                PopulationClass pops = classes.Find(popClass => popClass.type == type);
                if (pops == null)
                    pops = new PopulationClass(type, 0);

                if (type == PopType.Slaves)
                {
                    int total = pops.count + count;
                    float currentState = (float)pops.count * this.economicData.StateSlaves;
                    if (stateSlaves)
                        currentState += count;
                    this.economicData.StateSlaves = currentState / (float)total;
                }

                pops.count += count;
                if (pops.count < 0) pops.count = 0;
            }
        }

        public int GetTypeCount(PopType type)
        {
            int i = 0;
            PopulationClass targetClass = classes.Find(popClass => popClass.type == type);
            if (targetClass != null) i = targetClass.count;
            return MBMath.ClampInt(i, 0, 50000);
        }

        public float GetCurrentTypeFraction(PopType type) => (float)GetTypeCount(type) / (float)TotalPop;

        internal override void Update(PopulationData data)
        {
            BKGrowthModel model = (BKGrowthModel)BannerKingsConfig.Instance.Models.First(x => x.GetType() == typeof(BKGrowthModel));
            int growthFactor = (int)model.CalculateEffect(this.settlement, this).ResultNumber;
            this.UpdatePopulation(settlement, growthFactor, PopType.None);
            this.Stability += BannerKingsConfig.Instance.Models.First(x => x.GetType() == typeof(BKStabilityModel)).CalculateEffect(settlement).ResultNumber;
            economicData.Update(this);
            cultureData.Update(this);
            militaryData.Update(this);
            landData.Update(this);
            if (villageData != null) villageData.Update(this);
            if (tournamentData != null) tournamentData.Update(this);
            if (titleData == null) titleData = new TitleData(BannerKingsConfig.Instance.TitleManager.GetTitle(this.settlement));
            titleData.Update(this);
        }
    }

    public class PopulationClass
    {
        [SaveableProperty(1)]
        public PopType type { get; set; }

        [SaveableProperty(2)]
        public int count { get; set; }

        public PopulationClass(PopType type, int count)
        {
            this.type = type;
            this.count = count;
        }
    }

    public class CultureData : BannerKingsData
    {
        [SaveableProperty(1)]
        private List<CultureDataClass> cultures { get; set; }

        [SaveableProperty(2)]
        private Hero settlementOwner { get; set; }

        public CultureData(Hero settlementOwner, List<CultureDataClass> cultures)
        {
            this.settlementOwner = settlementOwner;
            this.cultures = cultures;
        }

        public List<CultureDataClass> Cultures => this.cultures;

        public CultureObject DominantCulture
        {
            get
            {
                CultureObject culture = null;
                CultureObject ownerCulture = settlementOwner.Culture;
                float ownerShare = 0f;
                float share = 0f;
                foreach (CultureDataClass data in this.cultures)
                {
                    if (data.Assimilation >= share && data.Culture.MilitiaSpearman != null)
                    {
                        culture = data.Culture;
                        share = data.Assimilation;
                        if (data.Culture == ownerCulture)
                            ownerShare = data.Assimilation;
                    }
                }

                return share > ownerShare ? culture : ownerCulture;
            }
        }

        public bool IsCulturePresent(CultureObject culture)
        {
            CultureDataClass data = this.cultures.FirstOrDefault(x => x.Culture == culture);
            return data != null;
        }

        public Hero SettlementOwner
        {
            get => this.settlementOwner;
            set
            {
                this.settlementOwner = value;
                if (!this.IsCulturePresent(settlementOwner.Culture))
                {
                    if (this.settlementOwner.Culture == this.DominantCulture)
                        this.AddCulture(settlementOwner.Culture, 1f, 1f);
                    else this.AddCulture(settlementOwner.Culture, 0f);
                }
            }
        }

        public void AddCulture(CultureObject culture, float acceptance)
        {
            CultureDataClass dataClass = null;
            foreach (CultureDataClass data in this.cultures)
                if (data.Culture == culture)
                {
                    dataClass = data;
                    break;
                }

            if (dataClass == null) this.cultures.Add(new CultureDataClass(culture, 0f, acceptance));
            else dataClass.Acceptance = acceptance;
        }

        public void AddCulture(CultureObject culture, float acceptance, float assim)
        {
            CultureDataClass dataClass = null;
            foreach (CultureDataClass data in this.cultures)
                if (data.Culture == culture)
                {
                    dataClass = data;
                    break;
                }

            if (dataClass == null) this.cultures.Add(new CultureDataClass(culture, assim, acceptance));
            else
            {
                dataClass.Acceptance = acceptance;
                dataClass.Assimilation = assim;
            }
        }

        public float GetAssimilation(CultureObject culture)
        {
            CultureDataClass data = this.cultures.FirstOrDefault(x => x.Culture == culture);
            return data != null ? data.Assimilation : 0f;
        }

        public float GetAcceptance(CultureObject culture)
        {
            CultureDataClass data = this.cultures.FirstOrDefault(x => x.Culture == culture);
            return data != null ? data.Acceptance : 0f;
        }

        internal override void Update(PopulationData data)
        {
            this.SettlementOwner = data.Settlement.Owner;
            BKCultureAssimilationModel assimModel = (BKCultureAssimilationModel)BannerKingsConfig.Instance.Models.First(x => x.GetType() == typeof(BKCultureAssimilationModel));
            BKCultureAcceptanceModel accModel = (BKCultureAcceptanceModel)BannerKingsConfig.Instance.Models.First(x => x.GetType() == typeof(BKCultureAcceptanceModel));
            HashSet<CultureDataClass> toDelete = new HashSet<CultureDataClass>();

            float foreignerShare = 0f;
            foreach (CultureDataClass cultureData in this.cultures)
            {
                cultureData.Acceptance += accModel.CalculateEffect(data.Settlement, cultureData).ResultNumber;
                cultureData.Assimilation += assimModel.CalculateEffect(data.Settlement, cultureData).ResultNumber;
                if (cultureData.Culture != settlementOwner.Culture && cultureData.Assimilation <= 0.01)
                    toDelete.Add(cultureData);

                if (cultureData.Culture != settlementOwner.Culture && cultureData.Culture != data.Settlement.Culture)
                    foreignerShare += cultureData.Assimilation;
            }

            if (toDelete.Count > 0)
                foreach (CultureDataClass cultureData in toDelete)
                this.cultures.Remove(cultureData);

            float foreignerTarget = data.Foreigner.ResultNumber;
            if (foreignerShare < foreignerTarget)
            {
                float random = MBRandom.RandomFloatRanged(foreignerTarget - foreignerShare);
                IEnumerable<CultureObject> presentCultures = from cultureClass in this.cultures select cultureClass.Culture;
                CultureObject randomForeign = MBObjectManager.Instance.GetObjectTypeList<CultureObject>()
                    .GetRandomElementWithPredicate(x => x != settlementOwner.Culture && x != data.Settlement.Culture && !x.IsBandit
                    && !presentCultures.Contains(x));
                if (randomForeign != null)
                {
                    this.cultures.Add(new CultureDataClass(randomForeign, random, random));
                    foreach (CultureDataClass cultureData in this.cultures)
                        if (cultureData.Culture == this.DominantCulture)
                        {
                            cultureData.Assimilation -= random;
                            break;
                        }
                }     
            }

            CultureObject dominant = this.DominantCulture;
            if (dominant.BasicTroop != null && dominant.MilitiaSpearman != null)
            {
                data.Settlement.Culture = dominant;
                if (data.Settlement.Notables != null && data.Settlement.Notables.Count > 0)
                    foreach (Hero notable in data.Settlement.Notables)
                        notable.Culture = dominant;
            }
        }
    }

    public class CultureDataClass
    {
        [SaveableProperty(1)]
        private CultureObject culture { get; set; }

        [SaveableProperty(2)]
        private float assimilation { get; set; }

        [SaveableProperty(3)]
        private float acceptance { get; set; }

        public CultureDataClass(CultureObject culture, float assimilation, float acceptance)
        {
            this.culture = culture;
            this.assimilation = assimilation;
            this.acceptance = acceptance;
        }

        internal float Assimilation
        {
            get => assimilation;
            set
            {
                assimilation = MBMath.ClampFloat(value, 0f, 1f);
            }
        }

        internal float Acceptance
        {
            get => acceptance;
            set
            {
                acceptance = MBMath.ClampFloat(value, 0f, 1f);
            }
        }

        internal CultureObject Culture => culture;
    }

    public class VillageData : BannerKingsData
    {
        [SaveableProperty(1)]
        Village village { get; set; }

        [SaveableProperty(2)]
        List<VillageBuilding> buildings { get; set; }

        [SaveableProperty(3)]
        VillageBuilding current { get; set; }

        [SaveableProperty(4)]
        VillageBuilding currentDefault { get; set; }

        [SaveableProperty(5)]
        Queue<Building> inProgress { get; set; }

        public VillageData(Village village)
        {
            this.village = village;
            this.buildings = new List<VillageBuilding>();
            foreach (BuildingType type in DefaultVillageBuildings.VillageBuildings(village))
                this.buildings.Add(new VillageBuilding(type, village.MarketTown, village));
            this.inProgress = new Queue<Building>();
        }

        public void StartRandomProject()
        {
            if (this.inProgress.IsEmpty())
                this.inProgress.Enqueue(this.buildings.GetRandomElementWithPredicate(x => x.BuildingType.BuildingLocation != BuildingLocation.Daily));
        }

        public int GetBuildingLevel(BuildingType type)
        {
            Building building = this.buildings.FirstOrDefault(x => x.BuildingType == type);
            if (building != null) return building.CurrentLevel;
            return 0;
        }

        public void ReInitializeBuildings()
        {
            List<VillageBuilding> toAdd = new List<VillageBuilding>();
            foreach (VillageBuilding b in this.buildings)
            {
                if (b.Explanation == null)
                {
                    string id = b.BuildingType.StringId;
                    BuildingType type = DefaultVillageBuildings.Instance.All()
                        .FirstOrDefault(x => x.StringId == id);
                    if (type != null)
                        toAdd.Add(new VillageBuilding(type, this.Village.MarketTown, this.Village,
                            b.BuildingProgress, b.CurrentLevel));
                }  
            }

            if (toAdd.Count > 0)
            {
                this.buildings.Clear();
                foreach (VillageBuilding b in toAdd)
                    this.buildings.Add(b);
            }

            List<VillageBuilding> toAddQueue = new List<VillageBuilding>();
            foreach (VillageBuilding b in this.inProgress)
                if (b.Explanation == null)
                {
                    string id = b.BuildingType.StringId;
                    BuildingType type = DefaultVillageBuildings.Instance.All()
                        .FirstOrDefault(x => x.StringId == id);
                    if (type != null)
                        toAddQueue.Add(new VillageBuilding(type, this.Village.MarketTown, this.Village,
                            b.BuildingProgress, b.CurrentLevel));
                }

            if (toAddQueue.Count > 0)
            {
                this.inProgress.Clear();
                foreach (VillageBuilding b in toAddQueue)
                    this.inProgress.Enqueue(b);
            }
        }

        public Village Village => this.village;
        public List<VillageBuilding> Buildings
        {
            get => this.buildings;
        }

        public VillageBuilding CurrentBuilding
        {
            get
            {
                VillageBuilding building = null;

                if (this.inProgress != null && !this.inProgress.IsEmpty())
                    building = (VillageBuilding?)this.inProgress.Peek();

                return building != null ? building : this.CurrentDefault;
            }
        }

        public VillageBuilding CurrentDefault
        {
            get
            {
                VillageBuilding building = this.buildings.FirstOrDefault(x => x.IsCurrentlyDefault);
                if (building == null)
                {
                    VillageBuilding dailyProd = this.buildings.FirstOrDefault(x => x.BuildingType.StringId == "bannerkings_daily_production");
                    dailyProd.IsCurrentlyDefault = true;
                    building = dailyProd;
                }

                return building;
            }
        }

        public Queue<Building> BuildingsInProgress
        {
            get => this.inProgress;
            set => this.inProgress = value;
        }

        public bool IsCurrentlyBuilding => this.BuildingsInProgress.Count() > 0;
        public float Construction => new BKConstructionModel().CalculateVillageConstruction(this.village.Settlement).ResultNumber;

        internal override void Update(PopulationData data)
        {
            VillageBuilding current = this.CurrentBuilding;
            if (current != null && this.BuildingsInProgress.Count() > 0)
                if (this.BuildingsInProgress.Peek().BuildingType.StringId == current.BuildingType.StringId)
                {
                    current.BuildingProgress += this.Construction;
                    if ((float)current.GetConstructionCost() <= current.BuildingProgress)
                    {
                        if (current.CurrentLevel < 3)
                            current.LevelUp();
                    
                        if (current.CurrentLevel == 3)
                            current.BuildingProgress = (float)current.GetConstructionCost();
                    
                        this.BuildingsInProgress.Dequeue();
                    }
                } 
        }

    }


    public class LandData : BannerKingsData
    {
        [SaveableProperty(1)]
        private PopulationData data { get; set; }

        [SaveableProperty(2)]
        private float farmland { get; set; }

        [SaveableProperty(3)]
        private float pasture { get; set; }

        [SaveableProperty(4)]
        private float woodland { get; set; }

        [SaveableProperty(5)]
        private float fertility { get; set; }

        [SaveableProperty(6)]
        private float terrainDifficulty  { get; set; }

        [SaveableProperty(7)]
        private float[] composition { get; set; }

        public LandData(PopulationData data)
        {
            this.data = data;
            this.composition = new float[3];
            this.Init(data.TotalPop);
        }

        private void Init(int totalPops)
        {
            float farmRatio = 0f;
            float pastureRatio = 0f;
            float woodRatio = 0f;
            if (this.Terrain == TerrainType.Desert)
            {
                this.fertility = 0.5f;
                this.terrainDifficulty = 1.4f;
                farmRatio = 0.9f;
                pastureRatio = 0.08f;
                woodRatio = 0.02f;
            }
            else if (this.Terrain == TerrainType.Steppe)
            {
                this.fertility = 0.75f;
                this.terrainDifficulty = 1f;
                farmRatio = 0.45f;
                pastureRatio = 0.5f;
                woodRatio = 0.05f;
            }
            else if (this.Terrain == TerrainType.Mountain)
            {
                this.fertility = 0.7f;
                this.terrainDifficulty = 2f;
                farmRatio = 0.5f;
                pastureRatio = 0.35f;
                woodRatio = 0.15f;
            }
            else if (this.Terrain == TerrainType.Canyon)
            {
                this.fertility = 0.5f;
                this.terrainDifficulty = 2f;
                farmRatio = 0.9f;
                pastureRatio = 0.08f;
                woodRatio = 0.02f;
            }
            else if (this.Terrain == TerrainType.Forest)
            {
                this.fertility = 0.5f;
                this.terrainDifficulty = 2f;
                farmRatio = 0.45f;
                pastureRatio = 0.15f;
                woodRatio = 0.40f;
            } else
            {
                this.fertility = 1f;
                this.terrainDifficulty = 1f;
                farmRatio = 0.7f;
                pastureRatio = 0.22f;
                woodRatio = 0.08f;
            }
            this.composition[0] = farmRatio;
            this.composition[1] = pastureRatio;
            this.composition[2] = woodRatio;
            float acres = this.data.Settlement.IsVillage ? (float)totalPops * MBRandom.RandomFloatRanged(3f, 3.5f) : (float)totalPops * MBRandom.RandomFloatRanged(2.5f, 3.0f);
            this.farmland = acres * farmRatio;
            this.pasture = acres * pastureRatio;
            this.woodland = acres * woodRatio;
        }

        public TerrainType Terrain => Campaign.Current.MapSceneWrapper.GetTerrainTypeAtPosition(data.Settlement.Position2D);

        public int AvailableWorkForce
        {
            get
            {
                float serfs = this.data.GetTypeCount(PopType.Serfs) * 0.5f;
                float slaves = this.data.GetTypeCount(PopType.Slaves);

                Town town = this.data.Settlement.Town;
                if (town != null && town.BuildingsInProgress.Count > 0)
                    slaves -= slaves * this.data.EconomicData.StateSlaves * 0.5f;

                if (!this.data.Settlement.IsVillage)
                {
                    if (BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(this.data.Settlement, "workforce", (int)WorkforcePolicy.Martial_Law))
                    {
                        float militia = this.data.Settlement.Town.Militia / 2;
                        serfs -= militia / 2f;
                    } else if (BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(this.data.Settlement, "workforce", (int)WorkforcePolicy.Land_Expansion))
                    {
                        serfs *= 0.8f;
                        slaves *= 0.8f;
                    }
                    else if (BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(this.data.Settlement, "workforce", (int)WorkforcePolicy.Construction))
                    {
                        serfs *= 0.85f;
                        slaves -= slaves * this.data.EconomicData.StateSlaves * 0.5f;
                    }
                }
                return Math.Max((int)(serfs + slaves), 0);
            }
        }

        public float WorkforceSaturation
        {
            get
            {
                float available = this.AvailableWorkForce;
                float farms = this.farmland / 4f;
                float pasture = this.pasture / 8f;
                return available / (farms + pasture);
            }
        }

        public float Acreage => this.farmland + this.pasture + this.woodland;
        public float Farmland => this.farmland;
        public float Pastureland => this.pasture;
        public float Woodland => this.woodland;
        public float Fertility => this.fertility;
        public float Difficulty => this.terrainDifficulty;

        internal override void Update(PopulationData data)
        {
            if (this.data.Settlement.IsVillage)
            {
                VillageData villageData = data.VillageData;
                float construction = this.data.VillageData.Construction;
                float progress = 15f / construction;
                BuildingType type = villageData.CurrentDefault.BuildingType;
                if (type != DefaultVillageBuildings.Instance.DailyProduction)
                {
                    if (type == DefaultVillageBuildings.Instance.DailyFarm)
                        this.farmland += progress;
                    else if (type == DefaultVillageBuildings.Instance.DailyPasture)
                        this.pasture += progress;
                    else this.woodland += progress;
                }
            } else if (BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(this.data.Settlement, "workforce", (int)WorkforcePolicy.Land_Expansion))
            {
                float laborers = (float)this.AvailableWorkForce * 0.2f;
                float construction = laborers * 0.010f;
                float progress = 15f / construction;

                if (progress > 0f)
                {
                    List<(int, float)> list = new List<(int, float)>();
                    list.Add(new(0, this.composition[0]));
                    list.Add(new(1, this.composition[1]));
                    list.Add(new(2, this.composition[2]));
                    int choosen = MBRandom.ChooseWeighted(list);

                    if (choosen == 0)
                        this.farmland += progress;
                    else if (choosen == 1)
                        this.pasture += progress;
                    else this.woodland += progress;
                }
            }


            if (this.WorkforceSaturation > 1f)
            {
                List<(int, float)> list = new List<(int, float)>();
                list.Add(new(0, this.composition[0]));
                list.Add(new(1, this.composition[1]));
                list.Add(new(2, this.composition[2]));
                int choosen = MBRandom.ChooseWeighted(list);

                float construction = this.data.Settlement.IsVillage ? this.data.VillageData.Construction : 
                    new BKConstructionModel().CalculateDailyConstructionPower(this.data.Settlement.Town).ResultNumber;
                construction *= 0.8f;
                float progress = 15f / construction;

                if (choosen == 0)
                    this.farmland += progress;
                else if (choosen == 1)
                    this.pasture += progress;
                else this.woodland += progress;
            }

            float farmland = this.farmland;
            float pastureland = this.pasture;
            float woodland = this.woodland;

            this.farmland = MBMath.ClampFloat(farmland, 0f, 100000f);
            this.pasture = MBMath.ClampFloat(pastureland, 0f, 50000f);
            this.woodland = MBMath.ClampFloat(woodland, 0f, 50000f);
        }
    }

    public class TournamentData : BannerKingsData
    {
        private Town town;

        [SaveableProperty(1)]
        private ItemRoster roster { get; set; }

        [SaveableProperty(2)]
        private ItemObject prize { get; set; }

        [SaveableProperty(3)]
        private bool active { get; set; }

        public TournamentData(Town town)
        {
            this.town = town;
            this.roster = new ItemRoster();
            this.active = true;
        }

        public bool Active
        {
            get => this.active;
            set
            {
                this.active = value;
            }
        }

        public ItemRoster Roster => this.roster;
        public ItemObject Prize
        {
            get
            {
                if (this.prize == null)
                {
                    List<ItemObject> items = new List<ItemObject>();
                    foreach (ItemRosterElement element in this.roster)
                    {
                        EquipmentElement equipment = element.EquipmentElement;
                        ItemObject item = equipment.Item;
                        if (item != null)
                            if (item.IsMountable || item.HasWeaponComponent || item.HasArmorComponent)
                                items.Add(item);
                    }

                    if (items.Count > 0)
                    {
                        items.Sort((x, y) => x.Value.CompareTo(y.Value));
                        this.prize = items[0];
                    }
                }
                
                return this.prize;
            }
        }

        internal override void Update(PopulationData data)
        {
            if (!data.Settlement.Town.HasTournament)
                this.active = false;
        }
    }
}
