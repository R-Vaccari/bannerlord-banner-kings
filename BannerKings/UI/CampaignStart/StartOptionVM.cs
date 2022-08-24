using System;
using System.Collections.Generic;
using BannerKings.Managers.CampaignStart;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.CampaignStart
{
    public class StartOptionVM : BannerKingsViewModel
    {
        private BasicTooltipViewModel hint;
        private readonly Action<StartOptionVM> onSelect;

        public StartOptionVM(StartOption option, Action<StartOptionVM> onSelect) : base(null, false)
        {
            Option = option;
            this.onSelect = onSelect;
            Hint = new BasicTooltipViewModel(() => GetHint());
        }

        public StartOption Option { get; }

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

        public Action Action => Option.Action;

        [DataSourceProperty] public string ShortDescription => Option.ShortDescription.ToString();

        [DataSourceProperty] public string Name => Option.Name.ToString();

        [DataSourceProperty] public string Gold => Option.Gold.ToString();

        [DataSourceProperty] public string Troops => Option.Troops.ToString();

        [DataSourceProperty] public string Influence => Option.Influence.ToString("0.0");

        [DataSourceProperty] public string Food => Option.Food.ToString();

        [DataSourceProperty] public string Morale => Option.Morale.ToString();

        [DataSourceProperty]
        public string Criminal => GameTexts.FindText(Option.IsCriminal ? "str_yes" : "str_no").ToString();

        private List<TooltipProperty> GetHint()
        {
            var list = new List<TooltipProperty>
            {
                new(string.Empty, Option.Name.ToString(), 0, false, TooltipProperty.TooltipPropertyFlags.Title),
                new(string.Empty, Option.Description.ToString(), 0, false,
                    TooltipProperty.TooltipPropertyFlags.MultiLine)
            };

            UIHelper.TooltipAddEmptyLine(list);
            list.Add(new TooltipProperty(new TextObject("{=nCf63SeHk}Stats").ToString(), " ", 0));
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

            list.Add(new TooltipProperty(new TextObject("{=GdwkCKUEZ}Criminal").ToString(), Criminal, 0));

            return list;
        }

        public void ExecuteSelectOption()
        {
            onSelect(this);
        }
    }
}