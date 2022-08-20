using System;
using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers.Innovations;
using BannerKings.Managers.Institutions.Guilds;
using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Populations.Villages;
using BannerKings.Managers.Skills;
using BannerKings.Managers.Titles;
using BannerKings.Models.BKModels;
using BannerKings.Models.Vanilla;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.SaveSystem;
using static BannerKings.Managers.PopulationManager;
using static BannerKings.Managers.Policies.BKWorkforcePolicy;

namespace BannerKings.Managers.Populations;

public class PopulationData : BannerKingsData
{
    public PopulationData(List<PopulationClass> classes, Settlement settlement, float assimilation,
        List<CultureDataClass> cultures = null, Guild guild = null)
    {
        this.classes = classes;
        stability = 0.5f;
        this.settlement = settlement;
        economicData = new EconomicData(settlement, guild);

        if (cultures == null)
        {
            var cultureData = new CultureDataClass(settlement.Culture, 1f, 1f);
            cultures = new List<CultureDataClass>();
            cultures.Add(cultureData);
        }

        this.cultureData = new CultureData(settlement.Owner, cultures);
        float total = TotalPop;
        float nobles = classes.First(x => x.type == PopType.Nobles).count;
        militaryData = new MilitaryData(settlement, (int) (total * 0.04f), (int) (nobles * 0.08f));
        landData = new LandData(this);

        if (settlement.Village != null)
        {
            villageData = new VillageData(settlement.Village);
        }
    }

    [SaveableProperty(1)] private List<PopulationClass> classes { get; set; }

    [SaveableProperty(2)] private float stability { get; set; }

    [SaveableProperty(3)] private Settlement settlement { get; }

    [SaveableProperty(4)] private CultureData cultureData { get; }

    [SaveableProperty(5)] private VillageData villageData { get; }

    [SaveableProperty(6)] private LandData landData { get; }

    [SaveableProperty(7)] private MilitaryData militaryData { get; }

    [SaveableProperty(8)] private EconomicData economicData { get; }

    [SaveableProperty(9)] private TournamentData tournamentData { get; set; }

    [SaveableProperty(10)] private TitleData titleData { get; set; }

    [SaveableProperty(11)] private float autonomy { get; set; }

    [SaveableProperty(12)] private ReligionData religionData { get; set; }

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

    public int TotalPop
    {
        get
        {
            var total = 0;
            classes.ForEach(popClass => total += popClass.count);
            return total;
        }
    }

    public Settlement Settlement => settlement;

    public ExplainedNumber Growth
    {
        get
        {
            var model = (BKGrowthModel) BannerKingsConfig.Instance.Models.First(x =>
                x.GetType() == typeof(BKGrowthModel));
            return model.CalculateEffect(settlement, this);
        }
    }

    public float Stability
    {
        get => stability;
        set
        {
            if (value != stability)
            {
                stability = MBMath.ClampFloat(value, 0f, 1f);
            }
        }
    }

    public float Autonomy
    {
        get => autonomy;
        set
        {
            if (value != autonomy)
            {
                autonomy = MBMath.ClampFloat(value, 0f, 1f);
            }
        }
    }

    public ExplainedNumber NotableSupport =>
        BannerKingsConfig.Instance.StabilityModel.CalculateNotableSupport(settlement);

    public List<PopulationClass> Classes
    {
        get => classes;
        set
        {
            if (value != classes)
            {
                classes = value;
            }
        }
    }

    public void UpdatePopulation(Settlement settlement, int pops, PopType target)
    {
        if (target == PopType.None)
        {
            if (settlement.Owner == Hero.MainHero)
            {
                InformationManager.DisplayMessage(new InformationMessage());
            }

            var divisibleNegative = pops * -1f > 20;
            if (pops > 20 || divisibleNegative)
            {
                var fractions = (int) (pops / (divisibleNegative ? -20f : 20f));
                var reminder = pops % 20;
                for (var i = 0; i < fractions; i++)
                {
                    SelectAndUpdatePop(settlement, divisibleNegative ? -20 : 20);
                }

                SelectAndUpdatePop(settlement, divisibleNegative ? -reminder : reminder);
            }
            else
            {
                SelectAndUpdatePop(settlement, pops);
            }
        }
        else
        {
            UpdatePopType(target, pops);
        }

        BalanceClasses(settlement);
    }

