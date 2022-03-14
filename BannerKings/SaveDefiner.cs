using BannerKings.Components;
using BannerKings.Managers;
using BannerKings.Managers.Court;
using BannerKings.Managers.Decisions;
using BannerKings.Managers.Policies;
using BannerKings.Managers.Populations;
using BannerKings.Managers.Populations.Villages;
using BannerKings.Populations;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;
using static BannerKings.Managers.Policies.BKCriminalPolicy;
using static BannerKings.Managers.Policies.BKMilitiaPolicy;
using static BannerKings.Managers.Policies.BKTariffPolicy;
using static BannerKings.Managers.Policies.BKTaxPolicy;
using static BannerKings.Managers.Policies.BKWorkforcePolicy;
using static BannerKings.Managers.PopulationManager;
using static BannerKings.Managers.TitleManager;

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
            base.AddClassDefinition(typeof(BannerKingsDecision), 8);
            base.AddClassDefinition(typeof(BannerKingsPolicy), 9);
            base.AddEnumDefinition(typeof(TaxType), 10);
            base.AddEnumDefinition(typeof(MilitiaPolicy), 11);
            base.AddEnumDefinition(typeof(WorkforcePolicy), 12);
            base.AddClassDefinition(typeof(PopulationManager), 13);
            base.AddClassDefinition(typeof(PolicyManager), 14);
            base.AddClassDefinition(typeof(PopulationPartyComponent), 15);
            base.AddClassDefinition(typeof(MilitiaComponent), 16);
            base.AddEnumDefinition(typeof(TariffType), 17);
            base.AddEnumDefinition(typeof(CriminalPolicy), 18);
            base.AddClassDefinition(typeof(TournamentData), 19);
            base.AddClassDefinition(typeof(VillageData), 20); 
            base.AddClassDefinition(typeof(VillageBuilding), 21);
            base.AddClassDefinition(typeof(CultureDataClass), 22);
            base.AddClassDefinition(typeof(FeudalTitle), 23); 
            base.AddClassDefinition(typeof(FeudalContract), 24);
            base.AddEnumDefinition(typeof(TitleType), 25);
            base.AddEnumDefinition(typeof(FeudalDuties), 26);
            base.AddEnumDefinition(typeof(FeudalRights), 27);
            base.AddEnumDefinition(typeof(GovernmentType), 28);
            base.AddEnumDefinition(typeof(SuccessionType), 29);
            base.AddEnumDefinition(typeof(InheritanceType), 30);
            base.AddEnumDefinition(typeof(GenderLaw), 31);
            base.AddClassDefinition(typeof(TitleManager), 32);
            base.AddEnumDefinition(typeof(CouncilPosition), 33);
            base.AddClassDefinition(typeof(CouncilMember), 34);
            base.AddClassDefinition(typeof(CouncilData), 35);
            base.AddClassDefinition(typeof(CourtManager), 36);
        }

        protected override void DefineContainerDefinitions()
        {
            base.ConstructContainerDefinition(typeof(List<PopulationClass>));
            base.ConstructContainerDefinition(typeof(List<VillageBuilding>));
            base.ConstructContainerDefinition(typeof(List<CultureDataClass>));
            base.ConstructContainerDefinition(typeof(Dictionary<Settlement, PopulationData>));
            base.ConstructContainerDefinition(typeof(List<BannerKingsDecision>));
            base.ConstructContainerDefinition(typeof(List<BannerKingsPolicy>));
            base.ConstructContainerDefinition(typeof(Dictionary<Settlement, List<BannerKingsPolicy>>));
            base.ConstructContainerDefinition(typeof(Dictionary<Settlement, List<BannerKingsDecision>>));
            base.ConstructContainerDefinition(typeof(Dictionary<FeudalTitle, Hero>));
            base.ConstructContainerDefinition(typeof(Dictionary<Kingdom, FeudalTitle>));
            base.ConstructContainerDefinition(typeof(List<FeudalTitle>));
            base.ConstructContainerDefinition(typeof(Dictionary<FeudalDuties, float>));
            base.ConstructContainerDefinition(typeof(List<FeudalRights>));
            base.ConstructContainerDefinition(typeof(Dictionary<Clan, CouncilData>));
            base.ConstructContainerDefinition(typeof(List<CouncilMember>));

        }
    }
}
