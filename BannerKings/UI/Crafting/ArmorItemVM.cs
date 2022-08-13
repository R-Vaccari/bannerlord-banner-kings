using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Crafting
{
    public class ArmorItemVM : ViewModel
    {

        private ItemObject item;
		private ImageIdentifierVM visual;
		private BasicTooltipViewModel hint;

		public ArmorItemVM(ItemObject item)
        {
            this.item = item;
			Visual = new ImageIdentifierVM(item, "");
			Hint = new BasicTooltipViewModel(() => GetHint());
		}

        public override void RefreshValues()
        {
            base.RefreshValues();
		}

		private List<TooltipProperty> GetHint()
        {
			List<TooltipProperty> list = new List<TooltipProperty>
			{
				new TooltipProperty("", item.Name.ToString(), 0, false, TooltipProperty.TooltipPropertyFlags.Title)
			};


			MBTextManager.SetTextVariable("LEFT", GameTexts.FindText("str_tooltip_label_type"));
			list.Add(new TooltipProperty(GameTexts.FindText("str_LEFT_ONLY").ToString(), GameTexts.FindText("str_inventory_type_" + item.ItemType)
				.ToString(), 0));


			if (item.Culture != null)
			{
				MBTextManager.SetTextVariable("LEFT", GameTexts.FindText("str_culture"));
				list.Add(new TooltipProperty(GameTexts.FindText("str_LEFT_ONLY").ToString(), item.Culture.Name.ToString(), 0));
			}

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