    private void BalanceClasses(Settlement settlement)
    {
        var dic = GetDesiredPopTypes(settlement);
        var currentDic = new Dictionary<PopType, float>
        {
            {PopType.Nobles, GetCurrentTypeFraction(PopType.Nobles)},
            {PopType.Craftsmen, GetCurrentTypeFraction(PopType.Craftsmen)},
            {PopType.Serfs, GetCurrentTypeFraction(PopType.Serfs)},
            {PopType.Slaves, GetCurrentTypeFraction(PopType.Slaves)}
        };

        if (currentDic[PopType.Slaves] > dic[PopType.Slaves][1])
        {
            var random = MBMath.ClampInt(MBRandom.RandomInt(0, 25), 0, GetTypeCount(PopType.Slaves));
            UpdatePopType(PopType.Slaves, -random);
            UpdatePopType(PopType.Serfs, random);
            if (settlement.Town != null)
            {
                settlement.Town.Security -= random * 0.01f;
            }
        }

        if (currentDic[PopType.Serfs] > dic[PopType.Serfs][1])
        {
            var random = MBMath.ClampInt(MBRandom.RandomInt(0, 25), 0, GetTypeCount(PopType.Serfs));
            UpdatePopType(PopType.Serfs, -random);
            UpdatePopType(PopType.Craftsmen, random);
        }

        if (currentDic[PopType.Craftsmen] > dic[PopType.Craftsmen][1])
        {
            var random = MBMath.ClampInt(MBRandom.RandomInt(0, 25), 0, GetTypeCount(PopType.Craftsmen));
            UpdatePopType(PopType.Craftsmen, -random);
            UpdatePopType(PopType.Nobles, random);
        }
    }

    private void SelectAndUpdatePop(Settlement settlement, int pops)
    {
        if (pops != 0)
        {
            var desiredTypes = GetDesiredPopTypes(settlement);
            var typesList = new List<ValueTuple<PopType, float>>();


            if (pops < 0)
            {
                var slaveClass = classes.FirstOrDefault(x => x.type == PopType.Slaves);
                if (slaveClass != null && slaveClass.count > 0)
                {
                    UpdatePopType(PopType.Slaves, pops);
                    return;
                }
            }

            classes.ForEach(popClass =>
            {
                var type = popClass.type;
                if (pops < 0 && popClass.count >= pops)
                {
                    var hasExcess = GetCurrentTypeFraction(type) > desiredTypes[type][1];
                    typesList.Add(new ValueTuple<PopType, float>(popClass.type,
                        (float) popClass.type * 5f + desiredTypes[type][0] * (hasExcess ? 2f : 1f)));
                }
                else if (pops > 0)
                {
                    var isLacking = GetCurrentTypeFraction(type) < desiredTypes[type][0];
                    typesList.Add(new ValueTuple<PopType, float>(popClass.type,
                        desiredTypes[type][0] * (isLacking ? 2f : 1f)));
                }
            });

            var targetType = MBRandom.ChooseWeighted(typesList);
            UpdatePopType(targetType, pops);
        }
    }

    public void UpdatePopType(PopType type, int count, bool stateSlaves = false)
    {
        if (type != PopType.None)
        {
            var pops = classes.Find(popClass => popClass.type == type);
            if (pops == null)
            {
                pops = new PopulationClass(type, 0);
            }

            if (type == PopType.Slaves)
            {
                var total = pops.count + count;
                var currentState = pops.count * economicData.StateSlaves;
                if (stateSlaves)
                {
                    currentState += count;
                }

                economicData.StateSlaves = currentState / total;
            }

            pops.count += count;
            if (pops.count < 0)
            {
                pops.count = 0;
            }
        }
    }

