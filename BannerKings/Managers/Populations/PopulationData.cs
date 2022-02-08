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

        public PopulationData(List<PopulationClass> classes, Settlement settlement, float assimilation, HashSet<CultureDataClass> cultures = null, Guild guild = null)
        {
            this.classes = classes;
            this.stability = 0.5f;
            this.settlement = settlement;
            this.economicData = new EconomicData(settlement, guild);

            if (cultures == null)
            {
                CultureDataClass cultureData = new CultureDataClass(settlement.Culture, 1f, 1f);
                cultures = new HashSet<CultureDataClass>();
                cultures.Add(cultureData);
            }
            this.cultureData = new CultureData(settlement.Owner, cultures);
            float total = TotalPop;
            float nobles = classes.First(x => x.type == PopType.Nobles).count;
            this.militaryData = new MilitaryData(settlement, (int)(total * 0.04f), (int)(nobles * 0.08f));
            this.landData = new LandData();
        }

        public CultureData CultureData => this.cultureData;
        public MilitaryData MilitaryData => this.militaryData;
        public LandData LandData => this.landData;
        public EconomicData EconomicData => this.economicData;


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
        private HashSet<CultureDataClass> cultures;
        private Hero settlementOwner;

        public CultureData(Hero settlementOwner, HashSet<CultureDataClass> cultures)
        {
            this.settlementOwner = settlementOwner;
            this.cultures = cultures;
        }

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
            CultureDataClass data = this.cultures.FirstOrDefault(x => x.Culture == settlementOwner.Culture);
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
            foreach (CultureDataClass cultureData in this.cultures)
            {
                //cultureData.Acceptance += accModel.CalculateEffect(data.Settlement, cultureData).ResultNumber;
                cultureData.Assimilation += assimModel.CalculateEffect(data.Settlement, cultureData).ResultNumber;
                if (cultureData.Culture != settlementOwner.Culture && cultureData.Assimilation == 0f)
                    toDelete.Add(cultureData);
            }

            foreach (CultureDataClass cultureData in toDelete)
                this.cultures.Remove(cultureData);
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

        public EconomicData(Settlement settlement,
            Guild guild = null)
        {
            this.settlement = settlement;
            this.guild = guild;
            this.satisfactions = new float[] { 0.5f, 0.5f, 0.5f,0.5f };
        }

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
        public ExplainedNumber Mercantilism
        {
            get
            {
                BKEconomyModel model = (BKEconomyModel)BannerKingsConfig.Instance.Models.First(x => x.GetType() == typeof(IEconomyModel));
                return model.CalculateEffect(settlement);
            }
        }
        public ExplainedNumber ProductionEfficiency
        {
            get
            {
                BKEconomyModel model = (BKEconomyModel)BannerKingsConfig.Instance.Models.First(x => x.GetType() == typeof(IEconomyModel));
                return model.CalculateProductionEfficiency(settlement);
            }
        }
        public ExplainedNumber ProductionQuality
        {
            get
            {
                BKEconomyModel model = (BKEconomyModel)BannerKingsConfig.Instance.Models.First(x => x.GetType() == typeof(IEconomyModel));
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


    public class LandData : BannerKingsData
    {
        private float acres;
        private float farmland;
        private float pasture;
        private float woodland;

        internal override void Update(PopulationData data)
        {
           
        }
    }

    public class Guild
    {
        private Hero guildMaster;
        private int capital;

    }

    public enum GuildType
    {
        Merchant_Guild,
        Masonry_Guild,

    }
}
