using BannerKings.Managers.CampaignStart;
using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.CampaignStart
{
    public class StartOptionVM : BannerKingsViewModel
    {
        private StartOption option;
        private Action<StartOptionVM> onSelect;
        private BasicTooltipViewModel hint;

        public StartOptionVM(StartOption option, Action<StartOptionVM> onSelect) : base(null, false)
        {
            this.option = option;
            this.onSelect = onSelect;
            Hint = new BasicTooltipViewModel(() => GetHint()); 
        }

        private List<TooltipProperty> GetHint()
        {
            List<TooltipProperty> list = new List<TooltipProperty>
            {
                new TooltipProperty(string.Empty, option.Name.ToString(), 0, false, TooltipProperty.TooltipPropertyFlags.Title)
            };

            list.Add(new TooltipProperty(string.Empty, option.Description.ToString(), 0, false, TooltipProperty.TooltipPropertyFlags.MultiLine));

            UIHelper.TooltipAddEmptyLine(list);
            list.Add(new TooltipProperty(new TextObject("{=!}Stats").ToString(), " ", 0));
            UIHelper.TooltipAddSeperator(list);

            MBTextManager.SetTextVariable("LEFT", GameTexts.FindText("str_gold"));
            list.Add(new TooltipProperty(GameTexts.FindText("str_LEFT_ONLY").ToString(), Gold, 0));

            MBTextManager.SetTextVariable("LEFT", GameTexts.FindText("str_influence"));
            list.Add(new TooltipProperty(GameTexts.FindText("str_LEFT_ONLY").ToString(), Influence, 0));

            MBTextManager.SetTextVariable("LEFT", GameTexts.FindText("str_food"));
            list.Add(new TooltipProperty(GameTexts.FindText("str_LEFT_ONLY").ToString(), Food, 0));

            MBTextManager.SetTextVariable("LEFT", GameTexts.FindText("str_troops_group"));
            list.Add(new TooltipProperty(GameTexts.FindText("str_LEFT_ONLY").ToString(), Troops, 0));

            MBTextManager.SetTextVariable("LEFT", GameTexts.FindText("str_party_morale"));
            list.Add(new TooltipProperty(GameTexts.FindText("str_LEFT_ONLY").ToString(), Morale, 0));

            list.Add(new TooltipProperty(new TextObject("{=!}Criminal").ToString(), Criminal, 0));

            return list;
        }

        public void ExecuteSelectOption()
        {
            onSelect(this);
        }

        public StartOption Option => option;

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

        public Action Action => option.Action;

        [DataSourceProperty]
        public string ShortDescription => option.ShortDescription.ToString();

        [DataSourceProperty]
        public string Name => option.Name.ToString();

        [DataSourceProperty]
        public string Gold => option.Gold.ToString();

        [DataSourceProperty]
        public string Troops => option.Troops.ToString();

        [DataSourceProperty]
        public string Influence => option.Influence.ToString("0.0");

        [DataSourceProperty]
        public string Food => option.Food.ToString();

        [DataSourceProperty]
        public string Morale => option.Morale.ToString();

        [DataSourceProperty]
        public string Criminal => GameTexts.FindText(option.IsCriminal ? "str_yes" : "str_no").ToString();
    }
}