    public int GetTypeCount(PopType type)
    {
        var i = 0;
        var targetClass = classes.Find(popClass => popClass.type == type);
        if (targetClass != null)
        {
            i = targetClass.count;
        }

        return MBMath.ClampInt(i, 0, 50000);
    }

    public float GetCurrentTypeFraction(PopType type)
    {
        return GetTypeCount(type) / (float) TotalPop;
    }

    internal override void Update(PopulationData data)
    {
        var model = (BKGrowthModel) BannerKingsConfig.Instance.Models.First(x => x.GetType() == typeof(BKGrowthModel));
        var growthFactor = (int) model.CalculateEffect(settlement, this).ResultNumber;
        UpdatePopulation(settlement, growthFactor, PopType.None);
        var stabilityModel =
            (BKStabilityModel) BannerKingsConfig.Instance.Models.First(x => x.GetType() == typeof(BKStabilityModel));
        Stability += stabilityModel.CalculateEffect(settlement).ResultNumber;
        Autonomy += stabilityModel.CalculateAutonomyEffect(settlement, Stability, Autonomy).ResultNumber;
        economicData.Update(this);
        cultureData.Update(this);
        militaryData.Update(this);
        landData.Update(this);
        if (villageData != null)
        {
            villageData.Update(this);
        }

        if (tournamentData != null)
        {
            tournamentData.Update(this);
        }

        if (titleData == null)
        {
            titleData = new TitleData(BannerKingsConfig.Instance.TitleManager.GetTitle(settlement));
        }

        titleData.Update(this);

        if (religionData == null)
        {
            var religion = BannerKingsConfig.Instance.ReligionsManager.GetIdealReligion(settlement.Culture);
            if (religion != null)
            {
                religionData = new ReligionData(religion, settlement);
            }
        }

        if (religionData != null)
        {
            religionData.Update(this);
        }
    }
}

public class PopulationClass
{
    public PopulationClass(PopType type, int count)
    {
        this.type = type;
        this.count = count;
    }

    [SaveableProperty(1)] public PopType type { get; set; }

    [SaveableProperty(2)] public int count { get; set; }
}

public class CultureData : BannerKingsData
{
    public CultureData(Hero settlementOwner, List<CultureDataClass> cultures)
    {
        this.settlementOwner = settlementOwner;
        this.cultures = cultures;
    }

    [SaveableProperty(1)] private List<CultureDataClass> cultures { get; }

    [SaveableProperty(2)] private Hero settlementOwner { get; set; }

    public List<CultureDataClass> Cultures => cultures;

    public CultureObject DominantCulture
    {
        get
        {
            var eligible = new List<(CultureObject, float)>();
            foreach (var data in cultures)
            {
                if (data.Culture.MilitiaPartyTemplate != null && data.Culture.DefaultPartyTemplate != null &&
                    !data.Culture.IsBandit)
                {
                    eligible.Add((data.Culture, data.Assimilation));
                }
            }

            eligible.OrderByDescending(pair => pair.Item2);

            return eligible[0].Item1;
        }
    }

    public Hero SettlementOwner
    {
        get => settlementOwner;
        set
        {
            settlementOwner = value;
            if (!IsCulturePresent(settlementOwner.Culture))
            {
                if (settlementOwner.Culture == DominantCulture)
                {
                    AddCulture(settlementOwner.Culture, 1f, 1f);
                }
                else
                {
                    AddCulture(settlementOwner.Culture, 0f);
                }
            }
        }
    }

    public bool IsCulturePresent(CultureObject culture)
    {
        var data = cultures.FirstOrDefault(x => x.Culture == culture);
        return data != null;
    }

    public void AddCulture(CultureObject culture, float acceptance)
    {
        CultureDataClass dataClass = null;
        foreach (var data in cultures)
        {
            if (data.Culture == culture)
            {
                dataClass = data;
                break;
            }
        }

        if (dataClass == null)
        {
            cultures.Add(new CultureDataClass(culture, 0f, acceptance));
        }
        else
        {
            dataClass.Acceptance = acceptance;
        }
    }

