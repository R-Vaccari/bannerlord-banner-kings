using BannerKings.Models;
using BannerKings.Models.Vanilla;
using BannerKings.Populations;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.SaveSystem;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Managers.Populations
{
    public class MilitaryData : BannerKingsData
    {
        [SaveableProperty(1)]
        private Settlement settlement { get; set; }

        [SaveableProperty(2)]
        private int peasantManpower { get; set; }

        [SaveableProperty(3)]
        private int nobleManpower { get; set; }

        [SaveableProperty(4)]
        private List<SiegeEngineType> engines { get; set; }

        public MilitaryData(Settlement settlement, int peasantManpower, int nobleManpower)
        {
            this.settlement = settlement;
            this.peasantManpower = peasantManpower;
            this.nobleManpower = nobleManpower;
            this.engines = new List<SiegeEngineType>();
        }

        public void DeduceManpower(PopulationData data, int quantity, CharacterObject troop)
        {
            int tier = troop.Tier;
            bool noble = BannerKings.Helpers.Helpers.IsRetinueTroop(troop, settlement.Culture);
            if (noble)
            {
                if (this.nobleManpower >= quantity) this.nobleManpower -= quantity;
                else this.nobleManpower = 0;
                data.UpdatePopType(PopType.Nobles, -quantity);
            }
            else
            {
                if (tier >= 3 && data.GetTypeCount(PopType.Craftsmen) > quantity)
                {
                    List<ValueTuple<PopType, float>> list = new List<(PopType, float)>();
                    float mil1 = data.GetCurrentTypeFraction(PopType.Craftsmen);
                    list.Add(new(PopType.Craftsmen, mil1));
                    float mil2 = data.GetCurrentTypeFraction(PopType.Serfs);
                    list.Add(new(PopType.Serfs, mil2));
                    PopType type = MBRandom.ChooseWeighted(list);
                    data.UpdatePopType(type, -quantity);
                }
                else
                {
                    data.UpdatePopType(PopType.Serfs, -quantity);
                }
                if (this.peasantManpower >= quantity) this.peasantManpower -= quantity;
                else this.peasantManpower = 0;
            }
        }

        public int Manpower => peasantManpower + nobleManpower;
        public int PeasantManpower => peasantManpower;
        public int NobleManpower => nobleManpower;
        public ExplainedNumber DraftEfficiency
        {
            get
            {
                ExplainedNumber number = new ExplainedNumber(0f);
                if (settlement.Notables != null && settlement.Notables.Count > 0)
                    number = new BKVolunteerModel().GetDraftEfficiency(settlement.Notables[0], 2, settlement);

                return number;
            }
        }
            
        public ExplainedNumber Militarism => new BKVolunteerModel().GetMilitarism(settlement);

        public int Holdout => new BKFoodModel().GetFoodEstimate(settlement, settlement.Town.FoodStocksUpperLimit());

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
            int peasantGrowth = (int)(data.Growth.ResultNumber * (serfMilitarism + craftsmanMilitarism));
            if (peasantGrowth == 0) peasantGrowth++;
            if (peasantManpower > peasantCap)
                this.peasantManpower += (int)((float)peasantGrowth * -1f);
            else if (peasantManpower < peasantCap)
                this.peasantManpower += peasantGrowth;

            float nobleMilitarism = model.GetClassMilitarism(PopType.Nobles);
            float nobles = data.GetTypeCount(PopType.Nobles);
            int nobleCap = (int)(nobles * nobleMilitarism);
            int nobleGrowth = (int)(data.Growth.ResultNumber * nobleMilitarism);
            if (nobleGrowth == 0) nobleGrowth++;
            if (nobleManpower > nobleCap)
                this.nobleManpower += (int)((float)nobleGrowth * -1f);
            else if (nobleManpower < nobleCap)
                this.nobleManpower += nobleGrowth;
        }
    }
}
