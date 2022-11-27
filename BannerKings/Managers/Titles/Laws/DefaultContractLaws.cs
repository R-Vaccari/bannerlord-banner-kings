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

        public DemesneLaw NoblesMilitaryServiceDuties { get; private set; } = new DemesneLaw("nobles_military_service_duties");
        public DemesneLaw NoblesTaxDuties { get; private set; } = new DemesneLaw("nobles_tax_duties");
        public DemesneLaw NoblesLaxDuties { get; private set; } = new DemesneLaw("nobles_lax_duties");

        public DemesneLaw CraftsmenMilitaryServiceDuties { get; private set; } = new DemesneLaw("craftsmen_military_service_duties");
        public DemesneLaw CraftsmenTaxDuties { get; private set; } = new DemesneLaw("craftsmen_tax_duties");
        public DemesneLaw CraftsmenLaxDuties { get; private set; } = new DemesneLaw("craftsmen_lax_duties");

        public DemesneLaw SerfsMilitaryServiceDuties { get; private set; } = new DemesneLaw("serfs_military_service_duties");
        public DemesneLaw SerfsAgricultureDuties { get; private set; } = new DemesneLaw("serfs_agriculture_duties");
        public DemesneLaw SerfsLaxDuties { get; private set; } = new DemesneLaw("serfs_lax_duties");

        public DemesneLaw SlavesHardLabor { get; private set; } = new DemesneLaw("slaves_hard_labor_duties");
        public DemesneLaw SlavesAgricultureDuties { get; private set; } = new DemesneLaw("slaves_agriculture_duties");
        public DemesneLaw SlavesDomesticDuties { get; private set; } = new DemesneLaw("slaves_domestic_duties");

        public DemesneLaw SlaveryVlandia { get; private set; } = new DemesneLaw("slavery_vlandia");
        public DemesneLaw SlaveryAserai { get; private set; } = new DemesneLaw("slavery_aserai");
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
                yield return NoblesMilitaryServiceDuties;
                yield return NoblesTaxDuties;
                yield return NoblesLaxDuties;
                yield return CraftsmenMilitaryServiceDuties;
                yield return CraftsmenTaxDuties;
                yield return CraftsmenLaxDuties;
                yield return SerfsMilitaryServiceDuties;
                yield return SerfsAgricultureDuties;
                yield return SerfsLaxDuties;
                yield return SlavesHardLabor;
                yield return SlavesAgricultureDuties;
                yield return SlavesDomesticDuties;
                yield return SlaveryStandard;
                yield return SlaveryVlandia;
                yield return SlaveryAserai;
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
            var government = title.contract.Government;
            var faction = BannerKingsConfig.Instance.TitleManager.GetTitleFaction(title.sovereign != null ? title.sovereign : title);
            CultureObject culture = null;
            if (faction != null)
            {
                culture = faction.Culture;
            }

            list.Add(NoblesTaxDuties.GetCopy());
            list.Add(CraftsmenTaxDuties.GetCopy());
            list.Add(SerfsAgricultureDuties.GetCopy());
            list.Add(SlavesHardLabor.GetCopy());   

            var slavery = SlaveryStandard.GetCopy();
            list.Add(slavery);

            if (government == GovernmentType.Feudal)
            {
                list.Add(DraftingVassalage.GetCopy());
                list.Add(EstateTenureFeeTail.GetCopy());

                if (culture != null)
                {
                    if (culture.StringId == "vlandia")
                    {
                        list.Remove(slavery);
                        list.Add(SlaveryVlandia.GetCopy());
                    }

                    if (culture.StringId == "aserai")
                    {
                        list.Remove(slavery);
                        list.Add(SlaveryAserai.GetCopy());
                    }
                }
            } 
            else if (government == GovernmentType.Tribal)
            {
                list.Add(DraftingHidage.GetCopy());
                list.Add(EstateTenureAllodial.GetCopy());
            }
            else
            {
                list.Add(DraftingFreeContracts.GetCopy());
                list.Add(EstateTenureFeeTail.GetCopy());
            }

            return list;
        }

        public override void Initialize()
        {
            var cultures = Campaign.Current.ObjectManager.GetObjectTypeList<CultureObject>();
            #region EstateTenure

            EstateTenureQuiaEmptores.Initialize(new TextObject("{=nDrgvaMa}Quia Emptores"),
                new TextObject("{=m2sbhA95}As a counter measure for increasingly complex vassalage relations, this laws prohibts the process of subinfeudation."),
                new TextObject("{=TaQVkuvp}Granting estates is banned\nLord party sizes +10%\nEstates cost +25%"),
                DemesneLawTypes.EstateTenure,
                0.8f,
                -0.2f,
                -0.4f,
                300,
                0);

            EstateTenureAllodial.Initialize(new TextObject("{=GLXtp5wp}Allodial Tenure"),
                new TextObject("{=noWN9WJB}The allodial tenure represents the absolute ownership of land. Estate owners have no duties towards fief lords. The absence of taxation and military requirements draws in tenants to these estates."),
                new TextObject("{=wM8wRg0L}Estate owners do not pay taxes\nImproved hearth growth"),
                DemesneLawTypes.EstateTenure,
                -0.2f,
                0.3f,
                0.1f,
                300,
                1);

            EstateTenureFeeTail.Initialize(new TextObject("{=esrBR5f3}Fee Tail"),
                new TextObject("{=SCZjCdwq}The fee tail tenure dictates that property is inherited exclusively through lawful inheritance or grant."),
                new TextObject("{=WabTyEdr}Buying or selling estates is banned\nAllows choosing estate duty"),
                DemesneLawTypes.EstateTenure,
                0.1f,
                -0.6f,
                0.7f,
                300,
                2);

            #endregion EstateTenure

            #region NobleDuties

            NoblesMilitaryServiceDuties.Initialize(new TextObject("{=SOjZv8Yk}Military Duties"),
               new TextObject("{=VNP7PvXs}Tailor the duty laws of {CLASS} towards military service. Extensive requirements of service ensure a bigger manpower pool to protect the realm. Increased class militarism and militia service.")
               .SetTextVariable("CLASS", Utils.Helpers.GetClassName(PopulationManager.PopType.Nobles, Hero.MainHero.Culture)),
               new TextObject("{=Jc476i8S}Nobles militarism +4% flat%\nMilitia quality +15%"),
               DemesneLawTypes.NobleDuties,
               0.8f,
               -0.4f,
               0.6f,
               300,
               0);

            NoblesTaxDuties.Initialize(new TextObject("{=aTqOs6gr}Tax Duties"),
               new TextObject("{=wutCVGJU}Tailor the duty laws of {CLASS} towards taxation. Stricter tax collection and more taxation forms squeeze more denarii out of {CLASS}. Increases tax output.")
               .SetTextVariable("CLASS", Utils.Helpers.GetClassName(PopulationManager.PopType.Nobles, Hero.MainHero.Culture)),
               new TextObject("{=pzP0OzbV}Increased nobles tax output by 25%"),
               DemesneLawTypes.NobleDuties,
               0.4f,
               -0.4f,
               0.5f,
               300,
               1);

            NoblesLaxDuties.Initialize(new TextObject("{=qGvM38At}Lax Duties"),
               new TextObject("{=QR8cTK3Y}Lessen the duty burdens of {CLASS}. Reduced duties makes the populace more content and gives them room for prosperity. Reduces output and military contribution.")
               .SetTextVariable("CLASS", Utils.Helpers.GetClassName(PopulationManager.PopType.Nobles, Hero.MainHero.Culture)),
               new TextObject("{=qhLeBUWB}Increased settlement research and influence outputs\nCraftsmen militarism -2% flat\nNobles tax output -40%"),
               DemesneLawTypes.NobleDuties,
               -0.5f,
               0.8f,
               -0.2f,
               300,
               2);

            #endregion NobleDuties

            #region CraftsmenDuties

            CraftsmenMilitaryServiceDuties.Initialize(new TextObject("{=SOjZv8Yk}Military Duties"),
               new TextObject("{=VNP7PvXs}Tailor the duty laws of {CLASS} towards military service. Extensive requirements of service ensure a bigger manpower pool to protect the realm. Increased class militarism and militia service.")
               .SetTextVariable("CLASS", Utils.Helpers.GetClassName(PopulationManager.PopType.Craftsmen, Hero.MainHero.Culture)),
               new TextObject("{=TkTHTJDm}Craftsmen militarism +3% flat\nMilitia quality +10%"),
               DemesneLawTypes.CraftsmenDuties,
               0.8f,
               -0.4f,
               0.6f,
               300,
               0);

            CraftsmenTaxDuties.Initialize(new TextObject("{=aTqOs6gr}Tax Duties"),
               new TextObject("{=wutCVGJU}Tailor the duty laws of {CLASS} towards taxation. Stricter tax collection and more taxation forms squeeze more denarii out of {CLASS}. Increases tax output.")
               .SetTextVariable("CLASS", Utils.Helpers.GetClassName(PopulationManager.PopType.Craftsmen, Hero.MainHero.Culture)),
               new TextObject("{=1UikMUyy}Increased nobles tax output by 35%"),
               DemesneLawTypes.CraftsmenDuties,
               0.4f,
               -0.4f,
               0.5f,
               300,
               1);

            CraftsmenLaxDuties.Initialize(new TextObject("{=qGvM38At}Lax Duties"),
               new TextObject("{=QR8cTK3Y}Lessen the duty burdens of {CLASS}. Reduced duties makes the populace more content and gives them room for prosperity. Reduces output and military contribution.")
               .SetTextVariable("CLASS", Utils.Helpers.GetClassName(PopulationManager.PopType.Craftsmen, Hero.MainHero.Culture)),
               new TextObject("{=u9xEHrAe}Increased craftsmen prosperity and loyalty\nIncreased production quality +5%\nCraftsmen militarism -1.5% flat\nCraftsmen tax output -40%"),
               DemesneLawTypes.CraftsmenDuties,
               -0.5f,
               0.8f,
               -0.2f,
               300,
               2);

            #endregion CraftsmenDuties

            #region SerfDuties

            SerfsMilitaryServiceDuties.Initialize(new TextObject("{=SOjZv8Yk}Military Duties"),
               new TextObject("{=VNP7PvXs}Tailor the duty laws of {CLASS} towards military service. Extensive requirements of service ensure a bigger manpower pool to protect the realm. Increased class militarism and militia service.")
               .SetTextVariable("CLASS", Utils.Helpers.GetClassName(PopulationManager.PopType.Serfs, Hero.MainHero.Culture)),
               new TextObject("{=51Kro6UV}Serf militarism +3% flat\nSerf militia contribution +20%"),
               DemesneLawTypes.SerfDuties,
               0.8f,
               -0.4f,
               0.6f,
               300,
               0);

            SerfsAgricultureDuties.Initialize(new TextObject("{=vt2Sq2aG}Agricultural Duties"),
               new TextObject("{=NXk9mSNW}Tailor the duty laws of {CLASS} towards agriculture. Labor requirements and movement restriction tie the {CLASS} to the land and its productivity. Increased agricultural output.")
               .SetTextVariable("CLASS", Utils.Helpers.GetClassName(PopulationManager.PopType.Serfs, Hero.MainHero.Culture)),
               new TextObject("{=tXvBhS3n}Increased production of farm goods in villages and food in castles and towns"),
               DemesneLawTypes.SerfDuties,
               0.4f,
               -0.4f,
               0.5f,
               300,
               1);

            SerfsLaxDuties.Initialize(new TextObject("{=qGvM38At}Lax Duties"),
               new TextObject("{=QR8cTK3Y}Lessen the duty burdens of {CLASS}. Reduced duties makes the populace more content and gives them room for prosperity. Reduces output and military contribution.")
               .SetTextVariable("CLASS", Utils.Helpers.GetClassName(PopulationManager.PopType.Serfs, Hero.MainHero.Culture)),
               new TextObject("{=jyMc8X74}Increased settlement prosperity (or Hearths) and loyalty\nReduced agricultural output\nSerf militarism -1.5% flat\nSerf militia contribution -10%"),
               DemesneLawTypes.SerfDuties,
               -0.5f,
               0.8f,
               -0.2f,
               300,
               2);

            #endregion SerfDuties

            #region SlaveDuties

            SlavesHardLabor.Initialize(new TextObject("{=AN0ccfH1}Hard Labor"),
               new TextObject("{=jq2vFB2n}Tailor the duty laws of {CLASS} towards hard labor. Hard labor involves unskilled, undesirable labors such as mining and construction. Increases mining production and settlement construction.")
               .SetTextVariable("SLAVES", Utils.Helpers.GetClassName(PopulationManager.PopType.Slaves, Hero.MainHero.Culture)),
               new TextObject("{=oy8RMSyd}Increased production of mining goods\nSlaves contruction contribution +20%"),
               DemesneLawTypes.SlaveDuties,
               0.4f,
               -0.8f,
               0.5f,
               300, 0);

            SlavesAgricultureDuties.Initialize(new TextObject("{=vt2Sq2aG}Agricultural Duties"),
               new TextObject("{=NXk9mSNW}Tailor the duty laws of {CLASS} towards agriculture. Labor requirements and movement restriction tie the {CLASS} to the land and its productivity. Increased agricultural output.")
               .SetTextVariable("SLAVES", Utils.Helpers.GetClassName(PopulationManager.PopType.Slaves, Hero.MainHero.Culture)),
               new TextObject("{=tXvBhS3n}Increased production of farm goods in villages and food in castles and towns"),
               DemesneLawTypes.SlaveDuties,
               0.4f,
               -0.6f,
               0.5f,
               300,
               1);

            SlavesDomesticDuties.Initialize(new TextObject("{=0Ps1q7J0}Domestic Duties"),
               new TextObject("{=Zq8Zcv3N}Tailor the duty laws of {CLASS} towards domestic and skilled labor. Citizen households will often have or want to have slaves for various domestic labors. Enslaved shopkeepers, artisans and professionals provide tax benefits to their owners.")
               .SetTextVariable("CLASS", Utils.Helpers.GetClassName(PopulationManager.PopType.Slaves, Hero.MainHero.Culture)),
               new TextObject("{=P3c1aJgi}Slave tax output +15%"),
               DemesneLawTypes.SlaveDuties,
               0.6f,
               -0.2f,
               0.3f,
               300,
               2);

            #endregion SlaveDuties

            #region Slavery

            SlaveryStandard.Initialize(new TextObject("{=6jyLYrts}Calradic Law"), 
                new TextObject("{=ZWmNxRMn}The Imperial or Calradic law stablishes the legal existance of slaves and their ownership. Though they may not be harmed or killed without just cause, the slave trade is rampant and devoid of restrictions. Any person found in debt or captured in battle may be enslaved, and slaves compose the labor force across all settlements."),
                new TextObject("{=!}"),
                DemesneLawTypes.Slavery,
                0.4f,
                -0.6f,
                0.3f,
                300, 0);

            SlaveryVlandia.Initialize(new TextObject("{=hBVZCcL3}Vlandic Law"),
                new TextObject("{=4vBS06Ds}The Vlandic tradition on slavery stipulates that Vlandians shall not enslave each other. Slaves are present in small quantities in rural estates. Though Vlandian individuals may become or be born slaves, Vlandian lords are prohibited from purposefuly enslaving them."),
                new TextObject("{=tK4GJ8bt}Slave demand reduced by 30%\nVlandian prisoners cannot be enslaved\n"),
                DemesneLawTypes.Slavery,
                0.2f,
                -0.2f,
                0.5f,
                300,
                1,
                cultures.First(x => x.StringId == "vlandia"));

            SlaveryAserai.Initialize(new TextObject("{=7Q9FJrZ8}Aseran Law"),
               new TextObject("{=szTYxGgd}The Aserai peoples have a long tradition of slavery. Aserai slaves are no more than a trade good, and as such, demand for them is quite high in the economy. ."),
               new TextObject("{=Z5Qopa2c}Slave demand increased by 50%\nSlaves count as military manpower\nSlaves loyalty impact increased by 10%"),
               DemesneLawTypes.Slavery,
               0.3f,
               -0.6f,
               0.6f,
               300,
               1,
               cultures.First(x => x.StringId == "aserai"));

            SlaveryManumission.Initialize(new TextObject("{=T4doWFdj}Manumission"),
                new TextObject("{=4vBS06Ds}The Vlandic tradition on slavery stipulates that Vlandians shall not enslave each other. Slaves are present in small quantities in rural estates. Though Vlandian individuals may become or be born slaves, Vlandian lords are prohibited from purposefuly enslaving them."),
                new TextObject("{=kncwdEvN}Slave demand reduced by 100%\n"),
                DemesneLawTypes.Slavery,
                -0.5f,
                0.9f,
                -0.1f,
                300,
                2);

            #endregion Slavery

            #region Drafting

            DraftingHidage.Initialize(new TextObject("{=WCkAop4m}Hidage"),
                new TextObject("{=nasOsyDh}Under hidage, landowners are expected to provide levies based on their land, calculated as hides."),
                new TextObject("{=YgwONvnn}Notables provide volunteers based on their estates or power to ally lords\nRural volunteers are restricted to the kingdom's lords\nRecruitment prices increased by 50%"),
                DemesneLawTypes.Drafting,
                0.3f,
                0.1f,
                0.2f,
                300, 0);

            DraftingFreeContracts.Initialize(new TextObject("{=WtxLpuAU}Free Contracts"),
              new TextObject("{=ZWmNxRMn}The Imperial or Calradic law stablishes the legal existance of slaves and their ownership. Though they may not be harmed or killed without just cause, the slave trade is rampant and devoid of restrictions. Any person found in debt or captured in battle may be enslaved, and slaves compose the labor force across all settlements."),
              new TextObject("{=KDRmz1zo}Notables provide volunteers to any neutral or allied lord\nRecruitment prices increased by 100%"),
              DemesneLawTypes.Drafting,
              0.1f,
              0.5f,
              -0.2f,
              300,
              1);

            DraftingVassalage.Initialize(new TextObject("{=92A5YP2x}Vassalage"),
              new TextObject("{=ZWmNxRMn}The Imperial or Calradic law stablishes the legal existance of slaves and their ownership. Though they may not be harmed or killed without just cause, the slave trade is rampant and devoid of restrictions. Any person found in debt or captured in battle may be enslaved, and slaves compose the labor force across all settlements."),
              new TextObject("{=i4Y6Sg9k}Notables provide volunteers to their suzerain and armies\nRural volunteers are restricted to the kingdom's lords\nInfluence from settlements reduced by 20%"),
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
