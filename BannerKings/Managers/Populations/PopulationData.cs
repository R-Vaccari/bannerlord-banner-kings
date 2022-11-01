using System;
using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers.Institutions.Guilds;
using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Populations.Estates;
using BannerKings.Managers.Titles;
using BannerKings.Models.BKModels;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.SaveSystem;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Managers.Populations
{
    public class PopulationData : BannerKingsData
    {
        public PopulationData(List<PopulationClass> classes, Settlement settlement, float assimilation, List<CultureDataClass> cultures = null, Guild guild = null)
        {
            this.classes = classes;
            stability = 0.5f;
            this.settlement = settlement;
            economicData = new EconomicData(settlement, guild);

            if (cultures == null)
            {
                var cultureData = new CultureDataClass(settlement.Culture, 1f, 1f);
                cultures = new List<CultureDataClass> {cultureData};
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

        [SaveableProperty(3)] private Settlement settlement { get; set; }

        [SaveableProperty(4)] private CultureData cultureData { get; set; }

        [SaveableProperty(5)] private VillageData villageData { get; set; }

        [SaveableProperty(6)] private LandData landData { get; set; }

        [SaveableProperty(7)] private MilitaryData militaryData { get; set; }

        [SaveableProperty(8)] private EconomicData economicData { get; set; }

        [SaveableProperty(9)] private TournamentData tournamentData { get; set; }

        [SaveableProperty(10)] private TitleData titleData { get; set; }

        [SaveableProperty(11)] private float autonomy { get; set; }

        [SaveableProperty(12)] private ReligionData religionData { get; set; }

        [SaveableProperty(13)] private MineralData mineralData { get; set; }

        [SaveableProperty(14)] public EstateData EstateData { get; private set; }

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
        public MineralData MineralData => mineralData;

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
                var model = (BKGrowthModel) BannerKingsConfig.Instance.Models.First(x => x.GetType() == typeof(BKGrowthModel));
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

            bool excessNobles = currentDic[PopType.Nobles] > dic[PopType.Nobles][1];

            if (currentDic[PopType.Craftsmen] > dic[PopType.Craftsmen][1])
            {
                var random = MBMath.ClampInt(MBRandom.RandomInt(0, 25), 0, GetTypeCount(PopType.Craftsmen));
                UpdatePopType(PopType.Craftsmen, -random);
                if (!excessNobles)
                {
                    UpdatePopType(PopType.Nobles, random);
                }
                else
                {
                    UpdatePopType(PopType.Serfs, random);
                }
            }

            if (excessNobles)
            {
                var random = MBMath.ClampInt(MBRandom.RandomInt(0, 25), 0, GetTypeCount(PopType.Nobles));
                UpdatePopType(PopType.Craftsmen, random);
                UpdatePopType(PopType.Nobles, -random);
            }
        }

        private void SelectAndUpdatePop(Settlement settlement, int pops)
        {
            if (pops != 0)
            {
                var desiredTypes = GetDesiredPopTypes(settlement);
                var typesList = new List<ValueTuple<PopType, float>>();


                classes.ForEach(popClass =>
                {
                    var type = popClass.type;
                    switch (pops)
                    {
                        case < 0 when popClass.count >= pops:
                        {
                            var hasExcess = GetCurrentTypeFraction(type) > desiredTypes[type][1];
                            typesList.Add(new ValueTuple<PopType, float>(popClass.type,
                                (float) popClass.type * 5f + desiredTypes[type][0] * (hasExcess ? 2f : 1f)));
                            break;
                        }
                        case > 0:
                        {
                            var isLacking = GetCurrentTypeFraction(type) < desiredTypes[type][0];
                            typesList.Add(new ValueTuple<PopType, float>(popClass.type,
                                desiredTypes[type][0] * (isLacking ? 2f : 1f)));
                            break;
                        }
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
            var stabilityModel = (BKStabilityModel) BannerKingsConfig.Instance.Models.First(x => x.GetType() == typeof(BKStabilityModel));
            Stability += stabilityModel.CalculateEffect(settlement).ResultNumber;
            Autonomy += stabilityModel.CalculateAutonomyEffect(settlement, Stability, Autonomy).ResultNumber;

            economicData.Update(this);
            cultureData.Update(this);
            militaryData.Update(this);
            landData.Update(this);
            villageData?.Update(this);
            tournamentData?.Update(this);

            titleData ??= new TitleData(BannerKingsConfig.Instance.TitleManager.GetTitle(settlement));
            titleData.Update(this);

            if (mineralData == null)
            {
                mineralData = new MineralData(this);
            }

            mineralData.Update(this);

            if (religionData == null)
            {
                var religion = BannerKingsConfig.Instance.ReligionsManager.GetIdealReligion(settlement.Culture);
                if (religion != null)
                {
                    religionData = new ReligionData(religion, settlement);
                }
            }

            religionData?.Update(this);

            if (EstateData == null && BannerKingsConfig.Instance.EstatesModel.CalculateEstatesMaximum(Settlement).ResultNumber > 0)
            {
                EstateData = new EstateData(Settlement);
            }

            EstateData?.Update(this);
        }
    }
}