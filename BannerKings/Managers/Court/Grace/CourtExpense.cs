using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Court.Grace
{
    public class CourtExpense : BannerKingsObject
    {
        public CourtExpense(string stringId) : base(stringId)
        {
        }

        public float Grace { get; private set; }
        public Dictionary<ItemCategory, int> ItemCategories { get; private set; }
        public ExpenseType Type { get; private set; }
        public float AdministrativeCost { get; private set; }
        public bool ConsumeItems { get; private set; }

        public void Initialize(TextObject name, TextObject description, float grace, float admCost,
            Dictionary<ItemCategory, int> itemCategories, bool consumeItems, ExpenseType type)
        {
            Initialize(name, description);
            Grace = grace;
            ItemCategories = itemCategories;
            Type = type;
            AdministrativeCost = admCost;
            ConsumeItems = consumeItems;
        }

        public void PostInitialize()
        {
            CourtExpense c = DefaultCourtExpenses.Instance.GetById(this);
            Initialize(c.Name, c.Description, c.Grace, c.AdministrativeCost, c.ItemCategories, c.ConsumeItems,
                c.Type);
        }

        public enum ExpenseType
        {
            Lodgings,
            Servants,
            Supplies,
            Security,
            Extravagance
        }

        public override bool Equals(object obj)
        {
            if (obj is CourtExpense) {
                var c = (obj as CourtExpense);
                return Type == c.Type && StringId == c.StringId;
            }
            return base.Equals(obj);
        }
    }
}
