using BannerKings.Managers.Institutions.Religions;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.CampaignStart
{
    public class ReligionStartOptionVM : BannerKingsViewModel
    {
        private BasicTooltipViewModel hint;
        private readonly Action<ReligionStartOptionVM> onSelect;

        public ReligionStartOptionVM(Managers.Institutions.Religions.Religion option, 
            Action<ReligionStartOptionVM> onSelect) : base(null, false)
        {
            Religion = option;
            this.onSelect = onSelect;
            Hint = new BasicTooltipViewModel(() => GetHint());
        }

        public Managers.Institutions.Religions.Religion Religion { get; }

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

        [DataSourceProperty] public ImageIdentifierVM Banner => new ImageIdentifierVM(BannerCode.CreateFrom(Religion.Faith.GetBanner()), true);
        [DataSourceProperty] public string ShortDescription => Religion.Faith.GetFaithDescription().ToString();
        [DataSourceProperty] public string Name => Religion.Faith.GetFaithName().ToString();
        [DataSourceProperty] public string Piety => BannerKingsConfig.Instance.ReligionsManager.GetStartingPiety(Religion,
            Hero.MainHero).ToString();

        private List<TooltipProperty> GetHint()
        {
            var list = new List<TooltipProperty>
            {
                new(string.Empty, Name.ToString(), 0, false, TooltipProperty.TooltipPropertyFlags.Title),
                new(string.Empty, ShortDescription.ToString(), 0, false,
                    TooltipProperty.TooltipPropertyFlags.MultiLine)
            };

            UIHelper.TooltipAddEmptyLine(list);
            list.Add(new TooltipProperty(new TextObject("{=kAZdhRKo}Details").ToString(), " ", 0));
            UIHelper.TooltipAddSeperator(list);

            MBTextManager.SetTextVariable("LEFT", new TextObject("{=eUet83h3}Starting Piety").ToString());
            list.Add(new TooltipProperty(GameTexts.FindText("str_LEFT_ONLY").ToString(), Piety, 0));

            MBTextManager.SetTextVariable("LEFT", new TextObject("{=OcaF5fMN}Natural Culture").ToString());
            list.Add(new TooltipProperty(GameTexts.FindText("str_LEFT_ONLY").ToString(), Religion.MainCulture.Name.ToString(), 0));

            if (Religion.FavoredCultures.Count > 1)
            {
                UIHelper.TooltipAddEmptyLine(list);
                list.Add(new TooltipProperty(new TextObject("{=MTBhu6a0}Accepted Cultures").ToString(), " ", 0));
                UIHelper.TooltipAddSeperator(list);

                foreach (var culture in Religion.FavoredCultures)
                {
                    list.Add(new TooltipProperty("", culture.Name.ToString(), 0));
                }
            }

            UIHelper.TooltipAddEmptyLine(list);
            list.Add(new TooltipProperty(new TextObject("{=zyKSROjQ}Kingdoms").ToString(), " ", 0));
            UIHelper.TooltipAddSeperator(list);

            int kingdomCount = 0;
            foreach (var kingdom in Kingdom.All)
            {
                if (BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(kingdom.RulingClan.Leader)
                    == Religion)
                {
                    list.Add(new TooltipProperty("", kingdom.Name.ToString(), 0));
                    kingdomCount++;
                }
            }
           
            if (kingdomCount == 0)
            {
                list.Add(new TooltipProperty("", 
                    new TextObject("{=28PPXPOL}Not an official religion in any kingdom").ToString(), 0));
            }

            return list;
        }

        public void ExecuteSelectOption()
        {
            onSelect(this);
        }
    }
}