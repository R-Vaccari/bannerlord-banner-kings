using System;
using System.Linq;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.SaveSystem;
using static BannerKings.Managers.PopulationManager;
using BannerKings.Models;
using BannerKings.Models.Vanilla;
using BannerKings.Models.Populations;
using TaleWorlds.ObjectSystem;
using BannerKings.Managers.Institutions;
using BannerKings.Managers.Populations.Villages;

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
        private EconomicData economicData { get; set; }
        [SaveableProperty(5)]
        private CultureData cultureData { get; set; }
        [SaveableProperty(6)]
        private MilitaryData militaryData { get; set; }
        [SaveableProperty(7)]
        private LandData landData { get; set; }
        [SaveableProperty(8)]
        private TournamentData tournamentData { get; set; }
        private VillageData villageData { get; set; }

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
            set
            {
                this.tournamentData = value;
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
                    stability = value;
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
                    {
                        SelectAndUpdatePop(settlement, divisibleNegative ? -20 : 20);
                    }
                    SelectAndUpdatePop(settlement, divisibleNegative ? -reminder : reminder);
                }
                else SelectAndUpdatePop(settlement, pops);
            }
            else UpdatePopType(target, pops);
        }

        private void SelectAndUpdatePop(Settlement settlement, int pops)
        {
            if (pops != 0)
            {
                Dictionary<PopType, float[]> desiredTypes = GetDesiredPopTypes(settlement);
                List<ValueTuple<PopType, float>> typesList = new List<ValueTuple<PopType, float>>();
                classes.ForEach(popClass =>
                {
                    PopType type = popClass.type;
                    if (pops < 0 && popClass.count >= pops)
                    {
                        bool hasExcess = GetCurrentTypeFraction(type) > desiredTypes[type][1];
                        typesList.Add(new ValueTuple<PopType, float>(popClass.type, desiredTypes[type][0] * (hasExcess ? 2f : 1f)));
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

        public void UpdatePopType(PopType type, int count)
        {
            if (type != PopType.None)
            {
                PopulationClass pops = classes.Find(popClass => popClass.type == type);
                if (pops == null)
                    pops = new PopulationClass(type, 0);

                pops.count += count;
                if (pops.count < 0) pops.count = 0;
            }
        }

        public int GetTypeCount(PopType type)
        {
            PopulationClass targetClass = classes.Find(popClass => popClass.type == type);
            return targetClass != null ? targetClass.count : 0;
        }

        public float GetCurrentTypeFraction(PopType type) => (float)GetTypeCount(type) / (float)TotalPop;

        internal override void Update(PopulationData data)
        {
            BKGrowthModel model = (BKGrowthModel)BannerKingsConfig.Instance.Models.First(x => x.GetType() == typeof(BKGrowthModel));
            int growthFactor = (int)model.CalculateEffect(this.settlement, this).ResultNumber;
            this.UpdatePopulation(settlement, growthFactor, PopType.None);
            economicData.Update(this);
            cultureData.Update(this);
            militaryData.Update(this);
            landData.Update(this);
            if (villageData != null) villageData.Update(this);
            if (tournamentData != null) tournamentData.Update(this);
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
        private List<CultureDataClass> cultures;
        private Hero settlementOwner;

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
                    if (data.Assimilation >= share)
                    {
                        culture = data.Culture;
                        share = data.Assimilation;
                        if (data.Culture == ownerCulture)
                            ownerShare = data.Assimilation;
                    }
                }

                return share == ownerShare ? ownerCulture : (share > ownerShare ? culture : ownerCulture);
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
                if (this.IsCulturePresent(settlementOwner.Culture))
                    this.AddCulture(settlementOwner.Culture, 0f);
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
            BKCultureAssimilationModel assimModel = (BKCultureAssimilationModel)BannerKingsConfig.Instance.Models.First(x => x.GetType() == typeof(BKCultureAssimilationModel));
            BKCultureAcceptanceModel accModel = (BKCultureAcceptanceModel)BannerKingsConfig.Instance.Models.First(x => x.GetType() == typeof(BKCultureAcceptanceModel));
            HashSet<CultureDataClass> toDelete = new HashSet<CultureDataClass>();

            float foreignerShare = 0f;
            foreach (CultureDataClass cultureData in this.cultures)
            {
                cultureData.Acceptance += accModel.CalculateEffect(data.Settlement, cultureData).ResultNumber;
                cultureData.Assimilation += assimModel.CalculateEffect(data.Settlement, cultureData).ResultNumber;
                if (cultureData.Culture != settlementOwner.Culture && cultureData.Assimilation == 0f)
                    toDelete.Add(cultureData);

                if (cultureData.Culture != settlementOwner.Culture && cultureData.Culture != data.Settlement.Culture)
                    foreignerShare += cultureData.Assimilation;
            }

            if (toDelete.Count > 0)
                foreach (CultureDataClass cultureData in toDelete)
                this.cultures.Remove(cultureData);

            float foreignerTarget = data.Foreigner.ResultNumber;
            float diff = foreignerTarget - foreignerShare;
            if (foreignerShare < foreignerTarget)
            {
                float random = MBRandom.RandomFloatRanged(diff);
                CultureObject randomForeign = MBObjectManager.Instance.GetObjectTypeList<CultureObject>()
                    .GetRandomElementWithPredicate(x => x != settlementOwner.Culture && x != data.Settlement.Culture && !x.IsBandit);
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

        }
    }

    public class CultureDataClass
    {
        private CultureObject culture { get; set; }
        private float assimilation { get; set; }
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

    public class EconomicData : BannerKingsData
    {
        private Settlement settlement { get; set; }
        private Guild guild { get; set; }
        private float[] satisfactions { get; set; }

        private Dictionary<Hero, float> slaveOwners { get; set; }

        public EconomicData(Settlement settlement,
            Guild guild = null)
        {
            this.settlement = settlement;
            this.guild = new Guild(settlement, Managers.Institutions.GuildType.Merchants, null);
            this.satisfactions = new float[] { 0.5f, 0.5f, 0.5f,0.5f };
            this.slaveOwners = new Dictionary<Hero, float>();
        }

        public float GetSlaveShare(Hero hero)
        {
            if (this.slaveOwners.ContainsKey(hero))
                return this.slaveOwners[hero];
            else return 0f;
        }

        public Guild Guild => this.guild;

        public float Corruption => 1f;

        public float Tariff => new BKTaxModel().GetTownTaxRatio(settlement.Town);

        public float StateSlaves => this.GetSlaveShare(this.settlement.Owner);

        public float[] Satisfactions => this.satisfactions;

        public void UpdateSatisfaction(ConsumptionType type, float value)
        {
            float current = this.satisfactions[(int)type];
            this.satisfactions[(int)type] = MathF.Clamp(current + value, 0f, 1f);
        }

        internal override void Update(PopulationData data)
        {
            
        }

        public ExplainedNumber AdministrativeCost => BannerKingsConfig.Instance.Models
            .First(x => x.GetType() == typeof(AdministrativeModel)).CalculateEffect(settlement);
        public float MerchantRevenue => settlement.Town != null ? new BKEconomyModel().GetMerchantIncome(settlement.Town) : 0f;
        public ExplainedNumber CaravanAttraction
        {
            get
            {
                BKCaravanAttractionModel model = (BKCaravanAttractionModel)BannerKingsConfig.Instance.Models.First(x => x.GetType() == typeof(BKCaravanAttractionModel));
                return model.CalculateEffect(settlement);
            }
        }
        public ExplainedNumber Mercantilism
        {
            get
            {
                BKEconomyModel model = (BKEconomyModel)BannerKingsConfig.Instance.Models.First(x => x.GetType() == typeof(BKEconomyModel));
                return model.CalculateEffect(settlement);
            }
        }
        public ExplainedNumber ProductionEfficiency
        {
            get
            {
                BKEconomyModel model = (BKEconomyModel)BannerKingsConfig.Instance.Models.First(x => x.GetType() == typeof(BKEconomyModel));
                return model.CalculateProductionEfficiency(settlement);
            }
        }
        public ExplainedNumber ProductionQuality
        {
            get
            {
                BKEconomyModel model = (BKEconomyModel)BannerKingsConfig.Instance.Models.First(x => x.GetType() == typeof(BKEconomyModel));
                return model.CalculateProductionQuality(settlement);
            }
        }
    }

    public class MilitaryData : BannerKingsData
    {
        private Settlement settlement;
        private int peasantManpower;
        private int nobleManpower;
        private List<SiegeEngineType> engines;

        public MilitaryData(Settlement settlement, int peasantManpower, int nobleManpower)
        {
            this.settlement = settlement;
            this.peasantManpower = peasantManpower;
            this.nobleManpower = nobleManpower;
            this.engines = new List<SiegeEngineType>();
        }

        public void DeduceManpower(int quantity, bool noble)
        {
            if (noble) this.nobleManpower -= quantity;
            else this.peasantManpower -= quantity;
        }

        public int Manpower => peasantManpower + nobleManpower;
        public int PeasantManpower => peasantManpower;
        public int NobleManpower => nobleManpower;
        public ExplainedNumber DraftEfficiency => this.settlement.IsTown ? new BKVolunteerModel().GetDraftEfficiency(settlement.Notables[0], 2, settlement) 
            : new ExplainedNumber(0f, true, new TaleWorlds.Localization.TextObject("Not a town"));
        public ExplainedNumber Militarism => this.settlement.IsTown ? new BKVolunteerModel().GetMilitarism(settlement)
            : new ExplainedNumber(0f, true, new TaleWorlds.Localization.TextObject("Not a town"));

        public int Holdout => new BKFoodModel().GetFoodEstimate(settlement.Town, true, settlement.Town.FoodStocksUpperLimit());

        public IEnumerable<SiegeEngineType> Engines => this.engines;

        public int Ballistae => new BKSiegeEventModel().GetPrebuiltSiegeEnginesOfSettlement(settlement).Count(x => x == DefaultSiegeEngineTypes.Ballista);
        public int Catapultae => new BKSiegeEventModel().GetPrebuiltSiegeEnginesOfSettlement(settlement).Count(x => x == DefaultSiegeEngineTypes.Catapult);
        public int Trebuchets => new BKSiegeEventModel().GetPrebuiltSiegeEnginesOfSettlement(settlement).Count(x => x == DefaultSiegeEngineTypes.Trebuchet);

        internal override void Update(PopulationData data)
        {
            BKVolunteerModel model = new BKVolunteerModel();
            float serfMilitarism = model.GetClassMilitarism(PopType.Serfs);
            float serfs = data.GetTypeCount(PopType.Serfs);

            float craftsmanMilitarism = model.GetClassMilitarism(PopType.Craftsmen);
            float craftsmen = data.GetTypeCount(PopType.Craftsmen);
            int peasantCap = (int)((serfs * serfMilitarism) + (craftsmen * craftsmanMilitarism));

            int growth = (int)data.Growth.ResultNumber;
            int peasantChange = (int)(growth * serfMilitarism);

            if (peasantManpower > peasantCap)
                this.peasantManpower = (int)((float)this.peasantManpower * -0.05f); // Change later
            else if (peasantManpower < peasantCap)
                this.peasantManpower = (int)((float)this.peasantManpower * 0.05f);

            float nobleMilitarism = model.GetClassMilitarism(PopType.Nobles);
            float nobles = data.GetTypeCount(PopType.Nobles);
            int nobleCap = (int)(nobles * nobleMilitarism);
            if (nobleManpower > nobleCap)
                this.nobleManpower = (int)((float)this.nobleManpower * -0.05f);
            else if (nobleManpower < nobleCap)
                this.nobleManpower = (int)((float)this.nobleManpower * 0.05f);
        }
    }

    public class VillageData : BannerKingsData
    {
        Village village;
        List<VillageBuilding> buildings;
        VillageBuilding current;
        VillageBuilding currentDefault;
        Queue<Building> inProgress;

        public VillageData(Village village)
        {
            this.village = village;
            this.buildings = new List<VillageBuilding>();
            foreach (BuildingType type in DefaultVillageBuildings.VillageBuildings(village))
                this.buildings.Add(new VillageBuilding(type, village.MarketTown, village));
            this.inProgress = new Queue<Building>();
        }

        public int GetBuildingLevel(BuildingType type)
        {
            Building building = this.buildings.FirstOrDefault(x => x.BuildingType == type);
            if (building != null) return building.CurrentLevel;
            return 0;
        }

        public Village Village => this.village;
        public List<VillageBuilding> Buildings => this.buildings;
        public VillageBuilding CurrentBuilding
        {
            get
            {
                if (this.current == null)
                    this.current = this.buildings.GetRandomElementWithPredicate(x => x.BuildingType.BuildingLocation != BuildingLocation.Daily);

                return this.current;
            }
            set => this.current = value;
        }

        public VillageBuilding CurrentDefault
        {
            get
            {
                if (this.currentDefault == null)
                    this.currentDefault = this.buildings.FirstOrDefault(x => x.BuildingType.StringId == "bannerkings_daily_production");

                return this.currentDefault;
            }
            set => this.currentDefault = value;
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
        private PopulationData data;
        private float acres;
        private float farmland;
        private float pasture;
        private float woodland;
        private float fertility;
        private float terrainDifficulty;
        TerrainType terrainType;

        public LandData(PopulationData data)
        {
            this.data = data;
            this.terrainType = Campaign.Current.MapSceneWrapper.GetTerrainTypeAtPosition(data.Settlement.Position2D);
            this.Init(data.TotalPop);
        }

        private void Init(int totalPops)
        {
            float farmRatio = 0f;
            float pastureRatio = 0f;
            float woodRatio = 0f;
            if (this.terrainType == TerrainType.Desert)
            {
                this.fertility = 0.5f;
                this.terrainDifficulty = 1.4f;
                farmRatio = 0.9f;
                pastureRatio = 0.08f;
                woodRatio = 0.02f;
            }
            else if (this.terrainType == TerrainType.Steppe)
            {
                this.fertility = 0.75f;
                this.terrainDifficulty = 1f;
                farmRatio = 0.45f;
                pastureRatio = 0.5f;
                woodRatio = 0.05f;
            }
            else if (this.terrainType == TerrainType.Mountain)
            {
                this.fertility = 0.7f;
                this.terrainDifficulty = 2f;
                farmRatio = 0.5f;
                pastureRatio = 0.35f;
                woodRatio = 0.15f;
            }
            else if (this.terrainType == TerrainType.Canyon)
            {
                this.fertility = 0.5f;
                this.terrainDifficulty = 2f;
                farmRatio = 0.9f;
                pastureRatio = 0.08f;
                woodRatio = 0.02f;
            }
            else if (this.terrainType == TerrainType.Forest)
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
        }

        public int AvailableWorkForce
        {
            get
            {
                float serfs = this.data.GetTypeCount(PopType.Serfs) * 0.5f;
                float slaves = this.data.GetTypeCount(PopType.Slaves);
                return (int)(serfs + slaves);
            }
        }

        public float Farmland => this.farmland;
        public float Pastureland => this.pasture;
        public float Woodland => this.woodland;

        internal override void Update(PopulationData data)
        {
            
        }
    }

    public class TournamentData : BannerKingsData
    {
        private Town town;
        private ItemRoster roster;
        private ItemObject prize;
        private bool active;

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