    public void AddCulture(CultureObject culture, float acceptance, float assim)
    {
        CultureDataClass dataClass = null;
        foreach (var data in cultures)
        {
            if (data.Culture == culture)
            {
                dataClass = data;
                break;
            }
        }

        if (dataClass == null)
        {
            cultures.Add(new CultureDataClass(culture, assim, acceptance));
        }
        else
        {
            dataClass.Acceptance = acceptance;
            dataClass.Assimilation = assim;
        }
    }

    public float GetAssimilation(CultureObject culture)
    {
        var data = cultures.FirstOrDefault(x => x.Culture == culture);
        return data != null ? data.Assimilation : 0f;
    }

    public float GetAcceptance(CultureObject culture)
    {
        var data = cultures.FirstOrDefault(x => x.Culture == culture);
        return data != null ? data.Acceptance : 0f;
    }

    internal override void Update(PopulationData data)
    {
        SettlementOwner = data.Settlement.Owner;


        BalanceCultures(data);
        var dominant = DominantCulture;
        if (dominant.BasicTroop != null && dominant.MilitiaSpearman != null)
        {
            data.Settlement.Culture = dominant;
            if (data.Settlement.Notables != null && data.Settlement.Notables.Count > 0)
            {
                foreach (var notable in data.Settlement.Notables)
                {
                    notable.Culture = dominant;
                }
            }
        }
    }

    private void BalanceCultures(PopulationData data)
    {
        var toDelete = new HashSet<CultureDataClass>();
        var foreignerShare = 0f;

        foreach (var cultureData in cultures)
        {
            if (cultureData.Culture != settlementOwner.Culture && cultureData.Assimilation <= 0.01)
            {
                toDelete.Add(cultureData);
                continue;
            }

            if (IsForeigner(data, cultureData))
            {
                foreignerShare += cultureData.Assimilation;
            }
            else
            {
                cultureData.Acceptance += BannerKingsConfig.Instance.CultureAcceptanceModel
                    .CalculateEffect(data.Settlement, cultureData).ResultNumber;
                cultureData.Assimilation += BannerKingsConfig.Instance.CultureAssimilationModel
                    .CalculateEffect(data.Settlement, cultureData).ResultNumber;
            }
        }

        if (toDelete.Count > 0)
        {
            foreach (var cultureData in toDelete)
            {
                cultures.Remove(cultureData);
            }
        }

        var totalAssim = 0f;
        foreach (var cultureData in cultures)
        {
            totalAssim += cultureData.Assimilation;
        }

        if (totalAssim != 1f)
        {
            var diff = totalAssim - 1f;
            var foreignerTarget = data.Foreigner.ResultNumber;

            var candidates = new List<(CultureDataClass, float)>();
            foreach (var cultureData in cultures)
            {
                if (cultureData.Assimilation > diff)
                {
                    var value = cultureData.Assimilation;
                    if (foreignerShare > foreignerTarget && IsForeigner(data, cultureData))
                    {
                        value *= 10f;
                    }

                    candidates.Add(new ValueTuple<CultureDataClass, float>(cultureData, value));
                }
            }

            var result = MBRandom.ChooseWeighted(candidates);
            result.Assimilation += diff;
        }


        /*if (foreignerShare < foreignerTarget)
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
        }*/
    }

    private bool IsForeigner(PopulationData data, CultureDataClass cultureData)
    {
        return cultureData.Culture != settlementOwner.Culture && cultureData.Culture != data.Settlement.Culture;
    }
}

public class CultureDataClass
{
    public CultureDataClass(CultureObject culture, float assimilation, float acceptance)
    {
        this.culture = culture;
        this.assimilation = assimilation;
        this.acceptance = acceptance;
    }

    [SaveableProperty(1)] private CultureObject culture { get; }

    [SaveableProperty(2)] private float assimilation { get; set; }

    [SaveableProperty(3)] private float acceptance { get; set; }

    internal float Assimilation
    {
        get => assimilation;
        set => assimilation = MBMath.ClampFloat(value, 0f, 1f);
    }

