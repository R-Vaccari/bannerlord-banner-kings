using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Titles.Laws
{
    public class DefaultDemesneLaws : DefaultTypeInitializer<DefaultDemesneLaws, DemesneLaw>
    {

        public DemesneLaw EstateTenureFeeTail { get; private set; } = new DemesneLaw("estate_tenure_fee_tail");
        public DemesneLaw EstateTenureQuiaEmptores { get; private set; } = new DemesneLaw("estate_tenure_quia_emptores");
        public DemesneLaw EstateTenureAllodial { get; private set; } = new DemesneLaw("estate_tenure_allodial");


        public DemesneLaw SerfsMilitaryServiceDuties { get; private set; } = new DemesneLaw("serfs_military_service_duties");
        public DemesneLaw SerfsAgricultureDuties { get; private set; } = new DemesneLaw("serfs_agriculture_duties");
        public DemesneLaw SerfsLaxDuties { get; private set; } = new DemesneLaw("serfs_lax_duties");

        public DemesneLaw SlavesHardLabor { get; private set; } = new DemesneLaw("slaves_hard_labor_duties");
        public DemesneLaw SlavesAgricultureDuties { get; private set; } = new DemesneLaw("slaves_agriculture_duties");
        public DemesneLaw SlavesDomesticDuties { get; private set; } = new DemesneLaw("slaves_domestic_duties");


        public DemesneLaw SlaveryVlandia { get; private set; } = new DemesneLaw("slavery_vlandia");
        public DemesneLaw SlaveryStandard { get; private set; } = new DemesneLaw("slavery_standard");
        public DemesneLaw SlaveryManumission { get; private set; } = new DemesneLaw("slavery_manumission");

        public DemesneLaw DraftingHidage { get; private set; } = new DemesneLaw("drafting_hidage");
        public DemesneLaw DraftingVassalage { get; private set; } = new DemesneLaw("drafting_vassalage");
        public DemesneLaw DraftingFreeContracts { get; private set; } = new DemesneLaw("drafting_free_contracts");

        public override IEnumerable<DemesneLaw> All
        {
            get
            {
                yield return EstateTenureQuiaEmptores;
                yield return EstateTenureAllodial;
                yield return EstateTenureFeeTail;
                yield return SerfsMilitaryServiceDuties;
                yield return SerfsAgricultureDuties;
                yield return SerfsLaxDuties;
                yield return SlavesHardLabor;
                yield return SlavesAgricultureDuties;
                yield return SlavesDomesticDuties;
                yield return SlaveryStandard;
                yield return SlaveryVlandia;
                yield return SlaveryManumission;
                yield return DraftingHidage;
                yield return DraftingFreeContracts;
                yield return DraftingVassalage;

            }
        }

        public List<DemesneLaw> GetLawsByType(DemesneLawTypes type) => All.ToList().FindAll(x => x.LawType == type);

        public DemesneLaw GetLawByIndex(DemesneLawTypes type, int index)
        {
            var law = All.FirstOrDefault(x => x.LawType == type && x.Index == index);
            return law;
        }

        public List<DemesneLaw> GetAdequateLaws(FeudalTitle title)
        {
            var list = new List<DemesneLaw>();
            list.Add(DraftingFreeContracts.GetCopy());
            list.Add(SlaveryStandard.GetCopy());
            list.Add(SlavesHardLabor.GetCopy());
            list.Add(SerfsAgricultureDuties.GetCopy());
            list.Add(EstateTenureFeeTail.GetCopy());

            return list;
        }

        public override void Initialize()
        {
            #region EstateTenure

            EstateTenureQuiaEmptores.Initialize(new TextObject("{=!}Quia Emptores"),
                new TextObject("{=!}As a counter measure for increasingly complex vassalage relations, this laws prohibts the process of subinfeudation."),
                new TextObject("{=!}Granting estates is banned\nLord party sizes +10%\nEstates cost +25%"),
                DemesneLawTypes.EstateTenure,
                0.8f,
                -0.2f,
                -0.4f,
                300,
                0);

            EstateTenureAllodial.Initialize(new TextObject("{=!}Allodial Tenure"),
                new TextObject("{=!}The allodial tenure represents the absolute ownership of land. Estate owners have no duties towards fief lords. The absence of taxation and military requirements draws in tenants to these estates."),
                new TextObject("{=!}Estate owners do not pay taxes\nImproved hearth growth"),
                DemesneLawTypes.EstateTenure,
                -0.2f,
                0.3f,
                0.1f,
                300,
                1);

            EstateTenureFeeTail.Initialize(new TextObject("{=!}Fee Tail"),
                new TextObject("{=!}The fee tail tenure dictates that property is inherited exclusively through lawful inheritance or grant."),
                new TextObject("{=!}Buying or selling estates is banned\nAllows choosing estate duty"),
                DemesneLawTypes.EstateTenure,
                0.1f,
                -0.6f,
                0.7f,
                300,
                2);

            #endregion EstateTenure



            #region SerfDuties

            SerfsMilitaryServiceDuties.Initialize(new TextObject("{=!}Military Duties"),
               new TextObject("{=!}Tailor the duty laws of {CLASS} towards military service. Extensive requirements of service ensure a bigger manpower pool to protect the realm. Increased class militarism and militia service.")
               .SetTextVariable("CLASS", Utils.Helpers.GetClassName(PopulationManager.PopType.Serfs, Hero.MainHero.Culture)),
               new TextObject("{=!}Serf militarism +3% flat\nSerf militia contribution +20%"),
               DemesneLawTypes.SerfDuties,
               0.8f,
               -0.4f,
               0.6f,
               300,
               0);

            SerfsAgricultureDuties.Initialize(new TextObject("{=!}Agricultural Duties"),
               new TextObject("{=!}Tailor the duty laws of {CLASS} towards agriculture. Labor requirements and movement restriction tie the serfs to the land and its productivity. Increased agricultural output.")
               .SetTextVariable("CLASS", Utils.Helpers.GetClassName(PopulationManager.PopType.Serfs, Hero.MainHero.Culture)),
               new TextObject("{=!}Increased production of farm goods in villages and food in castles and towns"),
               DemesneLawTypes.SerfDuties,
               0.4f,
               -0.4f,
               0.5f,
               300,
               1);

            SerfsLaxDuties.Initialize(new TextObject("{=!}Lax Duties"),
               new TextObject("{=!}Lessen the duty burdens of {CLASS}. Reduced duties makes the populace more content and gives them room for prosperity. Reduces output and military contribution.")
               .SetTextVariable("CLASS", Utils.Helpers.GetClassName(PopulationManager.PopType.Serfs, Hero.MainHero.Culture)),
               new TextObject("{=!}Increased settlement prosperity (or Hearths) and loyalty\nReduced agricultural output\nSerf militarism -1.5% flat\nSerf militia contribution -10%"),
               DemesneLawTypes.SerfDuties,
               -0.5f,
               0.8f,
               -0.2f,
               300,
               2);

            #endregion SerfDuties

            #region SlaveDuties

            SlavesHardLabor.Initialize(new TextObject("{=!}Hard Labor"),
               new TextObject("{=!}Tailor the duty laws of {CLASS} towards hard labor. Hard labor involves unskilled, undesirable labors such as mining and construction. Increases mining production and settlement construction.")
               .SetTextVariable("SLAVES", Utils.Helpers.GetClassName(PopulationManager.PopType.Serfs, Hero.MainHero.Culture)),
               new TextObject("{=!}Increased production of mining goods\nSlaves contruction contribution +20%"),
               DemesneLawTypes.SlaveDuties,
               0.4f,
               -0.8f,
               0.5f,
               300, 0);

            SlavesAgricultureDuties.Initialize(new TextObject("{=!}Agricultural Duties"),
               new TextObject("{=!}Tailor the duty laws of {CLASS} towards agriculture. Labor requirements and movement restriction tie the serfs to the land and its productivity. Increased agricultural output.")
               .SetTextVariable("SLAVES", Utils.Helpers.GetClassName(PopulationManager.PopType.Serfs, Hero.MainHero.Culture)),
               new TextObject("{=!}Increased production of farm goods in villages and food in castles and towns"),
               DemesneLawTypes.SlaveDuties,
               0.4f,
               -0.6f,
               0.5f,
               300,
               1);

            SlavesDomesticDuties.Initialize(new TextObject("{=!}Domestic Duties"),
               new TextObject("{=!}Tailor the duty laws of {CLASS} towards domestic and skilled labor. Citizen households will often have or want to have slaves for various domestic labors. Enslaved shopkeepers, artisans and professionals provide tax benefits to their owners.")
               .SetTextVariable("CLASS", Utils.Helpers.GetClassName(PopulationManager.PopType.Serfs, Hero.MainHero.Culture)),
               new TextObject("{=!}Slave tax output +15%"),
               DemesneLawTypes.SlaveDuties,
               0.6f,
               -0.2f,
               0.3f,
               300,
               2);

            #endregion SlaveDuties

            #region Slavery

            SlaveryStandard.Initialize(new TextObject("{=!}Calradic Law"), 
                new TextObject("{=!}The Imperial or Calradic law stablishes the legal existance of slaves and their ownership. Though they may not be harmed or killed without just cause, the slave trade is rampant and devoid of restrictions. Any person found in debt or captured in battle may be enslaved, and slaves compose the labor force across all settlements."),
                new TextObject("{=!}"),
                DemesneLawTypes.Slavery,
                0.4f,
                -0.6f,
                0.3f,
                300, 0);

            SlaveryVlandia.Initialize(new TextObject("{=!}Vlandic Law"),
                new TextObject("{=!}The Vlandic tradition on slavery stipulates that Vlandians shall not enslave each other. Slaves are present in small quantities in rural estates. Though Vlandian individuals may become or be born slaves, Vlandian lords are prohibited from purposefuly enslaving them."),
                new TextObject("{=!}"),
                DemesneLawTypes.Slavery,
                0.2f,
                -0.2f,
                0.5f,
                300,
                1);

            SlaveryManumission.Initialize(new TextObject("{=!}Manumission"),
                new TextObject("{=!}The Vlandic tradition on slavery stipulates that Vlandians shall not enslave each other. Slaves are present in small quantities in rural estates. Though Vlandian individuals may become or be born slaves, Vlandian lords are prohibited from purposefuly enslaving them."),
                new TextObject("{=!}"),
                DemesneLawTypes.Slavery,
                -0.5f,
                0.9f,
                -0.1f,
                300,
                2);

            #endregion Slavery

            #region Drafting

            DraftingHidage.Initialize(new TextObject("{=!}Hidage"),
                new TextObject("{=!}Under hidage, landowners are expected to provide levies based on their land, calculated as hides."),
                new TextObject("{=!}Notables provide volunteers based on their estates or power to ally lords\nRural volunteers are restricted to the kingdom's lords\nRecruitment prices increased by 50%"),
                DemesneLawTypes.Drafting,
                0.3f,
                0.1f,
                0.2f,
                300, 0);

            DraftingFreeContracts.Initialize(new TextObject("{=!}Free Contracts"),
              new TextObject("{=!}The Imperial or Calradic law stablishes the legal existance of slaves and their ownership. Though they may not be harmed or killed without just cause, the slave trade is rampant and devoid of restrictions. Any person found in debt or captured in battle may be enslaved, and slaves compose the labor force across all settlements."),
              new TextObject("{=!}Notables provide volunteers to any neutral or allied lord\nRecruitment prices increased by 100%"),
              DemesneLawTypes.Drafting,
              0.1f,
              0.5f,
              -0.2f,
              300,
              1);

            DraftingVassalage.Initialize(new TextObject("{=!}Vassalage"),
              new TextObject("{=!}The Imperial or Calradic law stablishes the legal existance of slaves and their ownership. Though they may not be harmed or killed without just cause, the slave trade is rampant and devoid of restrictions. Any person found in debt or captured in battle may be enslaved, and slaves compose the labor force across all settlements."),
              new TextObject("{=!}Notables provide volunteers to their suzerain and armies\nRural volunteers are restricted to the kingdom's lords\nInfluence from settlements reduced by 20%"),
              DemesneLawTypes.Drafting,
              0.5f,
              -0.5f,
              1f,
              300,
              2);

            #endregion Drafting
        }
    }
}
