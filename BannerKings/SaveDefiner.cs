using BannerKings.Components;
using BannerKings.Managers;
using BannerKings.Managers.Court;
using BannerKings.Managers.Decisions;
using BannerKings.Managers.Duties;
using BannerKings.Managers.Kingdoms;
using BannerKings.Managers.Kingdoms.Contract;
using BannerKings.Managers.Policies;
using BannerKings.Managers.Populations;
using BannerKings.Managers.Populations.Tournament;
using BannerKings.Managers.Populations.Villages;
using BannerKings.Managers.Titles;
using BannerKings.Populations;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;
using static BannerKings.Managers.Policies.BKCriminalPolicy;
using static BannerKings.Managers.Policies.BKDraftPolicy;
using static BannerKings.Managers.Policies.BKGarrisonPolicy;
using static BannerKings.Managers.Policies.BKMilitiaPolicy;
using static BannerKings.Managers.Policies.BKTaxPolicy;
using static BannerKings.Managers.Policies.BKWorkforcePolicy;
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
            AddEnumDefinition(typeof(PopType), 1);
            AddClassDefinition(typeof(PopulationClass), 2);
            AddClassDefinition(typeof(MilitaryData), 3);
            AddClassDefinition(typeof(CultureData), 4);
            AddClassDefinition(typeof(EconomicData), 5);
            AddClassDefinition(typeof(LandData), 6);
            AddClassDefinition(typeof(PopulationData), 7);
            AddClassDefinition(typeof(BannerKingsDecision), 8);
            AddClassDefinition(typeof(BannerKingsPolicy), 9);
            AddEnumDefinition(typeof(TaxType), 10);
            AddEnumDefinition(typeof(MilitiaPolicy), 11);
            AddEnumDefinition(typeof(WorkforcePolicy), 12);
            AddClassDefinition(typeof(PopulationManager), 13);
            AddClassDefinition(typeof(PolicyManager), 14);
            AddClassDefinition(typeof(PopulationPartyComponent), 15);
            AddClassDefinition(typeof(MilitiaComponent), 16);
            AddEnumDefinition(typeof(GarrisonPolicy), 17);
            AddEnumDefinition(typeof(CriminalPolicy), 18);
            AddClassDefinition(typeof(TournamentData), 19);
            AddClassDefinition(typeof(VillageData), 20); 
            AddClassDefinition(typeof(VillageBuilding), 21);
            AddClassDefinition(typeof(CultureDataClass), 22);
            AddClassDefinition(typeof(FeudalTitle), 23); 
            AddClassDefinition(typeof(FeudalContract), 24);
            AddEnumDefinition(typeof(TitleType), 25);
            AddEnumDefinition(typeof(FeudalDuties), 26);
            AddEnumDefinition(typeof(FeudalRights), 27);
            AddEnumDefinition(typeof(GovernmentType), 28);
            AddEnumDefinition(typeof(SuccessionType), 29);
            AddEnumDefinition(typeof(InheritanceType), 30);
            AddEnumDefinition(typeof(GenderLaw), 31);
            AddClassDefinition(typeof(TitleManager), 32);
            AddEnumDefinition(typeof(CouncilPosition), 33);
            AddClassDefinition(typeof(CouncilMember), 34);
            AddClassDefinition(typeof(CouncilData), 35);
            AddClassDefinition(typeof(CourtManager), 36);
            AddEnumDefinition(typeof(DraftPolicy), 37);
            AddClassDefinition(typeof(BKCriminalPolicy), 38);
            AddClassDefinition(typeof(BKDraftPolicy), 39);
            AddClassDefinition(typeof(BKGarrisonPolicy), 40);
            AddClassDefinition(typeof(BKMilitiaPolicy), 41);
            AddClassDefinition(typeof(BKTaxPolicy), 42);
            AddClassDefinition(typeof(BKWorkforcePolicy), 43);
            AddClassDefinition(typeof(BKRationDecision), 44);
            AddClassDefinition(typeof(BKExportSlavesDecision), 45);
            AddClassDefinition(typeof(BKTaxSlavesDecision), 46);
            AddClassDefinition(typeof(BKEncourageMilitiaDecision), 47);
            AddClassDefinition(typeof(BKSubsidizeMilitiaDecision), 48);
            AddClassDefinition(typeof(BKExemptTariffDecision), 49);
            AddClassDefinition(typeof(BKEncourageMercantilism), 50);
            AddClassDefinition(typeof(BannerKingsDuty), 51);
            AddClassDefinition(typeof(AuxiliumDuty), 52);
            AddClassDefinition(typeof(RansomDuty), 53);
            AddClassDefinition(typeof(BannerKingsTournament), 54);
            AddClassDefinition(typeof(BKContractDecision), 55);
            AddClassDefinition(typeof(BKGenderDecision), 56);
            AddClassDefinition(typeof(BKInheritanceDecision), 57);
            AddClassDefinition(typeof(BKSuccessionDecision), 58);
            AddClassDefinition(typeof(BKGovernmentDecision), 59);
            AddClassDefinition(typeof(RepublicElectionDecision), 60);
            AddClassDefinition(typeof(BKSettlementClaimantDecision), 61);
            AddClassDefinition(typeof(BKKingElectionDecision), 62);
            AddClassDefinition(typeof(TitleData), 63);
            AddEnumDefinition(typeof(ClaimType), 64);
            AddClassDefinition(typeof(RetinueComponent), 65);
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
            base.ConstructContainerDefinition(typeof(Dictionary<Hero, ClaimType>));
            base.ConstructContainerDefinition(typeof(Dictionary<FeudalTitle, float>));
            base.ConstructContainerDefinition(typeof(Dictionary<Settlement, List<Clan>>));
        }
    }
}
