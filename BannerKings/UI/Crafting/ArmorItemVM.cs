using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Crafting
{
    public class ArmorItemVM : BannerKingsViewModel
    {
        private readonly ArmorCraftingVM armorCrafting;
        private readonly int stamina;
        private BasicTooltipViewModel hint;
        private ImageIdentifierVM visual;

        public ArmorItemVM(ArmorCraftingVM armorCrafting, ItemObject item, ArmorCraftingVM.ItemType type) : base(null,
            false)
        {
            this.armorCrafting = armorCrafting;
            Item = item;
            Visual = new ImageIdentifierVM(item);
            Hint = new BasicTooltipViewModel(() => GetHint());
            stamina = BannerKingsConfig.Instance.SmithingModel.CalculateArmorStamina(item, armorCrafting.Hero);
            Difficulty = BannerKingsConfig.Instance.SmithingModel.CalculateArmorDifficulty(item);
            ItemType = type;
        }

        [DataSourceProperty] public string ItemName => Item.Name.ToString();

        [DataSourceProperty] public ArmorCraftingVM.ItemType ItemType { get; }

        [DataSourceProperty]
        public string ItemTypeText =>
            GameTexts.FindText("str_bk_crafting_itemtype", ItemType.ToString().ToLower()).ToString();

        [DataSourceProperty] public ItemObject Item { get; }

        [DataSourceProperty]
        public string ValueText => new TextObject("{=3RyRLEoG}{GOLD} denarii")
            .SetTextVariable("GOLD", $"{Item.Value:n0}")
            .ToString();

        [DataSourceProperty] public int Difficulty { get; }

        [DataSourceProperty]
        public string DifficultyText => Difficulty + " " + GameTexts.FindText("str_crafting_difficulty");

        [DataSourceProperty] public string StaminaText => stamina + " " + new TextObject("{=yceCZuEm}Stamina");

        [DataSourceProperty]
        public BasicTooltipViewModel Hint
        {
            get => hint;
            set
            {
                if (value != hint)
                {
                    hint = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public ImageIdentifierVM Visual
        {
            get => visual;
            set
            {
                if (value != visual)
                {
                    visual = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
        }

        public void ExecuteSelection()
        {
            armorCrafting.CurrentItem = this;
        }

        private List<TooltipProperty> GetHint()
        {
            var list = new List<TooltipProperty>
            {
                new("", Item.Name.ToString(), 0, false, TooltipProperty.TooltipPropertyFlags.Title)
            };


            MBTextManager.SetTextVariable("LEFT", GameTexts.FindText("str_tooltip_label_type"));
            list.Add(new TooltipProperty(GameTexts.FindText("str_LEFT_ONLY").ToString(), GameTexts
                .FindText("str_inventory_type_" + (int) Item.ItemType)
                .ToString(), 0));


            if (Item.Culture != null && Item.Culture.Name != null)
            {
                MBTextManager.SetTextVariable("LEFT", GameTexts.FindText("str_culture"));
                list.Add(new TooltipProperty(GameTexts.FindText("str_LEFT_ONLY").ToString(), Item.Culture.Name.ToString(),
                    0));
            }

            MBTextManager.SetTextVariable("LEFT", GameTexts.FindText("str_value"));
            list.Add(new TooltipProperty(GameTexts.FindText("str_LEFT_ONLY").ToString(), Item.Value.ToString(), 0));

            MBTextManager.SetTextVariable("LEFT", GameTexts.FindText("str_value"));
            list.Add(new TooltipProperty("Tier", Item.Tierf.ToString(), 0));

            MBTextManager.SetTextVariable("LEFT", GameTexts.FindText("str_crafting_stat", "Weight"));
            list.Add(new TooltipProperty(GameTexts.FindText("str_LEFT_ONLY").ToString().Replace(":", ""),
                Item.Weight.ToString(), 0));

            if (Item.HasArmorComponent)
            {
                MBTextManager.SetTextVariable("LEFT", new TextObject("{=4aKx5Pj9}Material"));
                list.Add(new TooltipProperty(GameTexts.FindText("str_LEFT_ONLY").ToString(),
                    Item.ArmorComponent.MaterialType.ToString(), 0));


                UIHelper.TooltipAddEmptyLine(list);
                list.Add(new TooltipProperty(new TextObject("{=xuzbT4GO}Armor").ToString(), " ", 0));
                UIHelper.TooltipAddSeperator(list);

                MBTextManager.SetTextVariable("LEFT", GameTexts.FindText("str_inventory_head_armor"));
                list.Add(new TooltipProperty(GameTexts.FindText("str_LEFT_ONLY").ToString(),
                    Item.ArmorComponent.HeadArmor.ToString(), 0));

                MBTextManager.SetTextVariable("LEFT", GameTexts.FindText("str_inventory_body_armor"));
                list.Add(new TooltipProperty(GameTexts.FindText("str_LEFT_ONLY").ToString(),
                    Item.ArmorComponent.BodyArmor.ToString(), 0));

                MBTextManager.SetTextVariable("LEFT", GameTexts.FindText("str_inventory_leg_armor"));
                list.Add(new TooltipProperty(GameTexts.FindText("str_LEFT_ONLY").ToString(),
                    Item.ArmorComponent.LegArmor.ToString(), 0));

                MBTextManager.SetTextVariable("LEFT", GameTexts.FindText("str_inventory_arm_armor"));
                list.Add(new TooltipProperty(GameTexts.FindText("str_LEFT_ONLY").ToString(),
                    Item.ArmorComponent.ArmArmor.ToString(), 0));
            }


            UIHelper.TooltipAddEmptyLine(list);
            list.Add(new TooltipProperty(GameTexts.FindText("str_crafting").ToString(), " ", 0));
            UIHelper.TooltipAddSeperator(list);


            MBTextManager.SetTextVariable("LEFT", GameTexts.FindText("str_crafting_difficulty"));
            list.Add(new TooltipProperty(GameTexts.FindText("str_LEFT_ONLY").ToString(), Difficulty.ToString(), 0));
            list.Add(new TooltipProperty(new TextObject("{=yceCZuEm}Stamina").ToString(), stamina.ToString(), 0));
            list.Add(new TooltipProperty(new TextObject("{=mBSXX0zj}Botching Chance").ToString(),
                FormatValue(
                    BannerKingsConfig.Instance.SmithingModel.CalculateBotchingChance(armorCrafting.Hero, Difficulty)), 0));


            UIHelper.TooltipAddEmptyLine(list);
            list.Add(new TooltipProperty(new TextObject("{=4aKx5Pj9}Materials").ToString(), " ", 0));
            UIHelper.TooltipAddSeperator(list);

            var materials = BannerKingsConfig.Instance.SmithingModel.GetCraftingInputForArmor(Item);
            for (var l = 0; l < 11; l++)
            {
                if (materials[l] == 0)
                {
                    continue;
                }

                string name;
                if (l < 9)
                {
                    name = BannerKingsConfig.Instance.SmithingModel.GetCraftingMaterialItem((CraftingMaterials) l).Name.ToString();
                }
                else
                {
                    name = GameTexts.FindText("str_item_category", l == 9 ? "leather" : "linen").ToString();
                }

                list.Add(new TooltipProperty(name, materials[l].ToString(), 0));
            }


            return list;
        }
    }
}