using System;
using System.Collections.Generic;
using System.Linq;
using BannerKings.Models;
using BannerKings.Models.Vanilla;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.SaveSystem;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Managers.Populations
{
    public class MilitaryData : BannerKingsData
    {
        public MilitaryData(Settlement settlement, int peasantManpower, int nobleManpower)
        {
            this.settlement = settlement;
            this.peasantManpower = peasantManpower;
            this.nobleManpower = nobleManpower;
            engines = new List<SiegeEngineType>();
        }

        [SaveableProperty(1)] private Settlement settlement { get; }

        [SaveableProperty(2)] private int peasantManpower { get; set; }

        [SaveableProperty(3)] private int nobleManpower { get; set; }

        [SaveableProperty(4)] private List<SiegeEngineType> engines { get; }

        public int Manpower => peasantManpower + nobleManpower;
        public int PeasantManpower => peasantManpower;
        public int NobleManpower => nobleManpower;

        public ExplainedNumber DraftEfficiency
        {
            get
            {
                var number = new ExplainedNumber(0f);
                if (settlement.Notables != null && settlement.Notables.Count > 0)
                {
                    number = new BKVolunteerModel().GetDraftEfficiency(settlement.Notables[0], 2, settlement);
                }

                return number;
            }
        }

        public ExplainedNumber Militarism => new BKVolunteerModel().GetMilitarism(settlement);

        public int Holdout => new BKFoodModel().GetFoodEstimate(settlement, settlement.Town.FoodStocksUpperLimit());

        public IEnumerable<SiegeEngineType> Engines => engines;

        public int Ballistae => new BKSiegeEventModel().GetPrebuiltSiegeEnginesOfSettlement(settlement)
            .Count(x => x == DefaultSiegeEngineTypes.Ballista);

        public int Catapultae => new BKSiegeEventModel().GetPrebuiltSiegeEnginesOfSettlement(settlement)
            .Count(x => x == DefaultSiegeEngineTypes.Catapult);

        public int Trebuchets => new BKSiegeEventModel().GetPrebuiltSiegeEnginesOfSettlement(settlement)
            .Count(x => x == DefaultSiegeEngineTypes.Trebuchet);

        public void DeduceManpower(PopulationData data, int quantity, CharacterObject troop)
        {
            var tier = troop.Tier;
            var noble = Utils.Helpers.IsRetinueTroop(troop, settlement.Culture);
            if (noble)
            {
                if (nobleManpower >= quantity)
                {
                    nobleManpower -= quantity;
                }
                else
                {
                    nobleManpower = 0;
                }

                data.UpdatePopType(PopType.Nobles, -quantity);
            }
            else
            {
                if (tier >= 3 && data.GetTypeCount(PopType.Craftsmen) > quantity)
                {
                    var list = new List<(PopType, float)>();
                    var mil1 = data.GetCurrentTypeFraction(PopType.Craftsmen);
                    list.Add(new ValueTuple<PopType, float>(PopType.Craftsmen, mil1));
                    var mil2 = data.GetCurrentTypeFraction(PopType.Serfs);
                    list.Add(new ValueTuple<PopType, float>(PopType.Serfs, mil2));
                    var type = MBRandom.ChooseWeighted(list);
                    data.UpdatePopType(type, -quantity);
                }
                else
                {
                    data.UpdatePopType(PopType.Serfs, -quantity);
                }

                if (peasantManpower >= quantity)
                {
                    peasantManpower -= quantity;
                }
                else
                {
                    peasantManpower = 0;
                }
            }

            nobleManpower = Math.Max(nobleManpower, 0);
            peasantManpower = Math.Max(peasantManpower, 0);
        }

        internal override void Update(PopulationData data)
        {
            var model = new BKVolunteerModel();
            var serfMilitarism = model.GetClassMilitarism(PopType.Serfs);
            float serfs = data.GetTypeCount(PopType.Serfs);

            var craftsmanMilitarism = model.GetClassMilitarism(PopType.Craftsmen);
            float craftsmen = data.GetTypeCount(PopType.Craftsmen);
            var peasantCap = (int) (serfs * serfMilitarism + craftsmen * craftsmanMilitarism);
            var peasantGrowth = (int) (data.Growth.ResultNumber * (serfMilitarism + craftsmanMilitarism));
            if (peasantGrowth == 0)
            {
                peasantGrowth++;
            }

            if (peasantManpower > peasantCap)
            {
                peasantManpower += (int) (peasantGrowth * -1f);
            }
            else if (peasantManpower < peasantCap)
            {
                peasantManpower += peasantGrowth;
            }

            var nobleMilitarism = model.GetClassMilitarism(PopType.Nobles);
            float nobles = data.GetTypeCount(PopType.Nobles);
            var nobleCap = (int) (nobles * nobleMilitarism);
            var nobleGrowth = (int) (data.Growth.ResultNumber * nobleMilitarism);
            if (nobleGrowth == 0)
            {
                nobleGrowth++;
            }

            if (nobleManpower > nobleCap)
            {
                nobleManpower += (int) (nobleGrowth * -1f);
            }
            else if (nobleManpower < nobleCap)
            {
                nobleManpower += nobleGrowth;
            }
        }
    }
}