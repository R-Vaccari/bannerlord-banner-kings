using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Crafting
{
    public class ArmorItemVM : BannerKingsViewModel
    {
		private ArmorCraftingVM armorCrafting;
        private ItemObject item;
		private ImageIdentifierVM visual;
		private BasicTooltipViewModel hint;

		public ArmorItemVM(ArmorCraftingVM armorCrafting, ItemObject item) : base(null, false)
        {
			this.armorCrafting = armorCrafting;
            this.item = item;
			Visual = new ImageIdentifierVM(item, "");
			Hint = new BasicTooltipViewModel(() => GetHint());
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
			List<TooltipProperty> list = new List<TooltipProperty>
			{
				new TooltipProperty("", item.Name.ToString(), 0, false, TooltipProperty.TooltipPropertyFlags.Title)
			};


			MBTextManager.SetTextVariable("LEFT", GameTexts.FindText("str_tooltip_label_type"));
			list.Add(new TooltipProperty(GameTexts.FindText("str_LEFT_ONLY").ToString(), GameTexts.FindText("str_inventory_type_" + (int)item.ItemType)
				.ToString(), 0));


			if (item.Culture != null)
			{
				MBTextManager.SetTextVariable("LEFT", GameTexts.FindText("str_culture"));
				list.Add(new TooltipProperty(GameTexts.FindText("str_LEFT_ONLY").ToString(), item.Culture.Name.ToString(), 0));
			}

			MBTextManager.SetTextVariable("LEFT", GameTexts.FindText("str_value"));
			list.Add(new TooltipProperty(GameTexts.FindText("str_LEFT_ONLY").ToString(), item.Value.ToString(), 0));

			MBTextManager.SetTextVariable("LEFT", GameTexts.FindText("str_value"));
			list.Add(new TooltipProperty("Tier", item.Tierf.ToString(), 0));

			MBTextManager.SetTextVariable("LEFT", GameTexts.FindText("str_crafting_stat", "Weight"));
			list.Add(new TooltipProperty(GameTexts.FindText("str_LEFT_ONLY").ToString().Replace(":", ""), item.Weight.ToString(), 0));

			MBTextManager.SetTextVariable("LEFT", new TextObject("{=!}Material"));
			list.Add(new TooltipProperty(GameTexts.FindText("str_LEFT_ONLY").ToString(), item.ArmorComponent.MaterialType.ToString(), 0));



			UIHelper.TooltipAddEmptyLine(list);
			list.Add(new TooltipProperty(new TextObject("{=!}Armor").ToString(), " ", 0));
			UIHelper.TooltipAddSeperator(list);

			MBTextManager.SetTextVariable("LEFT", GameTexts.FindText("str_inventory_head_armor"));
			list.Add(new TooltipProperty(GameTexts.FindText("str_LEFT_ONLY").ToString(), item.ArmorComponent.HeadArmor.ToString(), 0));

			MBTextManager.SetTextVariable("LEFT", GameTexts.FindText("str_inventory_body_armor"));
			list.Add(new TooltipProperty(GameTexts.FindText("str_LEFT_ONLY").ToString(), item.ArmorComponent.BodyArmor.ToString(), 0));

			MBTextManager.SetTextVariable("LEFT", GameTexts.FindText("str_inventory_leg_armor"));
			list.Add(new TooltipProperty(GameTexts.FindText("str_LEFT_ONLY").ToString(), item.ArmorComponent.LegArmor.ToString(), 0));

			MBTextManager.SetTextVariable("LEFT", GameTexts.FindText("str_inventory_arm_armor"));
			list.Add(new TooltipProperty(GameTexts.FindText("str_LEFT_ONLY").ToString(), item.ArmorComponent.ArmArmor.ToString(), 0));



			UIHelper.TooltipAddEmptyLine(list);
			list.Add(new TooltipProperty(GameTexts.FindText("str_crafting").ToString(), " ", 0));
			UIHelper.TooltipAddSeperator(list);

			int difficulty = BannerKingsConfig.Instance.SmithingModel.CalculateArmorDifficulty(item);
			MBTextManager.SetTextVariable("LEFT", GameTexts.FindText("str_crafting_difficulty"));
			list.Add(new TooltipProperty(GameTexts.FindText("str_LEFT_ONLY").ToString(),difficulty.ToString(), 0));

			list.Add(new TooltipProperty(new TextObject("{=!}Stamina").ToString(), 
				BannerKingsConfig.Instance.SmithingModel.CalculateArmorStamina(item).ToString(), 0));

			list.Add(new TooltipProperty(new TextObject("{=!}Botching Chance").ToString(),
				FormatValue(BannerKingsConfig.Instance.SmithingModel.CalculateBotchingChance(armorCrafting.Hero, difficulty)), 0));


			UIHelper.TooltipAddEmptyLine(list);
			list.Add(new TooltipProperty(new TextObject("{=!}Materials").ToString(), " ", 0));
			UIHelper.TooltipAddSeperator(list);

			int[] materials = BannerKingsConfig.Instance.SmithingModel.GetSmeltingOutputForArmor(item);
			for (int l = 0; l < 11; l++)
            {
				if (materials[l] == 0) continue;

				string name;
				if (l < 9) name = BannerKingsConfig.Instance.SmithingModel.GetCraftingMaterialItem((CraftingMaterials)l).Name.ToString();
				else name = GameTexts.FindText("str_item_category", l == 9 ? "leather" : "linen").ToString();

				list.Add(new TooltipProperty(name, materials[l].ToString(), 0));
			}


			return list;
		}

		[DataSourceProperty]
		public string ItemName => item.Name.ToString();

		[DataSourceProperty]
		public ItemObject Item => item;

		[DataSourceProperty]
		public BasicTooltipViewModel Hint
		{
			get => hint;
			set
			{
				if (value != hint)
				{
					hint = value;
					OnPropertyChangedWithValue(value, "Hint");
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
					OnPropertyChangedWithValue(value, "Visual");
				}
			}
		}
	}
}
