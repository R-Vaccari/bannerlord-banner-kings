using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Court.Grace
{
    public class DefaultCourtExpenses : DefaultTypeInitializer<DefaultCourtExpenses, CourtExpense>
    {
        public CourtExpense MinimalLodgings { get; } = new CourtExpense("MinimalLodgings");
        public CourtExpense SufficientLodgings { get; } = new CourtExpense("SufficientLodgings");
        public CourtExpense GoodLodgings { get; } = new CourtExpense("GoodLodgings");
        public CourtExpense ExcellentLodgings { get; } = new CourtExpense("ExcellentLodgings");

        public CourtExpense MinimalServants { get; } = new CourtExpense("MinimalServants");
        public CourtExpense SufficientServants { get; } = new CourtExpense("SufficientServants");
        public CourtExpense AmpleServants { get; } = new CourtExpense("AmpleServants");
        public CourtExpense PlentifulServants { get; } = new CourtExpense("PlentifulServants");

        public CourtExpense MinimalSecurity { get; } = new CourtExpense("MinimalSecurity");
        public CourtExpense SufficientSecurity { get; } = new CourtExpense("SufficientSecurity");
        public CourtExpense GoodSecurity { get; } = new CourtExpense("GoodSecurity");
        public CourtExpense ExcellentSecurity { get; } = new CourtExpense("ExcellentSecurity");

        public CourtExpense MinimalExtravagance { get; } = new CourtExpense("MinimalExtravagance");
        public CourtExpense SufficientExtravagance { get; } = new CourtExpense("SufficientExtravagance");
        public CourtExpense GoodExtravagance { get; } = new CourtExpense("GoodExtravagance");
        public CourtExpense ExcellentExtravagance { get; } = new CourtExpense("ExcellentExtravagance");

        public CourtExpense MinimalSupplies { get; } = new CourtExpense("MinimalSupplies");
        public CourtExpense SufficientSupplies { get; } = new CourtExpense("SufficientSupplies");
        public CourtExpense GoodSupplies { get; } = new CourtExpense("GoodSupplies");
        public CourtExpense ExcellentSupplies { get; } = new CourtExpense("ExcellentSupplies");

        public override IEnumerable<CourtExpense> All
        {
            get
            {
                yield return MinimalExtravagance;
                yield return SufficientExtravagance;
                yield return GoodExtravagance;
                yield return ExcellentExtravagance;
                yield return MinimalLodgings;
                yield return SufficientLodgings;
                yield return GoodLodgings;
                yield return ExcellentLodgings;
                yield return MinimalSupplies;
                yield return SufficientSupplies;
                yield return GoodSupplies;
                yield return ExcellentSupplies;
                yield return MinimalServants;
                yield return SufficientServants;
                yield return AmpleServants;
                yield return PlentifulServants;
                yield return MinimalSecurity;
                yield return SufficientSecurity;
                yield return GoodSecurity;
                yield return ExcellentSecurity;
            }
        }

        public override void Initialize()
        {
            MinimalLodgings.Initialize(new TextObject("{=Y0NWoa6b}Minimal Lodgings"),
                new TextObject("{=pZyTEB62}Provide bare minimum accomodations for guests. Increases guest capacity by 1."),
                -25f,
                0f,
                new Dictionary<ItemCategory, int>()
                {
                    { DefaultItemCategories.Linen, 1 }
                },
                true,
                CourtExpense.ExpenseType.Lodgings);

            SufficientLodgings.Initialize(new TextObject("{=p0WegVVv}Sufficient Lodgings"),
                new TextObject("{=ju5hjtTV}Provide sufficient accomodations for guests, both in space and quality. Increases guest capacity by 2. Improves feast satisfaction."),
                10f,
                0.02f,
                new Dictionary<ItemCategory, int>()
                {
                    { DefaultItemCategories.Linen, 1 },
                    { DefaultItemCategories.Wool, 1 }
                },
                true,
                CourtExpense.ExpenseType.Lodgings);

            GoodLodgings.Initialize(new TextObject("{=8kQyH2uu}Good Lodgings"),
                new TextObject("{=J4x60RZT}Provide good accomodations for guests, both in space and quality. Increases guest capacity by 3. Further improves feast satisfaction."),
                40f,
                0.04f,
                new Dictionary<ItemCategory, int>()
                {
                    { DefaultItemCategories.Linen, 2 },
                    { DefaultItemCategories.Wool, 2 },
                    { DefaultItemCategories.Hides, 1 }
                },
                true,
                CourtExpense.ExpenseType.Lodgings);

            ExcellentLodgings.Initialize(new TextObject("{=Lemq7MqD}Excellent Lodgings"),
                new TextObject("{=fcx3fZ1r}Provide excellent accomodations for guests, both in space and quality. Increases guest capacity by 4. Improves feast satisfaction the most."),
                80f,
                0.06f,
                new Dictionary<ItemCategory, int>()
                {
                    { DefaultItemCategories.Linen, 3 },
                    { DefaultItemCategories.Wool, 3 },
                    { DefaultItemCategories.Hides, 2 },
                    { DefaultItemCategories.Velvet, 1 }
                },
                true,
                CourtExpense.ExpenseType.Lodgings);

            MinimalSupplies.Initialize(new TextObject("{=ZqEzWPMx}Minimal Supplies"),
               new TextObject("{=FmWniY4S}Stock a minimum amount of supplies to be used later during wars. Buys a very small amount of all types of party supplies."),
               -25f,
               0f,
               new Dictionary<ItemCategory, int>()
               {
                    { DefaultItemCategories.Linen, 1 },
                    { DefaultItemCategories.Wood, 1 },
                    { DefaultItemCategories.Tools, 1 },
                    { DefaultItemCategories.Shield1, 1 },
                    { DefaultItemCategories.MeleeWeapons1, 1 },
                    { DefaultItemCategories.Arrows, 1 },
                    { DefaultItemCategories.Horse, 1 },
                    { DefaultItemCategories.Meat, 1 },
                    { DefaultItemCategories.Beer, 1 }
               },
               false,
               CourtExpense.ExpenseType.Supplies);

            SufficientSupplies.Initialize(new TextObject("{=VmWFHoYw}Sufficient Supplies"),
                new TextObject("{=4NTpE1Pt}Stock a small amount of supplies to be used later during wars. Buys a small amount of all types of party supplies."),
                10f,
                0.02f,
                new Dictionary<ItemCategory, int>()
                {
                    { DefaultItemCategories.Linen, 1 },
                    { DefaultItemCategories.Wood, 1 },
                    { DefaultItemCategories.Tools, 1 },
                    { DefaultItemCategories.Shield1, 1 },
                    { DefaultItemCategories.MeleeWeapons1, 1 },
                    { DefaultItemCategories.Arrows, 1 },
                    { DefaultItemCategories.Horse, 1 },
                    { DefaultItemCategories.Meat, 1 },
                    { DefaultItemCategories.Beer, 1 }
                },
                false,
                CourtExpense.ExpenseType.Supplies);

            GoodSupplies.Initialize(new TextObject("{=DGhLNj66}Good Supplies"),
                new TextObject("{=SyjW0CfG}Stock a good amount of supplies to be used later during wars. Buys a good amount of all types of party supplies."),
                40f,
                0.04f,
                new Dictionary<ItemCategory, int>()
                {
                    { DefaultItemCategories.Linen, 1 },
                    { DefaultItemCategories.Wood, 1 },
                    { DefaultItemCategories.Tools, 1 },
                    { DefaultItemCategories.Shield1, 1 },
                    { DefaultItemCategories.MeleeWeapons1, 1 },
                    { DefaultItemCategories.Arrows, 1 },
                    { DefaultItemCategories.Horse, 1 },
                    { DefaultItemCategories.Meat, 1 },
                    { DefaultItemCategories.Beer, 1 }
                },
                false,
                CourtExpense.ExpenseType.Supplies);

            ExcellentSupplies.Initialize(new TextObject("{=hfi3Dviw}Excellent Supplies"),
                new TextObject("{=SS4K6gtP}Stock an excellent amount of supplies to be used later during wars. Buys an excellent amount of all types of party supplies."),
                80f,
                0.06f,
                new Dictionary<ItemCategory, int>()
                {
                    { DefaultItemCategories.Linen, 1 },
                    { DefaultItemCategories.Wood, 1 },
                    { DefaultItemCategories.Tools, 1 },
                    { DefaultItemCategories.Shield1, 1 },
                    { DefaultItemCategories.MeleeWeapons1, 1 },
                    { DefaultItemCategories.Arrows, 1 },
                    { DefaultItemCategories.Horse, 1 },
                    { DefaultItemCategories.Meat, 1 },
                    { DefaultItemCategories.Beer, 1 }
                },
                false,
                CourtExpense.ExpenseType.Supplies);

            MinimalExtravagance.Initialize(new TextObject("{=Wojb1tFw}Shoddy Court"),
               new TextObject("{=pZyTEB62}Provide bare minimum accomodations for guests. Increases guest capacity by 1."),
               -50f,
               0.02f,
               new Dictionary<ItemCategory, int>()
               {
                    { DefaultItemCategories.Wine, 1 },
                    { DefaultItemCategories.Beer, 4 },
                    { DefaultItemCategories.Oil, 2 },
                    { DefaultItemCategories.Salt, 2 }
               },
               true,
               CourtExpense.ExpenseType.Extravagance);

            SufficientExtravagance.Initialize(new TextObject("{=sMGjTVpS}Modest Court"),
                new TextObject("{=pZyTEB62}Provide bare minimum accomodations for guests. Increases guest capacity by 1."),
                50f,
                0.06f,
                new Dictionary<ItemCategory, int>()
                {
                    { DefaultItemCategories.Wine, 2 },
                    { DefaultItemCategories.Beer, 6 },
                    { DefaultItemCategories.Oil, 4 },
                    { DefaultItemCategories.Salt, 4 },
                    { DefaultItemCategories.Fur, 1 }
                },
                true,
                CourtExpense.ExpenseType.Extravagance);

            GoodExtravagance.Initialize(new TextObject("{=hv34pxAv}Noble Court"),
                new TextObject("{=pZyTEB62}Provide bare minimum accomodations for guests. Increases guest capacity by 1."),
                150f,
                0.1f,
                new Dictionary<ItemCategory, int>()
                {
                    { DefaultItemCategories.Wine, 4 },
                    { DefaultItemCategories.Beer, 8 },
                    { DefaultItemCategories.Oil, 6 },
                    { DefaultItemCategories.Salt, 6 },
                    { DefaultItemCategories.Fur, 2 },
                    { DefaultItemCategories.Velvet, 1 },
                },
                true,
                CourtExpense.ExpenseType.Extravagance);

            ExcellentExtravagance.Initialize(new TextObject("{=FU9MdDPz}Regal Court"),
                new TextObject("{=pZyTEB62}Provide bare minimum accomodations for guests. Increases guest capacity by 1."),
                250f,
                0.14f,
                new Dictionary<ItemCategory, int>()
                {
                    { DefaultItemCategories.Wine, 8 },
                    { DefaultItemCategories.Beer, 12 },
                    { DefaultItemCategories.Oil, 8 },
                    { DefaultItemCategories.Salt, 8 },
                    { DefaultItemCategories.Fur, 3 },
                    { DefaultItemCategories.Velvet, 2 },
                    { DefaultItemCategories.Jewelry, 1 }
                },
                true,
                CourtExpense.ExpenseType.Extravagance);

            MinimalServants.Initialize(new TextObject("{=awPo7QsQ}Minimal Servants"),
               new TextObject("{=VCbuZK6B}Hire a small amount of servants to operate mundane tasks such as cleaning or cooking within the court."),
               -25f,
               0.02f,
               new Dictionary<ItemCategory, int>()
               {
                   { DefaultItemCategories.Garment, 5 },
                   { DefaultItemCategories.Pottery, 2 }
               },
               true,
               CourtExpense.ExpenseType.Servants);

            SufficientServants.Initialize(new TextObject("{=8R4F0LH1}Sufficient Servants"),
                new TextObject("{=ZDOuCuYp}Hire a sufficient amount of servants to operate mundane tasks such as cleaning or cooking within the court. Increases feast satisfaction. Decreases defense against schemes."),
                20f,
                0.04f,
                new Dictionary<ItemCategory, int>()
                {
                    { DefaultItemCategories.Garment, 10 },
                    { DefaultItemCategories.Pottery, 4 }
                },
                true,
                CourtExpense.ExpenseType.Servants);

            AmpleServants.Initialize(new TextObject("{=6SJgGAVR}Ample Servants"),
                new TextObject("{=DnjmWQV6}Hire a good amount of servants to operate mundane tasks such as cleaning or cooking within the court. Further increases feast satisfaction and decreases defense against schemes."),
                80f,
                0.07f,
                new Dictionary<ItemCategory, int>()
                {
                    { DefaultItemCategories.Garment, 20 },
                   { DefaultItemCategories.Pottery, 8 }
                },
                true,
                CourtExpense.ExpenseType.Servants);

            PlentifulServants.Initialize(new TextObject("{=Y8OMJJ8P}Plentiful Servants"),
                new TextObject("{=2tr8Q4jj}Hire a small amount of servants to operate mundane tasks such as cleaning or cooking within the court. Increases feast satisfaction and decreases defense against schemes the most."),
                120f,
                0.1f,
                new Dictionary<ItemCategory, int>()
                {
                    { DefaultItemCategories.Garment, 40 },
                    { DefaultItemCategories.Pottery, 16 }
                },
                true,
                CourtExpense.ExpenseType.Servants);

            MinimalSecurity.Initialize(new TextObject("{=pxxHAa57}Minimal Security"),
                new TextObject("{=pZyTEB62}Provide bare minimum accomodations for guests. Increases guest capacity by 1."),
                -25f,
                0f,
                new Dictionary<ItemCategory, int>()
                {
                    { DefaultItemCategories.Linen, 1 }
                },
                true,
                CourtExpense.ExpenseType.Security);

            SufficientSecurity.Initialize(new TextObject("{=6oCNmffJ}Sufficient Security"),
                new TextObject("{=pZyTEB62}Provide bare minimum accomodations for guests. Increases guest capacity by 1."),
                10f,
                0.02f,
                new Dictionary<ItemCategory, int>()
                {
                    { DefaultItemCategories.Linen, 2 }
                },
                true,
                CourtExpense.ExpenseType.Security);

            GoodSecurity.Initialize(new TextObject("{=sRWDgNgy}Good Security"),
                new TextObject("{=pZyTEB62}Provide bare minimum accomodations for guests. Increases guest capacity by 1."),
                40f,
                0.04f,
                new Dictionary<ItemCategory, int>()
                {
                    { DefaultItemCategories.Linen, 2 },
                    { DefaultItemCategories.Wool, 2 }
                },
                true,
                CourtExpense.ExpenseType.Security);

            ExcellentSecurity.Initialize(new TextObject("{=U41twbPF}Excellent Security"),
                new TextObject("{=pZyTEB62}Provide bare minimum accomodations for guests. Increases guest capacity by 1."),
                80f,
                0.06f,
                new Dictionary<ItemCategory, int>()
                {
                    { DefaultItemCategories.Linen, 3 },
                    { DefaultItemCategories.Wool, 3 },
                    { DefaultItemCategories.Velvet, 1 }
                },
                true,
                CourtExpense.ExpenseType.Security);
        }
    }
}
