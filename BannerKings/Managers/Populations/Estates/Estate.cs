using BannerKings.Managers.Recruits;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Managers.Populations.Estates
{
    public class Estate
    {
        public Estate(Hero owner, EstateData data, float farmland, float pastureland, float woodland,
            int population, int slaves)
        {
            Owner = owner;
            Farmland = farmland;
            Pastureland = pastureland;
            Woodland = woodland;
            Population = population;
            Slaves = slaves;
            EstatesData = data;
            TroopRoster = TroopRoster.CreateDummyTroopRoster();
        }

        public static Estate CreateNotableEstate(Hero notable, PopulationData data, EstateData estateData= null)
        {
            if (data == null || data.LandData == null)
            {
                return null;
            }

            float acreage = data.LandData.Acreage;
            float acres = MBRandom.RandomFloatRanged(BannerKingsConfig.Instance.EstatesModel.MinimumEstateAcreage,
                BannerKingsConfig.Instance.EstatesModel.MaximumEstateAcreagePercentage * acreage);
            var composition = data.LandData.Composition;
            float farmland = acres * composition[0];
            float pastureland = acres * composition[1];
            float woodland = acres * composition[2];

            float popReference = data.GetTypeCount(PopType.Tenants) + data.GetTypeCount(PopType.Serfs);
            float totalSlaves = data.GetTypeCount(PopType.Slaves) * (1f - data.EconomicData.StateSlaves);

            int desiredWorkforce = (int)(acres / 5f);
            float desiredAddPopulation = (int)(desiredWorkforce * 0.8f);
            float desiredSlaves = (int)(desiredWorkforce * 0.2f);

            var result = new Estate(notable, 
                estateData != null ? estateData : data.EstateData, 
                farmland, 
                pastureland, 
                woodland,
                (int)MathF.Min(desiredAddPopulation, popReference * 0.15f),
                (int)MathF.Min(desiredSlaves, totalSlaves * 0.25f));

            return result;
        }

        public TextObject Name => Owner != null ? new TextObject("{=pKtOLvPi}Estate of {OWNER}").SetTextVariable("OWNER", Owner.Name) : new TextObject();

        public void SetOwner(Hero newOnwer)
        {
            BannerKingsConfig.Instance.PopulationManager.ChangeEstateOwner(this, newOnwer);
            Owner = newOnwer;
            if (newOnwer == Hero.MainHero)
            {
                MBInformationManager.AddQuickInformation(new TextObject("{=U6bVmS8Z}You are now the owner of an estate at {SETTLEMENT}")
                    .SetTextVariable("SETTLEMENT", EstatesData.Settlement.Name),
                    0,
                    null,
                    Utils.Helpers.GetRelationDecisionSound());
            }
        }

        public ExplainedNumber TaxRatio => BannerKingsConfig.Instance.EstatesModel.GetTaxRatio(this, true);
        public bool IsDisabled => Owner == null;
        public ExplainedNumber AcrePriceExplained => BannerKingsConfig.Instance.EstatesModel.CalculateAcrePrice(EstatesData.Settlement, true);
        public ExplainedNumber EstateValue => BannerKingsConfig.Instance.EstatesModel.CalculateEstatePrice(this, true);
        public ExplainedNumber AcreageGrowth => Task == EstateTask.Land_Expansion ? BannerKingsConfig.Instance.ConstructionModel
            .CalculateLandExpansion(BannerKingsConfig.Instance.PopulationManager.GetPopData(EstatesData.Settlement),
            LandExpansionWorkforce) : new ExplainedNumber(0f);
        public ExplainedNumber Production => BannerKingsConfig.Instance.EstatesModel.CalculateEstateProduction(this, true);
        public ExplainedNumber PopulationCapacity => BannerKingsConfig.Instance.GrowthModel.CalculateEstateCap(this, false);
        public ExplainedNumber PopulationCapacityExplained => BannerKingsConfig.Instance.GrowthModel.CalculateEstateCap(this, true);
        public int MaxManpower => (int)(Population * BannerKingsConfig.Instance.VolunteerModel.GetMilitarism(EstatesData.Settlement).ResultNumber);
        public int Income => (int)(TaxAccumulated * 0.8f);
        public int AvailableWorkForce
        {
            get
            {
                int toSubtract = 0;
                if (Task == EstateTask.Land_Expansion)
                {
                    toSubtract += LandExpansionWorkforce;
                }

                return Population + Slaves - toSubtract;
            }
        }

        public int LandExpansionWorkforce => (int)((Population * 0.5f) + Slaves);

        public float WorkforceSaturation
        {
            get
            {
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(EstatesData.Settlement);
                float available = AvailableWorkForce;
                var farms = Farmland / data.LandData.GetRequiredLabor("farmland");
                var pasture = Pastureland / data.LandData.GetRequiredLabor("pasture");
                return available / (farms + pasture);
            }
        }

        public float Influence => 0;

        public float Acreage => Farmland + Pastureland + Woodland;

        [SaveableProperty(1)] public Hero Owner { get; private set; }
        [SaveableProperty(2)] public EstateData EstatesData { get; private set; }
        [SaveableProperty(3)] public float Farmland { get; private set; }
        [SaveableProperty(4)] public float Pastureland { get; private set; }
        [SaveableProperty(5)] public float Woodland { get; private set; }
        [SaveableProperty(6)] public int TaxAccumulated { get; set; } = 0;

        [SaveableProperty(8)] public int Population { get; private set; }
        [SaveableProperty(9)] public int Slaves { get; private set; }
     
        public void ChangeTask(EstateTask task) => Task = task;
        public void ChangeDuty(EstateDuty duty) => Duty = duty;

        [SaveableProperty(10)] public EstateDuty Duty { get; private set; }
        [SaveableProperty(11)] public EstateTask Task { get; private set; }
        [SaveableProperty(12)] public TroopRoster TroopRoster { get; private set; }
        [SaveableProperty(13)] public int LastIncome { get; set; }

        public void SpawnParty()
        {

        }

        public void PostInitialize()
        {
            if (TroopRoster == null) TroopRoster = TroopRoster.CreateDummyTroopRoster();
        }

        public TroopRoster RaiseManpower(int limit)
        {
            TroopRoster roster = TroopRoster.CreateDummyTroopRoster();
            CultureObject culture = EstatesData.Settlement.Culture;
            roster.AddToCounts(culture.BasicTroop, (int)(limit / 2f));

            var upgrades = culture.BasicTroop.UpgradeTargets;
            if (upgrades != null && upgrades.Count() > 0)
            {
                for (int i = 0; i < upgrades.Count(); i++)
                {
                    int toAdd = (int)(limit * 0.5f / upgrades.Count());
                    roster.AddToCounts(upgrades[i], toAdd);
                }
            }

            return roster;
        }

        public void Tick(PopulationData data)
        {
            if (TroopRoster == null) TroopRoster = TroopRoster.CreateDummyTroopRoster(); 

            if (TroopRoster.TotalManCount < MaxManpower)
            {
                float tenantProportion = data.GetCurrentTypeFraction(PopType.Tenants);
                float serfProportion = data.GetCurrentTypeFraction(PopType.Serfs);
                foreach (var spawn in DefaultRecruitSpawns.Instance.GetPossibleSpawns(data.Settlement.Culture, data.Settlement))
                {
                    float random = MBRandom.RandomFloat;
                    if (random * tenantProportion < spawn.GetChance(PopType.Tenants))
                    {
                        TroopRoster.AddToCounts(spawn.Troop, 1);
                        break;
                    }
                    else if (random * serfProportion < spawn.GetChance(PopType.Serfs))
                    {
                        TroopRoster.AddToCounts(spawn.Troop, 1);
                        break;
                    }
                }
            }

            if (IsDisabled) return;

            float maxFarmland = data.LandData.Farmland * 0.2f;
            Farmland = MathF.Clamp(Farmland, 0f, maxFarmland);

            float maxPastureland= data.LandData.Pastureland * 0.2f;
            Pastureland = MathF.Clamp(Pastureland, 0f, maxPastureland);

            float maxWoodland = data.LandData.Woodland * 0.2f;
            Woodland = MathF.Clamp(Woodland, 0f, maxWoodland);

            Population = (int)MathF.Clamp(Population, 0f, PopulationCapacity.ResultNumber);
            BannerKingsConfig.Instance.PopulationManager.AddEstate(this);
            if (Task == EstateTask.Land_Expansion)
            {
                var progress = AcreageGrowth.ResultNumber;
                if (progress > 0f)
                {
                    var composition = data.LandData.Composition;
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
                            Farmland += progress;
                            break;
                        case 1:
                            Pastureland += progress;
                            break;
                        default:
                            Woodland += progress;
                            break;
                    }
                }
            }
        }

        public void AddPopulation(int toAdd)
        {
            Population += toAdd;
        }

        public enum EstateDuty
        {
            Taxation,
            Military
        }

        public enum EstateTask
        {
            Prodution,
            Land_Expansion,
            Military
        }
    }
}
