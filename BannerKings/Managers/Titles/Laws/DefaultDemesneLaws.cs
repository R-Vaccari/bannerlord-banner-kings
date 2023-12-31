using BannerKings.Managers.Titles.Governments;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Titles.Laws
{
    public class DefaultDemesneLaws : DefaultTypeInitializer<DefaultDemesneLaws, DemesneLaw>
    {
        public DemesneLaw EstateTenureFeeTail { get; } = new DemesneLaw("estate_tenure_fee_tail");
        public DemesneLaw EstateTenureQuiaEmptores { get; } = new DemesneLaw("estate_tenure_quia_emptores");
        public DemesneLaw EstateTenureAllodial { get; } = new DemesneLaw("estate_tenure_allodial");

        public DemesneLaw NoblesMilitaryServiceDuties { get; private set; } = new DemesneLaw("nobles_military_service_duties");
        public DemesneLaw NoblesTaxDuties { get; } = new DemesneLaw("nobles_tax_duties");
        public DemesneLaw NoblesLaxDuties { get; } = new DemesneLaw("nobles_lax_duties");

        public DemesneLaw CraftsmenMilitaryServiceDuties { get; private set; } = new DemesneLaw("craftsmen_military_service_duties");
        public DemesneLaw CraftsmenTaxDuties { get; } = new DemesneLaw("craftsmen_tax_duties");
        public DemesneLaw CraftsmenLaxDuties { get; } = new DemesneLaw("craftsmen_lax_duties");

        public DemesneLaw SerfsMilitaryServiceDuties { get; } = new DemesneLaw("serfs_military_service_duties");
        public DemesneLaw SerfsAgricultureDuties { get; } = new DemesneLaw("serfs_agriculture_duties");
        public DemesneLaw SerfsLaxDuties { get; } = new DemesneLaw("serfs_lax_duties");

        public DemesneLaw SlavesHardLabor { get; } = new DemesneLaw("slaves_hard_labor_duties");
        public DemesneLaw SlavesAgricultureDuties { get; } = new DemesneLaw("slaves_agriculture_duties");
        public DemesneLaw SlavesDomesticDuties { get; } = new DemesneLaw("slaves_domestic_duties");

        public DemesneLaw SlaveryVlandia { get; } = new DemesneLaw("slavery_vlandia");
        public DemesneLaw SlaveryAserai { get; } = new DemesneLaw("slavery_aserai");
        public DemesneLaw SlaveryStandard { get; } = new DemesneLaw("slavery_standard");
        public DemesneLaw SlaveryManumission { get; } = new DemesneLaw("slavery_manumission");

        public DemesneLaw DraftingHidage { get; } = new DemesneLaw("drafting_hidage");
        public DemesneLaw DraftingVassalage { get; } = new DemesneLaw("drafting_vassalage");
        public DemesneLaw DraftingFreeContracts { get; } = new DemesneLaw("drafting_free_contracts");

        public DemesneLaw TenancyFull { get; } = new DemesneLaw("tenancy_full");
        public DemesneLaw TenancyMixed { get; } = new DemesneLaw("tenancy_mixed");
        public DemesneLaw TenancyNone { get; } = new DemesneLaw("tenancy_none");

        public DemesneLaw CouncilAppointed { get; } = new DemesneLaw("council_appointed");
        public DemesneLaw CouncilElected { get; } = new DemesneLaw("council_elected");

        public DemesneLaw ArmyPrivate { get; } = new DemesneLaw("ArmyPrivate");
        public DemesneLaw ArmyHorde { get; } = new DemesneLaw("ArmyHorde");
        public DemesneLaw ArmyLegion { get; } = new DemesneLaw("ArmyLegion");

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
                yield return TenancyFull;
                yield return TenancyMixed;
                yield return TenancyNone;
                yield return CouncilAppointed;
                yield return CouncilElected;
                yield return ArmyPrivate;
                yield return ArmyHorde;
                yield return ArmyLegion;
            }
        }

        public List<DemesneLaw> GetLawsByType(DemesneLawTypes type) => All.ToList().FindAll(x => x.LawType == type);

        public DemesneLaw GetLawByIndex(DemesneLawTypes type, int index)
        {
            var law = All.FirstOrDefault(x => x.LawType == type);
            return law;
        }

        public List<DemesneLaw> GetAdequateLaws(FeudalTitle title)
        {
            var list = new List<DemesneLaw>();
            var government = title.Contract.Government;
            var faction = BannerKingsConfig.Instance.TitleManager.GetTitleFaction(title.Sovereign != null ? title.Sovereign : title);
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

            if (government == DefaultGovernments.Instance.Feudal)
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

                list.Add(TenancyNone.GetCopy());
                list.Add(ArmyPrivate.GetCopy());
            } 
            else if (government == DefaultGovernments.Instance.Tribal)
            {
                list.Add(DraftingHidage.GetCopy());
                list.Add(EstateTenureAllodial.GetCopy());
                list.Add(TenancyFull.GetCopy());
                list.Add(ArmyHorde.GetCopy());
            }
            else
            {
                list.Add(DraftingFreeContracts.GetCopy());
                list.Add(EstateTenureFeeTail.GetCopy());
                list.Add(TenancyMixed.GetCopy());
                list.Add(ArmyLegion.GetCopy());
            }

            if (government == DefaultGovernments.Instance.Republic)
            {
                list.Add(CouncilElected.GetCopy());
            }
            else
            {
                list.Add(CouncilAppointed.GetCopy());
            }

            return list;
        }

        public override void Initialize()
        {
            var cultures = TaleWorlds.CampaignSystem.Campaign.Current.ObjectManager.GetObjectTypeList<CultureObject>();
            ArmyPrivate.Initialize(new TextObject("{=R9xGiMLf}Private Armies"),
                new TextObject("{=n1ck3HDs}Every head of a House, beholder of any title superior to a Lordship, not accounting for army privilege policies, is allowed to form their own army."),
                new TextObject("{=sLTkCtmc}Army creation limited by title level (minimum: Barony)\nCalling potential army leaders costs twice as much"),
                DemesneLawTypes.Army,
                -0.1f,
                -0.7f,
                0.8f,
                300);

            ArmyHorde.Initialize(new TextObject("{=VYAP7Arh}Hordes"),
                new TextObject("{=jwdDWjKF}Every chief of a House is allowed to gather their own horde. Hordes are easier to gather than armies, but harder to maintain."),
                new TextObject("{=vOPRVto1}Army loses cohesion faster\nInfluence cost to call parties reduced\nBattle renown increased while participating in armies"),
                DemesneLawTypes.Army,
                -0.4f,
                0.8f,
                0.2f,
                300,
                null,
                (Kingdom kingdom) =>
                {
                    FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(kingdom);
                    if (title != null) return title.Contract.Government == DefaultGovernments.Instance.Tribal;
                    return false;
                });

            ArmyLegion.Initialize(new TextObject("{=Qs1qPddP}Legions"),
                new TextObject("{=eW400qSp}A concept developed by the Calradoi, legions are armies with the purpose of serving the state, and their leadership, Imperium, is granted to select commanders. More so than any other armies, legions are highly organized and efficient, the results of centuries of continuous and methodical improvement."),
                new TextObject("{=P1yotQ6Q}Army creation delegated to Legion Commander council positions\nLegion cohesion lasts longer\nSupply buildup reduced for armies\nReduced influence for army participation\nCalling potential army leaders costs 5 times as much"),
                DemesneLawTypes.Army,
                0.5f,
                0.4f,
                -0.7f,
                300,
                null,
                (Kingdom kingdom) =>
                {
                    FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(kingdom);
                    if (title != null) return title.Contract.Government == DefaultGovernments.Instance.Imperial ||
                        title.Contract.Government == DefaultGovernments.Instance.Republic;
                    return false;
                });

            #region EstateTenure

            EstateTenureQuiaEmptores.Initialize(new TextObject("{=nDrgvaMa}Quia Emptores"),
                new TextObject("{=m2sbhA95}As a counter measure for increasingly complex vassalage relations, this laws prohibts the process of subinfeudation."),
                new TextObject("{=TaQVkuvp}Granting estates is banned\nLord party sizes +10%\nEstates cost +25%"),
                DemesneLawTypes.EstateTenure,
                0.8f,
                -0.2f,
                -0.4f,
                300);

            EstateTenureAllodial.Initialize(new TextObject("{=GLXtp5wp}Allodial Tenure"),
                new TextObject("{=noWN9WJB}The allodial tenure represents the absolute ownership of land. Estate owners have no duties towards fief lords. The absence of taxation and military requirements draws in tenants to these estates."),
                new TextObject("{=wM8wRg0L}Estate owners do not pay taxes\nImproved hearth growth"),
                DemesneLawTypes.EstateTenure,
                -0.2f,
                0.3f,
                0.1f,
                300);

            EstateTenureFeeTail.Initialize(new TextObject("{=esrBR5f3}Fee Tail"),
                new TextObject("{=SCZjCdwq}The fee tail tenure dictates that property is inherited exclusively through lawful inheritance or grant."),
                new TextObject("{=!}Buying or selling estates is banned\nReduced taxation on estate incomes"),
                DemesneLawTypes.EstateTenure,
                0.1f,
                -0.6f,
                0.7f,
                300);

            #endregion EstateTenure

            #region NobleDuties

            NoblesMilitaryServiceDuties.Initialize(new TextObject("{=!}Military Duties (Nobles)"),
               new TextObject("{=VNP7PvXs}Tailor the duty laws of {CLASS} towards military service. Extensive requirements of service ensure a bigger manpower pool to protect the realm. Increased class militarism and militia service.")
               .SetTextVariable("CLASS", Utils.Helpers.GetClassName(PopulationManager.PopType.Nobles, Hero.MainHero.Culture)),
               new TextObject("{=Jc476i8S}Nobles militarism +4% flat%\nMilitia quality +15%"),
               DemesneLawTypes.NobleDuties,
               0.8f,
               -0.4f,
               0.6f,
               300);

            NoblesTaxDuties.Initialize(new TextObject("{=!}Tax Duties (Nobles)"),
               new TextObject("{=wutCVGJU}Tailor the duty laws of {CLASS} towards taxation. Stricter tax collection and more taxation forms squeeze more denarii out of {CLASS}. Increases tax output.")
               .SetTextVariable("CLASS", Utils.Helpers.GetClassName(PopulationManager.PopType.Nobles, Hero.MainHero.Culture)),
               new TextObject("{=pzP0OzbV}Increased nobles tax output by 25%"),
               DemesneLawTypes.NobleDuties,
               0.4f,
               -0.4f,
               0.5f,
               300);

            NoblesLaxDuties.Initialize(new TextObject("{=!}Lax Duties (Nobles)"),
               new TextObject("{=QR8cTK3Y}Lessen the duty burdens of {CLASS}. Reduced duties makes the populace more content and gives them room for prosperity. Reduces output and military contribution.")
               .SetTextVariable("CLASS", Utils.Helpers.GetClassName(PopulationManager.PopType.Nobles, Hero.MainHero.Culture)),
               new TextObject("{=qhLeBUWB}Increased settlement research and influence outputs\nCraftsmen militarism -2% flat\nNobles tax output -40%"),
               DemesneLawTypes.NobleDuties,
               -0.5f,
               0.8f,
               -0.2f,
               300);

            #endregion NobleDuties

            #region CraftsmenDuties

            CraftsmenMilitaryServiceDuties.Initialize(new TextObject("{=!}Military Duties (Craftsmen)"),
               new TextObject("{=VNP7PvXs}Tailor the duty laws of {CLASS} towards military service. Extensive requirements of service ensure a bigger manpower pool to protect the realm. Increased class militarism and militia service.")
               .SetTextVariable("CLASS", Utils.Helpers.GetClassName(PopulationManager.PopType.Craftsmen, Hero.MainHero.Culture)),
               new TextObject("{=TkTHTJDm}Craftsmen militarism +3% flat\nMilitia quality +10%"),
               DemesneLawTypes.CraftsmenDuties,
               0.8f,
               -0.4f,
               0.6f,
               300);

            CraftsmenTaxDuties.Initialize(new TextObject("{=!}Tax Duties (Craftsmen)"),
               new TextObject("{=wutCVGJU}Tailor the duty laws of {CLASS} towards taxation. Stricter tax collection and more taxation forms squeeze more denarii out of {CLASS}. Increases tax output.")
               .SetTextVariable("CLASS", Utils.Helpers.GetClassName(PopulationManager.PopType.Craftsmen, Hero.MainHero.Culture)),
               new TextObject("{=1UikMUyy}Increased nobles tax output by 35%"),
               DemesneLawTypes.CraftsmenDuties,
               0.4f,
               -0.4f,
               0.5f,
               300);

            CraftsmenLaxDuties.Initialize(new TextObject("{=!}Lax Duties (Craftsmen)"),
               new TextObject("{=QR8cTK3Y}Lessen the duty burdens of {CLASS}. Reduced duties makes the populace more content and gives them room for prosperity. Reduces output and military contribution.")
               .SetTextVariable("CLASS", Utils.Helpers.GetClassName(PopulationManager.PopType.Craftsmen, Hero.MainHero.Culture)),
               new TextObject("{=u9xEHrAe}Increased craftsmen prosperity and loyalty\nIncreased production quality +5%\nCraftsmen militarism -1.5% flat\nCraftsmen tax output -40%"),
               DemesneLawTypes.CraftsmenDuties,
               -0.5f,
               0.8f,
               -0.2f,
               300);

            #endregion CraftsmenDuties

            #region SerfDuties

            SerfsMilitaryServiceDuties.Initialize(new TextObject("{=!}Military Duties (Serfs)"),
               new TextObject("{=VNP7PvXs}Tailor the duty laws of {CLASS} towards military service. Extensive requirements of service ensure a bigger manpower pool to protect the realm. Increased class militarism and militia service.")
               .SetTextVariable("CLASS", Utils.Helpers.GetClassName(PopulationManager.PopType.Serfs, Hero.MainHero.Culture)),
               new TextObject("{=51Kro6UV}Serf militarism +3% flat\nSerf militia contribution +20%"),
               DemesneLawTypes.SerfDuties,
               0.8f,
               -0.4f,
               0.6f,
               300);

            SerfsAgricultureDuties.Initialize(new TextObject("{=!}Agricultural Duties (Serfs)"),
               new TextObject("{=NXk9mSNW}Tailor the duty laws of {CLASS} towards agriculture. Labor requirements and movement restriction tie the {CLASS} to the land and its productivity. Increased agricultural output.")
               .SetTextVariable("CLASS", Utils.Helpers.GetClassName(PopulationManager.PopType.Serfs, Hero.MainHero.Culture)),
               new TextObject("{=tXvBhS3n}Increased production of farm goods in villages and food in castles and towns"),
               DemesneLawTypes.SerfDuties,
               0.4f,
               -0.4f,
               0.5f,
               300);

            SerfsLaxDuties.Initialize(new TextObject("{=!}Lax Duties (Serfs)"),
               new TextObject("{=QR8cTK3Y}Lessen the duty burdens of {CLASS}. Reduced duties makes the populace more content and gives them room for prosperity. Reduces output and military contribution.")
               .SetTextVariable("CLASS", Utils.Helpers.GetClassName(PopulationManager.PopType.Serfs, Hero.MainHero.Culture)),
               new TextObject("{=jyMc8X74}Increased settlement prosperity (or Hearths) and loyalty\nReduced agricultural output\nSerf militarism -1.5% flat\nSerf militia contribution -10%"),
               DemesneLawTypes.SerfDuties,
               -0.5f,
               0.8f,
               -0.2f,
               300);

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
               300);

            SlavesAgricultureDuties.Initialize(new TextObject("{=vt2Sq2aG}Agricultural Duties"),
               new TextObject("{=NXk9mSNW}Tailor the duty laws of {CLASS} towards agriculture. Labor requirements and movement restriction tie the {CLASS} to the land and its productivity. Increased agricultural output.")
               .SetTextVariable("SLAVES", Utils.Helpers.GetClassName(PopulationManager.PopType.Slaves, Hero.MainHero.Culture)),
               new TextObject("{=tXvBhS3n}Increased production of farm goods in villages and food in castles and towns"),
               DemesneLawTypes.SlaveDuties,
               0.4f,
               -0.6f,
               0.5f,
               300);

            SlavesDomesticDuties.Initialize(new TextObject("{=0Ps1q7J0}Domestic Duties"),
               new TextObject("{=Zq8Zcv3N}Tailor the duty laws of {CLASS} towards domestic and skilled labor. Citizen households will often have or want to have slaves for various domestic labors. Enslaved shopkeepers, artisans and professionals provide tax benefits to their owners.")
               .SetTextVariable("CLASS", Utils.Helpers.GetClassName(PopulationManager.PopType.Slaves, Hero.MainHero.Culture)),
               new TextObject("{=P3c1aJgi}Slave tax output +15%"),
               DemesneLawTypes.SlaveDuties,
               0.6f,
               -0.2f,
               0.3f,
               300);

            #endregion SlaveDuties

            #region Slavery

            SlaveryStandard.Initialize(new TextObject("{=6jyLYrts}Calradic Law"), 
                new TextObject("{=ZWmNxRMn}The Imperial or Calradic law stablishes the legal existance of slaves and their ownership. Though they may not be harmed or killed without just cause, the slave trade is rampant and devoid of restrictions. Any person found in debt or captured in battle may be enslaved, and slaves compose the labor force across all settlements."),
                new TextObject("{=!}"),
                DemesneLawTypes.Slavery,
                0.4f,
                -0.6f,
                0.3f,
                300);

            SlaveryVlandia.Initialize(new TextObject("{=hBVZCcL3}Vlandic Law"),
                new TextObject("{=4vBS06Ds}The Vlandic tradition on slavery stipulates that Vlandians shall not enslave each other. Slaves are present in small quantities in rural estates. Though Vlandian individuals may become or be born slaves, Vlandian lords are prohibited from purposefuly enslaving them."),
                new TextObject("{=tK4GJ8bt}Slave demand reduced by 30%\nVlandian prisoners cannot be enslaved\n"),
                DemesneLawTypes.Slavery,
                0.2f,
                -0.2f,
                0.5f,
                300,
                cultures.First(x => x.StringId == "vlandia"));

            SlaveryAserai.Initialize(new TextObject("{=7Q9FJrZ8}Aseran Law"),
               new TextObject("{=szTYxGgd}The Aserai peoples have a long tradition of slavery. Aserai slaves are no more than a trade good, and as such, demand for them is quite high in the economy. ."),
               new TextObject("{=Z5Qopa2c}Slave demand increased by 50%\nSlaves count as military manpower\nSlaves loyalty impact increased by 10%"),
               DemesneLawTypes.Slavery,
               0.3f,
               -0.6f,
               0.6f,
               300,
               cultures.First(x => x.StringId == "aserai"));

            SlaveryManumission.Initialize(new TextObject("{=T4doWFdj}Manumission"),
                new TextObject("{=4vBS06Ds}The Vlandic tradition on slavery stipulates that Vlandians shall not enslave each other. Slaves are present in small quantities in rural estates. Though Vlandian individuals may become or be born slaves, Vlandian lords are prohibited from purposefuly enslaving them."),
                new TextObject("{=kncwdEvN}Slave demand reduced by 100%\n"),
                DemesneLawTypes.Slavery,
                -0.5f,
                0.9f,
                -0.1f,
                300);

            #endregion Slavery

            #region Drafting

            DraftingHidage.Initialize(new TextObject("{=WCkAop4m}Hidage"),
                new TextObject("{=nasOsyDh}Under hidage, landowners are expected to provide levies based on their land, calculated as hides."),
                new TextObject("{=YgwONvnn}Notables provide volunteers based on their estates or power to ally lords\nRural volunteers are restricted to the kingdom's lords\nRecruitment prices increased by 50%"),
                DemesneLawTypes.Drafting,
                0.3f,
                0.1f,
                0.2f,
                300);

            DraftingFreeContracts.Initialize(new TextObject("{=WtxLpuAU}Free Contracts"),
              new TextObject("{=0ZwNdeWq}Free contracts allows levies to serve whoever they wish. No strict duty relationship is set between levies and their suzerains. However, foreigner contractors pay a premium for their services."),
              new TextObject("{=KDRmz1zo}Notables provide volunteers to any neutral or allied lord\nRecruitment prices increased by 100%"),
              DemesneLawTypes.Drafting,
              0.1f,
              0.5f,
              -0.2f,
              300);

            DraftingVassalage.Initialize(new TextObject("{=92A5YP2x}Vassalage"),
              new TextObject("{=zWqQMdHK}Under Vassalage, levies are bound to their most direct suzerain. The de facto and de jure ownerships of fiefs set the precedence for acces to levies."),
              new TextObject("{=i4Y6Sg9k}Notables provide volunteers to their suzerain and armies\nRural volunteers are restricted to the kingdom's lords\nInfluence from settlements reduced by 20%"),
              DemesneLawTypes.Drafting,
              0.5f,
              -0.5f,
              1f,
              300);

            #endregion Drafting

            #region Tenancy

            TenancyFull.Initialize(new TextObject("{=JEbygACX}Full Tenancy"),
              new TextObject("{=uf1BxiQJ}Under Full Tenancy, serfdom does not thrive anymore. Instead, all non-slave commoners will tend to be free tenants, who rent their land under contracts of goods or monetary taxation. Tenants are of higher class than serfs and are not boun to their land, and so have more mobility and are less exploitable for taxes."),
              new TextObject("Serfs will tend to be 100% replaced by tenants\nTenants pay less taxes but are more stable and prosperous"),
              DemesneLawTypes.Tenancy,
              -0.5f,
              1f,
              -0.8f,
              900);

            TenancyMixed.Initialize(new TextObject("{=oT9UmZUt}Mixed Tenure"),
              new TextObject("{=z5ox1p40}Mixed tenure allows the coexistence of both tenants and serfs. Their compositions will tend to be similar."),
              new TextObject("Serfs will tend to be 50% replaced by tenants"),
              DemesneLawTypes.Tenancy,
              0.2f,
              -0.3f,
              0.4f,
              900);

            TenancyNone.Initialize(new TextObject("{=LGuO56aW}Full Serfdom"),
              new TextObject("{=FuKNBsvU}When bound by serfdom, a commoner is unable to leave their suzerain's land without permission. Though their status is above a slave's, they often miss the rights of free men."),
              new TextObject("{=h9UDWQcM}Tenants will tend to be 0% of population\nSerfs yield more taxes, but are more unhappy and produce less economic prosperity"),
              DemesneLawTypes.Tenancy,
              0.8f,
              -1f,
              1f,
              900);

            #endregion Tenancy

            #region Council

            CouncilAppointed.Initialize(new TextObject("{=3fSea0Y1}Appointed Council"),
              new TextObject("{=FuGK7Am5}The realm's high council is appointed by the ruler. They are free to chose their councillors as they please."),
              new TextObject("{=bsaOK0eQ}Ruler's privy council can be altered at will\nAll non-ruling clans have influence cap increased by 5%"),
              DemesneLawTypes.HighCouncil,
              1f,
              -0.3f,
              -0.6f,
              1000);

            CouncilElected.Initialize(new TextObject("{=SP01UfQ4}Elected Council"),
             new TextObject("{=diuCoLDf}The realm's high council is elected by the Peers. The election result will determine the next occupant of the position in question."),
             new TextObject("{=9KKN4gkP}Ruler's privy council can only be altered by elections\nRuler's influence cap is increased by 8%"),
             DemesneLawTypes.HighCouncil,
             -0.7f,
             0.1f,
             0.6f,
             1000);

            #endregion Council
        }
    }
}
