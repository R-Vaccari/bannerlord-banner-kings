using BannerKings.Components;
using BannerKings.Managers;
using BannerKings.Populations;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;
using static BannerKings.Managers.PolicyManager;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings
{
    class SaveDefiner : SaveableTypeDefiner
    {

        public SaveDefiner() : base(82818189)
        {

        }

        protected override void DefineClassTypes()
        {
            base.AddEnumDefinition(typeof(PopType), 1);
            base.AddClassDefinition(typeof(PopulationClass), 2);
            base.AddClassDefinition(typeof(MilitaryData), 3);
            base.AddClassDefinition(typeof(CultureData), 4);
            base.AddClassDefinition(typeof(EconomicData), 5);
            base.AddClassDefinition(typeof(LandData), 6);
            base.AddClassDefinition(typeof(PopulationData), 7);
            base.AddEnumDefinition(typeof(PolicyType), 8);
            base.AddClassDefinition(typeof(PolicyElement), 9);
            base.AddEnumDefinition(typeof(TaxType), 10);
            base.AddEnumDefinition(typeof(MilitiaPolicy), 11);
            base.AddEnumDefinition(typeof(WorkforcePolicy), 12);
            base.AddClassDefinition(typeof(PopulationManager), 13);
            base.AddClassDefinition(typeof(PolicyManager), 14);
            base.AddClassDefinition(typeof(PopulationPartyComponent), 15);
            base.AddClassDefinition(typeof(MilitiaComponent), 16);
            base.AddEnumDefinition(typeof(TariffType), 17);
            base.AddEnumDefinition(typeof(CriminalPolicy), 18);
        }

        protected override void DefineContainerDefinitions()
        {
            base.ConstructContainerDefinition(typeof(List<PopulationClass>));
            base.ConstructContainerDefinition(typeof(Dictionary<Settlement, PopulationData>));
            base.ConstructContainerDefinition(typeof(List<PolicyElement>));
            base.ConstructContainerDefinition(typeof(Dictionary<Settlement, List<PolicyElement>>));
            base.ConstructContainerDefinition(typeof(Dictionary<Settlement, TaxType>));
            base.ConstructContainerDefinition(typeof(Dictionary<Settlement, MilitiaPolicy>));
            base.ConstructContainerDefinition(typeof(Dictionary<Settlement, WorkforcePolicy>));
            base.ConstructContainerDefinition(typeof(Dictionary<Settlement, TariffType>));
        }
    }
}
