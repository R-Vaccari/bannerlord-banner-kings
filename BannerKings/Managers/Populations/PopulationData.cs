using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Populations;
using BannerKings.Managers.Populations.Villages;
using BannerKings.Managers.Titles;
using BannerKings.Models;
using BannerKings.Models.Populations;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;
using static BannerKings.Managers.PopulationManager;
using static BannerKings.Managers.Policies.BKWorkforcePolicy;
using BannerKings.Managers.Institutions.Guilds;

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

        [SaveableProperty(11)]
        private float autonomy { get; set; } = 0f;

        [SaveableProperty(12)]
        private ReligionData religionData { get; set; }

        public PopulationData(List<PopulationClass> classes, Settlement settlement, float assimilation, List<CultureDataClass> cultures = null, Guild guild = null)
        {
            this.classes = classes;
            stability = 0.5f;
            this.settlement = settlement;
            economicData = new EconomicData(settlement, guild);

            if (cultures == null)
            {
                CultureDataClass cultureData = new CultureDataClass(settlement.Culture, 1f, 1f);
                cultures = new List<CultureDataClass>();
                cultures.Add(cultureData);
            }
            this.cultureData = new CultureData(settlement.Owner, cultures);
            float total = TotalPop;
            float nobles = classes.First(x => x.type == PopType.Nobles).count;
            militaryData = new MilitaryData(settlement, (int)(total * 0.04f), (int)(nobles * 0.08f));
            landData = new LandData(this);

            if (settlement.Village != null)
                villageData = new VillageData(settlement.Village);
        }

        public CultureData CultureData => cultureData;
        public MilitaryData MilitaryData => militaryData;
        public LandData LandData => landData;
        public EconomicData EconomicData => economicData;
        public TournamentData TournamentData
        {
            get => tournamentData;
            set => tournamentData = value;
        }

        public TitleData TitleData => titleData;
        public ReligionData ReligionData => religionData;
        public VillageData VillageData => villageData;

        public ExplainedNumber Foreigner => new BKForeignerModel().CalculateEffect(settlement);

        public int TotalPop {
            get
            {
                int total = 0;
                classes.ForEach(popClass => total += popClass.count);
                return total;
            }
        }

        public Settlement Settlement => settlement;
        public ExplainedNumber Growth
        {
            get
            {
                BKGrowthModel model = (BKGrowthModel)BannerKingsConfig.Instance.Models.First(x => x.GetType() == typeof(BKGrowthModel));
                return model.CalculateEffect(settlement, this);
            }
        }

        public float Stability
        {
            get => stability;
            set
            {
                if (value != stability)
                    stability = MBMath.ClampFloat(value, 0f, 1f);
            }
        }

        public float Autonomy
        {
            get => autonomy;
            set
            {
                if (value != autonomy)
                    autonomy = MBMath.ClampFloat(value, 0f, 1f);
            }
        }

        public ExplainedNumber NotableSupport => BannerKingsConfig.Instance.StabilityModel.CalculateNotableSupport(settlement);

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
                bool divisibleNegative = (pops * -1f) > 20;
                if (pops > 20 || divisibleNegative)
                {
                    int fractions = (int)(pops / (divisibleNegative ? -20f : 20f));
                    int reminder = pops % 20;
                    for (int i = 0; i < fractions; i++)
                        SelectAndUpdatePop(settlement, divisibleNegative ? -20 : 20);
                    
                    SelectAndUpdatePop(settlement, divisibleNegative ? -reminder : reminder);
                }
                else SelectAndUpdatePop(settlement, pops);
            }
            else UpdatePopType(target, pops);

            BalanceClasses(settlement);
        }

        private void BalanceClasses(Settlement settlement)
        {
            Dictionary<PopType, float[]> dic = GetDesiredPopTypes(settlement);
            Dictionary<PopType, float> currentDic = new Dictionary<PopType, float>
            {
                { PopType.Nobles, GetCurrentTypeFraction(PopType.Nobles) },
                { PopType.Craftsmen, GetCurrentTypeFraction(PopType.Craftsmen) },
                { PopType.Serfs, GetCurrentTypeFraction(PopType.Serfs) },
                { PopType.Slaves, GetCurrentTypeFraction(PopType.Slaves) }
            };
            
            if (currentDic[PopType.Slaves] > dic[PopType.Slaves][1])
            {
                int random = MBMath.ClampInt(MBRandom.RandomInt(0, 25), 0, GetTypeCount(PopType.Slaves));
                UpdatePopType(PopType.Slaves, -random);
                UpdatePopType(PopType.Serfs, random);
                if (settlement.Town != null)
                    settlement.Town.Security -= random * 0.01f;
            }

            if (currentDic[PopType.Serfs] > dic[PopType.Serfs][1])
            {
                int random = MBMath.ClampInt(MBRandom.RandomInt(0, 25), 0, GetTypeCount(PopType.Serfs));
                UpdatePopType(PopType.Serfs, -random);
                UpdatePopType(PopType.Craftsmen, random);
            }

            if (currentDic[PopType.Craftsmen] > dic[PopType.Craftsmen][1])
            {
                int random = MBMath.ClampInt(MBRandom.RandomInt(0, 25), 0, GetTypeCount(PopType.Craftsmen));
                UpdatePopType(PopType.Craftsmen, -random);
                UpdatePopType(PopType.Nobles, random);
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
                    float currentState = pops.count * economicData.StateSlaves;
                    if (stateSlaves)
                        currentState += count;
                    economicData.StateSlaves = currentState / total;
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

        public float GetCurrentTypeFraction(PopType type) => GetTypeCount(type) / (float)TotalPop;

        internal override void Update(PopulationData data)
        {
            BKGrowthModel model = (BKGrowthModel)BannerKingsConfig.Instance.Models.First(x => x.GetType() == typeof(BKGrowthModel));
            int growthFactor = (int)model.CalculateEffect(this.settlement, this).ResultNumber;
            UpdatePopulation(settlement, growthFactor, PopType.None);
            BKStabilityModel stabilityModel = (BKStabilityModel)BannerKingsConfig.Instance.Models.First(x => x.GetType() == typeof(BKStabilityModel));
            Stability += stabilityModel.CalculateEffect(settlement).ResultNumber;
            Autonomy += stabilityModel.CalculateAutonomyEffect(settlement, Stability, Autonomy).ResultNumber;
            economicData.Update(this);
            cultureData.Update(this);
            militaryData.Update(this);
            landData.Update(this);
            if (villageData != null) villageData.Update(this);
            if (tournamentData != null) tournamentData.Update(this);
            if (titleData == null) titleData = new TitleData(BannerKingsConfig.Instance.TitleManager.GetTitle(settlement));
            titleData.Update(this);

            if (religionData == null)
            {
                Religion religion = BannerKingsConfig.Instance.ReligionsManager.GetIdealReligion(settlement.Culture);
                if (religion != null)
                    religionData = new ReligionData(religion, settlement);
            }

            if (religionData != null)
                religionData.Update(this);
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

        public List<CultureDataClass> Cultures => cultures;

        public CultureObject DominantCulture
        {
            get
            {
                List<ValueTuple<CultureObject, float>> eligible = new List<(CultureObject, float)>();
                foreach (CultureDataClass data in this.cultures)
                    if (data.Culture.MilitiaPartyTemplate != null && data.Culture.DefaultPartyTemplate != null && !data.Culture.IsBandit)
                        eligible.Add((data.Culture, data.Assimilation));
                eligible.OrderByDescending(pair => pair.Item2);

                return eligible[0].Item1;
            }
        }

        public bool IsCulturePresent(CultureObject culture)
        {
            CultureDataClass data = cultures.FirstOrDefault(x => x.Culture == culture);
            return data != null;
        }

        public Hero SettlementOwner
        {
            get => settlementOwner;
            set
            {
                this.settlementOwner = value;
                if (!this.IsCulturePresent(settlementOwner.Culture))
                {
                    if (settlementOwner.Culture == DominantCulture)
                        AddCulture(settlementOwner.Culture, 1f, 1f);
                    else AddCulture(settlementOwner.Culture, 0f);
                }
            }
        }

        public void AddCulture(CultureObject culture, float acceptance)
        {
            CultureDataClass dataClass = null;
            foreach (CultureDataClass data in cultures)
                if (data.Culture == culture)
                {
                    dataClass = data;
                    break;
                }

            if (dataClass == null) cultures.Add(new CultureDataClass(culture, 0f, acceptance));
            else dataClass.Acceptance = acceptance;
        }

        public void AddCulture(CultureObject culture, float acceptance, float assim)
        {
            CultureDataClass dataClass = null;
            foreach (CultureDataClass data in cultures)
                if (data.Culture == culture)
                {
                    dataClass = data;
                    break;
                }

            if (dataClass == null) cultures.Add(new CultureDataClass(culture, assim, acceptance));
            else
            {
                dataClass.Acceptance = acceptance;
                dataClass.Assimilation = assim;
            }
        }

        public float GetAssimilation(CultureObject culture)
        {
            CultureDataClass data = cultures.FirstOrDefault(x => x.Culture == culture);
            return data != null ? data.Assimilation : 0f;
        }

        public float GetAcceptance(CultureObject culture)
        {
            CultureDataClass data = cultures.FirstOrDefault(x => x.Culture == culture);
            return data != null ? data.Acceptance : 0f;
        }

        internal override void Update(PopulationData data)
        {
            SettlementOwner = data.Settlement.Owner;
            BKCultureAssimilationModel assimModel = (BKCultureAssimilationModel)BannerKingsConfig.Instance.Models.First(x => x.GetType() == typeof(BKCultureAssimilationModel));
            BKCultureAcceptanceModel accModel = (BKCultureAcceptanceModel)BannerKingsConfig.Instance.Models.First(x => x.GetType() == typeof(BKCultureAcceptanceModel));
            HashSet<CultureDataClass> toDelete = new HashSet<CultureDataClass>();

            float foreignerShare = 0f;
            foreach (CultureDataClass cultureData in cultures)
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
                cultures.Remove(cultureData);

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
                    cultures.Add(new CultureDataClass(randomForeign, random, random));
                    foreach (CultureDataClass cultureData in cultures)
                        if (cultureData.Culture == DominantCulture)
                        {
                            cultureData.Assimilation -= random;
                            break;
                        }
                }     
            }

            CultureObject dominant = DominantCulture;
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
            buildings = new List<VillageBuilding>();
            foreach (BuildingType type in DefaultVillageBuildings.VillageBuildings(village))
                buildings.Add(new VillageBuilding(type, village.MarketTown, village));
            inProgress = new Queue<Building>();
        }

        public void StartRandomProject()
        {
            if (inProgress.IsEmpty())
                inProgress.Enqueue(buildings.GetRandomElementWithPredicate(x => x.BuildingType.BuildingLocation != BuildingLocation.Daily));
        }

        public int GetBuildingLevel(BuildingType type)
        {
            Building building = buildings.FirstOrDefault(x => x.BuildingType == type);
            if (building != null) return building.CurrentLevel;
            return 0;
        }

        public void ReInitializeBuildings()
        {
            List<VillageBuilding> toAdd = new List<VillageBuilding>();
            foreach (VillageBuilding b in buildings)
            {
                if (b.Explanation == null)
                {
                    string id = b.BuildingType.StringId;
                    BuildingType type = DefaultVillageBuildings.Instance.All()
                        .FirstOrDefault(x => x.StringId == id);
                    if (type != null)
                        toAdd.Add(new VillageBuilding(type, Village.MarketTown, Village,
                            b.BuildingProgress, b.CurrentLevel));
                }  
            }

            if (toAdd.Count > 0)
            {
                buildings.Clear();
                foreach (VillageBuilding b in toAdd)
                    buildings.Add(b);
            }

            List<VillageBuilding> toAddQueue = new List<VillageBuilding>();
            foreach (VillageBuilding b in inProgress)
                if (b.Explanation == null)
                {
                    string id = b.BuildingType.StringId;
                    BuildingType type = DefaultVillageBuildings.Instance.All()
                        .FirstOrDefault(x => x.StringId == id);
                    if (type != null)
                        toAddQueue.Add(new VillageBuilding(type, Village.MarketTown, Village,
                            b.BuildingProgress, b.CurrentLevel));
                }

            if (toAddQueue.Count > 0)
            {
                inProgress.Clear();
                foreach (VillageBuilding b in toAddQueue)
                    inProgress.Enqueue(b);
            }
        }

        public Village Village => village;
        public List<VillageBuilding> Buildings
        {
            get => buildings;
        }

        public VillageBuilding CurrentBuilding
        {
            get
            {
                VillageBuilding building = null;

                if (inProgress != null && !inProgress.IsEmpty())
                    building = (VillageBuilding?)inProgress.Peek();

                return building != null ? building : CurrentDefault;
            }
        }

        public VillageBuilding CurrentDefault
        {
            get
            {
                VillageBuilding building = buildings.FirstOrDefault(x => x.IsCurrentlyDefault);
                if (building == null)
                {
                    VillageBuilding dailyProd = buildings.FirstOrDefault(x => x.BuildingType.StringId == "bannerkings_daily_production");
                    dailyProd.IsCurrentlyDefault = true;
                    building = dailyProd;
                }

                return building;
            }
        }

        public Queue<Building> BuildingsInProgress
        {
            get => inProgress;
            set => inProgress = value;
        }

        public bool IsCurrentlyBuilding => BuildingsInProgress.Count() > 0;
        public float Construction => new BKConstructionModel().CalculateVillageConstruction(village.Settlement).ResultNumber;

        internal override void Update(PopulationData data)
        {
            VillageBuilding current = CurrentBuilding;
            if (current != null && BuildingsInProgress.Count() > 0)
                if (BuildingsInProgress.Peek().BuildingType.StringId == current.BuildingType.StringId)
                {
                    current.BuildingProgress += Construction;
                    if (current.GetConstructionCost() <= current.BuildingProgress)
                    {
                        if (current.CurrentLevel < 3)
                            current.LevelUp();
                    
                        if (current.CurrentLevel == 3)
                            current.BuildingProgress = current.GetConstructionCost();
                    
                        BuildingsInProgress.Dequeue();
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
            composition = new float[3];
            Init(data.TotalPop);
        }

        private void Init(int totalPops)
        {
            float farmRatio = 0f;
            float pastureRatio = 0f;
            float woodRatio = 0f;
            if (Terrain == TerrainType.Desert)
            {
                fertility = 0.5f;
                terrainDifficulty = 1.4f;
                farmRatio = 0.9f;
                pastureRatio = 0.08f;
                woodRatio = 0.02f;
            }
            else if (Terrain == TerrainType.Steppe)
            {
                fertility = 0.75f;
                terrainDifficulty = 1f;
                farmRatio = 0.45f;
                pastureRatio = 0.5f;
                woodRatio = 0.05f;
            }
            else if (Terrain == TerrainType.Mountain)
            {
                fertility = 0.7f;
                terrainDifficulty = 2f;
                farmRatio = 0.5f;
                pastureRatio = 0.35f;
                woodRatio = 0.15f;
            }
            else if (Terrain == TerrainType.Canyon)
            {
                fertility = 0.5f;
                terrainDifficulty = 2f;
                farmRatio = 0.9f;
                pastureRatio = 0.08f;
                woodRatio = 0.02f;
            }
            else if (Terrain == TerrainType.Forest)
            {
                fertility = 0.5f;
                terrainDifficulty = 2f;
                farmRatio = 0.45f;
                pastureRatio = 0.15f;
                woodRatio = 0.40f;
            } else
            {
                fertility = 1f;
                terrainDifficulty = 1f;
                farmRatio = 0.7f;
                pastureRatio = 0.22f;
                woodRatio = 0.08f;
            }
            composition[0] = farmRatio;
            composition[1] = pastureRatio;
            composition[2] = woodRatio;
            float acres = data.Settlement.IsVillage ? totalPops * MBRandom.RandomFloatRanged(3f, 3.5f) : totalPops * MBRandom.RandomFloatRanged(2.5f, 3.0f);
            farmland = acres * farmRatio;
            pasture = acres * pastureRatio;
            woodland = acres * woodRatio;
        }

        public TerrainType Terrain => Campaign.Current.MapSceneWrapper.GetTerrainTypeAtPosition(data.Settlement.Position2D);

        public int AvailableWorkForce
        {
            get
            {
                float serfs = data.GetTypeCount(PopType.Serfs) * (data.Settlement.IsVillage ? 0.85f : 0.5f);
                float slaves = data.GetTypeCount(PopType.Slaves);

                Town town = data.Settlement.Town;
                if (town != null && town.BuildingsInProgress.Count > 0)
                    slaves -= slaves * data.EconomicData.StateSlaves * 0.5f;

                if (!data.Settlement.IsVillage)
                {
                    if (BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(data.Settlement, "workforce", (int)WorkforcePolicy.Martial_Law))
                    {
                        float militia = data.Settlement.Town.Militia / 2;
                        serfs -= militia / 2f;
                    } else if (BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(data.Settlement, "workforce", (int)WorkforcePolicy.Land_Expansion))
                    {
                        serfs *= 0.8f;
                        slaves *= 0.8f;
                    }
                    else if (BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(data.Settlement, "workforce", (int)WorkforcePolicy.Construction))
                    {
                        serfs *= 0.85f;
                        slaves -= slaves * data.EconomicData.StateSlaves * 0.5f;
                    }
                }
                return Math.Max((int)(serfs + slaves), 0);
            }
        }

        public float WorkforceSaturation
        {
            get
            {
                float available = AvailableWorkForce;
                float farms = farmland / GetRequiredLabor("farmland");
                float pasture = this.pasture / GetRequiredLabor("pasture");
                return available / (farms + pasture);
            }
        }

        public float GetRequiredLabor(string type)
        {
            if (type == "farmland")
                return 4f;
            else if (type == "pasture")
                return 8f;
            else return 10f;
        }

        public float GetAcreOutput(string type)
        {
            if (type == "farmland")
                return 0.022f;
            else if (type == "pasture")
                return 0.008f;
            else return 0.0018f;
        }

        public float Acreage => farmland + pasture + woodland;
        public float Farmland => farmland;
        public float Pastureland => pasture;
        public float Woodland => woodland;
        public float Fertility => fertility;
        public float Difficulty => terrainDifficulty;

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
                        pasture += progress;
                    else this.woodland += progress;
                }
            } else if (BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(this.data.Settlement, "workforce", (int)WorkforcePolicy.Land_Expansion))
            {
                float laborers = AvailableWorkForce * 0.2f;
                float construction = laborers * 0.010f;
                float progress = 15f / construction;

                if (progress > 0f)
                {
                    List<(int, float)> list = new List<(int, float)>();
                    list.Add(new(0, composition[0]));
                    list.Add(new(1, composition[1]));
                    list.Add(new(2, composition[2]));
                    int choosen = MBRandom.ChooseWeighted(list);

                    if (choosen == 0)
                        this.farmland += progress;
                    else if (choosen == 1)
                        pasture += progress;
                    else this.woodland += progress;
                }
            }


            if (WorkforceSaturation > 1f)
            {
                List<(int, float)> list = new List<(int, float)>();
                list.Add(new(0, composition[0]));
                list.Add(new(1, composition[1]));
                list.Add(new(2, composition[2]));
                int choosen = MBRandom.ChooseWeighted(list);

                float construction = this.data.Settlement.IsVillage ? this.data.VillageData.Construction : 
                    new BKConstructionModel().CalculateDailyConstructionPower(this.data.Settlement.Town).ResultNumber;
                construction *= 0.8f;
                float progress = 15f / construction;

                if (choosen == 0)
                    this.farmland += progress;
                else if (choosen == 1)
                    pasture += progress;
                else this.woodland += progress;
            }

            float farmland = this.farmland;
            float pastureland = pasture;
            float woodland = this.woodland;

            this.farmland = MBMath.ClampFloat(farmland, 0f, 100000f);
            pasture = MBMath.ClampFloat(pastureland, 0f, 50000f);
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
            roster = new ItemRoster();
            active = true;
        }

        public bool Active
        {
            get => active;
            set
            {
                active = value;
            }
        }

        public ItemRoster Roster => roster;
        public ItemObject Prize
        {
            get
            {
                if (prize == null)
                {
                    List<ItemObject> items = new List<ItemObject>();
                    foreach (ItemRosterElement element in roster)
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
                        prize = items[0];
                    }
                }
                
                return prize;
            }
        }

        internal override void Update(PopulationData data)
        {
            if (!data.Settlement.Town.HasTournament)
                active = false;
        }
    }
}