    internal float Acceptance
    {
        get => acceptance;
        set => acceptance = MBMath.ClampFloat(value, 0f, 1f);
    }

    internal CultureObject Culture => culture;
}

public class VillageData : BannerKingsData
{
    public VillageData(Village village)
    {
        this.village = village;
        buildings = new List<VillageBuilding>();
        foreach (var type in DefaultVillageBuildings.VillageBuildings(village))
        {
            buildings.Add(new VillageBuilding(type, village.Bound.Town, village));
        }

        inProgress = new Queue<Building>();
    }

    [SaveableProperty(1)] private Village village { get; }

    [SaveableProperty(2)] private List<VillageBuilding> buildings { get; }

    [SaveableProperty(5)] private Queue<Building> inProgress { get; set; }

    public Village Village => village;

    public List<VillageBuilding> Buildings => buildings;

    public VillageBuilding CurrentBuilding
    {
        get
        {
            VillageBuilding building = null;

            if (inProgress != null && !inProgress.IsEmpty())
            {
                building = (VillageBuilding?) inProgress.Peek();
            }

            return building != null ? building : CurrentDefault;
        }
    }

    public VillageBuilding CurrentDefault
    {
        get
        {
            var building = buildings.FirstOrDefault(x => x.IsCurrentlyDefault);
            if (building == null)
            {
                var dailyProd =
                    buildings.FirstOrDefault(x => x.BuildingType.StringId == "bannerkings_daily_production");
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

    public float Construction =>
        new BKConstructionModel().CalculateVillageConstruction(village.Settlement).ResultNumber;

    public void StartRandomProject()
    {
        if (inProgress.IsEmpty())
        {
            inProgress.Enqueue(buildings.GetRandomElementWithPredicate(x =>
                x.BuildingType.BuildingLocation != BuildingLocation.Daily));
        }
    }

    public int GetBuildingLevel(BuildingType type)
    {
        Building building = buildings.FirstOrDefault(x => x.BuildingType == type);
        if (building != null)
        {
            return building.CurrentLevel;
        }

        return 0;
    }

    public void ReInitializeBuildings()
    {
        foreach (var building in buildings)
        {
            building.PostInitialize();
        }

        foreach (VillageBuilding building in inProgress)
        {
            building.PostInitialize();
        }
    }

    internal override void Update(PopulationData data)
    {
        var current = CurrentBuilding;
        if (current != null && BuildingsInProgress.Count() > 0)
        {
            if (BuildingsInProgress.Peek().BuildingType.StringId == current.BuildingType.StringId)
            {
                current.BuildingProgress += Construction;
                if (current.GetConstructionCost() <= current.BuildingProgress)
                {
                    if (current.CurrentLevel < 3)
                    {
                        current.LevelUp();
                    }

                    if (current.CurrentLevel == 3)
                    {
                        current.BuildingProgress = current.GetConstructionCost();
                    }

                    BuildingsInProgress.Dequeue();
                }
            }
        }
    }
}

public class LandData : BannerKingsData
{
    public LandData(PopulationData data)
    {
        this.data = data;
        composition = new float[3];
        Init(data.TotalPop);
    }

    [SaveableProperty(1)] private PopulationData data { get; }

    [SaveableProperty(2)] private float farmland { get; set; }

    [SaveableProperty(3)] private float pasture { get; set; }

    [SaveableProperty(4)] private float woodland { get; set; }

    [SaveableProperty(5)] private float fertility { get; set; }

    [SaveableProperty(6)] private float terrainDifficulty { get; set; }

    [SaveableProperty(7)] private float[] composition { get; }

    public TerrainType Terrain => Campaign.Current.MapSceneWrapper.GetTerrainTypeAtPosition(data.Settlement.Position2D);

    public int AvailableWorkForce
    {
        get
        {
            var serfs = data.GetTypeCount(PopType.Serfs) * (data.Settlement.IsVillage ? 0.85f : 0.5f);
            float slaves = data.GetTypeCount(PopType.Slaves);

            var town = data.Settlement.Town;
            if (town != null && town.BuildingsInProgress.Count > 0)
            {
                slaves -= slaves * data.EconomicData.StateSlaves * 0.5f;
            }

            if (!data.Settlement.IsVillage)
            {
                if (BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(data.Settlement, "workforce",
                        (int) WorkforcePolicy.Martial_Law))
                {
                    var militia = data.Settlement.Town.Militia / 2;
                    serfs -= militia / 2f;
                }
                else if (BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(data.Settlement, "workforce",
                             (int) WorkforcePolicy.Land_Expansion))
                {
                    serfs *= 0.8f;
                    slaves *= 0.8f;
                }
                else if (BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(data.Settlement, "workforce",
                             (int) WorkforcePolicy.Construction))
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
        }
        else
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
        var acres = data.Settlement.IsVillage
            ? totalPops * MBRandom.RandomFloatRanged(3f, 3.5f)
            : totalPops * MBRandom.RandomFloatRanged(2.5f, 3.0f);
        farmland = acres * farmRatio;
        pasture = acres * pastureRatio;
        woodland = acres * woodRatio;
    }

    public float GetRequiredLabor(string type)
    {
        if (type == "farmland")
        {
            return 4f;
        }

        if (type == "pasture")
        {
            return 8f;
        }

        return 10f;
    }

    public float GetAcreOutput(string type)
    {
        float result;
        if (type == "farmland")
        {
            result = 0.018f;
        }
        else if (type == "pasture")
        {
            result = 0.006f;
        }
        else
        {
            result = 0.0012f;
        }

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
                     (int) WorkforcePolicy.Land_Expansion))
        {
            var laborers = AvailableWorkForce * 0.2f;
            var construction = laborers * 0.010f;
            var progress = 15f / construction;

            if (progress > 0f)
            {
                var list = new List<(int, float)>();
                list.Add(new ValueTuple<int, float>(0, composition[0]));
                list.Add(new ValueTuple<int, float>(1, composition[1]));
                list.Add(new ValueTuple<int, float>(2, composition[2]));
                var choosen = MBRandom.ChooseWeighted(list);

                if (choosen == 0)
                {
                    this.farmland += progress;
                }
                else if (choosen == 1)
                {
                    pasture += progress;
                }
                else
                {
                    this.woodland += progress;
                }
            }
        }


        if (WorkforceSaturation > 1f)
        {
            var list = new List<(int, float)>();
            list.Add(new ValueTuple<int, float>(0, composition[0]));
            list.Add(new ValueTuple<int, float>(1, composition[1]));
            list.Add(new ValueTuple<int, float>(2, composition[2]));
            var choosen = MBRandom.ChooseWeighted(list);

            var construction = this.data.Settlement.IsVillage
                ? this.data.VillageData.Construction
                : new BKConstructionModel().CalculateDailyConstructionPower(this.data.Settlement.Town).ResultNumber;
            construction *= 0.8f;
            var progress = 15f / construction;

            if (choosen == 0)
            {
                this.farmland += progress;
            }
            else if (choosen == 1)
            {
                pasture += progress;
            }
            else
            {
                this.woodland += progress;
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

public class TournamentData : BannerKingsData
{
    private Town town;

    public TournamentData(Town town)
    {
        this.town = town;
        roster = new ItemRoster();
        active = true;
    }

    [SaveableProperty(1)] private ItemRoster roster { get; }

    [SaveableProperty(2)] private ItemObject prize { get; set; }

    [SaveableProperty(3)] private bool active { get; set; }

    public bool Active
    {
        get => active;
        set => active = value;
    }

    public ItemRoster Roster => roster;

    public ItemObject Prize
    {
        get
        {
            if (prize == null)
            {
                var items = new List<ItemObject>();
                foreach (var element in roster)
                {
                    var equipment = element.EquipmentElement;
                    var item = equipment.Item;
                    if (item != null)
                    {
                        if (item.IsMountable || item.HasWeaponComponent || item.HasArmorComponent)
                        {
                            items.Add(item);
                        }
                    }
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
        {
            active = false;
        }
    }
}