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
            MinimalLodgings.Initialize(new TextObject("{=!}Minimal Lodgings"),
                new TextObject("{=!}Provide bare minimum accomodations for guests. Increases guest capacity by 1."),
                -25f,
                0f,
                new Dictionary<ItemCategory, int>()
                {
                    { DefaultItemCategories.Linen, 1 }
                },
                true,
                CourtExpense.ExpenseType.Lodgings);

            SufficientLodgings.Initialize(new TextObject("{=!}Sufficient Lodgings"),
                new TextObject("{=!}Provide sufficient accomodations for guests, both in space and quality. Increases guest capacity by 2. Improves feast satisfaction."),
                10f,
                0.02f,
                new Dictionary<ItemCategory, int>()
                {
                    { DefaultItemCategories.Linen, 1 },
                    { DefaultItemCategories.Wool, 1 }
                },
                true,
                CourtExpense.ExpenseType.Lodgings);

            GoodLodgings.Initialize(new TextObject("{=!}Good Lodgings"),
                new TextObject("{=!}Provide good accomodations for guests, both in space and quality. Increases guest capacity by 3. Further improves feast satisfaction."),
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

            ExcellentLodgings.Initialize(new TextObject("{=!}Excellent Lodgings"),
                new TextObject("{=!}Provide excellent accomodations for guests, both in space and quality. Increases guest capacity by 4. Improves feast satisfaction the most."),
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

            MinimalSupplies.Initialize(new TextObject("{=!}Minimal Supplies"),
               new TextObject("{=!}Stock a minimum amount of supplies to be used later during wars. Buys a very small amount of all types of party supplies."),
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

            SufficientSupplies.Initialize(new TextObject("{=!}Sufficient Supplies"),
                new TextObject("{=!}Stock a small amount of supplies to be used later during wars. Buys a small amount of all types of party supplies."),
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

            GoodSupplies.Initialize(new TextObject("{=!}Good Supplies"),
                new TextObject("{=!}Stock a good amount of supplies to be used later during wars. Buys a good amount of all types of party supplies."),
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

            ExcellentSupplies.Initialize(new TextObject("{=!}Excellent Supplies"),
                new TextObject("{=!}Stock an excellent amount of supplies to be used later during wars. Buys an excellent amount of all types of party supplies."),
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

            MinimalExtravagance.Initialize(new TextObject("{=!}Shoddy Court"),
               new TextObject("{=!}Provide bare minimum accomodations for guests.Increases guest capacity by 1."),
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

            SufficientExtravagance.Initialize(new TextObject("{=!}Modest Court"),
                new TextObject("{=!}Provide bare minimum accomodations for guests.Increases guest capacity by 1."),
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

            GoodExtravagance.Initialize(new TextObject("{=!}Noble Court"),
                new TextObject("{=!}Provide bare minimum accomodations for guests.Increases guest capacity by 1."),
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

            ExcellentExtravagance.Initialize(new TextObject("{=!}Regal Court"),
                new TextObject("{=!}Provide bare minimum accomodations for guests.Increases guest capacity by 1."),
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

            MinimalServants.Initialize(new TextObject("{=!}Minimal Servants"),
               new TextObject("{=!}Hire a small amount of servants to operate mundane tasks such as cleaning or cooking within the court."),
               -25f,
               0.02f,
               new Dictionary<ItemCategory, int>()
               {
                   { DefaultItemCategories.Garment, 5 },
                   { DefaultItemCategories.Pottery, 2 }
               },
               true,
               CourtExpense.ExpenseType.Servants);

            SufficientServants.Initialize(new TextObject("{=!}Sufficient Servants"),
                new TextObject("{=!}Hire a sufficient amount of servants to operate mundane tasks such as cleaning or cooking within the court. Increases feast satisfaction. Decreases defense against schemes."),
                20f,
                0.04f,
                new Dictionary<ItemCategory, int>()
                {
                    { DefaultItemCategories.Garment, 10 },
                    { DefaultItemCategories.Pottery, 4 }
                },
                true,
                CourtExpense.ExpenseType.Servants);

            AmpleServants.Initialize(new TextObject("{=!}Ample Servants"),
                new TextObject("{=!}Hire a good amount of servants to operate mundane tasks such as cleaning or cooking within the court. Further increases feast satisfaction and decreases defense against schemes."),
                80f,
                0.7f,
                new Dictionary<ItemCategory, int>()
                {
                    { DefaultItemCategories.Garment, 20 },
                   { DefaultItemCategories.Pottery, 8 }
                },
                true,
                CourtExpense.ExpenseType.Servants);

            PlentifulServants.Initialize(new TextObject("{=!}Plentiful Servants"),
                new TextObject("{=!}Hire a small amount of servants to operate mundane tasks such as cleaning or cooking within the court. Increases feast satisfaction and decreases defense against schemes the most."),
                120f,
                0.1f,
                new Dictionary<ItemCategory, int>()
                {
                    { DefaultItemCategories.Garment, 40 },
                    { DefaultItemCategories.Pottery, 16 }
                },
                true,
                CourtExpense.ExpenseType.Servants);

            MinimalSecurity.Initialize(new TextObject("{=!}Minimal Security"),
                new TextObject("{=!}Provide bare minimum accomodations for guests.Increases guest capacity by 1."),
                -25f,
                0f,
                new Dictionary<ItemCategory, int>()
                {
                    { DefaultItemCategories.Linen, 1 }
                },
                true,
                CourtExpense.ExpenseType.Security);

            SufficientSecurity.Initialize(new TextObject("{=!}Sufficient Security"),
                new TextObject("{=!}Provide bare minimum accomodations for guests.Increases guest capacity by 1."),
                10f,
                0.02f,
                new Dictionary<ItemCategory, int>()
                {
                    { DefaultItemCategories.Linen, 2 }
                },
                true,
                CourtExpense.ExpenseType.Security);

            GoodSecurity.Initialize(new TextObject("{=!}Good Security"),
                new TextObject("{=!}Provide bare minimum accomodations for guests.Increases guest capacity by 1."),
                40f,
                0.04f,
                new Dictionary<ItemCategory, int>()
                {
                    { DefaultItemCategories.Linen, 2 },
                    { DefaultItemCategories.Wool, 2 }
                },
                true,
                CourtExpense.ExpenseType.Security);

            ExcellentSecurity.Initialize(new TextObject("{=!}Excellent Security"),
                new TextObject("{=!}Provide bare minimum accomodations for guests.Increases guest capacity by 1."),
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
